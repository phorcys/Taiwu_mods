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
using System.Runtime.Serialization;
using BaseResourceMod;


namespace Majordomo
{
    public class Settings : UnityModManager.ModSettings
    {
        // 自动收获
        public bool autoHarvestItems = true;            // 自动收获物品
        public bool autoHarvestActors = true;           // 自动接纳新村民

        // 资源维护
        public int resMinHolding = 3;                   // 资源保有量警戒值（每月消耗量的倍数）
        public int[] resIdealHolding = null;            // 期望资源保有量
        public float resInitIdealHoldingRatio = 0.8f;   // 期望资源保有量的初始值（占当前最大值的比例）
        public int moneyMinHolding = 10000;             // 银钱最低保有量（高于此值管家可花费银钱进行采购）

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

        public const string RES_PATH_BASE = "resources";
        public const string RES_PATH_TXT = "txt";
        public const string RES_PATH_SPRITE = "Texture";


        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Main.Logger = modEntry.Logger;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            var resBasePath = System.IO.Path.Combine(modEntry.Path, RES_PATH_BASE);
            var resTxtPath = System.IO.Path.Combine(resBasePath, RES_PATH_TXT);
            var resSpritePath = System.IO.Path.Combine(resBasePath, RES_PATH_SPRITE);
            BaseResourceMod.Main.registModResourceDir(modEntry, resTxtPath, resSpritePath);

            Main.settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = Main.OnToggle;
            modEntry.OnGUI = Main.OnGUI;
            modEntry.OnSaveGUI = Main.OnSaveGUI;

            return true;
        }


        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Main.enabled = value;
            return true;
        }


        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("<color=#87CEEB>自动收获</color>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Main.settings.autoHarvestItems = GUILayout.Toggle(Main.settings.autoHarvestItems, "自动收获物品", GUILayout.Width(120));
            Main.settings.autoHarvestActors = GUILayout.Toggle(Main.settings.autoHarvestActors, "自动接纳新村民", GUILayout.Width(120));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

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
        }


        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Main.settings.Save(modEntry);
        }
    }


    public class TurnEvent
    {
        // 太吾管家过月事件 ID
        public const int EVENT_ID = 1001;


        // 注册过月事件
        // changTrunEvent format: [turnEventId, param1, param2, ...]
        // current changTrunEvent: [TurnEvent.EVENT_ID]
        // current GameObject.name: "TrunEventIcon,{TurnEvent.EVENT_ID}"
        public static void RegisterEvent(UIDate __instance)
        {
            __instance.changTrunEvents.Add(new int[] { TurnEvent.EVENT_ID });
        }


        // 设置过月事件文字
        public static void SetEventText(WindowManage __instance, bool on, GameObject tips)
        {
            if (tips == null || !on) return;
            if (tips.tag != "TrunEventIcon") return;

            string[] eventParams = tips.name.Split(',');
            int eventId = (eventParams.Length > 1) ? int.Parse(eventParams[1]) : 0;

            if (eventId != TurnEvent.EVENT_ID) return;

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


    // Patch: 展示过月事件
    [HarmonyPatch(typeof(UIDate), "SetTrunChangeWindow")]
    public static class UIDate_SetTrunChangeWindow_Patch
    {
        private static bool Prefix(UIDate __instance)
        {
            if (!Main.enabled) return true;

            AutoHarvest.GetAllBooties();

            ResourceMaintainer.TryBuyingResources();

            ResourceMaintainer.UpdateResourceWarning();

            TurnEvent.RegisterEvent(__instance);

            return true;
        }
    }


    // Patch: 设置浮窗文字
    [HarmonyPatch(typeof(WindowManage), "WindowSwitch")]
    public static class WindowManage_WindowSwitch_Patch
    {
        static void Postfix(WindowManage __instance, bool on, GameObject tips)
        {
            if (!Main.enabled) return;

            TurnEvent.SetEventText(__instance, on, tips);
        }
    }


    // Patch: 创建 UI
    [HarmonyPatch(typeof(UIDate), "Start")]
    public static class UIDate_Start_Patch
    {
        static void Postfix()
        {
            if (!Main.enabled) return;

            ResourceMaintainer.InitialzeResourcesIdealHolding();
        }
    }


    // Patch: 更新 UI
    [HarmonyPatch(typeof(UIDate), "Update")]
    public static class UIDate_Update_Patch
    {
        static void Postfix()
        {
            if (!Main.enabled) return;

            ResourceMaintainer.ShowResourceIdealHoldingText();
        }
    }


    // Patch: 显示浮窗
    [HarmonyPatch(typeof(WindowManage), "LateUpdate")]
    public static class WindowManage_LateUpdate_Patch
    {
        static bool Prefix(WindowManage __instance)
        {
            if (!Main.enabled) return true;

            ResourceMaintainer.InterfereFloatWindow(__instance);

            return true;
        }
    }
}
