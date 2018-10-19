using Harmony12;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace VillageHeadOfTaiwu
{
    class VillagersList : MonoBehaviour
    {
        class Worker
        {
            public string content;
            public int part;
            public int place;
            public int manpower;
            public int type;
        }

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

        static VillagersList instance;
        static GameObject obj;

        GameObject canvas;

        Rect windowRect;
        Vector2 scrollPosition;

        bool open;
        bool cursorLock;
        //bool collapse;

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
            instance = this;
            DontDestroyOnLoad(this);
        }

        public void Start()
        {
            Main.logger.Log("start");

            open = false;
            //collapse = false;

            windowRect = new Rect(Screen.width * 0.833f, 50f, Screen.width * 0.164f, 0);
            scrollPosition = Vector2.zero;

            windowStyle = new GUIStyle
            {
                name = "window",
                padding = new RectOffset(5, 5, 5, 5),
            };

            collapseStyle = new GUIStyle
            {
                name = "collapse",
                fontSize = 24,
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(5, 5, 5, 5),
            };
            collapseStyle.normal.textColor = Color.blue;

            labelStyle = new GUIStyle
            {
                name = "label",
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(0, 0, 5, 0),
            };
            labelStyle.normal.textColor = Color.yellow;

            buttonStyle = new GUIStyle
            {
                name = "button",
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                fixedWidth = windowRect.width - 40,
                margin = new RectOffset(0, 0, 5, 0),
            };
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.richText = true;

            seperatorStyle = new GUIStyle
            {
                name = "seperator",
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                fixedWidth = windowRect.width - 40,
            };
            seperatorStyle.normal.textColor = Color.cyan;

            df = DateFile.instance;
            wms = WorldMapSystem.instance;
        }

        public void OnGUI()
        {
            if (open)
            {
                var bgColor = GUI.backgroundColor;
                var color = GUI.color;

                GUI.backgroundColor = Color.black;
                GUI.color = Color.white;
                windowRect = GUILayout.Window(666, windowRect, WindowFunc, "",
                    windowStyle, GUILayout.Height(Screen.height * 0.8f));

                GUI.backgroundColor = bgColor;
                GUI.color = color;
            }

        }

        private void WindowFunc(int windowId)
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < 6; i++)
            {
                if (GUILayout.Button(workStr[i], labelStyle))
                {
                    ArrangeWork(workType: i);
                }
                if (i < 5)
                {
                    GUILayout.Label("|");
                }
            }
            GUILayout.EndHorizontal();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false,
                GUILayout.Width(windowRect.width - 20),
                GUILayout.MaxHeight(Screen.height - 200));
            GUILayout.BeginVertical();
            //if (GUILayout.Button("收起", collapseStyle))
            //{
            //    collapse = !collapse;
            //}
            //if (!collapse)
            {
                for (int i = 0; i < 6; i++)
                {
                    GUILayout.Label($"------------{workStr[i]}-------------", seperatorStyle);
                    var wokers = new List<Worker>(GetWorkers((WorkType)i));
                    foreach (var worker in wokers)
                    {
                        if (GUILayout.Button(worker.content, buttonStyle))
                        {
                            CancelWork(worker);
                        }
                    }
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private bool Explored(int part, int place)
        {
            return (df.mapPlaceShowDate.ContainsKey(part)
                && df.mapPlaceShowDate[part].ContainsKey(place)
                && df.mapPlaceShowDate[part][place] > 0);
        }

        private void ArrangeWork(int workType)
        {
            var size = int.Parse(df.partWorldMapDate[df.mianPartId][98]); // size of map
            var part = df.mianPartId;

            var maxPlace = -1;
            var maxRes = 0;

            for (int place = 0; place < size * size; place++)
            {
                if (Explored(part, place) && !df.PlaceIsBad(part, place) && !df.HaveWork(part, place))
                {
                    var res = UIDate.instance.GetWorkPower((int)workType, part, place);
                    if (res > maxRes)
                    {
                        maxRes = res;
                        maxPlace = place;
                    }
                }
            }
            if (maxRes > 0 && maxPlace >= 0)
            {
                var manPool = UIDate.instance.GetUseManPower();
                var manNeed = int.Parse(df.GetNewMapDate(part, maxPlace, 12));
                if (manPool >= manNeed)
                {
                    var choosePartId = wms.choosePartId;
                    var choosePlaceId = wms.choosePlaceId;
                    var chooseWorkTyp = wms.chooseWorkTyp;

                    wms.choosePartId = part;
                    wms.choosePlaceId = maxPlace;
                    wms.chooseWorkTyp = workType;
                    wms.DoManpowerWork();

                    wms.choosePartId = choosePartId;
                    wms.choosePlaceId = choosePlaceId;
                    wms.chooseWorkTyp = chooseWorkTyp;
                    return;
                }
            }

        }

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

        private IEnumerable<Worker> GetWorkers(WorkType work)
        {
            Dictionary<int, Dictionary<int, int[]>> list = null;
            switch (work)
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
                    var type = (int)work;
                    var resource = UIDate.instance.GetWorkPower(type, part, place);
                    var worker = new Worker
                    {
                        content = $"{df.SetColoer(10003, position)}：+" +
                            $"{df.SetColoer(10002, resource.ToString())}/" +
                            $"{df.SetColoer(10005, manpower.ToString())}人 /时节",
                        part = part,
                        place = place,
                        manpower = manpower,
                        type = type,
                    };
                    yield return worker;
                }
            }
        }

        public void Update()
        {
            if (!Main.enabled)
            {
                Destroy(canvas);
            }
            if (open)
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
                && Input.GetKeyUp(Main.settings.key))
            {
                ToggleWindow();
            }
        }

        private void ToggleWindow()
        {
            Main.logger.Log($"Toggle {(!open ? "on" : "off")}");
            open = !open;
            BlockGameUI(open);
            if (open)
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

        private void BlockGameUI(bool open)
        {
            if (open)
            {
                canvas = new GameObject("", typeof(Canvas), typeof(GraphicRaycaster));
                canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
                canvas.GetComponent<Canvas>().sortingOrder = short.MaxValue;
                DontDestroyOnLoad(canvas);
                var panel = new GameObject("", typeof(Image));
                panel.transform.SetParent(canvas.transform);
                panel.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0);
                panel.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                panel.GetComponent<RectTransform>().offsetMin = Vector2.zero;
                panel.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            }
            else
            {
                Destroy(canvas);
            }
        }
    }

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
        public KeyCode key = KeyCode.F8;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    public class Main
    {
        public static UnityModManager.ModEntry.ModLogger logger;
        public static Settings settings;
        public static bool enabled;

        public static bool bindingKey = false;

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);
            logger = modEntry.Logger;
            modEntry.OnToggle = new Func<UnityModManager.ModEntry, bool, bool>(OnToggle);
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
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
                        settings.key = e.keyCode;
                    }
                    bindingKey = false;
                }
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("设置快捷键： Ctrl +", GUILayout.Width(130));
            if (GUILayout.Button((bindingKey ? "请按键":settings.key.ToString()),
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