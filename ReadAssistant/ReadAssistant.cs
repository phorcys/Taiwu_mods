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
        //public static int[] power36Num = { 0, 0, 0, 0 };//3总数|3使用|6总数|6使用


        public static void GetReadNum()
        {//计算已读页数
            int bookId = int.Parse(DateFile.instance.GetItemDate(HomeSystem.instance.readBookId, 32, true));
            int[] bookPages = (HomeSystem.instance.studySkillTyp != 17)
                ? ((!DateFile.instance.skillBookPages.ContainsKey(bookId)) ? new int[10] : DateFile.instance.skillBookPages[bookId])
                : ((!DateFile.instance.gongFaBookPages.ContainsKey(bookId)) ? new int[10] : DateFile.instance.gongFaBookPages[bookId]);
            Main.readNum = 0;
            for (int i = 0; i < 10; i++)
            {
                if (bookPages[i] != 0)
                {
                    Main.readNum++;
                }
            }
        }
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
            settings.just50Hard = GUILayout.Toggle(settings.just50Hard, "仅对50%研读难度的书籍生效");
            GUILayout.Label("说明： ");
            GUILayout.Label("仅对已读完的书籍有效，最低悟性要求是上限足够放一个【温故知新】。");
            GUILayout.Label("根据玩家初始耐心选取最优刷历练方案并自动实施，刷历练期间请不要手动释放技能。");
            GUILayout.Label("研读难度较高的书籍历练收益可能低于预期甚至低于低品书。");

            GUILayout.Label(" ");

            GUILayout.Label("实验性功能：");
            settings.autoUsePower4 = GUILayout.Toggle(settings.autoUsePower4, "自动使用【唯融融】或【唯妙妙】");
            GUILayout.Label("遇到残卷时，若悟性足够则根据已读章节数自动使用【唯融融】或【唯妙妙】。");
            GUILayout.Label("想要使用【大智若愚】跳过残卷时不要开启。");
            settings.autoUsePower5 = GUILayout.Toggle(settings.autoUsePower5, "自动使用【迁思回虑】");
            GUILayout.Label("说明： ");
            GUILayout.Label("在耐心开始下降的前一时刻自动使用【迁思回虑】。");
            GUILayout.Label("有可能出现90%进度放技能，下一时刻就跳到100%的情况。");
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
                int maxIncome = 0;
                int maxPatience = ReadBook.instance.GetMaxPatience();
                Type type = ReadBook.instance.GetType();
                int actorValue = (int)type.InvokeMember("actorValue", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
                int readSkillId = int.Parse(DateFile.instance.GetItemDate(HomeSystem.instance.readBookId, 32, true));
                int needInt = HomeSystem.instance.GetNeedInt(actorValue, readSkillId);
                int canUseInt = DateFile.instance.BaseAttr(DateFile.instance.mianActorId, 4, 0) / 2;
                int cost3 = int.Parse(DateFile.instance.readBookDate[3][1]) * needInt / 100;
                int incomeBuff = int.Parse(DateFile.instance.readBookDate[3][6]);
                for (int i = 0; i <= 30; i++)
                {//i - 温的次数
                    int income1 = 0;
                    for (int k = 0; k < 10; k++)
                    {//无加成部分收益
                        income1 += 100 * (1 + (i < (27 - 3 * k) ? i : (27 - 3 * k)));
                    }
                    for (int j = 0; j <= i - 5 && j <= 3 * (30 - i); j++)
                    {//j - 需要补耐心的次数，起始耐心保底30可以温5次
                        int addPower3 = (j + 1) / 2;
                        int addPower4 = 0;
                        while (addPower3 >= 0)
                        {
                            if (i + addPower3 + addPower4 <= 30)
                            {//仅在格子合适时计算收益
                                int income2 = ((30 - i - addPower3 - addPower4) * i + addPower3 * (addPower3 + 3 * addPower4 + 2)) * incomeBuff;

                                int addInt = (32 - i) / 3 * 20 - ((32 - i) % 3 == 0 ? 20 : 10) + canUseInt;
                                if ((30 - i - addPower3 - addPower4) * cost3 - addInt > 0)
                                {
                                    income2 -= ((30 - i - addPower3 - addPower4) * cost3 - addInt + cost3 - 1) / cost3 * incomeBuff;
                                }

                                int patienceNeed = (1 << (i - j - 1)) + 1 - (30 - i - addPower3 - addPower4) * 2;
                                if (patienceNeed <= maxPatience && (maxIncome < income1 + income2 || (maxIncome == income1 + income2 && Main.plan.power3Num[0] > 30 - i - addPower3 - addPower4)))
                                {//耐心达标且收益更高时更换方案
                                    maxIncome = income1 + income2;
                                    Main.plan.power3Num[0] = 30 - i - addPower3 - addPower4;
                                    Main.plan.power6Num[0] = i;
                                    Main.plan.addPower3Num[0] = addPower3;
                                    Main.plan.addPower4Num[0] = addPower4;
                                }
                            }
                            addPower3--;
                            addPower4++;
                            if (addPower3 * 2 + addPower4 * 3 == j + 2)
                            {
                                addPower3--;
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ReadBook), "UpdateRead")]
    static class ReadBook_UpdateRead_Patch
    {
        static void Postfix()
        {
            if (!Main.enabled)
                return;

            Main.GetReadNum();

            Type type = ReadBook.instance.GetType();
            int readPageIndex = (int)type.InvokeMember("readPageIndex", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
            int readLevel = (int)type.InvokeMember("readLevel", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
            int canUseInt = (int)type.InvokeMember("canUseInt", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);

            int actorValue = (int)type.InvokeMember("actorValue", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
            int readSkillId = int.Parse(DateFile.instance.GetItemDate(HomeSystem.instance.readBookId, 32, true));
            int needInt = HomeSystem.instance.GetNeedInt(actorValue, readSkillId);

            if (Main.isRead)
            {
                int cost6 = int.Parse(DateFile.instance.readBookDate[6][1]) * needInt / 100;
                if (!Main.settings.autoRead || (Main.settings.just50Hard && needInt > 50) || DateFile.instance.BaseAttr(DateFile.instance.mianActorId, 4, 0) < cost6)
                    return;


                int patience = (int)type.InvokeMember("patience", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
                List<int[]> pageState = (List<int[]>)type.InvokeMember("pageState", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
                int cost3 = int.Parse(DateFile.instance.readBookDate[3][1]) * needInt / 100;
                int cost4 = int.Parse(DateFile.instance.readBookDate[4][1]) * needInt / 100;

                if (readLevel == 0 || readLevel == 50)
                {
                    if (Main.plan.power3Num[0] > Main.plan.power3Num[1])
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (Main.plan.power3Num[0] > Main.plan.power3Num[1] && readPageIndex * 3 + i < 30 - Main.plan.power6Num[0] - Main.plan.addPower4Num[0]
                                && pageState[readPageIndex][i] == 0 && canUseInt >= cost3)
                            {
                                ReadBook.instance.UseIntPower(3);
                                pageState[readPageIndex][i] = 3;
                                Main.plan.power3Num[1]++;
                                canUseInt = (int)type.InvokeMember("canUseInt", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
                            }
                        }
                    }
                    if (patience == 1)
                    {
                        if (Main.plan.addPower3Num[0] > Main.plan.addPower3Num[1] && pageState[readPageIndex][2] == 0 && canUseInt >= cost3)
                        {
                            ReadBook.instance.UseIntPower(3);
                            Main.plan.addPower3Num[1]++;
                            canUseInt = (int)type.InvokeMember("canUseInt", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
                        }
                        else if (Main.plan.addPower4Num[0] > Main.plan.addPower4Num[1] && pageState[readPageIndex][2] == 0 && canUseInt >= cost4)
                        {
                            ReadBook.instance.UseIntPower(4);
                            Main.plan.addPower4Num[1]++;
                            canUseInt = (int)type.InvokeMember("canUseInt", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
                        }
                    }
                    if (Main.plan.power6Num[0] > Main.plan.power6Num[1] && readPageIndex == (9 - (Main.plan.power6Num[1] / 3)))
                    {
                        ReadBook.instance.UseIntPower(6);
                        Main.plan.power6Num[1]++;
                        canUseInt = (int)type.InvokeMember("canUseInt", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
                    }
                }
            }
            else
            {
                int cost4 = int.Parse(DateFile.instance.readBookDate[4][1]) * needInt / 100;
                int cost7 = int.Parse(DateFile.instance.readBookDate[7][1]) * needInt / 100;
                int cost8 = int.Parse(DateFile.instance.readBookDate[8][1]) * needInt / 100;
                int readPageTime = (int)type.InvokeMember("readPageTime", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
                if (Main.settings.autoUsePower5 && readPageTime > 24)
                {
                    ReadBook.instance.UseIntPower(5);
                    canUseInt = (int)type.InvokeMember("canUseInt", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
                }
                int bookId = int.Parse(DateFile.instance.GetItemDate(HomeSystem.instance.readBookId, 32, true));
                int[] bookPages = (HomeSystem.instance.studySkillTyp != 17)
                    ? ((!DateFile.instance.skillBookPages.ContainsKey(bookId)) ? new int[10] : DateFile.instance.skillBookPages[bookId])
                    : ((!DateFile.instance.gongFaBookPages.ContainsKey(bookId)) ? new int[10] : DateFile.instance.gongFaBookPages[bookId]);
                int[] bookPage = DateFile.instance.GetBookPage(HomeSystem.instance.readBookId);
                if (Main.settings.autoUsePower4 && bookPage[readPageIndex] == 0 && bookPages[readPageIndex] == 0)
                {
                    if (canUseInt >= (cost4 + cost7 * 2) * needInt / 100 && Main.readNum >= 5)
                    {
                        ReadBook.instance.UseIntPower(4);
                        ReadBook.instance.UseIntPower(7);
                        ReadBook.instance.UseIntPower(7);
                        canUseInt = (int)type.InvokeMember("canUseInt", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
                    }
                    else if (canUseInt >= (cost4 + cost8 * 2) * needInt / 100 && Main.readNum < 5)
                    {
                        ReadBook.instance.UseIntPower(4);
                        ReadBook.instance.UseIntPower(8);
                        ReadBook.instance.UseIntPower(8);
                        canUseInt = (int)type.InvokeMember("canUseInt", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
                    }
                }
            }

        }
    }
}