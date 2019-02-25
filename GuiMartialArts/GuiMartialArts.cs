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

            if (GUILayout.Button("xxx"))
            {
                if (test == null)
                    test = YesOrNoWindow.instance.gameObject.AddComponent<Test>();
                else
                    Logger.Log("Test已经存在");
            }
        }


        [HarmonyPatch(typeof(BattleSystem), "UseGongFaStart")]
        public static class SetPlaceActor_ShowEventMassage_Patch
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
        public static class WorldMapSystem_RemoveActor_Patch1
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
        public static class WorldMapSystem_UpdatePlaceActor_Patch1
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

    }
}