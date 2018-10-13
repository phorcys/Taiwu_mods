using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NpcScan
{
    public class UI : MonoBehaviour
    {
        int minage = 0;
        int maxage = 0;
        int strValue = 0;
        int conValue = 0;
        int agiValue = 0;
        int bonValue = 0;
        int intValue = 0;
        int patValue = 0;
        int genderValue = 0;
        int charmValue = 0;

        ArrayList actorList = new ArrayList();
        Vector2 scrollPosition = new Vector2(0, 0);

        bool isall = true;
        bool isman = false;
        bool iswoman = false;

        internal static bool Load()
        {
            try
            {
                new GameObject(typeof(UI).FullName, typeof(UI));
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return false;
        }

        private static UI mInstance = null;

        public static UI Instance
        {
            get { return mInstance; }
        }

        public static GUIStyle window = null;
        public static GUIStyle h1 = null;
        public static GUIStyle h2 = null;
        public static GUIStyle bold = null;
        private static GUIStyle settings = null;
        private static GUIStyle status = null;
        private static GUIStyle www = null;
        private static GUIStyle updates = null;

        private bool mInit = false;

        private bool mOpened = false;
        public bool Opened { get { return mOpened; } }

        private Rect mWindowRect = new Rect(0, 0, 0, 0);

        private void Awake()
        {
            mInstance = this;
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            CalculateWindowPos();
        }

        private void Update()
        {
            if (mOpened)
                mLogTimer += Time.unscaledDeltaTime;
            bool toggle = false;
            if (Input.GetKeyUp(KeyCode.F12) && (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)))
            {
                toggle = true;
            }
            if (toggle)
            {
                ToggleWindow();
            }
        }

        private void OnDestroy()
        {
            //SaveSettingsAndParams();
        }

        private void PrepareGUI()
        {
            window = new GUIStyle();
            window.name = "umm window";
            //window.normal.background = Textures.Window;
            //window.normal.background.wrapMode = TextureWrapMode.Repeat;
            window.padding = RectOffset(5);

            h1 = new GUIStyle();
            h1.name = "umm h1";
            h1.normal.textColor = Color.white;
            h1.fontSize = 16;
            h1.fontStyle = FontStyle.Bold;
            h1.alignment = TextAnchor.MiddleCenter;
            h1.margin = RectOffset(0, 5);

            h2 = new GUIStyle();
            h2.name = "umm h2";
            h2.normal.textColor = new Color(0.6f, 0.91f, 1f);
            h2.fontSize = 13;
            h2.fontStyle = FontStyle.Bold;
            //                h2.alignment = TextAnchor.MiddleCenter;
            h2.margin = RectOffset(0, 3);

            bold = new GUIStyle(GUI.skin.label);
            bold.name = "umm bold";
            bold.normal.textColor = Color.white;
            bold.fontStyle = FontStyle.Bold;

            int iconHeight = 28;
            settings = new GUIStyle();
            settings.alignment = TextAnchor.MiddleCenter;
            settings.stretchHeight = true;
            settings.fixedWidth = 24;
            settings.fixedHeight = iconHeight;

            status = new GUIStyle();
            status.alignment = TextAnchor.MiddleCenter;
            status.stretchHeight = true;
            status.fixedWidth = 12;
            status.fixedHeight = iconHeight;

            www = new GUIStyle();
            www.alignment = TextAnchor.MiddleCenter;
            www.stretchHeight = true;
            www.fixedWidth = 24;
            www.fixedHeight = iconHeight;

            updates = new GUIStyle();
            updates.alignment = TextAnchor.MiddleCenter;
            updates.stretchHeight = true;
            updates.fixedWidth = 26;
            updates.fixedHeight = iconHeight;
        }

        private void OnGUI()
        {
            if (!mInit)
            {
                mInit = true;
                PrepareGUI();
            }

            if (mOpened)
            {
                var backgroundColor = GUI.backgroundColor;
                var color = GUI.color;
                GUI.backgroundColor = Color.white;
                GUI.color = Color.white;
                mWindowRect = GUILayout.Window(0, mWindowRect, WindowFunction, "", window, GUILayout.Height(Screen.height - 200));
                GUI.backgroundColor = backgroundColor;
                GUI.color = color;
            }
        }

        class Column
        {
            public bool expand = false;
            public bool skip = false;
        }

        private float mLogTimer = 0;

        private void CalculateWindowPos()
        {

            mWindowRect = new Rect(200f, 50f, 0, 0);
        }

        private void WindowFunction(int windowId)
        {
            if (Input.GetKey(KeyCode.LeftControl))
                GUI.DragWindow(mWindowRect);

            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("NPC查找器");
            if (GUILayout.Button("关闭", GUILayout.Width(150)))
            {
                ToggleWindow();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("年龄:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(minage.ToString(), 3, GUILayout.Width(30)), out minage);
            GUILayout.Label("--", GUILayout.Width(10));
            int.TryParse(GUILayout.TextField(maxage.ToString(), 3, GUILayout.Width(30)), out maxage);
            GUILayout.Space(5);
            GUILayout.Label("性别:", GUILayout.Width(30));
            GUILayout.Space(5);
            isall = GUILayout.Toggle(isall, "全部", GUILayout.Width(45));
            if (isall)
            {
                iswoman = false;
                isman = false;
                genderValue = 0;
            }

            isman = GUILayout.Toggle(isman, "男", GUILayout.Width(30));
            if (isman)
            {
                isall = false;
                iswoman = false;
                genderValue = 1;
            }
            iswoman = GUILayout.Toggle(iswoman, "女", GUILayout.Width(30));
            if (iswoman)
            {
                isall = false;
                isman = false;
                genderValue = 2;
            }
            GUILayout.Space(10);
            GUILayout.Label("膂力:",GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(strValue.ToString(), 10, GUILayout.Width(30)), out strValue);
            GUILayout.Space(5);
            GUILayout.Label("体质:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(conValue.ToString(), 10, GUILayout.Width(30)), out conValue);
            GUILayout.Space(5);
            GUILayout.Label("灵敏:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(agiValue.ToString(), 10, GUILayout.Width(30)), out agiValue);
            GUILayout.Space(5);
            GUILayout.Label("根骨:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(bonValue.ToString(), 10, GUILayout.Width(30)), out bonValue);
            GUILayout.Space(5);
            GUILayout.Label("悟性:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(intValue.ToString(), 10, GUILayout.Width(30)), out intValue); 
            GUILayout.Space(5);
            GUILayout.Label("定力:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(patValue.ToString(), 10, GUILayout.Width(30)), out patValue);
            GUILayout.Space(5);
            GUILayout.Label("魅力:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(charmValue.ToString(), 10, GUILayout.Width(30)), out charmValue);
            GUILayout.Space(50);
            if (GUILayout.Button("查找", GUILayout.Width(150)))
            {
                actorList.Clear();
                DateFile dateFile = DateFile.instance;
                Dictionary<int, Dictionary<int, string>> actors = dateFile.actorsDate;
                foreach (int index in actors.Keys)
                {
                    Dictionary<int, string> actor = actors[index];
                    int str = dateFile.BaseAttr(index, 0, 0);
                    int con = dateFile.BaseAttr(index, 1, 0);
                    int agi = dateFile.BaseAttr(index, 2, 0);
                    int bon = dateFile.BaseAttr(index, 3, 0);
                    int inv = dateFile.BaseAttr(index, 4, 0);
                    int pat = dateFile.BaseAttr(index, 5, 0);
                    int age = int.Parse(dateFile.GetActorDate(index, 11, false));
                    int gender = int.Parse(dateFile.GetActorDate(index, 14, false));
                    int charm = int.Parse(DateFile.instance.GetActorDate(index, 15, true));
                    if (inv >= intValue
                        && str > strValue
                        && con > conValue
                        && agi > agiValue
                        && bon > bonValue
                        && pat > patValue
                        && charm > charmValue
                        && age > minage
                        && (maxage == 0 || age < maxage)
                        && (genderValue == 0 || gender == genderValue)
                        )
                    {
                        string genderText;
                        if (gender == 1)
                        {
                            genderText = "男";
                        }
                        else
                        {
                            genderText = "女";

                        }
                        string place;
                        if (int.Parse(dateFile.GetActorDate(index, 8, false)) != 1)
                        {
                            place = dateFile.massageDate[8010][3].Split(new char[] { '|' })[1];
                        }
                        else
                        {
                            List<int> list = new List<int>(dateFile.GetActorAtPlace(index));
                            place = string.Format("{0}{1}", new object[]
                            {
                            dateFile.GetNewMapDate(list[0], list[1], 98),
                            dateFile.GetNewMapDate(list[0], list[1], 0)
                            });
                        }

                        string charmText = ((int.Parse(DateFile.instance.GetActorDate(index, 11, false)) > 14) ? ((int.Parse(DateFile.instance.GetActorDate(index, 8, false)) != 1 || int.Parse(DateFile.instance.GetActorDate(index, 305, false)) != 0) ? DateFile.instance.massageDate[25][int.Parse(DateFile.instance.GetActorDate(index, 14, false)) - 1].Split(new char[]
                        {
                            '|'
                        })[Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(index, 15, true)) / 100, 0, 9)] : DateFile.instance.massageDate[25][5].Split(new char[]
                        {
                            '|'
                        })[1]) : DateFile.instance.massageDate[25][5].Split(new char[]
                        {
                            '|'
                        })[0]);

                        actorList.Add(string.Format("\n姓名:{1}  年龄:{8}  性别:{9}  位置:{10}  魅力:{11}  膂力:{2}  体质:{3}  灵敏:{4}  根骨:{5}  悟性:{6}  定力:{7} ", index, dateFile.GetActorName(index), str, con, agi, bon, inv, pat, age, genderText, place, charm + "("+ charmText + ")"));
                    }
                }
            }
            GUILayout.EndHorizontal();
            if (actorList.Count > 0)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(false));
                foreach (string actor in actorList)
                {
                    GUILayout.Label(actor);
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
            GUILayout.Space(3);
        }

        internal bool GameCursorLocked { get; set; }

        public void ToggleWindow()
        {
            ToggleWindow(!mOpened);
        }

        public void ToggleWindow(bool open)
        {
            mOpened = open;
            BlockGameUI(open);
            if (!mOpened)
            {
                //SaveSettingsAndParams();
            }
            if (open)
            {
                GameCursorLocked = Cursor.lockState == CursorLockMode.Locked || !Cursor.visible;
                if (GameCursorLocked)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
            }
            else
            {
                if (GameCursorLocked)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }

        private GameObject mCanvas = null;

        private void BlockGameUI(bool value)
        {
            if (value)
            {
                mCanvas = new GameObject("", typeof(Canvas), typeof(GraphicRaycaster));
                mCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                mCanvas.GetComponent<Canvas>().sortingOrder = Int16.MaxValue;
                DontDestroyOnLoad(mCanvas);
                var panel = new GameObject("", typeof(Image));
                panel.transform.SetParent(mCanvas.transform);
                panel.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0);
                panel.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                panel.GetComponent<RectTransform>().offsetMin = Vector2.zero;
                panel.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            }
            else
            {
                Destroy(mCanvas);
            }
        }

        private static RectOffset RectOffset(int value)
        {
            return new RectOffset(value, value, value, value);
        }

        private static RectOffset RectOffset(int x, int y)
        {
            return new RectOffset(x, x, y, y);
        }
    }

    //        [HarmonyPatch(typeof(Screen), "lockCursor", MethodType.Setter)]
    static class Screen_lockCursor_Patch
    {
        static bool Prefix(bool value)
        {
            if (UI.Instance != null && UI.Instance.Opened)
            {
                UI.Instance.GameCursorLocked = value;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                return false;
            }

            return true;
        }
    }

}
