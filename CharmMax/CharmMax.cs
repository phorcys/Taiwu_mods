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

namespace CharmMax
{

    public static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Logger = modEntry.Logger;

            modEntry.OnToggle = OnToggle;

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
                return false;

            enabled = value;

            return true;
        }

    }


    /// <summary>
    ///  建立人物时拦截并修改结果
    /// </summary>
    [HarmonyPatch(typeof(NewGame), "SetNewGameDate")]
    public static class NewGame_SetNewGameDate_Patch
    {

        private static void Postfix()
        {
            if (!Main.enabled)
            {
                return;
            }

            Dictionary<int, string> actor;
            if (DateFile.instance.actorsDate.TryGetValue(10001, out actor)) 
            {
                actor[15] = "960";
            }

            return;
        }

    }

}