using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmartWear
{
    //HomeSystem.CloseStudyWindow
    [HarmonyPatch(typeof(HomeSystem), "CloseStudyWindow")]
    public class HomeSystem_CloseStudyWindow_Patch
    {
        private static void Prefix(HomeSystem __instance)
        {
            // 關閉視窗時還原原本的裝備
            // 不檢查 Main.Enabled 與 Setting, 避免使用者於關閉視窗前, 先關閉功能, 造成一些資料錯亂的問題
            StateManager.Restore();
        }
    }

    public class HomeSystem_Patch
    {
        /// <summary>
        /// 切換功法 / 裝備的共用函數
        /// </summary>
        /// <param name="homeSystem">HomeSystem instance</param>
        /// <param name="keyValue">關鍵值, 修習=studySkillId, 突破=levelUPSkillId, 讀書=readBookId</param>
        /// <param name="aptitudeFunc">使用 keyValue 計算對應的技能種類的Function</param>
        public static void SwitchProc(HomeSystem homeSystem, int keyValue, Func<int, int> aptitudeFunc)
        {
            if (!Main.Enabled) return;
            if (keyValue <= 0) return;

            int aptitudeType;
            if (homeSystem.studySkillTyp < 17)
            {
                // 技藝
                aptitudeType = AptitudeTypeHelper.GetAptitudeTypeBySkillType(homeSystem.studySkillTyp);
            }
            else 
            {
                // 功法
                aptitudeType = aptitudeFunc(keyValue);
            }
            StateManager.EquipAccessories(aptitudeType);
            StateManager.UseGongFa(Main.settings.HomeSystemGongFaIndex);
        }
    }

    //HomeSystem.UpdateReadBookWindow()
    [HarmonyPatch(typeof(HomeSystem), "UpdateReadBookWindow")]
    public class HomeSystem_UpdateReadBookWindow_Patch
    {
        private static void Prefix(HomeSystem __instance)
        {
            HomeSystem_Patch.SwitchProc(__instance, __instance.readBookId, AptitudeTypeHelper.GetAptitudeTypeByBookId);
            //
            //DateFile df = DateFile.instance;
            //int actorId = df.MianActorID();
            //int readSkillId = int.Parse(df.GetItemDate(__instance.readBookId, 32, true));
            //int num5 = int.Parse(df.skillDate[readSkillId][3]);
            // n6 <= 100

            //int n6 = UnityEngine.Mathf.Min(df.GetActorValue(actorId, 501 + num5, true) * 100 / int.Parse(df.skillDate[readSkillId][15]), 100);
            //int num = 1000 - 10 * n6;
            //num < 50;
            //1000 - 10 * n6 < 50;
            //10 * n6 > 950;
            //n6 > 95;
            // df.GetActorValue(actorId, 501 + num5, true) * 100 / df.skillDate[readSkillId][15] > 95
            // df.GetActorValue(actorId, 501 + num5, true) * 100 > 95 * df.skillDate[readSkillId][15]
            // df.GetActorValue(actorId, 501 + num5, true) > 95 * df.skillDate[readSkillId][15] / 100

        }
    }

    //HomeSystem.UpdateLevelUPSkillWindow()
    [HarmonyPatch(typeof(HomeSystem), "UpdateLevelUPSkillWindow")]
    public class HomeSystem_UpdateLevelUPSkillWindow_Patch
    {
        private static void Prefix(HomeSystem __instance)
        {
            HomeSystem_Patch.SwitchProc(__instance, __instance.levelUPSkillId, AptitudeTypeHelper.GetAptitudeTypeByGongFaId);
        }
    }

    //HomeSystem.UpdateStudySkillWindow()
    [HarmonyPatch(typeof(HomeSystem), "UpdateStudySkillWindow")]
    public class HomeSystem_UpdateStudySkillWindow_Patch
    {
        private static void Prefix(HomeSystem __instance, int ___studySkillId)
        {
            HomeSystem_Patch.SwitchProc(__instance, ___studySkillId, AptitudeTypeHelper.GetAptitudeTypeByGongFaId);
        }
    }

}
