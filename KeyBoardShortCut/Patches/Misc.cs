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
    /// 产业地图：快捷键关闭
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

                HomeSystem.instance.CloseHomeSystem();
            });
        }
    }


    /// <summary>
    /// 产业地图 - 派遣列表：快捷键确认
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
                HomeSystem.instance.ChanageWorkingAcotr();
            });
        }
    }


    /// <summary>
    /// 奇遇进入界面：快捷键确认
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
                StorySystem.instance.OpenStory();
            });
        }
    }


    /// <summary>
    /// 过月事件窗口：快捷键确认
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
                UIDate.instance.CloseTrunChangeWindow();
            });
        }
    }


    /// <summary>
    /// 建筑新建、增筑、拆除界面：快捷键确认
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "Start")]
    public static class HomeSystem_ConfirmBuild_Patch
    {
        private static void Postfix(HomeSystem __instance)
        {
            if (!Main.enabled || Main.binding_key) return;

            var comp = __instance.newBuildingWindowBack.AddComponent<ConfirmComponent>();
            comp.SetActionOnConfirm(() =>
            {
                if (!HomeSystem.instance.newBuildingWindowBack.activeInHierarchy) return;
                if (!HomeSystem.instance.canBuildingButton.interactable) return;
                HomeSystem.instance.StartNewBuilding();
            });

            comp = __instance.buildingUPWindowBack.AddComponent<ConfirmComponent>();
            comp.SetActionOnConfirm(() =>
            {
                if (!HomeSystem.instance.buildingUPWindowBack.activeInHierarchy) return;
                if (!HomeSystem.instance.buildingUpCanBuildingButton.interactable) return;
                HomeSystem.instance.StartBuildingUp();
            });

            comp = __instance.removeBuildingWindowBack.AddComponent<ConfirmComponent>();
            comp.SetActionOnConfirm(() =>
            {
                if (!HomeSystem.instance.removeBuildingWindowBack.activeInHierarchy) return;
                if (!HomeSystem.instance.buildingRemoveCanBuildingButton.interactable) return;
                HomeSystem.instance.StartBuildingRemove();
            });
        }
    }


    /// <summary>
    /// 商店界面：快捷键确认
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
                ShopSystem.instance.ShopOK();
            });
        }
    }


    /// <summary>
    /// 交换藏书界面：快捷键确认
    /// </summary>
    [HarmonyPatch(typeof(BookShopSystem), "Start")]
    public static class BookShopSystem_ConfirmShopWindow_Patch
    {
        private static void Postfix(BookShopSystem __instance)
        {
            if (!Main.enabled || Main.binding_key) return;

            var comp = __instance.shopWindow.AddComponent<ConfirmComponent>();
            comp.SetActionOnConfirm(() =>
            {
                if (!BookShopSystem.instance.shopWindow.activeInHierarchy) return;
                if (!BookShopSystem.instance.shopOkButton.interactable) return;
                BookShopSystem.instance.ShopOK();
            });
        }
    }
}
