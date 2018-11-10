using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;


// 游戏变速
namespace GameSpeeder
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }

        public bool enabled = true; // 是否生效
        public float speedScale = 1f;// 速度倍率
        public bool stopOnDesperateFight = true; // 死斗时暂停变速
        public bool stopOnReading = true; // 读书时暂停变速
        public bool stopOnCatching = true; // 捉蟋蟀时暂停变速
        public KeyCode hotKeyEnable = KeyCode.N; // 激活变速热键
    }

    public class GameSpeeder_Looper : UnityEngine.MonoBehaviour
    {
        //void Update() { }

        void LateUpdate()
        {
            Main.CheckPerFrame();
        }
    }

    public static class Main
    {
        const uint MAX_SPEED = 16;
        static int ctrlId_hotKeyEnable = int.MinValue;
        private static bool _enable;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        private static float lastTimeScale = 1f;
        private static float realTimeScale = 1f;
        private static GameSpeeder_Looper _looper = null;
        private static bool _isHotKeyHangUp = false;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            realTimeScale = Time.timeScale;
            if (_looper == null)
            {
                _looper = (new UnityEngine.GameObject()).AddComponent(
                    typeof(GameSpeeder_Looper)) as GameSpeeder_Looper;
                UnityEngine.Object.DontDestroyOnLoad(_looper);
            }
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            _enable = value;
            return true;
        }

        public static void ApplyTimeScale(bool enable, bool updateSetting = false)
        {
            _enable = enable;
            if (updateSetting)
                settings.enabled = enable;
            if (lastTimeScale != Time.timeScale)
                realTimeScale = Time.timeScale;
            if (_enable)
                lastTimeScale = Time.timeScale = realTimeScale * Main.settings.speedScale + 0.00001f;
            else
                lastTimeScale = Time.timeScale = realTimeScale;
        }

        public static void CheckPerFrame()
        {
            if (lastTimeScale != Time.timeScale) // may be changed in game logic
                ApplyTimeScale(_enable);
            if (_isHotKeyHangUp)
                _isHotKeyHangUp = false;
            else if (Input.GetKeyDown(settings.hotKeyEnable))
                ApplyTimeScale(!_enable, true);
            
        }        

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            Color orgContentColor = GUI.contentColor;
            GUIStyle txtFieldStyle = GUI.skin.textField;
            txtFieldStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.Label("---基本配置---", new GUILayoutOption[0]);
            GUILayout.BeginHorizontal();
            GUI.contentColor = Main.settings.enabled ? Color.green : Color.red;
            Main.settings.enabled = GUILayout.Toggle(Main.settings.enabled,
                Main.settings.enabled ? "变速已激活" : "变速未激活", new GUILayoutOption[0]);
            GUI.contentColor = orgContentColor;
            GUILayout.Space(40);

            GUILayout.Label("倍速", new GUILayoutOption[0]);
            GUILayout.Label(Main.settings.speedScale.ToString() + "x",
                txtFieldStyle, GUILayout.Width(40));
            int oldPos = (int)(Main.settings.speedScale < 1 ? Main.settings.speedScale * 10 : Main.settings.speedScale + 9);
            int newPos = (int)(GUILayout.HorizontalSlider(oldPos, 1, 10 + MAX_SPEED - 1, GUILayout.Width(250)));
            if (oldPos != newPos)
                Main.settings.speedScale = newPos < 10 ? newPos / 10f : newPos - 9;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(0);
            GUILayout.Label("---扩展配置---", new GUILayoutOption[0]);
            GUILayout.BeginHorizontal();
            Main.settings.stopOnDesperateFight = GUILayout.Toggle(
                Main.settings.stopOnDesperateFight, "死斗开启时自动暂停变速", new GUILayoutOption[0]);
            Main.settings.stopOnReading = GUILayout.Toggle(
                Main.settings.stopOnReading, "读书开启时自动暂停变速", new GUILayoutOption[0]);
            Main.settings.stopOnCatching = GUILayout.Toggle(
                Main.settings.stopOnCatching, "捕促织时自动暂停变速", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();

            GUILayout.Space(0);
            GUILayout.BeginHorizontal();
            GUILayout.Label("---配置热键---", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("激活/暂停变速：", new GUILayoutOption[0]);
            
            const string showTip = "<按键...>";
            bool isReadyToSet = ctrlId_hotKeyEnable == GUIUtility.hotControl;
            string sShowed = isReadyToSet ? showTip : Main.settings.hotKeyEnable.ToString();
            Color setColor = isReadyToSet ? Color.yellow : orgContentColor;
            GUI.contentColor = setColor;
            bool bClick = GUILayout.Button(sShowed, txtFieldStyle, GUILayout.Width(120));
            GUI.contentColor = orgContentColor;
            if (bClick)
            {
                ctrlId_hotKeyEnable = GUIUtility.GetControlID(FocusType.Passive); // 捕获按键
                GUIUtility.hotControl = ctrlId_hotKeyEnable;
                _isHotKeyHangUp = true;
            }
            if (isReadyToSet)
            {
                _isHotKeyHangUp = true;
                if (Event.current.type == EventType.KeyDown)
                {
                    if (KeyCode.Escape != Event.current.keyCode)
                    {
                        Main.settings.hotKeyEnable = Event.current.keyCode;
                    }
                    Event.current.Use();
                    ctrlId_hotKeyEnable = int.MinValue;
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Main.settings.speedScale = (float)Math.Round(Main.settings.speedScale, 1);
            if (Main.settings.speedScale < 0.1f)
                Main.settings.speedScale = 0.1f;
            else if (Main.settings.speedScale > MAX_SPEED)
                Main.settings.speedScale = MAX_SPEED;
            settings.Save(modEntry);
            ApplyTimeScale(Main.settings.enabled);
        }
    }

    [HarmonyPatch(typeof(BattleSystem), "ShowBattleWindow")]
    public static class EnterBattlePatch
    {
        private static void Postfix(BattleSystem __instance)
        {
            // Main.Logger.Log("start battle " + StartBattle.instance.battleLoseTyp);
            
            if (Main.settings.stopOnDesperateFight && StartBattle.instance.battleLoseTyp >= 100)
                Main.ApplyTimeScale(false);
        }
    }

    [HarmonyPatch(typeof(BattleSystem), "BattleEnd")]
    public static class ExitBattlePatch
    {
        private static void Postfix(BattleSystem __instance)
        {
            // Main.Logger.Log("end battle " + StartBattle.instance.battleLoseTyp);
            Main.ApplyTimeScale(Main.settings.enabled);
        }
    }

    [HarmonyPatch(typeof(ReadBook), "SetReadBookWindow")]
    public static class StartReadBook
    {
        private static void Postfix(ReadBook __instance)
        {
            // Main.Logger.Log("start readbook ");
            if (Main.settings.stopOnReading)
                Main.ApplyTimeScale(false);
        }
    }

    [HarmonyPatch(typeof(ReadBook), "CloseReadBookWindow")]
    public static class EndReadBook
    {
        private static void Postfix(ReadBook __instance)
        {
            // Main.Logger.Log("end readbook " + StartBattle.instance.battleTyp);
            Main.ApplyTimeScale(Main.settings.enabled);
        }
    }

    [HarmonyPatch(typeof(GetQuquWindow), "ShowGetQuquWindow")]
    public static class StartCatching
    {
        private static void Postfix(GetQuquWindow __instance)
        {
            // Main.Logger.Log("start catching ");
            if (Main.settings.stopOnReading)
                Main.ApplyTimeScale(false);
        }
    }

    [HarmonyPatch(typeof(GetQuquWindow), "CloseGetQuquWindow")]
    public static class EndCatching
    {
        private static void Postfix(GetQuquWindow __instance)
        {
            // Main.Logger.Log("end catching ");
            Main.ApplyTimeScale(Main.settings.enabled);
        }
    }
}


