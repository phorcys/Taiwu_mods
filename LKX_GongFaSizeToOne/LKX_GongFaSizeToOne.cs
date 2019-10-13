using System.Reflection;
using System.Collections.Generic;
using GameData;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;

namespace LKX_GongFaSizeToOne
{
    /// <summary>
    /// 设置文件
    /// </summary>
    public class Settings : UnityModManager.ModSettings
    {
        /// <summary>
        /// GUI选择的类型
        /// </summary>
        public int gongFaGuiType;

        /// <summary>
        /// 内功
        /// </summary>
        public bool neigongActive;
        /// <summary>
        /// 身法
        /// </summary>
        public bool shenfaActive;
        /// <summary>
        /// 绝技
        /// </summary>
        public bool juejiActive;
        /// <summary>
        /// 拳掌
        /// </summary>
        public bool quanfaActive;
        /// <summary>
        /// 指法
        /// </summary>
        public bool zhifaActive;
        /// <summary>
        /// 腿法
        /// </summary>
        public bool tuifaActive;
        /// <summary>
        /// 奇门
        /// </summary>
        public bool qimenActive;
        /// <summary>
        /// 剑法
        /// </summary>
        public bool jianfaActive;
        /// <summary>
        /// 刀法
        /// </summary>
        public bool daofaActive;
        /// <summary>
        /// 长兵
        /// </summary>
        public bool changbingActive;
        /// <summary>
        /// 暗器
        /// </summary>
        public bool anqiActive;
        /// <summary>
        /// 软兵
        /// </summary>
        public bool ruanbingActive;
        /// <summary>
        /// 御射
        /// </summary>
        public bool yusheActive;
        /// <summary>
        /// 乐器
        /// </summary>
        public bool yueqiActive;

        /// <summary>
        /// 是否开启全内功9格
        /// </summary>
        public bool neigongOk;

        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="modEntry"></param>
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    public class Main
    {
        /// <summary>
        /// umm日志
        /// </summary>
        public static UnityModManager.ModEntry.ModLogger logger;

        /// <summary>
        /// mod设置
        /// </summary>
        public static Settings settings;

        /// <summary>
        /// 是否开启mod
        /// </summary>
        public static bool enabled;

        /// <summary>
        /// 开启全内功9格
        /// </summary>
        public static bool neigongOk;

        /// <summary>
        /// 初始全功法类型
        /// </summary>
        public static List<string> gongFaKey = new List<string> { "101", "102", "103", "104", "105", "106", "107", "108", "109", "110", "111", "112", "113", "114" };

        public static string messageLabel;

        /// <summary>
        /// 载入mod。
        /// </summary>
        /// <param name="modEntry">mod管理器对象</param>
        /// <returns></returns>
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Main.logger = modEntry.Logger;
            Main.settings = Settings.Load<Settings>(modEntry);
            
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());

            modEntry.OnToggle = Main.OnToggle;
            modEntry.OnGUI = Main.OnGUI;
            modEntry.OnSaveGUI = Main.OnSaveGUI;

            return true;
        }

