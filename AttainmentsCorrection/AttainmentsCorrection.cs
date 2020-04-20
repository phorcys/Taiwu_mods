using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityModManagerNet;

namespace AttainmentsCorrection
{

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static Type patchType = typeof(Harmony_Patches);

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);

            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            harmony.Patch(AccessTools.Method(typeof(DateFile), "GetFamilyGongFaLevel"), new HarmonyMethod(patchType, "GetFamilyGongFaLevel_Prefix"));

            return true;
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            if(GUILayout.Toggle(settings.UsedMod, "Mod的计算函数"))
            {
                settings.UsedMod = true;
                settings.UsedOriginal = false;
            }
            if (GUILayout.Toggle(settings.UsedOriginal, "原版的计算函数"))
            {
                settings.UsedMod = false;
                settings.UsedOriginal = true;
            }

            GUILayout.EndHorizontal();
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Main.settings.Save(modEntry);
        }

    }

    public static class Harmony_Patches
    {
        public static bool GetFamilyGongFaLevel_Prefix(DateFile __instance, ref int __result,int actorId, int typ, bool getSize = false)
        {
            if (!Main.enabled || Main.settings.UsedOriginal || actorId == 0)
                return true;

            
            int gongFaCount = 0;
            float[] pages = new float[9];

            foreach(var gongFaId in __instance.actorGongFas[actorId].Keys)
            {
                if (int.Parse(__instance.gongFaDate[gongFaId][1]) == typ)
                {
                    int gongFaGrade = int.Parse(__instance.gongFaDate[gongFaId][2]) - 1;
                    pages[gongFaGrade] += __instance.GetGongFaFLevel(actorId, gongFaId) * 
                        (__instance.GetGongFaAchievement(actorId, gongFaId) / int.Parse(__instance.gongFaDate[gongFaId][5]));
                    gongFaCount++;
                }
            }
            
            int attainments = 0;
            for (int j = 0; j < pages.Length; j++)
            {
                float i = pages[j];

                if(pages[j] <= 10)
                {

                }
                else if(pages[j] <= 50)
                {
                    i = (float)(-18.9957 - 0.55331 * i + 0.00343 * i * i + 14.8466 * Mathf.Log(i));
                }
                else
                {
                    i = Mathf.Log(i, (float)1.25893) + (float)3.0103;
                }
                i *= (j + 1);
                attainments += Mathf.RoundToInt(i);
            }

            if (actorId == __instance.MianActorID() && StorySystem.useFoodId > 0)
                attainments += int.Parse(__instance.GetItemDate(StorySystem.useFoodId, 50601 + typ));
            __result = getSize ? gongFaCount : attainments;

            return false;
        }
    }

    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }

        public bool UsedMod = true;
        public bool UsedOriginal = false;

    }

}
