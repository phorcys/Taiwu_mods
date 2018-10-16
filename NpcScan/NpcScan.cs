using System.Collections.Generic;
using System.Reflection;
using Harmony12;
using UnityEngine;
using UnityModManagerNet;

namespace NpcScan
{

    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UI.key = key;
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        public KeyCode key;
        public Settings()
        {
            key = KeyCode.F12;
        }
    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static bool uiIsShow = false;
        public static bool binding_key = false;
        static KeyCode last_key_code = KeyCode.None;

        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            settings = Settings.Load<Settings>(modEntry);
            modEntry.OnGUI = OnGUI;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            if (!Main.uiIsShow)
            {
                UI.Load();
                UI.key = settings.key;
                Main.uiIsShow = true;
            }
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
                return false;
            enabled = value;

            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            Event e = Event.current;
            if (e.isKey && Input.anyKeyDown == true)
            {
                if (binding_key == true)
                {
                    settings.key = e.keyCode;
                }
                binding_key = false;
                settings.Save(modEntry);
            }
            var value = settings.key;
            GUILayout.BeginHorizontal();
            GUILayout.Label("快捷键", new GUILayoutOption[] { GUILayout.MinWidth(200.0f), GUILayout.MaxWidth(200.0f) });
            var ret = GUILayout.Button(value == KeyCode.None ? "请按下需要的按键" : value.ToString(), new GUILayoutOption[] { GUILayout.MinWidth(200.0f), GUILayout.MaxWidth(200.0f) });
            if (ret == true)
            {
                if (binding_key == true)
                {
                    settings.key = last_key_code;
                    binding_key = false;
                }
                else
                {
                    //保存临时值
                    last_key_code = value;
                    settings.key = KeyCode.None;
                    binding_key = true;
                }
            }
            GUILayout.EndHorizontal();
        }

        private static void processKeyPress()
        {


        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            UI.key = settings.key;
            settings.Save(modEntry);
        }

    }
}