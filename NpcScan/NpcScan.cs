using Harmony12;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace NpcScan
{

    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry) => Save(this, modEntry);
        public KeyCode[] keys = { KeyCode.F3, KeyCode.PageUp, KeyCode.PageDown };

        // 每页最多显示的npc数目
        public int countPerPage = 200;
    }

    public static class Main
    {
        /// <summary>是否启用mod</summary>
        internal static bool enabled;
        /// <summary>Mod设置</summary>
        internal static Settings settings;
        /// <summary>mod的UI是否已加载</summary>
        internal static bool uiIsShow = false;
        /// <summary>是否正在修改热键</summary>
        internal static bool[] bindingKeys;
        /// <summary>特性字典</summary>
        internal static readonly Dictionary<int, Features> featuresList = new Dictionary<int, Features>();
        /// <summary>特性名称对应特性ID</summary>
        internal static readonly Dictionary<string, int> featureNameList = new Dictionary<string, int>();
        /// <summary>特性搜索条件中，一个名称对应多个特性ID的特性组别ID列表</summary>
        internal static readonly HashSet<int> multinameFeatureGroupIdSet = new HashSet<int>();
        /// <summary>功法名字反查ID</summary>
        internal static readonly Dictionary<string, int> gongFaNameList = new Dictionary<string, int>();
        /// <summary>技艺名字反查ID</summary>
        internal static readonly Dictionary<string, int> skillNameList = new Dictionary<string, int>();
        /// <summary>物品名字反查物品基础ID</summary>
        internal static readonly Dictionary<string, List<int>> itemNameList = new Dictionary<string, List<int>>();
        /// <summary>文字颜色</summary>
        internal static readonly Dictionary<int, string> textColor = new Dictionary<int, string>()
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
        internal static readonly Dictionary<string, int> colorText = new Dictionary<string, int>()
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
            if (modEntry == null)
                throw new ArgumentNullException(nameof(modEntry));

            Logger = modEntry.Logger;
            try
            {
                var harmony = HarmonyInstance.Create(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                settings = Settings.Load<Settings>(modEntry);
                bindingKeys = new bool[settings.keys.Length];

                modEntry.OnToggle = OnToggle;
                modEntry.OnGUI = OnGUI;
                modEntry.OnSaveGUI = OnSaveGUI;

                if (!uiIsShow)
                {
                    UI.Load();
                    uiIsShow = true;
                }
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
                for (int i = 0; i < bindingKeys.Length; i++)
                {
                    if (bindingKeys[i])
                    {
                        if ((e.keyCode >= KeyCode.A && e.keyCode <= KeyCode.Z)
                            || (e.keyCode >= KeyCode.F1 && e.keyCode <= KeyCode.F12)
                            || (e.keyCode >= KeyCode.Alpha0 && e.keyCode <= KeyCode.Alpha9)
                            || e.keyCode == KeyCode.PageDown
                            || e.keyCode == KeyCode.PageUp
                            )
                        {
                            // 若热键被其他功能占用，则交换热键
                            for (int j = 0; j < settings.keys.Length; j++)
                            {
                                if (j != i && settings.keys[j] == e.keyCode)
                                {
                                    settings.keys[j] = settings.keys[i];
                                }
                            }
                            settings.keys[i] = e.keyCode;
                        }
                        bindingKeys[i] = false;
                    }
                }
            }
            SettingKeys("设置打开/关闭窗口快捷键： ", 0, 160);
            GUILayout.Label("该功能传统热键Ctrl+F12依然有效");
            SettingKeys("设置向上翻页快捷键： ", 1, 130);
            SettingKeys("设置向下翻页快捷键： ", 2, 130);
            GUILayout.BeginHorizontal();
            // 设置每页最多显示npc的数目
            GUILayout.Label("每页最大显示NPC数量(1~300)：", GUILayout.Width(370));
            int.TryParse(GUILayout.TextArea(settings.countPerPage.ToString(), GUILayout.Width(60)), out settings.countPerPage);
            settings.countPerPage = settings.countPerPage > 0 && settings.countPerPage < 301 ? settings.countPerPage : 200;
            GUILayout.EndHorizontal();
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry) => settings.Save(modEntry);

        /// <summary>
        /// 设置热键控件
        /// </summary>
        /// <param name="text">控件的说明文字</param>
        /// <param name="index">控件的序号</param>
        /// <param name="textWidth">控件的说明文字框宽度</param>
        public static void SettingKeys(string text, int index, float textWidth)
        {
            if (index < bindingKeys.Length)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(text, GUILayout.Width(textWidth > 0 ? textWidth : 130));
                if (GUILayout.Button((bindingKeys[index] ? "请按键" : settings.keys[index].ToString()),
                    GUILayout.Width(80)))
                {
                    bindingKeys[index] = !bindingKeys[index];
                }
                GUILayout.Label("（支持0-9,A-Z,F1-F12,PageDown,PageUp）");
                GUILayout.EndHorizontal();
            }
        }
    }

    /// <summary>
    /// 扩展方法
    /// </summary>
    internal static class ExtendedMethod
    {
        /// <summary>
        /// 在所给序数位置给数组添加成员并将序数加一
        /// </summary>
        /// <typeparam name="T">数组成员类型</typeparam>
        /// <param name="array">操作的数组</param>
        /// <param name="data">要添加的数据</param>
        /// <param name="index">要添加的位置</param>
        public static void Add<T>(this T[] array, T data, ref int index)
        {
            array[index] = data;
            index++;
        }
    }
}
