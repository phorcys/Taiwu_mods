using Harmony12;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityModManagerNet;

namespace Majordomo
{
    public class Settings : UnityModManager.ModSettings
    {
        // 管家界面
        public int messageImportanceThreshold = Message.IMPORTANCE_LOWEST;    // 消息过滤等级

        // 自动收获
        public bool autoHarvestItems = true;            // 自动收获物品
        public bool autoHarvestActors = true;           // 自动接纳新村民
        public bool showNewActorWindow = true;          // 接纳新村民时显示人物窗口
        public bool filterNewActorGender = false;       // 过滤新村民性别
        public int newActorGenderFilterAccept = 2;      // 保留性别 (1: 男, 2: 女)
        public bool filterNewActorCharm = false;        // 过滤新村民魅力
        public int newActorCharmFilterThreshold = 500;
        public bool filterNewActorGoodness = false;     // 过滤新村民立场
        public bool[] newActorGoodnessFilters = new bool[] { true, true, true, true, true };    // 0: 中庸, 1: 仁善, 2: 刚正, 3: 叛逆, 4: 唯我
        public bool filterNewActorAttr = false;         // 过滤新村民资质
        public int newActorAttrFilterThreshold = 100;

        // 资源维护
        public int resMinHolding = 3;                   // 资源保有量警戒值（每月消耗量的倍数）
        public int[] resIdealHolding = null;            // 期望资源保有量
        public float resInitIdealHoldingRatio = 0.8f;   // 期望资源保有量的初始值（占当前最大值的比例）
        public int moneyMinHolding = 10000;             // 银钱最低保有量（高于此值管家可花费银钱进行采购）

        // 人员指派
        public bool autoAssignBuildingWorkers = true;   // 自动指派建筑工作人员
        // 建筑类型优先级因子
        public SerializableDictionary<BuildingType, float> buildingTypePriorityFactors =
            new SerializableDictionary<BuildingType, float>()
            {
                [BuildingType.Bedroom] = 1.00f,
                [BuildingType.Hospital] = 1.50f,
                [BuildingType.Recruitment] = 1.25f,
                [BuildingType.GettingResource] = 1.00f,
                [BuildingType.GettingItem] = 0.75f,
                [BuildingType.GettingCricket] = 0.50f,
                [BuildingType.Unknown] = 0.25f,
            };
        // 建筑排除列表
        // partId -> {placeId -> {buildingIndex,}}
        public SerializableDictionary<int, SerializableDictionary<int, HashSet<int>>> excludedBuildings =
            new SerializableDictionary<int, SerializableDictionary<int, HashSet<int>>>();
        // 建筑排除操作鼠标键位, 0: 右键, 1: 中键
        public int exclusionMouseButton = 1;
        public static readonly string[] EXCLUSION_MOUSE_BUTTONS = { "右键", "中键" };


        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }


    public static class Main
    {
        public static bool enabled = true;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static string resBasePath;
        public static GetSpritesInfoAsset getSpritesInfoAsset;
        public const string MOD_ID = "Majordomo";

        private static readonly Dictionary<string, FloatField> floatFields = new Dictionary<string, FloatField>();

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            Main.Logger = modEntry.Logger;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Main.settings = Settings.Load<Settings>(modEntry);

            resBasePath = System.IO.Path.Combine(modEntry.Path, "resources");

            var dynamicSetSprite = SingletonObject.getInstance<DynamicSetSprite>();
            getSpritesInfoAsset = (GetSpritesInfoAsset) Traverse.Create(dynamicSetSprite).Field("gsInfoAsset").GetValue();

            modEntry.OnToggle = Main.OnToggle;
            modEntry.OnGUI = Main.OnGUI;
            modEntry.OnSaveGUI = Main.OnSaveGUI;

            return true;
        }


