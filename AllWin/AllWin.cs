using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AllWin

 {

    public class Settings : UnityModManager.ModSettings
    {
    }


    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

          //  settings = Settings.Load<Settings>(modEntry);

            Logger = modEntry.Logger;

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
      
            enabled = value;

            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
           // settings.Save(modEntry);
        }

    }


    /// <summary>
    ///   // , new Type[] { typeof(bool), typeof(int) }
    /// </summary>
    [HarmonyPatch(typeof(BattleEndWindow), "BattleEnd")]
    public static class The_Patch
    {

        static void Prefix(ref bool actorWin)
        {
            if (!Main.enabled)
            {
                return ;
            }
            Main.Logger.Log("you win, actual " + actorWin);
            actorWin = true;
            return ;
        }

    }

}
