using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Reflection.Emit;
using System.Xml.Serialization;

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
        public bool close_with_right_mouse_button = false;
        public SerializableDictionary<HK_TYPE, KeyValuePair<KeyCode, string>> hotkeys;
        public MODIFIER_KEY_TYPE qinggong_modifier_key = MODIFIER_KEY_TYPE.MKT_SHIFT;
        public MODIFIER_KEY_TYPE special_modifierkey = MODIFIER_KEY_TYPE.MKT_ALT;
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

            foreach(var kv in hotkeys)
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
        public static bool enabled;
        public static Settings settings;

        public static bool notify_reset_key = false;
        public static GameObject _go_gongfatree = null;

        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            settings = Settings.Load<Settings>(modEntry);
            Main.Logger.Log("hotkey count:" + Main.settings.hotkeys.Count);
            settings.modee = modEntry;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Main.Logger.Log("hotkey count:" + Main.settings.hotkeys.Count);
            if(Main.settings.hotkeys.Count < 51)
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

        public static bool binding_key = false;
        static HK_TYPE current_binding_key = HK_TYPE.HK_NONE;
        static KeyCode last_key_code = KeyCode.None;

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            Main.Logger.Log("hotkey count:" + Main.settings.hotkeys.Count);
            processKeyPress();
            GUILayout.BeginVertical("box");
            settings.close_with_right_mouse_button = GUILayout.Toggle(settings.close_with_right_mouse_button, "是否使用鼠标右键关闭窗口");
            
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
                renderHK_GUI(key, value);
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

    public class EscClose : UnityEngine.MonoBehaviour
    {
        protected Type ins;
        protected string mname;
        protected MethodInfo mi;
        protected FieldInfo fi;
        protected MonoBehaviour insobj;

        protected Func<bool> testret = null;

        public void setparam(Type type_ins, string methodname, Func<bool> tester)
        {
            Main.Logger.Log(this.GetType().ToString()+" Regist " + type_ins.ToString() + "  method :" + methodname);
            mname = methodname;
            ins = type_ins;
            mi = ins.GetMethod(mname);
            fi = ins.GetField("instance", BindingFlags.Static | BindingFlags.Public);
            insobj = fi.GetValue(null) as MonoBehaviour;

            testret = tester;
        }

        public void Update()
        {

            if (!Main.enabled && Main.binding_key)
            {
                return;
            }

            if (insobj.gameObject.activeInHierarchy == true 
                && (Main.GetKeyDown(HK_TYPE.HK_CLOSE) == true
                    || (Main.settings.close_with_right_mouse_button == true && Input.GetMouseButtonDown(1) == true))
                    )
            {

                if (testret() == true)
                {
                    Main.Logger.Log(this.GetType().ToString() + " Registed " + ins.ToString() + "  method :" + mname + "triggered , going to call ");
                    if (ins == typeof(ReadBook))
                    {
                        // ReadBook.CloseReadbookWindows need null as param
                        mi.Invoke(insobj, new object[] { null });
                    }
                    else
                    {
                        mi.Invoke(insobj, null);
                    }
                }
                
                
            }
        }
    }

    public class ConfirmConfirm :EscClose
    {
        public new void Update()
        {

            if (!Main.enabled && Main.binding_key)
            {
                return;
            }

            if (insobj.gameObject.activeInHierarchy == true
                && (Main.GetKeyDown(HK_TYPE.HK_COMFIRM) == true
                    || Main.GetKeyDown(HK_TYPE.HK_CONFIRM2) == true)
                    )
            {

                if (testret() == true)
                {
                    Main.Logger.Log(this.GetType().ToString() + " Registed " + ins.ToString() + "  method :" + mname + "triggered , going to call ");
                    if (ins == typeof(ReadBook))
                    {
                        // ReadBook.CloseReadbookWindows need null as param
                        mi.Invoke(insobj, new object[] { null });
                    }
                    else
                    {
                        mi.Invoke(insobj, null);
                    }
                }
            }
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
            if(Main.notify_reset_key == true)
            {
                Main.notify_reset_key = false;
                Main.Logger.Log("display  keyboard shotcut reset message...");
                DateFile.instance.massageDate[8013][0]="键盘快捷键配置更新|由于键盘快捷键升级，配置文件已经重置为默认，如有需要请重新配置快捷键";
            }
            
        }
    }


    /// <summary>
    ///  worldmap 事件
    /// </summary>
    [HarmonyPatch(typeof(WorldMapSystem), "Update")]
    public static class WorldMapSystem_Update_Patch
    {
        static MethodInfo GetMoveKey = typeof(WorldMapSystem).GetMethod("GetMoveKey",
                                            BindingFlags.NonPublic | BindingFlags.Instance);

        private static bool Prefix(WorldMapSystem __instance, bool ___moveButtonDown, bool ___isShowPartWorldMap)
        {
            if (!Main.enabled && Main.binding_key)
            {
                return true;
            }

            if (DateFile.instance.battleStart == false //无战斗
                && UIDate.instance.trunChangeImage[0].gameObject.activeSelf == false //非回合结算
                && SystemSetting.instance.SystemSettingWindow.activeInHierarchy == false) // 系统设置未开启
            {
                //处理关闭                                                         
                if (YesOrNoWindow.instance.yesOrNoIsShow == true && YesOrNoWindow.instance.isActiveAndEnabled == true)
                {
                    if ( (Main.GetKeyDown(HK_TYPE.HK_CLOSE) == true
                           || (Main.settings.close_with_right_mouse_button == true && Input.GetMouseButtonDown(1) == true))
                        && YesOrNoWindow.instance.no.isActiveAndEnabled == true)
                    {
                        YesOrNoWindow.instance.CloseYesOrNoWindow();
                        return false;
                    }
                    if (Main.GetKeyDown(HK_TYPE.HK_COMFIRM) == true || Main.GetKeyDown(HK_TYPE.HK_CONFIRM2) == true)
                    {
                        OnClick.instance.Index();
                        YesOrNoWindow.instance.CloseYesOrNoWindow();
                        return false;
                    }

                }

                //界面快捷键  人物/世界地图/村子地图
                if (YesOrNoWindow.instance.MaskShow() == false)  //无模态对话框
                {
                    if (Main.GetKeyDown(HK_TYPE.HK_ACTORMENU) && __instance.partWorldMapWindow.activeInHierarchy == false)
                    {
                        if (ActorMenu.instance.actorMenu.activeInHierarchy == false)
                        {
                            ActorMenu.instance.ShowActorMenu(false);
                            return false;
                        }
                        else
                        {
                            ActorMenu.instance.CloseActorMenu();
                            return false;
                        }
                    }
                    if (Main.GetKeyDown(HK_TYPE.HK_VILLAGE) && __instance.partWorldMapWindow.activeInHierarchy == false)
                    {
                        if (HomeSystem.instance.homeSystem.activeInHierarchy == false)
                        {
                            HomeSystem.instance.ShowHomeSystem(true);
                        }
                        else
                        {
                            HomeSystem.instance.CloseHomeSystem();
                            return false;
                        }

                    }
                    if (Main.GetKeyDown(HK_TYPE.HK_WORLDMAP))
                    {
                        if (__instance.partWorldMapWindow.activeInHierarchy == false)
                        {
                            WorldMapSystem.instance.ShowPartWorldMapWindow(DateFile.instance.mianWorldId);
                            return false;
                        }
                        else
                        {
                            if (Main._go_gongfatree == null || Main._go_gongfatree.activeInHierarchy == false)
                            {
                                WorldMapSystem.instance.ColsePartWorldMapWindow();
                                return false;
                            }
                        }

                    }
                    if (Main.GetKeyDown(HK_TYPE.HK_VILLAGE_LOCAL))
                    {
                        if (HomeSystem.instance.homeSystem.activeInHierarchy == false)
                        {
                            HomeSystem.instance.ShowHomeSystem(false);
                        }
                        else
                        {
                            HomeSystem.instance.CloseHomeSystem();
                            return false;
                        }

                    }
                }


                //治疗和采集 奇遇
                if (__instance.partWorldMapWindow.activeInHierarchy == false  //世界地图未开启
                        && ActorMenu.instance.actorMenu.activeInHierarchy == false  //人物菜单未开启
                        && HomeSystem.instance.homeSystem.activeInHierarchy == false //村镇地图未开启
                        && BattleSystem.instance.battleWindow.activeInHierarchy == false//非战斗状态
                        && StorySystem.instance.storySystem.activeInHierarchy == false  //奇遇
                        && StorySystem.instance.toStoryMenu.activeInHierarchy == false)  //奇遇开启
                {
                    //治疗
                    if (Main.GetKeyDown(HK_TYPE.HK_HEAL) 
                        && WorldMapSystem.instance.mapHealingButton[0].interactable == true)
                    {

                        WorldMapSystem.instance.MapHealing(0);
                        return false;
                    }
                    //治疗中毒
                    if (Main.GetKeyDown(HK_TYPE.HK_POISON)
                        && WorldMapSystem.instance.mapHealingButton[1].interactable == true)
                    {
                        WorldMapSystem.instance.MapHealing(1);
                        return false;

                    }
                    //采集食物
                    if (Main.GetKeyDown(HK_TYPE.HK_GATHER_FOOD) && __instance.timeWorkWindow.activeInHierarchy == false)
                    {
                        WorldMapSystem.instance.choosePartId = DateFile.instance.mianPartId;
                        WorldMapSystem.instance.choosePlaceId = DateFile.instance.mianPlaceId;
                        WorldMapSystem.instance.chooseWorkTyp = 0; // 0= 粮食
                        WorldMapSystem.instance.ChooseTimeWork();
                        return false;
                    }
                    //采集金石
                    if (Main.GetKeyDown(HK_TYPE.HK_GATHER_MINERAL) && __instance.timeWorkWindow.activeInHierarchy == false)
                    {
                        WorldMapSystem.instance.choosePartId = DateFile.instance.mianPartId;
                        WorldMapSystem.instance.choosePlaceId = DateFile.instance.mianPlaceId;
                        WorldMapSystem.instance.chooseWorkTyp = 2; // 2= 金石
                        WorldMapSystem.instance.ChooseTimeWork();
                        return false;
                    }
                    //采集药草
                    if (Main.GetKeyDown(HK_TYPE.HK_GATHER_HERB) && __instance.timeWorkWindow.activeInHierarchy == false)
                    {
                        WorldMapSystem.instance.choosePartId = DateFile.instance.mianPartId;
                        WorldMapSystem.instance.choosePlaceId = DateFile.instance.mianPlaceId;
                        WorldMapSystem.instance.chooseWorkTyp = 4; // 4= 草药
                        WorldMapSystem.instance.ChooseTimeWork();
                        return false;
                    }
                    //采集银钱
                    if (Main.GetKeyDown(HK_TYPE.HK_GATHER_MONEY) && __instance.timeWorkWindow.activeInHierarchy == false)
                    {
                        WorldMapSystem.instance.choosePartId = DateFile.instance.mianPartId;
                        WorldMapSystem.instance.choosePlaceId = DateFile.instance.mianPlaceId;
                        WorldMapSystem.instance.chooseWorkTyp = 5; // 5= 银钱
                        WorldMapSystem.instance.ChooseTimeWork();
                        return false;
                    }
                    //采集织物
                    if (Main.GetKeyDown(HK_TYPE.HK_GATHER_CLOTH) && __instance.timeWorkWindow.activeInHierarchy == false)
                    {
                        WorldMapSystem.instance.choosePartId = DateFile.instance.mianPartId;
                        WorldMapSystem.instance.choosePlaceId = DateFile.instance.mianPlaceId;
                        WorldMapSystem.instance.chooseWorkTyp = 3; // 3= 织物
                        WorldMapSystem.instance.ChooseTimeWork();
                        return false;
                    }
                    //采集木材
                    if (Main.GetKeyDown(HK_TYPE.HK_GATHER_WOOD) && __instance.timeWorkWindow.activeInHierarchy == false)
                    {
                        WorldMapSystem.instance.choosePartId = DateFile.instance.mianPartId;
                        WorldMapSystem.instance.choosePlaceId = DateFile.instance.mianPlaceId;
                        WorldMapSystem.instance.chooseWorkTyp = 1; // 1=木材
                        WorldMapSystem.instance.ChooseTimeWork();
                        return false;
                    }

                    //奇遇
                    if (Main.GetKeyDown(HK_TYPE.HK_VISITEVENT)
                        && DateFile.instance.HaveShow(DateFile.instance.mianPartId, DateFile.instance.mianPlaceId) > 0
                        && WorldMapSystem.instance.openToStoryButton.interactable == true)
                    {

                        WorldMapSystem.instance.OpenToStory();
                    }
                }
            }

            //原有Update代码修改
            if (Main.GetKeyDown(HK_TYPE.HK_COMFIRM) || Main.GetKeyDown(HK_TYPE.HK_CONFIRM2))
            {
                UIDate.instance.ChangeTrunButton();
                return false;
            }
            if (!___moveButtonDown)
            {
                if (Main.GetKey(HK_TYPE.HK_UP) || Main.GetKey(HK_TYPE.HK_UP2))
                {
                    ___moveButtonDown = true;
                    GetMoveKey.Invoke(__instance, new object[] { 1 });
                    return false;
                }
                else if (Main.GetKey(HK_TYPE.HK_LEFT) || Main.GetKey(HK_TYPE.HK_LEFT2))
                {
                    ___moveButtonDown = true;
                    GetMoveKey.Invoke(__instance, new object[] { 2 });
                    return false;
                }
                else if (Main.GetKey(HK_TYPE.HK_DOWN) || Main.GetKey(HK_TYPE.HK_DOWN2))
                {
                    ___moveButtonDown = true;
                    GetMoveKey.Invoke(__instance, new object[] { 3 });
                    return false;
                }
                else if (Main.GetKey(HK_TYPE.HK_RIGHT) || Main.GetKey(HK_TYPE.HK_RIGHT2))
                {
                    ___moveButtonDown = true;
                    GetMoveKey.Invoke(__instance, new object[] { 4 });
                    return false;
                }
            }
            return false;
        }
    }


    /// <summary>
    ///  BattleSystem 事件
    /// </summary>
    [HarmonyPatch(typeof(BattleSystem), "Update")]
    public static class BattleSystem_Update_Patch
    {
        static MethodInfo GetMoveKey = typeof(BattleSystem).GetMethod("GetMoveKey",
                                            BindingFlags.NonPublic | BindingFlags.Instance);

        private static void Postfix(BattleSystem __instance, bool ___battleEnd)
        {
            if (!Main.enabled && Main.binding_key)
            {
                return;
            }

            if (!___battleEnd && __instance.battleWindow.activeInHierarchy == true)
            {
                //轻功技能
                int skillindex = Main.GetKeyListDown(Main.qinggongskilllist);
                if (skillindex != -1 && Settings.testModifierKey(Main.settings.qinggong_modifier_key) == true)
                {
                    // actorGongFaHolder[1] = 轻功列表
                    var allskill = __instance.actorGongFaHolder[1];
                    if (allskill != null)
                    {
                        var btngo = allskill.childCount > skillindex ? allskill.GetChild(skillindex) : null;
                        if (btngo != null)
                        {
                            // 功法存在
                            int skillid = int.Parse(btngo.name.Split(new char[] { ',' })[1]);
                            //按钮可按下
                            if (btngo.Find("GongFaIcon," + skillid).GetComponent<Button>().interactable == true)
                            {
                                //施放技能
                                BattleSystem.instance.SetUseGongFa(true, skillid);
                                return;
                            }
                        }
                    }
                }

                //特殊技能
                skillindex = Main.GetKeyListDown(Main.specialskilllist);
                if (skillindex != -1 && Settings.testModifierKey(Main.settings.special_modifierkey) == true)
                {
                    // actorGongFaHolder[2] = 绝技列表
                    var allskill = __instance.actorGongFaHolder[2];
                    if (allskill != null)
                    {
                        var btngo = allskill.childCount > skillindex ? allskill.GetChild(skillindex) : null;
                        if (btngo != null)
                        {
                            // 功法存在
                            int skillid = int.Parse(btngo.name.Split(new char[] { ',' })[1]);
                            //按钮可按下
                            if (btngo.Find("GongFaIcon," + skillid).GetComponent<Button>().interactable == true)
                            {
                                //施放技能
                                BattleSystem.instance.SetUseGongFa(true, skillid);
                                return;
                            }
                        }
                    }
                }

                //主技能
                skillindex = Main.GetKeyListDown(Main.mainskilllist);
                if (skillindex != -1)
                {
                    // actorGongFaHolder[0] = 攻击技能列表
                    var allskill = __instance.actorGongFaHolder[0];
                    if (allskill != null)
                    {
                        var btngo = allskill.childCount > skillindex ? allskill.GetChild(skillindex) : null;
                        if (btngo != null)
                        {
                            // 功法存在
                            int skillid = int.Parse(btngo.name.Split(new char[] { ',' })[1]);
                            //按钮可按下
                            if (btngo.Find("GongFaIcon," + skillid).GetComponent<Button>().interactable == true)
                            {
                                //施放技能
                                BattleSystem.instance.SetUseGongFa(true, skillid);
                                return;
                            }
                        }
                    }
                }

                //治疗，治疗毒伤
                if (Main.GetKeyDown(HK_TYPE.HK_HEAL) == true && __instance.doHealButton.interactable == true)
                {
                    BattleSystem.instance.ActorDoHeal(true);
                    return;
                }
                if (Main.GetKeyDown(HK_TYPE.HK_POISON) == true && __instance.doRemovePoisonButton.interactable == true)
                {
                    BattleSystem.instance.ActorDoRemovePoison(true);
                }

            }
        }

    }
    /// <summary>
    ///  ActorMenu  关闭人物信息界面
    /// </summary>
    [HarmonyPatch(typeof(ActorMenu), "Start")]
    public static class ActorMenu_Update_Patch
    {
        private static void Postfix(WorldMapSystem __instance)
        {
            if (!Main.enabled && Main.binding_key)
            {
                return;
            }
            EscClose newobj = __instance.gameObject.AddComponent(typeof(EscClose)) as EscClose;
            newobj.setparam(typeof(ActorMenu), "CloseActorMenu", () =>
            {
                return ActorMenu.instance.actorMenu.activeInHierarchy;
            });
        }
    }

    /// <summary>
    ///  世界地图  关闭世界地图界面
    /// </summary>
    [HarmonyPatch(typeof(WorldMapSystem), "Start")]
    public static class WorldMapSystem_ColsePartWorldMapWindow_Patch
    {
        private static void Postfix(WorldMapSystem __instance)
        {
            if (!Main.enabled && Main.binding_key)
            {
                return;
            }
            EscClose newobj = __instance.partWorldMapWindow.gameObject.AddComponent(typeof(EscClose)) as EscClose;
            newobj.setparam(typeof(WorldMapSystem), "ColsePartWorldMapWindow", () =>
            {
                // 如果 剧情/奇遇 窗口开着，就不处理
                if (StorySystem.instance.toStoryIsShow == true
                        ||StorySystem.instance.storySystem.activeInHierarchy == true)
                {
                    return false;
                }

                // 如果制造窗口开着，就不处理
                if (MakeSystem.instance.makeWindowBack.gameObject.activeInHierarchy == true)
                {
                    return false;
                }
                // 如果商店窗口开着，就不处理
                if (ShopSystem.instance.shopWindow.activeInHierarchy == true
                || BookShopSystem.instance.shopWindow.activeInHierarchy == true
                || SystemSetting.instance.SystemSettingWindow.activeInHierarchy == true)
                {
                    return false;
                }

                //功法树窗口开着就不关
                if(Main._go_gongfatree != null && Main._go_gongfatree.activeInHierarchy == true)
                {
                    return false;
                }
                //关闭工作窗口
                //if(WorldMapSystem.instance.choo)

                return WorldMapSystem.instance.partWorldMapWindow.activeInHierarchy;
            });
        }
    }

    /// <summary>
    ///  建筑地图  关闭建筑地图界面
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "Start")]
    public static class HomeSystem_CloseHomeSystem_Patch
    {
        private static void Postfix(HomeSystem __instance)
        {
            if (!Main.enabled && Main.binding_key)
            {
                return;
            }
            EscClose newobj = __instance.gameObject.AddComponent(typeof(EscClose)) as EscClose;
            newobj.setparam(typeof(HomeSystem), "CloseHomeSystem", () =>
            {
                //依次检测子窗口,顺序很重要

                // 如果制造窗口开着，就不处理
                if(MakeSystem.instance.makeWindowBack.gameObject.activeInHierarchy == true)
                {
                    return false;
                }
                // 如果商店窗口开着，就不处理
                if (ShopSystem.instance.shopWindow.activeInHierarchy == true
                || BookShopSystem.instance.shopWindow.activeInHierarchy == true
                || SystemSetting.instance.SystemSettingWindow.activeInHierarchy == true)
                {
                    return false;
                }


                //Warehouse window
                if (Warehouse.instance.warehouseWindow.gameObject.activeInHierarchy == true)
                {
                    Warehouse.instance.CloseWarehouse();
                    return false;
                }

                //bookWindow
                if (HomeSystem.instance.bookWindow.activeInHierarchy == true)
                {
                    HomeSystem.instance.CloseBookWindow();
                    return false;
                }


                // setstudyWindow
                if (HomeSystem.instance.setStudyWindow.gameObject.activeInHierarchy == true)
                {
                    HomeSystem.instance.CloseSetStudyWindow();
                    return false;
                }

                // studywindow
                if (HomeSystem.instance.studyWindow.activeInHierarchy == true)
                {
                    HomeSystem.instance.CloseStudyWindow();
                    return false;
                }


                //building window
                if (HomeSystem.instance.buildingWindow.gameObject.activeInHierarchy == true)
                {
                    HomeSystem.instance.CloseBuildingWindow();
                    return false;
                }

                return HomeSystem.instance.homeSystem.activeInHierarchy;
            });
        }
    }

    /// <summary>
    /// 关闭系统设置界面
    /// </summary>
    [HarmonyPatch(typeof(SystemSetting), "Start")]
    public static class SystemSetting_CloseSystemSetting_Patch
    {
        private static void Postfix(SystemSetting __instance)
        {
            if (!Main.enabled && Main.binding_key)
            {
                return;
            }
            EscClose newobj = __instance.gameObject.AddComponent(typeof(EscClose)) as EscClose;
            newobj.setparam(typeof(SystemSetting), "CloseSettingWindow", () =>
            {
                return SystemSetting.instance.SystemSettingWindow.activeInHierarchy;
            });
        }
    }


    /// <summary>
    /// 关闭制造界面
    /// </summary>
    [HarmonyPatch(typeof(MakeSystem), "Start")]
    public static class MakeSystem_CloseMakeSystem_Patch
    {
        private static void Postfix(MakeSystem __instance)
        {
            if (!Main.enabled && Main.binding_key)
            {
                return;
            }
            EscClose newobj = __instance.gameObject.AddComponent(typeof(EscClose)) as EscClose;
            newobj.setparam(typeof(MakeSystem), "CloseMakeWindow", () =>
            {
                return MakeSystem.instance.makeWindowBack.gameObject.activeInHierarchy;
            });
        }
    }

    /// <summary>
    /// 关闭书店界面
    /// </summary>
    [HarmonyPatch(typeof(BookShopSystem), "Start")]
    public static class BookShopSystem_CloseBookShopSystem_Patch
    {

        private static void Postfix(BookShopSystem __instance)
        {
            if (!Main.enabled && Main.binding_key)
            {
                return;
            }
            EscClose newobj = __instance.gameObject.AddComponent(typeof(EscClose)) as EscClose;
            newobj.setparam(typeof(BookShopSystem), "CloseShopWindow", () =>
            {
                return BookShopSystem.instance.shopWindow.activeInHierarchy;
            });
        }
    }

    /// <summary>
    /// 关闭商店界面
    /// </summary>
    [HarmonyPatch(typeof(ShopSystem), "Start")]
    public static class ShopSystem_CloseShopSystem_Patch
    {
        private static void Postfix(ShopSystem __instance)
        {
            if (!Main.enabled && Main.binding_key)
            {
                return;
            }
            EscClose newobj = __instance.gameObject.AddComponent(typeof(EscClose)) as EscClose;
            newobj.setparam(typeof(ShopSystem), "CloseShopWindow", () =>
            {
                return ShopSystem.instance.shopWindow.activeInHierarchy;
            });
        }
    }


    /// <summary>
    ///  关闭story/奇遇界面
    /// </summary>
    [HarmonyPatch(typeof(StorySystem), "Start")]
    public static class StorySystem_CloseHomeSystem_Patch
    {
        private static void Postfix(StorySystem __instance)
        {
            if (!Main.enabled && Main.binding_key)
            {
                return;
            }
            ConfirmConfirm newobj = __instance.gameObject.AddComponent(typeof(ConfirmConfirm)) as ConfirmConfirm;
            newobj.setparam(typeof(StorySystem), "OpenStory", () =>
            {
                //依次检测子窗口,顺序很重要

                // 如果开始奇遇没有显示，则不处理
                if (StorySystem.instance.toStoryIsShow != true)
                {
                    return false;
                }
                // 如果按钮不可以交互，不处理
                if (StorySystem.instance.openStoryButton.interactable != true)
                {
                    return false;
                }

                return StorySystem.instance.openStoryButton.interactable;
            });
        }
    }

    /// <summary>
    ///  确定 关闭 过时节
    /// </summary>
    [HarmonyPatch(typeof(UIDate), "Start")]
    public static class UIDate_CloseTurnchange_Patch
    {
        private static void Postfix(UIDate __instance)
        {
            if (!Main.enabled && Main.binding_key)
            {
                return;
            }
            ConfirmConfirm newobj = __instance.gameObject.AddComponent(typeof(ConfirmConfirm)) as ConfirmConfirm;
            newobj.setparam(typeof(UIDate), "CloseTrunChangeWindow", () =>
            {
                //依次检测子窗口,顺序很重要

                // 如果开始奇遇没有显示，则不处理
                if (UIDate.instance.trunChangeWindow.activeInHierarchy != true)
                {
                    return false;
                }

                return UIDate.instance.trunChangeWindow.activeInHierarchy;
            });
        }
    }

    /// <summary>
    ///  确定 确认建筑
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "Start")]
    public static class HomeSystem_Confirm_Patch
    {
        private static void Postfix(UIDate __instance )
        {
            if (!Main.enabled && Main.binding_key)
            {
                return;
            }
            ConfirmConfirm newobj = __instance.gameObject.AddComponent(typeof(ConfirmConfirm)) as ConfirmConfirm;
            newobj.setparam(typeof(HomeSystem), "StartNewBuilding", () =>
            {
                //依次检测子窗口,顺序很重要
                if(HomeSystem.instance.homeSystem.activeInHierarchy == false )
                {
                    return false;
                }

                if(HomeSystem.instance.buildingUPWindowBack.activeInHierarchy== true && HomeSystem.instance.buildingUpCanBuildingButton.interactable)
                {
                    HomeSystem.instance.StartBuildingUp();
                    return false;
                }
                if (HomeSystem.instance.buildingRemoveCanBuildingButton.gameObject.transform.parent.gameObject.activeInHierarchy == true 
                && HomeSystem.instance.buildingRemoveCanBuildingButton.interactable)
                {
                    HomeSystem.instance.StartBuildingRemove();
                    return false;
                }
                if(HomeSystem.instance.buildingWindow.Find("NewBuildingWindowBack").gameObject.activeInHierarchy == true )
                {
                    return HomeSystem.instance.canBuildingButton.interactable;
                }
                return false;
                
            });
        }

    }

    /// <summary>
    ///  确定 确认 购买
    /// </summary>
    [HarmonyPatch(typeof(ShopSystem), "Start")]
    public static class ShopSystem_Confirm_Patch
    {
        private static void Postfix(UIDate __instance)
        {
            if (!Main.enabled && Main.binding_key)
            {
                return;
            }
            ConfirmConfirm newobj = __instance.gameObject.AddComponent(typeof(ConfirmConfirm)) as ConfirmConfirm;
            newobj.setparam(typeof(ShopSystem), "ShopOK", () =>
            {
                //依次检测子窗口,顺序很重要

                // 如果开始奇遇没有显示，则不处理
                if (ShopSystem.instance.shopWindow.activeInHierarchy != true)
                {
                    return false;
                }

                return ShopSystem.instance.shopOkButton.interactable;
            });
        }
    }

    /// <summary>
    ///  确定 bookshop 确认 购买
    /// </summary>
    [HarmonyPatch(typeof(BookShopSystem), "Start")]
    public static class BookShopSystem_Confirm_Patch
    {
        private static void Postfix(UIDate __instance)
        {
            if (!Main.enabled && Main.binding_key)
            {
                return;
            }
            ConfirmConfirm newobj = __instance.gameObject.AddComponent(typeof(ConfirmConfirm)) as ConfirmConfirm;
            newobj.setparam(typeof(BookShopSystem), "ShopOK", () =>
            {
                //依次检测子窗口,顺序很重要

                // 如果开始奇遇没有显示，则不处理
                if (BookShopSystem.instance.shopWindow.activeInHierarchy != true)
                {
                    return false;
                }

                return BookShopSystem.instance.shopOkButton.interactable;
            });
        }
    }


    public class EscCloseNoneSingleton :EscClose
    {
        public new void setparam(Type type_ins, string methodname, Func<bool> tester)
        {
            Main.Logger.Log(this.GetType().ToString() + " Regist " + type_ins.ToString() + "  method :" + methodname);
            mname = methodname;
            ins = type_ins;
            mi = ins.GetMethod(mname);
            insobj = gameObject.GetComponent(ins) as MonoBehaviour;
            testret = tester;
        }
    }

    /// <summary>
    ///  功法树关闭
    /// </summary>
    [HarmonyPatch(typeof(GongFaTreeWindow), "Start")]
    public static class GongFaTreeWindow_EscCloseNonSingleton_Patch
    {
        private static void Postfix(GongFaTreeWindow __instance)
        {
            if (!Main.enabled && Main.binding_key)
            {
                return;
            }
            EscCloseNoneSingleton newobj = __instance.gameObject.AddComponent(typeof(EscCloseNoneSingleton)) as EscCloseNoneSingleton;
            Main._go_gongfatree = __instance.gameObject;
            newobj.setparam(typeof(GongFaTreeWindow), "CloseGongFaTreeWindow", () =>
            {

                return __instance.gongFaTreeWindow.activeInHierarchy;
            });
        }
    }

    ///// <summary>
    /////  BattleSystem 战斗显示快捷键
    ///// </summary>
    //[HarmonyPatch(typeof(BattleSystem), "SetGongFa")]
    //public static class BattleSystem_SetGongFa_Patch
    //{
    //    private static void Postfix(BattleSystem __instance, int typ, int id)
    //    {
    //if (!Main.enabled && Main.binding_key)
    //{
    //    return;
    //}
    //        var gotrans = __instance.actorGongFaHolder[typ].Find("BattleGongFa," + id);
    //        int index = 0;
    //        for (int i = 0; i < __instance.actorGongFaHolder[typ].childCount; i++)
    //        {
    //            if (__instance.actorGongFaHolder[typ].GetChild(i) == gotrans)
    //            {
    //                index = i;
    //            }
    //        }

    //        if (gotrans != null)
    //        {
    //            var go = new GameObject();
    //            go.transform.parent = gotrans;
    //            go.transform.localPosition = new Vector3(0.0f, -1.0f);
    //            var text = go.gameObject.AddComponent<Text>();

    //            if (typ ==1)
    //            {
    //                text.text = Main.getqinggongskill_keystr(index);
    //            }else if( typ ==2)
    //            {
    //                text.text = Main.getspecialskill_keystr(index);
    //            }

    //        }

    //    }
    //}

    ///// <summary>
    /////  BattleSystem 战斗显示快捷键
    ///// </summary>
    //[HarmonyPatch(typeof(BattleSystem), "SetAttackGongFa")]
    //public static class BattleSystem_SetAttackGongFa_Patch
    //{
    //    private static void Postfix(BattleSystem __instance, int id)
    //    {
    //if (!Main.enabled && Main.binding_key)
    //{
    //    return;
    //}
    //        Main.Logger.Log(" try add key info to attach skill " + id);
    //        var gotrans = __instance.actorGongFaHolder[0].Find("BattleGongFa," + id);
    //        int index = -1;
    //        for (int i = 0; i < __instance.actorGongFaHolder[0].childCount; i++)
    //        {
    //            if (__instance.actorGongFaHolder[0].GetChild(i) == gotrans)
    //            {
    //                index = i;
    //            }
    //        }
    //        Main.Logger.Log(" try add key info to attach skill " + id + " found index：" + index);
    //        if (gotrans != null && index >= 0)
    //        {
    //            var go = new GameObject();
    //            go.transform.parent = gotrans;
    //            var text = go.gameObject.AddComponent<Text>();
    //            text.text = Main.getmainskill_keystr(index);
    //            go.transform.localPosition = new Vector3(0.0f, -1.0f);

    //            Main.Logger.Log(" try add key info to attach skill " + id + " found index：" + index + " text:" + text.text);
    //        }

    //    }
    //}
}