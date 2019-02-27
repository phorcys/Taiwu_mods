using Harmony12;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace GuiMartialArts
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }


    }
    public static class Main
    {
        public static bool OnChangeList;
        public static bool showNpcInfo;

        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static MartialArtsData artsData = new MartialArtsData();

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            #region 基础设置
            settings = Settings.Load<Settings>(modEntry);
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            HarmonyInstance harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            #endregion

            return true;
        }

        static string title = "鬼的战斗艺术";
        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        static Test test;
        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(title, GUILayout.Width(300));

            if (GUILayout.Button("xxx")||UnityEngine.Time.time > 10)
            {
                if (test == null)
                {
                    Main.Logger.Log("添加Test");
                    test = YesOrNoWindow.instance.gameObject.AddComponent<Test>();
                }
            }
        }


        [HarmonyPatch(typeof(BattleSystem), "UseGongFaStart")]
        public static class BattleSystem_UseGongFaStart_Patch
        {
            public static void Postfix(bool isActor, int gongFaId)
            {
                if (!Main.enabled)
                    return;

                Logger.Log("使用功法开始 isActor=" + isActor + " gongFaId=" + gongFaId);

                artsData.AddUseGongfa(isActor, gongFaId);

                return;
            }
        }

        [HarmonyPatch(typeof(BattleSystem), "UseGongFa")]
        public static class BattleSystem_UseGongFa_Patch1
        {
            public static void Postfix(bool isActor, int gongFaId)
            {
                if (!Main.enabled)
                    return;

                Logger.Log("使用功法 isActor=" + isActor + " gongFaId=" + gongFaId);

                artsData.AddUseGongfa(isActor, gongFaId);

                return;
            }
        }

        [HarmonyPatch(typeof(BattleSystem), "BattleEnd")]
        public static class BattleSystem_BattleEnd_Patch1
        {
            public static void Postfix(bool actorWin, int actorRun = 0)
            {
                if (!Main.enabled)
                    return;

                Logger.Log("战斗结束 actorWin=" + actorWin + " actorRun=" + actorRun);

                artsData.SaveWindows();

                return;
            }
        }

        [HarmonyPatch(typeof(BattleSystem), "ChangeEnemy")]
        public static class BattleSystem_ChangeEnemy_Patch1
        {
            public static void Postfix(int index)
            {
                if (!Main.enabled)
                    return;

                int _newEnemyId = DateFile.instance.enemyBattlerIdDate[index];
                int _oldEnemyId = DateFile.instance.enemyBattlerIdDate[0];
                Logger.Log("更换敌人 index=" + index + " _newEnemyId=" + _newEnemyId + " _oldEnemyId=" + _oldEnemyId);
                artsData.AddBattleEnemy(_newEnemyId, _oldEnemyId);

                return;
            }
        }


        //[HarmonyPatch(typeof(ReadBook), "GetReadBooty")]
        //public static class ReadBook_GetReadBooty_Patch1
        //{
        //    public static bool Prefix()
        //    {
        //        if (!Main.enabled)
        //            return true;


        //        ReadBook __this = ReadBook.instance;
        //        var _readPageIndex = typeof(ReadBook).GetField("readPageIndex", BindingFlags.NonPublic | BindingFlags.Instance);
        //        var __readPageIndex = (int)_readPageIndex.GetValue(ReadBook.instance);


        //        var _pageState = typeof(ReadBook).GetField("pageState", BindingFlags.NonPublic | BindingFlags.Instance);
        //        var __pageState = (List<int[]>)_pageState.GetValue(ReadBook.instance);


        //        int num = DateFile.instance.MianActorID();
        //        Logger.Log("num " + num);
        //        int num2 = 100;
        //        for (int i = 0; i < 3; i++)
        //        {
        //            int key = __pageState[__readPageIndex][i];
        //            num2 += int.Parse(DateFile.instance.readBookDate[key][6]); // 6是奖励 // 6是奖励
        //        }

        //        int num3 = int.Parse(DateFile.instance.GetItemDate(HomeSystem.instance.readBookId, 32, true)); // 功法id
        //        Logger.Log("功法idnum3 " + num3);
        //        int num4 = int.Parse(DateFile.instance.GetItemDate(HomeSystem.instance.readBookId, 34, true)) * num2 / 100; // 分数
        //        Logger.Log("分数num4 " + num4);
        //        int[] bookPage = DateFile.instance.GetBookPage(HomeSystem.instance.readBookId);
        //        bool flag = HomeSystem.instance.studySkillTyp == 17;
        //        Logger.Log("flag " + flag);
        //        if (flag) // 读的是功法书
        //        {
        //            Logger.Log("读的是功法书");
        //            bool flag2 = !DateFile.instance.gongFaBookPages.ContainsKey(num3);
        //            Logger.Log("flag2 " + flag2);
        //            if (flag2)
        //            {
        //                DateFile.instance.gongFaBookPages.Add(num3, new int[10]);
        //            }
        //            int num5 = DateFile.instance.gongFaBookPages[num3][__readPageIndex];
        //            Logger.Log("num5 " + num5);
        //            bool flag3 = num5 != 1 && num5 > -100;
        //            Logger.Log("flag3 " + flag3);
        //            if (flag3)
        //            {
        //                int num6 = int.Parse(DateFile.instance.gongFaDate[num3][2]);
        //                Logger.Log("num6 " + num6);
        //                int num7 = int.Parse(DateFile.instance.GetItemDate(HomeSystem.instance.readBookId, 35, true));
        //                Logger.Log("num7 " + num7);
        //                DateFile.instance.ChangeActorGongFa(num, num3, 0, 0, num7, true);
        //                bool flag4 = !DateFile.instance.actorGongFas[num].ContainsKey(num3);
        //                Logger.Log("flag4 " + flag4);
        //                if (flag4)
        //                {
        //                    __this.getSkillWindow.SetActive(true);
        //                    __this.getSkillNameText.text = DateFile.instance.massageDate[7010][3] + WindowManage.instance.Mut() + DateFile.instance.gongFaDate[num3][0];
        //                    //TweenSettingsExtensions.SetEase<Tweener>(TweenSettingsExtensions.SetUpdate<Tweener>(ShortcutExtensions.DOScale(__this.getSkillNameText.GetComponent<RectTransform>(), new Vector3(1.8f, 1.8f, 1f), 0.1f), true), 27);
        //                    //TweenSettingsExtensions.SetEase<Tweener>(TweenSettingsExtensions.SetDelay<Tweener>(TweenSettingsExtensions.SetUpdate<Tweener>(ShortcutExtensions.DOScale(__this.getSkillNameText.GetComponent<RectTransform>(), new Vector3(1f, 1f, 1f), 0.4f), true), 0.1f), 27);
        //                }
        //                bool flag5 = num7 != 0;
        //                Logger.Log("flag5 " + flag5);
        //                if (flag5)
        //                {
        //                    ActorMenu.instance.ChangeMianQi(num, 50 * num6, 5);
        //                    TipsWindow.instance.SetTips(0, new string[]
        //                    {
        //                DateFile.instance.readBookDate[105][98]
        //                    }, 180, -730f, -310f, 450, 100);
        //                }
        //                else
        //                {
        //                    TipsWindow.instance.SetTips(0, new string[]
        //                    {
        //                DateFile.instance.readBookDate[102][98]
        //                    }, 180, -730f, -310f, 450, 100);
        //                }
        //                __pageState[__readPageIndex][5] = 1;
        //                DateFile.instance.gongFaBookPages[num3][__readPageIndex] = 1;
        //                //TweenSettingsExtensions.SetEase<Tweener>(TweenSettingsExtensions.SetUpdate<Tweener>(ShortcutExtensions.DOScale(__this.getFlevelText.GetComponent<RectTransform>(), new Vector3(1.8f, 1.8f, 1f), 0.1f), true), 27);
        //                //TweenSettingsExtensions.SetEase<Tweener>(TweenSettingsExtensions.SetDelay<Tweener>(TweenSettingsExtensions.SetUpdate<Tweener>(ShortcutExtensions.DOScale(__this.getFlevelText.GetComponent<RectTransform>(), new Vector3(1f, 1f, 1f), 0.4f), true), 0.1f), 27);
        //                DateFile.instance.AddActorScore(303, num6 * 100);
        //                bool flag6 = DateFile.instance.GetGongFaLevel(num, num3, 0) >= 100 && DateFile.instance.GetGongFaFLevel(num, num3, false) >= 10;
        //                Logger.Log("flag6 " + flag6);
        //                if (flag6)
        //                {
        //                    DateFile.instance.AddActorScore(304, num6 * 100);
        //                }
        //                bool flag7 = bookPage[__readPageIndex] == 0;
        //                Logger.Log("flag7 " + flag7);
        //                if (flag7)
        //                {
        //                    DateFile.instance.AddActorScore(305, num6 * 100);
        //                }
        //            }
        //            else
        //            {
        //                num4 = num4 * 10 / 100;
        //                TipsWindow.instance.SetTips(0, new string[]
        //                {
        //            DateFile.instance.readBookDate[102][98]
        //                }, 180, -730f, -310f, 450, 100);
        //            }
        //            DateFile.instance.gongFaExperienceP += num4;
        //        }
        //        else // 读的是技艺书
        //        {
        //            bool flag8 = !DateFile.instance.skillBookPages.ContainsKey(num3);
        //            if (flag8)
        //            {
        //                DateFile.instance.skillBookPages.Add(num3, new int[10]);
        //            }
        //            int num8 = DateFile.instance.skillBookPages[num3][__readPageIndex];
        //            bool flag9 = num8 != 1 && num8 > -100;
        //            if (flag9)
        //            {
        //                int num9 = int.Parse(DateFile.instance.skillDate[num3][2]);
        //                bool flag10 = !DateFile.instance.actorSkills.ContainsKey(num3);
        //                if (flag10)
        //                {
        //                    DateFile.instance.ChangeMianSkill(num3, 0, 0, true);
        //                    __this.getSkillWindow.SetActive(true);
        //                    __this.getSkillNameText.text = DateFile.instance.massageDate[7010][3] + WindowManage.instance.Mut() + DateFile.instance.skillDate[num3][0];
        //                    //TweenSettingsExtensions.SetEase<Tweener>(TweenSettingsExtensions.SetUpdate<Tweener>(ShortcutExtensions.DOScale(__this.getSkillNameText.GetComponent<RectTransform>(), new Vector3(1.8f, 1.8f, 1f), 0.1f), true), 27);
        //                    //TweenSettingsExtensions.SetEase<Tweener>(TweenSettingsExtensions.SetDelay<Tweener>(TweenSettingsExtensions.SetUpdate<Tweener>(ShortcutExtensions.DOScale(__this.getSkillNameText.GetComponent<RectTransform>(), new Vector3(1f, 1f, 1f), 0.4f), true), 0.1f), 27);
        //                }
        //                __pageState[__readPageIndex][5] = 1;
        //                DateFile.instance.skillBookPages[num3][__readPageIndex] = 1;
        //                //TweenSettingsExtensions.SetEase<Tweener>(TweenSettingsExtensions.SetUpdate<Tweener>(ShortcutExtensions.DOScale(__this.getFlevelText.GetComponent<RectTransform>(), new Vector3(1.8f, 1.8f, 1f), 0.1f), true), 27);
        //                //TweenSettingsExtensions.SetEase<Tweener>(TweenSettingsExtensions.SetDelay<Tweener>(TweenSettingsExtensions.SetUpdate<Tweener>(ShortcutExtensions.DOScale(__this.getFlevelText.GetComponent<RectTransform>(), new Vector3(1f, 1f, 1f), 0.4f), true), 0.1f), 27);
        //                DateFile.instance.AddActorScore(203, num9 * 100);
        //                bool flag11 = DateFile.instance.GetSkillLevel(num3) >= 100 && DateFile.instance.GetSkillFLevel(num3) >= 10;
        //                if (flag11)
        //                {
        //                    DateFile.instance.AddActorScore(204, num9 * 100);
        //                }
        //                bool flag12 = bookPage[__readPageIndex] == 0;
        //                if (flag12)
        //                {
        //                    DateFile.instance.AddActorScore(205, num9 * 100);
        //                }
        //            }
        //            else
        //            {
        //                num4 = num4 * 10 / 100;
        //            }
        //            TipsWindow.instance.SetTips(0, new string[]
        //            {
        //        DateFile.instance.readBookDate[102][98]
        //            }, 180, -730f, -310f, 450, 100);
        //            DateFile.instance.gongFaExperienceP += num4;
        //        }
        //        __pageState[__readPageIndex][4] += num4;
        //        //TweenSettingsExtensions.SetEase<Tweener>(TweenSettingsExtensions.SetUpdate<Tweener>(ShortcutExtensions.DOScale(__this.getExpText.GetComponent<RectTransform>(), new Vector3(1.8f, 1.8f, 1f), 0.1f), true), 27);
        //        //TweenSettingsExtensions.SetEase<Tweener>(TweenSettingsExtensions.SetDelay<Tweener>(TweenSettingsExtensions.SetUpdate<Tweener>(ShortcutExtensions.DOScale(__this.getExpText.GetComponent<RectTransform>(), new Vector3(1f, 1f, 1f), 0.4f), true), 0.1f), 27);
        //        //__this.UpdatePageText();
        //        //__this.UpdateGetP();

        //        return false;
        //    }
        //}

    }
}