using Harmony12;
using Litfal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SmartWear
{
    //ChoosePlaceWindow.DoMapHealing
    [HarmonyPatch(typeof(ChoosePlaceWindow), "DoMapHealing")]
    public class ChoosePlaceWindow_DoMapHealing_Patch
    {
        private static void Prefix(int typ)
        {
            if (!Main.Enabled || !Main.settings.HealingAutoAccessories) return;

            if (typ == 0)
            {
                // 醫療
                StateManager.EquipAccessories((int)AptitudeType.Treatment);
            }
            else
            {
                // 解毒
                StateManager.EquipAccessories((int)AptitudeType.Poison);
            }
        }

        private static void Postfix()
        {
            StateManager.RestoreAll();
        }
    }
}
