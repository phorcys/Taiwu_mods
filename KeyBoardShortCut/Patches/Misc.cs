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
using System.Threading.Tasks;

namespace KeyBoardShortCut
{
    // 测试
    [HarmonyPatch(typeof(ui_SystemSetting), "OnInit")]
    public static class Test_Patch
    {
        private static void Postfix(ui_SystemSetting __instance)
        {
            Main.Logger.Error("ui_SystemSetting OnInit");
            Utils.isUIActive("ui_SystemSetting");
        }
    }

    // 关闭延迟
    [HarmonyPatch(typeof(UIState), "Back")]
    public static class UIState_Close_Wait_Patch
    {
        private static void Prefix()
        {
            if (!Main.on) return;
            Wait();
        }

        public static async void Wait()
        {
            Utils.canClose = false;
            await Task.Delay(300);
            Utils.canClose = true;
        }
    }

    // 通用选择框：确认延迟
    [HarmonyPatch(typeof(YesOrNoWindow), "ShowYesOrNoWindow")]
    public static class YesOrNoWindow_Confirm_Wait_Patch
    {
        private static void Prefix(bool show, bool backMask, bool canClose)
        {
            if (!Main.on) return;
            if (show && !YesOrNoWindow.instance.yesOrNoIsShow) Wait();
        }

        public static async void Wait()
        {
            YesOrNoWindow_Confirm_Patch.canYes = false;
            await Task.Delay(300);
            YesOrNoWindow_Confirm_Patch.canYes = true;
        }
    }

    // 通用选择框：确认
    [HarmonyPatch(typeof(YesOrNoWindow), "Awake")]
    public static class YesOrNoWindow_Confirm_Patch
    {
        public static bool canYes = true;
        private static void Postfix(YesOrNoWindow __instance)
        {
            if (!Main.on) return;
            Utils.ButtonConfirm(__instance.yes, (_) => 
                canYes && 
                __instance.yesOrNoIsShow && 
                __instance.yesOrNoWindow.gameObject.activeInHierarchy
            );
        }
    }

    // 事件选择 使用物品 人物 确认
    [HarmonyPatch(typeof(ui_MessageWindow), "Awake")]
    public static class MessageWindow_Confirm_Patch
    {
        private static void Postfix(ui_MessageWindow __instance) {
            if (!Main.on) return;
            Utils.ButtonConfirm(__instance.useItemButton);
            Utils.ButtonConfirm(__instance.useActorButton);
        }
    }


    // 事件选择窗口 右键默认选项
        [HarmonyPatch(typeof(ui_MessageWindow), "Awake")]
    public static class MessageWindow_Close_Patch
    {
        private static void Postfix(ui_MessageWindow __instance)
        {
            if (!Main.on) return;
            __instance.gameObject.AddComponent<ActionsComponent>()
                .OnCheck(CHECK_TYPE.CLOSE)
                .OnCheck((_) => ui_MessageWindow.Exists)
                .OnCheck((_) => ui_MessageWindow.Instance.gameObject.activeInHierarchy)
                .OnCheck((_) => UIManager.Instance.curState != UIState.ActorMenu)
                .OnCheck((_) => Utils.canClose)
                .AddAction(() => {
                    var holder = __instance.chooseHolder;
                    var count = holder.childCount;
                    var child = holder.GetChild(count- 1);
                    var button = child.gameObject.GetComponent<Button>();
                    // 唯我选项
                    if (button.name.EndsWith("20700007")) return;
                    if (button.name.EndsWith("21000005")) return;
                    if (button.name.EndsWith("21500006")) return;
                    if (button.name.EndsWith("20300005")) return;
                    if (button.name.EndsWith("22800005")) return;
                    if (button.name.EndsWith("104600005")) return;
                    if (button.name.EndsWith("105000005")) return;
                    if (button.name.EndsWith("1700005")) return;
                    if (button.name.EndsWith("106500005")) return;
                    if (button.name.EndsWith("107600005")) return;
                    if (button.name.EndsWith("210100005")) return;
                    if (button.name.EndsWith("128200005")) return;
                    if (button.name.EndsWith("25200005")) return;
                    if (button.name.EndsWith("28000005")) return;
                    if (button.name.EndsWith("1212000005")) return;
                    if (button.name.EndsWith("000005")) return;
                        button.onClick.Invoke();
                });
        }
    }

