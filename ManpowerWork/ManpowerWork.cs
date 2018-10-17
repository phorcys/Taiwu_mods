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
using System.Reflection.Emit;

namespace ManpowerWork
{

    public static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value) {
                return false;
            }
            enabled = value;
            return true;
        }

    }

    [HarmonyPatch(typeof(WorldMapSystem), "UpdateWorkWindow")]
    public static class WorldMapSystem_UpdateWorkWindow_Patch
    {
        //激活工作按钮
        private static void Postfix(WorldMapSystem __instance)
        {
            if (!Main.enabled)
            {
                return;
            }

            //Main.Logger.Log(" __state get state: " + __state);

            if (DateFile.instance.mianWorldId != __instance.chooseWorldId)
            {
                return;
            }

            if (DateFile.instance.mianPartId != __instance.choosePartId)
            {
                return;
            }

            if (int.Parse(DateFile.instance.GetNewMapDate(__instance.choosePartId, __instance.choosePlaceId, 12)) <= 0)
            {
                return;
            }

            if (DateFile.instance.PlaceIsBad(__instance.choosePartId, __instance.choosePlaceId))
            {
                return;
            }

            for (int i = 0; i < __instance.workTimeNeed.Length; i += 1)
            {
                if (__instance.workTimeNeed[i].activeSelf)
                {
                    return;
                }
            }

            for (int i = 0; i < __instance.allworkButton.Length; i++)
            {
                __instance.allworkButton[i].interactable = true;
            }
            return;
        }
    }

    [HarmonyPatch(typeof(WorldMapSystem), "WorkButton")]
    public static class WorldMapSystem_WorkButton_Patch
    {
        private static void Postfix(WorldMapSystem __instance)
        {
            var wtw = __instance.workTypWindow;
            bool flag = (((DateFile.instance.mianWorldId == __instance.chooseWorldId) && (DateFile.instance.mianPartId == __instance.choosePartId)) && ((DateFile.instance.mianPlaceId == __instance.choosePlaceId) && (int.Parse(DateFile.instance.GetNewMapDate(__instance.choosePartId, __instance.choosePlaceId, 12)) > 0))) && !DateFile.instance.PlaceIsBad(__instance.choosePartId, __instance.choosePlaceId);
            wtw.transform.GetChild(1).gameObject.SetActive(flag);
            return;
        }
    }
}