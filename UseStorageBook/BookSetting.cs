using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace Sth4nothing.UseStorageBook
{
    class BookSetting : MonoBehaviour
    {
        public static BookSetting Instance { get; private set; }
        private static GameObject canvas;
        private static GameObject go;
        private Rect windowRect;
        private bool cursorLock;
        public const float designWidth = 1600;
        public const float designHeight = 900;


        public bool Open {get; private set;}

        public static bool Load()
        {
            try
            {
                Main.Logger.Log("load"); 
                if (Instance == null)
                {
                    go = new GameObject("", typeof(BookSetting));
                    DontDestroyOnLoad(go);
                }
            }
            catch (System.Exception)
            {
                return false;
            }
            return true;
        }

        private void Awake()
        {
            Main.Logger.Log("awake");
            Instance = this;
            DontDestroyOnLoad(this);
        }

        public void Start()
        {
            Main.Logger.Log("start");
            Open = false;
            windowRect = new Rect(0, 0, designWidth * 0.25f, designHeight);
        }

        public void OnGUI()
        {
            if (Open)
            {
                var bgColor = GUI.backgroundColor;
                var color = GUI.color;
                var svMat = GUI.matrix;

                Vector2 resizeRatio = new Vector2((float)Screen.width / designWidth, (float)Screen.height / designHeight);
                GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(resizeRatio.x, resizeRatio.y, 1.0f));

                GUI.backgroundColor = Color.black;
                GUI.color = Color.white;

                windowRect = GUILayout.Window(667, windowRect, WindowFunc, "");

                GUI.matrix = svMat;
                GUI.backgroundColor = bgColor;
                GUI.color = color;
            }
        }

        public void Update()
        {
            if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl))
                && Input.GetKeyUp(KeyCode.F6))
            {
                ToggleWindow();
            }
        }

        private static void ShowSetting(string label, Dictionary<int, bool> dict, string[] setting, ref bool changed)
        {
            GUILayout.BeginVertical("Box");
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(60));
            if (setting.Length >= 5)
            {
                if (GUILayout.Button("全部", GUILayout.Width(60)))
                {
                    var all = dict.All((pair) => pair.Value);
                    for (int i = 0; i < setting.Length; i++)
                    {
                        dict[i] = !all;
                    }
                    changed = true;
                }
            }
            GUILayout.EndHorizontal();
            for (int i = 0; i < setting.Length; i++)
            {
                if (i % 5 == 0)
                    GUILayout.BeginHorizontal();
                var state = GUILayout.Toggle(dict[i], setting[i], GUILayout.Width(60));
                if (dict[i] != state)
                {
                    dict[i] = state;
                    changed = true;
                }
                if (i % 5 == 4 || i == setting.Length - 1)
                    GUILayout.EndHorizontal();
            }
            GUILayout.Space(10);
            GUILayout.EndVertical();
        }

        private void WindowFunc(int windowId)
        {
            var changed = false;
            GUILayout.BeginVertical();
            GUILayout.Space(200);

            ShowSetting("背包/仓库", Main.Setting.repo, Main.repo, ref changed);

            if (BuildingWindow.instance.studySkillTyp >= 17)
                ShowSetting("真传/手抄", Main.Setting.tof, Main.tof, ref changed);

            ShowSetting("阅读进度", Main.Setting.read, Main.read, ref changed);

            ShowSetting("品级", Main.Setting.pinji, Main.pinji, ref changed);

            if (BuildingWindow.instance.studySkillTyp >= 17)
                ShowSetting("功法", Main.Setting.gongfa, Main.gongfa, ref changed);

            if (BuildingWindow.instance.studySkillTyp >= 17)
                ShowSetting("帮派", Main.Setting.gang, Main.gang, ref changed);

            GUILayout.EndVertical();

            if (changed)
            {
                BuildingWindow_SetBook_Patch.SetBookData();
            }
        }

        /// <summary>
        /// 切换窗体显示状态
        /// </summary>
        public void ToggleWindow()
        {
            if (!BuildingWindow.instance.bookWindow.activeSelf && !Open)
            {
                Main.Logger.Log("bookWindow未启动");
                return;
            }
            Open = !Open;
            Main.Logger.Log($"toggle: {Open}");
            BlockGameUI(Open);
            if (Open)
            {
                cursorLock = Cursor.lockState == CursorLockMode.Locked || !Cursor.visible;
                if (cursorLock)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
            }
            else
            {
                if (cursorLock)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }

        /// <summary>
        /// 挡住游戏的UI
        /// </summary>
        /// <param name="open"></param>
        private void BlockGameUI(bool open)
        {
            if (open)
            {
                canvas = new GameObject("canvas667", typeof(Canvas), typeof(GraphicRaycaster));
                canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.GetComponent<Canvas>().sortingOrder = short.MaxValue;
                DontDestroyOnLoad(canvas);
                var panel = new GameObject("panel", typeof(Image));
                panel.transform.SetParent(canvas.transform);
                panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
                panel.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
                panel.GetComponent<RectTransform>().anchorMax = new Vector2(0.25f, 1);
                panel.GetComponent<RectTransform>().offsetMin = Vector2.zero;
                panel.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            }
            else
            {
                Destroy(canvas);
            }
        }
    }
}