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
        public static readonly int IMPORTANCE_LOWEST = 0;
        public static readonly int IMPORTANCE_LOW = 25;
        public static readonly int IMPORTANCE_NORMAL = 50;
        public static readonly int IMPORTANCE_HIGH = 75;
        public static readonly int IMPORTANCE_HIGHEST = 100;

        public static readonly string[] MESSAGE_IMPORTANCE_NAMES = new string[] { "最低", "低", "普通", "高", "最高" };

        public int importance;
        public string content;


        public Message(int importance, string content)
        {
            this.importance = importance;
            this.content = content;
        }
    }


    /// <summary>
    /// 管家界面内的单个月内的所有消息的集合
    /// </summary>
    public class Record
    {
        public List<Message> messages;
        public float compositeWorkIndex;
        public float earnedMoney;


        public Record()
        {
            this.messages = new List<Message>();
        }


        public Record(TaiwuDate date)
        {
            this.messages = new List<Message>
            {
                new Message(Message.IMPORTANCE_HIGHEST, $"进入{date.ToString()}。"),
            };
        }
    }


    /// <summary>
    /// 管家界面
    /// </summary>
    public class MajordomoWindow
    {
        public static readonly int RECORD_SHELF_LIFE = 10 * 12;
        public static readonly int MESSAGE_SHELF_LIFE = 2 * 12;

        public static MajordomoWindow instance;

        private GameObject window;
        private bool isWindowOpening;
        private GameObject menu;
        private GameObject assignBuildingWorkersButton;
        private GameObject mainHolder;
        private Text textMessage;
        private Text textMessagePage;
        private Text textSummary;

        private Dictionary<TaiwuDate, Record> history = new Dictionary<TaiwuDate, Record>();
        private int dateIndex = -1;


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
                !MajordomoWindow.instance.mainHolder ||
                !MajordomoWindow.instance.textSummary)
            {
                MajordomoWindow.instance.CreateMainWindow();
            }

            if (!MajordomoWindow.instance.assignBuildingWorkersButton ||
                !MajordomoWindow.instance.textMessage ||
                !MajordomoWindow.instance.textMessagePage)
            {
                MajordomoWindow.instance.CreateMessagePage();
            }

            MajordomoWindow.instance.window.SetActive(false);
            MajordomoWindow.instance.isWindowOpening = false;
        }


        /// <summary>
        /// 从 DateFile.modDate 中载入已保存的数据
        /// </summary>
        public void LoadSavedData()
        {
            if (!DateFile.instance.modDate.ContainsKey(Main.MOD_ID)) return;
            var savedData = DateFile.instance.modDate[Main.MOD_ID];

            string saveKeyHistory = "MajordomoWindow.history";
            if (savedData.ContainsKey(saveKeyHistory))
            {
                string data = savedData[saveKeyHistory];
                this.history = JsonConvert.DeserializeObject<KeyValuePair<TaiwuDate, Record>[]>(data)
                    .ToDictionary(kv => kv.Key, kv => kv.Value);
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

            this.dateIndex = this.history.Count - 1;
        }


        private void CreateMainWindow()
        {
            // clone & modify main window
            Debug.Assert(QuquBox.instance.ququBoxWindow);
            this.window = UnityEngine.Object.Instantiate(
                QuquBox.instance.ququBoxWindow, QuquBox.instance.ququBoxWindow.transform.parent);
            this.window.SetActive(true);
            this.window.name = "MajordomoWindow";

            Common.RemoveComponent<QuquBox>(this.window);
            Common.RemoveChildren(this.window, new List<string> { "ChooseItemMask", "ItemsBack" });

            // modify main holder
            this.mainHolder = Common.GetChild(this.window, "QuquBoxHolder");
            Debug.Assert(this.mainHolder);
            this.mainHolder.name = "MajordomoWindowMainHolder";

            Common.RemoveComponent<GridLayoutGroup>(this.mainHolder);
            Common.RemoveChildren(this.mainHolder);

            // clone & modify menu
            this.menu = UnityEngine.Object.Instantiate(this.mainHolder, this.window.transform);
            this.menu.SetActive(true);
            this.menu.name = "MajordomoWindowMenu";

            Common.RemoveChildren(this.menu);

            // resize main holder & menu
            var rectTransform = this.mainHolder.GetComponent<RectTransform>();
            float width = rectTransform.offsetMax.x - rectTransform.offsetMin.x;
            float mainHolderWidth = width * 0.9f;
            float menuWidth = width - mainHolderWidth;
            rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x + menuWidth, rectTransform.offsetMin.y);

            rectTransform = this.menu.GetComponent<RectTransform>();
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x - mainHolderWidth, rectTransform.offsetMax.y);

            // set menu layout
            int menuItemWidth = 70;
            int menuItemMargin = 20;
            var gridLayoutGroup = Common.RemoveComponent<GridLayoutGroup>(this.menu, recreate: true);
            gridLayoutGroup.padding = new RectOffset(menuItemMargin, menuItemMargin, menuItemMargin, menuItemMargin);
            gridLayoutGroup.cellSize = new Vector2(menuItemWidth, menuItemWidth);
            gridLayoutGroup.spacing = new Vector2(menuItemMargin, menuItemMargin);
            gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
            gridLayoutGroup.childAlignment = TextAnchor.UpperCenter;

            // modify close button
            var goCloseButton = Common.GetChild(this.window, "CloseQuquBoxButton");
            Debug.Assert(goCloseButton);
            goCloseButton.name = "MajordomoWindowCloseButton";

            var closeButton = Common.RemoveComponent<Button>(goCloseButton, recreate: true);
            closeButton.onClick.AddListener(() => this.Close());

            var escWinComponent = Common.RemoveComponent<EscWinComponent>(goCloseButton, recreate: true);
            escWinComponent.escEvent = new EscWinComponent.EscCompEvent();
            escWinComponent.escEvent.AddListener(() => this.Close());

            // modify summary bar
            var goSummaryBack = Common.GetChild(this.window, "AllQuquLevelBack");
            Debug.Assert(goSummaryBack);
            goSummaryBack.name = "MajordomoWindowSummaryBack";

            var goSummary = Common.GetChild(goSummaryBack, "AllQuquLevelText");
            Debug.Assert(goSummary);
            goSummary.name = "MajordomoWindowSummary";

            this.textSummary = goSummary.GetComponent<Text>();
            Debug.Assert(this.textSummary);
        }


        private void CreateMessagePage()
        {
            this.CreateMessagePageMenuControls();

            // clone & modify message view
            Debug.Assert(ActorMenu.instance.actorMassage);
            var oriMessageView = Common.GetChild(ActorMenu.instance.actorMassage, "ActorMassageView");
            Debug.Assert(oriMessageView);

            var messageView = UnityEngine.Object.Instantiate(oriMessageView, this.mainHolder.transform);
            messageView.SetActive(true);
            messageView.name = "MajordomoMessageView";

            // modify message view port
            var viewPort = Common.GetChild(messageView, "ActorMassageViewport");
            Debug.Assert(viewPort);
            viewPort.name = "MajordomoMessageViewport";

            // modify message text
            var messageText = Common.GetChild(viewPort, "ActorMassageText");
            Debug.Assert(messageText);
            messageText.name = "MajordomoMessageText";

            this.textMessage = messageText.GetComponent<Text>();
            Debug.Assert(this.textMessage);

            // modify message scroll bar
            var scrollbar = Common.GetChild(messageView, "ActorMassageScrollbar");
            Debug.Assert(scrollbar);
            scrollbar.name = "MajordomoMessageScrollbar";

            // clone & modify page text
            var oriPageText = Common.GetChild(ActorMenu.instance.actorMassage, "PageText");
            Debug.Assert(oriPageText);

            var pageText = UnityEngine.Object.Instantiate(oriPageText, this.mainHolder.transform);
            pageText.SetActive(true);
            pageText.name = "MajordomoPageText";
            Common.TranslateUI(pageText, 0, 20);

            this.textMessagePage = pageText.GetComponent<Text>();
            Debug.Assert(this.textMessagePage);

            // clone & modify page button prev
            var oriPageButtonPrev = Common.GetChild(ActorMenu.instance.actorMassage, "Page-Button");
            Debug.Assert(oriPageButtonPrev);

            var pageButtonPrev = UnityEngine.Object.Instantiate(oriPageButtonPrev, this.mainHolder.transform);
            pageButtonPrev.SetActive(true);
            pageButtonPrev.name = "MajordomoPageButtonPrev";
            Common.TranslateUI(pageButtonPrev, 0, 20);

            var btnPrev = Common.RemoveComponent<Button>(pageButtonPrev, recreate: true);
            btnPrev.onClick.AddListener(() => this.ChangeMessagePage(next: false));

            // clone & modify page button next
            var oriPageButtonNext = Common.GetChild(ActorMenu.instance.actorMassage, "Page+Button");
            Debug.Assert(oriPageButtonNext);

            var pageButtonNext = UnityEngine.Object.Instantiate(oriPageButtonNext, this.mainHolder.transform);
            pageButtonNext.SetActive(true);
            pageButtonNext.name = "MajordomoPageButtonNext";
            Common.TranslateUI(pageButtonNext, 0, 20);

            var btnNext = Common.RemoveComponent<Button>(pageButtonNext, recreate: true);
            btnNext.onClick.AddListener(() => this.ChangeMessagePage(next: true));
        }


        private void CreateMessagePageMenuControls()
        {
            this.assignBuildingWorkersButton = this.CreateMenuButton("AssignBuildingWorkersButton",
                () => { HumanResource.AssignBuildingWorkersForTaiwuVillage(); this.ShowMessage(showLastPage: true); },
                Path.Combine(Path.Combine(Main.resBasePath, "Texture"), $"ButtonIcon_Majordomo_AssignBuildingWorkers.png"),
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_WHITE, "重新指派工作"));
        }


        private GameObject CreateMenuButton(string name, UnityEngine.Events.UnityAction callback, string iconPath, string label)
        {
            // clone & modify button
            Debug.Assert(HomeSystem.instance.studyActor);
            var studySkillButton = Common.GetChild(HomeSystem.instance.studyActor, "StudySkill,0");
            Debug.Assert(studySkillButton);

            var goMenuButton = UnityEngine.Object.Instantiate(studySkillButton, this.menu.transform);
            goMenuButton.SetActive(true);
            goMenuButton.name = name;
            goMenuButton.tag = "Untagged";

            var button = goMenuButton.AddComponent<Button>();
            button.onClick.AddListener(callback);
            goMenuButton.AddComponent<PointerClick>();

            // modify button background
            var buttonBack = Common.GetChild(goMenuButton, "StudyEffectBack");
            Debug.Assert(buttonBack);
            buttonBack.name = "MajordomoMenuButtonBack";

            var rectTransform = buttonBack.GetComponent<RectTransform>();
            rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x - 10, rectTransform.offsetMin.y);
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x + 10, rectTransform.offsetMax.y);

            // modify button icon
            var buttonIcon = Common.GetChild(goMenuButton, "StudySkillIcon,0");
            Debug.Assert(buttonIcon);
            buttonIcon.name = "MajordomoMenuButtonIcon";

            var buttonIconImage = buttonIcon.GetComponent<Image>();
            buttonIconImage.sprite = ResourceLoader.CreateSpriteFromImage(iconPath);
            if (!buttonIconImage.sprite) throw new Exception($"Failed to create sprite: {iconPath}");

            // modify button text
            var buttonText = Common.GetChild(goMenuButton, "StudyEffectText");
            Debug.Assert(buttonText);
            buttonText.name = "MajordomoMenuButtonText";

            rectTransform = buttonText.GetComponent<RectTransform>();
            rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x - 10, rectTransform.offsetMin.y);
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x + 10, rectTransform.offsetMax.y);

            var text = buttonText.GetComponent<Text>();
            Debug.Assert(text);
            text.text = label;

            return goMenuButton;
        }


        public void Open()
        {
            if (this.isWindowOpening) return;
            this.isWindowOpening = true;

            this.ShowMessage(showLastPage: true);
            this.ShowSummary();

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
        /// 由于过早的月份的消息会被删除，故只显示最近的数个月的消息
        /// </summary>
        /// <param name="showLastPage"></param>
        private void ShowMessage(bool showLastPage = false)
        {
            if (showLastPage) this.dateIndex = this.history.Count - 1;

            int baseDateIndex = Mathf.Max(this.history.Count - MESSAGE_SHELF_LIFE, 0);
            var orderedDates = this.history.Keys.OrderByDescending(date => date).Take(MESSAGE_SHELF_LIFE).Reverse().ToList();

            if (this.dateIndex >= baseDateIndex && this.dateIndex < this.history.Count)
            {
                var date = orderedDates[this.dateIndex - baseDateIndex];
                var record = this.history[date];
                this.textMessage.text = string.Join("\n", record.messages
                    .Where(message => message.importance >= Main.settings.messageImportanceThreshold)
                    .Select(message => TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_GRAY, "·") + " " + message.content)
                    .ToArray());
            }
            else
                this.textMessage.text = string.Empty;

            this.textMessagePage.text = $"{this.dateIndex - baseDateIndex + 1} / {orderedDates.Count}";
        }


        private void ChangeMessagePage(bool next)
        {
            int baseDateIndex = Mathf.Max(this.history.Count - MESSAGE_SHELF_LIFE, 0);
            int nPages = this.history.Count - baseDateIndex;

            if (nPages == 0) return;

            this.dateIndex += next ? 1 : -1;

            if (this.dateIndex >= this.history.Count)
                this.dateIndex -= nPages;
            else if (this.dateIndex < baseDateIndex)
                this.dateIndex += nPages;

            this.ShowMessage();
        }


        private void ShowSummary()
        {
            this.textSummary.text = "综合工作指数:  " + this.GetCurrentCompositeWorkIndex() + " 点" +
                "          一年内收入:  " + this.GetEarnedMoneyOfLastYear() + " 银钱";
        }


        /// <summary>
        /// 获取当前综合工作指数
        /// </summary>
        /// <returns></returns>
        private string GetCurrentCompositeWorkIndex()
        {
            string text;
            if (this.history.Count > 0)
            {
                var newestDate = this.history.Keys.Max(date => date);
                text = this.history[newestDate].compositeWorkIndex.ToString("F0");
            }
            else
                text = "???";

            return TaiwuCommon.SetColor(TaiwuCommon.COLOR_DARK_BROWN, text);
        }


        /// <summary>
        /// 计算过去一年内金钱收入
        /// 数据不足时会进行估算，至少有三个月数据才计算
        /// 如果数据中间有空缺月份，则计算跨度会大于一年
        /// </summary>
        /// <returns></returns>
        private string GetEarnedMoneyOfLastYear()
        {
            const int MIN_CALCULATING_MONTHS = 3;
            const int N_EXPECTED_MONTHS = 12;

            string text;
            if (this.history.Count >= MIN_CALCULATING_MONTHS)
            {
                var earnedMoneyList = this.history.OrderByDescending(entry => entry.Key).Take(N_EXPECTED_MONTHS)
                    .Select(entry => entry.Value.earnedMoney);
                float earnedMoneyOfLastYear = earnedMoneyList.Sum() / earnedMoneyList.Count() * N_EXPECTED_MONTHS;
                text = earnedMoneyOfLastYear.ToString("F0");
            }
            else
                text = "???";

            return TaiwuCommon.SetColor(TaiwuCommon.COLOR_YELLOW, text);
        }


        public void AppendMessage(TaiwuDate date, int importance, string message)
        {
            if (!this.history.ContainsKey(date)) this.history[date] = new Record(date);

            this.history[date].messages.Add(new Message(importance, message));
        }


        public void SetCompositeWorkIndex(TaiwuDate date, float compositeWorkIndex)
        {
            if (!this.history.ContainsKey(date)) this.history[date] = new Record(date);

            this.history[date].compositeWorkIndex = compositeWorkIndex;
        }


        public void SetEarnedMoney(TaiwuDate date, int earnedMoney)
        {
            if (!this.history.ContainsKey(date)) this.history[date] = new Record(date);

            this.history[date].earnedMoney = earnedMoney;
        }
    }


    /// <summary>
    /// Patch: 注册管家界面（在其他 mod 之后注册）
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "Start")]
    [HarmonyPriority(Priority.Last)]
    public static class HomeSystem_Start_RegisterMajordomoWindow
    {
        static void Postfix()
        {
            if (!Main.enabled) return;

            MajordomoWindow.TryRegisterResources();
        }
    }
}
