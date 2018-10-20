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
        public bool autoUsePower4 =false;
        public bool autoUsePower5 = false;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

    }

    static class Main
    {
        public static bool enabled;
        public static bool isRead = false;
        public static int readNum = 0;
        public static Settings settings;
        public static int[,] powerNum = new int[,] { { 15, 9, 16 }, { 43, 11, 15 }, { 109, 10, 16 }, { 233, 12, 15 }, { 485, 14, 14 }, { 491, 11, 16 }, { 999, 13, 15 }, { 2019, 15, 14 }, { 2025, 12, 16 }, { 4069, 14, 15 }, { 8161, 16, 14 }, { 8167, 13, 16 }, { 16355, 15, 15 }, { 32741, 14, 16 }, { 65511, 13, 17 }, { 131049, 12, 18 } };
        public static int[] power36Num = { 0, 0, 0, 0 };//3总数|3使用|6总数|6使用

        public static List<int[]> pageState=new List<int[]>();

        public static void GetReadNum()
        {
            int bookId = int.Parse(DateFile.instance.GetItemDate(HomeSystem.instance.readBookId, 32, true));
            int[] bookPages=(HomeSystem.instance.studySkillTyp != 17) 
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
            //计算已读页数}
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
            GUILayout.Label("说明： ");
            GUILayout.Label("根据玩家初始耐心选取最优刷历练方案并自动实施，刷历练期间请不要手动释放技能。");
            GUILayout.Label("仅对已读完且阅读难度为50%的书籍有效，需求30悟性。");

            GUILayout.Label("");

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
                Main.isRead=true;
            }

            Main.power36Num[0] = 0;
            Main.power36Num[2] = 0;
            if (Main.isRead)
            {
                int maxPatience = ReadBook.instance.GetMaxPatience();
                int canUseInt = DateFile.instance.BaseAttr(DateFile.instance.mianActorId, 4, 0) / 2;
                int num3 = 0, num6 = 0;
                int maxIncome = 0;
                for (int i = 0; i < 31; i++)
                {
                    for (int j = 0; j <= 30 - i && j <= (i + 1) / 2; j++)
                    {
                        //i - 温的次数，j - 补充耐心的独的个数
                        int patienceNeed = (((i - 2 * j - 1) < 0) ? 0 : (1 << (i - 2 * j - 1))) + 1 - 2 * (30 - i - j);
                        //需求耐心下限
                        num3 = 30 - i - j;
                        num6 = i;
                        int intNeed = 2 * (10 * (30 - i - j) - 20 * ((30 - i - j + 2) / 3) + 10);
                        //需求初始悟性下限=悟性/2
                        int income = 0;
                        for (int k = 0; k < 10; k++)
                        {
                            income += 2 * (1 + (i < (27 - 3 * k) ? i : (27 - 3 * k)));
                        }
                        //无加成部分收益
                        income += (30 - i - j) * (i + 1);
                        //开场加成部分
                        for (int k = 0; k < j; k++)
                        {
                            income += 2 * k + 3;
                        }
                        //补充加成部分
                        if ((intNeed - canUseInt + 9) / 10 > 0)
                        {
                            income -= (intNeed - canUseInt + 9) / 10;
                        }//悟性不足修正
                        if (maxPatience >= patienceNeed && income > maxIncome)
                        {
                            maxIncome = income;
                            Main.power36Num[0] = num3;
                            Main.power36Num[2] = num6;
                        }
                    }
                }
            }
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
                if (!Main.settings.autoRead || needInt != 50 || DateFile.instance.BaseAttr(DateFile.instance.mianActorId, 4, 0) < 30)
                    return;


                int patience = (int)type.InvokeMember("patience", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
                List<int[]> pageState = (List<int[]>)type.InvokeMember("pageState", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
                int cost3 = int.Parse(DateFile.instance.readBookDate[3][1]) * needInt / 100;
                int cost6 = int.Parse(DateFile.instance.readBookDate[6][1]) * needInt / 100;

                if (readLevel == 50)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (Main.power36Num[0] > Main.power36Num[1] && readPageIndex * 3 + i + 1 < 30 - Main.power36Num[2] && pageState[readPageIndex][i] == 0 && canUseInt >= 10)
                        {
                            ReadBook.instance.UseIntPower(3);
                            Main.power36Num[1]++;
                            canUseInt = (int)type.InvokeMember("canUseInt", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
                        }
                    }
                    if (patience == 1)
                    {
                        ReadBook.instance.UseIntPower(3);
                        canUseInt = (int)type.InvokeMember("canUseInt", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
                    }
                    if (Main.power36Num[2] > Main.power36Num[3] && readPageIndex == (9 - (Main.power36Num[3] / 3)))
                    {
                        ReadBook.instance.UseIntPower(6);
                        Main.power36Num[3]++;
                        canUseInt = (int)type.InvokeMember("canUseInt", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
                    }
                }
            }
            else
            {
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
                    if (canUseInt > 140 * needInt / 100)
                    {
                        if (Main.readNum >= 5)
                        {
                            ReadBook.instance.UseIntPower(4);
                            ReadBook.instance.UseIntPower(7);
                            ReadBook.instance.UseIntPower(7);
                            canUseInt = (int)type.InvokeMember("canUseInt", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, ReadBook.instance, null);
                        }
                        else
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
}