using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Majordomo
{
    public class PanelLogs : ITaiwuWindow
    {
        public const int N_MESSAGES_PER_CONTENT_ITEM = 10;
        private const int MESSAGE_MAX_LENGTH = 1000;

        private GameObject parent;
        private GameObject panel;
        private GameObject messageContent;
        private Text textMessagePage;

        private readonly Dictionary<TaiwuDate, Record> history;
        private int sortedDateIndex = -1;     // 历史记录按日期从小到大排列时，当前页数对应日期的序号


        public GameObject gameObject {
            get
            {
                return panel;
            }
        }


        public PanelLogs(GameObject panelContainer, Dictionary<TaiwuDate, Record> history)
        {
            this.parent = panelContainer;
            this.history = history;
        }


        public void TryRegisterResources(GameObject parent)
        {
            this.parent = parent;

            if (!this.panel || !this.messageContent || !this.textMessagePage)
                this.CreatePanel();

            this.panel.SetActive(false);
        }


        public void Open()
        {
            this.UpdateMessage();
            this.panel.SetActive(true);
        }


        public void Update()
        {
            this.UpdateMessage();
        }


        public void Close()
        {
            this.panel.SetActive(false);
        }


        /// <summary>
        /// 设置历史记录按日期从小到大排列时，当前页数对应的序号，从 0 开始
        /// 序号小于 0 或大于总页数时，表示翻到最后一页
        /// </summary>
        /// <param name="index"></param>
        public void SetPageIndex(int index)
        {
            if (index >= 0 && index < this.history.Count)
                this.sortedDateIndex = index;
            else
                this.sortedDateIndex = this.history.Count - 1;
        }


        private void CreatePanel()
        {
            // 此函数的触发条件就是 BuildingWindow.Start, 所以 BuildingWindow 的实例一定存在
            var ququBox = BuildingWindow.instance.GetComponentInChildren<QuquBox>();

            // clone & modify panel
            var oriPanel = Common.GetChild(ququBox.ququBoxWindow, "QuquBoxHolder");
            this.panel = UnityEngine.Object.Instantiate(oriPanel, this.parent.transform);
            this.panel.SetActive(true);
            this.panel.name = "MajordomoPanelLogs";

            var rectTransform = this.panel.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = new Vector2(0, 0);
            rectTransform.offsetMax = new Vector2(0, 0);

            Common.RemoveComponent<GridLayoutGroup>(this.panel);
            Common.RemoveChildren(this.panel);

            // clone & modify message view
            var goActorMenu = Resources.Load<GameObject>("OldScenePrefabs/ActorMenu");
            var actorMenu = goActorMenu.GetComponentInChildren<ActorMenu>();

            var oriMessageView = Common.GetChild(actorMenu.actorMassage, "ActorMassageView");
            if (!oriMessageView) throw new Exception("Failed to get child 'ActorMassageView' from ActorMenu.actorMassage");

            var messageView = UnityEngine.Object.Instantiate(oriMessageView, this.panel.transform);
            messageView.SetActive(true);
            messageView.name = "MajordomoMessageView";

            // modify message view port
            var viewPort = Common.GetChild(messageView, "ActorMassageViewport");
            if (!viewPort) throw new Exception("Failed to get child 'ActorMassageViewport' from 'ActorMassageView'");
            viewPort.name = "MajordomoMessageViewport";

            // get message content, create message content item
            this.messageContent = Common.GetChild(viewPort, "ActorMassageText");
            if (!this.messageContent) throw new Exception("Failed to get child 'ActorMassageText' from 'ActorMassageViewport'");
            this.messageContent.name = "MajordomoMessageContent";

            var messageContentItem = UnityEngine.Object.Instantiate(this.messageContent, this.messageContent.transform);
            messageContentItem.SetActive(true);
            messageContentItem.name = "MajordomoMessageContentItem";

            // modify message content
            Common.RemoveComponent<Text>(this.messageContent);
            Common.RemoveComponent<SetFont>(this.messageContent);

            var verticalLayoutGroup = this.messageContent.AddComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.childForceExpandWidth = false;
            verticalLayoutGroup.childForceExpandHeight = false;

            // modify message content item
            var text = messageContentItem.GetComponent<Text>();
            TaiwuCommon.SetFont(text);
            text.text = string.Empty;

            Common.RemoveComponent<ContentSizeFitter>(messageContentItem);
            Common.RemoveComponent<SetFont>(messageContentItem);

            // modify message scroll bar
            var scrollbar = Common.GetChild(messageView, "ActorMassageScrollbar");
            if (!scrollbar) throw new Exception("Failed to get child 'ActorMassageScrollbar' from 'ActorMassageView'");
            scrollbar.name = "MajordomoMessageScrollbar";

            // clone & modify page text
            var oriPageText = Common.GetChild(actorMenu.actorMassage, "PageText");
            if (!oriPageText) throw new Exception("Failed to get child 'PageText' from ActorMenu.actorMassage");

            var pageText = UnityEngine.Object.Instantiate(oriPageText, this.panel.transform);
            pageText.SetActive(true);
            pageText.name = "MajordomoPageText";
            Common.TranslateUI(pageText, 0, 20);

            this.textMessagePage = pageText.GetComponent<Text>();
            if (!this.textMessagePage) throw new Exception("Failed to get Text component from 'PageText'");
            TaiwuCommon.SetFont(this.textMessagePage);

            Common.RemoveComponent<SetFont>(pageText);

            // clone & modify page button prev
            var oriPageButtonPrev = Common.GetChild(actorMenu.actorMassage, "Page-Button");
            if (!oriPageButtonPrev) throw new Exception("Failed to get child 'Page-Button' from ActorMenu.actorMassage");

            var pageButtonPrev = UnityEngine.Object.Instantiate(oriPageButtonPrev, this.panel.transform);
            pageButtonPrev.SetActive(true);
            pageButtonPrev.name = "MajordomoPageButtonPrev";
            Common.TranslateUI(pageButtonPrev, 0, 20);

            var btnPrev = Common.RemoveComponent<Button>(pageButtonPrev, recreate: true);
            btnPrev.onClick.AddListener(() => this.ChangeMessagePage(next: false));

            // clone & modify page button next
            var oriPageButtonNext = Common.GetChild(actorMenu.actorMassage, "Page+Button");
            if (!oriPageButtonNext) throw new Exception("Failed to get child 'Page+Button' from ActorMenu.actorMassage");

            var pageButtonNext = UnityEngine.Object.Instantiate(oriPageButtonNext, this.panel.transform);
            pageButtonNext.SetActive(true);
            pageButtonNext.name = "MajordomoPageButtonNext";
            Common.TranslateUI(pageButtonNext, 0, 20);

            var btnNext = Common.RemoveComponent<Button>(pageButtonNext, recreate: true);
            btnNext.onClick.AddListener(() => this.ChangeMessagePage(next: true));
        }


        /// <summary>
        /// 由于过早的月份的消息会被删除，故只显示最近的数个月的消息
        /// </summary>
        private void UpdateMessage()
        {
            int baseDateIndex = Mathf.Max(this.history.Count - MajordomoWindow.MESSAGE_SHELF_LIFE, 0);
            var orderedDates = this.history.Keys.OrderByDescending(date => date)
                .Take(MajordomoWindow.MESSAGE_SHELF_LIFE).Reverse().ToList();

            if (this.sortedDateIndex >= baseDateIndex && this.sortedDateIndex < this.history.Count)
            {
                var date = orderedDates[this.sortedDateIndex - baseDateIndex];
                var record = this.history[date];
                var messages = record.messages
                    .Where(message => message.importance >= Main.settings.messageImportanceThreshold)
                    .Select(message => {
                        var content = message.content.Length > MESSAGE_MAX_LENGTH ?
                            message.content.Substring(0, MESSAGE_MAX_LENGTH) +
                                TaiwuCommon.SetColor(TaiwuCommon.COLOR_DARK_GRAY, "……") :
                            message.content;
                        return TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_GRAY, "·") + " " + content;
                        })
                    .ToList();
                this.CreateMessageContentItems(messages);
            }
            else
                this.CreateMessageContentItems(new List<string>());

            this.textMessagePage.text = $"{this.sortedDateIndex - baseDateIndex + 1} / {orderedDates.Count}";
        }


        /// <summary>
        /// 根据消息行数，创建对应数量的消息容纳控件（或重复利用之前的控件），并填入对应的文本
        /// </summary>
        /// <param name="messages"></param>
        private void CreateMessageContentItems(List<string> messages)
        {
            var item0 = this.messageContent.transform.GetChild(0).gameObject;
            if (!item0) throw new Exception("Failed to find the first item of MajordomoMessageContent");

            var messageChunks = Common.SplitList(messages, N_MESSAGES_PER_CONTENT_ITEM).ToList();
            int nItemsNeeded = messageChunks.Count;
            int nActualItems = this.messageContent.transform.childCount;

            for (int i = 0; i < nItemsNeeded; ++i)
            {
                GameObject currItem;

                if (i < nActualItems)
                {
                    currItem = this.messageContent.transform.GetChild(i).gameObject;
                }
                else
                {
                    currItem = UnityEngine.Object.Instantiate(item0, this.messageContent.transform);
                    currItem.SetActive(true);
                }

                var currText = currItem.GetComponent<Text>();
                currText.text = string.Join("\n", messageChunks[i].ToArray());
            }

            for (int i = nItemsNeeded; i < nActualItems; ++i)
            {
                // 不删除第一个 Text 控件（没有它就不能复制了）
                var currItem = this.messageContent.transform.GetChild(i).gameObject;
                if (i == 0)
                {
                    var currText = currItem.GetComponent<Text>();
                    currText.text = string.Empty;
                }
                else
                    UnityEngine.Object.Destroy(currItem);
            }
        }


        private void ChangeMessagePage(bool next)
        {
            int baseDateIndex = Mathf.Max(this.history.Count - MajordomoWindow.MESSAGE_SHELF_LIFE, 0);
            int nPages = this.history.Count - baseDateIndex;

            if (nPages == 0) return;

            this.sortedDateIndex += next ? 1 : -1;

            if (this.sortedDateIndex >= this.history.Count)
                this.sortedDateIndex -= nPages;
            else if (this.sortedDateIndex < baseDateIndex)
                this.sortedDateIndex += nPages;

            this.UpdateMessage();
        }
    }
}
