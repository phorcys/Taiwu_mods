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
    ///  worldmap 事件
    /// </summary>
    [HarmonyPatch(typeof(WorldMapSystem), "Update")]
    public static class WorldMapSystem_Update_Patch
    {
        static MethodInfo GetMoveKey = typeof(WorldMapSystem)
            .GetMethod("GetMoveKey", BindingFlags.NonPublic | BindingFlags.Instance);

        private static bool Prefix(WorldMapSystem __instance, bool ___moveButtonDown, bool ___isShowPartWorldMap)
        {
            if (!Main.enabled || Main.binding_key)
            {
                return true;
            }

            if (DateFile.instance.battleStart == false //无战斗
                && UIDate.instance.trunChangeImage[0].gameObject.activeSelf == false //非回合结算
                && SystemSetting.instance.SystemSettingWindow.activeInHierarchy == false) // 系统设置未开启
            {
                //处理关闭                                                         
                if (YesOrNoWindow.instance.yesOrNoIsShow == true && YesOrNoWindow.instance.isActiveAndEnabled == true)
                {
                    if (Main.settings.enable_close && (Main.GetKeyDown(HK_TYPE.HK_CLOSE) == true || Input.GetMouseButtonDown(1) == true)
                        && YesOrNoWindow.instance.no.isActiveAndEnabled == true)
                    {
                        YesOrNoWindow.instance.CloseYesOrNoWindow();
                        return false;
                    }
                    if (Main.GetKeyDown(HK_TYPE.HK_COMFIRM) == true || Main.GetKeyDown(HK_TYPE.HK_CONFIRM2) == true)
                    {
                        OnClick.instance.Index();
                        YesOrNoWindow.instance.CloseYesOrNoWindow();
                        return false;
                    }

                }

                //界面快捷键  人物/世界地图/村子地图
                if (YesOrNoWindow.instance.MaskShow() == false)  //无模态对话框
                {
                    if (Main.GetKeyDown(HK_TYPE.HK_ACTORMENU) && __instance.partWorldMapWindow.activeInHierarchy == false)
                    {
                        if (ActorMenu.instance.actorMenu.activeInHierarchy == false)
                        {
                            ActorMenu.instance.ShowActorMenu(false);
                            return false;
                        }
                        else
                        {
                            ActorMenu.instance.CloseActorMenu();
                            return false;
                        }
                    }
                    if (Main.GetKeyDown(HK_TYPE.HK_VILLAGE) && __instance.partWorldMapWindow.activeInHierarchy == false)
                    {
                        if (HomeSystem.instance.homeSystem.activeInHierarchy == false)
                        {
                            HomeSystem.instance.ShowHomeSystem(true);
                        }
                        else
                        {
                            HomeSystem.instance.CloseHomeSystem();
                            return false;
                        }

                    }
                    if (Main.GetKeyDown(HK_TYPE.HK_WORLDMAP))
                    {
                        if (__instance.partWorldMapWindow.activeInHierarchy == false)
                        {
                            WorldMapSystem.instance.ShowPartWorldMapWindow(DateFile.instance.mianWorldId);
                            return false;
                        }
                        else
                        {
                            if (Main._go_gongfatree == null || Main._go_gongfatree.activeInHierarchy == false)
                            {
                                WorldMapSystem.instance.ColsePartWorldMapWindow();
                                return false;
                            }
                        }

                    }
                    if (Main.GetKeyDown(HK_TYPE.HK_VILLAGE_LOCAL))
                    {
                        if (HomeSystem.instance.homeSystem.activeInHierarchy == false)
                        {
                            HomeSystem.instance.ShowHomeSystem(false);
                        }
                        else
                        {
                            HomeSystem.instance.CloseHomeSystem();
                            return false;
                        }

                    }
                }

                //治疗和采集 奇遇
                if (__instance.partWorldMapWindow.activeInHierarchy == false  //世界地图未开启
                        && ActorMenu.instance.actorMenu.activeInHierarchy == false  //人物菜单未开启
                        && HomeSystem.instance.homeSystem.activeInHierarchy == false //村镇地图未开启
                        && BattleSystem.instance.battleWindow.activeInHierarchy == false//非战斗状态
                        && StorySystem.instance.storySystem.activeInHierarchy == false  //奇遇
                        && StorySystem.instance.toStoryMenu.activeInHierarchy == false)  //奇遇开启
                {
                    //治疗
                    if (Main.GetKeyDown(HK_TYPE.HK_HEAL)
                        && WorldMapSystem.instance.mapHealingButton[0].interactable == true)
                    {

                        WorldMapSystem.instance.MapHealing(0);
                        return false;
                    }
                    //治疗中毒
                    if (Main.GetKeyDown(HK_TYPE.HK_POISON)
                        && WorldMapSystem.instance.mapHealingButton[1].interactable == true)
                    {
                        WorldMapSystem.instance.MapHealing(1);
                        return false;

                    }
                    //采集食物
                    if (Main.GetKeyDown(HK_TYPE.HK_GATHER_FOOD) && __instance.timeWorkWindow.activeInHierarchy == false)
                    {
                        WorldMapSystem.instance.choosePartId = DateFile.instance.mianPartId;
                        WorldMapSystem.instance.choosePlaceId = DateFile.instance.mianPlaceId;
                        WorldMapSystem.instance.chooseWorkTyp = 0; // 0= 粮食
                        WorldMapSystem.instance.ChooseTimeWork();
                        return false;
                    }
                    //采集金石
                    if (Main.GetKeyDown(HK_TYPE.HK_GATHER_MINERAL) && __instance.timeWorkWindow.activeInHierarchy == false)
                    {
                        WorldMapSystem.instance.choosePartId = DateFile.instance.mianPartId;
                        WorldMapSystem.instance.choosePlaceId = DateFile.instance.mianPlaceId;
                        WorldMapSystem.instance.chooseWorkTyp = 2; // 2= 金石
                        WorldMapSystem.instance.ChooseTimeWork();
                        return false;
                    }
                    //采集药草
                    if (Main.GetKeyDown(HK_TYPE.HK_GATHER_HERB) && __instance.timeWorkWindow.activeInHierarchy == false)
                    {
                        WorldMapSystem.instance.choosePartId = DateFile.instance.mianPartId;
                        WorldMapSystem.instance.choosePlaceId = DateFile.instance.mianPlaceId;
                        WorldMapSystem.instance.chooseWorkTyp = 4; // 4= 草药
                        WorldMapSystem.instance.ChooseTimeWork();
                        return false;
                    }
                    //采集银钱
                    if (Main.GetKeyDown(HK_TYPE.HK_GATHER_MONEY) && __instance.timeWorkWindow.activeInHierarchy == false)
                    {
                        WorldMapSystem.instance.choosePartId = DateFile.instance.mianPartId;
                        WorldMapSystem.instance.choosePlaceId = DateFile.instance.mianPlaceId;
                        WorldMapSystem.instance.chooseWorkTyp = 5; // 5= 银钱
                        WorldMapSystem.instance.ChooseTimeWork();
                        return false;
                    }
                    //采集织物
                    if (Main.GetKeyDown(HK_TYPE.HK_GATHER_CLOTH) && __instance.timeWorkWindow.activeInHierarchy == false)
                    {
                        WorldMapSystem.instance.choosePartId = DateFile.instance.mianPartId;
                        WorldMapSystem.instance.choosePlaceId = DateFile.instance.mianPlaceId;
                        WorldMapSystem.instance.chooseWorkTyp = 3; // 3= 织物
                        WorldMapSystem.instance.ChooseTimeWork();
                        return false;
                    }
                    //采集木材
                    if (Main.GetKeyDown(HK_TYPE.HK_GATHER_WOOD) && __instance.timeWorkWindow.activeInHierarchy == false)
                    {
                        WorldMapSystem.instance.choosePartId = DateFile.instance.mianPartId;
                        WorldMapSystem.instance.choosePlaceId = DateFile.instance.mianPlaceId;
                        WorldMapSystem.instance.chooseWorkTyp = 1; // 1=木材
                        WorldMapSystem.instance.ChooseTimeWork();
                        return false;
                    }

                    //奇遇
                    if (Main.GetKeyDown(HK_TYPE.HK_VISITEVENT)
                        && DateFile.instance.HaveShow(DateFile.instance.mianPartId, DateFile.instance.mianPlaceId) > 0
                        && WorldMapSystem.instance.openToStoryButton.interactable == true)
                    {

                        WorldMapSystem.instance.OpenToStory();
                    }
                }
            }

            //原有Update代码修改
            if (Main.GetKeyDown(HK_TYPE.HK_COMFIRM) || Main.GetKeyDown(HK_TYPE.HK_CONFIRM2))
            {
                UIDate.instance.ChangeTrunButton();
                return false;
            }
            if (!___moveButtonDown)
            {
                if (Main.GetKey(HK_TYPE.HK_UP) || Main.GetKey(HK_TYPE.HK_UP2))
                {
                    ___moveButtonDown = true;
                    GetMoveKey.Invoke(__instance, new object[] { 1 });
                    return false;
                }
                else if (Main.GetKey(HK_TYPE.HK_LEFT) || Main.GetKey(HK_TYPE.HK_LEFT2))
                {
                    ___moveButtonDown = true;
                    GetMoveKey.Invoke(__instance, new object[] { 2 });
                    return false;
                }
                else if (Main.GetKey(HK_TYPE.HK_DOWN) || Main.GetKey(HK_TYPE.HK_DOWN2))
                {
                    ___moveButtonDown = true;
                    GetMoveKey.Invoke(__instance, new object[] { 3 });
                    return false;
                }
                else if (Main.GetKey(HK_TYPE.HK_RIGHT) || Main.GetKey(HK_TYPE.HK_RIGHT2))
                {
                    ___moveButtonDown = true;
                    GetMoveKey.Invoke(__instance, new object[] { 4 });
                    return false;
                }
            }
            return false;
        }
    }
}