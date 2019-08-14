using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWear
{
    // CloseMakeWindow
    [HarmonyPatch(typeof(MakeSystem), "CloseMakeWindow")]
    public class MakeSystem_CloseMakeWindow_Patch
    {
        public static void Prefix()
        {
            // 關閉視窗時還原原本的裝備
            // 不檢查 Main.Enabled, 避免使用者於關閉視窗前, 先關閉Mod, 造成一些資料錯亂的問題
            StateManager.RestoreAll();
        }
    }


    // ShowMakeWindow
    [HarmonyPatch(typeof(MakeSystem), "ShowMakeWindow")]
    public class MakeSystem_ShowMakeWindow_Patch
    {
        public static void Postfix(MakeSystem __instance, int ___baseMakeTyp)
        {
            if (!Main.Enabled || !Main.settings.MakeSystemAutoAccessories) return;
            StateManager.EquipAccessories(AptitudeTypeHelper.GetAptitudeTypeByBaseMakeType(___baseMakeTyp));
            __instance.UpdateMakeWindow();
        }
    }
}
