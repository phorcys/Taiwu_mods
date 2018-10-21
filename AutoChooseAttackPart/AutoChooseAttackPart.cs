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

namespace AutoChooseAttackPart//变招时自动选择变招
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        public const int PartCount = 8;
        public const int NoPart = 7;
        public int priorityPart= NoPart;//非负整数 变招的优先部位，NoPart=不变，0~NoPart=变
    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static String[] toolBarText = { "胸背", "腰", "头", "左臂", "右臂", "左腿", "右腿", "不变" };

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
            Main.settings.priorityPart = GUILayout.Toolbar(Main.settings.priorityPart, toolBarText);
            GUILayout.EndHorizontal();

        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }
    //在显示变招8按钮后点击其中一个。
    [HarmonyPatch(typeof(BattleSystem), "SetAttPartChooseButton", new Type[] { typeof(int) })]
    public static class BattleSystem_SetAttPartChooseButton_Patch
    {
        static void Postfix(BattleSystem __instance)
        {
            if (Main.enabled)
            {
                if (__instance.attackPartChooseButton.Count() < Settings.PartCount)
                {
                    Main.Logger.Log("AutoChooseAttackPart Mod Down\n");
                    return;
                }
                int index = Settings.NoPart;//默认=不变招=button[7]
                if (Main.settings.priorityPart != Settings.NoPart)
                    if (__instance.attackPartChooseButton[Main.settings.priorityPart].IsInteractable())
                        index = Main.settings.priorityPart;
                BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
                Type type = __instance.attackPartChooseButton[index].GetType();
                type.GetMethod("Press", flag).Invoke(__instance.attackPartChooseButton[index], null);
            }
        }
    }
}
