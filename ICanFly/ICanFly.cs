using GameData;
using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;

namespace ICanFly
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry) => Save(this, modEntry);
        public bool avoid_battle = true;
        public int level = 9;
        public int jingchun = 1;
    }

    public static class Main
    {
        internal static bool enabled;
        internal static Settings settings;
        internal static UnityModManager.ModEntry.ModLogger Logger;

        private static Version gameVersion;
        internal static Version GameVersion
        {
            get
            {
                gameVersion = gameVersion ?? new Version(DateFile.instance.gameVersion.Replace("Beta V", "").Replace("[Test]", ""));
                return gameVersion;
            }
        }

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            try
            {
                settings = Settings.Load<Settings>(modEntry);
                var harmony = HarmonyInstance.Create(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
      
                modEntry.OnToggle = OnToggle;
                modEntry.OnGUI = OnGUI;
                modEntry.OnSaveGUI = OnSaveGUI;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                var inner = ex.InnerException;
                while (inner != null)
                {
                    Logger.Log(inner.ToString());
                    inner = inner.InnerException;
                }
                Debug.LogException(ex);
                return false;
            }
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("插件功能:");
            GUILayout.Label("避免暗渊对太吾及同道的伤害");
            GUILayout.Label("不再弹出追击/放走的对话框, 自动放走");

            settings.avoid_battle = GUILayout.Toggle(settings.avoid_battle, "自动放走");
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("不放走品级大于:");
            int.TryParse(GUILayout.TextField(settings.level.ToString(), 1, GUILayout.Width(30)), out settings.level);
            GUILayout.Label("品的敌人");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("不放走精纯值大于:");
            int.TryParse(GUILayout.TextField(settings.jingchun.ToString(), 1, GUILayout.Width(30)), out settings.jingchun);
            GUILayout.Label("的敌人");
            GUILayout.EndHorizontal();
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry) => settings.Save(modEntry);
    }

    /// <summary>
    /// 利用fastmove特性跳过暗渊伤害结算
    /// <see cref="WorldMapSystem.PlayerMoveDone"/>, a private method
    /// </summary>
    [HarmonyPatch(typeof(WorldMapSystem), "PlayerMoveDone")]
    internal static class WorldMapSystem_PlayerMoveDone_Patch
    {
        public static void Prefix(WorldMapSystem __instance, bool __state, int partId, int placeId, ref bool fastMove)
        {
            if (!Main.enabled)
                return;
            __state = fastMove;
            int num = int.Parse(DateFile.instance.GetNewMapDate(partId, placeId, 999));
            if (num == 20002)
            {
                fastMove = true;
            }
        }

        public static void Postfix(WorldMapSystem __instance, bool __state, int partId, int placeId, ref bool fastMove)
        {
            fastMove = __state;
        }
    }

    /// <summary>
    /// 让遭遇低精纯野怪的对话框不弹出, 直接给威望
    /// <see cref="UIManager.AddUI(string, object[])"/>
    /// </summary>
    [HarmonyPatch(typeof(UIManager), "AddUI")]
    internal static class UIManager_AddUI_Patch
    {
        public static bool Prefix(string uiPrefabName, params object[] onShowArgs)
        {
            if (!Main.enabled | !Main.settings.avoid_battle)
                return true;
            if (uiPrefabName == "ui_MessageWindow" && onShowArgs.Length > 0)
            {
                var arr = onShowArgs[0] as int[];
                // 112是追击/放走事件
                if (arr?.Length > 2 && arr[2] == 112)
                {
                    int enemy_id = arr[1];
                    // 判断对手的真气和精纯值
                    int enemy_level = Mathf.Abs(int.Parse(DateFile.instance.GetActorDate(enemy_id, 20, false)));
                    //int gangValueId = DateFile.instance.GetGangValueId(int.Parse(DateFile.instance.GetActorDate(enemy_id, 19, false)), num4);
                    var real_id = DateFile.instance.presetActorDate[int.Parse(DateFile.instance.GetActorDate(enemy_id, 997, false))][8];
                    var real_date = DateFile.instance.enemyRandDate[Convert.ToInt32(real_id)];

                    int zhenqi = int.Parse(real_date[3]);
                    int jingchun = Convert.ToInt32(real_date[6]);
#if DEBUG
                    Main.Logger.Log($"遇到ID为{enemy_id}的敌人{DateFile.instance.GetActorDate(enemy_id, 0, true)},身份组{enemy_level}, 精纯{jingchun}");
#endif
                    if (enemy_level < Main.settings.level || jingchun > Main.settings.jingchun)
                    {
                        return true;
                    }
                    MessageEventManager.Instance.MainEventData = arr;
                    EndEvent112_3();
                    return false;
                }
            }
            return true;

        }

        private static MethodInfo _endEvent112_3;

        private static void EndEvent112_3()
        {
            if (_endEvent112_3 == null)
            {
                _endEvent112_3 = typeof(MessageEventManager).GetMethod("EndEvent113_3", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            _endEvent112_3.Invoke(MessageEventManager.Instance, new object[] { });
        }

    }
}
