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
using BaseResourceMod;


namespace Majordomo
{
    public class Settings : UnityModManager.ModSettings
    {
        
        public bool autoHarvestItems = true;        // 自动收获物品
        public bool autoHarvestActors = true;       // 自动接纳新村民


        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }


    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public const string RES_PATH_BASE = "resources";
        public const string RES_PATH_TXT = "txt";
        public const string RES_PATH_SPRITE = "Texture";


        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Main.Logger = modEntry.Logger;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            var resBasePath = System.IO.Path.Combine(modEntry.Path, RES_PATH_BASE);
            var resTxtPath = System.IO.Path.Combine(resBasePath, RES_PATH_TXT);
            var resSpritePath = System.IO.Path.Combine(resBasePath, RES_PATH_SPRITE);
            BaseResourceMod.Main.registModResourceDir(modEntry, resTxtPath, resSpritePath);

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
            GUILayout.BeginHorizontal();
            GUILayout.Label("自动收获");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Main.settings.autoHarvestItems = GUILayout.Toggle(Main.settings.autoHarvestItems, "自动收获物品");
            Main.settings.autoHarvestActors = GUILayout.Toggle(Main.settings.autoHarvestActors, "自动接纳新村民");
            GUILayout.EndHorizontal();
        }


        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Main.settings.Save(modEntry);
        }
    }


    // 月初自动工作入口
    [HarmonyPatch(typeof(UIDate), "SetTrunChangeWindow")]
    public static class UIDate_SetTrunChangeWindow_Patch
    {
        private static bool Prefix(ref UIDate __instance)
        {
            if (!Main.enabled) return true;

            AutoHarvest.GetAllBooties();

            AutoHarvest.RegisterEvent(ref __instance);

            return true;
        }
    }


    // 月初事件图标文字
    [HarmonyPatch(typeof(WindowManage), "WindowSwitch")]
    public static class WindowManage_WindowSwitch_Patch
    {
        static void Postfix(WindowManage __instance, bool on, GameObject tips)
        {
            if (!Main.enabled) return;

            AutoHarvest.SetEventText(__instance, on, tips);
        }
    }
}
