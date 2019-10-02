using DG.Tweening;
using Harmony12;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityModManagerNet;

namespace Majordomo
{
    /// <summary>
    /// 管家界面内的单条文本消息
    /// </summary>
    public class Message
    {
        public const int IMPORTANCE_LOWEST = 0;
        public const int IMPORTANCE_LOW = 25;
        public const int IMPORTANCE_NORMAL = 50;
        public const int IMPORTANCE_HIGH = 75;
        public const int IMPORTANCE_HIGHEST = 100;

        public static readonly string[] MESSAGE_IMPORTANCE_NAMES = { "最低", "低", "普通", "高", "最高" };

        public int importance;
        public string content;


        public Message(int importance, string content)
        {
            this.importance = importance;
            this.content = content;
        }
    }


    public class WorkerStats
    {
        public int nWorkers;                // 工作人员数量
        public float avgHealthInjury;       // 平均健康（伤势）
        public float avgHealthCirculating;  // 平均健康（内息）
        public float avgHealthPoison;       // 平均健康（中毒）
        public float avgHealthLifespan;     // 平均健康（寿命）
        public float avgCompositeHealth;    // 平均综合健康
        public float avgMood;               // 平均心情
        public float avgFriendliness;       // 平均好感
        public float avgWorkMotivation;     // 平均工作动力
    }


    public class WorkingStats
    {
        public int nProductiveBuildings;    // 生产性建筑数量
        public float avgWorkEffectiveness;  // 平均工作效率
        public float compositeWorkIndex;    // 综合工作指数
    }


    public class EarningStats
    {
        public int earnedMoney;             // 收获银钱
        public int earnedFame;              // 收获威望
        public int gdp;                     // 本地生产总值（收获等价银钱）
    }


    /// <summary>
    /// 管家界面内的单个月内的所有消息的集合
    /// </summary>
    public class Record
    {
        public List<Message> messages;
        public WorkerStats workerStats;
        public WorkingStats workingStats;
        public EarningStats earningStats;


        public Record()
        {
            this.messages = new List<Message>();
            this.workerStats = new WorkerStats();
            this.workingStats = new WorkingStats();
            this.earningStats = new EarningStats();
        }


        public Record(TaiwuDate date)
        {
            this.messages = new List<Message>
            {
                new Message(Message.IMPORTANCE_HIGHEST, $"进入{date.ToString(richText: true)}。"),
            };
            this.workerStats = new WorkerStats();
            this.workingStats = new WorkingStats();
            this.earningStats = new EarningStats();
        }
    }


    /// <summary>
    /// 管家界面
    /// </summary>
    public class MajordomoWindow
    {
        public const int RECORD_SHELF_LIFE = 100 * 12;
        public const int MESSAGE_SHELF_LIFE = 2 * 12;
        public const int N_SUMMARY_ITEMS = 5;

        public static readonly Color TEXT_COLOR_DEFAULT = new Color(0.882f, 0.804f, 0.667f, 1.000f);
        public static readonly Color MENU_BTN_COLOR_UNSELECTED = new Color(0.984f, 0.984f, 0.984f, 1.000f);
        public static readonly Color MENU_BTN_COLOR_SELECTED = new Color(0.016f, 0.016f, 0.016f, 1.000f);
        public static readonly Color MENU_BTN_BG_COLOR_UNSELECTED = new Color(0.191f, 0.169f, 0.157f, 1.000f);
        public static readonly Color MENU_BTN_BG_COLOR_SELECTED = new Color(0.809f, 0.831f, 0.843f, 1.000f);

        public static MajordomoWindow instance;

        private GameObject window;
        private bool isWindowOpening;

        private GameObject menu;
        private GameObject logsPanelButton;
        private GameObject chartsPanelButton;
        private GameObject assignBuildingWorkersButton;

        private GameObject panelContainer;
        private PanelLogs panelLogs;
        private PanelCharts panelCharts;
        private Dictionary<ITaiwuWindow, GameObject> panels;    // panel -> menu button
        private ITaiwuWindow selectedPanel;

        private GameObject summaryBar;

        private readonly Dictionary<TaiwuDate, Record> history = new Dictionary<TaiwuDate, Record>();


