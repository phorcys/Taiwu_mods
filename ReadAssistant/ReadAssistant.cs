using Harmony12;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace ReadAssistant
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool autoRead = true;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

    }

    static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static int[,] powerNum = new int[,] { { 15, 16, 9 }, { 43, 15, 11 }, { 109, 16, 10 }, { 233, 15, 12 }, { 485, 14, 14 }, { 491, 16, 11 }, { 999, 15, 13 }, { 2019, 14, 15 }, { 2025, 16, 12 }, { 4069, 15, 14 }, { 8161, 14, 16 }, { 8167, 16, 13 }, { 16355, 15, 15 }, { 32741, 16, 14 }, { 65511, 17, 13 }, { 131049, 18, 12 } };
        public static int[] power36Num = { 0, 0, 0, 0 };//3总数|3使用|6总数|6使用

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;

            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginVertical("Box");
            settings.autoRead = GUILayout.Toggle(settings.autoRead, "读书刷历练助手");

            GUILayout.Label("说明： ");
            GUILayout.Label("根据玩家初始耐心选取最优刷历练方案并自动实施，刷历练期间请不要手动释放技能。");
            GUILayout.EndVertical();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }

    [HarmonyPatch(typeof(ReadBook), "ShowReadBookWindow")]
    static class ReadBook_ShowReadBookWindow_Patch
    {
        static void Postfix()
        {
            if (!Main.enabled)
                return;
            
            int i = 16;
            while (Main.powerNum[i, 0] > ReadBook.instance.GetMaxPatience())
            {
                i--;
            }
            Main.power36Num[0] = Main.powerNum[i, 1];
            Main.power36Num[2] = Main.powerNum[i, 2];
            Main.power36Num[1] = 0;
            Main.power36Num[3] = 0;
        }
    }

    [HarmonyPatch(typeof(ReadBook), "UpdateRead")]
    static class ReadBook_UpdateRead_Patch
    {
        static void Postfix()
        {
            if (!Main.enabled)
                return;

            Type type = ReadBook.instance.GetType();
            int actorValue = (int)type.InvokeMember("actorValue", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
            int readSkillId = int.Parse(DateFile.instance.GetItemDate(HomeSystem.instance.readBookId, 32, true));
            int needInt = HomeSystem.instance.GetNeedInt(actorValue, readSkillId);

            if (needInt != 50 || !Main.settings.autoRead)
                return;
            

            int readLevel = (int)type.InvokeMember("readLevel", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
            int patience = (int)type.InvokeMember("patience", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
            int canUseInt = (int)type.InvokeMember("canUseInt", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
            int readPageIndex = (int)type.InvokeMember("readPageIndex", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
            List<int[]> pageState = (List<int[]>)type.InvokeMember("pageState", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
            int cost3 = int.Parse(DateFile.instance.readBookDate[3][1]) * needInt / 100;
            int cost6 = int.Parse(DateFile.instance.readBookDate[6][1]) * needInt / 100;


            if (readLevel == 50)
            {
                while (Main.power36Num[0] > Main.power36Num[1] && readPageIndex <= 4 && pageState[readPageIndex][2] == 0 && canUseInt >= cost3)
                {
                    ReadBook.instance.UseIntPower(3);
                    Main.power36Num[1]++;
                }
                if (patience == 1)
                {
                    ReadBook.instance.UseIntPower(3);
                }
                if (Main.power36Num[2] > Main.power36Num[3] && readPageIndex == (9 - (Main.power36Num[3]/3)))
                {
                    ReadBook.instance.UseIntPower(6);
                    Main.power36Num[3]++;
                }
            }

        }
    }
}