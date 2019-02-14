using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityModManagerNet;

namespace KeyBoardShortCut
{
    public enum MODIFIER_KEY_TYPE
    {
        MKT_SHIFT = 0,
        MKT_CTRL = 1,
        MKT_ALT = 2,
    };

    public enum HK_TYPE
    {
        HK_NONE = 0,
        HK_CLOSE = 1,
        HK_COMFIRM,
        HK_CONFIRM2,

        HK_UP,
        HK_LEFT,
        HK_DOWN,
        HK_RIGHT,
        HK_UP2,
        HK_LEFT2,
        HK_DOWN2,
        HK_RIGHT2,

        HK_ACTORMENU,
        HK_VILLAGE,
        HK_VILLAGE_LOCAL,
        HK_WORLDMAP,

        HK_HEAL,
        HK_POISON,
        HK_GATHER_FOOD,
        HK_GATHER_MINERAL,
        HK_GATHER_HERB,
        HK_GATHER_MONEY,
        HK_GATHER_CLOTH,
        HK_GATHER_WOOD,
        HK_VISITEVENT,

        HK_BATTEL_QINGGONG_1,
        HK_BATTEL_QINGGONG_2,
        HK_BATTEL_QINGGONG_3,
        HK_BATTEL_QINGGONG_4,
        HK_BATTEL_QINGGONG_5,
        HK_BATTEL_QINGGONG_6,
        HK_BATTEL_QINGGONG_7,
        HK_BATTEL_QINGGONG_8,
        HK_BATTEL_QINGGONG_9,

        HK_BATTEL_SKILL_1,
        HK_BATTEL_SKILL_2,
        HK_BATTEL_SKILL_3,
        HK_BATTEL_SKILL_4,
        HK_BATTEL_SKILL_5,
        HK_BATTEL_SKILL_6,
        HK_BATTEL_SKILL_7,
        HK_BATTEL_SKILL_8,
        HK_BATTEL_SKILL_9,

        HK_BATTEL_SPECIAL_1,
        HK_BATTEL_SPECIAL_2,
        HK_BATTEL_SPECIAL_3,
        HK_BATTEL_SPECIAL_4,
        HK_BATTEL_SPECIAL_5,
        HK_BATTEL_SPECIAL_6,
        HK_BATTEL_SPECIAL_7,
        HK_BATTEL_SPECIAL_8,
        HK_BATTEL_SPECIAL_9
    };


