using Harmony12;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityModManagerNet;

namespace Samsara1k
{
    // [HarmonyPatch(typeof(ActorMenu), "NewMianActor")]
    // public class ActorMenu_NewMainActor_Patch
    // {
    //     public static void Prefix()
    //     {
    //         if (Main.enabled)
    //         {
    //             DateFile.instance.samsara += 999;
    //             Main.logger.Log(DateFile.instance.samsara.ToString());
    //         }
    //     }
    // }

    [HarmonyPatch(typeof(MassageWindow), "EndEvent11_1")]
    public class MassageWindow_EndEvent11_1_Patch
    {
        public static void Prefix()
        {
            if (Main.enabled)
            {
                if (MassageWindow.instance.eventValue[1] == 1)
                {
                    DateFile.instance.samsara += 999;
                    Main.logger.Log(DateFile.instance.samsara.ToString());
                }
            }
        }
    }

    public class Main
    {
        public static UnityModManager.ModEntry.ModLogger logger;

        public static bool enabled;

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            modEntry.OnToggle = new Func<UnityModManager.ModEntry, bool, bool>(OnToggle);
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }
    }
}