        /// <summary>
        /// 确定是否激活mod
        /// </summary>
        /// <param name="modEntry">umm</param>
        /// <param name="value">是否激活</param>
        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Main.enabled = value;
            return true;
        }

        /// <summary>
        /// 展示mod的设置
        /// </summary>
        /// <param name="modEntry">umm</param>
        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUIStyle redLabelStyle = new GUIStyle();
            redLabelStyle.normal.textColor = new Color(159f / 256f, 20f / 256f, 29f / 256f);
            GUILayout.Label("如果status亮红灯代表mod失效！游戏过程中修改了mod配置需要重新开启游戏让mod生效！", redLabelStyle);
            if (GUILayout.Button("手动激活功法1格大小"))
            {
                if (DateFile.instance.gongFaDate.Count == 0)
                {
                    messageLabel = "请进入游戏存档";
                }
                else
                {
                    Main.ProcessingGongFaSize();
                    messageLabel = "已执行修改";
                }
            }
            GUILayout.Label(messageLabel);

            Main.settings.neigongOk = GUILayout.Toggle(Main.settings.neigongOk, "开启全内功9格");

            Main.settings.gongFaGuiType = GUILayout.SelectionGrid(Main.settings.gongFaGuiType, new string[] { "全功法1格", "自定义功法1格" }, 2);

            if (Main.settings.gongFaGuiType == 1)
            {
                GUILayout.BeginHorizontal("Box", new GUILayoutOption[0]);
                GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
                Main.settings.neigongActive = GUILayout.Toggle(Main.settings.neigongActive, "内功类");
                Main.settings.shenfaActive = GUILayout.Toggle(Main.settings.shenfaActive, "身法类");
                Main.settings.juejiActive = GUILayout.Toggle(Main.settings.juejiActive, "绝技类");
                Main.settings.quanfaActive = GUILayout.Toggle(Main.settings.quanfaActive, "拳掌类");
                Main.settings.zhifaActive = GUILayout.Toggle(Main.settings.zhifaActive, "指法类");
                GUILayout.EndVertical();

                GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
                Main.settings.tuifaActive = GUILayout.Toggle(Main.settings.tuifaActive, "腿法类");
                Main.settings.qimenActive = GUILayout.Toggle(Main.settings.qimenActive, "奇门类");
                Main.settings.jianfaActive = GUILayout.Toggle(Main.settings.jianfaActive, "剑法类");
                Main.settings.daofaActive = GUILayout.Toggle(Main.settings.daofaActive, "刀法类");
                Main.settings.changbingActive = GUILayout.Toggle(Main.settings.changbingActive, "长兵类");
                GUILayout.EndVertical();

                GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
                Main.settings.anqiActive = GUILayout.Toggle(Main.settings.anqiActive, "暗器类");
                Main.settings.ruanbingActive = GUILayout.Toggle(Main.settings.ruanbingActive, "软兵类");
                Main.settings.yusheActive = GUILayout.Toggle(Main.settings.yusheActive, "御射类");
                Main.settings.yueqiActive = GUILayout.Toggle(Main.settings.yueqiActive, "乐器类");
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            
        }

        /// <summary>
        /// 保存mod的设置
        /// </summary>
        /// <param name="modEntry">umm</param>
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Main.settings.Save(modEntry);
        }

        public static void ProcessingGongFaSize()
        {
            if (!Main.enabled) return;
            
            Main.logger.Log("开始执行功法替换1格");
            foreach (Dictionary<int, string> item in DateFile.instance.gongFaDate.Values)
            {
                if (Main.gongFaKey.Contains(item[61])) item[7] = "1";
                if (Main.settings.neigongOk && item[61] == "101")
                {
                    item[921] = item[922] = item[923] = item[924] = "9";
                }
            }
        }

        /// <summary>
        /// 检查用户选项，没选的去掉。于是剩下的就是选择的
        /// </summary>
        public static void ProcessingGongFaType()
        {
            if (Main.settings.gongFaGuiType != 0)
            {
                if (!Main.settings.neigongActive) Main.gongFaKey.Remove("101");
                if (!Main.settings.shenfaActive) Main.gongFaKey.Remove("102");
                if (!Main.settings.juejiActive) Main.gongFaKey.Remove("103");
                if (!Main.settings.quanfaActive) Main.gongFaKey.Remove("104");
                if (!Main.settings.zhifaActive) Main.gongFaKey.Remove("105");
                if (!Main.settings.tuifaActive) Main.gongFaKey.Remove("106");
                if (!Main.settings.anqiActive) Main.gongFaKey.Remove("107");
                if (!Main.settings.jianfaActive) Main.gongFaKey.Remove("108");
                if (!Main.settings.daofaActive) Main.gongFaKey.Remove("109");
                if (!Main.settings.changbingActive) Main.gongFaKey.Remove("110");
                if (!Main.settings.qimenActive) Main.gongFaKey.Remove("111");
                if (!Main.settings.ruanbingActive) Main.gongFaKey.Remove("112");
                if (!Main.settings.yusheActive) Main.gongFaKey.Remove("113");
                if (!Main.settings.yueqiActive) Main.gongFaKey.Remove("114");
            }
        }
    }

    /// <summary>
    /// 点击人物读取游戏时载入。
    /// </summary>
    [HarmonyPatch(typeof(MainMenu), "Start")]
    public static class SetGongFaSizeToOne_For_MainMenu_Start
    {

        /// <summary>
        /// 前置处理
        /// </summary>
        static void Prepare()
        {
            if (!Main.enabled) return;
            
            Main.ProcessingGongFaType();
        }
        
        /// <summary>
        /// 开始处理功法格子数。
        /// </summary>
        static void Postfix()
        {
            if (!Main.enabled) return;

            Main.ProcessingGongFaSize();
        }
    }
    
}
