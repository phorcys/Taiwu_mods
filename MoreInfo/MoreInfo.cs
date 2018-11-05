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

        public bool showInBag = true;//显示包裹中物品特殊词条
        public bool showInEquuipBag = true;//显示装备界面包裹中物品特殊词条
        public bool showInBank = true;//显示仓库中物品特殊词条
        public bool showInShop = true;//显示商店中物品特殊词条
        public bool showExtraValue = false;//显示特殊词条加成值


        public bool showGongFaLevel = true;//显示功法等级颜色
        public bool showGongFaGang = true;//显示功法所属门派
        public bool showGongFaProgress = true;//强化显示功法进度
        public bool showInStudyWindow = true;//在修习界面显示
        public bool showInLevelUpWindow = true;//在突破界面显示

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
            GUILayout.Label("显示功能");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Main.settings.showExtraName = GUILayout.Toggle(Main.settings.showExtraName, "显示宝物特殊词条", new GUILayoutOption[0]);
            Main.settings.showBookGang = GUILayout.Toggle(Main.settings.showBookGang, "显示书籍门派", new GUILayoutOption[0]);
            Main.settings.showBlueprintName = GUILayout.Toggle(Main.settings.showBlueprintName, "显示图纸名称", new GUILayoutOption[0]);

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("显示区域");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Main.settings.showInBag = GUILayout.Toggle(Main.settings.showInBag, "包裹中显示", new GUILayoutOption[0]);
            Main.settings.showInEquuipBag = GUILayout.Toggle(Main.settings.showInEquuipBag, "装备界面包裹显示", new GUILayoutOption[0]);
            Main.settings.showInBank = GUILayout.Toggle(Main.settings.showInBank, "仓库中显示", new GUILayoutOption[0]);
            Main.settings.showInShop = GUILayout.Toggle(Main.settings.showInShop, "商店中显示", new GUILayoutOption[0]);
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

            GUILayout.EndHorizontal();
        }


        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }



    public class Changer
    {

        public static Dictionary<int, string> itemExtraAttr = new Dictionary<int, string>()
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

        //获取物品名称
        public string getItemName(int itemId)
        {
            return DateFile.instance.GetItemDate(itemId, 0, false);
        }

        //获取特殊词条的名称
        public string getItemExtraName(int itemId)
        {
            string itemName = this.getItemName(itemId);

            string value = "";
            foreach (var item in itemExtraAttr)
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

        //变更文本内容
        public void changeText(Text text, string newText)
        {
            text.text = newText;
        }

        //变更宝物名称
        public void changeDecName(Text text, int itemId)
        {
            string extraName = getItemExtraName(itemId);
            if (extraName != "")
            {
                changeText(text, extraName);
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
                    changeDecName(text, itemId);
                    break;
            }

        }

        //变更图纸名称
        public void changeBlueprintName(Text text, int itemId)
        {
            if (!Main.settings.showBlueprintName) return;
            string name = getItemName(itemId);
            changeText(text, name);
        }

        //变更书籍名称
        public void changeBookName(Text text, int itemId)
        {
            if (!Main.settings.showBookGang) return;
            int typ3 = getItemThirdType(itemId);
            //4-19为技艺，20-33为功法
            if (typ3 >= 20)
            {
                int gongFaId = getGongFaId(itemId);
                string gangName = getGangName(gongFaId);
                text.text = gangName + "\n" + text.text;
                //changeText(text, gangName);
            }
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

        //变更物品名称
        public void changeItemName(Text text, int itemId)
        {
            int typ = this.getItemType(itemId);
            switch (typ)
            {
                case 4:
                    changeEquipName(text, itemId);
                    break;
                case 5:
                    changeType5Name(text, itemId);
                    break;
            }

        }
    }

    //50061-50066 膂力- 定力
    //51361-51366 膂力%- 悟性% 
    //51367 - 51372 护体% - 闪避%
    //50501 - 50516 音律 - 织锦
    //50601 - 50614 内功 - 杂学
    //DateFile.instance.actorAttrDate actorAttr_date.txt

    //将人物包裹中的的宝物耐久度替换为特殊词条显示
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

    //将人物装备界面中物品栏的宝物耐久度替换为特殊词条显示
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


    //将仓库中的的宝物耐久度替换为特殊词条显示
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

    //将商店中的的宝物耐久度替换为特殊词条显示
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

}


