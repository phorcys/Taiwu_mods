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
    /// 通用选择框：确认
    /// </summary>
    [HarmonyPatch(typeof(YesOrNoWindow), "Start")]
    public static class YesOrNoWindow_Confirm_Patch
    {
        private static void Postfix(YesOrNoWindow __instance)
        {
            if (!Main.enabled || Main.binding_key) return;

            // 为通用选择框设置正确的初始状态，防止出现 bug
            OnClick.instance.Over = true;

            var confirmComp = __instance.yesOrNoWindow.gameObject.AddComponent<ConfirmComponent>();
            confirmComp.SetActionOnConfirm(() =>
            {
                if (!YesOrNoWindow.instance.yesOrNoWindow.gameObject.activeInHierarchy) return;
                if (OnClick.instance.Over) return;
                DateFile.instance.PlayeSE(2);
                OnClick.instance.Index();
                YesOrNoWindow.instance.CloseYesOrNoWindow();
            });
        }
    }


    /// <summary>
    /// 产业地图：关闭
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "Start")]
    public static class HomeSystem_CloseHomeSystem_Patch
    {
        private static void Postfix(HomeSystem __instance)
        {
            if (!Main.enabled || Main.binding_key || !Main.settings.enable_close) return;

            var comp = __instance.homeSystem.AddComponent<CloseComponent>();
            comp.SetActionOnClose(() =>
            {
                if (!HomeSystem.instance.homeSystem.activeInHierarchy) return;

                // 依次检测子窗口,顺序很重要
                // 制造界面
                if (MakeSystem.instance.makeWindowBack.gameObject.activeInHierarchy) return;
                // 商店界面
                if (ShopSystem.instance.shopWindow.activeInHierarchy) return;
                // 交换藏书界面
                if (BookShopSystem.instance.shopWindow.activeInHierarchy) return;
                // 配置界面
                if (SystemSetting.instance.SystemSettingWindow.activeInHierarchy) return;
                // 仓库界面
                if (Warehouse.instance.warehouseWindow.activeInHierarchy) return;
                // bookWindow
                if (HomeSystem.instance.bookWindow.activeInHierarchy) return;
                // setStudyWindow
                if (HomeSystem.instance.setStudyWindow.gameObject.activeInHierarchy) return;
                // studyWindow
                if (HomeSystem.instance.studyWindow.activeInHierarchy) return;
                // 角色列表界面
                if (HomeSystem.instance.actorListWindow.activeInHierarchy) return;
                // 蛐蛐盒界面
                if (QuquBox.instance.ququBoxWindow.activeInHierarchy) return;
                // 建筑界面
                if (HomeSystem.instance.buildingWindow.gameObject.activeInHierarchy) return;

                DateFile.instance.PlayeSE(3);
                HomeSystem.instance.CloseHomeSystem();
            });
        }
    }


    /// <summary>
    /// 产业地图 - 派遣列表：确认
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "Start")]
    public static class HomeSystem_ConfirmActorListWindow_Patch
    {
        private static void Postfix(HomeSystem __instance)
        {
            if (!Main.enabled || Main.binding_key) return;

            var comp = __instance.actorListWindow.AddComponent<ConfirmComponent>();
            comp.SetActionOnConfirm(() =>
            {
                if (!HomeSystem.instance.actorListWindow.activeInHierarchy) return;
                if (!HomeSystem.instance.canChanageActorButton.interactable) return;
                DateFile.instance.PlayeSE(2);
                HomeSystem.instance.ChanageWorkingAcotr();
            });
        }
    }


    /// <summary>
    /// 奇遇进入界面：确认
    /// </summary>
    [HarmonyPatch(typeof(StorySystem), "Start")]
    public static class StorySystem_ConfirmToStoryMenu_Patch
    {
        private static void Postfix(StorySystem __instance)
        {
            if (!Main.enabled || Main.binding_key) return;

            var comp = __instance.toStoryMenu.AddComponent<ConfirmComponent>();
            comp.SetActionOnConfirm(() =>
            {
                if (!StorySystem.instance.toStoryMenu.activeInHierarchy) return;
                if (!StorySystem.instance.toStoryIsShow) return;
                if (!StorySystem.instance.openStoryButton.interactable) return;
                DateFile.instance.PlayeSE(2);
                StorySystem.instance.OpenStory();
            });
        }
    }


    /// <summary>
    /// 过月事件窗口：确认
    /// </summary>
    [HarmonyPatch(typeof(UIDate), "Start")]
    public static class UIDate_ConfirmTrunChangeWindow_Patch
    {
        private static void Postfix(UIDate __instance)
        {
            if (!Main.enabled || Main.binding_key) return;

            var comp = __instance.trunChangeWindow.AddComponent<ConfirmComponent>();
            comp.SetActionOnConfirm(() =>
            {
                if (!UIDate.instance.trunChangeWindow.activeInHierarchy) return;
                DateFile.instance.PlayeSE(2);
                UIDate.instance.CloseTrunChangeWindow();
            });
        }
    }


    /// <summary>
    /// 建筑新建、增筑、拆除界面：确认
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "Start")]
    public static class HomeSystem_ConfirmBuilding_Patch
    {
        private static void Postfix(HomeSystem __instance)
        {
            if (!Main.enabled || Main.binding_key) return;

            var comp = __instance.newBuildingWindowBack.AddComponent<ConfirmComponent>();
            comp.SetActionOnConfirm(() =>
            {
                if (!HomeSystem.instance.newBuildingWindowBack.activeInHierarchy) return;
                if (!HomeSystem.instance.canBuildingButton.interactable) return;
                DateFile.instance.PlayeSE(2);
                HomeSystem.instance.StartNewBuilding();
            });

            comp = __instance.buildingUPWindowBack.AddComponent<ConfirmComponent>();
            comp.SetActionOnConfirm(() =>
            {
                if (!HomeSystem.instance.buildingUPWindowBack.activeInHierarchy) return;
                if (!HomeSystem.instance.buildingUpCanBuildingButton.interactable) return;
                DateFile.instance.PlayeSE(2);
                HomeSystem.instance.StartBuildingUp();
            });

            comp = __instance.removeBuildingWindowBack.AddComponent<ConfirmComponent>();
            comp.SetActionOnConfirm(() =>
            {
                if (!HomeSystem.instance.removeBuildingWindowBack.activeInHierarchy) return;
                if (!HomeSystem.instance.buildingRemoveCanBuildingButton.interactable) return;
                DateFile.instance.PlayeSE(2);
                HomeSystem.instance.StartBuildingRemove();
            });
        }
    }


    /// <summary>
    /// 商店界面：确认
    /// </summary>
    [HarmonyPatch(typeof(ShopSystem), "Start")]
    public static class ShopSystem_ConfirmShopWindow_Patch
    {
        private static void Postfix(ShopSystem __instance)
        {
            if (!Main.enabled || Main.binding_key) return;

            var comp = __instance.shopWindow.AddComponent<ConfirmComponent>();
            comp.SetActionOnConfirm(() =>
            {
                if (!ShopSystem.instance.shopWindow.activeInHierarchy) return;
                if (!ShopSystem.instance.shopOkButton.interactable) return;
                DateFile.instance.PlayeSE(2);
                ShopSystem.instance.ShopOK();
            });
        }
    }


    /// <summary>
    /// 交换藏书界面：确认
    /// </summary>
    [HarmonyPatch(typeof(BookShopSystem), "Start")]
    public static class BookShopSystem_ConfirmBookShopWindow_Patch
    {
        private static void Postfix(BookShopSystem __instance)
        {
            if (!Main.enabled || Main.binding_key) return;

            var comp = __instance.shopWindow.AddComponent<ConfirmComponent>();
            comp.SetActionOnConfirm(() =>
            {
                if (!BookShopSystem.instance.shopWindow.activeInHierarchy) return;
                if (!BookShopSystem.instance.shopOkButton.interactable) return;
                DateFile.instance.PlayeSE(2);
                BookShopSystem.instance.ShopOK();
            });
        }
    }


    /// <summary>
    /// 较艺界面：结束确认
    /// </summary>
    [HarmonyPatch(typeof(SkillBattleSystem), "Start")]
    public static class SkillBattleSystem_ConfirmEnd_Patch
    {
        private static void Postfix(SkillBattleSystem __instance)
        {
            if (!Main.enabled || Main.binding_key) return;

            var comp = __instance.battleEndWindow.AddComponent<ConfirmComponent>();
            comp.SetActionOnConfirm(() =>
            {
                if (!SkillBattleSystem.instance.battleEndWindow.activeInHierarchy) return;
                if (!SkillBattleSystem.instance.closeBattleButton.activeInHierarchy) return;
                DateFile.instance.PlayeSE(2);
                SkillBattleSystem.instance.CloseSkillBattleWindow();
            });
        }
    }
}
