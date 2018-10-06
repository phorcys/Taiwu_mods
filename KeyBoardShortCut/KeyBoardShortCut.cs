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
    public enum MODIFIER_KEY_TYPE{
        MKT_SHIFT=0,
        MKT_CTRL=1,
        MKT_ALT=2,

    };

    public enum HK_TYPE
    {
        HK_NONE =0,
        HK_CLOSE =1,
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
        public int tester = 1;
        public SerializableDictionary<HK_TYPE, KeyValuePair<KeyCode, string>> hotkeys;
        public MODIFIER_KEY_TYPE qinggong_modifier_key = MODIFIER_KEY_TYPE.MKT_SHIFT;
        public MODIFIER_KEY_TYPE special_modifierkey = MODIFIER_KEY_TYPE.MKT_ALT;
        [XmlIgnore]
        public UnityModManager.ModEntry modee;
        public Settings()
        {
            initDefaultHotKeys();
        }
        public void initDefaultHotKeys()
        {
            hotkeys = new SerializableDictionary<HK_TYPE, KeyValuePair<KeyCode,string> >()
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
                {HK_TYPE.HK_VILLAGE, new KeyValuePair<KeyCode, string>( KeyCode.P, "打开产业地图")},
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

        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            settings = Settings.Load<Settings>(modEntry);
            settings.modee = modEntry;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
                return false;
            enabled = value;

            return true;
        }

        static bool binding_key = false;
        static HK_TYPE current_binding_key = HK_TYPE.HK_NONE;
        static KeyCode last_key_code = KeyCode.None;

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            processKeyPress();
            GUILayout.BeginVertical("box");
            GUILayout.Label("战斗中释放轻功技能的装饰键");
            settings.qinggong_modifier_key = (MODIFIER_KEY_TYPE)GUILayout.SelectionGrid((int)settings.qinggong_modifier_key, new string[] { "Shift", "Ctrl", "Alt" }, 3);
            GUILayout.Label("战斗中释放绝技的装饰键");
            settings.special_modifierkey = (MODIFIER_KEY_TYPE)GUILayout.SelectionGrid((int)settings.special_modifierkey, new string[] { "Shift", "Ctrl", "Alt" }, 3);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box", new GUILayoutOption[] { GUILayout.MinWidth(700.0f), GUILayout.MaxWidth(700.0f) });
            var keys = settings.hotkeys.Keys.ToArray();

            for (int index =0;index < keys.Length;index ++)
            {
                var key = keys[index];
                var value = settings.hotkeys[key];
                renderHK_GUI(key,value);
            }
            GUILayout.EndVertical();
        }

        private static void processKeyPress()
        {
            Event e = Event.current;
            if (e.isKey && Input.anyKeyDown == true)
            {
                
                if (binding_key == true )
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

        private static void renderHK_GUI(HK_TYPE key,  KeyValuePair<KeyCode, string> value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(value.Value,new GUILayoutOption[] { GUILayout.MinWidth(200.0f),GUILayout.MaxWidth(200.0f)});
            var ret = GUILayout.Button(value.Key == KeyCode.None ? "请按下需要的按键":value.Key.ToString(), new GUILayoutOption[] { GUILayout.MinWidth(200.0f), GUILayout.MaxWidth(200.0f) });
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
            if(settings.hotkeys.ContainsKey(hotkey_name))
            {
                if(Input.GetKeyDown(settings.hotkeys[hotkey_name].Key) )
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class EscClose : UnityEngine.MonoBehaviour
    {
        Type ins;
        string mname;
        MethodInfo mi;
        FieldInfo fi;
        MonoBehaviour insobj;

        public void setparam(Type type_ins, string methodname)
        {
            mname = methodname;
            ins = type_ins;
            mi = ins.GetMethod(mname);
            fi = ins.GetField("instance", BindingFlags.Static | BindingFlags.Public);
            insobj = fi.GetValue(null) as MonoBehaviour;
        }

        public void Update()
        {
            if (insobj.gameObject.active == true && Main.GetKeyDown(HK_TYPE.HK_CLOSE) == true)
            {
                mi.Invoke(insobj, null);
            }
        }
    }

    /// <summary>
    ///  YesOrNoWindow  是否提示确认键
    /// </summary>
    [HarmonyPatch(typeof(WorldMapSystem), "Update")]
    public static class WorldMapSystem_Update_Patch
    {
        static MethodInfo GetMoveKey = typeof(WorldMapSystem).GetMethod("GetMoveKey",
    BindingFlags.NonPublic | BindingFlags.Instance);
        
        private static bool Prefix(WorldMapSystem __instance, bool ___moveButtonDown, bool ___isShowPartWorldMap)
        {
            if(YesOrNoWindow.instance.yesOrNoIsShow == true && YesOrNoWindow.instance.isActiveAndEnabled==true)
            {
                if(Main.GetKeyDown(HK_TYPE.HK_CLOSE)==true && YesOrNoWindow.instance.no.isActiveAndEnabled == true)
                {
                    YesOrNoWindow.instance.CloseYesOrNoWindow();
                    return false;
                }
                if (Main.GetKeyDown(HK_TYPE.HK_COMFIRM) == true|| Main.GetKeyDown(HK_TYPE.HK_CONFIRM2) == true)
                {
                    OnClick.instance.Index();
                    YesOrNoWindow.instance.CloseYesOrNoWindow();
                    return false;
                }

            }
            if(Main.GetKeyDown(HK_TYPE.HK_ACTORMENU))
            {
                if(ActorMenu.instance.actorMenu.activeSelf == false)
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
            if ( Main.GetKeyDown(HK_TYPE.HK_VILLAGE))
            {
                if(HomeSystem.instance.homeSystem.activeSelf == false)
                {
                    HomeSystem.instance.ShowHomeSystem(true);
                }
                else
                {
                    HomeSystem.instance.CloseHomeSystem();
                    return false;
                }

            }
            if ( Main.GetKeyDown(HK_TYPE.HK_WORLDMAP))
            {
                if (__instance.partWorldMapWindow.active == false)
                {
                    WorldMapSystem.instance.ShowPartWorldMapWindow(DateFile.instance.mianWorldId);
                    return false;
                }
                else
                {
                    WorldMapSystem.instance.ColsePartWorldMapWindow();
                    return false;
                }

            }

            if (Main.GetKeyDown(HK_TYPE.HK_HEAL))
            {
                if (__instance.partWorldMapWindow.active == false )
                {
                    WorldMapSystem.instance.MapHealing(0);
                    return false;
                }

            }
            if (Main.GetKeyDown(HK_TYPE.HK_POISON))
            {
                if (__instance.partWorldMapWindow.active == false )
                {
                    WorldMapSystem.instance.MapHealing(1);
                    return false;
                }

            }
            //采集食物
            if (Main.GetKeyDown(HK_TYPE.HK_GATHER_FOOD))
            {
                if (__instance.partWorldMapWindow.active == false )
                {
                    WorldMapSystem.instance.chooseWorkTyp = 0; // 0= 粮食
                    WorldMapSystem.instance.ChooseTimeWork();
                    return false;
                }

            }
            //采集金石
            if (Main.GetKeyDown(HK_TYPE.HK_GATHER_MINERAL))
            {
                if (__instance.partWorldMapWindow.active == false )
                {
                    WorldMapSystem.instance.chooseWorkTyp = 2; // 2= 金石
                    WorldMapSystem.instance.ChooseTimeWork(); return false;
                }

            }
            //采集药草
            if (Main.GetKeyDown(HK_TYPE.HK_GATHER_HERB))
            {
                if (__instance.partWorldMapWindow.active == false )
                {
                    WorldMapSystem.instance.chooseWorkTyp = 4; // 4= 草药
                    WorldMapSystem.instance.ChooseTimeWork();
                    return false;
                }

            }
            //采集银钱
            if (Main.GetKeyDown(HK_TYPE.HK_GATHER_MONEY))
            {
                if (__instance.partWorldMapWindow.active == false )
                {
                    WorldMapSystem.instance.chooseWorkTyp = 5; // 5= 银钱
                    WorldMapSystem.instance.ChooseTimeWork(); return false;
                }

            }
            //采集织物
            if (Main.GetKeyDown(HK_TYPE.HK_GATHER_CLOTH))
            {
                if (__instance.partWorldMapWindow.active == false )
                {
                    WorldMapSystem.instance.chooseWorkTyp = 3; // 3= 织物
                    WorldMapSystem.instance.ChooseTimeWork();
                    return false;
                }

            }
            //采集木材
            if (Main.GetKeyDown(HK_TYPE.HK_GATHER_WOOD))
            {
                if (__instance.partWorldMapWindow.active == false )
                {
                    WorldMapSystem.instance.chooseWorkTyp = 1; // 1=木材
                    WorldMapSystem.instance.ChooseTimeWork();
                    return false;
                }

            }

            if (Main.GetKeyDown(HK_TYPE.HK_COMFIRM) || Main.GetKeyDown(HK_TYPE.HK_CONFIRM2))
            {
                UIDate.instance.ChangeTrunButton();
                return false;
            }
            if (!___moveButtonDown)
            {
                if (Main.GetKeyDown(HK_TYPE.HK_UP) || Main.GetKeyDown(HK_TYPE.HK_UP2))
                {
                    ___moveButtonDown = true;
                    GetMoveKey.Invoke(__instance, new object[] { 1});
                    return false;
                }
                else if (Main.GetKeyDown(HK_TYPE.HK_LEFT) || Main.GetKeyDown(HK_TYPE.HK_LEFT2))
                {
                    ___moveButtonDown = true;
                    GetMoveKey.Invoke(__instance, new object[] { 2 });
                    return false;
                }
                else if (Main.GetKeyDown(HK_TYPE.HK_DOWN) || Main.GetKeyDown(HK_TYPE.HK_DOWN2))
                {
                    ___moveButtonDown = true;
                    GetMoveKey.Invoke(__instance, new object[] { 3 });
                    return false;
                }
                else if (Main.GetKeyDown(HK_TYPE.HK_RIGHT) || Main.GetKeyDown(HK_TYPE.HK_RIGHT2))
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
    ///  ActorMenu  关闭人物信息界面
    /// </summary>
    [HarmonyPatch(typeof(ActorMenu), "Start")]
    public static class ActorMenu_Update_Patch
    {
        private static void Postfix(WorldMapSystem __instance)
        {
            EscClose newobj = __instance.gameObject.AddComponent(typeof(EscClose)) as EscClose;
            newobj.setparam(typeof(ActorMenu), "CloseActorMenu");
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
            EscClose newobj = __instance.partWorldMapWindow.gameObject.AddComponent(typeof(EscClose)) as EscClose;
            newobj.setparam(typeof(WorldMapSystem), "ColsePartWorldMapWindow");
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
            EscClose newobj = __instance.gameObject.AddComponent(typeof(EscClose)) as EscClose;
            newobj.setparam(typeof(HomeSystem), "CloseHomeSystem");
        }
    }
}