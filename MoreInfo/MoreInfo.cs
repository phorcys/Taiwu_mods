using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;


//信息显示增强
namespace MoreInfo
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }

        public bool showExtraName = true;//显示特殊词条
        public bool showBookGang = true;//显示书籍门派
        public bool showBlueprintName = true;//显示图纸名称
        public bool showOtherBookAbility = true;//显示技艺书对应技艺名称
        public bool showMagerialName = false;//显示材料名称
        public bool showFoodExtraName = true;//显示食物特殊词条

        public bool showInBag = true;//包裹中物品显示
        public bool showInEquuipBag = true;//装备界面包裹中物品显示
        public bool showInBank = true;//仓库中物品显示
        public bool showInShop = true;//商店中物品显示
        public bool showInStory = false;//奇遇使用物品界面显示
        public bool showInBooty = false;//战利品界面显示
        public bool showInBuild = true;//建筑界面显示
        public bool showInMake = false;//制造界面显示
        public bool showInBookChange = false;//交换藏书界面
        public bool showInGift = false;//赠送礼物界面
        public bool showInReadBook = false;//读书界面

        public bool showInOthers = true;//在其他界面显示        
        public bool showExtraValue = false;//显示特殊词条加成值

        public bool showGongFaLevel = true;//显示功法等级颜色
        public bool showGongFaGang = true;//显示功法所属门派
        public bool showGongFaProgress = true;//强化显示功法进度

        //经历筛选
        public bool showAllMassage = true;

        public bool[] showMassageType = new bool[] { true, true, true, true, true, true, true, true, true };

        public bool showStroyLevel = true;//显示奇遇等级
    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;


        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
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
            settings.showInShop = GUILayout.Toggle(settings.showInShop, "商店中显示<", GUILayout.Width(120));

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
            settings.showGongFaLevel = GUILayout.Toggle(settings.showGongFaLevel, "显示功法等级颜色", GUILayout.Width(120));
            settings.showGongFaGang = GUILayout.Toggle(settings.showGongFaGang, "显示功法所属门派", GUILayout.Width(120));
            settings.showGongFaProgress = GUILayout.Toggle(settings.showGongFaProgress, "进度心得显示增强", GUILayout.Width(120));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("\n<color=#87CEEB>经历筛选</color>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            settings.showAllMassage = GUILayout.Toggle(settings.showAllMassage, "显示所有", GUILayout.Width(120));
            settings.showMassageType[0] = GUILayout.Toggle(settings.showMassageType[0], "结怨寻仇", GUILayout.Width(80));
            settings.showMassageType[1] = GUILayout.Toggle(settings.showMassageType[1], "师徒亲子", GUILayout.Width(80));
            settings.showMassageType[2] = GUILayout.Toggle(settings.showMassageType[2], "修习功法", GUILayout.Width(80));
            settings.showMassageType[3] = GUILayout.Toggle(settings.showMassageType[3], "资源物品", GUILayout.Width(80));
            settings.showMassageType[4] = GUILayout.Toggle(settings.showMassageType[4], "身份变更", GUILayout.Width(80));
            settings.showMassageType[5] = GUILayout.Toggle(settings.showMassageType[5], "友情爱情", GUILayout.Width(80));
            settings.showMassageType[6] = GUILayout.Toggle(settings.showMassageType[6], "战斗切磋", GUILayout.Width(80));
            settings.showMassageType[7] = GUILayout.Toggle(settings.showMassageType[7], "伤病毒医", GUILayout.Width(80));
            settings.showMassageType[8] = GUILayout.Toggle(settings.showMassageType[8], "寻访追随", GUILayout.Width(80));


            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("\n<color=#87CEEB>其他</color>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            settings.showStroyLevel = GUILayout.Toggle(settings.showStroyLevel, "显示奇遇等级", GUILayout.Width(120));

            GUILayout.EndHorizontal();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }

    /// <summary>
    /// 定义变更器
    /// </summary>
    public static class Changer
    {
        /// <summary>
        /// 入魔图标，储存相关地点ID
        /// </summary>
        private static string placeIds = "";
        public static void AddPlaceId(int pid) => placeIds += pid + "|";
        public static void ResetPlaceIds() => placeIds = "";
        public static string[] GetSplitPlaceIds() => placeIds.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        public static string GetPlaceIds() => placeIds;

        /// <summary>
        /// 经历筛选
        /// </summary>
        private static List<int[]> massageBackup = new List<int[]> { };
        private static readonly List<int[]> massageFilter = new List<int[]>() {
            new int[]{7, 10, 11, 12, 13, 14, 15, 16, 29,30,38,39,48,49,52,53,64,65,67,68,74,75,76,
                77,78,82,83,96,97,98,99,100,115,116,117,118,119,120,121,122,123,124,125},
            new int[]{43,44,47,55,79,80,81,93,94,101,102,103,104,105,106},//子女师徒
            new int[]{25,26,27,28,70,71,72,73,74,75,85,86,96,110,113},//修习
            new int[]{1,2,3,4,8,9,10,11,12,13,14,15,16,17,21,22,59,60,107},//物品
            new int[]{84,112,114},//身份
            new int[]{40,41,42,45,46,50,51,52,54,56,57,66,69,72,73,76,90,91,92,96,97,98,99,100,109,125},//情爱
            new int[]{31,32,33,34,35,36,37},//战斗
            new int[]{5,6,7,18,19,20,61,62,63,108,109,110,111,122},//伤病
            new int[]{53,54,55,56,57,58},//跟随
        };

        private static int backupId = 0;
        /// <summary>
        /// 存储数据
        /// </summary>
        /// <param name="id"></param>
        public static void BackupMassage(int id)
        {
            backupId = id;
            massageBackup = DateFile.instance.actorLifeMassage[id];
        }
        public static List<int[]> GetBackupMassage() => massageBackup;
        public static void ResetBackup() => backupId = 0;
        public static int GetBackupId() => backupId;

        //获取需要显示的经历id列表
        public static List<int> GetTypeList()
        {
            //Main.Logger.Log("getTypeList");
            List<int> tlist = new List<int>();
            for (int i = 0; i < Main.settings.showMassageType.Length; i++)
            {
                bool show = Main.settings.showMassageType[i];
                //Main.Logger.Log("getTypeList.step2" + "show:" + show + "index:" + i + "count:" + massageFilter.Count);

                if (show && i < massageFilter.Count)
                {
                    for (int j = 0; j < massageFilter[i].Length; j++)
                    {
                        tlist.Add(massageFilter[i][j]);
                    }
                }
            }
            return tlist.Distinct().ToList();
        }

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
        public static int GetGangId(int gongFaId) => DateFile.instance.ParseInt(DateFile.instance.gongFaDate[gongFaId][3]);
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
            int bookTyp = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 999, false));
            //500000以上为普通秘籍，700000以上为手抄
            return IsOriginalBook(itemId) ? (bookTyp - 500000) : (bookTyp - 700000);
        }

        /// <summary>
        /// 判断秘籍是真传还是手抄
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public static bool IsOriginalBook(int itemId) =>
            //序列35 0为普通,1为手抄本
            DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 35, false)) == 0;

        /// <summary>
        /// 获取物品大类
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public static int GetItemType(int itemId) => DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 4, false));

        /// <summary>
        /// 获取物品小类
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public static int GetItemSecondType(int itemId) => DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 5, false));

        /// <summary>
        /// 获取物品细类
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public static int GetItemThirdType(int itemId) => DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 506, false));

        /// <summary>
        /// 物品制作类型
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public static int GetMakeType(int itemId) =>
            //0为材料包，7铁8木9药10毒11布12玉15食材
            DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 41, false));

        /// <summary>
        /// 物品制作成品方向
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public static int GetProductType(int itemId) =>
            //0无法制作-装备类1硬2软
            DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 48, false));

        //获取物品名称
        public static string GetItemName(int itemId) => DateFile.instance.GetItemDate(itemId, 0, false);

        /// <summary>
        /// 获取一类词条的名称
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public static string GetItemExtraNameType1(int itemId)
        {
            string value = "";
            foreach (var item in itemExtraAttrType1)
            {
                int val = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, item.Key, false));
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
        public static string GetItemExtraNameType2(int itemId)
        {
            string value = "";
            foreach (var item in itemExtraAttrType2)
            {
                int val = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, item.Key, false));
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
        public static void ChangeText(Text text, string newText) => text.text = newText;

        /// <summary>
        /// 按照特殊词条类型1显示名称（功法技艺加成等）
        /// </summary>
        /// <param name="text"></param>
        /// <param name="itemId"></param>
        /// <param name="add">是否同时显示耐久。耐久度大于10的显示不下</param>
        public static void ChangeDecNameType1(Text text, int itemId, bool add = false)
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
        public static void ChangeDecNameType2(Text text, int itemId, bool add = false)
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
        public static void ChangeBlueprintName(Text text, int itemId)
        {
            if (!Main.settings.showBlueprintName) return;
            ChangeText(text, GetItemName(itemId));
        }

        /// <summary>
        /// 变更功法书名称
        /// </summary>
        /// <param name="text"></param>
        /// <param name="itemId"></param>
        public static void ChangeGongFaBookName(Text text, int itemId)
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
        public static void ChangeAbilityBookName(Text text, int itemid, int typ3)
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
        public static void ChangeBookName(Text text, int itemId)
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
                    ChangeAbilityBookName(text, itemId, typ3);
                }
            }
        }

        /// <summary>
        /// 变更制造类物品名称
        /// </summary>
        /// <param name="text"></param>
        /// <param name="itemId"></param>
        public static void ChangeType1Name(Text text, int itemId)
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
        public static void ChangeType3Name(Text text, int itemId)
        {
            if (!Main.settings.showFoodExtraName) return;
            ChangeDecNameType1(text, itemId, true);
        }

        /// <summary>
        /// 变更图书与图纸名称
        /// </summary>
        /// <param name="text"></param>
        /// <param name="itemId"></param>
        public static void ChangeType5Name(Text text, int itemId)
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
        public static void ChangeEquipName(Text text, int itemId)
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
                ChangeItemName(gameText, DateFile.instance.ParseInt(tmpArray[1]));
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
    public static class SetItem_SetActorMenuItemIcon_Patch
    {
        static void Postfix(SetItem __instance, int itemId)
        {
            if (!Main.enabled || !Main.settings.showInBag)
                return;
            Changer.ChangeItemName(__instance.itemNumber, itemId);
        }
    }

    // 将人物装备界面中物品栏的物品替换为特殊词条显示
    [HarmonyPatch(typeof(SetItem), "SetActorEquipIcon")]
    public static class SetItem_SetActorEquipIcon_Patch
    {
        static void Postfix(SetItem __instance, int itemId)
        {
            if (!Main.enabled || !Main.settings.showInEquuipBag)
                return;
            Changer.ChangeItemName(__instance.itemNumber, itemId);
        }
    }

    //将仓库中的的物品替换为特殊词条显示
    [HarmonyPatch(typeof(SetItem), "SetWarehouseItemIcon")]
    public static class SetItem_SetWarehouseItemIcon_Patch
    {
        static void Postfix(SetItem __instance, int itemId)
        {
            if (!Main.enabled || !Main.settings.showInBank)
                return;
            Changer.ChangeItemName(__instance.itemNumber, itemId);
        }
    }

    //将商店中的的物品替换为特殊词条显示
    [HarmonyPatch(typeof(SetItem), "SetShopItemIcon")]
    public static class SetItem_SetShopItemIcon_Patch
    {
        static void Postfix(SetItem __instance, int itemId)
        {
            if (!Main.enabled || !Main.settings.showInShop)
                return;
            Changer.ChangeItemName(__instance.itemNumber, itemId);
        }
    }

    // 将书店中的的物品替换为特殊词条显示
    [HarmonyPatch(typeof(SetItem), "SetBookShopItemIcon")]
    public static class SetItem_SetBookShopItemIcon_Patch
    {
        static void Postfix(SetItem __instance, int itemId)
        {
            if (!Main.enabled || !Main.settings.showInBookChange)
                return;
            Changer.ChangeItemName(__instance.itemNumber, itemId);
        }
    }

    // 奇遇选择物品界面设置物品名称
    [HarmonyPatch(typeof(StorySystem), "GetItem")]
    public static class StorySystem_GetItem_Patch
    {
        static void Postfix(StorySystem __instance, int typ)
        {
            if (!Main.enabled || !Main.settings.showInStory)
                return;
            Changer.ChangeObjectsName(__instance.itemHolder, "ItemNumberText");
        }

    }
    //修习图书界面设置物品名称
    [HarmonyPatch(typeof(HomeSystem), "SetBook")]
    public static class HomeSystem_SetBook_Patch
    {
        static void Postfix(HomeSystem __instance)
        {
            if (!Main.enabled || !Main.settings.showInReadBook)
                return;
            Changer.ChangeObjectsName(__instance.bookHolder, "ItemHpText");
        }
    }

    //修建界面显示
    [HarmonyPatch(typeof(HomeSystem), "GetItem")]
    public static class HomeSystem_GetItem_Patch
    {
        static void Postfix(HomeSystem __instance)
        {
            if (!Main.enabled || !Main.settings.showInStory)
                return;
            Changer.ChangeObjectsName(__instance.itemHolder, "ItemNumberText");
        }

    }

    //战利品界面
    [HarmonyPatch(typeof(BattleSystem), "ShowBattleBooty")]
    public static class BattleSystem_ShowBattleBooty_Patch
    {
        static void Postfix(BattleSystem __instance)
        {
            if (!Main.enabled || !Main.settings.showInStory)
                return;
            Changer.ChangeObjectsName(__instance.battleBootyHolder, "ItemNumberText");
        }
    }

    //赠送礼物界面
    [HarmonyPatch(typeof(MassageWindow), "GetItem")]
    public static class MassageWindow_GetItem_Patch
    {
        static void Postfix(MassageWindow __instance)
        {
            if (!Main.enabled || !Main.settings.showInGift)
                return;
            Changer.ChangeObjectsName(__instance.itemHolder, "ItemNumberText");
        }
    }

    //将人物装备界面的人物已装备物品
    [HarmonyPatch(typeof(ActorMenu), "UpdateEquips")]
    public static class ActorMenu_UpdateEquips_Patch
    {
        static void Postfix(Image[] ___equipIcons, Text[] ___equipHpText, int key)
        {
            if (!Main.enabled || !Main.settings.showInEquuipBag)
                return;
            for(int i=0; i < ___equipIcons.Length; i++)
            {
                int equipId = DateFile.instance.ParseInt(DateFile.instance.GetActorDate(key, 301 + i, addValue: false));
                Changer.ChangeItemName(___equipHpText[i], equipId);
            }
        }
    }

    // 将获取物品界面设置物品名称??具体是哪个界面未知
    [HarmonyPatch(typeof(GetItemWindow), "SetGetItem")]
    public static class GetItemWindow_SetGetItem_Patch
    {
        static void Postfix(GetItemWindow __instance, int index, int itemId)
        {
            if (!Main.enabled || !Main.settings.showInOthers)
                return;
            Changer.ChangeItemName(__instance.itemHpText[index], itemId);
        }
    }

    // 制造界面右侧物品栏
    [HarmonyPatch(typeof(MakeSystem), "SetMianToolItem")]
    public static class MakeSystem_SetMianToolItem_Patch
    {
        static void Postfix(MakeSystem __instance, int id, GameObject item, Transform holder, Image dragDes, bool showSize)
        {
            if (!Main.enabled || !Main.settings.showInMake)
                return;
            Text numberText = holder.transform.Find("Item," + id).Find("ItemNumberText").GetComponent<Text>();
            Changer.ChangeItemName(numberText, id);
        }
    }

    /// <summary>
    /// 功法
    /// </summary>

    //人物功法界面根据功法品级显示颜色
    [HarmonyPatch(typeof(SetGongFaIcon), "SetGongFa")]
    public static class SetGongFaIcon_SetGongFa_Patch
    {
        static void Postfix(SetGongFaIcon __instance, int gongFaId, int actorId)
        {
            if (!Main.enabled)
                return;
            //功法所属门派
            if (Main.settings.showGongFaGang)
            {
                string gangName = Changer.GetGangName(gongFaId);
                __instance.gongFaSizeText.text = $"{gangName}\n{__instance.gongFaSizeText.text}";
            }
            //根据修习进度与心得变更颜色增加区分度
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

    //HOOK掉功法颜色
    [HarmonyPatch(typeof(Loading), "LoadBaseDate")]
    public static class Loading_LoadingScene_Patch
    {
        static void Postfix()
        {
            if (!Main.enabled || !Main.settings.showGongFaLevel)
                return;
            foreach (var item in DateFile.instance.gongFaDate)
            {
                var GData = item.Value;
                int lv = DateFile.instance.ParseInt(GData[2]);

                GData[0] = DateFile.instance.SetColoer(20001 + lv, GData[0]);
            }
        }
    }

    /// <summary>
    /// 其他
    /// </summary>
    //奇遇显示等级
    [HarmonyPatch(typeof(WorldMapPlace), "UpdatePlaceStory")]
    public static class WorldMapPlace_UpdatePlaceStory_Patch
    {
        static void Postfix(WorldMapPlace __instance, int ___placeId)
        {
            if (!Main.enabled
                || !Main.settings.showStroyLevel
                || __instance.storyTime.text == "99"
                || DateFile.instance.HaveShow(DateFile.instance.mianPartId, ___placeId) < 1
                || !DateFile.instance.HaveStory(DateFile.instance.mianPartId, ___placeId)
                || !DateFile.instance.worldMapState[DateFile.instance.mianPartId].ContainsKey(___placeId))
                return;
            int storyId = DateFile.instance.worldMapState[DateFile.instance.mianPartId][___placeId][0];
            string level = DateFile.instance.baseStoryDate[storyId][3];
            if (DateFile.instance.ParseInt(level) < 1)
                return;
            int storyTime = DateFile.instance.worldMapState[DateFile.instance.mianPartId][___placeId][1];
            __instance.storyTime.text = storyTime > 0 ? string.Format("难度:{0}时间{1}", level, storyTime) : $"难度:{level}";
        }
    }

    //经历筛选
    [HarmonyPatch(typeof(ActorMenu), "ShowActorMassage")]
    public static class ActorMenu_ShowActorMassage_Patch
    {
        static void Prefix(ActorMenu __instance, int key)
        {
            if (!Main.enabled || Main.settings.showAllMassage)
                return;
            if (!DateFile.instance.actorLifeMassage.ContainsKey(key)) return;
            int backupId = Changer.GetBackupId();
            //Main.Logger.Log("Pre-----" + changer.getActorName(key));
            if (backupId == key) return;
            if (backupId != 0)
            {
                //Main.Logger.Log("reset-----" + changer.getActorName(key) + "Count:" + DateFile.instance.actorLifeMassage[key].Count);

                DateFile.instance.actorLifeMassage[backupId] = Changer.GetBackupMassage();
                Changer.ResetBackup();
            }
            List<int[]> newLifeMassage = new List<int[]> { };

            Changer.BackupMassage(key);

            //Main.Logger.Log("setp1" + changer.getActorName(key));
            List<int> tlist = Changer.GetTypeList();
            int count = DateFile.instance.actorLifeMassage[key].Count;
            //Main.Logger.Log("setp2:MaxCount:" + count);
            //Main.Logger.Log("setp3:showCount:" + tlist.Count);
            for (int i = 0; i < count; i++)
            {
                //Main.Logger.Log("setp4:" + i);
                int[] array = DateFile.instance.actorLifeMassage[key][i];
                int key2 = array[0];//根据经历类型ID进行筛选
                for (int j = 0; j < tlist.Count; j++)
                {
                    if (tlist[j] == key2)
                    {
                        newLifeMassage.Add(DateFile.instance.actorLifeMassage[key][i]);
                        //Main.Logger.Log("setp6:" + tlist[j]);
                        break;
                    }
                }
            }
            DateFile.instance.actorLifeMassage[key] = newLifeMassage;
        }
    }

    //恢复备份的经历
    [HarmonyPatch(typeof(ActorMenu), "ShowActorMassage")]
    public static class ActorMenu_ShowActorMassage_Patch2
    {
        static void Postfix(ActorMenu __instance, int key)
        {
            if (!Main.enabled || Main.settings.showAllMassage)
                return;
            if (!DateFile.instance.actorLifeMassage.ContainsKey(key)) return;
            if (Changer.GetBackupId() == key)
            {
                //Main.Logger.Log("Exit-----" + "ID:" + key + "Count:" + changer.getBackupMassage(key).Count);
                DateFile.instance.actorLifeMassage[key] = Changer.GetBackupMassage();
                //Main.Logger.Log("ExitReset-----" + changer.getActorName(key) + "Count:" + DateFile.instance.actorLifeMassage[key].Count);
                Changer.ResetBackup();
            }

        }
    }
}