    // 设置快捷键关闭 太吾/本地 产地   关闭技能建筑浏览
    [HarmonyPatch(typeof(HomeSystemWindow), "Awake")]
    public static class HomeSystemWindow_Awake_Patch
    {
        private static void Postfix(HomeSystemWindow __instance)
        {
            if (!Main.on) return;
             __instance.gameObject.AddComponent<ActionsComponent>()
                .OnCheck(CHECK_TYPE.CLOSE)
                .AddAction(() => {
                    if (HomeSystemWindow.Instance.skillView.activeInHierarchy)
                    {
                        var homeViewButtom = Utils.GetUI("ui_HomeViewBottom");
                        if (homeViewButtom)
                        {
                            HomeSystemWindow.Instance.ShowSkillView();
                            Traverse.Create(homeViewButtom).Method("ToggleZoomButtons").GetValue();
                        }
                    } else
                    {
                        Utils.CloseHomeSystemWindow();
                    }
                });
        }
    }

    // 设置快捷键打开 太吾/本地 产地
    [HarmonyPatch(typeof(WorldMapSystem), "Update")]
    public static class WorldMapSystem_Update_Patch
    {
        private static void Postfix(WorldMapSystem __instance)
        {
            if (!Main.enabled || Main.binding_key) return;
            if (UIManager.Instance.curState == UIState.MainWorld)
            {
                if (Main.GetKeyDown(HK_TYPE.VILLAGE_LOCAL))
                {
                    Utils.ShowLocalHomeSystem();
                }
                else if (Main.GetKeyDown(HK_TYPE.VILLAGE))
                {
                    Utils.ShowHomeSystem();
                }
            } else if (UIManager.Instance.curState == UIState.HomeSystem)
            {
                if (Main.GetKeyDown(HK_TYPE.VILLAGE_LOCAL) || Main.GetKeyDown(HK_TYPE.VILLAGE))
                {
                    Utils.CloseHomeSystemWindow();
                }
            }
        }
    }

    // 建筑界面 新建 升级 人力调整
    [HarmonyPatch(typeof(BuildingWindow), "Start")]
    public static class BuildingWindow_Up_Patch
    {
        private static void Postfix(BuildingWindow __instance)
        {
            if (!Main.enabled || Main.binding_key) return;
            Refers component = BuildingWindow.instance.GetComponent<Refers>();

            // 新建 人力调整
            Utils.ButtonHK(component.CGet<Button>("NewBuildingManpowerDownButton"), HK_TYPE.DECREASE);
            Utils.ButtonHK(component.CGet<Button>("NewBuildingManpowerUpButton"), HK_TYPE.INCREASE);

            // 新建 确定
            Utils.ButtonConfirm(component.CGet<Button>("NewBuildingButton"));

            // 升级 人力调整
            Utils.ButtonHK(component.CGet<Button>("UpBuildingManpowerDownButton"), HK_TYPE.DECREASE);
            Utils.ButtonHK(component.CGet<Button>("UpBuildingManpowerUpButton"), HK_TYPE.INCREASE);

            // 升级 确定
            Utils.ButtonConfirm(component.CGet<Button>("UpBuildingButton"));

            // 移除 人力调整
            Utils.ButtonHK(component.CGet<Button>("RemoveBuildingManpowerDownButton"), HK_TYPE.DECREASE);
            Utils.ButtonHK(component.CGet<Button>("RemoveBuildingManpowerUpButton"), HK_TYPE.INCREASE);

            // 移除 确定
            Utils.ButtonConfirm(component.CGet<Button>("RemoveBuildingButton"));
        }
    }

    // 功法书籍选择确认
    [HarmonyPatch(typeof(BuildingWindow), "Start")]
    public static class BuildingWindow_ChooseBookConfirm_Patch
    {
        private static void Postfix(BuildingWindow __instance)
        {
            if (!Main.on) return;
            // 修习， 突破
            Utils.ButtonConfirm(__instance.setGongFaButton);
            // 研读
            Utils.ButtonConfirm(__instance.chooseBookButton);
        }
    }

