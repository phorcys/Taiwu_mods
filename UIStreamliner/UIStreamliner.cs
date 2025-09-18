using Harmony12;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace UIStreamliner
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool IsActorSay = true;
        public bool IsUpdateBuildingUp = true;
        public bool IsBattleTips = true;
        internal bool IsUpdateTips = true;
        internal bool IsSeekTips = true;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
    public static class Main
    {

        public static bool enabled;
        public static Settings settings;


        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool binding_key = false;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            return true;
        }


        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }


        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginVertical("box");
            settings.IsActorSay = GUILayout.Toggle(settings.IsActorSay, "较艺弹窗改为提示");
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            settings.IsUpdateBuildingUp = GUILayout.Toggle(settings.IsUpdateBuildingUp, "建筑升级人力最大化");
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            settings.IsBattleTips = GUILayout.Toggle(settings.IsBattleTips, "战斗时按Shift隐藏提示");
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            settings.IsUpdateTips = GUILayout.Toggle(settings.IsUpdateTips, "突破、研读提示智力");
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            settings.IsSeekTips = GUILayout.Toggle(settings.IsSeekTips, "显示NPC可请教品级（仅技艺）");
            GUILayout.EndVertical();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

    }

    #region 删除部分提示（比如较艺提示轮到自己）

    [HarmonyPatch(typeof(YesOrNoWindow), "SetYesOrNoWindow")]
    public static class YesOrNoWindow_SetYesOrNoWindow_Patch
    {
        private static void ActorSay(bool isLeft, string message)
        {
            // 调用 TipsWindow 的 ActorSay 方法显示信息
            var pos = isLeft ? SkillBattleSystem.instance.actorBattleTyp : SkillBattleSystem.instance.enemyBattleTyp;
            var tipPos = TipsWindow.instance.transform.InverseTransformPoint(pos.transform.position) + new Vector3(isLeft ? -70f : -110f, -170f, 0.0f);
            TipsWindow.instance.ActorSay(0, new string[] { message }, 240, tipPos.x, tipPos.y, 300);
        }

        private static bool Prefix(YesOrNoWindow __instance, ref int clickId, ref string title, ref string massage)
        {
            if (!Main.enabled || !Main.settings.IsActorSay)
                return true;

            if (clickId == -1 && title.Contains("我方提问"))
            {
                ActorSay(true, massage);
                return false;
            }
            else if (clickId == 801)
            {
                ActorSay(false, massage);
                __instance.StartCoroutine(SkillBattleSystem.instance.EnemySetQusetion(1f));
                return false;
            }
            else if (clickId == 405)
            {
                //额外：商人不够钱时红色加深提示
                if (massage == "对方没有足够的「银钱」支付给你，仍然要完成此次交易吗？")
                    massage = DateFile.instance.SetColoer(10004, massage);
            }
            return true;
        }
    }

    #endregion

    #region 建筑人力分配简化

    [HarmonyPatch(typeof(BuildingWindow), "UpdateBuildingUpWindow")]
    public static class BuildingWindow_UpdateBuildingUpWindow_Patch
    {

        //应该其它地方调用导致按钮状态更新被覆盖，延时更新
        static IEnumerator DelayedAction(Button but, bool interactable, float delay)
        {
            yield return new WaitForSeconds(delay);
            but.interactable = interactable;
        }

        private static void Postfix(BuildingWindow __instance, ref int ___useMenpower2, ref int ___needManpower2, ref int ___buildingId, ref int ___buildingTime2)
        {
            if (!Main.enabled || !Main.settings.IsUpdateBuildingUp)
                return;
            if (!__instance.buildingUpCanBuildingButton.interactable)
                return;

            //当前人力
            int useManPower = UIDate.instance.GetUseManPower();

            //最低所需人力
            int needManpower2 = ___needManpower2;
            if (int.Parse(DateFile.instance.basehomePlaceDate[___buildingId][21]) < 0)
                needManpower2 *= 20;

            ///当前分配人力应当
            ___useMenpower2 = Mathf.Min(needManpower2 + ___needManpower2 - 1, 20, useManPower);

            ////需要时间
            int num = Mathf.Max(needManpower2 * 2 - ___useMenpower2, 1);

            //用户当前分配人力
            ___buildingTime2 = num;
            __instance.buildingUpUseMenpowerText.text = ___useMenpower2.ToString();
            __instance.buildingUpNeedTimeText.text = num.ToString();
            __instance.StartCoroutine(DelayedAction(__instance.buildingUpUseMenpowerUpButton, ___useMenpower2 < 20 && ___useMenpower2 < useManPower && num > 1, 0.1f));
            __instance.StartCoroutine(DelayedAction(__instance.buildingUpUseMenpowerDownButton, ___needManpower2 < ___useMenpower2, 0.1f));
        }
    }

    [HarmonyPatch(typeof(BuildingWindow), "UpdateNeedTime2")]
    public static class BuildingWindow_UpdateNeedTime2_Patch
    {
        private static void Postfix(BuildingWindow __instance, ref int ___useMenpower2, ref int ___needManpower2)
        {
            if (!Main.enabled || !Main.settings.IsUpdateBuildingUp)
                return;
            int useManPower = UIDate.instance.GetUseManPower();
            //当人力可以满足时,按钮可修改
            int num = ___needManpower2 * 2 - ___useMenpower2;
            __instance.buildingUpUseMenpowerUpButton.interactable = ___useMenpower2 < 20 && ___useMenpower2 < useManPower && num > 1;
        }
    }

    #endregion

    #region 战斗默认不显示提示

    //PointerEnter.OnPointerEnter代码太长，直接WindowSwitch里实现
    [HarmonyPatch(typeof(WindowManage), "WindowSwitch")]
    public static class WindowManage_WindowSwitch_Patch
    {
        private static bool Prefix(ref bool on, ref GameObject tips)
        {
            if (!Main.enabled || !Main.settings.IsBattleTips)
                return true;

            // 非战斗状态下正常执行原方法
            if (!BattleSystem.Exists || !BattleSystem.instance.battleWindow.activeInHierarchy)
                return true;

            // 非战斗状态下正常执行原方法
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                return true;
            on = false;
            tips = null;
            return true;
        }
    }

    #endregion

    #region 突破、研读显示智力

    [HarmonyPatch(typeof(ActorMenu), "UpdateMianQi")]
    public static class ActorMenu_UpdateMianQi_Patch
    {
        public static string GetIQ { get; private set; } = "（null）";
        private static void Postfix(ActorMenu __instance) =>
            GetIQ = __instance.actorId == DateFile.instance.MianActorID() ?
            DateFile.instance.SetColoer(20002, $"（IQ:{__instance.intText.text}）") :
            GetIQ;
    }

    [HarmonyPatch(typeof(BuildingWindow), "UpdateLevelUPSkillWindow")]
    public static class BuildingWindow_UpdateLevelUPSkillWindow_Patch
    {
        //获取的值总是很奇怪，懒得深究
        //public static string GetIQ() => DateFile.instance.SetColoer(20002, $"（IQ:{DateFile.instance.BaseAttr(DateFile.instance.MianActorID(), 4, 0)}）");

        private static void Postfix(int ___levelUPSkillId, Text ___levelUPSizeText)
        {
            if (!Main.enabled || !Main.settings.IsUpdateTips)
                return;

            if (___levelUPSkillId <= 0)
                return;

            ___levelUPSizeText.text += ActorMenu_UpdateMianQi_Patch.GetIQ;
        }
    }

    [HarmonyPatch(typeof(BuildingWindow), "UpdateReadBookWindow")]
    public static class BuildingWindow_UpdateReadBookWindow_Patch
    {
        public static float threshold = 0.3f;
        private static float lastTime = 0;

        private static void Postfix(int ___readBookId, Text ___needIntText)
        {
            if (!Main.enabled || !Main.settings.IsUpdateTips)
                return;
            if (___readBookId <= 0) return;

            if (Time.time - lastTime < threshold && lastTime != 0)
                return;
            lastTime = Time.time;

            ___needIntText.text += ActorMenu_UpdateMianQi_Patch.GetIQ;
        }
    }
    #endregion

    #region 请教功法/技艺 显示前置要求  功法未实现

    [HarmonyPatch(typeof(ui_MessageWindow), "SetMassageWindow")]
    public static class Ui_MessageWindow_SetMassageWindow_Patch
    {
        static readonly Dictionary<string, int> SkillKey = new Dictionary<string, int>
        {
            { "Choose,931900001", 0 },
            { "Choose,931900002", 1 },
            { "Choose,931900003", 2 },
            { "Choose,931900004", 3 },
            { "Choose,931900005", 4 },
            { "Choose,931900006", 5 },
            { "Choose,932200001", 6 },
            { "Choose,932200002", 7 },
            { "Choose,932900001", 8 },
            { "Choose,932900002", 9 },
            { "Choose,932200003", 10 },
            { "Choose,932200004", 11 },
            { "Choose,932300001", 12 },
            { "Choose,932300002", 13 },
            { "Choose,932300003", 14 },
            { "Choose,932300004", 15 },
        };
        static readonly string[] Grade = { "零", "九", "八", "七", "六", "五", "四", "三", "二", "一" };

        private static void Postfix(ui_MessageWindow __instance, int[] baseEventDate, int chooseId)
        {
            if (!Main.enabled || !Main.settings.IsSeekTips)
                return;

            var chooseSkills = __instance.chooseHolder.GetComponentsInChildren<Transform>()
               .Where(x => Regex.IsMatch(x.name, @"^Choose,(9319|9322|9323|9329)0"))
               .ToDictionary(keySelector: item => item.name, elementSelector: item => item.Find("MassageChooseText").GetComponent<Text>());

            // 获取NPC基于好感度的可教授技艺等级上限
            int favorLevelLimit = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), baseEventDate[1]) / 6000 - 1;

            favorLevelLimit = Mathf.Max(favorLevelLimit, 0);

            //请教才艺  请教手艺    请教杂艺    请教技艺（门派）    大夫请教
            if (baseEventDate[2] == 9319 || baseEventDate[2] == 9322 || baseEventDate[2] == 9323 || baseEventDate[2] == 9334 || chooseId == 1000000003)
            {
                foreach (var item in chooseSkills)
                {
                    //Choose,931900001
                    var eventDateId = SkillKey[item.Key];
                    string note = SkillNote(SkillKey[item.Key], baseEventDate[1], favorLevelLimit);
                    item.Value.text += note;
                }
            }
        }
        private static string SkillNote(int skillKey, int actorid, int favorLevelLimit)
        {
            // 获取NPC基于资质造诣的可教授技艺等级上限
            int teachLevelLimit = MessageEventManager.Instance.GetSkillValue(actorid, 501 + skillKey);

            // 检查是否满足条件
            bool favorEnough = false;
            bool zhiEnough = false;

            //额外增加修习程度达到50%的判断
            bool isStudyLow = false;

            //判断是否已经全部学习
            bool isStudy = false;

            //检查技艺前置等级
            for (int i = 0; i < 9; i++)
            {
                // 技艺ID
                int zhiEnoughID = int.Parse(DateFile.instance.baseSkillDate[skillKey][i + 1]);
                bool isSkill = DateFile.instance.actorSkills.ContainsKey(zhiEnoughID);
                //修习程度
                bool isLevel = DateFile.instance.GetSkillLevel(zhiEnoughID) > 49;
                if (isSkill && !isLevel)
                    isStudyLow = true;
                if (isSkill && i == 8)
                {
                    isStudy = true;
                    break;
                }

                if (i > favorLevelLimit && i > teachLevelLimit)
                    break;

                //跳过已学习的技艺      修习度为满足50%会影响判断
                if (isSkill)
                    continue;

                //如果待学习的技艺等级超过NPC好感
                if (i < favorLevelLimit)
                    favorEnough = true;

                //如果待学习的技艺等级超过NPC资质
                if (i < teachLevelLimit)
                    zhiEnough = true;
                break;
            }
            teachLevelLimit = Mathf.Min(teachLevelLimit, 9);

            string note = $"（好感{SetColoer(20001 + favorLevelLimit, Grade[favorLevelLimit] + "品")}，" +
                $"资质{SetColoer(20001 + teachLevelLimit, Grade[teachLevelLimit] + "品")}；";
            if (isStudy)
                note += SetColoer(20004, "已学完");
            else if (zhiEnough && favorEnough && !isStudyLow)
                note += SetColoer(20004, "可请教");
            else
            {
                List<string> note2 = new List<string>();
                if (!favorEnough) note2.Add("好感");
                if (!zhiEnough) note2.Add("资质");
                if (isStudyLow) note2.Add("修习程度");
                note += SetColoer(20010, string.Join("，", note2) + "不足");
            }

            return SetColoer(20002, note + "）");
        }

        static string SetColoer(int color, string text) =>
            DateFile.instance.SetColoer(color, text);
    }

    #endregion

}
