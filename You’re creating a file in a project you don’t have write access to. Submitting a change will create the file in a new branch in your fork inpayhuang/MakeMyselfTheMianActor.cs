using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Harmony12;
using UnityModManagerNet;
using System.Reflection;
//using System.IO;

namespace MakeMyselfTheMianActor
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
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static Settings settings;
        public static bool enabled;
        public static bool check;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            settings = Settings.Load<Settings>(modEntry);

            Logger = modEntry.Logger;

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            return true;
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
                return false;

            enabled = value;
            return true;
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (GUILayout.Button("传剑"))
            {
                try
                {
                    Main.Logger.Log("传剑给自己：START");
                    check = true;
                    DateFile.instance.oldMianActorId = DateFile.instance.mianActorId;
                    DateFile.instance.SetEvent(new int[3] { 0, -1, 17 }, addToFirst: true);
                }
                catch (Exception ex)
                {
                    check = false;
                    Main.Logger.Log("传剑异常.请报BUG.");
                    Main.Logger.Log("Message:" + ex.Message);
                    Main.Logger.Log("Source:" + ex.Source);
                    Main.Logger.Log("StackTrace:" + ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// 生平遺惠使用界面关闭后触发
        /// </summary>
        [HarmonyPatch(typeof(ActorScore), "CloseScoreWindow")]
        public static class CloseScoreWindow_Patch
        {
            public static void Postfix()
            {
                if (!Main.enabled)
                {
                    return;
                }
                if (check)
                {
                    check = false;
                    int mainId = DateFile.instance.mianActorId;
                    if (!DateFile.instance.actorGetScore.ContainsKey(mainId)) return;
                    DateFile.instance.actorGetScore.Remove(mainId);
                    Main.Logger.Log("清除生平遺惠");
                    Main.Logger.Log("传剑给自己：END");
                }
            }
        }
    }
}
