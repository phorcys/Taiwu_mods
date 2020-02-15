using GameData;
using Harmony12;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;


/// <summary>
/// 信息显示增强
/// </summary>
namespace MoreInfo
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry) => Save(this, modEntry);
        /// <summary>显示特殊词条</summary>
        public bool showExtraName = true;
        /// <summary>显示书籍门派</summary>
        public bool showBookGang = true;
        /// <summary>显示图纸名称</summary>
        public bool showBlueprintName = true;
        /// <summary>显示技艺书对应技艺名称</summary>
        public bool showOtherBookAbility = true;
        /// <summary>显示材料名称</summary>
        public bool showMagerialName = false;
        /// <summary>显示食物特殊词条</summary>
        public bool showFoodExtraName = true;
        /// <summary>包裹中物品显示</summary>
        public bool showInBag = true;
        /// <summary>装备界面包裹中物品显示</summary>
        public bool showInEquuipBag = true;
        /// <summary>仓库中物品显示</summary>
        public bool showInBank = true;
        /// <summary>商店中物品显示</summary>
        public bool showInShop = true;
        /// <summary>奇遇使用物品界面显示</summary>
        public bool showInStory = false;
        /// <summary>战利品界面显示</summary>
        public bool showInBooty = false;
        /// <summary>建筑界面显示</summary>
        public bool showInBuild = true;
        /// <summary>制造界面显示</summary>
        public bool showInMake = false;
        /// <summary>交换藏书界面</summary>
        public bool showInBookChange = false;
        /// <summary>赠送礼物界面</summary>
        public bool showInGift = false;
        /// <summary>读书界面</summary>
        public bool showInReadBook = false;
        /// <summary>在其他界面显示</summary>
        public bool showInOthers = true;
        /// <summary>显示特殊词条加成值</summary>
        public bool showExtraValue = false;
        /// <summary>显示功法等级颜色</summary>
        public bool showGongFaLevel = true;
        /// <summary>显示功法所属门派</summary>
        public bool showGongFaGang = true;
        /// <summary>强化显示功法进度</summary>
        public bool showGongFaProgress = true;
        /// <summary>经历筛选全部显示</summary>
        public bool showAllMessage = true;
        /// <summary>经历筛选显示, 0:结怨寻仇 1: 师徒亲子 2: 修习功法 3: 资源物品 4: 身份变更 5: 友情爱情 6: 战斗切磋 7: 伤病毒医 8: 寻访追随</summary>
        public bool[] showMessageType = new bool[] { true, true, true, true, true, true, true, true, true };
        /// <summary>类别名称</summary>
        public static readonly string[] messsageTypeName = new[]
        {
            "结怨寻仇","师徒亲子","修习功法","资源物品","身份变更","友情爱情","战斗切磋","伤病毒医","寻访追随"
        };
        /// <summary>
        /// 经历类型
        /// </summary>
        /// <remarks>游戏 V0.2.5.10 人物经历数据缺乏162</remarks>
        public static readonly List<HashSet<int>> messageTypes = new List<HashSet<int>>() {
            new HashSet<int>{7, 10, 11, 12, 13, 14, 15, 16, 29,30,38,39,48,49,52,53,64,65,67,68,74,75,76,
                77,78,82,83,96,97,98,99,100,115,116,117,118,119,120,121,122,123,124,125,126,127}, //结怨寻仇
            new HashSet<int>{43,44,47,55,79,80,81,93,94,101,102,103,104,105,106},//子女师徒
            new HashSet<int>{25,26,27,28,70,71,72,73,74,75,85,86,96,110,113},//修习
            new HashSet<int>{1,2,3,4,8,9,10,11,12,13,14,15,16,17,21,22,59,60,107,131,137,138139,140,141,142,
                143,144,145,146,147,148,149,150,151,152,153,154,155,156,157,158,169,160,161,163,164,165,166,
                188,189,190},//物品
            new HashSet<int>{84,112,114,168,169,170},//身份
            new HashSet<int>{40,41,42,45,46,50,51,52,54,56,57,66,69,72,73,76,90,91,92,96,97,98,99,100,109,125,
                171,172,173,174},//情爱
            new HashSet<int>{31,32,33,34,35,36,37,175,176,177,178,179,180,181,182,183,184,185,186,188,189,190,191},//战斗
            new HashSet<int>{5,6,7,18,19,20,61,62,63,108,109,110,111,122,128,129,130,163,164,167,187},//伤病
            new HashSet<int>{53,54,55,56,57,58,132,133,134,135,136},//跟随
        };
        /// <summary>显示奇遇等级</summary>
        public bool showStroyLevel = true;
    }

    public static class Main
    {
        internal static bool enabled;
        internal static Settings settings;
        internal static UnityModManager.ModEntry.ModLogger Logger;
        private static bool[] showMessageTypeTmp;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            if (showMessageTypeTmp == null)
            {
                showMessageTypeTmp = new bool[settings.showMessageType.Length];
            }
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            return true;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }


        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {

            GUILayout.BeginHorizontal();
            GUILayout.Label("<color=#FFA500>信息增强MOD,作者ignaz_chou,橙色内容为强烈推荐</color>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("<color=#87CEEB>显示功能</color>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            settings.showExtraName = GUILayout.Toggle(settings.showExtraName, "显示宝物特殊词条", GUILayout.Width(120));
            settings.showFoodExtraName = GUILayout.Toggle(settings.showFoodExtraName, "显示食物特殊词条", GUILayout.Width(120));
            settings.showOtherBookAbility = GUILayout.Toggle(settings.showOtherBookAbility, "显示技艺书技艺", GUILayout.Width(120));
            settings.showBookGang = GUILayout.Toggle(settings.showBookGang, "显示书籍门派", GUILayout.Width(120));
            settings.showBlueprintName = GUILayout.Toggle(settings.showBlueprintName, "<color=#FFA500>显示图纸名称</color>", GUILayout.Width(120));
            settings.showMagerialName = GUILayout.Toggle(settings.showMagerialName, "显示材料类别", GUILayout.Width(120));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("\n<color=#87CEEB>显示区域-主要界面</color>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            settings.showInEquuipBag = GUILayout.Toggle(settings.showInEquuipBag, "装备界面包裹显示", GUILayout.Width(120));
            settings.showInBag = GUILayout.Toggle(settings.showInBag, "包裹中显示", GUILayout.Width(120));
            settings.showInBank = GUILayout.Toggle(settings.showInBank, "仓库中显示", GUILayout.Width(120));
            settings.showInShop = GUILayout.Toggle(settings.showInShop, "商店中显示", GUILayout.Width(120));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("\n <color=#87CEEB>显示区域-次要界面(注：部分次要界面开启后会明显变卡)</color>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            settings.showInBookChange = GUILayout.Toggle(settings.showInBookChange, "交换藏书界面显示", GUILayout.Width(120));
            settings.showInGift = GUILayout.Toggle(settings.showInGift, "赠送礼物界面显示", GUILayout.Width(120));
            settings.showInStory = GUILayout.Toggle(settings.showInStory, "奇遇物品栏显示", GUILayout.Width(120));
            settings.showInBooty = GUILayout.Toggle(settings.showInBooty, "战利品界面显示", GUILayout.Width(120));
            settings.showInBuild = GUILayout.Toggle(settings.showInBuild, "修建界面显示", GUILayout.Width(120));
            settings.showInMake = GUILayout.Toggle(settings.showInMake, "制造界面显示", GUILayout.Width(120));
            settings.showInReadBook = GUILayout.Toggle(settings.showInReadBook, "读书界面显示", GUILayout.Width(120));

            //settings.showExtraValue = GUILayout.Toggle(settings.showExtraValue, "显示加成数值", GUILayout.Width(120));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("\n<color=#87CEEB>功法增强(注:门派与心得增强只限人物功法界面)</color>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            settings.showGongFaLevel = GUILayout.Toggle(settings.showGongFaLevel, "显示功法等级颜色(需重启游戏生效)", GUILayout.Width(230));
            settings.showGongFaGang = GUILayout.Toggle(settings.showGongFaGang, "显示功法所属门派", GUILayout.Width(120));
            settings.showGongFaProgress = GUILayout.Toggle(settings.showGongFaProgress, "进度心得显示增强", GUILayout.Width(120));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("\n<color=#87CEEB>经历筛选(不要在查看人物经历时存档)</color>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            var showAllMessageTmp = GUILayout.Toggle(settings.showAllMessage, "显示所有", GUILayout.Width(120));
            for (int i = 0; i < showMessageTypeTmp.Length; ++i)
            {
                showMessageTypeTmp[i] = GUILayout.Toggle(settings.showMessageType[i], Settings.messsageTypeName[i], GUILayout.Width(80));
                if (showAllMessageTmp != settings.showAllMessage && showAllMessageTmp) // 若勾选显示所有
                {
                    showMessageTypeTmp[i] = true; // 将所有类型都选上, 并清空不显示经历类型列表
                    if (Changer.exclucdedMessageTypes.Count > 0)
                        Changer.exclucdedMessageTypes.Clear();
                }
                else if (showMessageTypeTmp[i] != settings.showMessageType[i]) // 显示的经历类型选择发生变化
                {
                    if (!showMessageTypeTmp[i]) // 若某一类型改为不显示
                    {
                        showAllMessageTmp = false;  // 去掉勾选显示所有
                        Changer.exclucdedMessageTypes.UnionWith(Settings.messageTypes[i]); // 将不显示的经历类型加入排除列表
                    }
                    else // 若某一类型改为显示
                    {
                        if (Changer.exclucdedMessageTypes.Count > 0)
                        {
                            // 因为不同经历筛选类型可能含有相同的messageId，故需清空排除列表再重新添加
                            Changer.exclucdedMessageTypes.Clear();
                            for (int j = 0; j < showMessageTypeTmp.Length; ++j)
                            {
                                if (j != i) // 不添加选择显示的类型
                                    Changer.exclucdedMessageTypes.UnionWith(Settings.messageTypes[j]);
                            }
                        }
                    }
                }
                settings.showMessageType[i] = showMessageTypeTmp[i];
            }
            settings.showAllMessage = showAllMessageTmp;

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("\n<color=#87CEEB>其他</color>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            settings.showStroyLevel = GUILayout.Toggle(settings.showStroyLevel, "显示奇遇等级", GUILayout.Width(120));

            GUILayout.EndHorizontal();
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry) => settings.Save(modEntry);
    }

    /// <summary>
    /// 定义变更器
    /// </summary>
    internal static class Changer
    {
        internal static readonly HashSet<int> exclucdedMessageTypes = new HashSet<int>();

        //获取需要显示的经历id列表
        public static bool MessageTypToShow(int messageTyp) => !exclucdedMessageTypes.Contains(messageTyp);

        //50061-50066 膂力- 定力
        //51361-51366 膂力%- 悟性% 
        //51367 - 51372 护体% - 闪避%
        //50501 - 50516 音律 - 织锦
        //50601 - 50614 内功 - 杂学
        //DateFile.instance.actorAttrDate actorAttr_date.txt

        /// <summary>
        /// 特殊词条类型1字典
        /// </summary>
        private static readonly Dictionary<int, string> itemExtraAttrType1 = new Dictionary<int, string>()
               {
                    { 50501,"音律"},
                    { 50502,"弈棋"},
                    { 50503,"诗书"},
                    { 50504,"绘画"},
                    { 50505,"术数"},
                    { 50506,"品鉴"},
                    { 50507,"锻造"},
                    { 50508,"制木"},
                    { 50509,"医术"},
                    { 50510,"毒术"},
                    { 50511,"织锦"},
                    { 50512,"巧匠"},
                    { 50513,"道法"},
                    { 50514,"佛学"},
                    { 50515,"厨艺"},
                    { 50516,"杂学"},

                    { 50601,"内功"},
                    { 50602,"身法"},
                    { 50603,"绝技"},
                    { 50604,"拳掌"},
                    { 50605,"指法"},
                    { 50606,"腿法"},
                    { 50607,"暗器"},
                    { 50608,"剑法"},
                    { 50609,"刀法"},
                    { 50610,"长兵"},
                    { 50611,"奇门"},
                    { 50612,"软兵"},
                    { 50613,"御射"},
                    { 50614,"乐器"},
               };
        /// <summary>
        /// 特殊词条类型2字典
        /// </summary>
        private static readonly Dictionary<int, string> itemExtraAttrType2 = new Dictionary<int, string>()
               {
                {51361,"膂力"},
                {51362,"体质"},
                {51363,"灵敏"},
                {51364,"根骨"},
                {51365,"悟性"},
                {51366,"定力"},
               };
        /// <summary>
        /// 材料分类
        /// </summary>
        private static readonly Dictionary<int, string> materialType = new Dictionary<int, string>()
               {
                    { 0,""},
                    { 1,"硬"},
                    { 2,"软"},
                    { 7,"铁"},
                    { 8,"木"},
                    { 11,"布"},
                    { 12,"玉"},
               };

        /// <summary>
        /// 获取人物名字
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        public static string GetActorName(int actorId) => DateFile.instance.GetActorName(actorId, true, false);

        /// <summary>
        /// 获取功法所属门派Id
        /// </summary>
        /// <param name="gongFaId"></param>
        /// <returns></returns>
        public static int GetGangId(int gongFaId) => int.Parse(DateFile.instance.gongFaDate[gongFaId][3]);
        /// <summary>
        /// 获取功法所属门派名称
        /// </summary>
        /// <param name="gongFaId"></param>
        /// <returns></returns>
        public static string GetGangName(int gongFaId)
        {
            int gangId = GetGangId(gongFaId);
            return DateFile.instance.presetGangDate[gangId][0];
        }

        /// <summary>
        /// 获取书籍对应的功法ID
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public static int GetGongFaId(int itemId)
        {
            int bookTyp = int.Parse(DateFile.instance.GetItemDate(itemId, 999, false));
            //500000以上为普通秘籍，700000以上为手抄
            return IsOriginalBook(itemId) ? (bookTyp - 500000) : (bookTyp - 700000);
        }

        /// <summary>
        /// 判断秘籍是真传还是手抄
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        private static bool IsOriginalBook(int itemId) =>
            //序列35 0为普通,1为手抄本
            int.Parse(DateFile.instance.GetItemDate(itemId, 35, false)) == 0;

        /// <summary>
        /// 获取物品大类
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        private static int GetItemType(int itemId) => int.Parse(DateFile.instance.GetItemDate(itemId, 4, false));

        /// <summary>
        /// 获取物品小类
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        private static int GetItemSecondType(int itemId) => int.Parse(DateFile.instance.GetItemDate(itemId, 5, false));

        /// <summary>
        /// 获取物品细类
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        private static int GetItemThirdType(int itemId) => int.Parse(DateFile.instance.GetItemDate(itemId, 506, false));

        /// <summary>
        /// 物品制作类型
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        private static int GetMakeType(int itemId) =>
            //0为材料包，7铁8木9药10毒11布12玉15食材
            int.Parse(DateFile.instance.GetItemDate(itemId, 41, false));

        /// <summary>
        /// 物品制作成品方向
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        private static int GetProductType(int itemId) =>
            //0无法制作-装备类1硬2软
            int.Parse(DateFile.instance.GetItemDate(itemId, 48, false));

        //获取物品名称
        private static string GetItemName(int itemId) => DateFile.instance.GetItemDate(itemId, 0, false);

        /// <summary>
        /// 获取一类词条的名称
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        private static string GetItemExtraNameType1(int itemId)
        {
            string value = "";
            foreach (var item in itemExtraAttrType1)
            {
                int val = int.Parse(DateFile.instance.GetItemDate(itemId, item.Key, false));
                if (val > 0)
                {
                    //使用/n换行后无法显示耐久，直接接属性名后方则耐久显示不全，暂时只显示属性
                    value = item.Value;
                    break;
                }
            }
            return value;
        }

        /// <summary>
        /// 获取二类词条的名称
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        private static string GetItemExtraNameType2(int itemId)
        {
            string value = "";
            foreach (var item in itemExtraAttrType2)
            {
                int val = int.Parse(DateFile.instance.GetItemDate(itemId, item.Key, false));
                if (val > 0)
                {
                    value = item.Value;
                    break;
                }
            }
            return value;
        }

        /// <summary>
        /// 变更文本内容
        /// </summary>
        /// <param name="text"></param>
        /// <param name="newText"></param>
        private static void ChangeText(Text text, string newText) => text.text = newText;

        /// <summary>
        /// 按照特殊词条类型1显示名称（功法技艺加成等）
        /// </summary>
        /// <param name="text"></param>
        /// <param name="itemId"></param>
        /// <param name="add">是否同时显示耐久。耐久度大于10的显示不下</param>
        private static void ChangeDecNameType1(Text text, int itemId, bool add = false)
        {
            string extraName = GetItemExtraNameType1(itemId);
            if (extraName != "")
            {
                text.text = add ? extraName + text.text : extraName;
            }
        }

        /// <summary>
        /// 按照特殊词条类型2显示名称装备的膂力-悟性等百分比加成
        /// </summary>
        /// <param name="text"></param>
        /// <param name="itemId"></param>
        /// <param name="add"></param>
        private static void ChangeDecNameType2(Text text, int itemId, bool add = false)
        {
            string extraName = GetItemExtraNameType2(itemId);
            if (extraName != "")
            {
                text.text = add ? extraName + text.text : extraName;
            }
        }

        /// <summary>
        /// 变更图纸名称
        /// </summary>
        /// <param name="text"></param>
        /// <param name="itemId"></param>
        private static void ChangeBlueprintName(Text text, int itemId)
        {
            if (!Main.settings.showBlueprintName) return;
            ChangeText(text, GetItemName(itemId));
        }

        /// <summary>
        /// 变更功法书名称
        /// </summary>
        /// <param name="text"></param>
        /// <param name="itemId"></param>
        private static void ChangeGongFaBookName(Text text, int itemId)
        {
            if (!Main.settings.showBookGang) return;
            string gangName = GetGangName(GetGongFaId(itemId)).Substring(0, 2);
            text.text = gangName + text.text;
        }

        /// <summary>
        /// 变更技艺书名称
        /// </summary>
        /// <param name="text"></param>
        /// <param name="itemid"></param>
        /// <param name="typ3"></param>
        private static void ChangeAbilityBookName(Text text, int typ3)
        {
            if (!Main.settings.showOtherBookAbility) return;
            string abilityName = itemExtraAttrType1[typ3 + 50501 - 4];
            text.text = abilityName + text.text;
        }

        /// <summary>
        /// 变更书籍名称
        /// </summary>
        /// <param name="text"></param>
        /// <param name="itemId"></param>
        private static void ChangeBookName(Text text, int itemId)
        {
            int typ3 = GetItemThirdType(itemId);
            //4-19为技艺，20-33为功法
            if (typ3 >= 20)
            {
                ChangeGongFaBookName(text, itemId);
            }
            else
            {
                if (typ3 < 20 && typ3 >= 4)
                {
                    ChangeAbilityBookName(text, typ3);
                }
            }
        }

        /// <summary>
        /// 变更制造类物品名称
        /// </summary>
        /// <param name="text"></param>
        /// <param name="itemId"></param>
        private static void ChangeType1Name(Text text, int itemId)
        {
            if (!Main.settings.showMagerialName) return;
            int mtyp = GetMakeType(itemId);
            //暂不处理药材食材和毒药
            switch (mtyp)
            {
                case 0: //材料包
                    string name = GetItemName(itemId);
                    name = name.Substring(name.Length - 2);
                    text.text = $"{name}{text.text}";
                    break;
                case 7:
                case 8:
                case 11:
                case 12:
                    int ptyp = GetProductType(itemId);
                    if (ptyp != 0)
                        text.text = $"{materialType[ptyp]}{materialType[mtyp]}{text.text}";
                    break;
            }
        }

        /// <summary>
        /// 变更食物名称
        /// </summary>
        /// <param name="text"></param>
        /// <param name="itemId"></param>
        private static void ChangeType3Name(Text text, int itemId)
        {
            if (!Main.settings.showFoodExtraName) return;
            ChangeDecNameType1(text, itemId, true);
        }

        /// <summary>
        /// 变更图书与图纸名称
        /// </summary>
        /// <param name="text"></param>
        /// <param name="itemId"></param>
        private static void ChangeType5Name(Text text, int itemId)
        {
            int typ2 = GetItemSecondType(itemId);
            switch (typ2)
            {
                case 20:
                    ChangeBlueprintName(text, itemId);
                    break;
                case 21:
                    ChangeBookName(text, itemId);
                    break;
            }
        }

        /// <summary>
        /// 变更装备名称
        /// </summary>
        /// <param name="text"></param>
        /// <param name="itemId"></param>
        private static void ChangeEquipName(Text text, int itemId)
        {
            if (!Main.settings.showExtraName) return;
            int typ2 = GetItemSecondType(itemId);
            switch (typ2)
            {
                case 13: //宝物
                    ChangeDecNameType1(text, itemId, true);
                    break;
                case 14: //帽子
                    ChangeDecNameType2(text, itemId, true);
                    break;
                case 15: //鞋子
                    ChangeDecNameType2(text, itemId, true);
                    break;
                case 16: //护甲
                    ChangeDecNameType2(text, itemId, true);
                    break;
            }

        }

        /// <summary>
        /// 变更Holder中所有object名称
        /// </summary>
        /// <param name="__Holder"></param>
        /// <param name="textName"></param>
        public static void ChangeObjectsName(Transform __Holder, string textName)
        {
            int childCount = __Holder.childCount;
            for (int i = 0; i < childCount; i++)
            {
                GameObject gameObject = __Holder.GetChild(i).gameObject;
                var gameText = gameObject.transform.Find(textName).GetComponent<Text>();
                string[] tmpArray = gameObject.name.Split(new char[] { ',' });
                ChangeItemName(gameText, int.Parse(tmpArray[1]));
            }
        }

        /// <summary>
        /// 变更物品名称
        /// </summary>
        /// <param name="text"></param>
        /// <param name="itemId"></param>
        public static void ChangeItemName(Text text, int itemId)
        {
            int typ = GetItemType(itemId);
            switch (typ)
            {
                case 1: // 制造类物品
                    ChangeType1Name(text, itemId);
                    break;
                case 3: // 食物
                    ChangeType3Name(text, itemId);
                    break;
                case 4: // 装备
                    ChangeEquipName(text, itemId);
                    break;
                case 5: // 图书与图纸
                    ChangeType5Name(text, itemId);
                    break;
            }
        }
    }

    /// <summary>
    /// 物品显示名称
    /// </summary>

    // 将人物包裹中的的物品替换为特殊词条显示
    [HarmonyPatch(typeof(SetItem), "SetActorMenuItemIcon")]
    internal static class SetItem_SetActorMenuItemIcon_Patch
    {
        public static void Postfix(SetItem __instance, int itemId)
        {
            if (!Main.enabled || !Main.settings.showInBag)
                return;
            Changer.ChangeItemName(__instance.itemNumber, itemId);
        }
    }

    // 将人物装备界面中物品栏的物品替换为特殊词条显示
    [HarmonyPatch(typeof(SetItem), "SetActorEquipIcon")]
    internal static class SetItem_SetActorEquipIcon_Patch
    {
        public static void Postfix(SetItem __instance, int itemId)
        {
            if (!Main.enabled || !Main.settings.showInEquuipBag)
                return;
            Changer.ChangeItemName(__instance.itemNumber, itemId);
        }
    }

    //将仓库中的的物品替换为特殊词条显示
    [HarmonyPatch(typeof(SetItem), "SetWarehouseItemIcon")]
    internal static class SetItem_SetWarehouseItemIcon_Patch
    {
        public static void Postfix(SetItem __instance, int itemId)
        {
            if (!Main.enabled || !Main.settings.showInBank)
                return;
            Changer.ChangeItemName(__instance.itemNumber, itemId);
        }
    }

    //将商店中的的物品替换为特殊词条显示
    [HarmonyPatch(typeof(SetItem), "SetShopItemIcon")]
    internal static class SetItem_SetShopItemIcon_Patch
    {
        public static void Postfix(SetItem __instance, int itemId)
        {
            if (!Main.enabled || !Main.settings.showInShop)
                return;
            Changer.ChangeItemName(__instance.itemNumber, itemId);
        }
    }

    // 将书店中的的物品替换为特殊词条显示
    [HarmonyPatch(typeof(SetItem), "SetBookShopItemIcon")]
    internal static class SetItem_SetBookShopItemIcon_Patch
    {
        public static void Postfix(SetItem __instance, int itemId)
        {
            if (!Main.enabled || !Main.settings.showInBookChange)
                return;
            Changer.ChangeItemName(__instance.itemNumber, itemId);
        }
    }

    // 奇遇选择物品界面设置物品名称
    [HarmonyPatch(typeof(ToStoryMenu), "GetItem")]
    internal static class StorySystem_GetItem_Patch
    {
        public static void Postfix(ToStoryMenu __instance)
        {
            if (!Main.enabled || !Main.settings.showInStory)
                return;
            Changer.ChangeObjectsName(__instance.itemHolder, "ItemNumberText");
        }

    }

    //修习图书界面设置物品名称
    [HarmonyPatch(typeof(BuildingWindow), "SetBook")]
    internal static class HomeSystem_SetBook_Patch
    {
        public static void Postfix(BuildingWindow __instance)
        {
            if (!Main.enabled || !Main.settings.showInReadBook)
                return;
            Changer.ChangeObjectsName(__instance.bookHolder, "ItemHpText");
        }
    }

    //修建界面显示
    [HarmonyPatch(typeof(BuildingWindow), "GetItem")]
    internal static class HomeSystem_GetItem_Patch
    {
        public static void Postfix(BuildingWindow __instance)
        {
            if (!Main.enabled || !Main.settings.showInStory)
                return;
            Changer.ChangeObjectsName(__instance.itemHolder, "ItemNumberText");
        }

    }

    //战利品界面
    [HarmonyPatch(typeof(BattleEndWindow), "ShowBattleBooty")]
    internal static class BattleSystem_ShowBattleBooty_Patch
    {
        public static void Postfix(BattleEndWindow __instance)
        {
            if (!Main.enabled || !Main.settings.showInStory)
                return;
            Changer.ChangeObjectsName(__instance.battleBootyHolder, "ItemNumberText");
        }
    }

    //赠送礼物界面
    [HarmonyPatch(typeof(ui_MessageWindow), "GetItem")]
    internal static class MassageWindow_GetItem_Patch
    {
        public static void Postfix(ui_MessageWindow __instance)
        {
            if (!Main.enabled || !Main.settings.showInGift)
                return;
            Changer.ChangeObjectsName(__instance.itemHolder, "ItemNumberText");
        }
    }

    //将人物装备界面的人物已装备物品
    [HarmonyPatch(typeof(ActorMenu), "UpdateEquips")]
    internal static class ActorMenu_UpdateEquips_Patch
    {
        public static void Postfix(Image[] ___equipIcons, Text[] ___equipHpText, int key)
        {
            if (!Main.enabled || !Main.settings.showInEquuipBag)
                return;
            for (int i = 0; i < ___equipIcons.Length; i++)
            {
                int equipId = int.Parse(DateFile.instance.GetActorDate(key, 301 + i, applyBonus: false));
                Changer.ChangeItemName(___equipHpText[i], equipId);
            }
        }
    }

    // 将获取物品界面设置物品名称??具体是哪个界面未知
    [HarmonyPatch(typeof(GetItemWindow), "SetGetItem")]
    internal static class GetItemWindow_SetGetItem_Patch
    {
        public static void Postfix(GetItemWindow __instance, int index, int itemId)
        {
            if (!Main.enabled || !Main.settings.showInOthers)
                return;
            Changer.ChangeItemName(__instance.itemHpText[index], itemId);
        }
    }

    // 制造界面右侧物品栏
    [HarmonyPatch(typeof(MakeSystem), "SetMianToolItem")]
    internal static class MakeSystem_SetMianToolItem_Patch
    {
        public static void Postfix(int id, Transform holder)
        {
            if (!Main.enabled || !Main.settings.showInMake)
                return;
            Text numberText = holder.transform.Find("Item," + id).Find("ItemNumberText").GetComponent<Text>();
            Changer.ChangeItemName(numberText, id);
        }
    }

    /// <summary>
    /// 人物功法界面根据功法品级显示颜色、显示功法门派、修习度区分
    /// </summary>
    [HarmonyPatch(typeof(SetGongFaIcon), "SetGongFa")]
    internal static class SetGongFaIcon_SetGongFa_Patch
    {
        public static void Postfix(SetGongFaIcon __instance, int gongFaId, int actorId)
        {
            if (!Main.enabled)
                return;

            // 功法所属门派
            if (Main.settings.showGongFaGang)
            {
                string gangName = Changer.GetGangName(gongFaId);
                __instance.gongFaSizeText.text = $"{gangName}\n{__instance.gongFaSizeText.text}";
            }
            // 根据修习进度与心得变更颜色增加区分度
            if (Main.settings.showGongFaProgress)
            {
                int level = DateFile.instance.GetGongFaLevel(actorId, gongFaId, 0);
                int colorFix = level / 10;
                __instance.gongFaLevelText.text = DateFile.instance.SetColoer(20001 + colorFix, __instance.gongFaLevelText.text);
                int bookLevel = DateFile.instance.GetGongFaFLevel(actorId, gongFaId, false);
                __instance.gongFaBookLevelText.text = DateFile.instance.SetColoer(20001 + bookLevel, __instance.gongFaBookLevelText.text);
            }
        }
    }

    /// <summary>
    /// HOOK掉功法颜色, 根据功法品级显示颜色
    /// </summary>
    [HarmonyPatch(typeof(ArchiveSystem.LoadGame), "LoadReadonlyData")]
    internal static class ArchiveSystem_GameData_ReadonlyData_Load_Patch
    {
        [HarmonyAfter("BaseResourceMod")]
        public static void Postfix()
        {
            if (!Main.enabled || !Main.settings.showGongFaLevel)
                return;

            foreach (var pair in DateFile.instance.gongFaDate)
            {
                if (int.TryParse(pair.Value[2], out var lv))
                {
                    pair.Value[0] = DateFile.instance.SetColoer(20001 + lv, pair.Value[0]);
                }
            }
        }
    }

    /// <summary>
    /// 奇遇显示等级
    /// </summary>
    [HarmonyPatch(typeof(WorldMapPlace), "UpdatePlaceStory")]
    internal static class WorldMapPlace_UpdatePlaceStory_Patch
    {
        public static void Postfix(WorldMapPlace __instance, int ___placeId)
        {
            if (!Main.enabled
                || !Main.settings.showStroyLevel
                || __instance.storyTime.text == "99"
                || DateFile.instance.HaveShow(DateFile.instance.mianPartId, ___placeId) < 1
                || !DateFile.instance.HaveStory(DateFile.instance.mianPartId, ___placeId)
                || !DateFile.instance.worldMapState[DateFile.instance.mianPartId].TryGetValue(___placeId, out var storyInfo))
            {
                return;
            }

            int storyId = storyInfo[0];
            string level = DateFile.instance.baseStoryDate[storyId][3];
            if (int.Parse(level) < 1)
                return;
            int storyTime = storyInfo[1];
            __instance.storyTime.text = storyTime > 0 ? $"难度:{level}时间{storyTime}" : $"难度:{level}";
        }
    }

    /// <summary>
    /// 经历筛选
    /// </summary>
    /// <remarks><see cref="ActorMenu.ShowActorMassage"/></remarks>
    [HarmonyPatch(typeof(ActorMenu), "ShowActorMassage")]
    internal static class ActorMenu_ShowActorMassage_Patch
    {
        private static Action<int> ShowMassage = null;
        /// <summary>
        /// 经历筛选
        /// </summary>
        public static bool Prefix(int key, ActorMenu __instance, List<string> ___showMassage, ref int ___showMassageIndex)
        {
            if (!Main.enabled || Main.settings.showAllMessage)
                return true;

            ___showMassage.Clear();
            int num = DateFile.instance.MianActorID();
            ___showMassage.Add(string.Format(DateFile.instance.SetColoer(20002, "·") + " {0}{1}{2}{3}{4}\n",
                                             DateFile.instance.massageDate[8010][1].Split('|')[0],
                                             DateFile.instance.SetColoer(
                                                 10002,
                                                 DateFile.instance.solarTermsDate[int.Parse(DateFile.instance.GetActorDate(key, 25, applyBonus: false))][102]),
                                             DateFile.instance.massageDate[8010][1].Split('|')[1],
                                             DateFile.instance.GetActorName(key, realName: false, baseName: true),
                                             DateFile.instance.massageDate[8010][1].Split('|')[2]));
            LifeRecords.LifeRecord[] allRecords = LifeRecords.GetAllRecords(key);
            if (allRecords != null)
            {
                int num2 = Mathf.Max(DateFile.instance.GetActorFavor(isEnemy: false, num, key), 0);
                for (int i = 0; i < allRecords.Length; i++)
                {
                    LifeRecords.LifeRecord record = allRecords[i];
                    if (!Changer.exclucdedMessageTypes.Contains(record.messageId)
                        && DateFile.instance.actorMassageDate.ContainsKey(record.messageId))
                    {
                        int num3 = int.Parse(DateFile.instance.actorMassageDate[record.messageId][4]);
                        num3 = 30000 * num3 / 100;
                        if (key != num && num2 < num3)
                        {
                            List<string> list = ___showMassage;
                            string format = DateFile.instance.SetColoer(20002, "·") + " {0}{1}：{2}\n";
                            string str = DateFile.instance.massageDate[16][1];
                            DateFile instance = DateFile.instance;
                            short year = record.year;
                            list.Add(string.Format(format,
                                                   str + instance.SetColoer(10002, year.ToString()) + DateFile.instance.massageDate[16][3],
                                                   DateFile.instance.SetColoer(20002, DateFile.instance.solarTermsDate[record.solarTerm][0]),
                                                   DateFile.instance.SetColoer(10001, DateFile.instance.massageDate[12][2])));
                        }
                        else
                        {
                            List<string> list2 = ___showMassage;
                            string format2 = DateFile.instance.SetColoer(20002, "·")
                                             + " {0}{1}："
                                             + DateFile.instance.actorMassageDate[record.messageId][1]
                                             + "\n";
                            object[] args = DateFile.instance.GetLifeRecordMassageElements(key, record).ToArray();
                            list2.Add(string.Format(format2, args));
                        }
                    }
                }
            }
            ___showMassageIndex = 0;
            if (ShowMassage == null)
            {
                var bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
                var method = typeof(ActorMenu).GetMethod("ShowMassage", bindingFlags, null, new[] { typeof(int) }, null);
                ShowMassage = (Action<int>)Delegate.CreateDelegate(typeof(Action<int>), __instance, method, true);
            }
            ShowMassage(___showMassageIndex);
            return false;
        }
    }
}


