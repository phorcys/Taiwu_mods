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
        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(title, GUILayout.Width(300));
        }


        [HarmonyPatch(typeof(BattleSystem), "UseGongFaStart")]
        public static class SetPlaceActor_ShowEventMassage_Patch
        {
            public static bool Prefix(bool isActor, int gongFaId)
            {
                if (!Main.enabled)
                    return true;

                Logger.Log("使用功法开始 isActor=" + isActor + " gongFaId=" + gongFaId);
                return true;
            }
        }

        [HarmonyPatch(typeof(BattleSystem), "UseGongFa")]
        public static class WorldMapSystem_RemoveActor_Patch1
        {
            public static bool Prefix(bool isActor, int gongFaId)
            {
                if (!Main.enabled)
                    return true;

                Logger.Log("使用功法 isActor=" + isActor + " gongFaId=" + gongFaId);
                return true;
            }
        }

        [HarmonyPatch(typeof(BattleSystem), "BattleEnd")]
        public static class WorldMapSystem_UpdatePlaceActor_Patch1
        {
            public static bool Prefix(bool actorWin, int actorRun = 0)
            {
                if (!Main.enabled)
                    return true;



                Logger.Log("战斗结束 actorWin=" + actorWin + " actorRun=" + actorRun);
                return true;
            }
        }

    }
}