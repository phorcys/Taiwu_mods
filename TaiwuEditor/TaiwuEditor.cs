using System;
using System.Collections.Generic;
using System.Reflection;
using System.Timers;
using Harmony12;
using UnityEngine;
using UnityModManagerNet;

namespace TaiwuEditor
{
    public static class Main
    {
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create(modEntry.Info.Id);
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            Main.settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            Main.logger = modEntry.Logger;
            modEntry.OnGUI = new Action<UnityModManager.ModEntry>(Main.OnGUI);
            modEntry.OnSaveGUI = new Action<UnityModManager.ModEntry>(Main.OnSaveGUI);
            Main.ReadBook_ReadLevel = typeof(ReadBook).GetField("readLevel", BindingFlags.Instance | BindingFlags.NonPublic);
            bool flag = Main.ReadBook_ReadLevel == null;
            if (flag)
            {
                Main.logger.Log("获取ReadBook.readLevel失败");
            }
            Main.MassageWindow_DoEvent = typeof(MassageWindow).GetMethod("DoEvent", BindingFlags.Instance | BindingFlags.NonPublic);
            bool flag2 = Main.MassageWindow_DoEvent == null;
            if (flag2)
            {
                Main.logger.Log("获取MassageWindow.DoEvent失败");
            }
            Main.timer = new Timer();
            Main.timer.Interval = 1000.0;
            Main.timer.Elapsed += Main.Timer_Elapsed;
            Main.timer.Start();
            return true;
        }

