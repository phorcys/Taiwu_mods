using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityModManagerNet;

namespace KeyBoardShortCut
{
    /// <summary>
    ///  BattleSystem 事件
    /// </summary>
    [HarmonyPatch(typeof(BattleSystem), "Update")]
    public static class BattleSystem_Update_Patch
    {
        static MethodInfo GetMoveKey = typeof(BattleSystem)
            .GetMethod("GetMoveKey", BindingFlags.NonPublic | BindingFlags.Instance);

        private static void Postfix(BattleSystem __instance, bool ___battleEnd)
        {
            if (!Main.enabled || Main.binding_key)
            {
                return;
            }

            if (!___battleEnd && __instance.battleWindow.activeInHierarchy == true)
            {
                //轻功技能
                int skillindex = Main.GetKeyListDown(Main.qinggongskilllist);
                if (skillindex != -1 && Settings.testModifierKey(Main.settings.qinggong_modifier_key) == true)
                {
                    // actorGongFaHolder[1] = 轻功列表
                    var allskill = __instance.actorGongFaHolder[1];
                    if (allskill != null)
                    {
                        var btngo = allskill.childCount > skillindex ? allskill.GetChild(skillindex) : null;
                        if (btngo != null)
                        {
                            // 功法存在
                            int skillid = int.Parse(btngo.name.Split(new char[] { ',' })[1]);
                            //按钮可按下
                            if (btngo.Find("GongFaIcon," + skillid).GetComponent<Button>().interactable == true)
                            {
                                //施放技能
                                BattleSystem.instance.SetUseGongFa(true, skillid);
                                return;
                            }
                        }
                    }
                }

                //特殊技能
                skillindex = Main.GetKeyListDown(Main.specialskilllist);
                if (skillindex != -1 && Settings.testModifierKey(Main.settings.special_modifierkey) == true)
                {
                    // actorGongFaHolder[2] = 绝技列表
                    var allskill = __instance.actorGongFaHolder[2];
                    if (allskill != null)
                    {
                        var btngo = allskill.childCount > skillindex ? allskill.GetChild(skillindex) : null;
                        if (btngo != null)
                        {
                            // 功法存在
                            int skillid = int.Parse(btngo.name.Split(new char[] { ',' })[1]);
                            //按钮可按下
                            if (btngo.Find("GongFaIcon," + skillid).GetComponent<Button>().interactable == true)
                            {
                                //施放技能
                                BattleSystem.instance.SetUseGongFa(true, skillid);
                                return;
                            }
                        }
                    }
                }

                //主技能
                skillindex = Main.GetKeyListDown(Main.mainskilllist);
                if (skillindex != -1)
                {
                    // actorGongFaHolder[0] = 攻击技能列表
                    var allskill = __instance.actorGongFaHolder[0];
                    if (allskill != null)
                    {
                        var btngo = allskill.childCount > skillindex ? allskill.GetChild(skillindex) : null;
                        if (btngo != null)
                        {
                            // 功法存在
                            int skillid = int.Parse(btngo.name.Split(new char[] { ',' })[1]);
                            //按钮可按下
                            if (btngo.Find("GongFaIcon," + skillid).GetComponent<Button>().interactable == true)
                            {
                                //施放技能
                                BattleSystem.instance.SetUseGongFa(true, skillid);
                                return;
                            }
                        }
                    }
                }

                //治疗，治疗毒伤
                if (Main.GetKeyDown(HK_TYPE.HK_HEAL) == true && __instance.doHealButton.interactable == true)
                {
                    BattleSystem.instance.ActorDoHeal(true);
                    return;
                }
                if (Main.GetKeyDown(HK_TYPE.HK_POISON) == true && __instance.doRemovePoisonButton.interactable == true)
                {
                    BattleSystem.instance.ActorDoRemovePoison(true);
                }
            }
        }
    }


    /// <summary>
    /// 战斗界面：结束确认
    /// </summary>
    [HarmonyPatch(typeof(BattleSystem), "Start")]
    public static class BattleSystem_ConfirmEnd_Patch
    {
        private static void Postfix(BattleSystem __instance)
        {
            if (!Main.enabled || Main.binding_key) return;

            var comp = __instance.battleEndWindow.AddComponent<ConfirmComponent>();
            comp.SetActionOnConfirm(() =>
            {
                if (!BattleSystem.instance.battleEndWindow.activeInHierarchy) return;
                if (!BattleSystem.instance.closeBattleEndWindowButton.interactable) return;
                DateFile.instance.PlayeSE(2);
                BattleSystem.instance.CloseBattleEndWindow();
            });
        }
    }


    ///// <summary>
    /////  BattleSystem 战斗显示快捷键
    ///// </summary>
    //[HarmonyPatch(typeof(BattleSystem), "SetGongFa")]
    //public static class BattleSystem_SetGongFa_Patch
    //{
    //    private static void Postfix(BattleSystem __instance, int typ, int id)
    //    {
    //if (!Main.enabled || Main.binding_key)
    //{
    //    return;
    //}
    //        var gotrans = __instance.actorGongFaHolder[typ].Find("BattleGongFa," + id);
    //        int index = 0;
    //        for (int i = 0; i < __instance.actorGongFaHolder[typ].childCount; i++)
    //        {
    //            if (__instance.actorGongFaHolder[typ].GetChild(i) == gotrans)
    //            {
    //                index = i;
    //            }
    //        }

    //        if (gotrans != null)
    //        {
    //            var go = new GameObject();
    //            go.transform.parent = gotrans;
    //            go.transform.localPosition = new Vector3(0.0f, -1.0f);
    //            var text = go.gameObject.AddComponent<Text>();

    //            if (typ ==1)
    //            {
    //                text.text = Main.getqinggongskill_keystr(index);
    //            }else if( typ ==2)
    //            {
    //                text.text = Main.getspecialskill_keystr(index);
    //            }
    //        }
    //    }
    //}


    ///// <summary>
    /////  BattleSystem 战斗显示快捷键
    ///// </summary>
    //[HarmonyPatch(typeof(BattleSystem), "SetAttackGongFa")]
    //public static class BattleSystem_SetAttackGongFa_Patch
    //{
    //    private static void Postfix(BattleSystem __instance, int id)
    //    {
    //if (!Main.enabled || Main.binding_key)
    //{
    //    return;
    //}
    //        Main.Logger.Log(" try add key info to attach skill " + id);
    //        var gotrans = __instance.actorGongFaHolder[0].Find("BattleGongFa," + id);
    //        int index = -1;
    //        for (int i = 0; i < __instance.actorGongFaHolder[0].childCount; i++)
    //        {
    //            if (__instance.actorGongFaHolder[0].GetChild(i) == gotrans)
    //            {
    //                index = i;
    //            }
    //        }
    //        Main.Logger.Log(" try add key info to attach skill " + id + " found index：" + index);
    //        if (gotrans != null && index >= 0)
    //        {
    //            var go = new GameObject();
    //            go.transform.parent = gotrans;
    //            var text = go.gameObject.AddComponent<Text>();
    //            text.text = Main.getmainskill_keystr(index);
    //            go.transform.localPosition = new Vector3(0.0f, -1.0f);
    //
    //            Main.Logger.Log(" try add key info to attach skill " + id + " found index：" + index + " text:" + text.text);
    //        }
    //    }
    //}
}