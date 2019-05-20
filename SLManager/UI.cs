using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Sth4nothing.SLManager
{
    class UI : MonoBehaviour
    {
        public static UI Instance { get; private set; }

        private static GameObject go;
        private static GameObject canvas;

        public bool Open { get; private set; }

        private const float designWidth = 1600, designHeight = 900;
        private Rect windowRect;
        private bool cursorLock;
        private Vector2 scrollPosition;
        private GUIStyle h1, itemStyle, btnStyle;
        private Dictionary<string, bool> state;

        public static bool Load()
        {
            try
            {
                if (Instance == null)
                {
                    go = new GameObject("", typeof(UI));
                    DontDestroyOnLoad(go);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void Awake()
        {
            Instance = this;
            Open = false;
            DontDestroyOnLoad(this);
        }

        public void Start()
        {
            windowRect = new Rect(600, 150, 400, 600);
            scrollPosition = Vector2.zero;
        }

        private void PrepareGUI()
        {
            h1 = new GUIStyle(GUI.skin.label);
            h1.alignment = TextAnchor.MiddleCenter;
            h1.padding = new RectOffset(5, 5, 5, 5);
            h1.fontStyle = FontStyle.Bold;
            h1.fontSize = 18;
            h1.fixedHeight = 40f;
            h1.normal.textColor = Color.white;

            itemStyle = new GUIStyle(GUI.skin.toggle);
            itemStyle.fontSize = 16;
            itemStyle.richText = true;
            itemStyle.alignment = TextAnchor.MiddleCenter;

            btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.alignment = TextAnchor.MiddleCenter;
            btnStyle.padding = new RectOffset(5, 5, 5, 5);
            btnStyle.fontSize = 20;
            btnStyle.fixedHeight = 40f;
            btnStyle.normal.textColor = Color.white;
        }

        public void OnGUI()
        {
            if (Open)
            {
                PrepareGUI();

                var bgColor = GUI.backgroundColor;
                var color = GUI.color;
                var svMat = GUI.matrix;

                Vector2 resizeRatio = new Vector2((float)Screen.width / designWidth,
                    (float)Screen.height / designHeight);
                GUI.matrix = Matrix4x4.TRS(Vector3.zero,
                    Quaternion.identity,
                    new Vector3(resizeRatio.x, resizeRatio.y, 1.0f));

                GUI.backgroundColor = Color.black;
                GUI.color = Color.white;

                windowRect = GUILayout.Window(668, windowRect, WindowFunc, "",
                    GUILayout.MaxHeight(windowRect.height),
                    GUILayout.MaxWidth(windowRect.width));

                GUI.matrix = svMat;
                GUI.backgroundColor = bgColor;
                GUI.color = color;
            }
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleWindow(false);
            }
        }

        private void WindowFunc(int windowId)
        {
            GUILayout.Label("读取存档", h1);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true,
                GUILayout.MaxWidth(windowRect.width),
                GUILayout.MaxHeight(windowRect.height - h1.fixedHeight - btnStyle.fixedHeight));
            GUILayout.BeginVertical();


            foreach (var key in LoadFile.SavedFiles)
            {
                if (!LoadFile.SavedInfos.ContainsKey(key) || LoadFile.SavedInfos[key] == null)
                    continue;
                var info = LoadFile.SavedInfos[key];
                if (GUILayout.Toggle(state[key],
                    $"{info.name}\n{info.year}年 {info.GetTurn()}时节 {info.samsara}轮回\n{info.playtime}\n{info.position}",
                    itemStyle) && GUI.changed)
                {
                    foreach (var key2 in LoadFile.SavedFiles)
                    {
                        state[key2] = key == key2;
                    }
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("确定", btnStyle) && state.Any((pair) => pair.Value))
            {
                ToggleWindow(false);

                var file = state.First((pair) => pair.Value).Key;
                StartCoroutine(LoadFile.Load(file));
            }
            if (GUILayout.Button("取消", btnStyle))
            {
                ToggleWindow(false);
            }
            GUILayout.EndHorizontal();
        }

        public void ShowData()
        {
            state = new Dictionary<string, bool>();
            foreach (var key in LoadFile.SavedFiles)
            {
                state[key] = false;
            }

            ToggleWindow(true);
        }

        private void ToggleWindow(bool open)
        {
            Open = open;
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

        private void BlockGameUI(bool open)
        {
            if (open)
            {
                canvas = new GameObject("", typeof(Canvas), typeof(GraphicRaycaster));
                canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.GetComponent<Canvas>().sortingOrder = short.MaxValue;
                DontDestroyOnLoad(canvas);
                var panel = new GameObject("panel", typeof(Image));
                panel.transform.SetParent(canvas.transform);
                panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
                panel.GetComponent<RectTransform>().anchorMin = Vector2.zero;
                panel.GetComponent<RectTransform>().anchorMax = Vector2.one;
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