        // 快速读书 Highly Inspired by ReadBook.GetReadBooty()
        private static void EasyReadV2()   
        {
            List<int[]> list = new List<int[]>();
            for (int i = 0; i < 10; i++)   //There are 10 pages
            {
                list.Add(new int[6]);
            }
            for (int j = 0; j < 10; j++)
            {
                int num = DateFile.instance.MianActorID();  
                int num2 = 100;
                for (int k = 0; k < 3; k++)
                {
                    int key = list[j][k];
                    num2 += DateFile.instance.ParseInt(DateFile.instance.readBookDate[key][6]);
                }
                int num3 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(HomeSystem.instance.readBookId, 32, true)); 
                int num4 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(HomeSystem.instance.readBookId, 34, true)) * num2 / 100; 
                int[] bookPage = DateFile.instance.GetBookPage(HomeSystem.instance.readBookId);
                bool flag = HomeSystem.instance.studySkillTyp == 17;
                if (flag)
                {
                    bool flag2 = !DateFile.instance.gongFaBookPages.ContainsKey(num3);
                    if (flag2)
                    {
                        DateFile.instance.gongFaBookPages.Add(num3, new int[10]);
                    }
                    int num5 = DateFile.instance.gongFaBookPages[num3][j];
                    bool flag3 = num5 != 1 && num5 > -100;
                    if (flag3)
                    {
                        int num6 = DateFile.instance.ParseInt(DateFile.instance.gongFaDate[num3][2]);
                        int num7 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(HomeSystem.instance.readBookId, 35, true)); 
                        DateFile.instance.ChangeActorGongFa(num, num3, 0, 0, num7, true); 
                        bool flag4 = num7 != 0; 
                        if (flag4)
                        {
                            ActorMenu.instance.ChangeMianQi(num2, 50 * num6, 5); 
                        }
                        list[j][5] = 1;
                        DateFile.instance.gongFaBookPages[num3][j] = 1;
                        DateFile.instance.AddActorScore(303, num6 * 100);
                        bool flag5 = DateFile.instance.GetGongFaLevel(num, num3, 0) >= 100 && DateFile.instance.GetGongFaFLevel(num, num3) >= 10;
                        if (flag5)
                        {
                            DateFile.instance.AddActorScore(304, num6 * 100);
                        }
                        bool flag6 = bookPage[j] == 0;
                        if (flag6)
                        {
                            DateFile.instance.AddActorScore(305, num6 * 100);
                        }
                    }
                    else
                    {
                        num4 = num4 * 10 / 100;
                    }
                    DateFile.instance.gongFaExperienceP += num4;
                }
                else
                {
                    bool flag7 = !DateFile.instance.skillBookPages.ContainsKey(num3);
                    if (flag7)
                    {
                        DateFile.instance.skillBookPages.Add(num3, new int[10]);
                    }
                    int num8 = DateFile.instance.skillBookPages[num3][j];
                    bool flag8 = num8 !=1 && num8 > -100;
                    if (flag8)
                    {
                        int num9 = DateFile.instance.ParseInt(DateFile.instance.skillDate[num3][2]);
                        bool flag9 = !DateFile.instance.actorSkills.ContainsKey(num3);
                        if (flag9)
                        {
                            DateFile.instance.ChangeMianSkill(num3, 0, 0, true);
                        }
                        list[j][5] = 1;
                        DateFile.instance.skillBookPages[num3][j] = 1;
                        DateFile.instance.AddActorScore(203, num9 * 100);
                        bool flag10 = DateFile.instance.GetSkillLevel(num3) >= 100 && DateFile.instance.GetSkillFLevel(num3) >= 10;
                        if (flag10)
                        {
                            DateFile.instance.AddActorScore(204, num9 * 100);
                        }
                        bool flag11 = bookPage[j] == 0;
                        if (flag11)
                        {
                            DateFile.instance.AddActorScore(205, num9 * 100);
                        }
                    }
                    else
                    {
                        num4 = num4 * 10 / 100;
                    }
                    DateFile.instance.gongFaExperienceP += num4;
                }
            }
        }

        // 锁定30天时间
        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool flag = DateFile.instance != null && Main.settings.lockTime;
            if (flag)
            {
                DateFile.instance.dayTime = 30;
            }
        }

        // 属性修改
        private static void FieldHelper(int resId, string fieldname)
        {
            DateFile instance = DateFile.instance;
            bool flag = instance == null || instance.actorsDate == null || !instance.actorsDate.ContainsKey(instance.mianActorId);
            GUILayout.BeginHorizontal("Box", new GUILayoutOption[0]);
            GUILayout.Label(fieldname, new GUILayoutOption[0]);
            if (flag)
            {
                GUILayout.TextField(Main.errorString, new GUILayoutOption[0]);
            }
            else
            {
                //Dictionary<int, string> dictionary = instance.actorsDate[instance.mianActorId];
                instance.actorsDate[instance.mianActorId][resId] = GUILayout.TextField(instance.GetActorDate(instance.mianActorId, resId, false), new GUILayoutOption[0]);
            }
            GUILayout.EndHorizontal();
        }

        // 属性修改
        private static void FieldHelper(ref int field, string fieldname)
        {
            DateFile instance = DateFile.instance;
            bool flag = instance == null || instance.actorsDate == null || !instance.actorsDate.ContainsKey(instance.mianActorId);
            GUILayout.BeginHorizontal("Box", new GUILayoutOption[0]);
            GUILayout.Label(fieldname, new GUILayoutOption[0]);
            if (flag)
            {
                GUILayout.TextField(Main.errorString, new GUILayoutOption[0]);
            }
            else
            {
                //Dictionary<int, string> dictionary = instance.actorsDate[instance.mianActorId];
                field = DateFile.instance.ParseInt(GUILayout.TextField(field.ToString(), new GUILayoutOption[0]));
            }
            GUILayout.EndHorizontal();
        }
        
        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Main.settings.Save(modEntry);
        }

        // 主界面
        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal("Box", new GUILayoutOption[0]);
            GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
            Main.FieldHelper(61, "膂力");
            Main.FieldHelper(62, "体质");
            Main.FieldHelper(63, "灵敏");
            Main.FieldHelper(64, "根骨");
            Main.FieldHelper(65, "悟性");
            Main.FieldHelper(66, "定力");
            GUILayout.Space(10f);
            Main.FieldHelper(401, "食材");
            Main.FieldHelper(402, "木材");
            Main.FieldHelper(403, "金石");
            Main.FieldHelper(404, "织物");
            Main.FieldHelper(405, "草药");
            Main.FieldHelper(406, "金钱");
            Main.FieldHelper(407, "威望");
            GUILayout.Space(10f);
            Main.FieldHelper(ref DateFile.instance.gongFaExperienceP, "历练");
            Main.settings.lockTime = GUILayout.Toggle(Main.settings.lockTime, "锁定一月行动不减", new GUILayoutOption[0]);
            Main.settings.lockFastRead = GUILayout.Toggle(Main.settings.lockFastRead, "快速读书（对残缺篇章有效）", new GUILayoutOption[0]);
            Main.settings.lockMaxOutProficiency = GUILayout.Toggle(Main.settings.lockMaxOutProficiency, "修习单击全满", new GUILayoutOption[0]);
            Main.settings.lockFastQiyuCompletion = GUILayout.Toggle(Main.settings.lockFastQiyuCompletion, "奇遇直接到达目的地", new GUILayoutOption[0]);
            Main.settings.lockNeverOverweigh = GUILayout.Toggle(Main.settings.lockNeverOverweigh, "身上物品永不超重（仓库无效）", new GUILayoutOption[0]);
            Main.settings.lockMaxOutRelation = GUILayout.Toggle(Main.settings.lockMaxOutRelation, "见面关系全满", new GUILayoutOption[0]);
            Main.settings.lockMaxLifeFace = GUILayout.Toggle(Main.settings.lockMaxLifeFace, "见面印象最深", new GUILayoutOption[0]);
            Main.settings.lockZeroWariness = GUILayout.Toggle(Main.settings.lockZeroWariness, "锁定戒心为零", new GUILayoutOption[0]);
            GUILayout.EndVertical();
            GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
            Main.FieldHelper(501, "音律");
            Main.FieldHelper(502, "弈棋");
            Main.FieldHelper(503, "诗书");
            Main.FieldHelper(504, "绘画");
            Main.FieldHelper(505, "术数");
            Main.FieldHelper(506, "品鉴");
            Main.FieldHelper(507, "锻造");
            Main.FieldHelper(508, "制木");
            Main.FieldHelper(509, "医术");
            Main.FieldHelper(510, "毒术");
            Main.FieldHelper(511, "织锦");
            Main.FieldHelper(512, "巧匠");
            Main.FieldHelper(513, "道法");
            Main.FieldHelper(514, "佛学");
            Main.FieldHelper(515, "厨艺");
            Main.FieldHelper(516, "杂学");
            GUILayout.EndVertical();
            GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
            Main.FieldHelper(601, "内功");
            Main.FieldHelper(602, "身法");
            Main.FieldHelper(603, "绝技");
            Main.FieldHelper(604, "拳掌");
            Main.FieldHelper(605, "指法");
            Main.FieldHelper(606, "腿法");
            Main.FieldHelper(607, "暗器");
            Main.FieldHelper(608, "剑法");
            Main.FieldHelper(609, "刀法");
            Main.FieldHelper(610, "长兵");
            Main.FieldHelper(611, "奇门");
            Main.FieldHelper(612, "软兵");
            Main.FieldHelper(613, "御射");
            Main.FieldHelper(614, "乐器");
            Main.FieldHelper(706, "内力修为");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private static UnityModManager.ModEntry.ModLogger logger;
        
        private static string errorString = "存档未载入！";
        
        private static Timer timer;
        
        private static FieldInfo ReadBook_ReadLevel;
        
        private static MethodInfo MassageWindow_DoEvent;
        
        private static Settings settings;

        // 最大好感和最大印象
        [HarmonyPatch(typeof(MassageWindow), "SetMassageWindow")]
        private static class SAW_Hook
        {
            private static bool Prefix(ref int[] baseEventDate)
            {
                bool flag = Main.settings.lockMaxOutRelation;
                if (flag)
                {
                    int key = baseEventDate[2];
                    int num = DateFile.instance.ParseInt(DateFile.instance.eventDate[key][2]);
                    int num2 = DateFile.instance.MianActorID();
                    int num3 = (num != 0) ? ((num != -1) ? num : num2) : baseEventDate[1];
                    bool flag2 = num3 != num2;
                    if (flag2)
                    {
                        bool flag3 = DateFile.instance.actorsDate.ContainsKey(num3);
                        if (flag3)
                        {
                            try   
                            {
                                DateFile.instance.ChangeFavor(num3, 60000, true, false); 
                            }
                            catch (Exception e)
                            {
                                Main.logger.Log("TaiwuEditor");
                                Main.logger.Log("好感修改失败");
                                Main.logger.Log(e.Message);
                                Main.logger.Log(e.StackTrace);
                            }
                            bool flag4 = Main.settings.lockMaxLifeFace;
                            if (flag4)
                            {
                                try
                                {
                                    DateFile.instance.ChangeActorLifeFace(num3, 0, 100);
                                }
                                catch (Exception e)
                                {
                                    Main.logger.Log("TaiwuEditor");
                                    Main.logger.Log("印象修改失败");
                                    Main.logger.Log(e.Message);
                                    Main.logger.Log(e.StackTrace);
                                }
                            }
                        }
                    }
                }
                return true;
            }
        }
        
        // 锁定戒心为0
        [HarmonyPatch(typeof(DateFile), "GetActorWariness")]
        private static class ZW_Hook
        {
            private static void Postfix(ref int __result)
            {
                bool flag = Main.settings.lockZeroWariness;
                if (flag)
                {
                    __result = 0;
                }
            }
        }

        // 快速修习
        [HarmonyPatch(typeof(HomeSystem), "StudySkillUp")]
        private static class SSU_Hook
        {
            private static bool Prefix(ref int ___studySkillId, ref int ___studySkillTyp, ref HomeSystem __instance)
            {
                bool flag = !Main.settings.lockMaxOutProficiency;
                bool result;
                if (flag)
                {
                    result = true;
                }
                else
                {
                    int num = DateFile.instance.MianActorID();
                    bool flag2 = ___studySkillId > 0;
                    if (flag2)
                    {
                        bool flag3 = ___studySkillTyp == 17;
                        if (flag3)
                        {
                            int num2 = DateFile.instance.ParseInt(DateFile.instance.gongFaDate[___studySkillId][2]);
                            DateFile.instance.addGongFaStudyValue = 0;
                            //DateFile.instance.actorGongFas[num][___studySkillId][0] = 100;
                            DateFile.instance.ChangeActorGongFa(num, ___studySkillId, 100, 0, 0, false);
                            DateFile.instance.AddActorScore(302, num2 * 100);
                            bool flag4 = DateFile.instance.GetGongFaLevel(num, ___studySkillId, 0) >= 100 && DateFile.instance.GetGongFaFLevel(num, ___studySkillId) >= 10;
                            if (flag4)
                            {
                                DateFile.instance.AddActorScore(304, DateFile.instance.ParseInt(DateFile.instance.gongFaDate[___studySkillId][2]) * 100);
                            }
                        }
                        else
                        {
                            int num3 = DateFile.instance.ParseInt(DateFile.instance.skillDate[___studySkillId][2]);
                            DateFile.instance.addSkillStudyValue = 0;
                            DateFile.instance.ChangeMianSkill(___studySkillId, 100, 0, false);
                            //DateFile.instance.actorSkills[___studySkillId][0] = 100;
                            DateFile.instance.AddActorScore(202, num3 * 100);
                            bool flag5 = DateFile.instance.GetSkillLevel(___studySkillId) >= 100 && DateFile.instance.GetSkillFLevel(___studySkillId) >= 10;
                            if (flag5)
                            {
                                DateFile.instance.AddActorScore(204, num3 * 100);
                            }
                        }
                        __instance.UpdateStudySkillWindow();
                        __instance.UpdateLevelUPSkillWindow();
                        __instance.UpdateReadBookWindow();
                    }
                    result = false;
                }
                return result;
            }
        }

        // 奇遇直接到终点
        [HarmonyPatch(typeof(StorySystem), "OpenStory")]
        private static class OS_Hook
        {
            private static bool Prefix(ref StorySystem __instance)
            {
                bool flag = !Main.settings.lockFastQiyuCompletion;
                bool result;
                if (flag)
                {
                    result = true;
                }
                else
                {
                    bool flag2 = __instance.storySystemStoryId == 10002 || __instance.storySystemStoryId == 10003 || __instance.storySystemStoryId == 10004;
                    if (flag2)
                    {
                        result = true;
                    }
                    else
                    {
                        __instance.ClossToStoryMenu();
                        int num = DateFile.instance.ParseInt(DateFile.instance.baseStoryDate[__instance.storySystemStoryId][302]);
                        bool flag3 = num != 0;
                        if (flag3)
                        {
                            DateFile.instance.SetEvent(new int[]
                            {
                                0,
                                -1,
                                num
                            }, true, true);
                            Main.logger.Log("MassageWindow.DoEvent called");
                            Main.MassageWindow_DoEvent.Invoke(MassageWindow.instance, new object[]
                            {
                                0
                            });
                        }
                        else
                        {
                            DateFile.instance.SetStory(true, __instance.storySystemPartId, __instance.storySystemPlaceId, __instance.storySystemStoryId, 0, 0);
                            __instance.StoryEnd();
                        }
                        result = false;
                    }
                }
                return result;
            }
        }

        // 背包最大载重
        [HarmonyPatch(typeof(ActorMenu), "GetMaxItemSize")]
        private static class GMIS_Hook
        {
            private static void Postfix(ref int key, ref int __result)
            {
                bool flag = !Main.settings.lockNeverOverweigh || DateFile.instance.mianActorId != key;
                if (!flag)
                {
                    __result = 999999999;
                }
            }
        }

        // 快速读书
        [HarmonyPatch(typeof(HomeSystem), "StartReadBook")]
        private static class SRB_Hook
        {
            private static bool Prefix()
            {
                bool flag = !Main.settings.lockFastRead;
                bool result;
                if (flag)
                {
                    result = true;
                }
                else
                {
                    Main.EasyReadV2();
                    HomeSystem.instance.UpdateReadBookWindow();
                    result = false;
                }
                return result;
            }
        }
    }
    
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }

        // 锁定时间
        public bool lockTime = false;

        // 快速读书
        public bool lockFastRead = false;

        // 快速修习
        public bool lockMaxOutProficiency = false;

        // 奇遇直接到终点
        public bool lockFastQiyuCompletion = false;

        // 背包无限
        public bool lockNeverOverweigh = false;

        // 见面最大化好感
        public bool lockMaxOutRelation = false;

        // 见面最大化印象
        public bool lockMaxLifeFace = false;

        // 锁定戒心为0
        public bool lockZeroWariness = false;
    }
}