    // 添加 移除 功法书籍
    [HarmonyPatch(typeof(BuildingWindow), "Start")]
    public static class BuildingWindow_RemoveStudyItem_Patch
    {
        private static void Postfix(BuildingWindow __instance)
        {
            if (!Main.on) return;
            Refers component = __instance.GetComponent<Refers>();
            var studyChooseTyp = Traverse.Create(BuildingWindow.instance).Field("studyChooseTyp");
            // 修习
            Utils.ButtonConfirm(component.CGet<Button>("AddStudySkillButton"), (b) => {
                return 0 == studyChooseTyp.GetValue<int>() && 
                    !__instance.setStudyWindow.gameObject.activeInHierarchy && 
                    Traverse.Create(__instance).Field<int>("studySkillId").Value == 0;
            });
            Utils.ButtonConfirm(component.CGet<Button>("StudySkillUpButton"), (b) => {
                return 0 == studyChooseTyp.GetValue<int>() && 
                    !__instance.setStudyWindow.gameObject.activeInHierarchy;
            });
            Utils.ButtonHK(__instance.removeGongFaButton, HK_TYPE.REMOVE_ITEM, (b) => {
                return 0 == studyChooseTyp.GetValue<int>();
            });
            // 突破
            Utils.ButtonConfirm(component.CGet<Button>("AddSkillLevelUpButton"), (b) => {
                return 1 == studyChooseTyp.GetValue<int>() && 
                    !__instance.setStudyWindow.gameObject.activeInHierarchy && 
                    __instance.levelUPSkillId == 0;
            });
            Utils.ButtonConfirm(component.CGet<Button>("StartSkillLevelUpButton"), (b) => {
                return 1 == studyChooseTyp.GetValue<int>() && 
                    !__instance.setStudyWindow.gameObject.activeInHierarchy &&
                    !StudyWindow.instance.gameObject.activeInHierarchy;
            });
            Utils.ButtonHK(__instance.removeLevelUPButton, HK_TYPE.REMOVE_ITEM, (b) => {
                return 1 == studyChooseTyp.GetValue<int>();
            });
            // 研读
            Utils.ButtonConfirm(component.CGet<Button>("AddReadBookButton"), (b) => {
                return 2 == studyChooseTyp.GetValue<int>() && 
                    !__instance.bookWindow.activeInHierarchy &&
                    __instance.readBookId == 0;
            });
            Utils.ButtonConfirm(component.CGet<Button>("StartReadBookButton"), (b) => {
                return 2 == studyChooseTyp.GetValue<int>() && 
                    !__instance.bookWindow.activeInHierarchy && 
                    !ReadBook.instance.gameObject.activeInHierarchy;
            });
            Utils.ButtonHK(__instance.removeReadBookButton, HK_TYPE.REMOVE_ITEM, (b) => {
                return 2 == studyChooseTyp.GetValue<int>();
            });
        }
    }

    // 建筑 功能 菜单切换
    [HarmonyPatch(typeof(BuildingWindow), "Start")]
    public static class BuildingWindow_Toggle_Type_Patch
    {
        private static void Postfix(BuildingWindow __instance)
        {
            if (!Main.on) return;
            Utils.ToggleSwitch(__instance.buildingTypHolder);
        }
    }

    // 更新信息窗口
    [HarmonyPatch(typeof(MainMenu), "Awake")]
    public static class MainMenu_StartMessage_Confirm_Patch
    {
        private static void Postfix(MainMenu __instance)
        {
            if (!Main.on) return;
            Transform welcome = __instance.transform.Find("WelcomeDialog");
            Refers refer = welcome.GetComponent<Refers>();
            Utils.ButtonConfirm(refer.CGet<CButton>("ConfirmBtn"));
        }
    }

    // 商店确认
    [HarmonyPatch(typeof(ShopSystem), "Start")]
    public static class ShopSystem_Confirm_Patch
    {
        private static void Postfix(ShopSystem __instance)
        {
            if (!Main.on) return;
            Utils.ButtonConfirm(__instance.shopOkButton);
        }
    }


    // 过月事件窗口：确认
    [HarmonyPatch(typeof(ui_TurnChange), "Awake")]
    public static class UI_TurnChange_ConfirmTrunChangeWindow_Patch
    {
        private static void Postfix(ui_TurnChange __instance)
        {
            if (!Main.on) return;
            Utils.ButtonConfirm(Traverse.Create(__instance).Field<CButton>("closeBtn").Value);
        }
    }

