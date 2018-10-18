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

        readonly string[] workString = {
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
        //bool collapse;

        GUIStyle windowStyle;
        GUIStyle collapseStyle;
        GUIStyle labelStyle;
        GUIStyle buttonStyle;
        GUIStyle scrollStyle;

        DateFile df;

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

            windowRect = new Rect(Screen.width * 0.85f, 50f, Screen.width * 0.15f, 0);
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
                fontSize = 20,
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(0, 0, 5, 0),
            };
            labelStyle.normal.textColor = Color.yellow;

            buttonStyle = new GUIStyle
            {
                name = "button",
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(0, 0, 5, 0),
            };
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.richText = true;

            scrollStyle = new GUIStyle
            {
                fixedHeight = 0f,
                fixedWidth = windowRect.width - 10,
                stretchHeight = true,
                stretchWidth = false,
            };

            df = DateFile.instance;
        }

        public void OnGUI()
        {
            if (open)
            {
                var bgColor = GUI.backgroundColor;
                var color = GUI.color;

                GUI.backgroundColor = Color.black;
                GUI.color = Color.white;
                windowRect = GUILayout.Window(666, windowRect, windowFunc, "",
                    windowStyle, GUILayout.Height(Screen.height * 0.8f));

                GUI.backgroundColor = bgColor;
                GUI.color = color;
            }

        }

        private void windowFunc(int windowId)
        {
            GUILayout.BeginScrollView(scrollPosition, scrollStyle);
            GUILayout.BeginVertical("box");
            //if (GUILayout.Button("收起", collapseStyle))
            //{
            //    collapse = !collapse;
            //}
            //if (!collapse)
            {
                for (int i = 0; i < 6; i++)
                {
                    var workStr = workString[i];
                    var type = (WorkType)i;
                    if (GUILayout.Button(workStr, labelStyle))
                    {
                        ArrangeWork(type);
                    }
                    var wokers = new List<Worker>(GetWorkers(type));
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

        private void ArrangeWork(WorkType workType)
        {
            var size = int.Parse(df.partWorldMapDate[df.mianPartId][98]); // size of map
            var part = df.mianPartId;

            var maxPlace = -1;
            var maxRes = 0;

            for (int place = 0; place < size * size; place++)
            {
                if (Explored(part, place) && !df.PlaceIsBad(part, place))
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
                    WorldMapSystem.instance.choosePartId = part;
                    WorldMapSystem.instance.choosePlaceId = maxPlace;
                    WorldMapSystem.instance.chooseWorkTyp = (int)workType;
                    WorldMapSystem.instance.DoManpowerWork();
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
                var wms = WorldMapSystem.instance;
                var type = typeof(WorldMapSystem);
                type.GetField("removeWorkPart", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(wms, worker.part);
                type.GetField("removeWorkPlace", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(wms, worker.place);
                wms.IconRemoveWorkingDate();
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
            if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl))
                && Input.GetKeyUp(KeyCode.F9))
            {
                ToggleWindow();
            }
        }

        private void ToggleWindow()
        {
            Main.logger.Log($"Toggle {(!open ? "on" : "off")}");
            open = !open;
            BlockGameUI(open);
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

    public class Main
    {
        public static UnityModManager.ModEntry.ModLogger logger;

        public static bool enabled;

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            modEntry.OnToggle = new Func<UnityModManager.ModEntry, bool, bool>(OnToggle);
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }
    }
}