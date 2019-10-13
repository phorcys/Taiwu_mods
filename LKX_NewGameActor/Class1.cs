using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using GameData;
using Harmony12;
using UnityEngine;
using UnityModManagerNet;

/// <summary>
/// 开始新游戏锁定和自定义特质MOD
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
        /// 传家宝id列表
        /// </summary>
        public List<int> newItemsId = new List<int>();

        /// <summary>
        /// 选择类型
        /// </summary>
        public int itemsType;

        /// <summary>
        /// 是否义父所制
        /// </summary>
        public bool isStepfatherCreate;

        /// <summary>
        /// 同伴是否拥有
        /// </summary>
        public bool friendItemCreate;

        /// <summary>
        /// 传家宝数量
        /// </summary>
        public uint itemsCount;

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

        public static Dictionary<int, string> tempItemsId = new Dictionary<int, string>();

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
        /// <param name="value">是否激活mod</param>
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

                if ((Main.settings.file != null || Main.settings.file != "") && File.Exists(@Path.Combine(Main.path, Main.settings.file))) {
                    GUILayout.Label("文件存在！");

                } else {
                    GUILayout.Label("文件不存在，请检查文件是否存放在" + Main.path + "目录中");

                }
            }

            GUILayout.Label("传家宝选择，关闭是系统自带。传家宝直接mod内选择后添加，与特质选择不相关。");
            Main.settings.itemsType = GUILayout.SelectionGrid(Main.settings.itemsType, new string[] { "关闭", "全部", "自定义" }, 3);
            if (Main.settings.itemsType == 2)
            {

                List<int> tempList = new List<int>();

                GUILayout.BeginHorizontal("Box", new GUILayoutOption[0]);
                int countNum = (Main.tempItemsId.Count / 5) + 1;
                int i = 0;
                foreach (KeyValuePair<int, string> itemKV in Main.tempItemsId)
                {
                    if (i == 0) GUILayout.BeginVertical("Box", new GUILayoutOption[0]);

                    if (GUILayout.Toggle(Main.settings.newItemsId.Contains(itemKV.Key), itemKV.Value)) {
                        tempList.Add(itemKV.Key);

                    } else {
                        tempList.Remove(itemKV.Key);

                    }
                    if (i == Main.tempItemsId.Count - 1) {
                        GUILayout.EndVertical();
                        break;

                    }
                    if (i % 5 == 0 && i != 0) {
                        GUILayout.EndVertical();
                        GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
                    }
                    i++;
                }
                GUILayout.EndHorizontal();

                if (GUI.changed) Main.settings.newItemsId = tempList;
            }

            if (Main.settings.itemsType != 0)
            {
                GUILayout.BeginHorizontal("Box", new GUILayoutOption[0]);
                Main.settings.isStepfatherCreate = GUILayout.Toggle(Main.settings.isStepfatherCreate, "是否义父所制");
                Main.settings.friendItemCreate = GUILayout.Toggle(Main.settings.friendItemCreate, "同伴也有同样传家宝");
                GUILayout.EndHorizontal();

                GUILayout.Label("义父所制会覆盖原有道具效果。");
                GUILayout.Label("传家宝数量：" + Main.settings.itemsCount.ToString());
                Main.settings.itemsCount = (uint) GUILayout.HorizontalScrollbar(Main.settings.itemsCount, 1, 1, 10);
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
    /// 读取基础数据的时候，现在发现有其他接口可以直接取，不用拆了。
    /// </summary>
    [HarmonyPatch(typeof(MainMenu), "Start")]
    public static class GetTempItemsId_For_MainMenu_Start
    {

        static void Postfix()
        {
            if (Main.enabled)
            {
                //获取index 5和index 8的key
                List<string> keys = new List<string>(GetSprites.instance.baseGameDate["Item_Date"].Trim().Split(','));

                //遍历，符合的就加入字典
                foreach (KeyValuePair<int, Dictionary<int, string>> items in DateFile.instance.presetitemDate)
                {
                    if (items.Value[8] == "9" && items.Value[5] == "13")
                    {
                        Main.tempItemsId[int.Parse(items.Value[999])] = items.Value[0];
                    }
                }
            }
        }
    }

    /// <summary>
    /// 新建人物的特性点数锁定为10，返回值是__result直接改就行了
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

    /// <summary>
    /// 反射类，从blob/master/SLManager/Main.cs中拷贝而来。
    /// 作者:sth4nothing
    /// 日期:2019-04-22
    /// </summary>
    public class ReflectionMethod
    {
        private const BindingFlags Flags = BindingFlags.Instance
                                           | BindingFlags.Static
                                           | BindingFlags.NonPublic
                                           | BindingFlags.Public;

        public static T2 Invoke<T1, T2>(T1 instance, string method, params object[] args)
        {
            return (T2)typeof(T1).GetMethod(method, Flags)?.Invoke(instance, args);
        }

        public static void Invoke<T1>(T1 instance, string method, params object[] args)
        {
            typeof(T1).GetMethod(method, Flags)?.Invoke(instance, args);
        }

        public static object Invoke<T>(T instance, string method, System.Type[] argTypes, params object[] args)
        {
            argTypes = argTypes ?? new System.Type[0];
            var methods = typeof(T).GetMethods(Flags).Where(m =>
            {
                if (m.Name != method)
                    return false;
                return m.GetParameters()
                    .Select(p => p.ParameterType)
                    .SequenceEqual(argTypes);
            });

            if (methods.Count() != 1)
            {
                throw new AmbiguousMatchException("cannot find method to invoke");
            }

            return methods.First()?.Invoke(instance, args);
        }

        public static T2 GetValue<T1, T2>(T1 instance, string field)
        {
            return (T2)typeof(T1).GetField(field, Flags)?.GetValue(instance);
        }

        public static object GetValue<T>(T instance, string field)
        {
            return typeof(T).GetField(field, Flags)?.GetValue(instance);
        }
    }

    /// <summary>
    /// 加入见经识经这个特质
    /// 点击开始新游戏的时候可以用，根据字面意思理解就是被点击的时候才会执行。
    /// </summary>
    [HarmonyPatch(typeof(NewGame), "Start")]
    public static class AddTalent_For_NewGame_ToNewGame
    {
        static void Prefix()
        {
            if (Main.enabled && Main.settings.talentActor)
            {
                Dictionary<int, Dictionary<int, string>> abilityDate = DateFile.instance.abilityDate;
                if (abilityDate.ContainsKey(1001)) return;

                abilityDate.Add(1001, new Dictionary<int, string>());
                abilityDate[1001].Add(0, "见经识经");
                abilityDate[1001].Add(1, "0");
                abilityDate[1001].Add(2, "1");
                abilityDate[1001].Add(3, "0");
                abilityDate[1001].Add(99, "见经识经，见书识书，你从小就非常聪明，具有非常高的悟性！（来自开始新游戏锁定和自定义特质MOD）");
            }
        }
    }

    /*
    /// <summary>
    /// 下面的postfix换到这里Patch
    /// </summary>
    [HarmonyPatch(typeof(Game), "Update")]
    public static class FeatureType_For_Loading_LoadEnd
    {

        static void Postfix(int sceneId, bool newGame)
        {
            if (Main.enabled && newGame)
            {
                List<int> att = ReflectionMethod.GetValue<NewGame, List<int>>(NewGame.instance, "chooseAbility");
                //处理特性
                if (Main.settings.featureType != 0)
                {
                    string file = Main.settings.featureType == 1 ? "" : Path.Combine(Main.path, Main.settings.file);

                    //chooseAbility是开始游戏选择的特性，key 6就是谷中密友
                    if (Main.settings.friend && att.Count > 0 && att.Contains(6)) FeatureType_For_NewGame_SetNewGameDate.ProcessingFeatureDate(10003, file);

                    FeatureType_For_NewGame_SetNewGameDate.ProcessingFeatureDate(10001, file);
                }

                //处理自己添加的特质“见经识经”，处理后就删除
                if (att.Contains(1001))
                {
                    FeatureType_For_NewGame_SetNewGameDate.ProcessingAbilityDate(10001);
                    if (att.Contains(6)) FeatureType_For_NewGame_SetNewGameDate.ProcessingAbilityDate(10003);

                    DateFile.instance.abilityDate.Remove(1001);
                }

                //处理成长
                if (Main.settings.developmentActor != 0)
                {
                    FeatureType_For_NewGame_SetNewGameDate.ProcessingDevelopmentDate(10001);
                    if (att.Contains(6)) FeatureType_For_NewGame_SetNewGameDate.ProcessingDevelopmentDate(10003);
                }

                //处理传家宝
                if (Main.settings.itemsType != 0)
                {
                    FeatureType_For_NewGame_SetNewGameDate.ProcessingItemsDate(10001);
                    if (att.Contains(6) && Main.settings.friendItemCreate) FeatureType_For_NewGame_SetNewGameDate.ProcessingItemsDate(10003);
                }
            }
        }
    }
    */

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
            if (Main.enabled)
            {

                //处理特性
                if (Main.settings.featureType != 0)
                {
                    string file = Main.settings.featureType == 1 ? "" : Path.Combine(Main.path, Main.settings.file);

                    //chooseAbility是开始游戏选择的特性，key 6就是谷中密友
                    if (Main.settings.friend && ___chooseAbility.Count > 0 && ___chooseAbility.Contains(6)) ProcessingFeatureDate(10003, file);

                    ProcessingFeatureDate(10001, file);
                }

                //处理自己添加的特质“见经识经”，处理后就删除
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

                //处理传家宝
                if (Main.settings.itemsType != 0)
                {
                    ProcessingItemsDate(10001);
                    if (___chooseAbility.Contains(6) && Main.settings.friendItemCreate) ProcessingItemsDate(10003);
                }
            }

        }

        /// <summary>
        /// 处理传家宝
        /// </summary>
        /// <param name="actorId">游戏人物的ID</param>
        public static void ProcessingItemsDate(int actorId)
        {
            if (Main.settings.itemsType == 1)
            {
                //全选的
                foreach (int itemId in Main.tempItemsId.Keys)
                {
                    int forNum = Main.settings.itemsCount > 0 || Main.settings.itemsCount < 10 ? int.Parse(Main.settings.itemsCount.ToString()) : 1;
                    for (int i = 0; i < forNum; i++)
                    {
                        int itemMake = DateFile.instance.MakeNewItem(itemId, 0, 0, 100, 20);
                        DateFile.instance.GetItem(actorId, itemMake, int.Parse(Main.settings.itemsCount.ToString()), false, -1, 0);
                        DateFile.instance.ChangItemDate(itemMake, 92, 900, false);
                        DateFile.instance.ChangItemDate(itemMake, 93, 1800, false);

                        if (Main.settings.isStepfatherCreate)
                        {
                            int itemDurable = int.Parse(DateFile.instance.GetItemDate(int.Parse(DateFile.instance.GetItemDate(itemMake, 999, true)), 902, true)) * 2;

                            Dictionary<int, string> item = Items.GetItem(itemMake);
                            
                            item[901] = itemDurable.ToString();
                            item[902] = itemDurable.ToString();
                            item[504] = "50001";

                            Items.SetItem(itemMake, item);
                        }
                    }
                }
            }
            else
            {
                //自定义的
                foreach (int itemId in Main.settings.newItemsId)
                {
                    int forNum = Main.settings.itemsCount > 0 || Main.settings.itemsCount < 10 ? int.Parse(Main.settings.itemsCount.ToString()) : 1;
                    for (int i = 0; i < forNum; i++)
                    {
                        int itemMake = DateFile.instance.MakeNewItem(itemId, 0, 0, 100, 20);
                        DateFile.instance.GetItem(actorId, itemMake, int.Parse(Main.settings.itemsCount.ToString()), false, -1, 0);
                        DateFile.instance.ChangItemDate(itemMake, 92, 900, false);
                        DateFile.instance.ChangItemDate(itemMake, 93, 1800, false);

                        if (Main.settings.isStepfatherCreate)
                        {
                            int itemDurable = int.Parse(DateFile.instance.GetItemDate(int.Parse(DateFile.instance.GetItemDate(itemMake, 999, true)), 902, true)) * 2;

                            Dictionary<int, string> item = Items.GetItem(itemMake);

                            item[901] = itemDurable.ToString();
                            item[902] = itemDurable.ToString();
                            item[504] = "50001";

                            Items.SetItem(itemMake, item);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 成长类型指定
        /// </summary>
        /// <param name="actorId">游戏人物的ID</param>
        public static void ProcessingDevelopmentDate(int actorId)
        {
            //551是技艺，651是武学
            if (Main.settings.developmentActor > 1)
            {
                
                int randDevelopment = Main.settings.developmentActor;
                if (Main.settings.developmentActor == 1) randDevelopment = UnityEngine.Random.Range(2, 4);
                Characters.SetCharProperty(actorId, 551, randDevelopment.ToString());
                Characters.SetCharProperty(actorId, 651, randDevelopment.ToString());

            }
        }

        /// <summary>
        /// 悟性100
        /// </summary>
        /// <param name="actorId">游戏人物的ID</param>
        public static void ProcessingAbilityDate(int actorId)
        {
            Characters.SetCharProperty(actorId, 65, "100");
        }

        /// <summary>
        /// 添加特性的操作
        /// </summary>
        /// <param name="actorId">游戏人物的ID，10001是初代主角，10003如果有伙伴就是伙伴ID否则的话就是其他人物</param>
        /// <param name="file">自定义特性文件路径</param>
        public static void ProcessingFeatureDate(int actorId, string file = "")
        {
            Dictionary<int, string> array = new Dictionary<int, string>();
            array = file == "" ? GetAllFeature() : GetFileValue(file);

            string featureDefault = Characters.GetCharProperty(actorId, 101);
            string[] features = featureDefault.Split('|');
            foreach (string feature in features)
            {
                int key = int.Parse(feature.Trim());
                if (array.ContainsKey(key)) array.Remove(key);
            }

            //删去身怀六甲和相怄真身
            array.Remove(4003);
            array.Remove(10011);
            foreach (int item in array.Keys)
            {
                featureDefault += "|" + item.ToString();
            }
            //添加到属性里去。
            Characters.SetCharProperty(actorId, 101, featureDefault);
            //刷新特性缓存，要不然游戏不生效
            DateFile.instance.ActorFeaturesCacheReset();
        }

        /// <summary>
        /// 获得自定义特性文件
        /// </summary>
        /// <param name="file">文件路径默认为该mod文件夹目录下的Feature目录里</param>
        /// <returns></returns>
        public static Dictionary<int, string> GetFileValue(string file)
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
        public static Dictionary<int, string> GetAllFeature()
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