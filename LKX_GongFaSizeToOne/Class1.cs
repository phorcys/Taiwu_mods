using System.Reflection;
using System.Collections.Generic;
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
    }

    /// <summary>
    /// 点击人物读取游戏时载入。
    /// </summary>
    [HarmonyPatch(typeof(Loading), "LoadGameDateStart2")]
    public static class SetGongFaSizeToOne_For_GetSprites_GetDate
    {
        /// <summary>
        /// 初始全功法类型
        /// </summary>
        static List<string> gongFaKey = new List<string> { "101", "102", "103", "104", "105", "106", "107", "108", "109", "110", "111", "112", "113", "114"};

        /// <summary>
        /// 前置处理
        /// </summary>
        static void Prepare()
        {
            //不是全功法就根据选择去删除未选择的功法类型
            if (Main.settings.gongFaGuiType != 0)
            {
                if (!Main.settings.neigongActive) gongFaKey.Remove("101");
                if (!Main.settings.shenfaActive) gongFaKey.Remove("102");
                if (!Main.settings.juejiActive) gongFaKey.Remove("103");
                if (!Main.settings.quanfaActive) gongFaKey.Remove("104");
                if (!Main.settings.zhifaActive) gongFaKey.Remove("105");
                if (!Main.settings.tuifaActive) gongFaKey.Remove("106");
                if (!Main.settings.qimenActive) gongFaKey.Remove("107");
                if (!Main.settings.jianfaActive) gongFaKey.Remove("108");
                if (!Main.settings.daofaActive) gongFaKey.Remove("109");
                if (!Main.settings.changbingActive) gongFaKey.Remove("110");
                if (!Main.settings.anqiActive) gongFaKey.Remove("111");
                if (!Main.settings.ruanbingActive) gongFaKey.Remove("112");
                if (!Main.settings.yusheActive) gongFaKey.Remove("113");
                if (!Main.settings.yueqiActive) gongFaKey.Remove("114");
            }
        }
        
        /// <summary>
        /// 开始处理功法格子数。
        /// </summary>
        static void Postfix()
        {
            if (!Main.enabled) return;

            foreach (Dictionary<int, string> item in DateFile.instance.gongFaDate.Values)
            {
                if (gongFaKey.Contains(item[61])) item[7] = "1";
            }
        }
    }
    
}
