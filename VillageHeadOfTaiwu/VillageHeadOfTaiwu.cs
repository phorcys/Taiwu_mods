using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace Sth4nothing.VillageHeadOfTaiwu
{
    public class ReflectionMethod
    {
        private const BindingFlags Flags = BindingFlags.Instance
                                           | BindingFlags.Static
                                           | BindingFlags.NonPublic
                                           | BindingFlags.Public;
        /// <summary>
        /// 反射执行方法
        /// </summary>
        /// <param name="instance">类实例(静态方法则为null)</param>
        /// <param name="method">方法名</param>
        /// <param name="args">方法的参数类型列表</param>
        /// <typeparam name="T1">类</typeparam>
        /// <typeparam name="T2">返回值类型</typeparam>
        /// <returns></returns>
        public static T2 Invoke<T1, T2>(T1 instance, string method, params object[] args)
        {
            return (T2)typeof(T1).GetMethod(method, Flags)?.Invoke(instance, args);
        }
        /// <summary>
        /// 反射执行方法
        /// </summary>
        /// <param name="instance">类实例(静态方法则为null)</param>
        /// <param name="method">方法名</param>
        /// <param name="args">方法的参数类型列表</param>
        /// <typeparam name="T1">类</typeparam>
        public static void Invoke<T1>(T1 instance, string method, params object[] args)
        {
            typeof(T1).GetMethod(method, Flags)?.Invoke(instance, args);
        }
        /// <summary>
        /// 反射执行方法
        /// </summary>
        /// <param name="instance">类实例(静态方法则为null)</param>
        /// <param name="method">方法名</param>
        /// <param name="argTypes">方法的参数类型列表</param>
        /// <param name="args">参数</param>
        /// <typeparam name="T">类</typeparam>
        /// <returns>函数的返回值(void则返回null)</returns>
        public static object Invoke<T>(T instance, string method, System.Type[] argTypes, params object[] args)
        {
            argTypes = argTypes ?? new System.Type[0];
            var methods = typeof(T).GetMethods(Flags).Where(m =>
            {
                if (m.Name != method)
                    return false;
                return m.GetParameters()
                    .Select(p => p.ParameterType)
                    .SequenceEqual(argTypes);
            });

            if (methods.Count() != 1)
            {
                throw new AmbiguousMatchException("cannot find method to invoke");
            }

            return methods.First()?.Invoke(instance, args);
        }
        /// <summary>
        /// 反射获取类字段的值
        /// </summary>
        /// <param name="instance">类实例(静态字段则为null)</param>
        /// <param name="field">字段名</param>
        /// <typeparam name="T1">类</typeparam>
        /// <typeparam name="T2">返回值类型</typeparam>
        /// <returns>字段的值</returns>
        public static T2 GetValue<T1, T2>(T1 instance, string field)
        {
            return (T2)typeof(T1).GetField(field, Flags)?.GetValue(instance);
        }
        /// <summary>
        /// 反射获取类字段的值
        /// </summary>
        /// <param name="instance">类实例(静态字段则为null)</param>
        /// <param name="field">字段名</param>
        /// <typeparam name="T">类</typeparam>
        /// <returns>字段的值</returns>
        public static object GetValue<T>(T instance, string field)
        {
            return typeof(T).GetField(field, Flags)?.GetValue(instance);
        }
        /// <summary>
        /// 反射设置类字段的值
        /// </summary>
        /// <param name="instance">类实例(静态字段则为null)</param>
        /// <param name="field">字段名</param>
        /// <param name="value">设置的字段的值</param>
        /// <typeparam name="T">类</typeparam>
        public static void SetValue<T>(T instance, string field, object value)
        {
            typeof(T).GetField(field, Flags)?.SetValue(instance, value);
        }
    }
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
        /// <summary>
        /// 每时节采集的资源量
        /// </summary>
        public int resource;
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

    class VillagersList : MonoBehaviour
    {
        private readonly string[] workStr = {
            "食材",
            "木材",
            "金石",
            "织物",
            "药材",
            "银钱"
        };

        private const float designWidth = 1600f, designHeight = 900f;

        public static VillagersList Instance { get; private set; }
        private static GameObject obj;

        GameObject canvas;

        Rect windowRect;
        Vector2 scrollPosition;

        public bool Open { get; private set; }
        bool cursorLock;
        bool collapse;
        bool[] showItems;

        GUIStyle windowStyle, collapseStyle, buttonStyle, seperatorStyle, itemStyle, seperatorStyle2;

        DateFile df => DateFile.instance;
        WorldMapSystem wms => WorldMapSystem.instance;

        public static bool Load()
        {
            try
            {
                if (obj == null)
                {
                    obj = new GameObject("Villagers", typeof(VillagersList));
                    DontDestroyOnLoad(obj);
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
            Main.Logger.Log("awake");
            Instance = this;
            DontDestroyOnLoad(this);
        }

        public void Start()
        {
            Main.Logger.Log("start");

            Open = false;
            collapse = true;

            showItems = new bool[] { true, true, true, true, true, true };
            scrollPosition = Vector2.zero;
            windowRect = new Rect(designWidth * 0.85f, designHeight * 0.05f, designWidth * 0.145f, 0);

            Init();

            ToggleWindow();
        }

        public void Init()
        {
            // 设置点击事件
            var manpowerText = ReflectionMethod.GetValue<ui_TopBar, CText>(ui_TopBar_Awake_Patch.TopBar, "ManpowerText");
            var btn = manpowerText.transform.parent.parent.gameObject.AddComponent<Button>();
            btn.targetGraphic = manpowerText;
            btn.interactable = true;
            btn.onClick.AddListener(ToggleWindow);
        }
        private void PrepareGUI()
        {
            windowStyle = new GUIStyle
            {
                name = "window",
                padding = new RectOffset(5, 5, 5, 5),
            };
            collapseStyle = new GUIStyle(GUI.skin.button);
            collapseStyle.name = "collapse";
            // fontSize = 12,
            collapseStyle.margin = new RectOffset(0, 0, 0, 0);
            collapseStyle.alignment = TextAnchor.MiddleRight;
            collapseStyle.fixedWidth = 25f;
            collapseStyle.fixedHeight = 25f;
            collapseStyle.normal.textColor = Color.red;

            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.name = "button";
            buttonStyle.margin = new RectOffset(0, 0, 0, 0);
            buttonStyle.alignment = TextAnchor.MiddleCenter;
            buttonStyle.fontSize = Main.Setting.buttonSize;
            buttonStyle.normal.textColor = Color.yellow;

            itemStyle = new GUIStyle
            {
                name = "item",
                alignment = TextAnchor.MiddleLeft,
                fontSize = Main.Setting.itemSize,
                richText = true,
            };
            itemStyle.normal.textColor = Color.white;

            seperatorStyle = new GUIStyle(GUI.skin.button);
            seperatorStyle.name = "seperator";
            seperatorStyle.alignment = TextAnchor.MiddleCenter;
            seperatorStyle.fontSize = Main.Setting.itemSize - 1;
            seperatorStyle.normal.textColor = Color.cyan;

            seperatorStyle2 = new GUIStyle(GUI.skin.button);
            seperatorStyle2.name = "seperator";
            seperatorStyle2.alignment = TextAnchor.MiddleCenter;
            seperatorStyle2.fontSize = Main.Setting.itemSize - 1;
            seperatorStyle2.normal.textColor = Color.green;
        }

        public void OnGUI()
        {
            if (Open)
            {
                PrepareGUI();

                var bgColor = GUI.backgroundColor;
                var color = GUI.color;

                Matrix4x4 svMat = GUI.matrix;
                Vector2 resizeRatio = new Vector2((float)Screen.width / designWidth, (float)Screen.height / designHeight);
                GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(resizeRatio.x, resizeRatio.y, 1.0f));

                GUI.backgroundColor = Color.black;
                GUI.color = Color.white;

                windowRect = GUILayout.Window(666, windowRect, WindowFunc, "",
                    windowStyle, GUILayout.Height(designHeight * 0.73f));

                GUI.matrix = svMat;
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
                Main.Logger.Log(collapse ? "展" : "收");
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
                    if (GUILayout.Button(workStr[i], buttonStyle))
                    {
                        ArrangeWork((WorkType)i);
                    }
                    if (GUILayout.Button("-", buttonStyle))
                    {
                        var workers = GetWorkers((WorkType)i);
                        if (workers.Count() > 0)
                        {
                            var minRes = workers.Min((w) => w.resource);
                            var worker = workers.First((w) => w.resource == minRes);
                            CancelWork(worker);
                        }
                    }
                    if (i % 3 == 2)
                    {
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndVertical();
                canvas.transform.Find("panel").GetComponent<RectTransform>().anchorMin = new Vector2(windowRect.x / designWidth, 0.22f);

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false,
                    GUILayout.Width(windowRect.width - 20), GUILayout.MaxHeight(designHeight * 0.73f));
                GUILayout.BeginVertical();
                for (int i = 0; i < 6; i++)
                {
                    if (GUILayout.Button(workStr[i], (showItems[i] ? seperatorStyle : seperatorStyle2)))
                    {
                        showItems[i] = !showItems[i];
                    }
                    if (showItems[i])
                    {
                        GetWorkers((WorkType)i).OrderBy(OrderFunc).Do((worker) =>
                        {
                            if (GUILayout.Button(worker.content, itemStyle))
                            {
                                CancelWork(worker);
                            }
                        });
                    }
                }
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
            else
            {
                canvas.transform.Find("panel").GetComponent<RectTransform>().anchorMin = new Vector2(1f - 25f / designWidth, 0.95f - 25f / designHeight);
            }
        }

        public void Update()
        {
            if (!Main.Enabled)
            {
                Destroy(canvas);
                Open = false;
            }
            if (Open)
            {
                if (Input.GetKey(KeyCode.PageUp))
                {
                    scrollPosition.y -= 40;
                    Main.Logger.Log($"pgup {scrollPosition}");
                }
                if (Input.GetKey(KeyCode.PageDown))
                {
                    scrollPosition.y += 40;
                    Main.Logger.Log($"pgdn {scrollPosition}");
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
            Main.Logger.Log($"Toggle {(!Open ? "on" : "off")}");
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
                panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.4f);
                panel.GetComponent<RectTransform>().anchorMin = new Vector2(1f - 25f / designWidth, 0.95f - 25f / designHeight);
                panel.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.95f);
                panel.GetComponent<RectTransform>().offsetMin = Vector2.zero;
                panel.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            }
            else
            {
                Destroy(canvas);
            }
        }

        static int OrderFunc(Worker worker)
        {
            return Main.Setting.reverse ? -worker.resource : worker.resource;
        }

        /// <summary>
        /// 分配特定种类资源的采集任务
        /// </summary>
        /// <param name="workType">资源种类</param>
        private void ArrangeWork(WorkType workType)
        {
            // var part = df.mianPartId;

            var maxPart = -1;
            var maxPlace = -1;
            var maxRes = 0f;
            var manNeed = 0x7fffffff;
            var manPool = UIDate.instance.GetUseManPower();

            IEnumerable<int> parts = null;
            switch (Main.Setting.area)
            {
                case 1:
                    parts = new int[] { int.Parse(df.GetGangDate(16, 3)) };
                    break;
                case 2:
                    parts = df.partWorldMapDate.Keys;
                    break;
                case 0:
                default:
                    parts = new int[] { df.mianPartId };
                    break;
            }

            foreach (int part in parts)
            {
                var size = int.Parse(df.partWorldMapDate[part][98]); // size of map
                for (int place = 0; place < size * size; place++)
                {
                    if (df.HaveShow(part, place) > 0
                        && !df.PlaceIsBad(part, place)
                        && !df.HaveWork(part, place))
                    {
                        var man = df.GetMarkNeedManPower(part, place);
                        var res = man == 0 ? 0f : UIDate.instance.GetWorkPower((int)workType, part, place) / (float)man;
                        if (res > maxRes && manPool >= man)
                        {
                            if (man <= 1 || !Main.Setting.skipTown)
                            {
                                maxRes = res;
                                maxPart = part;
                                maxPlace = place;
                                manNeed = man;
                            }
                        }
                    }
                }
            }
            if (maxRes > 0 && maxPart >= 0 && maxPlace >= 0)
            {
                if (manPool >= manNeed)
                {
                    Debug.Log($"开始在{maxPart},{maxPlace}采集{workStr[(int)workType]}({maxRes}/{manNeed}人力/时节)");
                    ChoosePlaceWindow.Instance.SetPlaceWork(maxPart, maxPlace, (int)workType);
                    UpdateUiManpower();
                }
            }
            else
            {
                TipsWindow.instance.SetTips(0,
                    new string[] { "<color=#AF3737FF>无资源可采集或人力不足</color>" }, 180);
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
                Debug.Log($"移除{worker.part},{worker.place}的采集{workStr[worker.type]}({worker.resource}/{worker.manpower}人力/时节)");
                ChoosePlaceWindow.Instance.RemovePlaceWork(worker.part, worker.place);
                DateFile.instance.MarkPlace(worker.part, worker.place, false);
                UpdateUiManpower();
            }
        }
        private void UpdateUiManpower()
        {
            var mpg = GameObject.FindObjectOfType<ui_ManPowerManage>();
            if (mpg == null) return;
            ReflectionMethod.Invoke(mpg, "UpdateTotalManPower");
            ReflectionMethod.Invoke(mpg, "UpdateMarkedPlace");
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
                        resource = resource,
                        type = type,
                    };
                    yield return worker;
                }
            }
        }
    }

    // /// <summary>
    // /// 修复🍆人力回复列表的bug
    // /// </summary>
    // [HarmonyPatch(typeof(UIDate), "AddBackManpower")]
    // public class UIDate_AddBackManpower_Patch
    // {
    //     public static bool Prefix(int partId, int placeId, int menpower)
    //     {
    //         var df = DateFile.instance;
    //         int time = 0;
    //         if (placeId != df.mianPlaceId)
    //         {
    //             time++;
    //         }
    //         if (partId != int.Parse(df.GetGangDate(16, 3)))
    //         {
    //             time++;
    //         }
    //         if (df.GetWorldId(partId) != int.Parse(df.GetGangDate(16, 11)))
    //         {
    //             time++;
    //         }
    //         if (time <= 0)
    //         {
    //             return false;
    //         }
    //         if (!df.backManpowerList.ContainsKey(partId))
    //         {
    //             df.backManpowerList.Add(partId, new Dictionary<int, int[]>());
    //         }

    //         var size = int.Parse(df.partWorldMapDate[partId][98]);
    //         while (df.backManpowerList[partId].ContainsKey(placeId))
    //         {
    //             // 防止key重复。如果在同一地点人力未恢复完成，再次分配人力然后取消，则会出现。
    //             placeId += size * size;
    //         }

    //         df.backManpowerList[partId].Add(placeId, new int[2]
    //         {
    //             menpower,
    //             time
    //         });

    //         return false;
    //     }
    // }

    /// <summary>
    /// 返回主界面时关闭窗口
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "BackToStartMenu")]
    public class DateFile_BackToStartMenu_Patch
    {
        static void Prefix()
        {
            if (VillagersList.Instance != null && VillagersList.Instance.Open)
            {
                VillagersList.Instance.ToggleWindow();
            }
        }
    }

    [HarmonyPatch(typeof(ui_TopBar), "Awake")]
    public class ui_TopBar_Awake_Patch
    {
        public static ui_TopBar TopBar { get; private set; }
        private static void Postfix(ui_TopBar __instance)
        {
            Debug.Log("ui_Topbar awake!");
            TopBar = __instance;

            if (VillagersList.Instance == null)
            {
                Main.Logger.Log($"create ui: {VillagersList.Load()}");
            }
            else
            {
                VillagersList.Instance.Init();
                if (!VillagersList.Instance.Open)
                    VillagersList.Instance.ToggleWindow();
            }
        }

    }
    public class Settings : UnityModManager.ModSettings
    {
        [XmlIgnore]
        public int buttonSize = 18;
        [XmlIgnore]
        public int itemSize = 14;
        /// <summary>
        /// 是否跳过城镇
        /// </summary>
        public bool skipTown = false;
        /// <summary>
        /// 热键 ctrl + ？
        /// </summary>
        public KeyCode key = KeyCode.F9;
        /// <summary>
        /// 是否逆序
        /// </summary>
        public bool reverse = false;
        /// <summary>
        /// 是否采集所有地图
        /// </summary>
        public int area = 0;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    public class Main
    {
        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }
        public static Settings Setting { get; private set; }
        public static bool Enabled { get; private set; }

        public static string[] areas = { "当前地图", "太吾村", "所有地图" };

        public static bool bindingKey = false;

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            Setting = Settings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
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
            Setting.reverse = GUILayout.Toggle(Setting.reverse, "降序排列");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("选择采集区域: ", GUILayout.Width(100));
            Setting.area = GUILayout.SelectionGrid(Setting.area, areas, areas.Length);
            GUILayout.EndHorizontal();
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Setting.Save(modEntry);
        }
    }
}