using Harmony12;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityModManagerNet;

namespace Sth4nothing.SLManager
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry) { Save(this, modEntry); }
        public bool blockAutoSave = false;
        public int maxSave = 8;
    }
    public static class Main
    {
        public static bool enabled;
        public static bool forceSave = false;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                Assembly.LoadFrom(Path.Combine(modEntry.Path, "DotNetZip.dll"));
            }
            catch (System.Exception) { }
            Logger = modEntry.Logger;
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            return true;
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("阻止游戏自动储存", GUILayout.Width(200));
            settings.blockAutoSave = GUILayout.SelectionGrid(settings.blockAutoSave ? 1 : 0, new string[] { "关闭", "启用" }, 2, GUILayout.Width(150)) == 1;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("存档列表的最大存档数(0表示不受限制)", GUILayout.Width(300));
            var num = -1;
            if (int.TryParse(GUILayout.TextField(Main.settings.maxSave.ToString(), GUILayout.Width(60)), out num))
            {
                if (num >= 0)
                    Main.settings.maxSave = num;
            }
            GUILayout.EndHorizontal();
        }
        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }
    }


}
