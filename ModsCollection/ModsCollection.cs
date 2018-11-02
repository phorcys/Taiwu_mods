using Harmony12;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace ModsCollection
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry) { Save(this, modEntry); }
        public int patch1 = 0;
        public int patch2 = 0;
        public bool patch3a = false;
        public bool patch3b = false;
        public bool patch3c = false;
        public int patch4 = 0;
    }
    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUIStyle labelStyle = UiStyle.Label();
            GUIStyle toggleStyle = UiStyle.Toggle();
            GUILayout.Label("以下是基于各大论坛帖子的小修改");

            GUILayout.Space(5);

            GUILayout.BeginHorizontal("Box");
            GUILayout.Label( "直接邀为同道", labelStyle, GUILayout.Width(250));
            settings.patch1 = GUILayout.SelectionGrid(settings.patch1, new string[] { "关闭", "启用" }, 2, toggleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("人口数量控制", labelStyle, GUILayout.Width(250));
            settings.patch2 = GUILayout.SelectionGrid(settings.patch2, new string[] { "关闭", "一孩政策", "壯大家族", "同时实行" }, 4, toggleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("天才世界", labelStyle, GUILayout.Width(250));
            settings.patch3a = GUILayout.Toggle(settings.patch3a, "優質血脈遺傳", toggleStyle, GUILayout.Width(200));
            settings.patch3b = GUILayout.Toggle(settings.patch3b, "行動力60", toggleStyle, GUILayout.ExpandWidth(false));
            settings.patch3c = GUILayout.Toggle(settings.patch3c, "全民天才", toggleStyle, GUILayout.ExpandWidth(false));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("个人用合理向练功房", labelStyle, GUILayout.Width(250));
            settings.patch4 = GUILayout.SelectionGrid(settings.patch4, new string[] { "关闭", "启用" }, 2, toggleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

        }

    }

}
