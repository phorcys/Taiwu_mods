using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
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



        public bool isLoaded = false;//是否为首次Loading

        public bool showExtraName = true;//显示特殊词条
        public bool showBookGang = true;//显示书籍门派
        public bool showBlueprintName = true;//显示图纸名称
        public bool showOtherBookAbility = true;//显示技艺书对应技艺名称
        public bool showMagerialName = true;//显示材料名称
        public bool showFoodExtraName = true;//显示食物特殊词条




        public bool showInBag = true;//包裹中物品显示
        public bool showInEquuipBag = true;//装备界面包裹中物品显示
        public bool showInBank = true;//仓库中物品显示
        public bool showInShop = true;//商店中物品显示
        public bool showInStory = true;//奇遇使用物品界面显示

        public bool showExtraValue = false;//显示特殊词条加成值


        public bool showGongFaLevel = true;//显示功法等级颜色
        public bool showGongFaGang = true;//显示功法所属门派
        public bool showGongFaProgress = true;//强化显示功法进度
        public bool showInStudyWindow = true;//在修习界面显示
        public bool showInLevelUpWindow = true;//在突破界面显示

        public bool showStroyLevel = true;//显示奇遇等级
        public bool mergeIcon = true;//合并切换时节时入魔图标
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
            GUILayout.Label("显示功能");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Main.settings.showExtraName = GUILayout.Toggle(Main.settings.showExtraName, "显示宝物特殊词条", new GUILayoutOption[0]);
            Main.settings.showFoodExtraName = GUILayout.Toggle(Main.settings.showFoodExtraName, "显示食物特殊词条", new GUILayoutOption[0]);
            Main.settings.showBookGang = GUILayout.Toggle(Main.settings.showBookGang, "显示书籍门派", new GUILayoutOption[0]);
            Main.settings.showBlueprintName = GUILayout.Toggle(Main.settings.showBlueprintName, "显示图纸名称", new GUILayoutOption[0]);
            Main.settings.showOtherBookAbility = GUILayout.Toggle(Main.settings.showOtherBookAbility, "显示技艺书技艺", new GUILayoutOption[0]);
            Main.settings.showMagerialName = GUILayout.Toggle(Main.settings.showMagerialName, "显示材料类别", new GUILayoutOption[0]);



            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("显示区域");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Main.settings.showInBag = GUILayout.Toggle(Main.settings.showInBag, "包裹中显示", new GUILayoutOption[0]);
            Main.settings.showInEquuipBag = GUILayout.Toggle(Main.settings.showInEquuipBag, "装备界面包裹显示", new GUILayoutOption[0]);
            Main.settings.showInBank = GUILayout.Toggle(Main.settings.showInBank, "仓库中显示", new GUILayoutOption[0]);
            Main.settings.showInShop = GUILayout.Toggle(Main.settings.showInShop, "商店中显示", new GUILayoutOption[0]);
            Main.settings.showInStory = GUILayout.Toggle(Main.settings.showInStory, "奇遇物品栏显示", new GUILayoutOption[0]);

            //Main.settings.showExtraValue = GUILayout.Toggle(Main.settings.showExtraValue, "显示加成数值", new GUILayoutOption[0]);

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("功法增强");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Main.settings.showGongFaLevel = GUILayout.Toggle(Main.settings.showGongFaLevel, "显示功法等级颜色", new GUILayoutOption[0]);
            Main.settings.showGongFaGang = GUILayout.Toggle(Main.settings.showGongFaGang, "显示功法所属门派", new GUILayoutOption[0]);
            Main.settings.showGongFaProgress = GUILayout.Toggle(Main.settings.showGongFaProgress, "进度心得显示增强", new GUILayoutOption[0]);
            Main.settings.showInStudyWindow = GUILayout.Toggle(Main.settings.showInStudyWindow, "在修习界面显示", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("其他");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Main.settings.showStroyLevel = GUILayout.Toggle(Main.settings.showStroyLevel, "显示奇遇等级", new GUILayoutOption[0]);
            Main.settings.mergeIcon = GUILayout.Toggle(Main.settings.mergeIcon, "合并入魔图标", new GUILayoutOption[0]);


            GUILayout.EndHorizontal();
        }


        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }



    public class Changer
    {



        //50061-50066 膂力- 定力
        //51361-51366 膂力%- 悟性% 
        //51367 - 51372 护体% - 闪避%
        //50501 - 50516 音律 - 织锦
        //50601 - 50614 内功 - 杂学
        //DateFile.instance.actorAttrDate actorAttr_date.txt

        public static Dictionary<int, string> itemExtraAttrType1 = new Dictionary<int, string>()
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

        public static Dictionary<int, string> itemExtraAttrType2 = new Dictionary<int, string>()
               {
                {51361,"膂力%"},
                {51362,"体质%"},
                {51363,"灵敏%"},
                {51364,"根骨%"},
                {51365,"悟性%"},
                {51366,"定力%"},
               };

        //材料分类
        public static Dictionary<int, string> materialType = new Dictionary<int, string>()
               {
                    { 0,""},
                    { 1,"硬"},
                    { 2,"软"},
                    { 7,"铁"},
                    { 8,"木"},
                    { 11,"布"},
                    { 12,"玉"},
               };



        public static string placeIds = "";
        public void addPlaceId(int pid)
        {
            placeIds += pid + "|";
        }
        public void resetPlaceIds()
        {
            placeIds = "";
        }
        public string[] getSplitPlaceIds()
        {
            return placeIds.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        }
        public string getPlaceIds()
        {
            return placeIds;
        }
        public Changer()
        {

        }

        //获取人物名字
        public string getActorName(int actorId)
        {
            return DateFile.instance.GetActorName(actorId, true, false);
        }

        //获取功法所属门派Id
        public int getGangId(int gongFaId)
        {
            return int.Parse(DateFile.instance.gongFaDate[gongFaId][3]);
        }
        //获取功法所属门派名称
        public string getGangName(int gongFaId)
        {
            int gangId = getGangId(gongFaId);
            return DateFile.instance.presetGangDate[gangId][0];
        }

        //获取书籍对应的功法ID
        public int getGongFaId(int itemId)
        {
            int bookTyp = int.Parse(DateFile.instance.GetItemDate(itemId, 999, false));
            //500000以上为普通秘籍，700000以上为手抄
            return isOriginalBook(itemId) ? (bookTyp - 500000) : (bookTyp - 700000);
        }

        //判断秘籍是真传还是手抄
        public bool isOriginalBook(int itemId)
        {
            //序列35 0为普通,1为手抄本
            return int.Parse(DateFile.instance.GetItemDate(itemId, 35, false)) == 0;
        }

        //获取物品大类
        public int getItemType(int itemId)
        {
            return int.Parse(DateFile.instance.GetItemDate(itemId, 4, false));
        }

        //获取物品小类
        public int getItemSecondType(int itemId)
        {
            return int.Parse(DateFile.instance.GetItemDate(itemId, 5, false));
        }

        //获取物品细类
        public int getItemThirdType(int itemId)
        {
            return int.Parse(DateFile.instance.GetItemDate(itemId, 506, false));
        }

        //物品制作类型
        public int getMakeType(int itemId)
        {
            //0为材料包，7铁8木9药10毒11布12玉15食材
            return int.Parse(DateFile.instance.GetItemDate(itemId, 41, false));
        }

        //物品制作成品方向
        public int getProductType(int itemId)
        {
            //0无法制作-装备类1硬2软
            return int.Parse(DateFile.instance.GetItemDate(itemId, 48, false));
        }

        //获取物品名称
        public string getItemName(int itemId)
        {
            return DateFile.instance.GetItemDate(itemId, 0, false);
        }

        //获取一类词条的名称
        public string getItemExtraNameType1(int itemId)
        {
            string itemName = this.getItemName(itemId);

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

        //获取二类词条的名称
        public string getItemExtraNameType2(int itemId)
        {
            string itemName = this.getItemName(itemId);

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



        //变更文本内容
        public void changeText(Text text, string newText)
        {
            text.text = newText;
        }

        //按照特殊词条类型1显示名称（功法技艺加成等）add决定了是否同时显示耐久。耐久度大于10的显示不下
        public void changeDecNameType1(Text text, int itemId, bool add = false)
        {
            string extraName = getItemExtraNameType1(itemId);
            if (extraName != "")
            {
                text.text = add ? extraName + text.text : extraName;
            }
        }
        //按照特殊词条类型2显示名称装备的膂力-悟性等百分比加成
        public void changeDecNameType2(Text text, int itemId, bool add = false)
        {
            string extraName = getItemExtraNameType2(itemId);
            if (extraName != "")
            {
                text.text = add ? extraName + text.text : extraName;
            }
        }

        //变更图纸名称
        public void changeBlueprintName(Text text, int itemId)
        {
            if (!Main.settings.showBlueprintName) return;
            string name = getItemName(itemId);
            changeText(text, name);
        }

        //变更功法书名称
        public void changeGongFaBookName(Text text, int itemId)
        {
            if (!Main.settings.showBookGang) return;
            int gongFaId = getGongFaId(itemId);
            string gangName = getGangName(gongFaId);
            gangName = gangName.Substring(0, 2);
            text.text = gangName + text.text;
        }

        //变更技艺书名称
        public void changeAbilityBookName(Text text, int itemid, int typ3)
        {
            if (!Main.settings.showOtherBookAbility) return;
            string abilityName = itemExtraAttrType1[typ3 + 50501 - 4];
            text.text = abilityName + text.text;
        }

        //变更书籍名称
        public void changeBookName(Text text, int itemId)
        {
            int typ3 = getItemThirdType(itemId);
            //4-19为技艺，20-33为功法
            if (typ3 >= 20)
            {
                changeGongFaBookName(text, itemId);
            }
            else
            {
                if (typ3 < 20 && typ3 >= 4)
                {
                    changeAbilityBookName(text, itemId, typ3);
                }
            }
        }


        //变更制造类物品名称
        public void changeType1Name(Text text, int itemId)
        {
            if (!Main.settings.showMagerialName) return;
            int mtyp = getMakeType(itemId);

            if (mtyp == 0) //材料包
            {
                string name = getItemName(itemId);
                name = name.Substring(name.Length - 2);
                text.text = name + text.text;
            }
            int ptyp = getProductType(itemId);
            if (ptyp == 0) return;//只显示可制作物品的材料
            //暂不处理药材食材和毒药
            switch (mtyp)
            {
                case 7:
                    text.text = materialType[ptyp] + materialType[mtyp] + text.text;
                    break;
                case 8:
                    text.text = materialType[ptyp] + materialType[mtyp] + text.text;
                    break;
                case 11:
                    text.text = materialType[ptyp] + materialType[mtyp] + text.text;
                    break;
                case 12:
                    text.text = materialType[ptyp] + materialType[mtyp] + text.text;
                    break;
            }
        }


        //变更食物名称
        public void changeType3Name(Text text, int itemId)
        {
            if (!Main.settings.showFoodExtraName) return;
            changeDecNameType1(text, itemId, true);

        }

        //变更图书与图纸名称
        public void changeType5Name(Text text, int itemId)
        {
            int typ2 = getItemSecondType(itemId);
            switch (typ2)
            {
                case 20:
                    changeBlueprintName(text, itemId);
                    break;
                case 21:
                    changeBookName(text, itemId);
                    break;
            }
        }

        //变更装备名称
        public void changeEquipName(Text text, int itemId)
        {
            if (!Main.settings.showExtraName) return;
            int typ2 = getItemSecondType(itemId);
            switch (typ2)
            {
                case 13: //宝物
                    changeDecNameType1(text, itemId);
                    break;
                case 14: //帽子
                    changeDecNameType2(text, itemId);
                    break;
                case 15: //鞋子
                    changeDecNameType2(text, itemId);
                    break;
                case 16: //护甲
                    changeDecNameType2(text, itemId);
                    break;
            }

        }

        //变更Holder中所有object名称
        public void changeObjectsName(Transform __Holder)
        {
            int childCount = __Holder.childCount;
            for (int i = 0; i < childCount; i++)
            {
                GameObject gameObject;
                gameObject = __Holder.GetChild(i).gameObject;
                var gameText = gameObject.transform.Find("ItemNumberText").GetComponent<Text>();
                string[] tmpArray = gameObject.name.Split(new char[] { ',' });
                this.changeItemName(gameText, int.Parse(tmpArray[1]));
            }
        }

        //变更物品名称
        public void changeItemName(Text text, int itemId)
        {
            int typ = this.getItemType(itemId);
            switch (typ)
            {
                case 1:
                    changeType1Name(text, itemId);
                    break;
                case 3:
                    changeType3Name(text, itemId);
                    break;
                case 4:
                    changeEquipName(text, itemId);
                    break;
                case 5:
                    changeType5Name(text, itemId);
                    break;
            }

        }


        public static Changer instance;
    }



    //将人物包裹中的的物品替换为特殊词条显示
    [HarmonyPatch(typeof(SetItem), "SetActorMenuItemIcon")]
    public static class SetItem_SetActorMenuItemIcon_Patch
    {
        static void Postfix(SetItem __instance, int actorId, int itemId, int actorFavor, int injuryTyp)
        {
            if (!Main.enabled || !Main.settings.showInBag)
                return;
            Changer changer = new Changer();
            changer.changeItemName(__instance.itemNumber, itemId);
        }
    }

    //将人物装备界面中物品栏的物品替换为特殊词条显示
    [HarmonyPatch(typeof(SetItem), "SetActorEquipIcon")]
    public static class SetItem_SetActorEquipIcon_Patch
    {
        static void Postfix(SetItem __instance, int itemId)
        {
            if (!Main.enabled || !Main.settings.showInEquuipBag)
                return;
            Changer changer = new Changer();
            changer.changeItemName(__instance.itemNumber, itemId);
        }
    }


    //将仓库中的的物品替换为特殊词条显示
    [HarmonyPatch(typeof(SetItem), "SetWarehouseItemIcon")]
    public static class SetItem_SetWarehouseItemIcon_Patch
    {
        static void Postfix(SetItem __instance, int actorId, int itemId, bool cantTake, Image itemDragDes = null, int itemDragTyp = -1)
        {
            if (!Main.enabled || !Main.settings.showInBank)
                return;
            Changer changer = new Changer();
            changer.changeItemName(__instance.itemNumber, itemId);

        }
    }

    //将商店中的的物品替换为特殊词条显示
    [HarmonyPatch(typeof(SetItem), "SetShopItemIcon")]
    public static class SetItem_SetShopItemIcon_Patch
    {
        static void Postfix(SetItem __instance, int itemId, int itemSize, int desIndex, int changeTyp, int itemMoney, Image dragDes)
        {
            if (!Main.enabled || !Main.settings.showInShop)
                return;
            Changer changer = new Changer();
            changer.changeItemName(__instance.itemNumber, itemId);
        }
    }

    //奇遇选择物品界面设置物品名称
    [HarmonyPatch(typeof(StorySystem), "GetItem")]
    public static class StorySystem_GetItem_Patch
    {
        static void Postfix(StorySystem __instance, int typ)
        {
            if (!Main.enabled || !Main.settings.showInStory)
                return;
            Changer changer = new Changer();
            changer.changeObjectsName(__instance.itemHolder);
        }

    }



    //人物功法界面根据功法品级显示颜色
    [HarmonyPatch(typeof(SetGongFaIcon), "SetGongFa")]
    public static class SetGongFaIcon_SetGongFa_Patch
    {
        static void Postfix(SetGongFaIcon __instance, int gongFaId, int actorId)
        {
            if (!Main.enabled)
                return;

            Changer changer = new Changer();

            //功法所属门派
            if (Main.settings.showGongFaGang)
            {
                string gangName = changer.getGangName(gongFaId);
                __instance.gongFaSizeText.text = gangName + "\n" + __instance.gongFaSizeText.text;
            }

            //根据修习进度与心得变更颜色增加区分度
            if (Main.settings.showGongFaProgress)
            {
                int level = DateFile.instance.GetGongFaLevel(actorId, gongFaId)[0];
                int colorFix = level / 10;
                __instance.gongFaLevelText.text = DateFile.instance.SetColoer(20001 + colorFix, __instance.gongFaLevelText.text);
                int bookLevel = DateFile.instance.GetGongFaFLevel(actorId, gongFaId);
                __instance.gongFaBookLevelText.text = DateFile.instance.SetColoer(20001 + bookLevel, __instance.gongFaBookLevelText.text);
            }
        }
    }

    //HOOK掉功法颜色
    [HarmonyPatch(typeof(Loading), "LoadingScene")]
    public static class Loading_LoadingScene_Patch
    {
        static void Postfix(Loading __instance, int sceneId, int loadingDateId, bool newGame = false, bool stopBGM = true, int teachingId = -1, bool teachingEnd = false)
        {
            if (!Main.enabled || !Main.settings.showGongFaLevel)
                return;
            if (sceneId == 1)
            {
                Main.settings.isLoaded = false;
                return;
            }
            if (Main.settings.isLoaded)
                return;
            foreach (var item in DateFile.instance.gongFaDate)
            {
                var GData = item.Value;
                int lv = int.Parse(GData[2]);

                GData[0] = DateFile.instance.SetColoer(20001 + lv, GData[0]);
            }
            Main.settings.isLoaded = true;
        }
    }

    //奇遇显示等级
    [HarmonyPatch(typeof(WorldMapPlace), "UpdatePlaceStory")]
    public static class WorldMapPlace_UpdatePlaceStory_Patch
    {
        static void Postfix(WorldMapPlace __instance, int ___placeId)
        {
            if (!Main.enabled || !Main.settings.showStroyLevel)
                return;
            if (__instance.storyTime.text == "99") return;
            DateFile df = DateFile.instance;
            int pid = ___placeId;
            int partId = df.mianPartId;
            if (df.HaveShow(partId, pid) <= 0) return;
            if (!DateFile.instance.HaveStory(partId, pid)) return;
            if (!df.worldMapState[partId].ContainsKey(pid)) return;

            int storyId = df.worldMapState[partId][pid][0];
            string level = df.baseStoryDate[storyId][3];

            if (int.Parse(level) < 1) return;

            int storyTime = df.worldMapState[partId][pid][1];

            if (storyTime > 0)
            {
                __instance.storyTime.text = "难度:" + level + "时间:" + storyTime;
                __instance.storyTime.text = string.Format("难度:{0}时间{1}", level, storyTime);
            }
            else
            {
                __instance.storyTime.text = "难度:" + level;
            }
        }
    }

    //入魔图标
    [HarmonyPatch(typeof(UIDate), "SetTrunChangeWindow")]
    public static class UIDate_SetTrunChangeWindow_Patch
    {
        static void Prefix(UIDate __instance)
        {
            if (!Main.enabled || !Main.settings.mergeIcon)
                return;
            if (int.Parse(DateFile.instance.partWorldMapDate[DateFile.instance.mianPartId][101]) == 0)
            {
                __instance.changTrunEvents.Clear();
            }
            if (__instance.changTrunEvents.Count <= 0)
            {
                __instance.changTrunEvents.Add(new int[1]);
            }
            List<int[]> newEventList = new List<int[]>();
            bool isAdded = false;
            Changer changer = new Changer();
            changer.resetPlaceIds();
            for (int i = __instance.changTrunEvents.Count - 1; i > 0; i--)
            {
                int num2 = __instance.changTrunEvents[i][0];
                int num3 = int.Parse(DateFile.instance.trunEventDate[num2][1]);
                if (num3 > 0 && num2 == 248)
                {
                    int placeId = __instance.changTrunEvents[i][1];
                    string name = string.Format("TrunEventIcon,{0},{1},{2}", num2, placeId, __instance.changTrunEvents[i][2]);
                    string[] array = name.Split(new char[] { ',' });
                    if (!isAdded)//仅保留一个图标
                    {
                        newEventList.Add(__instance.changTrunEvents[i]);
                        isAdded = true;
                    }
                    changer.addPlaceId(placeId);
                }
                else
                {
                    newEventList.Add(__instance.changTrunEvents[i]);
                }
            }
            __instance.changTrunEvents = newEventList;
        }
    }


    //处理换季时入魔提示图标

    [HarmonyPatch(typeof(WindowManage), "WindowSwitch")]
    public static class WindowManage_WindowSwitch_Patch
    {
        static void Postfix(WindowManage __instance, bool on, GameObject tips)
        {
            if (!Main.enabled || !Main.settings.mergeIcon)
                return;
            bool flag = false;

            if (tips == null)
            {
                __instance.anTips = flag;
            }
            else
            {
                if (on)
                {
                    flag = true;
                    int num = DateFile.instance.MianActorID();
                    string[] array = tips.name.Split(new char[]
                    {
                    ','
                    });
                    Changer changer = new Changer();

                    int num2 = (array.Length <= 1) ? 0 : int.Parse(array[1]);
                    if ((num2 == 634 || num2 == 635) && StartBattle.instance.startBattleWindow.activeSelf && StartBattle.instance.enemyTeamId == 4)
                    {
                        flag = false;
                    }
                    DateFile df = DateFile.instance;
                    string tag = tips.tag;
                    switch (tag)
                    {
                        case "TrunEventIcon":
                            {
                                int num27 = int.Parse(DateFile.instance.trunEventDate[num2][1]);
                                __instance.informationName.text = DateFile.instance.trunEventDate[num2][0];
                                switch (num27)
                                {
                                    case 10:
                                        string[] placeArray = changer.getPlaceIds().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                        string placeNames = "";
                                        for (int i = 0; i < placeArray.Length; i++)
                                        {
                                            string pid = placeArray[i];
                                            string pName = DateFile.instance.partWorldMapDate[int.Parse(pid)][0];
                                            placeNames += pName + "、";
                                        }
                                        string pre = df.trunEventDate[num2][99].Split(new char[] { '|' })[0];
                                        string post = df.trunEventDate[num2][99].Split(new char[] { '|' })[1];
                                        __instance.informationMassage.text = string.Format("{0}{1}{2}\n", pre, df.SetColoer(10002, placeNames), post);
                                        break;
                                }
                            }
                            break;
                    }
                }
            }
        }
    }


}


