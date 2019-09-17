using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace SkipEnemy
{
    public static class Main
    {
        public static bool Enabled { get; private set; }
        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }
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
            Enabled = value;
            return true;
        }
    }


    // 攔截遭遇逃亡小怪事件
    [HarmonyPatch(typeof(ui_MessageWindow), "SetEventWindow")]
    public class MassageWindow_SetEventWindow_Patch
    {
        private static void Postfix(ui_MessageWindow __instance, int[] eventDate)
        {
            if (Main.Enabled && eventDate.Length == 4 && eventDate[2] == 112)
            {

                var skipBtn = __instance.GetComponentsInChildren<Button>().FirstOrDefault(btn => btn.name == "Choose,11200002");
                if(skipBtn == null)
                {
                    Main.Logger.Log("Could't find the skip choose!");
                    return;
                }
#if (DEBUG)
                Main.Logger.Log("Skip");
#endif
                skipBtn.onClick.Invoke();
            }
        }
    }
}