        /// <summary>
        /// 检查资源是否存在，若不存在则创建并注册
        /// 需要在蛐蛐界面和人物界面初始化完毕后进行
        /// </summary>
        public static void TryRegisterResources()
        {
            if (MajordomoWindow.instance == null)
                MajordomoWindow.instance = new MajordomoWindow();

            if (!MajordomoWindow.instance.window ||
                !MajordomoWindow.instance.menu ||
                !MajordomoWindow.instance.panelContainer ||
                !MajordomoWindow.instance.summaryBar)
                MajordomoWindow.instance.CreateWindow();

            if (MajordomoWindow.instance.panelLogs == null)
                MajordomoWindow.instance.panelLogs = new PanelLogs(
                    MajordomoWindow.instance.panelContainer, MajordomoWindow.instance.history);
            MajordomoWindow.instance.panelLogs.TryRegisterResources(MajordomoWindow.instance.panelContainer);

            if (MajordomoWindow.instance.panelCharts == null)
                MajordomoWindow.instance.panelCharts = new PanelCharts(
                    MajordomoWindow.instance.panelContainer, MajordomoWindow.instance.history);
            MajordomoWindow.instance.panelCharts.TryRegisterResources(MajordomoWindow.instance.panelContainer);

            if (!MajordomoWindow.instance.logsPanelButton ||
                !MajordomoWindow.instance.chartsPanelButton ||
                !MajordomoWindow.instance.assignBuildingWorkersButton)
                MajordomoWindow.instance.CreateMenuItems();

            MajordomoWindow.instance.panels = new Dictionary<ITaiwuWindow, GameObject>()
            {
                [MajordomoWindow.instance.panelLogs] = MajordomoWindow.instance.logsPanelButton,
                [MajordomoWindow.instance.panelCharts] = MajordomoWindow.instance.chartsPanelButton,
            };

            MajordomoWindow.instance.SwitchPanel(MajordomoWindow.instance.panelLogs);

            MajordomoWindow.instance.window.SetActive(false);
            MajordomoWindow.instance.isWindowOpening = false;

            UnityEngine.Debug.Log("Resources of MajordomoWindow registered.");
        }


        /// <summary>
        /// 新建存档时初始化当前数据
        /// </summary>
        public void InitData()
        {
            this.history.Clear();
        }


        /// <summary>
        /// 从 DateFile.modDate 中载入已保存的数据
        /// 载入之前先重置当前数据
        /// </summary>
        public void LoadSavedData()
        {
            this.InitData();

            if (!DateFile.instance.modDate.ContainsKey(Main.MOD_ID)) return;
            var savedData = DateFile.instance.modDate[Main.MOD_ID];

            string saveKeyHistory = "MajordomoWindow.history";
            if (savedData.ContainsKey(saveKeyHistory))
            {
                string serializedData = savedData[saveKeyHistory];
                var deserializedData = JsonConvert.DeserializeObject<KeyValuePair<TaiwuDate, Record>[]>(serializedData);
                foreach (var entry in deserializedData) this.history.Add(entry.Key, entry.Value);
            }
        }


        /// <summary>
        /// 把数据保存到 DateFile.modDate 中
        /// 顺便删除超出保存期限的数据
        /// </summary>
        public void SaveData()
        {
            if (!DateFile.instance.modDate.ContainsKey(Main.MOD_ID))
                DateFile.instance.modDate[Main.MOD_ID] = new Dictionary<string, string>();
            var savedData = DateFile.instance.modDate[Main.MOD_ID];

            string saveKeyHistory = "MajordomoWindow.history";
            this.RemoveOutdatedData();
            savedData[saveKeyHistory] = JsonConvert.SerializeObject(this.history.ToArray());
        }


        /// <summary>
        /// 删除超出保存期限的数据
        /// </summary>
        private void RemoveOutdatedData()
        {
            var dates = this.history.Keys.OrderByDescending(date => date).Skip(RECORD_SHELF_LIFE);
            foreach (var date in dates) this.history.Remove(date);

            dates = this.history.Keys.OrderByDescending(date => date).Skip(MESSAGE_SHELF_LIFE);
            foreach (var date in dates) this.history[date].messages.Clear();

            if (this.panelLogs != null) this.panelLogs.SetPageIndex(-1);
        }


