using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using HC.UI;

namespace IllustratedHandbook
{
    public class IllustratedHandbookUI : MonoBehaviour
    {
        DateFile dataInstance;

        // 是否显示窗口
        public static bool mainCavasActive = true;


        Canvas mainCanvas; // 主控
        Image mainPanel; // 主面板
        Image handbookPanel; // 程序面板
        Image loadedPanel; // 功能面板
        Image loadingPanel; // 初始化面板
        Image filterPanel; // 过滤面板
        Image itemTypePanel; // 过滤面板 -> 物品分类面板
        Image itemLevelPanel; // 过滤面板 -> 物品品级面板
        GameObject filterTypeCurrent; // 过滤面板 -> 当前分类文字
        GameObject filterLevelCurrent;// 过滤面板 -> 当前品级文字
        Image loadingTextPanel; // 初始化文字面板
        Image itemPanel; // 功能面板 -> 物品面板
        Image titlePanel; // 标题
        EventTrigger titlePanelOndrag; // 拖动事件
        GameObject titleText; // 标题 -> 标题文字
        GameObject tipText; // 初始化面板 -> 提示
        GameObject aboutText; // 初始化文字面板 -> 关于
        Text itemPageText;  // 功能面板 -> 当前页
        GameObject NGAButton;
        GameObject GithubButton;
        GameObject itemGrid; // 物品Grid
        GameObject prePageButton; // 功能面板 -> 上一页按键
        GameObject nextPageButton; // 功能面板 -> 下一页按键
        List<GameObject> itemSlots = new List<GameObject>(); // 54个物品槽

        Sprite[] itemBack; // 物品槽背景图片
        Sprite[] itemIcon; // 物品图标

        Vector3 offsetPostion; // 用以记录拖拽事件偏移
        Vector3 offsetPostionLoding;

        int selectedLevel = 0; // 选择的物品品级
        int selectedType = 0;  // 选择的物品种类

        int selectedLevelUnsaved = 0;
        int selectedTypeUnsaved = 0;

        string itemType = "无";
        string[] itemLevel = { "全部|#FFF", "九品|#8E8E8EFF", "八品|#FBFBFBFF", "七品|#6DB75FFF", "六品|#8FBAE7FF", "五品|#63CED0FF", "四品|#AE5AC8FF", "三品|#E3C66DFF", "二品|#F28234FF", "一品|#E4504DFF" };
        Dictionary<int, Dictionary<int, string>> itemList = new Dictionary<int, Dictionary<int, string>>();
        int itemSlotPage = 0; // 当前所处页

        // 是否到达过最后一页 用来控制itemSlots的开关
        bool finalPageReached = false;
        // 判断游戏是否加载
        private bool gameLoaded = false;
        private bool gameExited = false;

        void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            DontDestroyOnLoad(this);
            InitUI();
        }