    // 人物界面 打开
    [HarmonyPatch(typeof(ui_BottomLeft), "Awake")]
    public static class ActorMenu_Open_Patch
    {
        private static void Postfix(ui_BottomLeft __instance)
        {
            if (!Main.on) return;
            Utils.ButtonHK(__instance.CGet<CButton>("PlayerFaceButton"), HK_TYPE.ACTORMENU);
        }
    }

    // 人物界面 切换
    [HarmonyPatch(typeof(ActorMenu), "Awake")]
    public static class ActorMenu_Toggle_Type_Patch
    {
        private static void Postfix(ActorMenu __instance)
        {
            if (!Main.on) return;
            Utils.ToggleSwitch(__instance.actorTeamToggle.group);
        }
    }

    // 人物界面 功法确认
    [HarmonyPatch(typeof(ActorMenu), "Awake")]
    public static class ActorMenu_Gongfa_Confirm_Patch
    {
        private static void Postfix(ActorMenu __instance)
        {
            if (!Main.on) return;
            Utils.ButtonConfirm(__instance.equipGongFaViewButton.GetComponent<Button>());
            Utils.ButtonHK(__instance.removeGongFaViewButton.GetComponent<Button>(), HK_TYPE.REMOVE_ITEM);
        }
    }

    // 较艺界面：结束确认
    [HarmonyPatch(typeof(SkillBattleSystem), "Awake")]
    public static class SkillBattleSystem_ConfirmEnd_Patch
    {
        private static void Postfix(SkillBattleSystem __instance)
        {
            if (!Main.on) return;
            Utils.ButtonConfirm(__instance.closeBattleButton.GetComponent<Button>());
        }
    }


    // 制作确认得到物品
    [HarmonyPatch(typeof(MakeSystem), "Awake")]
    public static class MakeSystem_ConfirmEnd_Patch
    {
        private static void Postfix(MakeSystem __instance)
        {
            if (!Main.on) return;
            Refers component = __instance.GetComponent<Refers>();
            Utils.ButtonConfirm(component.CGet<Button>("StartMakeButton"), (_) => !Checks.HasDialog() && !__instance.makeingImage.activeInHierarchy);
            Utils.ButtonConfirm(component.CGet<Button>("StartFixButton"));
            Utils.ButtonConfirm(component.CGet<Button>("GetItemButton"));
        }
    }

    // 战斗界面：结束确认
    [HarmonyPatch(typeof(BattleEndWindow), "Awake")]
    public static class BattleEndWindow_ConfirmEnd_Patch
    {
        private static void Postfix(BattleEndWindow __instance)
        {
            if (!Main.on) return;
            Utils.ButtonConfirm(__instance.closeBattleEndWindowButton);
        }
    }

    // 书籍 购买确认
    [HarmonyPatch(typeof(BookShopSystem), "Start")]
    public static class BookShopSystem_Ok_Patch
    {
        private static void Postfix(BookShopSystem __instance)
        {
            if (!Main.on) return;
            Refers component = __instance.GetComponent<Refers>();
            Utils.ButtonConfirm(component.CGet<Button>("ShopOkButton"));
        }
    }

    // 进入奇遇
    [HarmonyPatch(typeof(ChoosePlaceWindow), "Awake")]
    public static class ChoosePlaceWindow_ToStory_Patch
    {
        private static void Postfix(ChoosePlaceWindow __instance)
        {
            if (!Main.on) return;
            Utils.ButtonHK(__instance.openToStoryButton, HK_TYPE.STORY);
        }
    }

    // 进入主界面功法树
    [HarmonyPatch(typeof(ChoosePlaceWindow), "Awake")]
    public static class ChoosePlaceWindow_ToGongfaTree_Patch
    {
        private static void Postfix(ChoosePlaceWindow __instance)
        {
            if (!Main.on) return;
            Utils.ButtonHK(__instance.showGongFaTreeButton, HK_TYPE.GONGFA_TREE, (_) => !Utils.isUIActive("ui_PartWorldMap"));
        }
    }

    // 进入人物搜索
    [HarmonyPatch(typeof(ui_MiniMap), "Awake")]
    public static class ui_MiniMap_ToNameScan_Patch
    {
        private static void Postfix(ui_MiniMap __instance)
        {
            if (!Main.on) return;
            Utils.ButtonHK(Traverse.Create(__instance).Field<CButton>("SearchNpc").Value, HK_TYPE.NAME_SCAN);
        }
    }