        private void CreateWindow()
        {
            // 此函数的触发条件就是 BuildingWindow.instance 存在
            var ququBox = BuildingWindow.instance.GetComponentInChildren<QuquBox>();

            // clone & modify main window
            this.window = UnityEngine.Object.Instantiate(ququBox.ququBoxWindow, ququBox.ququBoxWindow.transform.parent);
            this.window.SetActive(true);
            this.window.name = "MajordomoWindow";

            Common.RemoveComponent<QuquBox>(this.window);
            Common.RemoveChildren(this.window, new List<string> { "ChooseItemMask", "ItemsBack" });

            // modify panel container
            this.panelContainer = Common.GetChild(this.window, "QuquBoxHolder");
            if (!this.panelContainer) throw new Exception("Failed to get child 'QuquBoxHolder' from QuquBox.ququBoxWindow");
            this.panelContainer.name = "MajordomoPanelContainer";

            // clone & modify menu
            this.menu = UnityEngine.Object.Instantiate(this.panelContainer, this.window.transform);
            this.menu.SetActive(true);
            this.menu.name = "MajordomoMenu";

            Common.RemoveChildren(this.menu);

            // modify panel container
            Common.RemoveComponent<GridLayoutGroup>(this.panelContainer);
            Common.RemoveComponent<Image>(this.panelContainer);
            Common.RemoveComponent<CanvasRenderer>(this.panelContainer);
            Common.RemoveChildren(this.panelContainer);

            // resize panel container & menu
            var rectTransform = this.panelContainer.GetComponent<RectTransform>();
            float width = rectTransform.offsetMax.x - rectTransform.offsetMin.x;
            float panelContainerWidth = width * 0.9f;
            float menuWidth = width - panelContainerWidth;
            rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x + menuWidth, rectTransform.offsetMin.y);

            rectTransform = this.menu.GetComponent<RectTransform>();
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x - panelContainerWidth, rectTransform.offsetMax.y);

            // set menu layout
            var gridLayoutGroup = Common.RemoveComponent<GridLayoutGroup>(this.menu, recreate: true);
            gridLayoutGroup.padding = new RectOffset(20, 20, 20, 20);
            gridLayoutGroup.cellSize = new Vector2(110, 120);
            gridLayoutGroup.spacing = new Vector2(20, 20);
            gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
            gridLayoutGroup.childAlignment = TextAnchor.UpperCenter;

            // modify close button
            var goCloseButton = Common.GetChild(this.window, "CloseQuquBoxButton");
            if (!goCloseButton) throw new Exception("Failed to get child 'CloseQuquBoxButton' from QuquBox.ququBoxWindow");
            goCloseButton.name = "MajordomoCloseButton";

            var closeButton = Common.RemoveComponent<Button>(goCloseButton, recreate: true);
            closeButton.onClick.AddListener(() => this.Close());

            var escWinComponent = Common.RemoveComponent<EscWinComponent>(goCloseButton, recreate: true);
            escWinComponent.escEvent = new EscWinComponent.EscCompEvent();
            escWinComponent.escEvent.AddListener(() => this.Close());

            // modify summary bar
            this.summaryBar = Common.GetChild(this.window, "AllQuquLevelBack");
            if (!this.summaryBar) throw new Exception("Failed to get child 'AllQuquLevelBack' from QuquBox.ququBoxWindow");
            this.summaryBar.name = "MajordomoSummary";

            var horizontalLayoutGroup = this.summaryBar.AddComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.padding = new RectOffset(20, 20, 0, 0);

            var summaryItem = Common.GetChild(this.summaryBar, "AllQuquLevelText");
            if (!summaryItem) throw new Exception("Failed to get child 'AllQuquLevelText' from 'AllQuquLevelBack'");
            summaryItem.name = "MajordomoSummaryItem";

            var textSummary = summaryItem.GetComponent<Text>();
            if (!textSummary) throw new Exception("Failed to get Text component from 'AllQuquLevelText'");
            textSummary.color = MajordomoWindow.TEXT_COLOR_DEFAULT;
            TaiwuCommon.SetFont(textSummary);

