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

        private void WindowFunc(int windowId)
        {
            var changed = false;
            GUILayout.BeginVertical();
            GUILayout.Space(200);

            for (int i = 0; i < Main.repo.Length; i++)
            {
                if (i % 5 == 0)
                    GUILayout.BeginHorizontal();
                var state = GUILayout.Toggle(Main.Setting.repo[i], Main.repo[i], GUILayout.Width(60));
                if (Main.Setting.repo[i] != state)
                {
                    Main.Setting.repo[i] = state;
                    changed = true;
                }
                if (i % 5 == 4 || i == Main.repo.Length - 1)
                    GUILayout.EndHorizontal();
            }
            GUILayout.Space(10);

            for (int i = 0; i < Main.tof.Length; i++)
            {
                if (i % 5 == 0)
                    GUILayout.BeginHorizontal();
                var state = GUILayout.Toggle(Main.Setting.tof[i], Main.tof[i], GUILayout.Width(60));
                if (Main.Setting.tof[i] != state)
                {
                    Main.Setting.tof[i] = state;
                    changed = true;
                }
                if (i % 5 == 4 || i == Main.tof.Length - 1)
                    GUILayout.EndHorizontal();
            }
            GUILayout.Space(10);

            for (int i = 0; i < Main.read.Length; i++)
            {
                if (i % 5 == 0)
                    GUILayout.BeginHorizontal();
                var state = GUILayout.Toggle(Main.Setting.read[i], Main.read[i], GUILayout.Width(60));
                if (Main.Setting.read[i] != state)
                {
                    Main.Setting.read[i] = state;
                    changed = true;
                }
                if (i % 5 == 4 || i == Main.read.Length - 1)
                    GUILayout.EndHorizontal();
            }
            GUILayout.Space(10);

            for (int i = 0; i < Main.pinji.Length; i++)
            {
                if (i % 5 == 0)
                    GUILayout.BeginHorizontal();
                var state = GUILayout.Toggle(Main.Setting.pinji[i], Main.pinji[i], GUILayout.Width(60));
                if (Main.Setting.pinji[i] != state)
                {
                    Main.Setting.pinji[i] = state;
                    changed = true;
                }
                if (i % 5 == 4 || i == Main.pinji.Length - 1)
                    GUILayout.EndHorizontal();
            }
            GUILayout.Space(10);

            for (int i = 0; i < Main.gongfa.Length; i++)
            {
                if (i % 5 == 0)
                    GUILayout.BeginHorizontal();
                var state = GUILayout.Toggle(Main.Setting.gongfa[i], Main.gongfa[i], GUILayout.Width(60));
                if (Main.Setting.gongfa[i] != state)
                {
                    Main.Setting.gongfa[i] = state;
                    changed = true;
                }
                if (i % 5 == 4 || i == Main.gongfa.Length - 1)
                    GUILayout.EndHorizontal();
            }
            GUILayout.Space(10);

            for (var i = 0; i < Main.gang.Length; i++)
            {
                if (i % 5 == 0)
                    GUILayout.BeginHorizontal();
                var state = GUILayout.Toggle(Main.Setting.gang[i], Main.gang[i], GUILayout.Width(60));
                if (Main.Setting.gang[i] != state)
                {
                    Main.Setting.gang[i] = state;
                    changed = true;
                }
                if (i % 5 == 4 || i == Main.gang.Length - 1)
                    GUILayout.EndHorizontal();
            }
            GUILayout.Space(10);

            GUILayout.EndVertical();

            if (changed)
            {
                HomeSystem_SetBook_Patch.SetBookData();
            }
        }

        /// <summary>
        /// 切换窗体显示状态
        /// </summary>
        public void ToggleWindow()
        {
            if (!HomeSystem.instance.bookWindow.activeSelf && !Open)
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