        void Update()
        {

            if (!gameLoaded)
            {
                if (DateFile.instance != null && ActorMenu.instance != null)
                {
                    gameLoaded = true;
                    OnGameLoaded();
                }
            }

            // 记录点击位置偏移值
            if (Input.GetMouseButtonDown(0))
            {
                // 如果游戏未加载 那么UI渲染模式为ScreenSpaceOverlay, 此时直接使用屏幕位置即可
                // 如果游戏已加载 那么UI将通过MainCamera渲染, 此时需要将屏幕位置换算成世界位置
                if (gameLoaded && !gameExited)
                    offsetPostion = mainPanel.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainPanel.transform.position.z));
                else
                    offsetPostionLoding = mainPanel.transform.position - Input.mousePosition;
            }

            if (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyUp(KeyCode.F11))
                {
                    if (mainCavasActive)
                    {
                        mainCavasActive = false;
                        mainCanvas.gameObject.SetActive(false);
                    }
                    else if (!mainCavasActive)
                    {
                        mainCavasActive = true;
                        mainCanvas.gameObject.SetActive(true);
                    }
                }
            }
        }

        // 用来判断是否返回了主菜单
        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "1_StartMenu")
            {
                if (gameLoaded)
                {
                    gameExited = true;
                    mainCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                    loadedPanel.gameObject.SetActive(false);
                    filterPanel.gameObject.SetActive(false);
                    loadingPanel.gameObject.SetActive(true);
                }
            }
            else if (scene.name == "3_WorldMap")
            {
                if (gameExited)
                {
                    gameExited = false;
                    mainCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
                    mainCanvas.GetComponent<Canvas>().worldCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
                    loadedPanel.gameObject.SetActive(true);
                    filterPanel.gameObject.SetActive(false);
                    loadingPanel.gameObject.SetActive(false);
                }
            }
        }
        void OnGameLoaded()
        {
            dataInstance = DateFile.instance;
            itemType = DateFile.instance.massageDate[301][0];
            itemIcon = GetSprites.instance.itemSprites;
            itemBack = GetSprites.instance.itemBackSprites;

            itemList = dataInstance.presetitemDate;

            // 游戏加载后将渲染模式改为ScreenSpaceCamera并附加MainCamera
            mainCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
            mainCanvas.GetComponent<Canvas>().worldCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
            mainCanvas.GetComponent<Canvas>().sortingLayerName = "GUI";
            mainCanvas.GetComponent<Canvas>().sortingOrder = 601;  // ShowTips 的 sortingorder 为1000
            mainCanvas.transform.position = new Vector3(0, 0, 10);
            loadingPanel.gameObject.SetActive(false);

            // 在过滤面板创建物品类别按钮
            string[] itemTypes = itemType.Split('|');
            for (int i = 0; i < itemTypes.Length; i++)
            {
                var itemTypeButton = CreateButton("ItemTypeButton|" + itemTypes[i], itemTypePanel.gameObject, itemTypes[i], Color.white, "Graphics/BaseUI/GUI_Base", Color.white, v2(0, 1), v2(0, 1), v2(100, 30), v3(100, -30, 0));

                int selectedIndex = i;

                itemTypeButton.GetComponent<Button>().onClick.AddListener(
                    () =>
                    {
                        selectedTypeUnsaved = selectedIndex;
                        filterTypeCurrent.GetComponent<Text>().text = itemTypes[selectedTypeUnsaved];
                    }
                );
            }

            // 设置品级过滤按钮
            for (int i = 0; i < itemLevel.Length; i++)
            {
                string[] levelSplit = itemLevel[i].Split('|');
                Color textColor = Color.white;

                // 转换HEX颜色至RGB颜色
                ColorUtility.TryParseHtmlString(levelSplit[1], out textColor);

                var itemLevelButton = CreateButton("ItemLevelButton|" + i, itemLevelPanel.gameObject, levelSplit[0], textColor, "Graphics/BaseUI/GUI_Base", Color.white, v2(0, 1), v2(0, 1), v2(100, 30), v3(100, -30, 0));

                int selectedIndex = i;

                itemLevelButton.GetComponent<Button>().onClick.AddListener(
                    () =>
                    {
                        selectedLevelUnsaved = selectedIndex;
                        filterLevelCurrent.GetComponent<Text>().text = itemLevel[selectedIndex].Split('|')[0];
                    }
                );
            }
            // 初始化物品
            DrawItems();

            loadedPanel.gameObject.SetActive(true);

            Debug.Log("OnLoadedBreakPoint");
        }


        void InitUI()
        {
            // 主Canvs 开关
            #region MainCanvas
            mainCanvas = UICreator.CreateCanvas(name: "IllustratedHandbookCanvas");
#if UNITY_EDITOR
        mainCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        mainCanvas.GetComponent<Canvas>().worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
#else
            mainCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
#endif
            mainCanvas.GetComponent<Canvas>().sortingLayerName = "GUI";
            mainCanvas.GetComponent<Canvas>().sortingOrder = 998;  // ShowTips 的 sortingorder 为1000
            DontDestroyOnLoad(mainCanvas);
            mainPanel = UICreator.CreatePanel(mainCanvas.gameObject, "MainPanel");
            mainPanel.sprite = Resources.Load("Graphics/BaseUI/GUI_Window_Big_Black_NoColor", typeof(Sprite)) as Sprite;
            mainPanel.color = new Color32(100, 100, 100, 255);
            SetAnchor(mainPanel.gameObject, v3(1), v2(0.5f), v2(0.5f), Vector2.zero, Vector2.zero).sizeDelta = v2(700, 600);
            #endregion

            // 主面板
            #region MainPanel
            handbookPanel = UICreator.CreatePanel(mainPanel.gameObject, "IllustratedHandbookPanel");
            SetAnchor(handbookPanel.gameObject, v3(1), v2(0), v2(1), v2(10), v2(-10, -25));

            titlePanel = UICreator.CreatePanel(handbookPanel.gameObject, "TitlePanel");
            SetAnchor(titlePanel.gameObject, v3(1), v2(0.5f, 1), v2(0.5f, 1), v2(0), v2(0)).sizeDelta = v2(680, 50);
            titlePanel.rectTransform.anchoredPosition3D = v3(0, -25, 0);

            titlePanel.gameObject.AddComponent(typeof(EventTrigger));
            titlePanelOndrag = titlePanel.GetComponent<EventTrigger>();
            EventTrigger.Entry eventEntry = new EventTrigger.Entry();
            eventEntry.eventID = EventTriggerType.Drag;
            eventEntry.callback.AddListener((data) =>
            {
                OnDragDelegate((PointerEventData)data);
            });
            titlePanelOndrag.triggers.Add(eventEntry);


            titleText = CreateText("TitleText", titlePanel.gameObject, "<b>图鉴/添加物品 1.0.2</b>", Color.white, 27, v2(0.5f, 1), v2(0.5f, 1), v2(680, 30), v2(0, -25));
            #endregion


            // 加载中面板
            #region LoadingPanel
            loadingPanel = UICreator.CreatePanel(handbookPanel.gameObject, "LoadingPanel");
            SetAnchor(loadingPanel.gameObject, v3(1), v2(0), v2(1), v2(0), v2(0, -50));

            // 加载中文字区域
            loadingTextPanel = UICreator.CreatePanel(loadingPanel.gameObject, "LoadingTextPanel");
            loadingTextPanel.sprite = Resources.Load("Graphics/BaseUI/GUI_BarBack", typeof(Sprite)) as Sprite;
            loadingTextPanel.color = new Color32(255, 255, 255, 44);
            SetAnchor(loadingTextPanel.gameObject, v3(1), v2(0), v2(1), v2(20, 130), v2(-20, 0));

            tipText = CreateText("TipText", loadingTextPanel.gameObject,
             "游戏未开始 尚未加载数据\n\n请先开始游戏~\n\n\n" +
             "<size=16><color=white><b>Tips:</b>\n Ctrl + F11开关窗口\n<b>拖动标题</b>可以移动窗口哦\n" +
             "获取书籍会随机残页、无法查看促织\n" +
             "获取物品后需重新打开背包才看的到\n推荐配合《功法书籍显示》MOD使用\n<color=red>NEW</color> 在人物菜单切换到同道可以将物品直接添加到同道背包(不会影响亲密)\n" +
             "<b>工具无错</b> 请<b>自觉</b>平衡游戏体验</color></size>",
              new Color32(0, 255, 213, 255), 28, v2(0.5f), v2(0.5f), v2(600, 400), v2(0)
            );

            aboutText = CreateText("AboutText", loadingPanel.gameObject, "By: yyuueexxiinngg", Color.white, 14, v2(0), v2(0), v2(160, 30), v2(128.8f, 13.9f));
            aboutText.GetComponent<Text>().alignment = TextAnchor.UpperLeft;

            NGAButton = CreateButton("NGAButton", loadingPanel.gameObject, "点此打开本MOD NGA发布地址", Color.white, "Graphics/BaseUI/GUI_Base", Color.black, v2(0.5f, 0), v2(0.5f, 0), v2(500, 31), v2(0, 100));
            NGAButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                Application.OpenURL("https://nga.178.com/read.php?tid=15239374");
            });

            GithubButton = CreateButton("GithubBotton", loadingPanel.gameObject, "点此打开MOD开源项目Github地址", Color.white, "Graphics/BaseUI/GUI_Base", Color.black, v2(0.5f, 0), v2(0.5f, 0), v2(500, 31), v2(0, 55));
            GithubButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                Application.OpenURL("https://github.com/yyuueexxiinngg/Taiwu_mods");
            });
            #endregion

            // 过滤面板
            #region FilterPanel
            filterPanel = UICreator.CreatePanel(handbookPanel.gameObject, "FilterPanel");
            SetAnchor(filterPanel.gameObject, v3(1), v2(0), v2(1), v2(50, 50), v2(-50, -50));

            itemTypePanel = UICreator.CreatePanel(filterPanel.gameObject, "ItemTypeGrid");
            SetAnchor(itemTypePanel.gameObject, v3(1), v2(0), v2(1), v2(0, 100), v2(0, -50));

            itemTypePanel.gameObject.AddComponent<GridLayoutGroup>();
            itemTypePanel.GetComponent<GridLayoutGroup>().cellSize = v2(110, 30);
            itemTypePanel.GetComponent<GridLayoutGroup>().spacing = v2(6, 3);

            CreateText("FilterTypeLabel", filterPanel.gameObject, "当前选择种类:", Color.white, 14, v2(0, 1), v2(0, 1), v2(160, 30), v3(50, -30, 0));
            filterTypeCurrent = CreateText("FilterTypeCurrent", filterPanel.gameObject, "未初始化", Color.white, 20, v2(0, 1), v2(0, 1), v2(160, 30), v3(120, -30, 0));


            itemLevelPanel = UICreator.CreatePanel(filterPanel.gameObject, "ItemLevelGrid");
            SetAnchor(itemLevelPanel.gameObject, v3(1), v2(0), v2(1), v2(0), v2(0, -370));

            itemLevelPanel.gameObject.AddComponent<GridLayoutGroup>();
            itemLevelPanel.GetComponent<GridLayoutGroup>().cellSize = v2(140, 30);
            itemLevelPanel.GetComponent<GridLayoutGroup>().spacing = v2(6, 3);

            CreateText("FilterLevelLabel", filterPanel.gameObject, "当前选择品级:", Color.white, 14, v2(0, 1), v2(0, 1), v2(160, 30), v3(300, -30, 0));
            filterLevelCurrent = CreateText("FilterLevelCurrent", filterPanel.gameObject, itemLevel[selectedLevel].Split('|')[0], Color.white, 20, v2(0, 1), v2(0, 1), v2(160, 30), v3(380, -30, 0));

            var filterCancelBtn = CreateButton("FilterCancelBtn", filterPanel.gameObject, "取消", Color.white, "Graphics/BaseUI/GUI_ValuBack", Color.white, v2(0), v2(0), v2(200, 30), v2(160, -25));
            var filterRefreshBtn = CreateButton("FilterRefreshBtn", filterPanel.gameObject, "刷新物品列表", Color.white, "Graphics/BaseUI/GUI_ValuBack", Color.white, v2(1, 0), v2(1, 0), v2(200, 30), v2(-160, -25));

            filterCancelBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                filterPanel.gameObject.SetActive(false);
                loadedPanel.gameObject.SetActive(true);
            });

            filterRefreshBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                selectedType = selectedTypeUnsaved;
                selectedLevel = selectedLevelUnsaved;
                // 清空物品列表
                itemList = new Dictionary<int, Dictionary<int, string>>();

                if (selectedType != 0) // 如果选择不是全部
                {
                    foreach (var item in dataInstance.presetitemDate)
                    {
                        // item.Value[5]即为物品种类 item.Value[8]即为物品品级
                        if (int.Parse(item.Value[5]) == selectedType && (selectedLevel == 0 || int.Parse(item.Value[8]) == selectedLevel))
                        {
                            itemList[item.Key] = item.Value;
                        }
                    }
                }
                else // 如果是全部
                {
                    if (selectedLevel == 0) //　如果品级是全部
                    {
                        itemList = dataInstance.presetitemDate;
                    }
                    else
                    {
                        foreach (var item in dataInstance.presetitemDate)
                        {
                            if (int.Parse(item.Value[8]) == selectedLevel)
                            {
                                itemList[item.Key] = item.Value;
                            }
                        }
                    }
                }
                DrawItems();
                filterPanel.gameObject.SetActive(false);
                loadedPanel.gameObject.SetActive(true);
            });


            #endregion


            // 加载完成后面板
            #region LoadedPanel
            loadedPanel = UICreator.CreatePanel(handbookPanel.gameObject, "LoadedPanel");
            SetAnchor(loadedPanel.gameObject, v3(1), v2(0), v2(1), v2(0, 50), v2(0, -50));


            itemPanel = UICreator.CreatePanel(loadedPanel.gameObject, "ItemPanel");
            itemPanel.sprite = Resources.Load("Graphics/BaseUI/GUI_BarBack", typeof(Sprite)) as Sprite;
            itemPanel.color = new Color32(255, 255, 255, 44);
            SetAnchor(itemPanel.gameObject, v3(1), v2(0), v2(1), v2(20, 0), v2(-20, 0));

            var selectionPanel = UICreator.CreatePanel(itemPanel.gameObject, "SelectionPanel");
            SetAnchor(selectionPanel.gameObject, v3(1), v2(0), v2(1), v2(30, 415), v2(-30, -15));

            var itemFilterButton = CreateButton("ItemFilterButton", selectionPanel.gameObject, "过滤", Color.white, "Graphics/BaseUI/GUI_ValuBack", Color.white, v2(0.5f), v2(0.5f), v2(160, 30), v3(0));
            itemFilterButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                string[] itemTypes = itemType.Split('|');
                filterTypeCurrent.GetComponent<Text>().text = itemTypes[selectedType];
                filterLevelCurrent.GetComponent<Text>().text = itemLevel[selectedLevel].Split('|')[0];
                loadedPanel.gameObject.SetActive(false);
                filterPanel.gameObject.SetActive(true);
            });

            // 物品栏
            itemGrid = new GameObject("ItemGrid", typeof(GridLayoutGroup));
            itemGrid.transform.SetParent(itemPanel.transform);
            var itemGridComponent = itemGrid.GetComponent<GridLayoutGroup>();
            itemGridComponent.cellSize = v2(65, 65);
            itemGridComponent.startCorner = GridLayoutGroup.Corner.UpperLeft;
            itemGridComponent.startAxis = GridLayoutGroup.Axis.Horizontal;

            SetAnchor(itemGrid.gameObject, v3(1), v2(0), v2(1), v2(30, 0), v2(0, -50));

            itemBack = Resources.LoadAll<Sprite>("Graphics/ItemIcon/ItemIconBack");
            itemIcon = Resources.LoadAll<Sprite>("Graphics/ItemIcon/ItemIcon");


            EventTrigger.Entry itemSlotOnMouseEnterEntry = new EventTrigger.Entry();
            itemSlotOnMouseEnterEntry.eventID = EventTriggerType.PointerEnter;
            itemSlotOnMouseEnterEntry.callback.AddListener((data) =>
            {
                OnMouseEnterDelegate((PointerEventData)data);
            });

            EventTrigger.Entry itemSlotOnMouseExitEntry = new EventTrigger.Entry();
            itemSlotOnMouseExitEntry.eventID = EventTriggerType.PointerExit;
            itemSlotOnMouseExitEntry.callback.AddListener((data) =>
            {
                OnMouseExitDelegate((PointerEventData)data);
            });

            // 物品槽
            for (int i = 0; i < 54; i++)
            {
                var itemSlot = new GameObject("ItemSlot|" + i, typeof(RectTransform));
                itemSlot.transform.SetParent(itemGrid.transform);
                itemSlot.transform.localScale = v3(1);

                itemSlot.AddComponent(typeof(Button));
                itemSlot.GetComponent<Button>().onClick.AddListener(
                    () =>
                    {
                        if (itemSlot.name.Split('|')[1] != "10000")
                        {
                            if (DateFile.instance.GetItem(ActorMenu.instance.actorMenu.activeInHierarchy ? ActorMenu.instance.acotrId : DateFile.instance.MianActorID(), int.Parse(itemSlot.name.Split('|')[1]), 1, true) > 0)
                            {
                                TipsWindow.instance.SetTips(5007, new string[] { dataInstance.GetActorName(ActorMenu.instance.actorMenu.activeInHierarchy ? ActorMenu.instance.acotrId : DateFile.instance.MianActorID()), itemList[int.Parse(itemSlot.name.Split('|')[1])][0], "" }, 100);
                            }
                            else
                            {
                                TipsWindow.instance.SetTips(0, new string[] { "无法添加此物品~" }, 100);
                            }
                        }
                        else
                        {
                            TipsWindow.instance.SetTips(0, new string[] { "无法添加促织~" }, 100);
                        }

                    }
                );

                itemSlot.AddComponent(typeof(EventTrigger));
                var itemSlotOnHover = itemSlot.GetComponent<EventTrigger>();
                itemSlotOnHover.triggers.Add(itemSlotOnMouseEnterEntry);
                itemSlotOnHover.triggers.Add(itemSlotOnMouseExitEntry);

                var itemBackObj = new GameObject("ItemBack", typeof(Image));
                itemBackObj.transform.SetParent(itemSlot.transform);
                itemBackObj.GetComponent<Image>().sprite = itemBack[4];
                itemBackObj.GetComponent<Image>().color = new Color32(254, 47, 17, 255);
                itemBackObj.transform.localScale = v3(1);
                itemBackObj.GetComponent<RectTransform>().sizeDelta = v2(60, 60);
                itemBackObj.GetComponent<Image>().preserveAspect = true;

                var itemIconObj = new GameObject("ItemIcon", typeof(Image));
                itemIconObj.transform.SetParent(itemBackObj.transform);
                itemIconObj.GetComponent<Image>().sprite = itemIcon[i];
                itemIconObj.transform.localScale = v3(1);
                itemIconObj.GetComponent<RectTransform>().sizeDelta = v2(40, 40);

                // 物品下标文字 i.e. 0/0 本MOD未使用到所以没有赋值
                CreateText("ItemText", itemBackObj, "", Color.white, 14, v2(0), v2(1, 0.3f), v2(1), v3(0));
                itemIconObj.GetComponent<Image>().preserveAspect = true;

                // itemSlot.SetActive(false);
                Vector3 position = itemSlot.transform.localPosition;
                position.z = 0;
                itemSlot.transform.localPosition = position;

                itemSlots.Add(itemSlot);
            }

            prePageButton = CreateButton("PrePageBotton", loadedPanel.gameObject, "上一页", Color.white, "Graphics/BaseUI/GUI_ValuBack", Color.white, v2(0), v2(0), v2(200, 30), v2(160, -25));
            nextPageButton = CreateButton("PrePageBotton", loadedPanel.gameObject, "下一页", Color.white, "Graphics/BaseUI/GUI_ValuBack", Color.white, v2(1, 0), v2(1, 0), v2(200, 30), v2(-160, -25));

            var itemPageTextObj = CreateText("ItemPageText", loadedPanel.gameObject, "0/0", Color.white, 20, v2(0.5f, 0), v2(0.5f, 0), v2(80, 30), v3(0, -25, 0));
            itemPageText = itemPageTextObj.GetComponent<Text>();

            // 初始化页标
            itemPageText.text = "1/1";

            prePageButton.GetComponent<Button>().onClick.AddListener(
                () =>
                {
                    if (itemSlotPage > 0)
                    {
                        // 如果到达过最后一页 遍历一遍itemSlots全部激活
                        if (finalPageReached)
                        {
                            foreach (var itemSlot in itemSlots)
                            {
                                itemSlot.SetActive(true);
                            }
                        }

                        --itemSlotPage;

                        // 用以获取当前循环对应的itemSlot
                        int index = 0;

                        foreach (var item in itemList.Skip(itemSlotPage * 54).Take(54))
                        {
                            itemSlots[index].name = "ItemSlot|" + item.Key;
                            itemSlots[index].transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = itemIcon[int.Parse(item.Value[98])];
                            itemSlots[index].transform.GetChild(0).GetComponent<Image>().sprite = itemBack[int.Parse(item.Value[4])];
                            itemSlots[index].transform.GetChild(0).GetChild(1).GetComponent<Text>().text = item.Value[0];
                            Color backgroundColor = Color.white;
                            ColorUtility.TryParseHtmlString(itemLevel[int.Parse(item.Value[8])].Split('|')[1], out backgroundColor);
                            itemSlots[index].transform.GetChild(0).GetComponent<Image>().color = backgroundColor;
                            index++;
                        }

                        itemPageText.text = string.Format("{0}/{1}", itemSlotPage + 1, itemList.Count % 54 == 0 ? itemList.Count / 54 : (itemList.Count / 54) + 1);
                    }

                }
            );

            nextPageButton.GetComponent<Button>().onClick.AddListener(
                () =>
                {
                    if (itemSlotPage < itemList.Count / 54)
                    {
                        ++itemSlotPage;

                        int index = 0;

                        foreach (var item in itemList.Skip(itemSlotPage * 54).Take(54))
                        {
                            if (itemSlotPage * 54 <= itemList.Count - 54)
                                finalPageReached = false;
                            else

                                finalPageReached = true;
                            itemSlots[index].name = "ItemSlot|" + item.Key;
                            itemSlots[index].transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = itemIcon[int.Parse(item.Value[98])];
                            itemSlots[index].transform.GetChild(0).GetComponent<Image>().sprite = itemBack[int.Parse(item.Value[4])];
                            itemSlots[index].transform.GetChild(0).GetChild(1).GetComponent<Text>().text = item.Value[0];
                            Color backgroundColor = Color.white;
                            ColorUtility.TryParseHtmlString(itemLevel[int.Parse(item.Value[8])].Split('|')[1], out backgroundColor);
                            itemSlots[index].transform.GetChild(0).GetComponent<Image>().color = backgroundColor;
                            index++;
                        }

                        itemPageText.text = string.Format("{0}/{1}", itemSlotPage + 1, itemList.Count % 54 == 0 ? itemList.Count / 54 : (itemList.Count / 54) + 1);

                        if (finalPageReached)
                        {
                            for (int i = 53; i >= index; i--)
                            {
                                itemSlots[i].SetActive(false);
                            }
                        }
                    }
                }
            );
            #endregion

            loadedPanel.gameObject.SetActive(false);
            loadingPanel.gameObject.SetActive(true);
            filterPanel.gameObject.SetActive(false);

        }

        private void DrawItems()
        {
            // 初始化页标
            itemPageText.text = string.Format("1/{0}", itemList.Count % 54 == 0 ? itemList.Count / 54 : (itemList.Count / 54) + 1);

            // 是否到达过最后一页 用来控制itemSlots的开关
            if (itemList.Count <= 54)
            {
                finalPageReached = true;
            }
            else
            {
                finalPageReached = false;
            }

            itemSlotPage = 0;

            // 初始化itemSlots
            int index = 0;
            foreach (var item in itemList.Skip(itemSlotPage * 54).Take(54))
            {
                itemSlots[index].SetActive(true);
                itemSlots[index].name = "ItemSlot|" + item.Key;
                if (itemSlotPage * 54 + index <= itemList.Count - 54)
                    finalPageReached = false;
                else
                    finalPageReached = true;

                itemSlots[index].transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = itemIcon[int.Parse(item.Value[98])];
                itemSlots[index].transform.GetChild(0).GetComponent<Image>().sprite = itemBack[int.Parse(item.Value[4])];
                itemSlots[index].transform.GetChild(0).GetChild(1).GetComponent<Text>().text = item.Value[0];
                Color backgroundColor = Color.white;
                ColorUtility.TryParseHtmlString(itemLevel[int.Parse(item.Value[8])].Split('|')[1], out backgroundColor);
                itemSlots[index].transform.GetChild(0).GetComponent<Image>().color = backgroundColor;

                index++;
            }

            // 如果到达最后一页 将多余的itemSlots关闭
            if (finalPageReached)
            {
                for (int i = 53; i >= index; i--)
                {
                    itemSlots[i].SetActive(false);
                }
            }
        }

        public void OnMouseEnterDelegate(PointerEventData data)
        {
            GameObject OnHover = data.pointerEnter;

            while (!OnHover.name.Contains("ItemSlot"))
            {
                OnHover = OnHover.transform.parent.gameObject;
            }

            GameObject tips = new GameObject();
            // 临时新建一个Item获取ID以显示信息
            tips.name = "ActorItem," + DateFile.instance.MakeNewItem(int.Parse(OnHover.name.Split('|')[1]), -5713);
            tips.tag = "ActorItem";
            WindowManage.instance.WindowSwitch(true, tips);

        }
        public void OnMouseExitDelegate(PointerEventData data)
        {
            GameObject OnHover = data.pointerEnter;

            while (!OnHover.name.Contains("ItemSlot"))
            {
                OnHover = OnHover.transform.parent.gameObject;
            }
            WindowManage.instance.WindowSwitch(false);
        }

        public void OnDragDelegate(PointerEventData data)
        {
            if (gameLoaded && !gameExited)
            {
                // 获取当前鼠标位置
                Vector3 newPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainPanel.transform.position.z);
                // 转换为世界坐标
                Vector3 objPosition = Camera.main.ScreenToWorldPoint(newPosition) + offsetPostion;
                mainPanel.transform.position = objPosition;
            }
            else
            {
                mainPanel.transform.position = Input.mousePosition + offsetPostionLoding;
            }
        }

        private GameObject CreateText(string name, GameObject parent, string content, Color color, int fontSize, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector3 anchoredPosition)
        {
            var text = new GameObject(name, typeof(Text));
            text.transform.SetParent(parent.transform);
            text.GetComponent<Text>().text = content;
            text.GetComponent<Text>().color = color;
            text.GetComponent<Text>().font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            text.GetComponent<Text>().fontSize = fontSize;
            var textTransform = text.GetComponent<RectTransform>();
            textTransform.localScale = v3(1, 1, 1);
            textTransform.anchorMax = anchorMax;
            textTransform.anchorMin = anchorMin;
            textTransform.sizeDelta = size;
            textTransform.anchoredPosition3D = anchoredPosition;
            return text;
        }

        private GameObject CreateButton(string buttonName, GameObject parent, string text, Color textColor, string texture, Color textureColor, Vector2 anchorMax, Vector2 anchorMin, Vector2 size, Vector3 position)
        {
            GameObject button = new GameObject(buttonName, typeof(Button), typeof(Image));
            var buttonText = new GameObject(buttonName + "Text", typeof(Text));
            buttonText.transform.SetParent(button.transform);
            buttonText.GetComponent<Text>().font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            buttonText.GetComponent<Text>().text = text;
            buttonText.GetComponent<Text>().color = textColor;
            buttonText.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            buttonText.GetComponent<Text>().fontSize = 14;
            buttonText.GetComponent<RectTransform>().localScale = v3(1, 1, 1);
            buttonText.GetComponent<RectTransform>().anchorMin = v2(0);
            buttonText.GetComponent<RectTransform>().anchorMax = v2(1);
            buttonText.GetComponent<RectTransform>().offsetMin = v2(0);
            buttonText.GetComponent<RectTransform>().offsetMax = v2(0);
            button.transform.SetParent(parent.transform);
            button.GetComponent<Image>().sprite = Resources.Load(texture, typeof(Sprite)) as Sprite;
            button.GetComponent<Image>().color = textureColor;
            var buttonTransform = button.GetComponent<RectTransform>();
            buttonTransform.localScale = v3(1, 1, 1);
            buttonTransform.anchorMax = anchorMax;
            buttonTransform.anchorMin = anchorMin;
            buttonTransform.sizeDelta = size;
            buttonTransform.anchoredPosition3D = position;
            return button;
        }

        #region Utils
        private static RectTransform SetAnchor(GameObject obj, Vector3 localScale, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            RectTransform transform = obj.GetComponent<RectTransform>();
            transform.localScale = localScale;
            transform.anchorMin = anchorMin;
            transform.anchorMax = anchorMax;
            transform.offsetMin = offsetMin;
            transform.offsetMax = offsetMax;
            transform.anchoredPosition3D = new Vector3(transform.anchoredPosition3D.x, transform.anchoredPosition3D.y, 0);
            return transform;
        }

        private static Vector2 v2(float var)
        {
            return new Vector2(var, var);
        }

        private static Vector2 v2(float x, float y)
        {
            return new Vector2(x, y);
        }

        private static Vector3 v3(float var)
        {
            return new Vector3(var, var, var);
        }

        private static Vector3 v3(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }
        #endregion
    }
}