    public class Settings : UnityModManager.ModSettings
    {
        public static readonly Dictionary<HK_TYPE, string> hotkeyNames = new Dictionary<HK_TYPE, string>()
        {
                {HK_TYPE.HK_CLOSE, "关闭窗口快捷键"},
                {HK_TYPE.HK_COMFIRM, "确认按键1"},
                {HK_TYPE.HK_CONFIRM2, "确认按键2"},

                {HK_TYPE.HK_UP, "向上移动"},
                {HK_TYPE.HK_LEFT, "向左移动"},
                {HK_TYPE.HK_DOWN, "向下移动"},
                {HK_TYPE.HK_RIGHT, "向右移动"},
                {HK_TYPE.HK_UP2, "向上移动2"},
                {HK_TYPE.HK_LEFT2, "向左移动2"},
                {HK_TYPE.HK_DOWN2, "向下移动2"},
                {HK_TYPE.HK_RIGHT2, "向右移动2"},

                {HK_TYPE.HK_ACTORMENU, "打开人物界面"},
                {HK_TYPE.HK_VILLAGE, "打开太吾村产业地图"},
                {HK_TYPE.HK_VILLAGE_LOCAL, "打开本地产业地图"},
                {HK_TYPE.HK_WORLDMAP, "打开世界地图"},

                {HK_TYPE.HK_HEAL, "进行治疗"},
                {HK_TYPE.HK_POISON, "进行驱毒"},
                {HK_TYPE.HK_GATHER_FOOD, "收集食材"},
                {HK_TYPE.HK_GATHER_MINERAL, "收集金石"},
                {HK_TYPE.HK_GATHER_HERB, "收集草药"},
                {HK_TYPE.HK_GATHER_MONEY, "收集银钱"},
                {HK_TYPE.HK_GATHER_CLOTH, "收集织物"},
                {HK_TYPE.HK_GATHER_WOOD, "收集木材"},
                {HK_TYPE.HK_VISITEVENT, "访问奇遇"},

                {HK_TYPE.HK_BATTEL_QINGGONG_1, "施放轻功技能1"},
                {HK_TYPE.HK_BATTEL_QINGGONG_2, "施放轻功技能2"},
                {HK_TYPE.HK_BATTEL_QINGGONG_3, "施放轻功技能3"},
                {HK_TYPE.HK_BATTEL_QINGGONG_4, "施放轻功技能4"},
                {HK_TYPE.HK_BATTEL_QINGGONG_5, "施放轻功技能5"},
                {HK_TYPE.HK_BATTEL_QINGGONG_6, "施放轻功技能6"},
                {HK_TYPE.HK_BATTEL_QINGGONG_7, "施放轻功技能7"},
                {HK_TYPE.HK_BATTEL_QINGGONG_8, "施放轻功技能8"},
                {HK_TYPE.HK_BATTEL_QINGGONG_9, "施放轻功技能9"},

                {HK_TYPE.HK_BATTEL_SKILL_1, "施放战斗技能1"},
                {HK_TYPE.HK_BATTEL_SKILL_2, "施放战斗技能2"},
                {HK_TYPE.HK_BATTEL_SKILL_3, "施放战斗技能3"},
                {HK_TYPE.HK_BATTEL_SKILL_4, "施放战斗技能4"},
                {HK_TYPE.HK_BATTEL_SKILL_5, "施放战斗技能5"},
                {HK_TYPE.HK_BATTEL_SKILL_6, "施放战斗技能6"},
                {HK_TYPE.HK_BATTEL_SKILL_7, "施放战斗技能7"},
                {HK_TYPE.HK_BATTEL_SKILL_8, "施放战斗技能8"},
                {HK_TYPE.HK_BATTEL_SKILL_9, "施放战斗技能9"},

                {HK_TYPE.HK_BATTEL_SPECIAL_1, "施放特殊技能1"},
                {HK_TYPE.HK_BATTEL_SPECIAL_2, "施放特殊技能2"},
                {HK_TYPE.HK_BATTEL_SPECIAL_3, "施放特殊技能3"},
                {HK_TYPE.HK_BATTEL_SPECIAL_4, "施放特殊技能4"},
                {HK_TYPE.HK_BATTEL_SPECIAL_5, "施放特殊技能5"},
                {HK_TYPE.HK_BATTEL_SPECIAL_6, "施放特殊技能6"},
                {HK_TYPE.HK_BATTEL_SPECIAL_7, "施放特殊技能7"},
                {HK_TYPE.HK_BATTEL_SPECIAL_8, "施放特殊技能8"},
                {HK_TYPE.HK_BATTEL_SPECIAL_9, "施放特殊技能9"},
        };

        public static readonly int nHotkeys = 51;

        public SerializableDictionary<HK_TYPE, KeyCode> hotkeys = Settings.GetDefaultHotKeys();
        public MODIFIER_KEY_TYPE qinggong_modifier_key = MODIFIER_KEY_TYPE.MKT_SHIFT;
        public MODIFIER_KEY_TYPE special_modifierkey = MODIFIER_KEY_TYPE.MKT_ALT;
        public bool escAsLastOption = true;
        public bool useNumpadKeysInMessageWindow = false;

        public bool enable_close = true;
        [XmlIgnore]
        public UnityModManager.ModEntry modee;


        public static bool testModifierKey(MODIFIER_KEY_TYPE key)
        {
            switch (key)
            {
                case MODIFIER_KEY_TYPE.MKT_SHIFT:
                    return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                case MODIFIER_KEY_TYPE.MKT_ALT:
                    return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
                case MODIFIER_KEY_TYPE.MKT_CTRL:
                    return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                default:
                    return false;
            }
        }


