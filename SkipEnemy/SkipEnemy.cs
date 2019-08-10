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
    [HarmonyPatch(typeof(MassageWindow), "SetEventWindow")]
    public class MassageWindow_SetEventWindow_Patch
    {
        private static void Postfix(MassageWindow __instance, int[] eventDate)
        {
            if(Main.Enabled && eventDate.Length == 4 && eventDate[2] == 112)
            {
#if (DEBUG)
                Main.Logger.Log("Skip");
#endif
                // make a choose and click
                GameObject choose = UnityEngine.Object.Instantiate<GameObject>(__instance.massageChoose1, Vector3.zero, Quaternion.identity);
                choose.name = "Choose,11200002";
                choose.GetComponent<Button>().onClick.Invoke();
            }
        }
    }
}