    // 进入世界地图
    [HarmonyPatch(typeof(ui_MiniMap), "Awake")]
    public static class ui_MiniMap_ToWorldMap_Patch
    {
        private static void Postfix(ui_MiniMap __instance)
        {
            if (!Main.on) return;
            Utils.ButtonHK(Traverse.Create(__instance).Field<CButton>("ShowMap").Value, HK_TYPE.WORLD_MAP);
        }
    }

    // 进入功法树
    [HarmonyPatch(typeof(ui_PartWorldMap), "Awake")]
    public static class ui_PartWorldMap_ToGongFaTree_Patch
    {
        private static void Postfix(ui_PartWorldMap __instance)
        {
            if (!Main.on) return;
            Utils.ButtonHK(Traverse.Create(__instance).Field<CButton>("ShowGongFaTree").Value, HK_TYPE.GONGFA_TREE);
        }
    }

    // 奇遇前选择菜单
    [HarmonyPatch(typeof(ToStoryMenu), "Awake")]
    public static class ToStoryMenu_Comfirm_Patch
    {
        private static void Postfix(ToStoryMenu __instance)
        {
            if (!Main.on) return;
            // 选择物品
            Utils.ButtonConfirm(__instance.useItemButton, (_) => ToStoryMenu.toStoryIsShow);
            // 进入奇遇
            Utils.ButtonConfirm(__instance.openStoryButton, (_) => ToStoryMenu.toStoryIsShow);
            // 移除物品
            Utils.ButtonRemove(__instance.removeItemButton, (_) => ToStoryMenu.toStoryIsShow);
        }
    }
    
    // 修复 奇遇菜单中可以过月
    [HarmonyPatch(typeof(UIDate), "ChangeTrunButton")]
    public static class UIDate_Month_Change_Fix_Patch
    {
        private static bool Prefix(UIDate __instance)
        {
            if (!Main.on) return true;
            if (ToStoryMenu.toStoryIsShow) return false;
            if (ShopSystem.Exists) return false;
            if (YesOrNoWindow.instance.yesOrNoIsShow) return false;
            if (Utils.isUIActive("ui_Dialog")) return false;
            return true;
        }
    }    
    // 蛐蛐战斗 选择蛐蛐 蛐蛐结束
    [HarmonyPatch(typeof(QuquBattleSystem), "Start")]
    public static class QuquBattleSystem_UseQUQU_Patch
    {
        private static void Postfix(QuquBattleSystem __instance)
        {
            if (!Main.on) return;
            Utils.ButtonConfirm(__instance.useItemButton);
            foreach(var b in __instance.nextButton)
            {
                Utils.ButtonConfirm(b);
            }
            Utils.ButtonConfirm(__instance.closeBattleButton.GetComponent<Button>());
        }
    }


    // 地图继续移动
    [HarmonyPatch(typeof(WorldMapSystem), "ShowChoosePlaceMenu")]
    public static class WorldMapSystem_ShowChoosePlaceMenu_Patch
    {
        private static void Prefix(int worldId, int partId, int placeId, Transform placeImage)
        {
            if (!Main.on) return;
            WorldMapSystem_KeepMove_Patch.worldId = worldId;
            WorldMapSystem_KeepMove_Patch.partId = partId;
            WorldMapSystem_KeepMove_Patch.placeId = placeId;
            WorldMapSystem_KeepMove_Patch.placeImage = placeImage;
        }
    }

    // 地图继续移动
    [HarmonyPatch(typeof(WorldMapSystem), "Awake")]
    public static class WorldMapSystem_KeepMove_Patch
    {
        public static int worldId;
        public static int partId;
        public static int placeId;
        public static Transform placeImage;
        
        private static void Postfix(WorldMapSystem __instance)
        {
            if (!Main.on) return;
            __instance.gameObject
                .AddComponent<ActionsComponent>()
                .OnCheck(HK_TYPE.MAP_MOVE)
                .OnCheck(_ => DateFile.instance.mianWorldId == worldId)
                .OnCheck(_ => DateFile.instance.mianPartId == partId)
                .OnCheck(_ => __instance.worldMapPlaces[placeId] != null)
                .OnCheck(_ => __instance.worldMapPlaces[placeId].gameObject.activeInHierarchy)
                .AddAction(() => __instance.ShowChoosePlaceMenu(worldId, partId, placeId, placeImage));
        }
    }
}