        public static SerializableDictionary<HK_TYPE, KeyCode> GetDefaultHotKeys()
        {
            var hotkeys = new SerializableDictionary<HK_TYPE, KeyCode>()
            {
                {HK_TYPE.HK_CLOSE, KeyCode.Escape},
                {HK_TYPE.HK_COMFIRM, KeyCode.Space},
                {HK_TYPE.HK_CONFIRM2,  KeyCode.Return},

                {HK_TYPE.HK_UP, KeyCode.W},
                {HK_TYPE.HK_LEFT,  KeyCode.A},
                {HK_TYPE.HK_DOWN,  KeyCode.S},
                {HK_TYPE.HK_RIGHT,  KeyCode.D},
                {HK_TYPE.HK_UP2, KeyCode.UpArrow},
                {HK_TYPE.HK_LEFT2,  KeyCode.LeftArrow},
                {HK_TYPE.HK_DOWN2,  KeyCode.DownArrow},
                {HK_TYPE.HK_RIGHT2,  KeyCode.RightArrow},

                {HK_TYPE.HK_ACTORMENU,  KeyCode.C},
                {HK_TYPE.HK_VILLAGE,  KeyCode.P},
                {HK_TYPE.HK_VILLAGE_LOCAL,  KeyCode.L},
                {HK_TYPE.HK_WORLDMAP,  KeyCode.M},

                {HK_TYPE.HK_HEAL,  KeyCode.H},
                {HK_TYPE.HK_POISON,  KeyCode.J},
                {HK_TYPE.HK_GATHER_FOOD,  KeyCode.E},
                {HK_TYPE.HK_GATHER_MINERAL,  KeyCode.R},
                {HK_TYPE.HK_GATHER_HERB,  KeyCode.T},
                {HK_TYPE.HK_GATHER_MONEY,  KeyCode.Y},
                {HK_TYPE.HK_GATHER_CLOTH,  KeyCode.U},
                {HK_TYPE.HK_GATHER_WOOD,  KeyCode.I},
                {HK_TYPE.HK_VISITEVENT,  KeyCode.F},

                {HK_TYPE.HK_BATTEL_QINGGONG_1,  KeyCode.Alpha1},
                {HK_TYPE.HK_BATTEL_QINGGONG_2,  KeyCode.Alpha2},
                {HK_TYPE.HK_BATTEL_QINGGONG_3,  KeyCode.Alpha3},
                {HK_TYPE.HK_BATTEL_QINGGONG_4,  KeyCode.Alpha4},
                {HK_TYPE.HK_BATTEL_QINGGONG_5,  KeyCode.Alpha5},
                {HK_TYPE.HK_BATTEL_QINGGONG_6,  KeyCode.Alpha6},
                {HK_TYPE.HK_BATTEL_QINGGONG_7,  KeyCode.Alpha7},
                {HK_TYPE.HK_BATTEL_QINGGONG_8,  KeyCode.Alpha8},
                {HK_TYPE.HK_BATTEL_QINGGONG_9,  KeyCode.Alpha9},

                {HK_TYPE.HK_BATTEL_SKILL_1,  KeyCode.Alpha1},
                {HK_TYPE.HK_BATTEL_SKILL_2,  KeyCode.Alpha2},
                {HK_TYPE.HK_BATTEL_SKILL_3,  KeyCode.Alpha3},
                {HK_TYPE.HK_BATTEL_SKILL_4,  KeyCode.Alpha4},
                {HK_TYPE.HK_BATTEL_SKILL_5,  KeyCode.Alpha5},
                {HK_TYPE.HK_BATTEL_SKILL_6,  KeyCode.Alpha6},
                {HK_TYPE.HK_BATTEL_SKILL_7,  KeyCode.Alpha7},
                {HK_TYPE.HK_BATTEL_SKILL_8,  KeyCode.Alpha8},
                {HK_TYPE.HK_BATTEL_SKILL_9,  KeyCode.Alpha9},

                {HK_TYPE.HK_BATTEL_SPECIAL_1,  KeyCode.Alpha1},
                {HK_TYPE.HK_BATTEL_SPECIAL_2,  KeyCode.Alpha2},
                {HK_TYPE.HK_BATTEL_SPECIAL_3,  KeyCode.Alpha3},
                {HK_TYPE.HK_BATTEL_SPECIAL_4,  KeyCode.Alpha4},
                {HK_TYPE.HK_BATTEL_SPECIAL_5,  KeyCode.Alpha5},
                {HK_TYPE.HK_BATTEL_SPECIAL_6,  KeyCode.Alpha6},
                {HK_TYPE.HK_BATTEL_SPECIAL_7,  KeyCode.Alpha7},
                {HK_TYPE.HK_BATTEL_SPECIAL_8,  KeyCode.Alpha8},
                {HK_TYPE.HK_BATTEL_SPECIAL_9,  KeyCode.Alpha9},
            };

            foreach (var kv in hotkeys)
                Main.Logger.Log($"Hotkey: {kv.Key}, key: {kv.Value}, desc: {Settings.hotkeyNames[kv.Key]}");

            return hotkeys;
        }


        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }


