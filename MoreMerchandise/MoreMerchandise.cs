using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Reflection.Emit;
using System.Runtime.Serialization;


namespace MoreMerchandise
{
    public class Settings : UnityModManager.ModSettings
    {
        // 是否隐藏低品级商品
        public bool hideLowQuality = false;
        // 隐藏低于此品级的商品
        public int lowQualityThreshold = 9;
        // 富裕因子
        public int wealthyFactor = 1;


        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }


        // 获取随游戏难度而变化的富裕倍数
        public static int GetWealthyMultiple()
        {
            // DateFile.instance.worldResource: 0: 桃源, 1: 普通, 2: 贫瘠，3: 荒芜
            return (3 - DateFile.instance.worldResource) * Main.settings.wealthyFactor + 1;
        }
    }


    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;


        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Main.Logger = modEntry.Logger;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Main.settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = Main.OnToggle;
            modEntry.OnGUI = Main.OnGUI;
            modEntry.OnSaveGUI = Main.OnSaveGUI;
            return true;
        }


        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Main.enabled = value;
            return true;
        }


        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal(GUILayout.Width(400));

            Main.settings.hideLowQuality = GUILayout.Toggle(Main.settings.hideLowQuality, "隐藏低品级商品");

            GUILayout.FlexibleSpace();
            GUILayout.Label("隐藏低于此品级的商品：");

            var lowQualityThreshold = GUILayout.TextField(Main.settings.lowQualityThreshold.ToString(), 1);
            if (GUI.changed && !int.TryParse(lowQualityThreshold, out Main.settings.lowQualityThreshold))
            {
                Main.settings.lowQualityThreshold = 9;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }


        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Main.settings.Save(modEntry);
        }
    }


    // 商店商品生成方法
    [HarmonyPatch(typeof(ShopSystem), "SetShopItems")]
    public static class ShopSystem_SetShopItems_Patch
    {
        // 更改商店商品生成逻辑，使物品数量增加
        private static bool Prefix(ref ShopSystem __instance, int shopTyp, int moneyCost, int levelAdd, int isTaiWu, int shopActorId,
            ref int ___actorShopId, ref int ___shopSystemTyp, ref int ___showLevelAdd,
            ref int ___newShopLevel, ref int ___shopSellCost, ref int ___shopSystemCost)
        {
            if (!Main.enabled) return true;

            Merchandise.SetShopItems(ref __instance, shopTyp, moneyCost, levelAdd, isTaiWu, shopActorId,
                ref ___actorShopId, ref ___shopSystemTyp, ref ___showLevelAdd,
                ref ___newShopLevel, ref ___shopSellCost, ref ___shopSystemCost);

            return false;
        }


        // 隐藏低品级商品
        // *** 在这个地方改，会导致交易之后隐藏的商品真的消失 ***
        // 应该在 ShopSystem::SetItems 方法的 actor == false 的流程中的 MakeItem 方法调用时过滤，但是太麻烦了，暂时不想改
        private static void Postfix(ref ShopSystem __instance)
        {
            if (!Main.enabled) return;

            if (Main.settings.hideLowQuality)
            {
                __instance.shopItems = Merchandise.FilterLowQualityItems(__instance.shopItems);
            }
        }
    }


    // 商店金钱生成方法
    [HarmonyPatch(typeof(DateFile), "UpdateShopMoney")]
    public static class DateFile_UpdateShopMoney_Patch
    {
        // 更改商店金钱生成逻辑，使金钱增加
        private static bool Prefix(ref DateFile __instance)
        {
            if (!Main.enabled) return true;

            MerchantMoney.UpdateShopMoney(ref __instance);

            return false;
        }
    }
}
