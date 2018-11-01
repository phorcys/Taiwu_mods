using System.Collections.Generic;
using System.Reflection;
using Harmony12;
using UnityEngine;
using UnityModManagerNet;

namespace NpcScan
{

    public class Settings : UnityModManager.ModSettings
    {
        public KeyCode key = KeyCode.F12;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static bool uiIsShow = false;
        public static bool bindingKey = false;
        public static Dictionary<int, Features> featuresList = new Dictionary<int, Features>();
        public static List<Features> findList = new List<Features>();
        public static Dictionary<string, int> fNameList = new Dictionary<string, int>();
        public static Dictionary<int, string> textColor = new Dictionary<int, string>()
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
        public static Dictionary<string,int> colorText = new Dictionary<string,int>()
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

        //static KeyCode last_key_code = KeyCode.None;

        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            settings = Settings.Load<Settings>(modEntry);
            modEntry.OnGUI = OnGUI;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            if (!Main.uiIsShow)
            {
                UI.Load();
                UI.key = settings.key;
                Main.uiIsShow = true;
                //Logger.Log("scan测试");
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
        }


        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }
}