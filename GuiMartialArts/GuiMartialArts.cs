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
using UnityEngine.EventSystems;

namespace GuiMartialArts
{
    public class Settings : UnityModManager.ModSettings
    {
        public int AddWind = 0;
        public int ReadLevel = 1;
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

        //static int x = 280;
        //static int y = 50;
        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(title, GUILayout.Width(300));

            GUILayout.BeginHorizontal();
            Main.settings.AddWind = (int)GUILayout.HorizontalSlider(Main.settings.AddWind, 0, 30);
            GUILayout.Label(string.Format("作弊增加成功率：{0}%", Main.settings.AddWind));
            float v = Main.settings.AddWind / 100f;
            GUILayout.Label("<color=#"+ (v * 255 > 16 ? "" : "0") + Convert.ToString((int)(v * 255), 16) + "0000>此举有伤天和，请酌情使用</color>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Main.settings.ReadLevel = (int)GUILayout.HorizontalSlider(Main.settings.ReadLevel, 1, 3);
            GUILayout.Label(string.Format("作弊增加研读页数：{0}倍", Main.settings.ReadLevel));
            v = (Main.settings.ReadLevel-1) / 9f;
            GUILayout.Label("<color=#" + (v * 255 > 16 ? "" : "0") + Convert.ToString((int)(v * 255), 16) + "0000>此举有伤天和，请酌情使用</color>");
            GUILayout.EndHorizontal();
        }


        [HarmonyPatch(typeof(BattleSystem), "UseGongFaStart")]
        public static class BattleSystem_UseGongFaStart_Patch
        {
            public static void Postfix(bool isActor, int gongFaId)
            {
                if (!Main.enabled)
                    return;

                // Logger.Log("使用功法开始 isActor=" + isActor + " gongFaId=" + gongFaId);

                artsData.AddUseGongfa(isActor, gongFaId);
                int _oldEnemyId = DateFile.instance.enemyBattlerIdDate[0];
                // Logger.Log("更换敌人 index=" + index + " _newEnemyId=" + _newEnemyId + " _oldEnemyId=" + _oldEnemyId);
                artsData.AddBattleEnemy(0, _oldEnemyId);

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

                // Logger.Log("使用功法 isActor=" + isActor + " gongFaId=" + gongFaId);

                artsData.AddUseGongfa(isActor, gongFaId);
                int _oldEnemyId = DateFile.instance.enemyBattlerIdDate[0];
                // Logger.Log("更换敌人 index=" + index + " _newEnemyId=" + _newEnemyId + " _oldEnemyId=" + _oldEnemyId);
                artsData.AddBattleEnemy(0, _oldEnemyId);

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

                // Logger.Log("战斗结束 actorWin=" + actorWin + " actorRun=" + actorRun);

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
                // Logger.Log("更换敌人 index=" + index + " _newEnemyId=" + _newEnemyId + " _oldEnemyId=" + _oldEnemyId);
                artsData.AddBattleEnemy(_newEnemyId, _oldEnemyId);

                return;
            }
        }

    }
}