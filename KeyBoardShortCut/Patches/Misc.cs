using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Reflection.Emit;
using System.Xml.Serialization;

namespace KeyBoardShortCut
{
    /// <summary>
    ///  建筑地图  关闭建筑地图界面
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "Start")]
    public static class HomeSystem_CloseHomeSystem_Patch
    {
        private static void Postfix(HomeSystem __instance)
        {
            if (!Main.enabled || Main.binding_key || !Main.settings.enable_close)
            {
                return;
            }
            EscClose newobj = __instance.gameObject.AddComponent(typeof(EscClose)) as EscClose;
            newobj.setparam(typeof(HomeSystem), "CloseHomeSystem", () =>
            {
                //依次检测子窗口,顺序很重要

                // 如果制造窗口开着，就不处理
                if (MakeSystem.instance.makeWindowBack.gameObject.activeInHierarchy == true)
                {
                    return false;
                }
                // 如果商店窗口开着，就不处理
                if (ShopSystem.instance.shopWindow.activeInHierarchy == true
                || BookShopSystem.instance.shopWindow.activeInHierarchy == true
                || SystemSetting.instance.SystemSettingWindow.activeInHierarchy == true)
                {
                    return false;
                }


                //Warehouse window
                if (Warehouse.instance.warehouseWindow.gameObject.activeInHierarchy == true)
                {
                    Warehouse.instance.CloseWarehouse();
                    return false;
                }

                //bookWindow
                if (HomeSystem.instance.bookWindow.activeInHierarchy == true)
                {
                    HomeSystem.instance.CloseBookWindow();
                    return false;
                }


                // setstudyWindow
                if (HomeSystem.instance.setStudyWindow.gameObject.activeInHierarchy == true)
                {
                    HomeSystem.instance.CloseSetStudyWindow();
                    return false;
                }

                // studywindow
                if (HomeSystem.instance.studyWindow.activeInHierarchy == true)
                {
                    HomeSystem.instance.CloseStudyWindow();
                    return false;
                }

                //working actor 
                if (HomeSystem.instance.actorListWindow.activeInHierarchy == true)
                {
                    HomeSystem.instance.CloseActorWindow();
                    return false;
                }
                //ququbox 
                if (QuquBox.instance.ququBoxWindow.activeInHierarchy == true)
                {
                    QuquBox.instance.CloseQuquBox();
                    return false;
                }
                //building window
                if (HomeSystem.instance.buildingWindow.gameObject.activeInHierarchy == true)
                {
                    HomeSystem.instance.CloseBuildingWindow();
                    return false;
                }

                return HomeSystem.instance.homeSystem.activeInHierarchy;
            });
        }
    }


    /// <summary>
    ///  关闭story/奇遇界面
    /// </summary>
    [HarmonyPatch(typeof(StorySystem), "Start")]
    public static class StorySystem_CloseHomeSystem_Patch
    {
        private static void Postfix(StorySystem __instance)
        {
            if (!Main.enabled || Main.binding_key || !Main.settings.enable_close)
            {
                return;
            }
            ConfirmConfirm newobj = __instance.gameObject.AddComponent(typeof(ConfirmConfirm)) as ConfirmConfirm;
            newobj.setparam(typeof(StorySystem), "OpenStory", () =>
            {
                //依次检测子窗口,顺序很重要

                // 如果开始奇遇没有显示，则不处理
                if (StorySystem.instance.toStoryIsShow != true)
                {
                    return false;
                }
                // 如果按钮不可以交互，不处理
                if (StorySystem.instance.openStoryButton.interactable != true)
                {
                    return false;
                }

                return StorySystem.instance.openStoryButton.interactable;
            });
        }
    }


    /// <summary>
    ///  确定 关闭 过时节
    /// </summary>
    [HarmonyPatch(typeof(UIDate), "Start")]
    public static class UIDate_CloseTurnchange_Patch
    {
        private static void Postfix(UIDate __instance)
        {
            if (!Main.enabled || Main.binding_key || !Main.settings.enable_close)
            {
                return;
            }
            ConfirmConfirm newobj = __instance.gameObject.AddComponent(typeof(ConfirmConfirm)) as ConfirmConfirm;
            newobj.setparam(typeof(UIDate), "CloseTrunChangeWindow", () =>
            {
                //依次检测子窗口,顺序很重要

                // 如果开始奇遇没有显示，则不处理
                if (UIDate.instance.trunChangeWindow.activeInHierarchy != true)
                {
                    return false;
                }

                return UIDate.instance.trunChangeWindow.activeInHierarchy;
            });
        }
    }


    /// <summary>
    ///  确定 确认建筑
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "Start")]
    public static class HomeSystem_Confirm_Patch
    {
        private static void Postfix(UIDate __instance)
        {
            if (!Main.enabled || Main.binding_key)
            {
                return;
            }
            ConfirmConfirm newobj = __instance.gameObject.AddComponent(typeof(ConfirmConfirm)) as ConfirmConfirm;
            newobj.setparam(typeof(HomeSystem), "StartNewBuilding", () =>
            {
                //依次检测子窗口,顺序很重要
                if (HomeSystem.instance.homeSystem.activeInHierarchy == false)
                {
                    return false;
                }

                if (HomeSystem.instance.buildingUPWindowBack.activeInHierarchy == true && HomeSystem.instance.buildingUpCanBuildingButton.interactable)
                {
                    HomeSystem.instance.StartBuildingUp();
                    return false;
                }
                if (HomeSystem.instance.buildingRemoveCanBuildingButton.gameObject.transform.parent.gameObject.activeInHierarchy == true
                && HomeSystem.instance.buildingRemoveCanBuildingButton.interactable)
                {
                    HomeSystem.instance.StartBuildingRemove();
                    return false;
                }
                if (HomeSystem.instance.buildingWindow.Find("NewBuildingWindowBack").gameObject.activeInHierarchy == true)
                {
                    return HomeSystem.instance.canBuildingButton.interactable;
                }
                return false;

            });
        }

    }


    /// <summary>
    ///  确定 确认 购买
    /// </summary>
    [HarmonyPatch(typeof(ShopSystem), "Start")]
    public static class ShopSystem_Confirm_Patch
    {
        private static void Postfix(UIDate __instance)
        {
            if (!Main.enabled || Main.binding_key)
            {
                return;
            }
            ConfirmConfirm newobj = __instance.gameObject.AddComponent(typeof(ConfirmConfirm)) as ConfirmConfirm;
            newobj.setparam(typeof(ShopSystem), "ShopOK", () =>
            {
                //依次检测子窗口,顺序很重要

                // 如果开始奇遇没有显示，则不处理
                if (ShopSystem.instance.shopWindow.activeInHierarchy != true)
                {
                    return false;
                }

                return ShopSystem.instance.shopOkButton.interactable;
            });
        }
    }


    /// <summary>
    ///  确定 bookshop 确认 购买
    /// </summary>
    [HarmonyPatch(typeof(BookShopSystem), "Start")]
    public static class BookShopSystem_Confirm_Patch
    {
        private static void Postfix(UIDate __instance)
        {
            if (!Main.enabled || Main.binding_key)
            {
                return;
            }
            ConfirmConfirm newobj = __instance.gameObject.AddComponent(typeof(ConfirmConfirm)) as ConfirmConfirm;
            newobj.setparam(typeof(BookShopSystem), "ShopOK", () =>
            {
                //依次检测子窗口,顺序很重要

                // 如果开始奇遇没有显示，则不处理
                if (BookShopSystem.instance.shopWindow.activeInHierarchy != true)
                {
                    return false;
                }

                return BookShopSystem.instance.shopOkButton.interactable;
            });
        }
    }
}