        public void Save()
        {
            Main.Logger.Log("hotkey saved");
            Save(modee);
        }
    }


    public static class Main
    {
        public static List<HK_TYPE> mainskilllist = new List<HK_TYPE>()
        {
            HK_TYPE.HK_BATTEL_SKILL_1,
            HK_TYPE.HK_BATTEL_SKILL_2,
            HK_TYPE.HK_BATTEL_SKILL_3,
            HK_TYPE.HK_BATTEL_SKILL_4,
            HK_TYPE.HK_BATTEL_SKILL_5,
            HK_TYPE.HK_BATTEL_SKILL_6,
            HK_TYPE.HK_BATTEL_SKILL_7,
            HK_TYPE.HK_BATTEL_SKILL_8,
            HK_TYPE.HK_BATTEL_SKILL_9,
        };

        public static List<HK_TYPE> qinggongskilllist = new List<HK_TYPE>()
        {
            HK_TYPE.HK_BATTEL_QINGGONG_1,
            HK_TYPE.HK_BATTEL_QINGGONG_2,
            HK_TYPE.HK_BATTEL_QINGGONG_3,
            HK_TYPE.HK_BATTEL_QINGGONG_4,
            HK_TYPE.HK_BATTEL_QINGGONG_5,
            HK_TYPE.HK_BATTEL_QINGGONG_6,
            HK_TYPE.HK_BATTEL_QINGGONG_7,
            HK_TYPE.HK_BATTEL_QINGGONG_8,
            HK_TYPE.HK_BATTEL_QINGGONG_9,
        };

        public static List<HK_TYPE> specialskilllist = new List<HK_TYPE>()
        {
            HK_TYPE.HK_BATTEL_SPECIAL_1,
            HK_TYPE.HK_BATTEL_SPECIAL_2,
            HK_TYPE.HK_BATTEL_SPECIAL_3,
            HK_TYPE.HK_BATTEL_SPECIAL_4,
            HK_TYPE.HK_BATTEL_SPECIAL_5,
            HK_TYPE.HK_BATTEL_SPECIAL_6,
            HK_TYPE.HK_BATTEL_SPECIAL_7,
            HK_TYPE.HK_BATTEL_SPECIAL_8,
            HK_TYPE.HK_BATTEL_SPECIAL_9,
        };

        public static bool enabled;
        public static Settings settings;

        public static bool notify_reset_key = false;
        public static GameObject _go_gongfatree = null;

        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool binding_key = false;
        static HK_TYPE current_binding_key = HK_TYPE.HK_NONE;
        static KeyCode last_key_code = KeyCode.None;


        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            settings = Settings.Load<Settings>(modEntry);
            settings.modee = modEntry;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Main.Logger.Log("Hotkey count: " + Main.settings.hotkeys.Count);
            if (Main.settings.hotkeys.Count != Settings.nHotkeys)
            {
                Main.settings.hotkeys = Settings.GetDefaultHotKeys();
                notify_reset_key = true;
                Main.settings.Save();
                Main.Logger.Log("Hotkeys reinitialized: " + Main.settings.hotkeys.Count);
            }
            return true;
        }


        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value) return false;
            enabled = value;
            return true;
        }


        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            processKeyPress();
            GUILayout.BeginVertical("box");
            settings.enable_close = GUILayout.Toggle(settings.enable_close, "是否使用本Mod的Esc/鼠标右键关闭功能（需要重启游戏生效）");
            settings.escAsLastOption = GUILayout.Toggle(settings.escAsLastOption, "是否在对话窗口使用Esc/鼠标右键选择最后一个选项");
            settings.useNumpadKeysInMessageWindow = GUILayout.Toggle(settings.useNumpadKeysInMessageWindow, "是否在对话窗口增加小键盘选择功能");

            GUILayout.Label("战斗中释放轻功技能的装饰键");
            settings.qinggong_modifier_key = (MODIFIER_KEY_TYPE)GUILayout.SelectionGrid((int)settings.qinggong_modifier_key,
                new string[] { "Shift", "Ctrl", "Alt" }, 3);
            GUILayout.Label("战斗中释放绝技的装饰键");
            settings.special_modifierkey = (MODIFIER_KEY_TYPE)GUILayout.SelectionGrid((int)settings.special_modifierkey,
                new string[] { "Shift", "Ctrl", "Alt" }, 3);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box", new GUILayoutOption[] { GUILayout.MinWidth(700.0f), GUILayout.MaxWidth(700.0f) });
            var keys = settings.hotkeys.Keys.ToArray();

            for (int index = 0; index < keys.Length; index++)
            {
                HK_TYPE key = keys[index];
                KeyCode keyCode = settings.hotkeys[key];
                string desc = Settings.hotkeyNames[key];
                // Do not allow modifying the close key 
                if (key == HK_TYPE.HK_CLOSE)
                {
                    settings.hotkeys[key] = KeyCode.Escape;
                }
                else
                {
                    renderHK_GUI(key, keyCode, desc);
                }
            }
            GUILayout.EndVertical();
        }


        /// <summary>
        /// 每次渲染 GUI 时处理绑定事件
        /// 若处于绑定事件中，且按下了键盘上的键，则设置快捷键并结束绑定事件
        /// </summary>
        private static void processKeyPress()
        {
            Event e = Event.current;
            if (e.isKey && Input.anyKeyDown)
            {
                if (Main.binding_key)
                {
                    Main.Logger.Log("Detected key while binding key, key code: " + e.keyCode);
                    if (Settings.hotkeyNames.ContainsKey(Main.current_binding_key))
                    {
                        Main.settings.hotkeys[Main.current_binding_key] = e.keyCode;
                    }
                    else
                    {
                        Main.Logger.Log("Error finding hotkey for " + Main.current_binding_key);
                    }
                    Main.binding_key = false;
                    Main.settings.Save();
                }
            }
        }


        private static void renderHK_GUI(HK_TYPE key, KeyCode keyCode, string desc)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(desc, new GUILayoutOption[] { GUILayout.MinWidth(200.0f), GUILayout.MaxWidth(200.0f) });

            string text = (Main.binding_key && Main.current_binding_key == key) ? "请按下需要的按键" : keyCode.ToString();
            bool ret = GUILayout.Button(text, new GUILayoutOption[] { GUILayout.MinWidth(200.0f), GUILayout.MaxWidth(200.0f) });

            if (ret)
            {
                // 点击绑定按钮时正处于绑定事件中
                if (Main.binding_key)
                {
                    // 若两次的按钮相同，则判断为清空快捷键
                    if (key == Main.current_binding_key)
                    {
                        Main.settings.hotkeys[Main.current_binding_key] = KeyCode.None;
                        Main.binding_key = false;
                        Main.settings.Save();
                    }
                    // 若两次的按钮不同，则上次的按钮对应的快捷键还原，当前的按钮开始进入绑定状态
                    else
                    {
                        Main.settings.hotkeys[Main.current_binding_key] = Main.last_key_code;
                        Main.current_binding_key = key;
                        Main.last_key_code = keyCode;
                        binding_key = true;
                    }
                }
                // 点击绑定按钮时不处于绑定事件中，则当前按钮开始进入绑定状态
                else
                {
                    Main.current_binding_key = key;
                    Main.last_key_code = keyCode;
                    binding_key = true;
                }
            }
            GUILayout.EndHorizontal();
        }


        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }


        public static bool GetKeyDown(HK_TYPE key)
        {
            return settings.hotkeys.ContainsKey(key) && Input.GetKeyDown(settings.hotkeys[key]);
        }


        public static bool GetKey(HK_TYPE key)
        {
            return settings.hotkeys.ContainsKey(key) && Input.GetKey(settings.hotkeys[key]);
        }


        public static int GetKeys(List<HK_TYPE> keys)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                if (settings.hotkeys.ContainsKey(keys[i]))
                {
                    if (Input.GetKey(settings.hotkeys[keys[i]]))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }


        public static string getmainskill_keystr(int index)
        {
            string ret = "";
            if (index < mainskilllist.Count)
            {
                ret = settings.hotkeys[mainskilllist[index]].ToString();
                ret = ret.Replace("Alpha", "");
            }
            return ret;
        }


        public static string getmodifyerstr(MODIFIER_KEY_TYPE mkt)
        {
            switch (mkt)
            {
                case MODIFIER_KEY_TYPE.MKT_ALT:
                    return "A+";
                case MODIFIER_KEY_TYPE.MKT_SHIFT:
                    return "S+";
                case MODIFIER_KEY_TYPE.MKT_CTRL:
                    return "C+";
                default:
                    return "";
            }
        }


        public static string getqinggongskill_keystr(int index)
        {
            string ret = "";
            ret = ret + getmodifyerstr(Main.settings.qinggong_modifier_key);
            if (index < qinggongskilllist.Count)
            {
                ret = settings.hotkeys[mainskilllist[index]].ToString();
                ret = ret.Replace("Alpha", "");
            }
            return ret;
        }


        public static string getspecialskill_keystr(int index)
        {
            string ret = "";
            ret = ret + getmodifyerstr(Main.settings.special_modifierkey);
            if (index < mainskilllist.Count)
            {
                ret = settings.hotkeys[mainskilllist[index]].ToString();
                ret = ret.Replace("Alpha", "");
            }
            return ret;
        }
    }


    public class CloseComponent : MonoBehaviour
    {
        private Action OnClose = null;


        public void SetActionOnClose(Action OnClose)
        {
            this.OnClose = OnClose;
        }


        public void Update()
        {
            if (!Main.enabled || Main.binding_key || !Main.settings.enable_close) return;
            if (this.OnClose == null) return;
            if (!Main.GetKeyDown(HK_TYPE.HK_CLOSE) && !Input.GetMouseButtonDown(1)) return;

            this.OnClose();
        }
    }


    public class ConfirmComponent : MonoBehaviour
    {
        private Action OnConfirm = null;


        public void SetActionOnConfirm(Action OnConfirm)
        {
            this.OnConfirm = OnConfirm;
        }


        public void Update()
        {
            if (!Main.enabled || Main.binding_key) return;
            if (this.OnConfirm == null) return;
            if (!Main.GetKeyDown(HK_TYPE.HK_COMFIRM) && !Main.GetKeyDown(HK_TYPE.HK_CONFIRM2)) return;

            this.OnConfirm();
        }
    }


    /// <summary>
    ///  重置配置提醒
    /// </summary>
    [HarmonyPatch(typeof(MainMenu), "CloseStartMask")]
    public static class MainMenu_CloseStartMask_Patch
    {
        private static void Prefix()
        {
            if (!Main.enabled) return;

            if (Main.notify_reset_key)
            {
                Main.notify_reset_key = false;
                Main.Logger.Log("display keyboard shotcut reset message...");
                DateFile.instance.massageDate[8013][0] = "键盘快捷键配置更新|由于键盘快捷键升级，配置文件已经重置为默认，如有需要请重新配置快捷键";
            }
        }
    }
}
