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
        public bool just50Hard = true;
        public bool autoUsePower4 = false;
        public bool autoUsePower5 = false;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

    }

    public class QuickReadPlan
    {
        public int[] power3Num;
        public int[] power6Num;
        public int[] addPower3Num;
        public int[] addPower4Num;

        public QuickReadPlan()
        {
            power3Num = new int[] { 0, 0 };
            power6Num = new int[] { 0, 0 };
            addPower3Num = new int[] { 0, 0 };
            addPower4Num = new int[] { 0, 0 };
        }
    }

    static class Main
    {
        public static bool enabled;

        public static bool isRead = false;

        public static int readNum = 0;

        public static Settings settings;

        public static QuickReadPlan plan;

        public static UnityModManager.ModEntry.ModLogger Logger;


        public static void GetReadNum()
        {//计算已读页数
            int bookId = int.Parse(DateFile.instance.GetItemDate(BuildingWindow.instance.readBookId, 32, true));
            int[] bookPages = (BuildingWindow.instance.studySkillTyp != 17)
                ? ((!DateFile.instance.skillBookPages.ContainsKey(bookId)) ? new int[10] : DateFile.instance.skillBookPages[bookId])
                : ((!DateFile.instance.gongFaBookPages.ContainsKey(bookId)) ? new int[10] : DateFile.instance.gongFaBookPages[bookId]);
            readNum = 0;
            for (int i = 0; i < 10; i++)
            {
                if (bookPages[i] != 0)
                {
                    readNum++;
                }
            }
        }
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            Logger = modEntry.Logger;

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
            settings.just50Hard = GUILayout.Toggle(settings.just50Hard, "仅对50%研读难度的书籍生效");
            GUILayout.Label("说明： ");
            GUILayout.Label("仅对已读完的书籍有效，最低悟性要求是上限足够放一个【温故知新】。");
            GUILayout.Label("根据玩家初始耐心选取最优刷历练方案并自动实施，刷历练期间请不要手动释放技能。");
            GUILayout.Label("研读难度较高的书籍历练收益可能低于预期甚至低于低品书。");
            GUILayout.Label("已尝试97悟性70耐心下能正常刷历练 过低的悟性可能不适合刷历练 多学点加悟性的内功吧 --- v1.0.9修复");
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
            {
                return;
            }
            Main.GetReadNum();
            if (Main.readNum == 10)
            {
                Main.isRead = true;
            }
            else
            {
                Main.isRead = false;
            }
            if (Main.isRead)
            {
                Main.plan = new QuickReadPlan();
                int num = ReadBook.instance.GetMaxPatience();
                while (num != 1)
                {
                    num = (int)Math.Ceiling((double)num / 2.0);
                    Main.plan.power6Num[0]++;
                }
                Main.plan.power3Num[0] = 30 - Main.plan.power6Num[0];
                Main.Logger.Log($"可以温 {Main.plan.power6Num[0]} 次");
            }
        }
    }

    [HarmonyPatch(typeof(ReadBook), "UpdateRead")]
    static class ReadBook_UpdateRead_Patch
    {
        static void Postfix()
        {
            if (!Main.enabled)
            {
                return;
            }
            Main.GetReadNum();
            Type type = ((object)ReadBook.instance).GetType();
            int num = (int)type.InvokeMember("readPageIndex", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField, null, ReadBook.instance, null);
            int num2 = (int)type.InvokeMember("readLevel", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField, null, ReadBook.instance, null);
            int actorValue = (int)type.InvokeMember("actorValue", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField, null, ReadBook.instance, null);
            int readSkillId = int.Parse(DateFile.instance.GetItemDate(BuildingWindow.instance.readBookId, 32));
            int needInt = BuildingWindow.instance.GetNeedInt(actorValue, readSkillId);
            if (!Main.isRead)
            {
                return;
            }
            int num3 = int.Parse(DateFile.instance.readBookDate[6][1]) * needInt / 100;
            if (!Main.settings.autoRead || (Main.settings.just50Hard && needInt > 50) || DateFile.instance.BaseAttr(DateFile.instance.mianActorId, 4, 0) < num3)
            {
                return;
            }
            List<int[]> list = (List<int[]>)type.InvokeMember("pageState", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField, null, ReadBook.instance, null);
            if (num2 != 0 && num2 != 50)
            {
                return;
            }
            int num4 = Main.plan.power6Num[0] % 3;
            if (Main.plan.power3Num[0] > Main.plan.power3Num[1] && num <= 9 - Main.plan.power6Num[0] / 3)
            {
                if (num == 9 - Main.plan.power6Num[0] / 3)
                {
                    int num5 = 0;
                    int num6 = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        if (list[num][i] == 0)
                        {
                            num5++;
                        }
                        else if (list[num][i] == 6)
                        {
                            num6++;
                        }
                    }
                    if (num6 == num4)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (list[num][j] == 0)
                            {
                                ReadBook.instance.UseIntPower(3);
                                if (list[num][j] == 3)
                                {
                                    Main.plan.power3Num[1]++;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int k = 0; k < 3 - num4; k++)
                        {
                            if (list[num][k] == 0)
                            {
                                ReadBook.instance.UseIntPower(3);
                                if (list[num][k] == 3)
                                {
                                    Main.plan.power3Num[1]++;
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int l = 0; l < 3; l++)
                    {
                        if (list[num][l] == 0)
                        {
                            ReadBook.instance.UseIntPower(3);
                            if (list[num][l] == 3)
                            {
                                Main.plan.power3Num[1]++;
                            }
                        }
                    }
                }
            }
            if (Main.plan.power3Num[0] >= Main.plan.power3Num[1] && Main.plan.power6Num[0] > Main.plan.power6Num[1] && num == 9 - Main.plan.power6Num[0] / 3)
            {
                int num7 = 0;
                for (int m = 0; m < 3; m++)
                {
                    if (list[num][m] == 0 && num7 < num4)
                    {
                        ReadBook.instance.UseIntPower(6);
                        if (list[num][m] == 6)
                        {
                            Main.plan.power6Num[1]++;
                        }
                    }
                    else if (list[num][m] == 6)
                    {
                        num7++;
                    }
                }
            }
            if (Main.plan.power6Num[0] <= Main.plan.power6Num[1] || num <= 9 - Main.plan.power6Num[0] / 3)
            {
                return;
            }
            for (int n = 0; n < 3; n++)
            {
                if (list[num][n] == 0)
                {
                    ReadBook.instance.UseIntPower(6);
                    if (list[num][n] == 6)
                    {
                        Main.plan.power6Num[1]++;
                    }
                }
            }
        }
    }
}