        /// <summary>
        /// 由于一些操作必须在游戏初始化时进行，所以关闭之后再打开 mod，一些初始化动作可能尚未进行，
        /// 因而设定成在此状态下，必须重启才能再次打开此 mod。
        /// </summary>
        /// <param name="modEntry"></param>
        /// <param name="enable"></param>
        /// <returns></returns>
        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool enable)
        {
            if (!Main.enabled && enable) return false;

            Main.enabled = enable;
            return true;
        }


        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            // 管家界面 --------------------------------------------------------
            GUILayout.BeginHorizontal();
            GUILayout.Label("<color=#87CEEB>管家界面</color>");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("消息过滤：只显示重要度不低于");
            Main.settings.messageImportanceThreshold = GUILayout.SelectionGrid(
                Mathf.Clamp(Main.settings.messageImportanceThreshold, Message.IMPORTANCE_LOWEST, Message.IMPORTANCE_HIGHEST) / 25,
                Message.MESSAGE_IMPORTANCE_NAMES, Message.MESSAGE_IMPORTANCE_NAMES.Length) * 25;
            GUILayout.Label("的消息");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // 自动收获 --------------------------------------------------------
            GUILayout.BeginHorizontal();
            GUILayout.Label("\n<color=#87CEEB>自动收获</color>");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.settings.autoHarvestItems = GUILayout.Toggle(Main.settings.autoHarvestItems, "自动收获物品", GUILayout.Width(120));
            Main.settings.autoHarvestActors = GUILayout.Toggle(Main.settings.autoHarvestActors, "自动接纳新村民", GUILayout.Width(120));
            Main.settings.showNewActorWindow = GUILayout.Toggle(Main.settings.showNewActorWindow, "接纳新村民时显示人物窗口", GUILayout.Width(200));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.settings.filterNewActorGender = GUILayout.Toggle(Main.settings.filterNewActorGender, "过滤新村民性别", GUILayout.Width(120));
            GUILayout.Label("保留：");
            Main.settings.newActorGenderFilterAccept = GUILayout.SelectionGrid(Main.settings.newActorGenderFilterAccept - 1, new string[] { "男性", "女性" }, 2) + 1;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.settings.filterNewActorCharm = GUILayout.Toggle(Main.settings.filterNewActorCharm, "过滤新村民魅力", GUILayout.Width(120));
            GUILayout.Label("保留魅力不低于");
            var newActorCharmFilterThreshold = GUILayout.TextField(Main.settings.newActorCharmFilterThreshold.ToString(), 3, GUILayout.Width(40));
            if (GUI.changed && !int.TryParse(newActorCharmFilterThreshold, out Main.settings.newActorCharmFilterThreshold))
            {
                Main.settings.newActorCharmFilterThreshold = 500;
            }
            GUILayout.Label("的新村民（0 ~ 900）");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.settings.filterNewActorGoodness = GUILayout.Toggle(Main.settings.filterNewActorGoodness, "过滤新村民立场", GUILayout.Width(120));
            GUILayout.Label("保留：");
            Main.settings.newActorGoodnessFilters[2] = GUILayout.Toggle(Main.settings.newActorGoodnessFilters[2], "刚正", GUILayout.Width(40));
            Main.settings.newActorGoodnessFilters[1] = GUILayout.Toggle(Main.settings.newActorGoodnessFilters[1], "仁善", GUILayout.Width(40));
            Main.settings.newActorGoodnessFilters[0] = GUILayout.Toggle(Main.settings.newActorGoodnessFilters[0], "中庸", GUILayout.Width(40));
            Main.settings.newActorGoodnessFilters[3] = GUILayout.Toggle(Main.settings.newActorGoodnessFilters[3], "叛逆", GUILayout.Width(40));
            Main.settings.newActorGoodnessFilters[4] = GUILayout.Toggle(Main.settings.newActorGoodnessFilters[4], "唯我", GUILayout.Width(40));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.settings.filterNewActorAttr = GUILayout.Toggle(Main.settings.filterNewActorAttr, "过滤新村民资质", GUILayout.Width(120));
            GUILayout.Label("保留任意原始资质不低于");
            var newActorAttrFilterThreshold = GUILayout.TextField(Main.settings.newActorAttrFilterThreshold.ToString(), 3, GUILayout.Width(40));
            if (GUI.changed && !int.TryParse(newActorAttrFilterThreshold, out Main.settings.newActorAttrFilterThreshold))
            {
                Main.settings.newActorAttrFilterThreshold = 100;
            }
            GUILayout.Label("的新村民");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // 资源维护 --------------------------------------------------------
            GUILayout.BeginHorizontal();
            GUILayout.Label("\n<color=#87CEEB>资源维护</color>");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("资源保有量警戒值：每月消耗量的");
            var resMinHolding = GUILayout.TextField(Main.settings.resMinHolding.ToString(), 4, GUILayout.Width(45));
            if (GUI.changed && !int.TryParse(resMinHolding, out Main.settings.resMinHolding))
            {
                Main.settings.resMinHolding = 3;
            }
            GUILayout.Label("倍，低于此值管家会进行提醒");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("银钱最低保有量：");
            var moneyMinHolding = GUILayout.TextField(Main.settings.moneyMinHolding.ToString(), 9, GUILayout.Width(85));
            if (GUI.changed && !int.TryParse(moneyMinHolding, out Main.settings.moneyMinHolding))
            {
                Main.settings.moneyMinHolding = 10000;
            }
            GUILayout.Label("，高于此值管家可花费银钱进行采购");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // 人员指派 --------------------------------------------------------
            GUILayout.BeginHorizontal();
            GUILayout.Label("\n<color=#87CEEB>人员指派</color>");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.settings.autoAssignBuildingWorkers = GUILayout.Toggle(Main.settings.autoAssignBuildingWorkers,
                "自动指派建筑工作人员", GUILayout.Width(120));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("建筑类型优先级因子：");
            ShowBuildingTypePriorityFactor("医疗类", BuildingType.Hospital, Main.settings.buildingTypePriorityFactors[BuildingType.Hospital]);
            ShowBuildingTypePriorityFactor("招募类", BuildingType.Recruitment, Main.settings.buildingTypePriorityFactors[BuildingType.Recruitment]);
            ShowBuildingTypePriorityFactor("资源类", BuildingType.GettingResource, Main.settings.buildingTypePriorityFactors[BuildingType.GettingResource]);
            ShowBuildingTypePriorityFactor("物品类", BuildingType.GettingItem, Main.settings.buildingTypePriorityFactors[BuildingType.GettingItem]);
            ShowBuildingTypePriorityFactor("蛐蛐类", BuildingType.GettingCricket, Main.settings.buildingTypePriorityFactors[BuildingType.GettingCricket]);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("排除建筑快捷键：Alt + 鼠标");
            Main.settings.exclusionMouseButton = GUILayout.SelectionGrid(Main.settings.exclusionMouseButton,
                Settings.EXCLUSION_MOUSE_BUTTONS, Settings.EXCLUSION_MOUSE_BUTTONS.Length);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }


        public static void ShowBuildingTypePriorityFactor(string label, BuildingType buildingType, float defaultValue)
        {
            GUILayout.Space(10);
            GUILayout.Label(label);

            if (!Main.floatFields.ContainsKey(label))
                Main.floatFields[label] = new FloatField(defaultValue);

            float factor = Main.floatFields[label].GetFloat(4, GUILayout.Width(40));
            if (!GUI.changed) return;

            Main.settings.buildingTypePriorityFactors[buildingType] = factor;
        }


        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Main.settings.Save(modEntry);
        }
    }


    public class TurnEvent
    {
        public const string IMAGE_NAME = "TrunEventImage_majordomo";

        // 太吾管家过月事件 ID
        public static int eventId = -1;


        /// <summary>
        /// 检查资源是否存在，若不存在则创建并注册
        /// </summary>
        /// <returns></returns>
        public static void TryRegisterResources()
        {
            if (TurnEvent.IsResourcesRegistered()) return;

            string eventImagePath = Path.Combine(Path.Combine(Main.resBasePath, "Texture"), $"{TurnEvent.IMAGE_NAME}.png");
            bool isSuccess = ResourceLoader.AppendSprite("trunEventImage", eventImagePath);
            if (!isSuccess) throw new Exception($"Failed to append sprite: {eventImagePath}");

            TurnEvent.eventId = ResourceLoader.AppendRow(DateFile.instance.trunEventDate,
                new Dictionary<int, string>
                {
                    [0] = "太吾管家",
                    [1] = "0",
                    [2] = "0",
                    [98] = "${" + TurnEvent.IMAGE_NAME + "}",
                    [99] = "您的管家禀告了如下收获：",
                });

            UnityEngine.Debug.Log("Resources of TurnEvent registered.");
        }


        /// <summary>
        /// 检查太吾管家事件相关资源是否已注册
        /// </summary>
        /// <returns></returns>
        private static bool IsResourcesRegistered()
        {
            if (TurnEvent.eventId < 0) return false;

            if (!DateFile.instance.trunEventDate.ContainsKey(TurnEvent.eventId)) return false;

            var data = DateFile.instance.trunEventDate[TurnEvent.eventId];
            int spriteId = int.Parse(data[98]);

            if (Main.getSpritesInfoAsset.trunEventImage.Length <= spriteId) return false;

            var spriteName = Main.getSpritesInfoAsset.trunEventImage[spriteId];
            if (spriteName != TurnEvent.IMAGE_NAME) return false;

            return true;
        }


        /// <summary>
        /// 往当前过月事件列表中添加太吾管家过月事件
        /// changTrunEvent format: [turnEventId, param1, param2, ...]
        /// current changTrunEvent: [TurnEvent.EVENT_ID]
        /// current GameObject.name: "TrunEventIcon,{TurnEvent.EVENT_ID}"
        /// </summary>
        /// <param name="__instance"></param>
        public static void AddEvent(UIDate __instance)
        {
            __instance.changTrunEvents.Add(new int[] { TurnEvent.eventId });
        }


        /// <summary>
        /// 设置过月事件文字
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="on"></param>
        /// <param name="tips"></param>
        public static void SetEventText(WindowManage __instance, bool on, GameObject tips)
        {
            if (tips == null || !on) return;
            if (tips.tag != "TrunEventIcon") return;

            string[] eventParams = tips.name.Split(',');
            int eventId = (eventParams.Length > 1) ? int.Parse(eventParams[1]) : 0;

            if (eventId != TurnEvent.eventId) return;

            __instance.informationName.text = DateFile.instance.trunEventDate[eventId][0];

            __instance.informationMassage.text = "您的管家向您禀报：\n" + AutoHarvest.GetBootiesSummary();

            if (!string.IsNullOrEmpty(ResourceMaintainer.shoppingRecord))
            {
                __instance.informationMassage.text += "\n" + ResourceMaintainer.shoppingRecord;
            }

            if (!string.IsNullOrEmpty(ResourceMaintainer.resourceWarning))
            {
                __instance.informationMassage.text += "\n" + ResourceMaintainer.resourceWarning;
            }
        }
    }


    /// <summary>
    /// Patch: 注册过月事件资源（在其他 mod 之后注册）
    /// </summary>
    [HarmonyPatch(typeof(EnterGame), "EnterWorld")]
    [HarmonyPriority(Priority.Last)]
    public static class EnterGame_EnterWorld_RegisterTurnEvent
    {
        static void Postfix()
        {
            if (!Main.enabled) return;

            TurnEvent.TryRegisterResources();
        }
    }


    /// <summary>
    /// Patch: 保存过月事件
    /// </summary>
    [HarmonyPatch(typeof(UIDate), "SaveTurnChangeEvent")]
    public static class UIDate_SaveTurnChangeEvent_OnChangeTurn
    {
        private static bool Prefix(UIDate __instance)
        {
            if (!Main.enabled) return true;

            AutoHarvest.GetAllBooties();

            ResourceMaintainer.TryBuyingResources();

            ResourceMaintainer.UpdateResourceWarning();

            TurnEvent.AddEvent(__instance);

            if (Main.settings.autoAssignBuildingWorkers) HumanResource.AssignBuildingWorkersForTaiwuVillage();

            return true;
        }
    }


    /// <summary>
    /// Patch: 设置浮窗文字
    /// </summary>
    [HarmonyPatch(typeof(WindowManage), "WindowSwitch")]
    public static class WindowManage_WindowSwitch_SetFloatWindowText
    {
        static void Postfix(WindowManage __instance, bool on, GameObject tips)
        {
            if (!Main.enabled) return;

            TurnEvent.SetEventText(__instance, on, tips);
        }
    }


    /// <summary>
    /// Patch: 新建存档时初始化数据
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "NewDate")]
    public static class DateFile_NewDate_InitData
    {
        static void Postfix()
        {
            if (!Main.enabled) return;

            if (MajordomoWindow.instance == null) MajordomoWindow.instance = new MajordomoWindow();
            MajordomoWindow.instance.InitData();
        }
    }


    /// <summary>
    /// Patch: 载入存档时载入已保存数据
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "LoadDate")]
    public static class DateFile_LoadDate_LoadSavedData
    {
        static void Postfix()
        {
            if (!Main.enabled) return;

            if (MajordomoWindow.instance == null) MajordomoWindow.instance = new MajordomoWindow();
            MajordomoWindow.instance.LoadSavedData();
        }
    }


    /// <summary>
    /// Patch: 保存存档时保存数据
    /// </summary>
    [HarmonyPatch(typeof(DateFile.SaveDate), "FillDate")]
    public static class DateFile_SaveDate_FillDate_SaveData
    {
        static bool Prefix()
        {
            if (!Main.enabled) return true;

            if (MajordomoWindow.instance != null) MajordomoWindow.instance.SaveData();

            return true;
        }
    }
}
