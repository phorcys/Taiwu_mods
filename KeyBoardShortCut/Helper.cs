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


    public class ActionsComponent : MonoBehaviour
    {
        public struct ActionSet
        {
            public Action action;
            public List<Func<System.Object, bool>> checks;
        }
        private List<ActionSet> actionSets = new List<ActionSet>();
        private System.Object obj = null;

        private List<Func<System.Object, bool>> currentChecks = new List<Func<System.Object, bool>>();

        public ActionsComponent With(System.Object obj)
        {
            this.obj = obj;
            return this;
        }

        public ActionsComponent OnCheck(Func<System.Object, bool> check)
        {
            if (check != null) currentChecks.Add(check);
            return this;
        }

        public ActionsComponent OnCheck(HK_TYPE key)
        {
            return OnCheck(_ => Main.GetKeyDown(key));
        }

        public ActionsComponent OnCheck(CHECK_TYPE type)
        {
            switch(type)
            {
                case CHECK_TYPE.COMFIRM:
                    return OnCheck(_ => Checks.Confirm());
                case CHECK_TYPE.CLOSE:
                    return OnCheck(_ => Checks.Close());
                case CHECK_TYPE.REMOVE:
                    return OnCheck(_ => Checks.Remove());
            }
            return this;
        }

        public ActionsComponent AddAction(Action action)
        {
            if (currentChecks.Count > 0)
            {
                actionSets.Add(new ActionSet
                {
                    checks = currentChecks,
                    action = action,
                });
                currentChecks = new List<Func<System.Object, bool>>();
            }
            return this;
        }

        public ActionsComponent AddModifyActions(Action OnDown, Action OnUp)
        {
            OnCheck(HK_TYPE.DECREASE);
            AddAction(OnDown);
            OnCheck(HK_TYPE.INCREASE);
            AddAction(OnUp);
            return this;
        }

        public ActionsComponent AddNavActions(Action OnLeft, Action OnRight)
        {
            OnCheck(HK_TYPE.NAV_LEFT);
            AddAction(OnLeft);
            OnCheck(HK_TYPE.NAV_RIGHT);
            AddAction(OnRight);
            return this;
        }

        public void Update()
        {
            if (!Main.on) return;
            actionSets.ForEach(set =>
            {
                if (set.action == null) return;
                if (set.checks.All(c => c(obj))) set.action();
            });
        }
    }

    public enum CHECK_TYPE
    {
        COMFIRM,
        CLOSE,
        REMOVE,
    };

    public class Checks
    {
        public static bool Confirm()
        {
            return Main.GetKeyDown(HK_TYPE.COMFIRM) || Main.GetKeyDown(HK_TYPE.CONFIRM2);
        }

        public static bool Close()
        {
            return Main.GetKeyDown(HK_TYPE.CLOSE) || Input.GetMouseButtonDown(1);
        }

        public static bool Remove()
        {
            return Main.GetKeyDown(HK_TYPE.REMOVE_ITEM);
        }

        public static bool HasDialog() {
            if (YesOrNoWindow.instance.yesOrNoIsShow) return true;
            if (Utils.isUIActive("ui_Dialog")) return true;
            return false;
        }
    }


    // 帮助
    public static class Utils
    {
        // 拿到UI
        public static UIBase GetUI(string name)
        {
            return UIManager.Instance.GetUI(name);
        }

        // UI是否活动
        public static bool isUIActive(string name)
        {
            var ui = GetUI(name);
            bool isNull = ui == null;
            bool isActive = ui && ui.gameObject.activeInHierarchy;
            return ui && ui.gameObject.activeInHierarchy;
        }

        // 关闭产地界面
        public static void CloseHomeSystemWindow()
        {
            if (!HomeSystemWindow.Instance.gameObject.activeInHierarchy) return;

            // 依次检测子窗口,顺序很重要
            // 提示ok
            if (ui_MessageWindow.Exists) return;
            // 建筑界面
            if (BuildingWindow.Exists()) return;
            // 村庄管理
            if (isUIActive("ui_VillageManage")) return;
            // 人力管理
            if (isUIActive("ui_ManPowerManage")) return;
            // 系统设置
            if (isUIActive("ui_SystemSetting")) return;
            // 时节回顾
            if (isUIActive("ui_TurnChange")) return;
            // 技能建筑
            if (HomeSystemWindow.Instance.skillView.activeInHierarchy) return;

            UIState.HomeSystem.Back();
        }

        // 打开太吾村产地
        public static void ShowHomeSystem()
        {
            UIManager.Instance.StackState();
            bool flag = DateFile.instance.gameLine >= 10;
            if (flag)
            {
                HomeSystemWindow.Instance.ShowHomeView(int.Parse(DateFile.instance.gangDate[16][3]), int.Parse(DateFile.instance.gangDate[16][4]));
            }
            else
            {
                HomeSystemWindow.Instance.ShowHomeView(9999, 40);
            }
        }

        // 打开当地产地
        public static void ShowLocalHomeSystem()
        {
            int choosePartId = ChoosePlaceWindow.Instance.choosePartId;
            int choosePlaceId = ChoosePlaceWindow.Instance.choosePlaceId;
            bool flag = choosePartId == DateFile.instance.mianPartId &&
                choosePlaceId == DateFile.instance.mianPlaceId;
            if (DateFile.instance.baseHomeDate[choosePartId].ContainsKey(choosePlaceId) && flag)
            {
                UIManager.Instance.StackState();
                HomeSystemWindow.Instance.ShowHomeView(WorldMapSystem.instance.choosePartId, WorldMapSystem.instance.choosePlaceId);
            }
        }

        public static void ButtonHK(Button button, HK_TYPE key, Func<System.Object, bool> check = null)
        {
            if (button == null) return;
            button.gameObject
                .AddComponent<ActionsComponent>()
                .With(button)
                .OnCheck(key)
                .OnCheck((b) => CheckSelectable((Button)b))
                .OnCheck(check)
                .AddAction(() => ClickButton(button));
        }

        public static void ButtonConfirm(Button button, Func<System.Object, bool> check = null)
        {
            if (button == null) return;
            button.gameObject
                .AddComponent<ActionsComponent>()
                .With(button)
                .OnCheck(CHECK_TYPE.COMFIRM)
                .OnCheck((b) => CheckSelectable((Button)b))
                .OnCheck(check)
                .AddAction(() => ClickButton(button));
        }

        public static bool canClose = true;
        public static void ButtonClose(Button button, Func<System.Object, bool> check = null)
        {
            if (button == null) return;
            button.gameObject
                .AddComponent<ActionsComponent>()
                .With(button)
                .OnCheck(CHECK_TYPE.CLOSE)
                .OnCheck((_) => canClose)
                .OnCheck((b) => CheckSelectable((Button)b))
                .OnCheck(check)
                .AddAction(() => ClickButton(button));
        }
        public static void ButtonRemove(Button button, Func<System.Object, bool> check = null)
        {
            if (button == null) return;
            button.gameObject
                .AddComponent<ActionsComponent>()
                .With(button)
                .OnCheck(CHECK_TYPE.REMOVE)
                .OnCheck((b) => CheckSelectable((Button)b))
                .OnCheck(check)
                .AddAction(() => ClickButton(button));
        }

        public static bool CheckSelectable(Selectable button)
        {
            return button != null && button.interactable && button.gameObject.activeInHierarchy;
        }
        public static void ClickButton(Button button)
        {
            button.onClick.Invoke();
        }

        public static void ToggleSwitch(ToggleGroup group)
        {
            var m_Toggles = Traverse.Create(group).Field("m_Toggles");
            group.gameObject
                .AddComponent<ActionsComponent>()
                .AddNavActions(() => {
                    var toggles = m_Toggles.GetValue<List<Toggle>>();
                    int index = toggles.FindIndex(t => t.isOn);
                    index--;
                    for (int i = 0; i < toggles.Count; i++)
                    {
                        if (index < 0) index = toggles.Count - 1;
                        if (toggles[index].interactable && toggles[index].gameObject.activeInHierarchy) break;
                        index--;
                    }
                    group.SetAllTogglesOff();
                    toggles[index].isOn = true;
                    group.NotifyToggleOn(toggles[index]);

                }, () =>
                {
                    var toggles = m_Toggles.GetValue<List<Toggle>>();
                    int index = toggles.FindIndex(t => t.isOn);
                    index++;
                    for (int i = 0; i < toggles.Count; i++)
                    {
                        if (index > toggles.Count - 1) index = 0;
                        if (toggles[index].interactable && toggles[index].gameObject.activeInHierarchy) break;
                        index++;
                    }
                    group.SetAllTogglesOff();
                    toggles[index].isOn = true;
                    group.NotifyToggleOn(toggles[index]);
                });
        }

        public static void LogToggle(Toggle t)
        {
            var m_Toggles = Traverse.Create(t.group).Field("m_Toggles").GetValue<List<Toggle>>();
            foreach (var tt in m_Toggles) { Main.Logger.Error(t.group.name + " " + tt.name + " " + tt.isOn); }
        }
    }

}
