using Harmony12;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace ModsCollection
{
    public class Settings : UnityModManager.ModSettings 
    {
        public override void Save(UnityModManager.ModEntry modEntry) { Save(this, modEntry); }
        public int patch1 = 1;
        public int patch2 = 2;
        public bool patch3a = true;
        public bool patch3b = true;
        public bool patch3c = false;
        public int patch4 = 0;
        public bool patch5a = true;
        public bool patch5b = true;
        public bool patch5c = true;
        public bool patch5d = true;
        public bool patch5e = true;
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
            ExecuteAllPatchs();
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
            GUILayout.Label("基于各大论坛帖子的txt mod\n(大部分mod附带还原功能, 少量打 * 的,在取消使用后,需要重启才可复原.)");

            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("直接邀为同道", labelStyle, GUILayout.Width(250));
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
            settings.patch3a = GUILayout.Toggle(settings.patch3a, "优质血脉遗传", toggleStyle, GUILayout.Width(200));
            settings.patch3b = GUILayout.Toggle(settings.patch3b, "行动力增加", toggleStyle);
            settings.patch3c = GUILayout.Toggle(settings.patch3c, "全民天才", toggleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("* 个人用合理向练功房", labelStyle, GUILayout.Width(250));
            settings.patch4 = GUILayout.SelectionGrid(settings.patch4, new string[] { "关闭", "启用" }, 2, toggleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("种田派(改)", labelStyle, GUILayout.Width(250));
            settings.patch5a = GUILayout.Toggle(settings.patch5a, "建筑\n降低人力", toggleStyle, GUILayout.Width(120));
            settings.patch5b = GUILayout.Toggle(settings.patch5b, "等级上限\n提升", toggleStyle, GUILayout.Width(145));
            settings.patch5c = GUILayout.Toggle(settings.patch5c, "维护费\n改税收", toggleStyle, GUILayout.Width(120));
            settings.patch5d = GUILayout.Toggle(settings.patch5d, "仓库容量\n提升", toggleStyle, GUILayout.Width(145));
            settings.patch5e = GUILayout.Toggle(settings.patch5e, "工作效率\n提升", toggleStyle, GUILayout.Width(145));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(20);

            GUILayout.Space(5);

        }

        public static void ExecuteAllPatchs()
        {
            Patchings instance = new Patchings();
            var patchs = instance
              .GetType()
              .GetMethods(BindingFlags.Public | BindingFlags.Instance)
              .Where(item => item.Name.StartsWith("Patch"));
            foreach (var patch in patchs)
                patch.Invoke(instance, new System.Object[0]);
        }
    }

    [HarmonyPatch(typeof(WindowManage), "WindowSwitch")]
    public static class WindowManage_WindowSwitch_Patch
    {
        [HarmonyAfter(new string[] { "CharacterFloatInfo" })]
        public static void Postfix(ref int ___tipsW)
        {
            if (Main.settings.patch3a && ___tipsW == 680) ___tipsW = 777;
            else if (!Main.settings.patch3a && ___tipsW == 777) ___tipsW = 680;
        }
    }

    [HarmonyPatch(typeof(ui_Loading), "OnShow")]
    public static class Loading_LoadBaseDate_Patch
    {
        public static void Postfix()
        {
            Main.ExecuteAllPatchs();
        }
    }

}
