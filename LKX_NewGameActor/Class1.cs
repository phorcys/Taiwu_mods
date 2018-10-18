using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Harmony12;
using UnityEngine;
using UnityModManagerNet;

/// <summary>
/// 新游戏人物加特性MOD
/// </summary>
namespace LKX_NewGameActor
{
    /// <summary>
    /// 设置文件
    /// </summary>
    public class Settings : UnityModManager.ModSettings
    {
        /// <summary>
        /// 是否锁定特性点数
        /// </summary>
        public bool lockAbilityNum;

        /// <summary>
        /// 选择特性类型
        /// </summary>
        public int featureType;

        /// <summary>
        /// 初始伙伴是否共同拥有特性
        /// </summary>
        public bool friend;

        /// <summary>
        /// 自定义特性文件路径
        /// </summary>
        public string file;

        /// <summary>
        /// 是否开启聪明过人，将占用Ability的id 1001
        /// </summary>
        public bool talentActor;

        /// <summary>
        /// 成长类型0为关闭，1为随机，2为均衡，3为早熟，4为晚熟
        /// </summary>
        public int developmentActor;

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
        /// 获取mod根目录
        /// </summary>
        public static string path;

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
            Main.path = Path.Combine(modEntry.Path, "Feature");
            if (Main.settings.file == null) Main.settings.file = "";

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
            GUILayout.Label("如果status亮红灯代表失效！", redLabelStyle);
            Main.settings.lockAbilityNum = GUILayout.Toggle(Main.settings.lockAbilityNum, "锁定新建人物时的特性点");
            Main.settings.talentActor = GUILayout.Toggle(Main.settings.talentActor, "开启新建人物高悟性选项（新建人物特性里自己找找）。");
            GUILayout.Label("选择人物成长，随机是MOD随机选择，关闭是系统自行选择，会同时修改技艺和武艺的成长但不会增加其数值。");
            Main.settings.developmentActor = GUILayout.SelectionGrid(Main.settings.developmentActor, new string[] { "关闭", "随机", "均衡", "早熟", "晚成" }, 5);
            Main.settings.featureType = GUILayout.SelectionGrid(Main.settings.featureType, new string[]{ "关闭", "全3级特性", "自定义" }, 3);

            if (Main.settings.featureType != 0) Main.settings.friend = GUILayout.Toggle(Main.settings.friend, "是否给初始伙伴分配这些特性");
            if (Main.settings.featureType == 1) GUILayout.Label("全3级特性会使游戏可玩性降低", redLabelStyle);
            if (Main.settings.featureType == 2)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("自定义特性文件名：", GUILayout.Width(130));
                string featureFilename = "";
                featureFilename = GUILayout.TextArea(Main.settings.file);
                if (GUI.changed) Main.settings.file = featureFilename;

                GUILayout.EndHorizontal();
                if ((Main.settings.file != null || Main.settings.file != "") && File.Exists(@Path.Combine(Main.path, Main.settings.file)))
                {
                    GUILayout.Label("文件存在！");
                }
                else
                {
                    GUILayout.Label("文件不存在，请检查文件是否存放在" + Main.path + "目录中");
                }
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
    /// 新建人物的特性点数锁定为10
    /// </summary>
    [HarmonyPatch(typeof(NewGame), "GetAbilityP")]
    public static class LockAbilityNum_For_NewGame_GetAbilityP
    {

        static void Postfix(ref int __result)
        {
            if (Main.enabled && Main.settings.lockAbilityNum) __result = 10;

            return;
        }
    }

    [HarmonyPatch(typeof(NewGame), "Awake")]
    public static class AddTalent_For_NewGame_ToNewGame
    {
        static void Prefix()
        {
            if (Main.enabled && Main.settings.talentActor)
            {
                Dictionary<int, Dictionary<int, string>> abilityDate = DateFile.instance.abilityDate;
                abilityDate.Add(1001, new Dictionary<int, string>());
                abilityDate[1001].Add(0, "见经识经");
                abilityDate[1001].Add(1, "0");
                abilityDate[1001].Add(2, "1");
                abilityDate[1001].Add(3, "0");
                abilityDate[1001].Add(99, "见经识经，见书识书，你从小就非常聪明，具有非常高的悟性！（来自新游戏人物加特性MOD）");
            }
        }
    }
    
