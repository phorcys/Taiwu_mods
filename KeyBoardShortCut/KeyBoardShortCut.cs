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
        public SerializableDictionary<HK_TYPE, KeyValuePair<KeyCode, string>> hotkeys;
        public MODIFIER_KEY_TYPE qinggong_modifier_key = MODIFIER_KEY_TYPE.MKT_SHIFT;
        public MODIFIER_KEY_TYPE special_modifierkey = MODIFIER_KEY_TYPE.MKT_ALT;
        public bool escAsLastOption = true;
        public bool useNumpadKeysInMessageWindow = false;

        public bool enable_close = true;
        [XmlIgnore]
        public UnityModManager.ModEntry modee;


        public Settings()
        {
            initDefaultHotKeys();
        }


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


        public void initDefaultHotKeys()
        {
            hotkeys = new SerializableDictionary<HK_TYPE, KeyValuePair<KeyCode, string>>()
            {
                {HK_TYPE.HK_CLOSE, new KeyValuePair<KeyCode, string>(KeyCode.Escape, "关闭窗口快捷键") },
                {HK_TYPE.HK_COMFIRM, new KeyValuePair<KeyCode, string>(KeyCode.Space, "确认按键1" ) },
                {HK_TYPE.HK_CONFIRM2, new KeyValuePair<KeyCode, string>( KeyCode.Return, "确认按键2")  },

                {HK_TYPE.HK_UP, new KeyValuePair<KeyCode, string>(KeyCode.W , "向上移动") },
                {HK_TYPE.HK_LEFT, new KeyValuePair<KeyCode, string>( KeyCode.A, "向左移动")},
                {HK_TYPE.HK_DOWN, new KeyValuePair<KeyCode, string>( KeyCode.S, "向下移动")},
                {HK_TYPE.HK_RIGHT, new KeyValuePair<KeyCode, string>( KeyCode.D, "向右移动")},
                {HK_TYPE.HK_UP2, new KeyValuePair<KeyCode, string>(KeyCode.UpArrow , "向上移动2") },
                {HK_TYPE.HK_LEFT2, new KeyValuePair<KeyCode, string>( KeyCode.LeftArrow, "向左移动2")},
                {HK_TYPE.HK_DOWN2, new KeyValuePair<KeyCode, string>( KeyCode.DownArrow, "向下移动2")},
                {HK_TYPE.HK_RIGHT2, new KeyValuePair<KeyCode, string>( KeyCode.RightArrow, "向右移动2")},

                {HK_TYPE.HK_ACTORMENU, new KeyValuePair<KeyCode, string>( KeyCode.C, "打开人物界面")},
                {HK_TYPE.HK_VILLAGE, new KeyValuePair<KeyCode, string>( KeyCode.P, "打开太吾村产业地图")},
                {HK_TYPE.HK_VILLAGE_LOCAL, new KeyValuePair<KeyCode, string>( KeyCode.L, "打开本地产业地图")},
                {HK_TYPE.HK_WORLDMAP, new KeyValuePair<KeyCode, string>( KeyCode.M, "打开世界地图")},

                {HK_TYPE.HK_HEAL, new KeyValuePair<KeyCode, string>( KeyCode.H, "进行治疗")},
                {HK_TYPE.HK_POISON, new KeyValuePair<KeyCode, string>( KeyCode.J, "进行驱毒")},
                {HK_TYPE.HK_GATHER_FOOD, new KeyValuePair<KeyCode, string>( KeyCode.E, "收集食材")},
                {HK_TYPE.HK_GATHER_MINERAL, new KeyValuePair<KeyCode, string>( KeyCode.R, "收集金石")},
                {HK_TYPE.HK_GATHER_HERB, new KeyValuePair<KeyCode, string>( KeyCode.T, "收集草药")},
                {HK_TYPE.HK_GATHER_MONEY, new KeyValuePair<KeyCode, string>( KeyCode.Y, "收集银钱")},
                {HK_TYPE.HK_GATHER_CLOTH, new KeyValuePair<KeyCode, string>( KeyCode.U, "收集织物")},
                {HK_TYPE.HK_GATHER_WOOD, new KeyValuePair<KeyCode, string>( KeyCode.I, "收集木材")},
                {HK_TYPE.HK_VISITEVENT, new KeyValuePair<KeyCode, string>( KeyCode.F, "访问奇遇")},

                {HK_TYPE.HK_BATTEL_QINGGONG_1, new KeyValuePair<KeyCode, string>( KeyCode.Alpha1, "施放轻功技能1")},
                {HK_TYPE.HK_BATTEL_QINGGONG_2, new KeyValuePair<KeyCode, string>( KeyCode.Alpha2, "施放轻功技能2")},
                {HK_TYPE.HK_BATTEL_QINGGONG_3, new KeyValuePair<KeyCode, string>( KeyCode.Alpha3, "施放轻功技能3")},
                {HK_TYPE.HK_BATTEL_QINGGONG_4, new KeyValuePair<KeyCode, string>( KeyCode.Alpha4, "施放轻功技能4")},
                {HK_TYPE.HK_BATTEL_QINGGONG_5, new KeyValuePair<KeyCode, string>( KeyCode.Alpha5, "施放轻功技能5")},
                {HK_TYPE.HK_BATTEL_QINGGONG_6, new KeyValuePair<KeyCode, string>( KeyCode.Alpha6, "施放轻功技能6")},
                {HK_TYPE.HK_BATTEL_QINGGONG_7, new KeyValuePair<KeyCode, string>( KeyCode.Alpha7, "施放轻功技能7")},
                {HK_TYPE.HK_BATTEL_QINGGONG_8, new KeyValuePair<KeyCode, string>( KeyCode.Alpha8, "施放轻功技能8")},
                {HK_TYPE.HK_BATTEL_QINGGONG_9, new KeyValuePair<KeyCode, string>( KeyCode.Alpha9, "施放轻功技能9")},

                {HK_TYPE.HK_BATTEL_SKILL_1, new KeyValuePair<KeyCode, string>( KeyCode.Alpha1, "施放战斗技能1")},
                {HK_TYPE.HK_BATTEL_SKILL_2, new KeyValuePair<KeyCode, string>( KeyCode.Alpha2, "施放战斗技能2")},
                {HK_TYPE.HK_BATTEL_SKILL_3, new KeyValuePair<KeyCode, string>( KeyCode.Alpha3, "施放战斗技能3")},
                {HK_TYPE.HK_BATTEL_SKILL_4, new KeyValuePair<KeyCode, string>( KeyCode.Alpha4, "施放战斗技能4")},
                {HK_TYPE.HK_BATTEL_SKILL_5, new KeyValuePair<KeyCode, string>( KeyCode.Alpha5, "施放战斗技能5")},
                {HK_TYPE.HK_BATTEL_SKILL_6, new KeyValuePair<KeyCode, string>( KeyCode.Alpha6, "施放战斗技能6")},
                {HK_TYPE.HK_BATTEL_SKILL_7, new KeyValuePair<KeyCode, string>( KeyCode.Alpha7, "施放战斗技能7")},
                {HK_TYPE.HK_BATTEL_SKILL_8, new KeyValuePair<KeyCode, string>( KeyCode.Alpha8, "施放战斗技能8")},
                {HK_TYPE.HK_BATTEL_SKILL_9, new KeyValuePair<KeyCode, string>( KeyCode.Alpha9, "施放战斗技能9")},

                {HK_TYPE.HK_BATTEL_SPECIAL_1, new KeyValuePair<KeyCode, string>( KeyCode.Alpha1, "施放特殊技能1")},
                {HK_TYPE.HK_BATTEL_SPECIAL_2, new KeyValuePair<KeyCode, string>( KeyCode.Alpha2, "施放特殊技能2")},
                {HK_TYPE.HK_BATTEL_SPECIAL_3, new KeyValuePair<KeyCode, string>( KeyCode.Alpha3, "施放特殊技能3")},
                {HK_TYPE.HK_BATTEL_SPECIAL_4, new KeyValuePair<KeyCode, string>( KeyCode.Alpha4, "施放特殊技能4")},
                {HK_TYPE.HK_BATTEL_SPECIAL_5, new KeyValuePair<KeyCode, string>( KeyCode.Alpha5, "施放特殊技能5")},
                {HK_TYPE.HK_BATTEL_SPECIAL_6, new KeyValuePair<KeyCode, string>( KeyCode.Alpha6, "施放特殊技能6")},
                {HK_TYPE.HK_BATTEL_SPECIAL_7, new KeyValuePair<KeyCode, string>( KeyCode.Alpha7, "施放特殊技能7")},
                {HK_TYPE.HK_BATTEL_SPECIAL_8, new KeyValuePair<KeyCode, string>( KeyCode.Alpha8, "施放特殊技能8")},
                {HK_TYPE.HK_BATTEL_SPECIAL_9, new KeyValuePair<KeyCode, string>( KeyCode.Alpha9, "施放特殊技能9")},
            };

            foreach (var kv in hotkeys)
            {
                Main.Logger.Log(String.Format("Hotkey : {0} , key: {1}  , desc : {2}", kv.Key, kv.Value.Key, kv.Value.Value));
            }
            Main.Logger.Log("hotkey count:" + hotkeys.Count);
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
            Main.Logger.Log("hotkey count:" + Main.settings.hotkeys.Count);
            Main.Logger.Log("hotkey enable close with esc or right button:" + Main.settings.enable_close);
            settings.modee = modEntry;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Main.Logger.Log("hotkey count:" + Main.settings.hotkeys.Count);
            if (Main.settings.hotkeys.Count < 51)
            {
                Main.settings.initDefaultHotKeys();
                notify_reset_key = true;
                Main.settings.Save();
                Main.Logger.Log("hotkey  new count:" + Main.settings.hotkeys.Count);
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
            Main.Logger.Log("hotkey count:" + Main.settings.hotkeys.Count);
            processKeyPress();
            GUILayout.BeginVertical("box");
            settings.enable_close = GUILayout.Toggle(settings.enable_close, "是否使用本Mod的Esc/鼠标右键关闭功能（需要重启游戏生效）");
            settings.escAsLastOption = GUILayout.Toggle(settings.escAsLastOption, "是否在对话窗口使用Esc/鼠标右键选择最后一个选项");
            settings.useNumpadKeysInMessageWindow = GUILayout.Toggle(settings.useNumpadKeysInMessageWindow, "是否在对话窗口增加小键盘选择功能");

            GUILayout.Label("战斗中释放轻功技能的装饰键");
            settings.qinggong_modifier_key = (MODIFIER_KEY_TYPE)GUILayout.SelectionGrid((int)settings.qinggong_modifier_key, new string[] { "Shift", "Ctrl", "Alt" }, 3);
            GUILayout.Label("战斗中释放绝技的装饰键");
            settings.special_modifierkey = (MODIFIER_KEY_TYPE)GUILayout.SelectionGrid((int)settings.special_modifierkey, new string[] { "Shift", "Ctrl", "Alt" }, 3);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box", new GUILayoutOption[] { GUILayout.MinWidth(700.0f), GUILayout.MaxWidth(700.0f) });
            var keys = settings.hotkeys.Keys.ToArray();

            for (int index = 0; index < keys.Length; index++)
            {
                var key = keys[index];
                var value = settings.hotkeys[key];
                // Do not allow modifying the close key 
                if (key == HK_TYPE.HK_CLOSE)
                {
                    settings.hotkeys[key] = new KeyValuePair<KeyCode, string>(KeyCode.Escape, value.Value);
                }
                else
                {
                    renderHK_GUI(key, value);
                }
            }
            GUILayout.EndVertical();
        }


        private static void processKeyPress()
        {
            Event e = Event.current;
            if (e.isKey && Input.anyKeyDown == true)
            {

                if (binding_key == true)
                {
                    Main.Logger.Log("Detected key  while binding key ,key code: " + e.keyCode);
                    if (settings.hotkeys.ContainsKey(current_binding_key))
                    {
                        var kv = settings.hotkeys[current_binding_key];
                        settings.hotkeys[current_binding_key] = new KeyValuePair<KeyCode, string>(e.keyCode, kv.Value);

                    }
                    else
                    {
                        Main.Logger.Log(" error finding hotkey  for " + current_binding_key);
                    }
                    binding_key = false;
                    settings.Save();
                }
            }

        }


        private static void renderHK_GUI(HK_TYPE key, KeyValuePair<KeyCode, string> value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(value.Value, new GUILayoutOption[] { GUILayout.MinWidth(200.0f), GUILayout.MaxWidth(200.0f) });
            var ret = GUILayout.Button(value.Key == KeyCode.None ? "请按下需要的按键" : value.Key.ToString(), new GUILayoutOption[] { GUILayout.MinWidth(200.0f), GUILayout.MaxWidth(200.0f) });
            if (ret == true)
            {
                if (binding_key == true)
                {
                    //重复绑定
                    //恢复上次绑定key的默认值
                    settings.hotkeys[current_binding_key] = new KeyValuePair<KeyCode, string>(last_key_code, settings.hotkeys[current_binding_key].Value);
                }
                //保存临时值
                current_binding_key = key;
                last_key_code = value.Key;
                settings.hotkeys[key] = new KeyValuePair<KeyCode, string>(KeyCode.None, value.Value);
                binding_key = true;
            }
            GUILayout.EndHorizontal();
        }


        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }


        public static bool GetKeyDown(HK_TYPE hotkey_name)
        {
            if (settings.hotkeys.ContainsKey(hotkey_name))
            {
                if (Input.GetKeyDown(settings.hotkeys[hotkey_name].Key))
                {
                    return true;
                }
            }
            return false;
        }


        public static bool GetKey(HK_TYPE hotkey_name)
        {
            if (settings.hotkeys.ContainsKey(hotkey_name))
            {
                if (Input.GetKey(settings.hotkeys[hotkey_name].Key))
                {
                    return true;
                }
            }
            return false;
        }


        public static int GetKeyListDown(List<HK_TYPE> hotkey_name_list)
        {
            for (int i = 0; i < hotkey_name_list.Count; i++)
            {
                if (settings.hotkeys.ContainsKey(hotkey_name_list[i]))
                {
                    if (Input.GetKey(settings.hotkeys[hotkey_name_list[i]].Key))
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
                ret = settings.hotkeys[mainskilllist[index]].Key.ToString();
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
                ret = settings.hotkeys[mainskilllist[index]].Key.ToString();
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
                ret = settings.hotkeys[mainskilllist[index]].Key.ToString();
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
            if (!Main.enabled || Main.binding_key ||!Main.settings.enable_close) return;
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
