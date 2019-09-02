using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Majordomo
{
    public class PanelCharts : ITaiwuWindow
    {
        public const int METRIC_IDX_AVG_COMPOSITE_HEALTH = 0;
        public const int METRIC_IDX_AVG_WORK_MOTIVATION = 1;
        public const int METRIC_IDX_AVG_WORK_EFFECTIVENESS = 2;
        public const int METRIC_IDX_COMPOSITE_WORK_INDEX = 3;
        public const int METRIC_IDX_EARNED_MONEY = 4;
        public const int METRIC_IDX_GDP = 5;

        public const int GRANULARITY_IDX_MONTHLY = 0;
        public const int GRANULARITY_IDX_QUARTERLY = 1;
        public const int GRANULARITY_IDX_ANNUAL = 2;

        public const int MIN_X_COUNT = 2;
        public const int MAX_X_COUNT = 60;
        public const int X_LABEL_COUNT = 10;
        public const int Y_LABEL_COUNT = 5;

        public const int DOT_RADIUS = 5;

        public static readonly Color WINDOW_BG_COLOR_DEFAULT = new Color(0f, 0f, 0f, 0.5f);
        public static readonly Color WINDOW_BG_COLOR_TRANSPARENT = new Color(0f, 0f, 0f, 0f);
        public static readonly Color BTN_COLOR_UNSELECTED = new Color(0.984f, 0.984f, 0.984f, 1.000f);
        public static readonly Color BTN_COLOR_SELECTED = new Color(0.016f, 0.016f, 0.016f, 1.000f);
        public static readonly Color BTN_BG_COLOR_UNSELECTED = new Color(0.191f, 0.169f, 0.157f, 1.000f);
        public static readonly Color BTN_BG_COLOR_SELECTED = new Color(0.809f, 0.831f, 0.843f, 1.000f);
        public static readonly Color CHART_AXIS_COLOR = new Color(0.75f, 0.75f, 0.75f, 1.0f);
        public static readonly Color CHART_LABEL_COLOR = new Color(0.75f, 0.75f, 0.75f, 1.0f);
        public static readonly Color CHART_LINE_COLOR = new Color(0.75f, 0.75f, 0.75f, 1.0f);
        public static readonly Color CHART_DOT_COLOR = new Color(0.75f, 0.75f, 0.75f, 1.0f);

        // 在记录序列中选取各个指标的方法
        private static readonly List<Func<KeyValuePair<TaiwuDate, Record>, double>> METRICS_SELECTORS =
            new List<Func<KeyValuePair<TaiwuDate, Record>, double>>
            {
                entry => entry.Value.workerStats.avgCompositeHealth,
                entry => entry.Value.workerStats.avgWorkMotivation,
                entry => entry.Value.workingStats.avgWorkEffectiveness,
                entry => entry.Value.workingStats.compositeWorkIndex,
                entry => entry.Value.earningStats.earnedMoney,
                entry => entry.Value.earningStats.gdp,
            };

        // 各个指标格式化成字串的方法
        private static readonly List<Func<double, string>> METRICS_FORMATTERS =
            new List<Func<double, string>>
            {
                value => TaiwuCommon.SetColor(TaiwuCommon.COLOR_WHITE, (value * 100).ToString("F2")) + " %",
                value => TaiwuCommon.SetColor(TaiwuCommon.COLOR_WHITE, (value * 100).ToString("F2")) + " %",
                value => TaiwuCommon.SetColor(TaiwuCommon.COLOR_WHITE, (value * 100).ToString("F2")) + " %",
                value => TaiwuCommon.SetColor(TaiwuCommon.COLOR_DARK_BROWN, value.ToString("F0")),
                value => TaiwuCommon.SetColor(TaiwuCommon.COLOR_YELLOW, value.ToString("F0")),
                value => TaiwuCommon.SetColor(TaiwuCommon.COLOR_YELLOW, value.ToString("F0")),
            };

        // 各个粒度下单个数据包含的月份数
        private static readonly int[] GRANULARITY_MONTHS = { 1, 3, 12 };

        private GameObject parent;
        private GameObject panel;
        private GameObject chartContainer;
        private GameObject metricButtonContainer;
        private GameObject granularityButtonContainer;
        private Sprite spriteChartDot;

        private readonly Dictionary<TaiwuDate, Record> history;

        private int selectedMetricId;
        private int selectedGranularityId;


        public GameObject gameObject
        {
            get
            {
                return panel;
            }
        }


        public PanelCharts(GameObject panelContainer, Dictionary<TaiwuDate, Record> history)
        {
            this.parent = panelContainer;
            this.history = history;
        }


        public void TryRegisterResources(GameObject parent)
        {
            this.parent = parent;

            if (!this.spriteChartDot)
            {
                string imagePath = Path.Combine(Path.Combine(Main.resBasePath, "Texture"), $"ChartSprite_Dot.png");
                this.spriteChartDot = ResourceLoader.CreateSpriteFromImage(imagePath);
                if (!this.spriteChartDot) throw new Exception($"Failed to create sprite: {imagePath}");
            }

            if (!this.panel || !this.chartContainer || !this.metricButtonContainer || !this.granularityButtonContainer)
                this.CreatePanel();

            this.SwitchMetric(PanelCharts.METRIC_IDX_COMPOSITE_WORK_INDEX, updateChart: false);
            this.SwitchGranularity(PanelCharts.GRANULARITY_IDX_MONTHLY, updateChart: false);

            this.panel.SetActive(false);
        }


        public void Open()
        {
            this.UpdateChart();
            this.panel.SetActive(true);
        }


        public void Update()
        {
            this.UpdateChart();
        }


        public void Close()
        {
            this.panel.SetActive(false);
        }


        private void CreatePanel()
        {
            // 此函数的触发条件就是 BuildingWindow.instance 存在
            var ququBox = BuildingWindow.instance.GetComponentInChildren<QuquBox>();

            // clone & modify panel
            var oriPanel = Common.GetChild(ququBox.ququBoxWindow, "QuquBoxHolder");
            this.panel = UnityEngine.Object.Instantiate(oriPanel, this.parent.transform);
            this.panel.SetActive(true);
            this.panel.name = "MajordomoPanelCharts";

            var rectTransform = this.panel.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = new Vector2(0, 0);
            rectTransform.offsetMax = new Vector2(0, 0);

            Common.RemoveComponent<GridLayoutGroup>(this.panel);
            Common.RemoveChildren(this.panel);

            // create metric buttons
            this.metricButtonContainer = new GameObject("MetricButtonContainer", typeof(Image));
            this.metricButtonContainer.transform.SetParent(this.panel.transform, false);

            this.metricButtonContainer.GetComponent<Image>().color = PanelCharts.WINDOW_BG_COLOR_TRANSPARENT;

            rectTransform = this.metricButtonContainer.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = new Vector2(30, -90);
            rectTransform.offsetMax = new Vector2(-30, -30);

            var gridLayoutGroup = this.metricButtonContainer.AddComponent<GridLayoutGroup>();
            gridLayoutGroup.padding = new RectOffset(20, 20, 0, 0);
            gridLayoutGroup.cellSize = new Vector2(110, 30);
            gridLayoutGroup.spacing = new Vector2(20, 20);
            gridLayoutGroup.childAlignment = TextAnchor.MiddleLeft;

            this.CreateMetricButtons();

            // create granularity buttons
            this.granularityButtonContainer = new GameObject("GranularityButtonContainer", typeof(Image));
            this.granularityButtonContainer.transform.SetParent(this.panel.transform, false);

            this.granularityButtonContainer.GetComponent<Image>().color = PanelCharts.WINDOW_BG_COLOR_TRANSPARENT;

            rectTransform = this.granularityButtonContainer.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.offsetMin = new Vector2(30, 30);
            rectTransform.offsetMax = new Vector2(-30, 90);

            gridLayoutGroup = this.granularityButtonContainer.AddComponent<GridLayoutGroup>();
            gridLayoutGroup.padding = new RectOffset(20, 20, 0, 0);
            gridLayoutGroup.cellSize = new Vector2(110, 30);
            gridLayoutGroup.spacing = new Vector2(20, 20);
            gridLayoutGroup.childAlignment = TextAnchor.MiddleRight;

            this.CreateGranularityButtons();

            // create chart container
            this.chartContainer = new GameObject("ChartContainer", typeof(Image));
            this.chartContainer.transform.SetParent(this.panel.transform, false);

            this.chartContainer.GetComponent<Image>().color = PanelCharts.WINDOW_BG_COLOR_TRANSPARENT;

            rectTransform = this.chartContainer.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = new Vector2(30, 100);
            rectTransform.offsetMax = new Vector2(-30, -100);
        }

        private void CreateMetricButtons()
        {
            this.CreateButton("AvgCompositeHealthButton", "平均综合健康",
                () => { this.SwitchMetric(PanelCharts.METRIC_IDX_AVG_COMPOSITE_HEALTH); },
                this.metricButtonContainer);

            this.CreateButton("AvgWorkMotivationButton", "平均工作动力",
                () => { this.SwitchMetric(PanelCharts.METRIC_IDX_AVG_WORK_MOTIVATION); },
                this.metricButtonContainer);

            this.CreateButton("AvgWorkEffectivenessButton", "平均工作效率",
                () => { this.SwitchMetric(PanelCharts.METRIC_IDX_AVG_WORK_EFFECTIVENESS); },
                this.metricButtonContainer);

            this.CreateButton("CompositeWorkIndexButton", "综合工作指数",
                () => { this.SwitchMetric(PanelCharts.METRIC_IDX_COMPOSITE_WORK_INDEX); },
                this.metricButtonContainer);

            this.CreateButton("EarnedMoneyButton", "收获银钱",
                () => { this.SwitchMetric(PanelCharts.METRIC_IDX_EARNED_MONEY); },
                this.metricButtonContainer);

            this.CreateButton("GdpButton", "本地生产总值",
                () => { this.SwitchMetric(PanelCharts.METRIC_IDX_GDP); },
                this.metricButtonContainer);
        }


        private void CreateGranularityButtons()
        {
            this.CreateButton("MonthlyButton", "月度",
                () => { this.SwitchGranularity(PanelCharts.GRANULARITY_IDX_MONTHLY); },
                this.granularityButtonContainer);

            this.CreateButton("QuarterlyButton", "季度",
                () => { this.SwitchGranularity(PanelCharts.GRANULARITY_IDX_QUARTERLY); },
                this.granularityButtonContainer);

            this.CreateButton("AnnualButton", "年度",
                () => { this.SwitchGranularity(PanelCharts.GRANULARITY_IDX_ANNUAL); },
                this.granularityButtonContainer);
        }


        private GameObject CreateButton(string name, string label, UnityEngine.Events.UnityAction callback, GameObject parent)
        {
            // clone & modify button
            // 此函数的触发条件就是 BuildingWindow.instance 存在
            var studySkillButton = Common.GetChild(BuildingWindow.instance.studyActor, "StudySkill,0");
            if (!studySkillButton) throw new Exception("Failed to get child 'StudySkill,0' from HomeSystem.instance.studyActor");

            var goButton = UnityEngine.Object.Instantiate(studySkillButton, parent.transform);
            goButton.SetActive(true);
            goButton.name = name;
            goButton.tag = "Untagged";

            var button = goButton.AddComponent<Button>();
            button.onClick.AddListener(callback);
            goButton.AddComponent<PointerClick>();

            Common.RemoveChildren(goButton, new List<string> { "StudySkillIcon,0" });

            // modify button background
            var buttonBack = Common.GetChild(goButton, "StudyEffectBack");
            if (!buttonBack) throw new Exception("Failed to get child 'StudyEffectBack' from 'StudySkill,0'");
            buttonBack.name = "ButtonBack";

            var image = buttonBack.GetComponent<Image>();
            image.color = PanelCharts.BTN_BG_COLOR_UNSELECTED;

            var rectTransform = buttonBack.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = new Vector2(0, 0);
            rectTransform.offsetMax = new Vector2(0, 0);

            // modify button text
            var buttonText = Common.GetChild(goButton, "StudyEffectText");
            if (!buttonText) throw new Exception("Failed to get child 'StudyEffectText' from 'StudySkill,0'");
            buttonText.name = "ButtonText";

            rectTransform = buttonText.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = new Vector2(0, 0);
            rectTransform.offsetMax = new Vector2(0, 0);

            var text = buttonText.GetComponent<Text>();
            if (!text) throw new Exception("Failed to get Text component from 'StudyEffectText'");
            text.text = label;
            text.color = PanelCharts.BTN_COLOR_UNSELECTED;
            TaiwuCommon.SetFont(text);

            Common.RemoveComponent<SetFont>(buttonText);

            return goButton;
        }


        /// <summary>
        /// 切换指标
        /// </summary>
        /// <param name="metricId"></param>
        /// <param name="updateChart"></param>
        private void SwitchMetric(int metricId, bool updateChart = true)
        {
            int childCount = this.metricButtonContainer.transform.childCount;
            if (metricId < 0 || metricId >= childCount)
                throw new Exception($"MetricId out of range: [0, {childCount - 1}] -> {metricId}");

            this.selectedMetricId = metricId;

            for (int i = 0; i < childCount; ++i)
            {
                if (i == metricId) continue;
                this.ChangeButtonStyle(this.metricButtonContainer.transform.GetChild(i).gameObject, selected: false);
            }

            this.ChangeButtonStyle(this.metricButtonContainer.transform.GetChild(metricId).gameObject, selected: true);

            if (updateChart) this.UpdateChart();
        }


        /// <summary>
        /// 切换维度的粒度
        /// </summary>
        /// <param name="granularityId"></param>
        /// <param name="updateChart"></param>
        private void SwitchGranularity(int granularityId, bool updateChart = true)
        {
            int childCount = this.granularityButtonContainer.transform.childCount;
            if (granularityId < 0 || granularityId >= childCount)
                throw new Exception($"GranularityId out of range: [0, {childCount - 1}] -> {granularityId}");

            this.selectedGranularityId = granularityId;

            for (int i = 0; i < childCount; ++i)
            {
                if (i == granularityId) continue;
                this.ChangeButtonStyle(this.granularityButtonContainer.transform.GetChild(i).gameObject, selected: false);
            }

            this.ChangeButtonStyle(this.granularityButtonContainer.transform.GetChild(granularityId).gameObject, selected: true);

            if (updateChart) this.UpdateChart();
        }


        /// <summary>
        /// 根据按钮选中状态更改按钮样式
        /// </summary>
        /// <param name="goButton"></param>
        /// <param name="selected"></param>
        private void ChangeButtonStyle(GameObject goButton, bool selected)
        {
            var goBackground = Common.GetChild(goButton, "ButtonBack");
            var image = goBackground.GetComponent<Image>();

            var goText = Common.GetChild(goButton, "ButtonText");
            var text = goText.GetComponent<Text>();

            if (selected)
            {
                image.color = PanelCharts.BTN_BG_COLOR_SELECTED;
                text.color = PanelCharts.BTN_COLOR_SELECTED;
            }
            else
            {
                image.color = PanelCharts.BTN_BG_COLOR_UNSELECTED;
                text.color = PanelCharts.BTN_COLOR_UNSELECTED;
            }
        }


        private void UpdateChart()
        {
            var yValues = this.GetChartData(out List<string> xLabels, out List<bool> drawXLabels);

            if (yValues.Count != xLabels.Count)
                throw new Exception($"Count of Y values and X labels are not match: {yValues.Count} - {xLabels.Count}");

            Common.RemoveChildren(this.chartContainer);

            if (yValues.Count < PanelCharts.MIN_X_COUNT)
                this.CreateEmptyChartNotice();
            else
                DateFile.instance.StartCoroutine(this.CreateChart(yValues, xLabels, drawXLabels));
        }


        /// <summary>
        /// 创建图表所需数据量不够的提示
        /// </summary>
        private void CreateEmptyChartNotice()
        {
            // create notice
            var notice = new GameObject("EmptyChartNotice", typeof(Image));
            notice.transform.SetParent(this.chartContainer.transform, false);

            notice.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f, 1.0f);

            var rectTransform = notice.GetComponent<RectTransform>();
            rectTransform.offsetMin = new Vector2(-200, 0);
            rectTransform.offsetMax = new Vector2(200, 80);

            // create inner border
            var innerBorder = new GameObject("InnerBorder", typeof(Image));
            innerBorder.transform.SetParent(notice.transform, false);

            innerBorder.GetComponent<Image>().color = new Color(0.125f, 0.125f, 0.125f, 1.0f);

            rectTransform = innerBorder.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = new Vector2(3, 3);
            rectTransform.offsetMax = new Vector2(-3, -3);

            // create text
            var goText = new GameObject("Text", typeof(Text));
            goText.transform.SetParent(innerBorder.transform, false);

            var text = goText.GetComponent<Text>();
            text.text = "数据量不足，无法生成图表";
            text.font = DateFile.instance.font;
            text.fontSize = 18;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);

            rectTransform = goText.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = new Vector2(0, 0);
            rectTransform.offsetMax = new Vector2(0, 0);
        }


        /// <summary>
        /// 创建标签之后，需要获取标签的大小，从而决定坐标轴的位置，
        /// 因此创建标签之后先 yield，直到下一帧才继续执行。
        /// 
        /// 也存在其他手段在当前帧更新标签大小，比如 LayoutRebuilder.ForceRebuildLayoutImmediate()，
        /// 以及 Canvas.ForceUpdateCanvasaes()，可惜在 editor 下面都能正常使用，到了 build 环境就无效了。
        /// 参考链接：
        /// https://forum.unity.com/threads/content-size-fitter-refresh-problem.498536/
        /// https://answers.unity.com/questions/1276433/get-layoutgroup-and-contentsizefitter-to-update-th.html
        /// </summary>
        /// <param name="yValues"></param>
        /// <param name="xLabels"></param>
        /// <param name="drawXLabels"></param>
        /// <returns></returns>
        private IEnumerator CreateChart(List<double> yValues, List<string> xLabels, List<bool> drawXLabels)
        {
            var xLabelGroup = CreateXLabelGroup(xLabels, drawXLabels);
            var yLabelGroup = CreateYLabelGroup(yValues);

            yield return null;

            UpdateXLabelsSizes(xLabelGroup);
            UpdateYLabelsSizes(yLabelGroup);

            var chartInfo = DrawChartFrame(xLabelGroup, yLabelGroup);

            DrawChartContent(chartInfo);
        }


        private void DrawChartContent(ChartInfo chartInfo)
        {
            DrawChartLines(chartInfo);

            DrawChartDots(chartInfo);

            chartInfo.tooltip.transform.SetAsLastSibling();
        }


        private void DrawChartLines(ChartInfo chartInfo)
        {
            var positionA = this.GetPointPosition(chartInfo, 0);

            for (int i = 1; i < chartInfo.yValues.Count; ++i)
            {
                var positionB = this.GetPointPosition(chartInfo, i);

                var dir = (positionB - positionA).normalized;
                float distance = Vector2.Distance(positionA, positionB);

                //
                var lineWrapper = new GameObject("LineWrapper", typeof(Image));
                lineWrapper.transform.SetParent(this.chartContainer.transform, false);

                lineWrapper.GetComponent<Image>().color = PanelCharts.WINDOW_BG_COLOR_TRANSPARENT;

                var rectTransform = lineWrapper.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);
                rectTransform.sizeDelta = new Vector2(distance, 2f);
                rectTransform.anchoredPosition = positionA + dir * distance * .5f;
                rectTransform.localEulerAngles = new Vector3(0, 0, Common.GetAngleFromVectorFloat(dir));

                //
                var line = new GameObject("Line", typeof(Image));
                line.transform.SetParent(lineWrapper.transform, false);

                line.GetComponent<Image>().color = PanelCharts.CHART_LINE_COLOR;

                rectTransform = line.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.offsetMin = new Vector2(PanelCharts.DOT_RADIUS + 2, 0);
                rectTransform.offsetMax = new Vector2(-PanelCharts.DOT_RADIUS - 2 , 0);

                positionA = positionB;
            }
        }


        private Vector2 GetPointPosition(ChartInfo chartInfo, int pointIndex)
        {
            double range = chartInfo.yMaxValue - chartInfo.yMinValue;
            float x = chartInfo.xPositions[pointIndex];
            float y = (float)(chartInfo.originY + (chartInfo.yValues[pointIndex] - chartInfo.yMinValue) / range * chartInfo.chartHeight);
            return new Vector2(x, y);
        }


        private void DrawChartDots(ChartInfo chartInfo)
        {
            for (int i = 0; i < chartInfo.yValues.Count; ++i)
            {
                var position = this.GetPointPosition(chartInfo, i);

                var dotWrapper = new GameObject("DotWrapper", typeof(Image));
                dotWrapper.transform.SetParent(this.chartContainer.transform, false);

                var image = dotWrapper.GetComponent<Image>();
                image.color = PanelCharts.WINDOW_BG_COLOR_TRANSPARENT;

                var rectTransform = dotWrapper.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);
                rectTransform.anchoredPosition = position;
                rectTransform.sizeDelta = new Vector2(20, 20);

                var handler = dotWrapper.AddComponent<ChartDotMouseEventHandler>();
                handler.SetData(position, chartInfo.tooltip,
                    chartInfo.xLabels[i] + ":  " + METRICS_FORMATTERS[this.selectedMetricId](chartInfo.yValues[i]));

                var dot = new GameObject("Dot", typeof(Image));
                dot.transform.SetParent(dotWrapper.transform, false);

                image = dot.GetComponent<Image>();
                image.sprite = this.spriteChartDot;
                image.color = PanelCharts.CHART_DOT_COLOR;

                rectTransform = dot.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.offsetMin = new Vector2(5, 5);
                rectTransform.offsetMax = new Vector2(-5, -5);
            }
        }


        private ChartInfo DrawChartFrame(XLabelGroup xLabelGroup, YLabelGroup yLabelGroup)
        {
            var rectTransform = this.chartContainer.GetComponent<RectTransform>();
            float totalWidth = rectTransform.rect.width;
            float totalHeight = rectTransform.rect.height;
            float originX = yLabelGroup.maxLabelWidth;
            float originY = xLabelGroup.maxLabelHeight;
            float chartWidth = totalWidth - originX;
            float chartHeight = totalHeight - originY;

            var xAxis = DrawXAxis(originX, originY, chartWidth, chartHeight);
            var yAxis = DrawYAxis(originX, originY, chartWidth, chartHeight);

            var xPositions = PlaceXLabels(xLabelGroup, originX, originY, chartWidth, chartHeight);
            PlaceYLabels(yLabelGroup, originX, originY, chartWidth, chartHeight);

            var tooltip = CreateTooltip();

            return new ChartInfo
            {
                originX = originX,
                originY = originY,
                chartWidth = chartWidth,
                chartHeight = chartHeight,
                xAxis = xAxis,
                yAxis = yAxis,
                goXLabels = xLabelGroup.goLabels,
                xLabels = xLabelGroup.labels,
                xPositions = xPositions,
                goYLabels = yLabelGroup.goLabels,
                yLabelValues = yLabelGroup.labelValues,
                yValues = yLabelGroup.values,
                yMinValue = yLabelGroup.minValue,
                yMaxValue = yLabelGroup.maxValue,
                tooltip = tooltip,
            };
        }


        private GameObject CreateTooltip()
        {
            // create tooltip
            var tooltip = new GameObject("Tooltip", typeof(Image));
            tooltip.transform.SetParent(this.chartContainer.transform, false);

            tooltip.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f, 1.0f);

            var horizontalLayoutGroup = tooltip.AddComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.padding = new RectOffset(3, 3, 3, 3);
            horizontalLayoutGroup.childControlWidth = true;
            horizontalLayoutGroup.childControlHeight = true;
            horizontalLayoutGroup.childForceExpandWidth = false;
            horizontalLayoutGroup.childForceExpandHeight = false;

            var contentSizeFitter = tooltip.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var rectTransform = tooltip.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0.5f, 0);

            // create inner border
            var innerBorder = new GameObject("InnerBorder", typeof(Image));
            innerBorder.transform.SetParent(tooltip.transform, false);

            innerBorder.GetComponent<Image>().color = new Color(0.125f, 0.125f, 0.125f, 1.0f);

            horizontalLayoutGroup = innerBorder.AddComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.padding = new RectOffset(20, 20, 10, 10);
            horizontalLayoutGroup.childControlWidth = true;
            horizontalLayoutGroup.childControlHeight = true;
            horizontalLayoutGroup.childForceExpandWidth = false;
            horizontalLayoutGroup.childForceExpandHeight = false;

            // create text
            var goText = new GameObject("Text", typeof(Text));
            goText.transform.SetParent(innerBorder.transform, false);

            var text = goText.GetComponent<Text>();
            text.font = DateFile.instance.font;
            text.fontSize = 16;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);

            tooltip.SetActive(false);
            return tooltip;
        }


        private List<float> PlaceXLabels(XLabelGroup xLabelGroup, float originX, float originY, float chartWidth, float chartHeight)
        {
            const float MARGIN_RATIO = 0.5f;    // 左右分别留出 0.5 个单位长度

            int nLabels = xLabelGroup.goLabels.Count;
            float unitWidth = chartWidth / (nLabels - 1 + MARGIN_RATIO * 2);

            List<float> xPositions = new List<float>();
            float beginPosition = originX + MARGIN_RATIO * unitWidth;
            for (int i = 0; i < nLabels; ++i)
            {
                float currPosition = beginPosition + i * unitWidth;
                xPositions.Add(currPosition);

                var goLabel = xLabelGroup.goLabels[i];
                if (goLabel == null) continue;

                var rectTransform = goLabel.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(currPosition, originY);
            }

            return xPositions;
        }


        private void PlaceYLabels(YLabelGroup yLabelGroup, float originX, float originY, float chartWidth, float chartHeight)
        {
            int nLabels = yLabelGroup.goLabels.Count;
            double range = yLabelGroup.maxValue - yLabelGroup.minValue;

            for (int i = 0; i < nLabels; ++i)
            {
                double yValue = yLabelGroup.labelValues[i];
                float position = (float)(originY + (yValue - yLabelGroup.minValue) / range * chartHeight);

                var goLabel = yLabelGroup.goLabels[i];
                var rectTransform = goLabel.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(originX, position);
            }
        }


        private GameObject DrawXAxis(float originX, float originY, float chartWidth, float chartHeight)
        {
            var xAxis = new GameObject("XAxis", typeof(Image));
            xAxis.transform.SetParent(this.chartContainer.transform, false);

            xAxis.GetComponent<Image>().color = PanelCharts.CHART_AXIS_COLOR;

            var rectTransform = xAxis.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(originX - 1, originY);
            rectTransform.sizeDelta = new Vector2(chartWidth + 1, 2);

            return xAxis;
        }


        private GameObject DrawYAxis(float originX, float originY, float chartWidth, float chartHeight)
        {
            var yAxis = new GameObject("YAxis", typeof(Image));
            yAxis.transform.SetParent(this.chartContainer.transform, false);

            yAxis.GetComponent<Image>().color = PanelCharts.CHART_AXIS_COLOR;

            var rectTransform = yAxis.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(originX, originY - 1);
            rectTransform.sizeDelta = new Vector2(2, chartHeight + 1);

            return yAxis;
        }


        private XLabelGroup CreateXLabelGroup(List<string> xLabels, List<bool> drawXLabels)
        {
            List<GameObject> goXLabels = new List<GameObject>();

            for (int i = 0; i < xLabels.Count; ++i)
            {
                if (drawXLabels[i])
                {
                    var goXLabel = this.CreateXLabel(xLabels[i]);
                    goXLabels.Add(goXLabel);
                }
                else
                    goXLabels.Add(null);
            }

            return new XLabelGroup
            {
                goLabels = goXLabels,
                labels = xLabels,
                maxLabelHeight = 0,
            };
        }


        private void UpdateXLabelsSizes(XLabelGroup xLabelGroup)
        {
            float maxXLabelHeight = 0;
            foreach (var goXLabel in xLabelGroup.goLabels)
            {
                if (goXLabel == null) continue;

                var goLabel = Common.GetChild(goXLabel, "Label");

                var rectTransform = goLabel.GetComponent<RectTransform>();
                float labelHeight = 5 + (rectTransform.rect.width + rectTransform.rect.height) * 0.707106f;
                maxXLabelHeight = Mathf.Max(labelHeight, maxXLabelHeight);

                goLabel.GetComponent<Text>().color = PanelCharts.CHART_LABEL_COLOR;

                var goCoordinate = Common.GetChild(goXLabel, "Coordinate");
                goCoordinate.GetComponent<Image>().color = PanelCharts.CHART_AXIS_COLOR;
            }
            xLabelGroup.maxLabelHeight = maxXLabelHeight;
        }


        private GameObject CreateXLabel(string label)
        {
            // create xLabel
            var goXLabel = new GameObject("XLabel", typeof(RectTransform));
            goXLabel.transform.SetParent(this.chartContainer.transform, false);

            var rectTransform = goXLabel.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(60, 50);
            rectTransform.sizeDelta = new Vector2(50, 20);

            // create label
            var goLabel = new GameObject("Label", typeof(Text), typeof(ContentSizeFitter));
            goLabel.transform.SetParent(goXLabel.transform, false);

            var text = goLabel.GetComponent<Text>();
            text.text = label;
            text.font = DateFile.instance.font;
            text.fontSize = 14;
            text.alignment = TextAnchor.MiddleRight;
            text.color = PanelCharts.WINDOW_BG_COLOR_TRANSPARENT;

            var contentSizeFitter = goLabel.GetComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            rectTransform = goLabel.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 1);
            rectTransform.anchoredPosition = new Vector2(-30, -5);
            rectTransform.Rotate(0, 0, 45);

            // create coordinate
            var goCoordinate = new GameObject("Coordinate", typeof(Image));
            goCoordinate.transform.SetParent(goXLabel.transform, false);

            goCoordinate.GetComponent<Image>().color = PanelCharts.WINDOW_BG_COLOR_TRANSPARENT;

            rectTransform = goCoordinate.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(0, 10);
            rectTransform.sizeDelta = new Vector2(1, 10);

            return goXLabel;
        }


        private YLabelGroup CreateYLabelGroup(List<double> yValues)
        {
            const float MARGIN_RATIO = 0.05f;   // 上下分别留出 0.05 的全高

            // 如果最小值大于等于 0，则以 0 为最小值
            double min = Math.Min(yValues.Min(), 0);
            double max = yValues.Max();

            if (min == max)
            {
                min -= 1;
                max += 1;
            }

            double range = max - min;
            min -= range * MARGIN_RATIO;
            max += range * MARGIN_RATIO;

            var yLabelValues = Common.FindIntegerLabels(min, max, PanelCharts.Y_LABEL_COUNT, out int nDecimalDigits);

            //
            List<GameObject> goYLabels = new List<GameObject>();
            foreach (double yLabelValue in yLabelValues)
            {
                string label = yLabelValue.ToString($"F{nDecimalDigits}");
                var goYLabel = this.CreateYLabel(label);
                goYLabels.Add(goYLabel);
            }

            return new YLabelGroup
            {
                goLabels = goYLabels,
                labelValues = yLabelValues,
                values = yValues,
                minValue = min,
                maxValue = max,
                maxLabelWidth = 0,
            };
        }


        private void UpdateYLabelsSizes(YLabelGroup yLabelGroup)
        {
            float maxYLabelWidth = 0;
            foreach (var goYLabel in yLabelGroup.goLabels)
            {
                var goLabel = Common.GetChild(goYLabel, "Label");

                var rectTransform = goLabel.GetComponent<RectTransform>();
                float labelWidth = rectTransform.rect.width + 10;
                maxYLabelWidth = Mathf.Max(labelWidth, maxYLabelWidth);

                goLabel.GetComponent<Text>().color = PanelCharts.CHART_LABEL_COLOR;

                var goCoordinate = Common.GetChild(goYLabel, "Coordinate");
                goCoordinate.GetComponent<Image>().color = PanelCharts.CHART_AXIS_COLOR;
            }
            yLabelGroup.maxLabelWidth = maxYLabelWidth;
        }


        private GameObject CreateYLabel(string label)
        {
            // create yLabel
            var goYLabel = new GameObject("YLabel", typeof(RectTransform));
            goYLabel.transform.SetParent(this.chartContainer.transform, false);

            var rectTransform = goYLabel.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(1f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(50, 60);
            rectTransform.sizeDelta = new Vector2(50, 20);

            // create label
            var goLabel = new GameObject("Label", typeof(Text), typeof(ContentSizeFitter));
            goLabel.transform.SetParent(goYLabel.transform, false);

            var text = goLabel.GetComponent<Text>();
            text.text = label;
            text.font = DateFile.instance.font;
            text.fontSize = 14;
            text.alignment = TextAnchor.MiddleRight;
            text.color = PanelCharts.WINDOW_BG_COLOR_TRANSPARENT;

            var contentSizeFitter = goLabel.GetComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            rectTransform = goLabel.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(-10, 0);

            // create coordinate
            var goCoordinate = new GameObject("Coordinate", typeof(Image));
            goCoordinate.transform.SetParent(goYLabel.transform, false);

            goCoordinate.GetComponent<Image>().color = PanelCharts.WINDOW_BG_COLOR_TRANSPARENT;

            rectTransform = goCoordinate.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1f, 0.5f);
            rectTransform.anchorMax = new Vector2(1f, 0.5f);
            rectTransform.pivot = new Vector2(0.0f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(0, 0);
            rectTransform.sizeDelta = new Vector2(10, 1);

            return goYLabel;
        }


        /// <summary>
        /// 计算待展示序列，以及对应坐标标签
        /// </summary>
        /// <param name="xLabels"></param>
        /// <param name="drawXLabels"></param>
        /// <returns></returns>
        private List<double> GetChartData(out List<string> xLabels, out List<bool> drawXLabels)
        {
            if (this.selectedMetricId < 0 || this.selectedMetricId >= PanelCharts.METRICS_SELECTORS.Count)
                throw new Exception("MetricId out of range: " + this.selectedMetricId);

            if (this.selectedGranularityId < 0 || this.selectedGranularityId >= PanelCharts.GRANULARITY_MONTHS.Length)
                throw new Exception("GranularityId out of range: " + this.selectedGranularityId);

            var metricSelector = PanelCharts.METRICS_SELECTORS[this.selectedMetricId];
            int nGranularityMonths = PanelCharts.GRANULARITY_MONTHS[this.selectedGranularityId];
            int nFetchingMonths = PanelCharts.MAX_X_COUNT * nGranularityMonths;

            var results = this.history.OrderByDescending(entry => entry.Key).Take(nFetchingMonths)
                .GroupBy(
                    entry => TaiwuDate.CreateWithMonth(entry.Key.year, entry.Key.GetMonthIndex() / nGranularityMonths * nGranularityMonths),
                    metricSelector,
                    (key, values) => new { key, count = values.Count(), value = values.Average() })
                .Where(data => data.count == nGranularityMonths)
                .OrderBy(data => data.key);

            var yValues = results.Select(data => data.value)
                .Select(value => double.IsInfinity(value) || double.IsNaN(value) ? 0 : value)
                .ToList();

            xLabels = new List<string>();
            drawXLabels = new List<bool>();
            var dates = results.Select(data => data.key).ToList();
            int labelSkipStep = dates.Count > PanelCharts.X_LABEL_COUNT ? dates.Count / PanelCharts.X_LABEL_COUNT : 1;
            for (int i = 0; i < dates.Count; ++i)
            {
                var date = dates[i];
                xLabels.Add(string.Format("{0:D}-{1:D2}", date.year, date.GetMonthIndex() + 1));
                drawXLabels.Add(i % labelSkipStep == 0);
            }

            return yValues;
        }


        public class XLabelGroup
        {
            public List<GameObject> goLabels;
            public List<string> labels;
            public float maxLabelHeight;
        }


        public class YLabelGroup
        {
            public List<GameObject> goLabels;
            public List<double> labelValues;
            public List<double> values;
            public double minValue;
            public double maxValue;
            public float maxLabelWidth;
        }


        public class ChartInfo
        {
            public float originX;
            public float originY;
            public float chartWidth;
            public float chartHeight;
            public GameObject xAxis;
            public GameObject yAxis;
            public List<GameObject> goXLabels;
            public List<string> xLabels;
            public List<float> xPositions;
            public List<GameObject> goYLabels;
            public List<double> yLabelValues;
            public List<double> yValues;
            public double yMinValue;
            public double yMaxValue;
            public GameObject tooltip;
        }


        /// <summary>
        /// 鼠标移入时展示 tooltip，移出时隐藏 tooltip
        /// </summary>
        public class ChartDotMouseEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
        {
            private static readonly Vector2 POSITION_OFFSET = new Vector2(0, 15);

            private GameObject tooltip;
            private Vector2 position;
            private string tooltipText;


            public void SetData(Vector2 position, GameObject tooltip, string tooltipText)
            {
                this.position = position;
                this.tooltip = tooltip;
                this.tooltipText = tooltipText;
            }


            public void OnPointerEnter(PointerEventData pointerEventData)
            {
                var rectTransform = this.tooltip.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = this.position + ChartDotMouseEventHandler.POSITION_OFFSET;

                var innerBorder = Common.GetChild(this.tooltip, "InnerBorder");
                var goText = Common.GetChild(innerBorder, "Text");
                var text = goText.GetComponent<Text>();
                text.text = this.tooltipText;

                this.tooltip.SetActive(true);
            }


            public void OnPointerExit(PointerEventData pointerEventData)
            {
                this.tooltip.SetActive(false);
            }
        }
    }
}
