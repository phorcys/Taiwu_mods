using Harmony12;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace NpcScan
{

    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry) => Save(this, modEntry);
        public KeyCode key = KeyCode.F12;

        // 每页最多显示的npc数目
        public int countPerPage = 8;
    }

    public static class Main
    {
        /// <summary>是否启用mod</summary>
        internal static bool enabled;
        /// <summary>是否正在搜索NPC</summary>
        internal static bool isScaningNpc;
        /// <summary>Mod设置</summary>
        internal static Settings settings;
        /// <summary>mod的UI是否已加载</summary>
        internal static bool uiIsShow = false;
        /// <summary>是否正在修改热键</summary>
        internal static bool bindingKey = false;
        /// <summary>角色特性字典</summary>
        internal static Dictionary<int, Features> featuresList = new Dictionary<int, Features>();
        /// <summary>特性搜索条件</summary>
        internal static HashSet<int> findSet = new HashSet<int>();
        /// <summary>特性名称对应特性ID</summary>
        internal static Dictionary<string, int> fNameList = new Dictionary<string, int>();
        /// <summary>功法搜索条件</summary>
        internal static List<int> gongFaList = new List<int>();
        /// <summary>功法搜索条件</summary>
        internal static List<int> skillList = new List<int>();
        /// <summary>功法名字反查ID</summary>
        internal static Dictionary<string, int> gNameList = new Dictionary<string, int>();
        /// <summary>技艺名字反查ID</summary>
        internal static Dictionary<string, int> bNameList = new Dictionary<string, int>();
        /// <summary>文字颜色</summary>
        internal static Dictionary<int, string> textColor = new Dictionary<int, string>()
        {
            { 10000,"<color=#323232FF>"},
            { 10001,"<color=#4B4B4BFF>"},
            { 10002,"<color=#B97D4BFF>"},
            { 10003,"<color=#9B8773FF>"},
            { 10004,"<color=#AF3737FF>"},
            { 10005,"<color=#FFE100FF>"},
            { 10006,"<color=#FF44A7FF>"},
            { 20001,"<color=#E1CDAAFF>"},
            { 20002,"<color=#8E8E8EFF>"}, //九品灰
            { 20003,"<color=#FBFBFBFF>"}, //八品白
            { 20004,"<color=#6DB75FFF>"}, //七品绿
            { 20005,"<color=#8FBAE7FF>"}, //六品青
            { 20006,"<color=#63CED0FF>"}, //五品蓝
            { 20007,"<color=#AE5AC8FF>"}, //四品紫
            { 20008,"<color=#E3C66DFF>"}, //三品金
            { 20009,"<color=#F28234FF>"}, //二品橙
            { 20010,"<color=#E4504DFF>"}, //一品红
            { 20011,"<color=#EDA723FF>"},
        };
        /// <summary>文字颜色反查</summary>
        public static Dictionary<string, int> colorText = new Dictionary<string, int>()
        {
            { "<color=#323232FF>",10000},
            { "<color=#4B4B4BFF>",10001},
            { "<color=#B97D4BFF>",10002},
            { "<color=#9B8773FF>",10003},
            { "<color=#AF3737FF>",10004},
            { "<color=#FFE100FF>",10005},
            { "<color=#FF44A7FF>",10006},
            { "<color=#E1CDAAFF>",20001},
            { "<color=#8E8E8EFF>",20002}, //九品灰
            { "<color=#FBFBFBFF>",20003}, //八品白
            { "<color=#6DB75FFF>",20004}, //七品绿
            { "<color=#8FBAE7FF>",20005}, //六品青
            { "<color=#63CED0FF>",20006}, //五品蓝
            { "<color=#AE5AC8FF>",20007}, //四品紫
            { "<color=#E3C66DFF>",20008}, //三品金
            { "<color=#F28234FF>",20009}, //二品橙
            { "<color=#E4504DFF>",20010}, //一品红
            { "<color=#EDA723FF>",20011},
        };

        internal static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            if (!uiIsShow)
            {
                UI.Load(modEntry);
                UI.key = settings.key;
                uiIsShow = true;
            }
            return true;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
                return false;
            enabled = value;

            return true;
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            Event e = Event.current;
            if (e.isKey && Input.anyKeyDown)
            {
                if (bindingKey)
                {
                    if ((e.keyCode >= KeyCode.A && e.keyCode <= KeyCode.Z)
                        || (e.keyCode >= KeyCode.F1 && e.keyCode <= KeyCode.F12)
                        || (e.keyCode >= KeyCode.Alpha0 && e.keyCode <= KeyCode.Alpha9)
                        )
                    {
                        settings.key = e.keyCode;
                    }
                    bindingKey = false;
                }
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("设置快捷键： Ctrl +", GUILayout.Width(130));
            if (GUILayout.Button((bindingKey ? "请按键" : settings.key.ToString()),
                GUILayout.Width(80)))
            {
                bindingKey = !bindingKey;
            }
            GUILayout.Label("（支持0-9,A-Z,F1-F12）");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            // 设置每页最多显示npc的数目
            GUILayout.Label("每页最大显示NPC数量(大于1)：", GUILayout.Width(370));
            int.TryParse(GUILayout.TextArea(settings.countPerPage.ToString(), GUILayout.Width(60)), out settings.countPerPage);
            settings.countPerPage = settings.countPerPage > 0 ? settings.countPerPage : 8;
            GUILayout.EndHorizontal();
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry) => settings.Save(modEntry);
    }

    /// <summary>
    /// 使<see cref="DateFile.GetActorFeature(int key)"/>显示儿童隐藏特性
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "GetActorFeature", typeof(int))]
    internal static class DateFile_GetActorFeature_Patch
    {
        private static bool Prefix(int key, ref List<int> __result)
        {
            if (!Main.enabled || !Main.isScaningNpc)
                return true;

            var age = int.Parse(DateFile.instance.GetActorDate(key, 11, false));
            // 游戏中儿童的特性会被隐藏一部分，因此儿童的特性获取不能直接用游戏中的缓存，成年人可以
            if (age > 14 && DateFile.instance.actorsFeatureCache.TryGetValue(key, out __result))
            {
                return false;
            }
            var list = new List<int>();
            string[] actorFeatures = DateFile.instance.GetActorDate(key, 101, addValue: false).Split('|');

            for (int j = 0; j < actorFeatures.Length; j++)
            {
                int fetureKey = int.Parse(actorFeatures[j]);
                if (fetureKey == 0)
                {
                    continue;
                }

                list.Add(fetureKey);
            }
            if (age > 14 && DateFile.instance.actorsDate.ContainsKey(key))
            {
                DateFile.instance.actorsFeatureCache.Add(key, list);
            }
            __result = list;
            return false;
        }
    }
}