            Common.RemoveComponent<SetFont>(summaryItem);

            for (int i = 1; i < N_SUMMARY_ITEMS; ++i)
            {
                var currSummaryItem = UnityEngine.Object.Instantiate(summaryItem, this.summaryBar.transform);
                currSummaryItem.SetActive(true);
                currSummaryItem.name = "MajordomoSummaryItem";

                Common.RemoveComponent<SetFont>(currSummaryItem);
            }
        }


        /// <summary>
        /// 只要在各个 panel 创建完之后再创建 menu item，事件中的 panel 引用就都是有效的。
        /// </summary>
        private void CreateMenuItems()
        {
            this.logsPanelButton = this.CreateMenuButton("LogsPanelButton",
                () => this.SwitchPanel(this.panelLogs),
                Path.Combine(Path.Combine(Main.resBasePath, "Texture"), $"ButtonIcon_Majordomo_LogsPanel.png"),
                "翻阅记录");

            this.chartsPanelButton = this.CreateMenuButton("ChartsPanelButton",
                () => this.SwitchPanel(this.panelCharts),
                Path.Combine(Path.Combine(Main.resBasePath, "Texture"), $"ButtonIcon_Majordomo_ChartsPanel.png"),
                "查看图表");

            this.assignBuildingWorkersButton = this.CreateMenuButton("AssignBuildingWorkersButton",
                () => {
                    YesOrNoWindow.instance.SetYesOrNoWindow(-1, "太吾管家", "管家已重新指派了各个建筑的负责人。", canClose: false);
                    HumanResource.AssignBuildingWorkersForTaiwuVillage();
                    this.panelLogs.SetPageIndex(-1);
                    if (this.panelLogs.gameObject.activeInHierarchy) this.panelLogs.Update();
                },
                Path.Combine(Path.Combine(Main.resBasePath, "Texture"), $"ButtonIcon_Majordomo_AssignBuildingWorkers.png"),
                "重新指派工作");
        }


        private GameObject CreateMenuButton(string name, UnityEngine.Events.UnityAction callback, string iconPath, string label)
        {
            // clone & modify button
            // 此函数的触发条件就是 BuildingWindow.instance 存在
            var studySkillButton = Common.GetChild(BuildingWindow.instance.studyActor, "StudySkill,0");
            if (!studySkillButton) throw new Exception("Failed to get child 'StudySkill,0' from HomeSystem.instance.studyActor");

            var goMenuButton = UnityEngine.Object.Instantiate(studySkillButton, this.menu.transform);
            goMenuButton.SetActive(true);
            goMenuButton.name = name;
            goMenuButton.tag = "Untagged";

            var button = goMenuButton.AddComponent<Button>();
            button.onClick.AddListener(callback);
            goMenuButton.AddComponent<PointerClick>();

            // modify button background
            var buttonBack = Common.GetChild(goMenuButton, "StudyEffectBack");
            if (!buttonBack) throw new Exception("Failed to get child 'StudyEffectBack' from 'StudySkill,0'");
            buttonBack.name = "MajordomoMenuButtonBack";

            var image = buttonBack.GetComponent<Image>();
            image.color = MajordomoWindow.MENU_BTN_BG_COLOR_UNSELECTED;

            var rectTransform = buttonBack.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.offsetMin = new Vector2(0, 0);
            rectTransform.offsetMax = new Vector2(0, 30);

            // modify button icon
            var buttonIcon = Common.GetChild(goMenuButton, "StudySkillIcon,0");
            if (!buttonIcon) throw new Exception("Failed to get child 'StudySkillIcon,0' from 'StudySkill,0'");
            buttonIcon.name = "MajordomoMenuButtonIcon";

            rectTransform = buttonIcon.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = new Vector2(25, -80);
            rectTransform.offsetMax = new Vector2(-25, -20);

            var buttonIconImage = buttonIcon.GetComponent<Image>();
            buttonIconImage.sprite = ResourceLoader.CreateSpriteFromImage(iconPath);
            if (!buttonIconImage.sprite) throw new Exception($"Failed to create sprite: {iconPath}");

            // modify button text
            var buttonText = Common.GetChild(goMenuButton, "StudyEffectText");
            if (!buttonText) throw new Exception("Failed to get child 'StudyEffectText' from 'StudySkill,0'");
            buttonText.name = "MajordomoMenuButtonText";

            rectTransform = buttonText.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.offsetMin = new Vector2(0, 0);
            rectTransform.offsetMax = new Vector2(0, 30);

            var text = buttonText.GetComponent<Text>();
            if (!text) throw new Exception("Failed to get Text component from 'StudyEffectText'");
            text.text = label;
            text.color = MajordomoWindow.MENU_BTN_COLOR_UNSELECTED;
            TaiwuCommon.SetFont(text);

            Common.RemoveComponent<SetFont>(buttonText);

            return goMenuButton;
        }


        public void Open()
        {
            if (this.isWindowOpening) return;
            this.isWindowOpening = true;

            this.panelLogs.SetPageIndex(-1);
            this.selectedPanel.Open();
            this.UpdateSummary();

            this.window.SetActive(true);

            this.window.transform.localPosition = new Vector3(-20f, 1200f, 0f);
            this.window.transform.DOLocalMoveY(-20f, 0.3f).SetUpdate(true).SetEase(Ease.OutBack)
                .OnComplete(() => this.isWindowOpening = false);
        }


        public void Close()
        {
            this.window.transform.DOLocalMoveY(1200f, 0.3f).SetUpdate(true).SetEase(Ease.OutBack)
                .OnComplete(() => this.window.SetActive(false));
        }


        /// <summary>
        /// 首先关闭所有 panel，然后打开指定 panel
        /// panel 的激活状态不同，菜单按钮的样式也会不同
        /// </summary>
        /// <param name="selectedPanel"></param>
        private void SwitchPanel(ITaiwuWindow selectedPanel)
        {
            if (!this.panels.ContainsKey(selectedPanel)) throw new Exception("Unregistered panel: " + selectedPanel.gameObject.name);

            this.selectedPanel = selectedPanel;

            foreach (var entry in this.panels)
            {
                var panel = entry.Key;
                var button = entry.Value;

                if (panel == selectedPanel) continue;

                this.ChangeMenuButtonStyle(button, selected: false);
                panel.Close();
            }

            this.ChangeMenuButtonStyle(this.panels[selectedPanel], selected: true);
            selectedPanel.Open();
        }


        /// <summary>
        /// 根据按钮选中状态更改按钮样式
        /// </summary>
        /// <param name="menuButton"></param>
        /// <param name="selected"></param>
        private void ChangeMenuButtonStyle(GameObject menuButton, bool selected)
        {
            var textBackground = Common.GetChild(menuButton, "MajordomoMenuButtonBack");
            var image = textBackground.GetComponent<Image>();

            var goText = Common.GetChild(menuButton, "MajordomoMenuButtonText");
            var text = goText.GetComponent<Text>();

            if (selected)
            {
                image.color = MajordomoWindow.MENU_BTN_BG_COLOR_SELECTED;
                text.color = MajordomoWindow.MENU_BTN_COLOR_SELECTED;
            }
            else
            {
                image.color = MajordomoWindow.MENU_BTN_BG_COLOR_UNSELECTED;
                text.color = MajordomoWindow.MENU_BTN_COLOR_UNSELECTED;
            }
        }


        private void UpdateSummary()
        {
            var statsTexts = this.GetSummaryStatsTexts();

            for (int i = 0; i < MajordomoWindow.N_SUMMARY_ITEMS; ++i)
            {
                var summaryItem = this.summaryBar.transform.GetChild(i).gameObject;
                var text = summaryItem.GetComponent<Text>();
                text.text = statsTexts[i];
            }
        }


        /// <summary>
        /// 计算汇总统计信息
        /// 
        /// 计算过去一年内金钱收入等时，数据不足时会进行估算，至少有三个月数据才计算。
        /// 如果数据中间有空缺月份，则计算跨度会大于一年。
        /// </summary>
        private string[] GetSummaryStatsTexts()
        {
            const int MIN_CALCULATING_MONTHS = 3;
            const int N_EXPECTED_MONTHS = 12;

            string avgCompositeHealth, avgWorkMotivation, avgWorkEffectiveness, earnedMoneyOfLastYear, gdpOfLastYear;

            if (this.history.Count > 0)
            {
                var newestDate = this.history.Keys.Max(date => date);
                avgCompositeHealth = (this.history[newestDate].workerStats.avgCompositeHealth * 100).ToString("F2");
                avgWorkMotivation = (this.history[newestDate].workerStats.avgWorkMotivation * 100).ToString("F2");
                avgWorkEffectiveness = (this.history[newestDate].workingStats.avgWorkEffectiveness * 100).ToString("F2");
            }
            else
            {
                avgCompositeHealth = "???";
                avgWorkMotivation = "???";
                avgWorkEffectiveness = "???";
            }

            if (this.history.Count >= MIN_CALCULATING_MONTHS)
            {
                var datesWithin = this.history.OrderByDescending(entry => entry.Key).Take(N_EXPECTED_MONTHS);

                var earnedMoneyList = datesWithin.Select(entry => entry.Value.earningStats.earnedMoney);
                float earnedMoneyOfLastYear_ = (float)earnedMoneyList.Sum() / earnedMoneyList.Count() * N_EXPECTED_MONTHS;
                earnedMoneyOfLastYear = earnedMoneyOfLastYear_.ToString("F0");

                var gdpList = datesWithin.Select(entry => entry.Value.earningStats.gdp);
                float gdpOfLastYear_ = (float)gdpList.Sum() / gdpList.Count() * N_EXPECTED_MONTHS;
                gdpOfLastYear = gdpOfLastYear_.ToString("F0");
            }
            else
            {
                earnedMoneyOfLastYear = "???";
                gdpOfLastYear = "???";
            }                

            return new string[] {
                "平均综合健康： " + TaiwuCommon.SetColor(TaiwuCommon.COLOR_WHITE, avgCompositeHealth) + " %",
                "平均工作动力： " + TaiwuCommon.SetColor(TaiwuCommon.COLOR_WHITE, avgWorkMotivation) + " %",
                "平均工作效率： " + TaiwuCommon.SetColor(TaiwuCommon.COLOR_WHITE, avgWorkEffectiveness) + " %",
                "一年内银钱收入： " + TaiwuCommon.SetColor(TaiwuCommon.COLOR_YELLOW, earnedMoneyOfLastYear),
                "一年内生产总值： " + TaiwuCommon.SetColor(TaiwuCommon.COLOR_YELLOW, gdpOfLastYear),
            };
        }


        public void AppendMessage(TaiwuDate date, int importance, string message)
        {
            if (!this.history.ContainsKey(date)) this.history[date] = new Record(date);

            this.history[date].messages.Add(new Message(importance, message));
        }


        public void SetWorkerStats(TaiwuDate date, WorkerStats workerStats)
        {
            if (!this.history.ContainsKey(date)) this.history[date] = new Record(date);

            this.history[date].workerStats = workerStats;
        }


        public void SetWorkingStats(TaiwuDate date, WorkingStats workingStats)
        {
            if (!this.history.ContainsKey(date)) this.history[date] = new Record(date);

            this.history[date].workingStats = workingStats;
        }


        public void SetEarningStats(TaiwuDate date, EarningStats earningStats)
        {
            if (!this.history.ContainsKey(date)) this.history[date] = new Record(date);

            this.history[date].earningStats = earningStats;
        }
    }


    /// <summary>
    /// Patch: 在建筑窗口创建时注册管家界面（在其他 mod 之后注册）
    /// </summary>
    [HarmonyPatch(typeof(BuildingWindow), "instance", MethodType.Getter)]
    [HarmonyPriority(Priority.Last)]
    public static class BuildingWindow_instance_Getter_RegisterMajordomoWindow
    {
        static void Prefix(ref bool __state, BuildingWindow ____inst)
        {
            if (!Main.enabled) return;

            __state = ____inst == null;
        }


        static void Postfix(bool __state)
        {
            if (!Main.enabled) return;

            if (__state) MajordomoWindow.TryRegisterResources();
        }
    }
}
