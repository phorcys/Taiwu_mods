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
        public bool showItemExtraInBag = true;//显示包裹中物品特殊词条
        public bool showItemExtraInBank = true;//显示仓库中物品特殊词条
        public bool showItemExtraInShop = true;//显示商店中物品特殊词条
        public bool showGongFaLevel = true;//显示功法等级颜色

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
            Main.settings.showItemExtraInBag = GUILayout.Toggle(Main.settings.showItemExtraInBag, "显示包裹中物品特殊词条", new GUILayoutOption[0]);
            Main.settings.showItemExtraInBank = GUILayout.Toggle(Main.settings.showItemExtraInBank, "显示仓库中物品特殊词条", new GUILayoutOption[0]);
            Main.settings.showItemExtraInShop = GUILayout.Toggle(Main.settings.showItemExtraInShop, "显示商店中物品特殊词条", new GUILayoutOption[0]);
            Main.settings.showGongFaLevel = GUILayout.Toggle(Main.settings.showGongFaLevel, "显示功法等级颜色", new GUILayoutOption[0]);

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
            if ( typ == 3 )
            {
                foreach (var item in Main.itemExtraAttr)
                {
                    int val = int.Parse(DateFile.instance.GetItemDate(itemId, item.Key, false));
                    if (val > 0)
                    {
                          //使用/n换行后无法显示耐久，直接接属性名后方则耐久显示不全，暂时只显示属性
                        __instance.itemNumber.text = item.Value;
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
                        __instance.itemNumber.text = item.Value;
                    }
                    Main.Logger.Log(string.Format("MoreInfo---UpdateItems-{0}-{1}-{2}-{3}-value-{4}", actorName, itemId, itemName, item.Value, val));
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
                        __instance.itemNumber.text = item.Value;
                    }
                }
            }
        }
    }

}



/*  

    ;*/
