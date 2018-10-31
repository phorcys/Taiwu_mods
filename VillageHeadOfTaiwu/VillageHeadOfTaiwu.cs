using Harmony12;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace VillageHeadOfTaiwu
{
    class VillagersList : MonoBehaviour
    {
        class Worker
        {
            /// <summary>
            /// 显示在列表中的任务字符串
            /// </summary>
            public string content;
            /// <summary>
            /// 坐标part
            /// </summary>
            public int part;
            /// <summary>
            /// 坐标place
            /// </summary>
            public int place;
            /// <summary>
            /// 消耗的人力
            /// </summary>
            public int manpower;
            /// <summary>
            /// 采集的资源类型
            /// </summary>
            public int type;
        }

        /// <summary>
        /// 采集的资源类型
        /// </summary>
        enum WorkType
        {
            FOOD,
            WOOD,
            STONE,
            SILK,
            HERBAL,
            MONEY
        }

        readonly string[] workStr = {
            "食材",
            "木材",
            "金石",
            "织物",
            "药材",
            "银钱"
        };

        public static VillagersList Instance { get; private set; }
        static GameObject obj;

        GameObject canvas;

        Rect windowRect;
        Vector2 scrollPosition;

        public bool Open { get; private set; }
        bool cursorLock;
        bool collapse;

        GUIStyle windowStyle;
        GUIStyle collapseStyle;
        GUIStyle labelStyle;
        GUIStyle seperatorStyle;
        GUIStyle buttonStyle;

        DateFile df;
        WorldMapSystem wms;

        public static bool Load()
        {
            try
            {
                obj = new GameObject("Villagers", typeof(VillagersList));
                DontDestroyOnLoad(obj);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void Awake()
        {
            Main.logger.Log("awake");
            Instance = this;
            DontDestroyOnLoad(this);
        }

        public void Start()
        {
            Main.logger.Log("start");

            Open = false;
            //collapse = false;

            scrollPosition = Vector2.zero;

            windowStyle = new GUIStyle
            {
                name = "window",
                padding = new RectOffset(5, 5, 5, 5),
            };

            collapseStyle = new GUIStyle
            {
                name = "collapse",
                fontSize = 12,
                alignment = TextAnchor.MiddleRight,
                fixedWidth = 25f,
            };
            collapseStyle.normal.textColor = Color.red;

            labelStyle = new GUIStyle
            {
                name = "label",
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(0, 0, 5, 5),
            };
            labelStyle.normal.textColor = Color.yellow;

            buttonStyle = new GUIStyle
            {
                name = "button",
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(0, 0, 5, 0),
            };
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.richText = true;

            seperatorStyle = new GUIStyle
            {
                name = "seperator",
                alignment = TextAnchor.MiddleCenter,
            };
            seperatorStyle.normal.textColor = Color.cyan;

            df = DateFile.instance;
            wms = WorldMapSystem.instance;

            CalcWindow();

            ToggleWindow();

            // 设置点击事件
            // var btn = UIDate.instance.manpowerText.gameObject.AddComponent<Button>();
            // btn.interactable = true;
            // btn.targetGraphic = UIDate.instance.manpowerText;
            // btn.onClick.AddListener(() => 
            // {
            //     VillagersList.Instance.ToggleWindow();
            //     Main.logger.Log("toggle");
            // });
        }

        public void CalcWindow()
        {
            windowRect = new Rect(Screen.width * 0.833f, Screen.height * 0.05f, Screen.width * 0.164f, 0);
            Main.logger.Log(windowRect.ToString());
        }
        private void PrepareGUI()
        {
            labelStyle.fontSize = Main.Setting.labelSize;
            buttonStyle.fontSize = Main.Setting.buttonSize;
            seperatorStyle.fontSize = Main.Setting.buttonSize;

            buttonStyle.fixedWidth = windowRect.width - 40;
            seperatorStyle.fixedWidth = windowRect.width - 40;
        }

        public void OnGUI()
        {
            if (Open)
            {
                PrepareGUI();

                var bgColor = GUI.backgroundColor;
                var color = GUI.color;

                GUI.backgroundColor = Color.black;
                GUI.color = Color.white;
                windowRect = GUILayout.Window(666, windowRect, WindowFunc, "",
                    windowStyle, GUILayout.Height(Screen.height * 0.73f));

                GUI.backgroundColor = bgColor;
                GUI.color = color;
            }
        }

        private void WindowFunc(int windowId)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(windowRect.width - 35f);
            if (GUILayout.Button((collapse ? "展" : "收"), collapseStyle))
            {
                collapse = !collapse;
            }
            GUILayout.EndHorizontal();

            if (!collapse)
            {
                GUILayout.BeginVertical();
                for (int i = 0; i < 6; i++)
                {
                    if (i % 3 == 0)
                    {
                        GUILayout.BeginHorizontal();
                    }
                    if (GUILayout.Button(workStr[i], labelStyle))
                    {
                        ArrangeWork((WorkType)i);
                    }
                    if (i % 3 < 2)
                    {
                        GUILayout.Label("|");
                    }
                    else if (i % 3 == 2)
                    {
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndVertical();
                canvas.transform.Find("panel").GetComponent<RectTransform>().anchorMin = new Vector2(windowRect.x / Screen.width, 0.22f);

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false,
                    GUILayout.Width(windowRect.width - 20),
                    GUILayout.MaxHeight(Screen.height * 0.73f));
                GUILayout.BeginVertical();
                for (int i = 0; i < 6; i++)
                {
                    GUILayout.Label($"------------{workStr[i]}-------------     ", seperatorStyle);
                    var wokers = new List<Worker>(GetWorkers((WorkType)i));
                    foreach (var worker in wokers)
                    {
                        if (GUILayout.Button(worker.content, buttonStyle))
                        {
                            CancelWork(worker);
                        }
                    }
                }
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
            else
            {
                canvas.transform.Find("panel").GetComponent<RectTransform>().anchorMin = new Vector2(1f - 25f / Screen.width, 0.95f - 25f / Screen.height);
            }
        }

        public void Update()
        {
            if (!Main.enabled)
            {
                Destroy(canvas);
                Open = false;
            }
            if (Open)
            {
                if (Input.GetKey(KeyCode.PageUp))
                {
                    scrollPosition.y -= 40;
                    Main.logger.Log($"pgup {scrollPosition}");
                }
                if (Input.GetKey(KeyCode.PageDown))
                {
                    scrollPosition.y += 40;
                    Main.logger.Log($"pgdn {scrollPosition}");
                }
            }
            if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl))
                && Input.GetKeyUp(Main.Setting.key))
            {
                ToggleWindow();
            }
        }

        /// <summary>
        /// 切换窗体显示状态
        /// </summary>
        public void ToggleWindow()
        {
            Main.logger.Log($"Toggle {(!Open ? "on" : "off")}");
            Open = !Open;
            BlockGameUI(Open);
            if (Open)
            {
                scrollPosition = Vector2.zero;
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
                canvas = new GameObject("canvas", typeof(Canvas), typeof(GraphicRaycaster));
                canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.GetComponent<Canvas>().sortingOrder = short.MaxValue;
                DontDestroyOnLoad(canvas);
                var panel = new GameObject("panel", typeof(Image));
                panel.transform.SetParent(canvas.transform);
                panel.GetComponent<Image>().color = new Color(0.6f, 0.6f, 0.6f, 0.2f);
                panel.GetComponent<RectTransform>().anchorMin = new Vector2(windowRect.x / Screen.width, 0.22f);
                panel.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.95f);
                panel.GetComponent<RectTransform>().offsetMin = Vector2.zero;
                panel.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            }
            else
            {
                Destroy(canvas);
            }
        }
        /// <summary>
        /// 判断地点是否已经探索出来
        /// </summary>
        /// <param name="part"></param>
        /// <param name="place"></param>
        /// <returns></returns>
        private bool Explored(int part, int place)
        {
            return (df.mapPlaceShowDate.ContainsKey(part)
                && df.mapPlaceShowDate[part].ContainsKey(place)
                && df.mapPlaceShowDate[part][place] > 0);
        }

        /// <summary>
        /// 分配特定种类资源的采集任务
        /// </summary>
        /// <param name="workType">资源种类</param>
        private void ArrangeWork(WorkType workType)
        {
            var size = int.Parse(df.partWorldMapDate[df.mianPartId][98]); // size of map
            var part = df.mianPartId;

            var maxPlace = -1;
            var maxRes = 0;
            var manNeed = 0x7fffffff;
            var manPool = UIDate.instance.GetUseManPower();

            for (int place = 0; place < size * size; place++)
            {
                if (Explored(part, place) && !df.PlaceIsBad(part, place) && !df.HaveWork(part, place))
                {
                    var res = UIDate.instance.GetWorkPower((int)workType, part, place);
                    var man = int.Parse(df.GetNewMapDate(part, place, 12));
                    if (res > maxRes && manPool >= man)
                    {
                        if (man <= 1 || !Main.Setting.skipTown)
                        {
                            maxRes = res;
                            maxPlace = place;
                            manNeed = man;
                        }
                    }
                }
            }
            if (maxRes > 0 && maxPlace >= 0)
            {
                if (manPool >= manNeed)
                {
                    var choosePartId = wms.choosePartId;
                    var choosePlaceId = wms.choosePlaceId;
                    var chooseWorkTyp = wms.chooseWorkTyp;

                    wms.choosePartId = part;
                    wms.choosePlaceId = maxPlace;
                    wms.chooseWorkTyp = (int)workType;
                    wms.DoManpowerWork();

                    wms.choosePartId = choosePartId;
                    wms.choosePlaceId = choosePlaceId;
                    wms.chooseWorkTyp = chooseWorkTyp;
                }
            }
            else
            {
                TipsWindow.instance.SetTips(0, new string[] { "<color=#AF3737FF>无资源可采集或人力不足</color>" }, 180);
            }
        }

        /// <summary>
        /// 取消任务
        /// </summary>
        /// <param name="worker"></param>
        private void CancelWork(Worker worker)
        {
            var manpowerList = DateFile.instance.manpowerUseList;
            if (manpowerList.ContainsKey(worker.part) &&
                manpowerList[worker.part].ContainsKey(worker.place))
            {
                var choosePartId = wms.choosePartId;
                var choosePlaceId = wms.choosePlaceId;

                wms.choosePartId = worker.part;
                wms.choosePlaceId = worker.place;
                wms.RemoveWorkingDate();

                wms.choosePartId = choosePartId;
                wms.choosePlaceId = choosePlaceId;
            }
        }

        /// <summary>
        /// 获取特定种类资源的所有采集任务
        /// </summary>
        /// <param name="workType">资源种类</param>
        /// <returns></returns>
        private IEnumerable<Worker> GetWorkers(WorkType workType)
        {
            Dictionary<int, Dictionary<int, int[]>> list = null;
            switch (workType)
            {
                case WorkType.FOOD:
                    list = df.foodUPList;
                    break;
                case WorkType.WOOD:
                    list = df.woodUPList;
                    break;
                case WorkType.STONE:
                    list = df.stoneUPList;
                    break;
                case WorkType.SILK:
                    list = df.silkUPList;
                    break;
                case WorkType.HERBAL:
                    list = df.herbalUPList;
                    break;
                case WorkType.MONEY:
                    list = df.moneyUPList;
                    break;
                default:
                    break;
            }
            if (list == null)
            {
                yield break;
            }
            foreach (var part in list.Keys)
            {
                foreach (var place in list[part].Keys)
                {
                    var position = df.GetNewMapDate(part, place, 98) +
                        df.GetNewMapDate(part, place, 0);
                    var manpower = list[part][place][1];
                    var type = (int)workType;
                    var resource = UIDate.instance.GetWorkPower(type, part, place);
                    var worker = new Worker
                    {
                        content = $"{df.SetColoer(10003, position)}：+" +
                            $"{df.SetColoer(10002, resource.ToString())}/" +
                            $"{df.SetColoer(10005, manpower.ToString())}人/时节",
                        part = part,
                        place = place,
                        manpower = manpower,
                        type = type,
                    };
                    yield return worker;
                }
            }
        }
    }

    /// <summary>
    /// 修复🍆人力回复列表的bug
    /// </summary>
    [HarmonyPatch(typeof(UIDate), "AddBackManpower")]
    public class UIDate_AddBackManpower_Patch
    {
        public static bool Prefix(int partId, int placeId, int menpower, int time)
        {
            var df = DateFile.instance;
            if (!df.backManpowerList.ContainsKey(partId))
            {
                df.backManpowerList.Add(partId, new Dictionary<int, int[]>());
            }

            var size = int.Parse(df.partWorldMapDate[df.mianPartId][98]);
            while (df.backManpowerList[partId].ContainsKey(placeId))
            {
                // 防止key重复。如果在同一地点人力未恢复完成，再次分配人力然后取消，则会出现。
                placeId += size * size;
            }

            df.backManpowerList[partId].Add(placeId, new int[2]
            {
                menpower,
                time
            });

            return false;
        }
    }

    /// <summary>
    /// 分辨率调整时调整窗体样式
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "SetScreenResolution")]
    public class DateFile_SetScreenResolution_Patch
    {
        public static void Prefix(int index, DateFile __instance)
        {
            if (VillagersList.Instance.Open)
            {
                VillagersList.Instance.ToggleWindow();
            }
            var width = __instance.gameResolutions[index].width;
            var height = __instance.gameResolutions[index].height;
            Main.Setting.FitFont(width, height);
            VillagersList.Instance.CalcWindow();
        }
    }

    /// <summary>
    /// 载入游戏时加载VillageList类
    /// </summary>
    [HarmonyPatch(typeof(WorldMapSystem), "Start")]
    public class WorldMapSystem_Start_Patch
    {
        public static void Postfix()
        {
            Main.logger.Log($"create ui: {VillagersList.Load()}");
        }
    }

    public class Settings : UnityModManager.ModSettings
    {
        [XmlIgnore]
        public int labelSize = 16;
        [XmlIgnore]
        public int buttonSize = 12;

        public bool skipTown = true;
        public KeyCode key = KeyCode.F9;

        public void FitFont(int width = 0, int height = 0)
        {
            if (width == 0)
            {
                width = Screen.width;
            }
            if (height == 0)
            {
                height = Screen.height;
            }
            if (height <= 800)
            {
                labelSize = 16;
                buttonSize = 12;
            }
            else if (height <= 900)
            {
                labelSize = 18;
                buttonSize = 15;
            }
            else if (height <= 1100)
            {
                labelSize = 20;
                buttonSize = 16;
            }
            else
            {
                labelSize = 22;
                buttonSize = 18;
            }
            Main.logger.Log($"width:{width}, height:{height}, label:{labelSize}, button:{buttonSize}");
        }
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    public class Main
    {
        public static UnityModManager.ModEntry.ModLogger logger;
        public static Settings Setting;
        public static bool enabled;

        public static bool bindingKey = false;

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            Setting = Settings.Load<Settings>(modEntry);
            modEntry.OnToggle = new Func<UnityModManager.ModEntry, bool, bool>(OnToggle);
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            Setting.FitFont();
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
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
                        Setting.key = e.keyCode;
                    }
                    bindingKey = false;
                }
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("设置快捷键： Ctrl +", GUILayout.Width(130));
            if (GUILayout.Button((bindingKey ? "请按键" : Setting.key.ToString()),
                GUILayout.Width(80)))
            {
                bindingKey = !bindingKey;
            }
            GUILayout.Label("（支持0-9,A-Z,F1-F12）");
            Setting.skipTown = GUILayout.Toggle(Setting.skipTown, "忽略城镇");
            GUILayout.EndHorizontal();
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Setting.Save(modEntry);
        }
    }
}