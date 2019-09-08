using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmartWear
{
    //BuildingWindow.CloseStudyWindow
    [HarmonyPatch(typeof(BuildingWindow), "CloseStudyWindow")]
    public class BuildingWindow_CloseStudyWindow_Patch
    {
        private static void Prefix(HomeSystem __instance)
        {
            // 關閉視窗時還原原本的裝備
            // 不檢查 Main.Enabled 與 Setting, 避免使用者於關閉視窗前, 先關閉功能, 造成一些資料錯亂的問題
            StateManager.RestoreAll();
        }
    }

    public class BuildingWindow_Patch
    {
        /// <summary>
        /// 切換功法 / 裝備的共用函數
        /// </summary>
        /// <param name="homeSystem">HomeSystem instance</param>
        /// <param name="keyValue">關鍵值, 修習=studySkillId, 突破=levelUPSkillId, 讀書=readBookId</param>
        /// <param name="aptitudeFunc">使用 keyValue 計算對應的技能種類的Function</param>
        public static void SwitchProc(BuildingWindow homeSystem, int keyValue, Func<int, int> aptitudeFunc)
        {
            if (!Main.Enabled) return;
            if (keyValue <= 0) return;

            if (Main.settings.HomeSystemAutoAccessories)
            {
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
            }

            if (Main.settings.HomeSystemGongFaIndex >= 0)
                StateManager.UseGongFa(Main.settings.HomeSystemGongFaIndex);
        }
    }

    //BuildingWindow.UpdateReadBookWindow()
    [HarmonyPatch(typeof(BuildingWindow), "UpdateReadBookWindow")]
    public class BuildingWindow_UpdateReadBookWindow_Patch
    {
        static int[] Emptys = new int[] { 0, 0, 0 };
        static bool CallByPatch = false;
        //static ItemData[] Emptys;

        //static HomeSystem_UpdateReadBookWindow_Patch()
        //{
        //    Emptys = Enumerable.Repeat(new ItemData(0, ItemSource.Bag), 3).ToArray();
        //}

        private static void Prefix(BuildingWindow __instance)
        {
            if (!Main.Enabled ||
                __instance.readBookId <= 0 ||
                CallByPatch) return;

            if (Main.settings.HomeSystemGongFaIndex >= 0)
                StateManager.UseGongFa(Main.settings.HomeSystemGongFaIndex);
            if (Main.settings.HomeSystemAutoAccessories)
            {
                // 換上三個空的避免裝備資質影響判斷
                StateManager.Equip(Emptys, EquipSlot.Accessory1);
            }

            // HomeSystem_Patch.SwitchProc(__instance, __instance.readBookId, AptitudeTypeHelper.GetAptitudeTypeByBookId);

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

        private static void Postfix(BuildingWindow __instance)
        {
            if (!Main.Enabled ||
                __instance.readBookId <= 0 ||
                !Main.settings.HomeSystemAutoAccessories) return;

            if (CallByPatch)
            {
                CallByPatch = false;
                return;
            }

            int aptitudeType;
            if (__instance.studySkillTyp < 17)
            {
                // 技藝
                aptitudeType = AptitudeTypeHelper.GetAptitudeTypeBySkillType(__instance.studySkillTyp);
            }
            else
            {
                // 功法
                aptitudeType = AptitudeTypeHelper.GetAptitudeTypeByBookId(__instance.readBookId);
            }

            var items = ItemHelper.GetAptitudeUpOrComprehensionUpAccessories(aptitudeType);
            if (Main.settings.AdvancedReadBookMode &&
                BuildingWindow_GetNeedInt_Patch.LastNeedInt <= 50)
            {
                // 如果開了進階閱讀模式, 閱讀難度小於等於50時, 悟性優先
                items = from item in items
                        orderby item.ComprehensionUp descending
                        select item;
            }
            else
            {
                // 否則資質優先
                items = from item in items
                        orderby item.AptitudeUp descending, item.ComprehensionUp descending
                        select item;
            }

            StateManager.EquipAccessories(items);
            // 重新整理
            CallByPatch = true;
            __instance.UpdateReadBookWindow();
        }
    }

    // BuildingWindow.GetNeedInt
    [HarmonyPatch(typeof(BuildingWindow), "GetNeedInt")]
    public class BuildingWindow_GetNeedInt_Patch
    {
        public static int LastNeedInt = 0;
        private static void Postfix(int __result)
        {
            LastNeedInt = __result;
        }

    }

    //BuildingWindow.UpdateLevelUPSkillWindow()
    [HarmonyPatch(typeof(BuildingWindow), "UpdateLevelUPSkillWindow")]
    public class BuildingWindow_UpdateLevelUPSkillWindow_Patch
    {
        private static void Prefix(BuildingWindow __instance)
        {
            BuildingWindow_Patch.SwitchProc(__instance, __instance.levelUPSkillId, AptitudeTypeHelper.GetAptitudeTypeByGongFaId);
        }
    }

    //BuildingWindow.UpdateStudySkillWindow()
    [HarmonyPatch(typeof(BuildingWindow), "UpdateStudySkillWindow")]
    public class BuildingWindow_UpdateStudySkillWindow_Patch
    {
        private static void Prefix(BuildingWindow __instance, int ___studySkillId)
        {
            BuildingWindow_Patch.SwitchProc(__instance, ___studySkillId, AptitudeTypeHelper.GetAptitudeTypeByGongFaId);
        }
    }

}
