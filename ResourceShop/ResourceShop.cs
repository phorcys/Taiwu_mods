using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace ResourceShop
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        public int resourceType = 0;
        public int resourceCount = 0;
    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static String[] toolBarText = { "食材", "木材", "金石", "织物", "药材" };
        public static float price=1.5f;
        //金钱在资源中的Index
        public const int MoneyIndex = 5;

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
            GUILayout.Label(string.Format("以{0}价格购买",price), GUILayout.Width(90));
            Main.settings.resourceType = GUILayout.Toolbar(Main.settings.resourceType, toolBarText);
            int.TryParse(GUILayout.TextField(Main.settings.resourceCount.ToString(), 5, GUILayout.Width(90)), out Main.settings.resourceCount);
            DateFile tbl = DateFile.instance;
            if (tbl == null || !GameData.Characters.HasChar(tbl.MianActorID()))
            {
                GUILayout.Label("  存档未载入!");
            }
            else
            {
                if (GUILayout.Button("确认购买"))
                    BuyResource();
            }
            GUILayout.EndHorizontal();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
        private static void BuyResource()
        {
            if (!Main.enabled)
                return;
            int id = DateFile.instance.MianActorID();
            int count = Main.settings.resourceCount;
            if (count < 0)
                count = 0;
            int cost =(int) Math.Ceiling(price * count);
            //int current_money= ActorMenu.instance.ActorResource(id)[MoneyIndex];
            int current_money = int.Parse(GameData.Characters.GetCharProperty(id, 406));
            if(cost>current_money)
            {
                cost = current_money;
                count =(int) Math.Floor(cost / price);
            }
            UIDate.instance.ChangeResource(id, Main.settings.resourceType, count, true);
            UIDate.instance.ChangeResource(id, MoneyIndex, -cost, true);
            Main.settings.resourceCount = 0;
        }
    }

}
