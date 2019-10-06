using Harmony12;
using System;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;


// 游戏变速
namespace GameSpeeder
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry) => Save(this, modEntry);
        /// <summary>是否生效</summary>
        public bool enabled = true;
        /// <summary>速度倍率</summary>
        public float speedScale = 8f;
        /// <summary>死斗时暂停变速</summary>
        //public bool stopOnDesperateFight = true;  
        /// <summary>最小精纯超出值</summary>
        public int minJingcunExc = 1;
        /// <summary>面对高精纯敌人时暂停变速</summary>
        public bool stopOnHiJingcunEnemy = true;
        /// <summary>读书时暂停变速</summary>
        public bool stopOnReading = true;
        /// <summary>捉蟋蟀时暂停变速</summary>
        public bool stopOnCatching = true;
        /// <summary>激活变速热键</summary>
        public KeyCode hotKeyEnable = KeyCode.N;
    }

    public class GameSpeeder_Looper : MonoBehaviour
    {
        private void LateUpdate() => Main.CheckPerFrame();
    }

    public static class Main
    {
        private const uint MAX_SPEED = 16;
        private static int ctrlId_hotKeyEnable = int.MinValue;
        private static bool _enable;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        /// <summary>当前设定的时间流逝速度</summary>
        public static float lastTimeScale = 1f;
        /// <summary>游戏中默认的时间流逝速度</summary>
        public static float realTimeScale = 1f;
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
            lastTimeScale = realTimeScale = Time.timeScale;
            if (_looper == null)
            {
                _looper = new GameObject().AddComponent(
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

        public static bool IsTimePatchEnable() => lastTimeScale != realTimeScale;

        public static void ApplyTimeScale(bool enable, bool updateSetting = false)
        {
            _enable = enable;
            if (updateSetting)
                settings.enabled = enable;
            if (lastTimeScale != Time.timeScale)
                realTimeScale = Time.timeScale;
            if (_enable)
                lastTimeScale = Time.timeScale = realTimeScale * settings.speedScale * 1.00001f; // * 1.00001f方便检测之后游戏逻辑是否对Time.timeScale作了更改
            else
                lastTimeScale = Time.timeScale = realTimeScale;
        }
        /// <summary>当前状态有按键被按下</summary>
        private static bool _keyCurrentlyHeldDown = false;
        public static void CheckPerFrame()
        {
            if (lastTimeScale != Time.timeScale) // may be changed in game logic
            {
                ApplyTimeScale(_enable);
            }

            if (_isHotKeyHangUp)
                _isHotKeyHangUp = false;
            else if (Input.GetKeyDown(settings.hotKeyEnable))
                ApplyTimeScale(!_enable, true);

            if (Input.anyKey) // 任何键盘按键按下期间停止变速
            {
                _keyCurrentlyHeldDown = true;
                lastTimeScale = Time.timeScale = realTimeScale;
            }
            else if (_keyCurrentlyHeldDown)
            {
                _keyCurrentlyHeldDown = false;
                ApplyTimeScale(_enable); // 恢复变速
            }
        }

        private static string _jcExcTxt;
        private static int _jcExcCtrlKb = -1;

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            Color orgContentColor = GUI.contentColor;
            GUIStyle txtFieldStyle = GUI.skin.textField;
            txtFieldStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.Label("---基本配置---", new GUILayoutOption[0]);
            GUILayout.BeginHorizontal();
            GUI.contentColor = settings.enabled ? Color.green : Color.red;
            settings.enabled = GUILayout.Toggle(settings.enabled,
                settings.enabled ? "变速已激活" : "变速未激活", new GUILayoutOption[0]);
            GUI.contentColor = orgContentColor;
            GUILayout.Space(40);

            GUILayout.Label("倍速", new GUILayoutOption[0]);
            GUILayout.Label(settings.speedScale.ToString() + "x",
                txtFieldStyle, GUILayout.Width(40));
            int oldPos = (int)(settings.speedScale < 1 ? settings.speedScale * 10 : settings.speedScale * 2 + 9);
            int newPos = (int)(GUILayout.HorizontalSlider(oldPos, 1, 10 + MAX_SPEED * 2 - 1, GUILayout.Width(250)));
            if (oldPos != newPos)
            {
                float newScale = newPos < 10 ? newPos / 10f : newPos - 9;
                if (newScale == 3)
                    newScale = 1.5f;
                else if (newScale > 1)
                    newScale = (float)Math.Floor(newScale / 2);
                settings.speedScale = newScale;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(0);
            GUILayout.Label("---扩展配置---", new GUILayoutOption[0]);

            GUILayout.BeginHorizontal();
            //settings.stopOnDesperateFight = GUILayout.Toggle(
            //    settings.stopOnDesperateFight, "死斗开启时自动暂停变速", new GUILayoutOption[0]);
            settings.stopOnHiJingcunEnemy = GUILayout.Toggle(
                settings.stopOnHiJingcunEnemy, "战斗首次有高于主角精纯", new GUILayoutOption[0]);

            GUI.enabled = settings.stopOnHiJingcunEnemy;
            if (_jcExcTxt == null)
                _jcExcTxt = settings.minJingcunExc.ToString();
            _jcExcTxt = GUILayout.TextField(_jcExcTxt, GUILayout.Width(32));
            if (GUI.changed)
            {
                _jcExcCtrlKb = GUIUtility.keyboardControl;
            }
            else if (_jcExcCtrlKb != -1 && _jcExcCtrlKb != GUIUtility.keyboardControl)
            {
                if (int.TryParse(_jcExcTxt, out int newJcExc))
                    settings.minJingcunExc = Math.Min(100, Math.Max(-99, newJcExc));
                _jcExcCtrlKb = -1;
                _jcExcTxt = null;
            }
            GUI.enabled = true;

            GUILayout.Label("点敌人出战时自动暂停变速", new GUILayoutOption[0]);

            GUILayout.FlexibleSpace();

            settings.stopOnReading = GUILayout.Toggle(
                settings.stopOnReading, "读书开启时自动暂停变速", new GUILayoutOption[0]);

            GUILayout.FlexibleSpace();

            settings.stopOnCatching = GUILayout.Toggle(
                settings.stopOnCatching, "捕促织时自动暂停变速", new GUILayoutOption[0]);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(0);
            GUILayout.BeginHorizontal();
            GUILayout.Label("---配置热键---", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("变速开关热键：", new GUILayoutOption[0]);

            const string showTip = "<按键...>";
            bool isReadyToSet = ctrlId_hotKeyEnable == GUIUtility.hotControl;
            string sShowed = isReadyToSet ? showTip : settings.hotKeyEnable.ToString();
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
                        settings.hotKeyEnable = Event.current.keyCode;
                    }
                    Event.current.Use();
                    ctrlId_hotKeyEnable = int.MinValue;
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.speedScale = (float)Math.Round(settings.speedScale, 1);
            if (settings.speedScale < 0.1f)
                settings.speedScale = 0.1f;
            else if (settings.speedScale > MAX_SPEED)
                settings.speedScale = MAX_SPEED;
            settings.Save(modEntry);
            ApplyTimeScale(settings.enabled);
        }
    }

    //[HarmonyPatch(typeof(BattleSystem), "ShowBattleWindow")]
    //public static class EnterBattlePatch
    //{
    //    private static void Postfix(BattleSystem __instance)
    //    {
    //        // Main.Logger.Log("start battle " + StartBattle.instance.battleLoseTyp);
    //        if (Main.settings.stopOnDesperateFight && StartBattle.instance.battleLoseTyp >= 100) // 死斗模式
    //            Main.ApplyTimeScale(false);
    //    }
    //}

    [HarmonyPatch(typeof(BattleSystem), "GetEnemy")]
    public static class ChangeEnemyPatch
    {
        private static bool _thisBattleAlreadySet = false;
        private static void Postfix(BattleSystem __instance, bool newBattle)
        {
            if (newBattle)
                _thisBattleAlreadySet = false;
            else if (_thisBattleAlreadySet)
                return;

            int nEnemyId = __instance.ActorId(false, false);
            int nPlayerId = __instance.ActorId(true, false);
            int enemyJingCunPoint = int.Parse(DateFile.instance.GetActorDate(nEnemyId, 901, false));
            int playerJingCunPoint = int.Parse(DateFile.instance.GetActorDate(nPlayerId, 901, false));
            // Main.Logger.Log("New Enemy Entered " + nEnemyId + " " + enemyJingCunPoint);
            if (Main.settings.stopOnHiJingcunEnemy
                && enemyJingCunPoint - playerJingCunPoint >= Main.settings.minJingcunExc)
            {
                _thisBattleAlreadySet = true;
                Main.ApplyTimeScale(false);
            }
        }
    }

    /// <summary>
    /// 解决某些情况下变招成功无法触发变招效果的问题
    /// </summary>
    /// <remarks>
    /// 猜测原作者此修复的原理:
    /// 游戏中<see cref="BattleSystem.AttackPartChooseEnd"/>是个协程，第一次运行时会等待现实时间的0.2s后继续执行，
    /// 加速后0.2s会发生很多游戏战斗事件，可能会在<see cref="BattleSystem.AttackPartChooseEnd"/>继续执行(触发
    /// 变招效果)之前覆盖变招效果，导致变招效果再也没机会触发。此修复是将<see cref="Time.timeScale"/> 设为0，此时
    /// 游戏中除frame rate dependent的事件外，全部事件暂停，协程<see cref="BattleSystem.AttackPartChooseEnd"/>
    /// 等待时间改为0，即为等待一帧后继续执行，可以避免变招效果在等待期间被覆盖。
    /// </remarks>
    [HarmonyPatch(typeof(BattleSystem), "SetChooseAttackPart")]
    public static class SetChooseAttackPartPatch
    {
        private static void Postfix() => Time.timeScale = 0; // Fix变招的逻辑step 1
    }

    [HarmonyPatch(typeof(BattleSystem), "AttackPartChooseEnd")]
    public static class AttackPartChooseEndPatch
    {
        private static void Prefix(ref float waitTime) => waitTime = 0; // Fix变招的逻辑step 2
    }


    [HarmonyPatch(typeof(BattleEndWindow), "BattleEnd")]
    public static class ExitBattlePatch
    {
        private static void Postfix() =>
            // Main.Logger.Log("end battle " + StartBattle.instance.battleLoseTyp);
            Main.ApplyTimeScale(Main.settings.enabled);
    }

    // 这个函数产生的协程使用了WaitForSecondsRealtime，timescale无法直接变速，故需patch it
    [HarmonyPatch(typeof(BattleSystem), "TimePause")]
    public static class TimePausePatch
    {
        private static void Prefix(ref float autoTime)
        {
            // 变速配置激活状态且倍速>1才改这个等待时间
            if (Main.settings.enabled && Main.settings.speedScale > 1)
                autoTime /= Main.settings.speedScale;
        }
    }

    [HarmonyPatch(typeof(ReadBook), "SetReadBookWindow")]
    public static class StartReadBook
    {
        private static void Postfix()
        {
            // Main.Logger.Log("start readbook ");
            if (Main.settings.stopOnReading)
                Main.ApplyTimeScale(false);
        }
    }

    [HarmonyPatch(typeof(ReadBook), "CloseReadBookWindow")]
    public static class EndReadBook
    {
        private static void Postfix() =>
            // Main.Logger.Log("end readbook " + StartBattle.instance.battleTyp);
            Main.ApplyTimeScale(Main.settings.enabled);
    }

    [HarmonyPatch(typeof(GetQuquWindow), "ShowGetQuquWindow")]
    public static class StartCatching
    {
        private static void Postfix()
        {
            // Main.Logger.Log("start catching ");
            if (Main.settings.stopOnCatching)
                Main.ApplyTimeScale(false);

            PatchQuquWindowUpdate.Reset();
        }
    }

    [HarmonyPatch(typeof(GetQuquWindow), "CloseGetQuquWindow")]
    public static class EndCatching
    {
        private static void Postfix() =>
            // Main.Logger.Log("end catching ");
            Main.ApplyTimeScale(Main.settings.enabled);
    }

    // 使GetQuquWindow受变速影响
    [HarmonyPatch(typeof(GetQuquWindow), "LateUpdate")]
    public static class PatchQuquWindowUpdate
    {
        private static float _fixDeltaTime = 0;
        private static bool _stopPatching = false;

        private static bool Prefix(GetQuquWindow __instance, MethodBase __originalMethod)
        {
            if (_stopPatching)
                return true;

            // 没有激活变速时不patch避免功能未激活时对原游戏功能可能产生的影响
            if (!Main.IsTimePatchEnable())
                return true;

            float realDeltaTime = Time.unscaledDeltaTime;
            _fixDeltaTime += Time.deltaTime;
            float fFramePass = _fixDeltaTime / realDeltaTime;
            int nFramePass = (int)Math.Floor(fFramePass);
            _fixDeltaTime -= nFramePass * realDeltaTime;

            while (nFramePass-- > 0)
            {
                if (!__instance.getQuquWindow.activeSelf)
                    return false;

                _stopPatching = true;
                __originalMethod.Invoke(__instance, null);
                _stopPatching = false;
            }

            return false;
        }

        public static void Reset() => _fixDeltaTime = 0;
    }
}


