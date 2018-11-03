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

        public bool showItemExtraInBag = true;//显示包裹中物品特殊词条
        public bool showItemExtraInEquuipBag = true;//显示装备界面包裹中物品特殊词条
        public bool showItemExtraInBank = true;//显示仓库中物品特殊词条
        public bool showItemExtraInShop = true;//显示商店中物品特殊词条
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
            GUILayout.Label("物品词条");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Main.settings.showItemExtraInBag = GUILayout.Toggle(Main.settings.showItemExtraInBag, "显示包裹中物品特殊词条", new GUILayoutOption[0]);
            Main.settings.showItemExtraInEquuipBag = GUILayout.Toggle(Main.settings.showItemExtraInEquuipBag, "显示装备界面包裹中物品特殊词条", new GUILayoutOption[0]);
            Main.settings.showItemExtraInBank = GUILayout.Toggle(Main.settings.showItemExtraInBank, "显示仓库中物品特殊词条", new GUILayoutOption[0]);
            Main.settings.showItemExtraInShop = GUILayout.Toggle(Main.settings.showItemExtraInShop, "显示商店中物品特殊词条", new GUILayoutOption[0]);
            Main.settings.showExtraValue = GUILayout.Toggle(Main.settings.showExtraValue, "显示加成数值", new GUILayoutOption[0]);

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

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
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
            if (!Main.enabled || !Main.settings.showItemExtraInBag)
                return;
            int typ = int.Parse(DateFile.instance.GetItemDate(itemId, 1, false));
            //只针对宝物的属性处理
            if (typ == 3)
            {
                foreach (var item in Main.itemExtraAttr)
                {
                    int val = int.Parse(DateFile.instance.GetItemDate(itemId, item.Key, false));
                    if (val > 0)
                    {
                        //使用/n换行后无法显示耐久，直接接属性名后方则耐久显示不全，暂时只显示属性
                        __instance.itemNumber.text = !Main.settings.showExtraValue ? item.Value : item.Value + val;
                        break;

                    }
                }
            }
        }
    }

    //将人物装备界面中物品栏的宝物耐久度替换为特殊词条显示
    [HarmonyPatch(typeof(SetItem), "SetActorEquipIcon")]
    public static class SetItem_SetActorEquipIcon_Patch
    {
        static void Postfix(SetItem __instance, int itemId)
        {
            if (!Main.enabled || !Main.settings.showItemExtraInEquuipBag)
                return;
            int typ = int.Parse(DateFile.instance.GetItemDate(itemId, 1, false));
            //只针对宝物的属性处理
            if (typ == 3)
            {
                foreach (var item in Main.itemExtraAttr)
                {
                    int val = int.Parse(DateFile.instance.GetItemDate(itemId, item.Key, false));
                    if (val > 0)
                    {
                        //使用/n换行后无法显示耐久，直接接属性名后方则耐久显示不全，暂时只显示属性
                        __instance.itemNumber.text = !Main.settings.showExtraValue ? item.Value : item.Value + val;
                        break;
                    }
                }
            }
        }
    }



    //将仓库中的的宝物耐久度替换为特殊词条显示
    [HarmonyPatch(typeof(SetItem), "SetWarehouseItemIcon")]
    public static class SetItem_SetWarehouseItemIcon_Patch
    {
        static void Postfix(SetItem __instance, int actorId, int itemId, bool cantTake, Image itemDragDes = null, int itemDragTyp = -1)
        {


            if (!Main.enabled || !Main.settings.showItemExtraInBank)
                return;
            string actorName = "";
            actorName = DateFile.instance.GetActorName(actorId, true, false);
            string itemName = DateFile.instance.GetItemDate(itemId, 0, false);

            int typ = int.Parse(DateFile.instance.GetItemDate(itemId, 1, false));

            //只针对宝物的属性处理
            if (typ == 3)
            {
                foreach (var item in Main.itemExtraAttr)
                {
                    int val = int.Parse(DateFile.instance.GetItemDate(itemId, item.Key, false));
                    if (val > 0)
                    {
                        //使用/n换行后无法显示耐久，直接接属性名后方则耐久显示不全，暂时只显示属性
                        __instance.itemNumber.text = !Main.settings.showExtraValue ? item.Value : item.Value + val;
                        break;
                    }
                }
            }
        }
    }

    //将商店中的的宝物耐久度替换为特殊词条显示
    [HarmonyPatch(typeof(SetItem), "SetShopItemIcon")]
    public static class SetItem_SetShopItemIcon_Patch
    {
        static void Postfix(SetItem __instance, int itemId, int itemSize, int desIndex, int changeTyp, int itemMoney, Image dragDes)
        {
            if (!Main.enabled || !Main.settings.showItemExtraInShop)
                return;
            int typ = int.Parse(DateFile.instance.GetItemDate(itemId, 1, false));
            //只针对宝物的属性处理
            if (typ == 3)
            {
                foreach (var item in Main.itemExtraAttr)
                {
                    int val = int.Parse(DateFile.instance.GetItemDate(itemId, item.Key, false));
                    if (val > 0)
                    {
                        //使用/n换行后无法显示耐久，直接接属性名后方则耐久显示不全，暂时只显示属性
                        __instance.itemNumber.text = !Main.settings.showExtraValue ? item.Value : item.Value + val;
                        break;
                    }
                }
            }
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
            //功法所属门派
            if (Main.settings.showGongFaGang)
            {
                int gangId = int.Parse(DateFile.instance.gongFaDate[gongFaId][3]);
                string gangName = DateFile.instance.presetGangDate[gangId][0];
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
                __instance.storyTime.text = string.Format("难度:{0}时间{1}" , level ,storyTime);

                Main.Logger.Log("XXXXXXXXXXXX :" + storyTime);
            }
            else
            {
                __instance.storyTime.text = "难度:"+ level;
                Main.Logger.Log("AAAAAAAAAAAAAA :" + storyTime);
            }
        }
    }
          
}


