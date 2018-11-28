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
    /// 对话界面事件：按下小键盘数字键时选择对应选项
    /// </summary>
    [HarmonyPatch(typeof(MassageWindow), "Update")]
    public static class MassageWindow_Update_Patch
    {
        public static KeyCode[] alternativeKeyCodes = new KeyCode[12]
        {
            KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4,
            KeyCode.Keypad5, KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8,
            KeyCode.Keypad9, KeyCode.Keypad0, KeyCode.KeypadMinus, KeyCode.KeypadPlus,
        };


        public static int GetAlternativeKeysDown()
        {
            for (int i = 0; i < alternativeKeyCodes.Length; ++i)
            {
                if (Input.GetKeyDown(alternativeKeyCodes[i])) return i;
            }
            return -1;
        }


        private static void Postfix(MassageWindow __instance)
        {
            if (!Main.enabled || Main.binding_key || !Main.settings.useNumpadKeysInMessageWindow) return;

            if (!__instance.massageWindow.activeInHierarchy) return;
            if (__instance.itemWindowIsShow) return;
            if (ShopSystem.instance != null && ShopSystem.instance.shopWindow.activeInHierarchy) return;
            if (BookShopSystem.instance != null && BookShopSystem.instance.shopWindow.activeInHierarchy) return;
            if (GetItemWindow.instance != null && GetItemWindow.instance.getItemWindow.activeInHierarchy) return;
            if (GetActorWindow.instance != null && GetActorWindow.instance.getActorWindow.activeInHierarchy) return;
            if (ActorMenu.instance != null && ActorMenu.instance.actorMenu.activeInHierarchy) return;

            int chooseIndex = GetAlternativeKeysDown();
            if (chooseIndex < 0 || chooseIndex >= __instance.chooseHolder.childCount) return;

            GameObject choose = __instance.chooseHolder.GetChild(chooseIndex).gameObject;
            if (!choose.GetComponent<Button>().interactable) return;

            int chooseId = int.Parse(choose.name.Split(',')[1]);
            SetChoose(chooseId);
        }


        // 摘抄自 OnChoose::SetChoose 方法
        public static void SetChoose(int chooseId)
        {
            if (MassageWindow.instance.chooseItemEvents.Contains(chooseId))
            {
                MassageWindow.instance.ShowItemsWindow(chooseId);
            }
            else if (MassageWindow.instance.chooseActorEvents.Contains(chooseId))
            {
                MassageWindow.instance.ShowActorsWindow(chooseId);
            }
            else
            {
                int actorId = int.Parse(DateFile.instance.eventDate[chooseId][2]);
                int targetEventId = int.Parse(DateFile.instance.eventDate[chooseId][7]);
                MassageWindow.instance.GetEventBooty((actorId != -1) ? actorId : DateFile.instance.MianActorID(), chooseId);
                if (targetEventId > -99)
                {
                    MassageWindow.instance.mianEventDate[2] = targetEventId;
                }
                else if (targetEventId == -99)
                {
                    chooseId = -chooseId;
                }
                MassageWindow.instance.ChangeMassageWindow(chooseId);
            }
        }
    }


    /// <summary>
    /// Esc / 鼠标右键事件处理类
    /// 除一般条件外，原游戏还需要满足以下两个条件之一，才会处理该事件：1. 对话窗口未打开；2. 当前窗口在对话窗口之上。
    /// 现在改为就算不满足上述两个条件，也处理该事件。
    /// </summary>
    [HarmonyPatch(typeof(EscKeyboardHandler), "Update")]
    public static class EscKeyboardHandler_Update_Patch
    {
        private static bool Prefix(EscKeyboardHandler __instance, bool ___keyState)
        {
            if (!Main.enabled || Main.binding_key || !Main.settings.escAsLastOption) return true;

            if (!EscKeyboardHandler.stopEscKey && !Loading.instance.LoadingWindow.activeInHierarchy && !DateFile.instance.doMapMoveing)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
                {
                    if (!___keyState)
                    {
                        ___keyState = true;
                        EscWinComponent.EscTop();
                    }
                }
                else
                {
                    ___keyState = false;
                }
            }

            return false;
        }
    }


    /// <summary>
    /// 为对话窗口类加上 Esc / 鼠标右键事件处理组件
    /// </summary>
    [HarmonyPatch(typeof(MassageWindow), "Start")]
    public static class MassageWindow_Start_Patch
    {
        private static bool Prefix(MassageWindow __instance)
        {
            if (!Main.enabled || Main.binding_key || !Main.settings.escAsLastOption) return true;

            // When adding a component to a GameObject using AddComponent<C>() the component C's OnEnable method is called immediately.
            var escWinComponent = __instance.massageWindowBack.AddComponent<EscWinComponent>();
            escWinComponent.escEvent.AddListener(OnEsc);

            return true;
        }


        private static void OnEsc()
        {
            if (!Main.enabled || Main.binding_key || !Main.settings.escAsLastOption) return;

            var chooseIndex = MassageWindow.instance.chooseHolder.childCount - 1;
            if (chooseIndex < 0) return;

            GameObject choose = MassageWindow.instance.chooseHolder.GetChild(chooseIndex).gameObject;
            if (!choose.GetComponent<Button>().interactable) return;

            int chooseId = int.Parse(choose.name.Split(',')[1]);
            MassageWindow_Update_Patch.SetChoose(chooseId);

            // 让 Esc / 鼠标右键事件处理组件在对话窗口中可以被触发多次
            var field = typeof(EscWinComponent).GetField("s_clickFlag", BindingFlags.NonPublic | BindingFlags.Static);
            field.SetValue(null, false);
        }
    }


    /// <summary>
    /// 修复原游戏在对话界面开启人物窗口时，依然可以选择对话分支的 bug
    /// </summary>
    [HarmonyPatch(typeof(OnChoose), "SetChoose")]
    public static class OnChoose_SetChoose_Patch
    {
        private static bool Prefix()
        {
            if (ActorMenu.instance != null && ActorMenu.instance.actorMenu.activeInHierarchy) return false;

            return true;
        }
    }
}