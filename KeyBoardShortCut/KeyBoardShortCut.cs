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
        NONE = 0,
        CLOSE = 1,
        COMFIRM,
        CONFIRM2,

        INCREASE,
        DECREASE,

        NAV_RIGHT,
        NAV_LEFT,

        REMOVE_ITEM,

        ACTORMENU,
        VILLAGE,
        VILLAGE_LOCAL,
        WORLD_MAP,
        GONGFA_TREE,
        STORY,
        NAME_SCAN,

        MAP_MOVE,

    };

    public class Settings : UnityModManager.ModSettings
    {
        public static readonly Dictionary<HK_TYPE, string> hotkeyNames = new Dictionary<HK_TYPE, string>()
        {
                {HK_TYPE.CLOSE, "关闭窗口快捷键"},
                {HK_TYPE.COMFIRM, "确认按键1"},
                {HK_TYPE.CONFIRM2, "确认按键2"},

                {HK_TYPE.INCREASE, "增加"},
                {HK_TYPE.DECREASE, "减少"},

                {HK_TYPE.NAV_RIGHT, "下一个子页面"},
                {HK_TYPE.NAV_LEFT, "上一个子页面"},

                {HK_TYPE.REMOVE_ITEM, "移除当前物品/功法"},

                {HK_TYPE.ACTORMENU, "打开人物界面"},
                {HK_TYPE.VILLAGE, "打开太吾村产业地图"},
                {HK_TYPE.VILLAGE_LOCAL, "打开本地产业地图"},
                {HK_TYPE.WORLD_MAP, "打开世界地图"},
                {HK_TYPE.GONGFA_TREE, "打开功法树"},
                {HK_TYPE.STORY, "打开本地奇遇"},
                {HK_TYPE.NAME_SCAN, "打开人名搜索"},

                {HK_TYPE.MAP_MOVE, "继续移动"},

        };

        public static readonly int nHotkeys = hotkeyNames.Count;

        public SerializableDictionary<HK_TYPE, KeyCode> hotkeys = Settings.GetDefaultHotKeys();

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
                {HK_TYPE.CLOSE, KeyCode.Escape},
                {HK_TYPE.COMFIRM, KeyCode.Space},
                {HK_TYPE.CONFIRM2,  KeyCode.Return},

                {HK_TYPE.INCREASE, KeyCode.D},
                {HK_TYPE.DECREASE,  KeyCode.A},
                {HK_TYPE.NAV_RIGHT, KeyCode.E},
                {HK_TYPE.NAV_LEFT,  KeyCode.Q},

                {HK_TYPE.REMOVE_ITEM, KeyCode.R},

                {HK_TYPE.ACTORMENU,  KeyCode.C},
                {HK_TYPE.VILLAGE,  KeyCode.Q},
                {HK_TYPE.VILLAGE_LOCAL,  KeyCode.E},
                {HK_TYPE.WORLD_MAP,  KeyCode.M},
                {HK_TYPE.GONGFA_TREE,  KeyCode.F},
                {HK_TYPE.STORY,  KeyCode.Tab},
                {HK_TYPE.NAME_SCAN,  KeyCode.N},

                {HK_TYPE.MAP_MOVE,  KeyCode.G},

            };

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

        public static bool enabled;
        public static Settings settings;

        public static bool notify_reset_key = false;
        public static GameObject _go_gongfatree = null;

        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool binding_key = false;
        static HK_TYPE current_binding_key = HK_TYPE.NONE;
        static KeyCode last_key_code = KeyCode.None;

        public static bool on
        {
            get { return enabled && !binding_key; }
        }


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

            if (Main.settings.hotkeys.Count != Settings.nHotkeys)
            {
                Main.settings.hotkeys = Settings.GetDefaultHotKeys();
                notify_reset_key = true;
                Main.settings.Save();
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
            GUILayout.BeginVertical("box", new GUILayoutOption[] { GUILayout.MinWidth(700.0f), GUILayout.MaxWidth(700.0f) });
            var keys = settings.hotkeys.Keys.ToArray();

            for (int index = 0; index < keys.Length; index++)
            {
                HK_TYPE key = keys[index];
                KeyCode keyCode = settings.hotkeys[key];
                string desc = Settings.hotkeyNames[key];
                // Do not allow modifying the close key 
                if (key == HK_TYPE.CLOSE)
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
                    //Main.Logger.Log("Detected key while binding key, key code: " + e.keyCode);
                    if (Settings.hotkeyNames.ContainsKey(Main.current_binding_key))
                    {
                        Main.settings.hotkeys[Main.current_binding_key] = e.keyCode;
                    }
                    else
                    {
                        //Main.Logger.Log("Error finding hotkey for " + Main.current_binding_key);
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
        
    }


}