    /// <summary>
    /// 开始新游戏创建人物后执行
    /// </summary>
    [HarmonyPatch(typeof(NewGame), "SetNewGameDate")]
    public static class FeatureType_For_NewGame_SetNewGameDate
    {
        /// <summary>
        /// 这个方法是固定的，更多方法和说明去看Harmony的wiki
        /// @see https://github.com/pardeike/Harmony/wiki/Patching
        /// </summary>
        /// <param name="___chooseAbility">NewGame的私有变量，三个下划线取得。</param>
        static void Postfix(ref List<int> ___chooseAbility)
        {
            if (!Main.enabled) return;

            //处理特性
            if (Main.settings.featureType != 0)
            {
                string file = Main.settings.featureType == 1 ? "" : Path.Combine(Main.path, Main.settings.file);

                //chooseAbility是开始游戏选择的特性，key = 6中就是
                if (Main.settings.friend && ___chooseAbility.Count > 0 && ___chooseAbility.Contains(6)) ProcessingFeatureDate(10003, file);

                ProcessingFeatureDate(10001, file);
            }

            //处理自己添加的Ability
            if (___chooseAbility.Contains(1001))
            {
                ProcessingAbilityDate(10001);
                if (___chooseAbility.Contains(6)) ProcessingAbilityDate(10003);

                DateFile.instance.abilityDate.Remove(1001);
            }

            //处理成长
            if (Main.settings.developmentActor != 0)
            {
                ProcessingDevelopmentDate(10001);
                if (___chooseAbility.Contains(6)) ProcessingDevelopmentDate(10003);
            }

            return;
        }

        /// <summary>
        /// 成长类型指定
        /// </summary>
        /// <param name="actorId">游戏人物的ID</param>
        static void ProcessingDevelopmentDate(int actorId)
        {
            Dictionary<int, string> actor;

            if (DateFile.instance.actorsDate.TryGetValue(actorId, out actor))
            {
                int randDevelopment = Main.settings.developmentActor;
                if (Main.settings.developmentActor == 1) randDevelopment = Random.Range(2, 4);
                actor[551] = randDevelopment.ToString();
                actor[651] = randDevelopment.ToString();
            }
        }

        /// <summary>
        /// 悟性100
        /// </summary>
        /// <param name="actorId">游戏人物的ID</param>
        static void ProcessingAbilityDate(int actorId)
        {
            Dictionary<int, string> actor;
            
            if (DateFile.instance.actorsDate.TryGetValue(actorId, out actor))
            {
                actor[65] = "100";
            }

        }

        /// <summary>
        /// 添加特性的操作
        /// </summary>
        /// <param name="actorId">游戏人物的ID，10001是初代主角，10003如果有伙伴就是伙伴ID否则的话就是其他人物</param>
        /// <param name="file">自定义特性文件路径</param>
        static void ProcessingFeatureDate(int actorId, string file = "")
        {
            Dictionary<int, string> actor, array = new Dictionary<int, string>();
            array = file == "" ? GetAllFeature() : GetFileValue(file);

            if (DateFile.instance.actorsDate.TryGetValue(actorId, out actor))
            {
                string[] features = actor[101].Split('|');
                foreach (string feature in features)
                {
                    int key = int.Parse(feature.Trim());
                    if (array.ContainsKey(key)) array.Remove(key);
                }
            }

            array.Remove(4003);
            array.Remove(10011);
            foreach (int item in array.Keys)
            {
                actor[101] += "|" + item.ToString();
            }

            DateFile.instance.actorsFeatureCache.Remove(actorId);
            
        }

        /// <summary>
        /// 获得自定义特性文件
        /// </summary>
        /// <param name="file">文件路径默认为该mod文件夹目录下的Feature目录里</param>
        /// <returns></returns>
        static Dictionary<int, string> GetFileValue(string file)
        {
            Dictionary<int, string> array = new Dictionary<int, string>();
            if (!File.Exists(file)) return array;

            StreamReader sr = File.OpenText(file);
            string content;

            while ((content = sr.ReadLine()) != null)
            {
                int key = int.Parse(content.Trim());
                if (!array.ContainsKey(key)) array.Add(key, "");
            }

            sr.Close();
            
            return array;
        }

        /// <summary>
        /// 获得系统内的3级特性，如添加过特性文件且为3级特性同样会获得。
        /// </summary>
        /// <returns></returns>
        static Dictionary<int, string> GetAllFeature()
        {
            Dictionary<int, string> array = new Dictionary<int, string>();
            foreach (KeyValuePair<int, Dictionary<int, string>> item in DateFile.instance.actorFeaturesDate)
            {
                if (item.Value[4] == "3") array[item.Key] = "";
            }

            return array;
        }
    }
}