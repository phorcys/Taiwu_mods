using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;


namespace NpcScan
{
    internal class UI : MonoBehaviour
    {
        /// <summary>启动窗口按键</summary>
        public static KeyCode key;

        public static UI Instance { get; private set; }

        #region 样式
        /// <summary>基准字体大小</summary>
        private const int baseFontSize = 12;
        /// <summary>基准字体大小的倒数，减少除法次数</summary>
        private const float reciprocalFontSize = (float)(1 / (double)baseFontSize);
        /// <summary>存放GUI.skin.verticalScrollBar.fixWidth, 上下滚动条基准宽度</summary>
        private float verticalScrollBarWidth;
        /// <summary>存放GUI.skin.horizonScrollBar.fixHeight, 左右滚动条基准高度</summary>
        private float horizonScollBarHeight;
        /// <summary>窗口控件样式</summary>
        public static GUIStyle window;
        /// <summary>Label控件样式</summary>
        private GUIStyle labelStyle;
        /// <summary>button控件样式</summary>
        private GUIStyle buttonStyle;
        /// <summary>toggle控件样式</summary>
        private GUIStyle toggleStyle;
        /// <summary>textfield控件样式</summary>
        private GUIStyle textStyle;
        /// <summary>左右滚动条样式</summary>
        private GUIStyle horizontalScrollbarStyle;
        /// <summary>上下滚动条样式</summary>
        private GUIStyle verticalScrollbarStyle;
        /// <summary>缩放比例</summary>
        private float ratio = 1f;
        /// <summary>窗口位置和大小</summary>
        private Rect mWindowRect = new Rect(0, 0, 0, 0);
        #endregion

        #region Constant
        /// <summary>求和时品阶的加权</summary>
        private static readonly long[] power = { (long)1E16, (long)1E14, (long)1E12, (long)1E10, (long)1E8, (long)1E6, (long)1E4, (long)1E2, 1 };
        #endregion

        #region 常用正则表达式
        /// <summary>用于匹配空白和颜色标签</summary>
        private static readonly Regex colorTagRegex = new Regex(@"(?>\s+|<color=#[0-9a-fA-F]{6,8}>|</color>)");
        #endregion


        #region 私有成员
        /// <summary>UI是否已经初始化</summary>
        private bool mInit = false;
        /// <summary>是否删除综合评分列</summary>
        private bool showlistshrinked;
        /// <summary>是否添加综合评分列</summary>
        private bool showlistadded = false;
        /// <summary>降序排列</summary>
        private bool desc = true;
        /// <summary>按哪一列排序</summary>
        private int sortIndex = -1;
        /// <summary>分页</summary>
        private int page = 1;
        /// <summary>综合评分列已添加</summary>
        private bool rankcolumnadded = false;
        /// <summary>立场选项</summary>
        private readonly bool[] goodness = new bool[] { true, false, false, false, false, false };
        /// <summary>立场</summary>
        private readonly string[] goodnessValue = new string[] { "全部", "刚正", "仁善", "中庸", "叛逆", "唯我" };
        /// <summary>每页显示条数</summary>
        private int countPerPage = 0;
        /// <summary>每页显示条数的倒数，减少浮点除法的运算次数</summary>
        private float reciprocalCountPerPage;
        /// <summary>性别选项双性</summary>
        private bool isall = true;
        /// <summary>性别选项男</summary>
        private bool isman = false;
        /// <summary>性别选项女</summary>
        private bool iswoman = false;
        /// <summary>搜索结果表头</summary>
        private static readonly List<Column> mColumns = new List<Column>
        {
            new Column {name = "姓名", width = 100},
            new Column {name = "年龄", width = 30},
            new Column {name = "性别", width = 30},
            new Column {name = "位置", width = 165},
            new Column {name = "魅力", width = 70},
            new Column {name = "从属", width = 60},//从属gangText
            new Column {name = "身份", width = 70},//身份gangLevelText
            new Column {name = "商会", width = 70},//商会
            new Column {name = "立场", width = 30},//立场goodnessText
            new Column {name = "婚姻", width = 30},//
            new Column {name = "技能成长", width = 70},
            new Column {name = "功法成长", width = 70},
            new Column {name = "健康", width = 60},
            new Column {name = "膂力", width = 30},
            new Column {name = "体质", width = 30},
            new Column {name = "灵敏", width = 30},
            new Column {name = "根骨", width = 30},
            new Column {name = "悟性", width = 30},
            new Column {name = "定力", width = 30},
            new Column {name = "内功", width = 30},
            new Column {name = "身法", width = 30},
            new Column {name = "绝技", width = 30},
            new Column {name = "拳掌", width = 30},
            new Column {name = "指法", width = 30},
            new Column {name = "腿法", width = 30},
            new Column {name = "暗器", width = 30},
            new Column {name = "剑法", width = 30},
            new Column {name = "刀法", width = 30},
            new Column {name = "长兵", width = 30},
            new Column {name = "奇门", width = 30},
            new Column {name = "软兵", width = 30},
            new Column {name = "御射", width = 30},
            new Column {name = "乐器", width = 30},
            new Column {name = "音律", width = 30},
            new Column {name = "弈棋", width = 30},
            new Column {name = "诗书", width = 30},
            new Column {name = "绘画", width = 30},
            new Column {name = "术数", width = 30},
            new Column {name = "品鉴", width = 30},
            new Column {name = "锻造", width = 30},
            new Column {name = "制木", width = 30},
            new Column {name = "医术", width = 30},
            new Column {name = "毒术", width = 30},
            new Column {name = "织锦", width = 30},
            new Column {name = "巧匠", width = 30},
            new Column {name = "道法", width = 30},
            new Column {name = "佛学", width = 30},
            new Column {name = "厨艺", width = 30},
            new Column {name = "杂学", width = 30},
            new Column {name = "银钱", width = 60},
            //七元
            new Column {name = "细腻", width = 30},
            new Column {name = "聪颖", width = 30},
            new Column {name = "水性", width = 30},
            new Column {name = "勇壮", width = 30},
            new Column {name = "坚毅", width = 30},
            new Column {name = "冷静", width = 30},
            new Column {name = "福缘", width = 30},
            new Column {name = "可学功法(不考虑好感度影响)", width = 210},
            new Column {name = "可学技艺(不考虑好感度影响)", width = 160},
            new Column {name = "前世", width = 120},
            new Column {name = "特性", width = 500}
        };
        /// <summary>搜索结果区域当前列宽</summary>
        private GUILayoutOption[] colWidth;
        /// <summary>NPC搜索结果滚动条, 0: 标题行； 1：结果区域</summary>
        private readonly Vector2[] scrollPosition = new Vector2[2];
        /// <summary>搜索结果</summary>
        private readonly List<string[]> actorList = new List<string[]>();
        #endregion

        #region 属性
        /// <summary>搜索最低年龄</summary>
        public int Minage { get; private set; } = 0;
        /// <summary>搜索最高年龄</summary>
        public int Maxage { get; private set; } = 0;
        /// <summary>膂力</summary>
        public int StrValue { get; private set; } = 0;
        /// <summary>体质</summary>
        public int ConValue { get; private set; } = 0;
        /// <summary>灵敏</summary>
        public int AgiValue { get; private set; } = 0;
        /// <summary>根骨</summary>
        public int BonValue { get; private set; } = 0;
        /// <summary>悟性</summary>
        public int IntValue { get; private set; } = 0;
        /// <summary>定力</summary>
        public int PatValue { get; private set; } = 0;
        /// <summary>搜索性别, 0双性, 1只搜男, 2只搜女</summary>
        public int GenderValue { get; private set; } = 0;
        /// <summary>魅力</summary>
        public int CharmValue { get; private set; } = 0;
        /// <summary>轮回次数</summary>
        public int SamsaraCount { get; private set; } = 0;
        /// <summary>健康</summary>
        public int HealthValue { get; private set; } = 1;
        /// <summary>是否只获取基础值</summary>
        public bool IsGetReal { get; private set; } = true;
        /// <summary>是否是排行模式</summary>
        public bool Rankmode { get; private set; } = false;
        /// <summary>从属gangText</summary>
        public string GangValue { get; private set; } = "";
        /// <summary>身份gangLevelText</summary>
        public string GangLevelValue { get; private set; } = "";
        /// <summary>立场</summary>
        public string GoodnessText { get; private set; } = "";
        /// <summary>商会</summary>
        public string AShopName { get; private set; } = "";
        /// <summary>
        /// 0:内功;1:身法;2:绝技;3:拳掌;4:指法;5:腿法;6:暗器;7:剑法;8:刀法;9:长兵;10:奇门;11:软兵;12:御射;13:乐器;
        /// </summary>
        public int[] Gongfa { get; private set; } = new int[14];
        /// <summary>
        /// 0:音律;1:弈棋;2:诗书;3:绘画;4:术数;5:品鉴;6:锻造;7:制木;8:医术;9:毒术;10:织锦;11:巧匠;12:道法;13:佛学;14:厨艺;15:杂学;
        /// </summary>
        public int[] Life { get; private set; } = new int[16];
        /// <summary>人物特性(多个特性用'|'分隔)</summary>
        public string ActorFeatureText { get; private set; } = "";
        /// <summary>是否精确查找,精确查找的情况下,特性用'|'分隔</summary>
        public bool TarFeature { get; private set; } = false;
        /// <summary>特性OR查询，默认为AND</summary>
        public bool TarFeatureOr { get; private set; } = false;
        /// <summary>可教功法(多个功法用'|'分隔)</summary>
        public string ActorGongFaText { get; private set; } = "";
        /// <summary>可教功法Or查询，默认AND</summary>
        public bool TarGongFaOr { get; private set; } = false;
        /// <summary>可学技艺(用技艺类别名的首字搜，用'|'分隔)</summary>
        public string ActorSkillText { get; private set; } = "";
        /// <summary>可教技艺Or查询，默认AND</summary>
        public bool TarSkillOr { get; private set; } = false;
        /// <summary>最高查询品级</summary>
        public int HighestLevel { get; private set; } = 1;
        /// <summary>是否开启门派识别</summary>
        public bool TarIsGang { get; private set; } = false;
        /// <summary>仅搜索门派</summary>
        public bool IsGang { get; private set; } = false;
        /// <summary>姓名(包括前世)</summary>
        public string AName { get; private set; } = "";

        /// <summary窗口已打开</summary>
        public bool Opened { get; private set; }
        #endregion

        private class Column
        {
            public string name;
            public float width;
            public bool expand = false;
        }

        internal static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                new GameObject(typeof(UI).FullName, typeof(UI));
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
            return false;
        }

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        private void Start() => CalculateWindowPos();

        private void Update()
        {
            if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl))
                && Input.GetKeyUp(Main.settings.key))
            {
                ToggleWindow();
            }
        }

        private void PrepareGUI()
        {
            window = new GUIStyle
            {
                name = "umm window",
                padding = RectOffset(5)
            };

            /*Texture2D pic = new Texture2D(200, 200);
            byte[] data = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAIAAAEACAYAAACZCaebAAAAnElEQVRIS63MtQHDQADAwPdEZmaG/fdJCq2g7qqLvu/7hRBCZOF9X0ILz/MQWrjvm1DHdV3MFs7zJLRwHAehhX3fCS1s20ZoYV1XQgvLshDqmOeZ2cI0TYQWxnEktDAMA6GFvu8JLXRdR2ihbVtCHU3TMFuo65rQQlVVhBbKsiS0UBQFoYU8zwktZFlGqCNNU2YLSZIQWojjmFDCH22GtZAncD8TAAAAAElFTkSuQmCC");
            pic.LoadImage(data);
            window.normal.background = pic;
            window.normal.background.wrapMode = TextureWrapMode.Repeat;*/

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                name = "label",
                fontSize = (int)(baseFontSize * ratio)
            };
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                name = "button",
                fontSize = (int)(baseFontSize * ratio)
            };
            toggleStyle = new GUIStyle(GUI.skin.toggle)
            {
                name = "toggle",
                fontSize = (int)(baseFontSize * ratio)
            };
            textStyle = new GUIStyle(GUI.skin.textField)
            {
                name = "text",
                fontSize = (int)(baseFontSize * ratio)
            };

            verticalScrollBarWidth = GUI.skin.verticalScrollbar.fixedWidth;
            horizonScollBarHeight = GUI.skin.horizontalScrollbar.fixedHeight;
            verticalScrollbarStyle = new GUIStyle(GUI.skin.verticalScrollbar)
            {
                fixedWidth = verticalScrollBarWidth * ratio
            };
            horizontalScrollbarStyle = new GUIStyle(GUI.skin.horizontalScrollbar)
            {
                fixedHeight = horizonScollBarHeight * ratio
            };
        }

        private void OnGUI()
        {
            if (!mInit)
            {
                mInit = true;
                PrepareGUI();
            }

            if (Opened)
            {
                Color backgroundColor = GUI.backgroundColor;
                Color color = GUI.color;
                GUI.backgroundColor = Color.white;
                GUI.color = Color.white;
                mWindowRect = GUILayout.Window(952, mWindowRect, WindowFunction, "", window, GUILayout.Height(Screen.height - 200));
                GUI.backgroundColor = backgroundColor;
                GUI.color = color;
            }
        }

        private void CalculateWindowPos()
        {
            mWindowRect = new Rect(ScreenWidth() * 0.05f, 40f, WindowWidth() + 60f, 0);
            // 按照1600*(WindowWidth() + 60f)/(1600*0.8+60)的乘法形式
            ratio = (float)Math.Max(1, (WindowWidth() + 60f) * 7.462687e-4);
            CalculateColWidth();
        }

        /// <summary>
        ///  搜索条件窗口
        /// </summary>
        /// <param name="windowId">没用</param>
        private void WindowFunction(int windowId)
        {
            GUILayout.BeginVertical("box");

            SetNo1Windows();
            SetNo2Windows();
            SetNo3Windows();
            SetNo4Windows();
            SetNo5Windows();
            SetNo6Windows();
            SetNo7Windows();

            if (actorList.Count > 0)
            {

                if (Rankmode && !rankcolumnadded && showlistadded)
                {
                    mColumns.Insert(0, new Column { name = "综合评分", width = 80 });
                    OptimizeScrollView.ResetAllView();
                    rankcolumnadded = true;
                    CalculateColWidth();
                }
                if (!Rankmode && rankcolumnadded && showlistshrinked)
                {
                    mColumns.RemoveAt(0);
                    OptimizeScrollView.ResetAllView();
                    rankcolumnadded = false;
                    CalculateColWidth();
                }

                SetSearchResultTitle();
                SetSearchResultContent();
            }
            GUILayout.EndVertical();
            GUILayout.Space(3);
            // 窗口拖动，必须放到WindowFunction的最后
            if (Input.GetKey(KeyCode.LeftControl))
            {
                GUI.DragWindow();
            }
        }

        /// <summary>
        /// (设置第一行内容)
        /// </summary>
        private void SetNo1Windows()
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("NPC查找器(按住 左Ctrl 可以用鼠标拖动窗口)", labelStyle);
            if (GUILayout.Button("关闭", buttonStyle, GUILayout.Width(150 * ratio)))
            {
                ToggleWindow();
            }
            GUILayout.EndHorizontal();
        }
        /// <summary>
        /// 设置第二行内容
        /// </summary>
        private void SetNo2Windows()
        {
            float currentwidth = 0;
            GUILayout.BeginHorizontal("box");
            #region add 年龄 性别
            GUILayout.Label("年龄:", labelStyle, GUILayout.Width(30 * ratio));
            if (int.TryParse(GUILayout.TextField(Minage.ToString(), 3, textStyle, GUILayout.Width(30 * ratio)), out int value))
            {
                Minage = value;
            }
            GUILayout.Label("--", labelStyle, GUILayout.Width(10 * ratio));
            if (int.TryParse(GUILayout.TextField(Maxage.ToString(), 3, textStyle, GUILayout.Width(30 * ratio)), out value))
            {
                Maxage = value;
            }
            GUILayout.Space(10 * ratio);
            currentwidth += 110 * ratio;
            currentwidth = AddHorizontal(currentwidth, (30 + 5 + 45 + 30 + 30 + 10) * ratio);
            GUILayout.Label("性别:", labelStyle, GUILayout.Width(30 * ratio));
            GUILayout.Space(5 * ratio);
            isall = GUILayout.Toggle(isall, "全部", toggleStyle, GUILayout.Width(45 * ratio));
            if (isall)
            {
                iswoman = false;
                isman = false;
                GenderValue = 0;
            }

            isman = GUILayout.Toggle(isman, "男", toggleStyle, GUILayout.Width(30 * ratio));
            if (isman)
            {
                isall = false;
                iswoman = false;
                GenderValue = 1;
            }
            iswoman = GUILayout.Toggle(iswoman, "女", toggleStyle, GUILayout.Width(30 * ratio));
            if (iswoman)
            {
                isall = false;
                isman = false;
                GenderValue = 2;
            }
            GUILayout.Space(10 * ratio);
            #endregion

            #region add 基础属性
            currentwidth = AddLabelAndTextField("膂力", currentwidth, StrValue, out value);
            StrValue = value;
            currentwidth = AddLabelAndTextField("体质", currentwidth, ConValue, out value);
            ConValue = value;
            currentwidth = AddLabelAndTextField("灵敏", currentwidth, AgiValue, out value);
            AgiValue = value;
            currentwidth = AddLabelAndTextField("根骨", currentwidth, BonValue, out value);
            BonValue = value;
            currentwidth = AddLabelAndTextField("悟性", currentwidth, IntValue, out value);
            IntValue = value;
            currentwidth = AddLabelAndTextField("定力", currentwidth, PatValue, out value);
            PatValue = value;
            currentwidth = AddLabelAndTextField("魅力", currentwidth, CharmValue, out value);
            CharmValue = value;
            currentwidth = AddLabelAndTextField("健康", currentwidth, HealthValue, out value);
            HealthValue = value;
            currentwidth = AddLabelAndTextField("轮回次数", currentwidth, (85 + 10) * ratio, 55 * ratio, 30 * ratio, SamsaraCount, out value);
            SamsaraCount = value;
            #endregion
            GUILayout.Space(10 * ratio);
            #region add 翻页
            AddHorizontal(currentwidth, (80 + 60 + 60) * ratio);
            GUILayout.Label($"页码:{page}/{Mathf.Max((int)Math.Ceiling(actorList.Count * reciprocalCountPerPage), 1)}", labelStyle, GUILayout.Width(80 * ratio));
            if (GUILayout.Button("上页", buttonStyle, GUILayout.Width(60 * ratio)))
            {
                if (page > 1)
                {
                    OptimizeScrollView.GetInstance("Content").ResetView();
                    page--;
                }
            }
            if (GUILayout.Button("下页", buttonStyle, GUILayout.Width(60 * ratio)))
            {
                if (actorList.Count > page * countPerPage)
                {
                    OptimizeScrollView.GetInstance("Content").ResetView();
                    page++;
                }
            }
            #endregion
            GUILayout.EndHorizontal();
        }
        /// <summary>
        /// 设置第三行内容
        /// </summary>
        private void SetNo3Windows()
        {
            float currentwidth = 0;
            GUILayout.BeginHorizontal("box");

            #region add 功法属性
            currentwidth = AddLabelAndTextField("内功", currentwidth, Gongfa[0], out Gongfa[0]);
            currentwidth = AddLabelAndTextField("身法", currentwidth, Gongfa[1], out Gongfa[1]);
            currentwidth = AddLabelAndTextField("绝技", currentwidth, Gongfa[2], out Gongfa[2]);
            currentwidth = AddLabelAndTextField("拳掌", currentwidth, Gongfa[3], out Gongfa[3]);
            currentwidth = AddLabelAndTextField("指法", currentwidth, Gongfa[4], out Gongfa[4]);
            currentwidth = AddLabelAndTextField("腿法", currentwidth, Gongfa[5], out Gongfa[5]);
            currentwidth = AddLabelAndTextField("暗器", currentwidth, Gongfa[6], out Gongfa[6]);
            currentwidth = AddLabelAndTextField("剑法", currentwidth, Gongfa[7], out Gongfa[7]);
            currentwidth = AddLabelAndTextField("刀法", currentwidth, Gongfa[8], out Gongfa[8]);
            currentwidth = AddLabelAndTextField("长兵", currentwidth, Gongfa[9], out Gongfa[9]);
            currentwidth = AddLabelAndTextField("奇门", currentwidth, Gongfa[10], out Gongfa[10]);
            currentwidth = AddLabelAndTextField("软兵", currentwidth, Gongfa[11], out Gongfa[11]);
            currentwidth = AddLabelAndTextField("御射", currentwidth, Gongfa[12], out Gongfa[12]);
            AddLabelAndTextField("乐器", currentwidth, Gongfa[13], out Gongfa[13]);
            #endregion

            GUILayout.Label("取值:", labelStyle, GUILayout.Width(30 * ratio));
            GUILayout.Space(5 * ratio);
            IsGetReal = GUILayout.Toggle(IsGetReal, "基础值", toggleStyle, GUILayout.Width(55 * ratio));
            GUILayout.Space(5 * ratio);
            Rankmode = GUILayout.Toggle(Rankmode, "排行模式", toggleStyle, GUILayout.Width(65 * ratio));
            GUILayout.EndHorizontal();
        }
        /// <summary>
        /// 设置第四行内容
        /// </summary>
        private void SetNo4Windows()
        {
            float currentwidth = 0;
            GUILayout.BeginHorizontal("box");

            #region add 技艺属性
            currentwidth = AddLabelAndTextField("音律", currentwidth, Life[0], out Life[0]);
            currentwidth = AddLabelAndTextField("弈棋", currentwidth, Life[1], out Life[1]);
            currentwidth = AddLabelAndTextField("诗书", currentwidth, Life[2], out Life[2]);
            currentwidth = AddLabelAndTextField("绘画", currentwidth, Life[3], out Life[3]);
            currentwidth = AddLabelAndTextField("术数", currentwidth, Life[4], out Life[4]);
            currentwidth = AddLabelAndTextField("品鉴", currentwidth, Life[5], out Life[5]);
            currentwidth = AddLabelAndTextField("锻造", currentwidth, Life[6], out Life[6]);
            currentwidth = AddLabelAndTextField("制木", currentwidth, Life[7], out Life[7]);
            currentwidth = AddLabelAndTextField("医术", currentwidth, Life[8], out Life[8]);
            currentwidth = AddLabelAndTextField("毒术", currentwidth, Life[9], out Life[9]);
            currentwidth = AddLabelAndTextField("织锦", currentwidth, Life[10], out Life[10]);
            currentwidth = AddLabelAndTextField("巧匠", currentwidth, Life[11], out Life[11]);
            currentwidth = AddLabelAndTextField("道法", currentwidth, Life[12], out Life[12]);
            currentwidth = AddLabelAndTextField("佛学", currentwidth, Life[13], out Life[13]);
            currentwidth = AddLabelAndTextField("厨艺", currentwidth, Life[14], out Life[14]);
            AddLabelAndTextField("杂学", currentwidth, Life[15], out Life[15]);
            #endregion

            GUILayout.EndHorizontal();
        }
        /// <summary>
        /// 设置第五行内容
        /// </summary>
        private void SetNo5Windows()
        {
            float currentwidth = 0;
            GUILayout.BeginHorizontal("box");

            currentwidth = AddLabelAndTextField("姓名（包括前世）", currentwidth, 180 * ratio, 100 * ratio, 80 * ratio, AName, out string text);
            AName = text;
            //从属gangText
            currentwidth = AddLabelAndTextField("从属", currentwidth, 90 * ratio, 30 * ratio, 60 * ratio, GangValue, out text);
            GangValue = text;
            //身份gangLevelText
            currentwidth = AddLabelAndTextField("身份", currentwidth, 90 * ratio, 30 * ratio, 60 * ratio, GangLevelValue, out text);
            GangLevelValue = text;
            //商会
            currentwidth = AddLabelAndTextField("商会", currentwidth, 90 * ratio, 30 * ratio, 60 * ratio, AShopName, out text);
            AShopName = text;
            currentwidth = AddHorizontal(currentwidth, (30 + 45 * 6) * ratio);
            //立场goodnessText
            GUILayout.Label("立场:", labelStyle, GUILayout.Width(30 * ratio));
            for (int i = 0; i < goodness.Length; i++)
            {
                goodness[i] = GUILayout.Toggle(goodness[i], goodnessValue[i].ToString(), toggleStyle, GUILayout.Width(45 * ratio));
                if (goodness[i])
                {
                    for (int j = 0; j < goodness.Length; j++)
                    {
                        if (i != j)
                        {
                            goodness[j] = false;
                            GoodnessText = goodnessValue[i].ToString();
                        }
                    }
                }
            }
            GUILayout.Space(30 * ratio);
            currentwidth = AddLabelAndTextField("最高查询品级", currentwidth, 170 * ratio, 80 * ratio, 60 * ratio, HighestLevel, out int value);
            HighestLevel = value;
            GUILayout.Space(10 * ratio);
            AddHorizontal(currentwidth, 250 * ratio);
            TarIsGang = GUILayout.Toggle(TarIsGang, "是否开启识别门派", toggleStyle, GUILayout.Width(120 * ratio));
            IsGang = GUILayout.Toggle(IsGang, "仅搜索门派", toggleStyle);
            GUILayout.EndHorizontal();
        }

        private void SetNo6Windows()
        {
            float currentwidth = 0;
            GUILayout.BeginHorizontal("box");
            currentwidth = AddLabelAndTextField("人物特性(多个特性用'|'分隔)", currentwidth, (280 + 60 + 100) * ratio, 160 * ratio, 120 * ratio, ActorFeatureText, out string text);
            ActorFeatureText = text;
            TarFeature = GUILayout.Toggle(TarFeature, "精确特性", toggleStyle, GUILayout.Width(65 * ratio));//是否精确查找,精确查找的情况下,特性用'|'分隔
            TarFeatureOr = GUILayout.Toggle(TarFeatureOr, "OR查询(否则默认AND)", toggleStyle, GUILayout.Width(150 * ratio));//默认AND查询方式
            currentwidth = AddLabelAndTextField("可教功法(多个功法用'|'分隔)", currentwidth, 310 * ratio, 160 * ratio, 150 * ratio, ActorGongFaText, out text);
            ActorGongFaText = text;
            AddHorizontal(currentwidth, 180 * ratio);
            TarGongFaOr = GUILayout.Toggle(TarGongFaOr, "OR查询(否则默认AND)", toggleStyle, GUILayout.Width(150 * ratio));//默认AND查询方式
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 设置第七行内容
        /// </summary>
        private void SetNo7Windows()
        {
            float currentwidth = 0;
            GUILayout.BeginHorizontal("box");
            currentwidth = AddLabelAndTextField("可学技艺(用技艺类别名的第一个字搜，多个技艺用'|'分隔)", currentwidth, 410 * ratio, 310 * ratio, 100 * ratio, ActorSkillText, out string text);
            ActorSkillText = text;
            TarSkillOr = GUILayout.Toggle(TarSkillOr, "OR查询(否则默认AND)", toggleStyle, GUILayout.Width(150 * ratio));//默认AND查询方式

            GUILayout.FlexibleSpace();
            AddHorizontal(currentwidth, 150 * ratio);
            if (GUILayout.Button("查找", buttonStyle, GUILayout.Width(150 * ratio)))
            {
                // 每次点击查找。获取当前每页显示页数。
                countPerPage = Main.settings.countPerPage;
                reciprocalCountPerPage = (float)(1 / (double)countPerPage);

                page = 1;
                // 首次搜索时，载入特性信息
                if (Main.featuresList.Count == 0)
                {
                    foreach (int key in DateFile.instance.actorFeaturesDate.Keys)
                    {
                        var feature = new Features(key);
                        Main.featuresList.Add(feature.Key, feature);
                        Main.fNameList.Add(feature.Name, feature.Key);
                    }
                }
                if (ActorFeatureText != "")
                {
                    GetFeatures(ActorFeatureText, TarFeature);
                }

                if (Main.gNameList.Count == 0)
                {
                    foreach (var pair in DateFile.instance.gongFaDate)
                    {
                        // 去掉空白
                        var tem = Regex.Replace(pair.Value[0], @"\s", "");
                        var index = tem.IndexOf('<');
                        // 去掉颜色格式
                        if (index > -1)
                            tem = colorTagRegex.Replace(tem, "", -1, index);
                        if (tem != "")
                            Main.gNameList.Add(tem, pair.Key);
                    }
                }

                if (ActorGongFaText != "")
                {
                    GetGongFaKey(ActorGongFaText);
                }

                if (Main.bNameList.Count == 0)
                {
                    foreach (var pair in DateFile.instance.baseSkillDate)
                    {
                        var tem = pair.Value[0].Substring(0, 1);
                        Main.bNameList.Add(tem, pair.Key);
                    }
                }

                if (ActorSkillText != "")
                {
                    GetSkillKey(ActorSkillText);
                }

                ScanNpc();

                if (!Rankmode)
                {
                    showlistshrinked = true;
                    showlistadded = false;
                }
                else
                {
                    showlistadded = true;
                    showlistshrinked = false;
                }
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 设置搜索结果标题栏
        /// </summary>
        // 优化：只渲染当前窗口能够显示出来的列
        private void SetSearchResultTitle()
        {
            // 用于冻结标题行，且列会跟着搜索结果区域的左右滚动条动
            scrollPosition[0] = GUILayout.BeginScrollView(scrollPosition[0], new GUIStyle(), new GUIStyle(),
                                                          // 以12pt字体为基准，对应 16 pixels, 两行文字则加倍32pixel, 若字体乘以ratio转成int后并没有实际增大则不增加宽度
                                                          // 先碱法后“除法”(虽然转换成乘法了)，减少浮点型运算积累误差
                                                          GUILayout.Height(70 + ((int)(ratio * baseFontSize) - baseFontSize) * reciprocalFontSize * 32),
                                                          GUILayout.Width(WindowWidth() + 60f));
            var optInstance1 = OptimizeScrollView.GetInstance("TitleName");
            var optInstance2 = OptimizeScrollView.GetInstance("TitleBtn");
            // 第一次执行时记录各列的位置和大小信息
            if (!optInstance1.IsReady(OptimizeScrollView.OptType.ColOnly) || !optInstance2.IsReady(OptimizeScrollView.OptType.ColOnly))
            {
                GUILayout.BeginHorizontal("box");
                for (int i = 0; i < mColumns.Count; i++)
                {
                    GUILayout.Label(mColumns[i].name, labelStyle, colWidth[i]);
                    if (Event.current.type == EventType.Repaint)
                    {
                        // 将各列位置信息记录
                        optInstance1.AddColRect(i, GUILayoutUtility.GetLastRect(), mColumns.Count);
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("box");
                for (int i = 0; i < mColumns.Count; i++)
                {
                    if (GUILayout.Button("O", buttonStyle, colWidth[i]))
                    {
                        OnClickColBtn(i);
                    }
                    if (Event.current.type == EventType.Repaint)
                    {
                        // 将各列位置信息记录
                        optInstance2.AddColRect(i, GUILayoutUtility.GetLastRect(), mColumns.Count);
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.EndScrollView();
                if (Event.current.type == EventType.Repaint)
                {
                    // 记录可显示区域的位置和大小
                    optInstance1.AddViewRect(GUILayoutUtility.GetLastRect());
                    optInstance2.AddViewRect(GUILayoutUtility.GetLastRect());
                }
            }
            else
            {
                // 已经获取各列的位置和大小信息后, 只渲染能在当前窗口里显示的列
                optInstance1.GetFirstAndLastColVisible(scrollPosition[0], out int firstColVisible, out int lastColVisible);
                GUILayout.BeginHorizontal("box");
                if (firstColVisible > 0)
                {
                    // 用空白填充不能显示的区域
                    GUILayout.Space(optInstance1.GetColEndPosition(firstColVisible - 1));
                }
                // 只渲染可以显示的内容
                for (int i = Math.Max(firstColVisible, 0); i <= lastColVisible; i++)
                {
                    GUILayout.Label(mColumns[i].name, labelStyle, colWidth[i]);
                }
                if (lastColVisible != -1)
                {
                    if (lastColVisible < optInstance1.ColCount - 1)
                    {
                        // 用空白填充不能显示的区域
                        GUILayout.Space(optInstance1.GetRowEndPosition(optInstance1.ColCount - 1) - optInstance1.GetRowEndPosition(lastColVisible));
                    }
                    else
                    {
                        // 多填充两个上下滚动条的宽度，防止搜索结果显示区域出现上下滚动条时，左右滚动条滚到最右时，标题栏宽度不够而溢出报错
                        GUILayout.Space(1.5f * GUI.skin.verticalScrollbar.fixedWidth * ratio);
                    }
                }

                GUILayout.EndHorizontal();
                optInstance2.GetFirstAndLastColVisible(scrollPosition[0], out firstColVisible, out lastColVisible);
                GUILayout.BeginHorizontal("box");
                if (firstColVisible > 0)
                {
                    // 用空白填充不能显示的区域
                    GUILayout.Space(optInstance2.GetColEndPosition(firstColVisible - 1));
                }
                // 只渲染可以显示的内容
                for (int i = Math.Max(firstColVisible, 0); i <= lastColVisible; i++)
                {
                    if (GUILayout.Button("O", buttonStyle, colWidth[i]))
                    {
                        OnClickColBtn(i);
                    }
                }
                if (lastColVisible != -1)
                {
                    if (lastColVisible < optInstance2.ColCount - 1)
                    {
                        // 用空白填充不能显示的区域
                        GUILayout.Space(optInstance2.GetRowEndPosition(optInstance2.ColCount - 1) - optInstance2.GetRowEndPosition(lastColVisible));
                    }
                    else
                    {
                        // 多填充一个上下滚动条的宽度，防止搜索结果显示区域出现上下滚动条时，左右滚动条滚到最右时，标题栏宽度不够而溢出报错
                        GUILayout.Space(1.5f * GUI.skin.verticalScrollbar.fixedWidth * ratio);
                    }
                }

                GUILayout.EndHorizontal();
                GUILayout.EndScrollView();
            }
        }

        /// <summary>
        /// 搜索结果展示
        /// </summary>
        private void SetSearchResultContent()
        {
            // 分页: 当前页最后一行内容序号
            int c = actorList.Count > countPerPage * page ? countPerPage * page : actorList.Count;
            float verticalThumbWidth = GUI.skin.verticalScrollbarThumb.fixedWidth;
            float horizonThumbHeight = GUI.skin.horizontalScrollbarThumb.fixedHeight;
            GUI.skin.verticalScrollbarThumb.fixedWidth = verticalThumbWidth * ratio;
            GUI.skin.horizontalScrollbarThumb.fixedHeight = horizonThumbHeight * ratio;
            scrollPosition[1] = GUILayout.BeginScrollView(scrollPosition[1], horizontalScrollbarStyle, verticalScrollbarStyle, GUILayout.ExpandHeight(false), GUILayout.Width(WindowWidth() + 60f));
            scrollPosition[0].x = scrollPosition[1].x; // 标题栏列随着结果的列一起移动
            scrollPosition[0].y = 0; // 冻结标题行
            var optInstance3 = OptimizeScrollView.GetInstance("Content");
            // 第一次执行时记录各列和各行的位置和大小信息
            if (!optInstance3.IsReady())
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical("box");
                for (int i = (page - 1) * countPerPage; i < c; i++)
                {
                    GUILayout.BeginHorizontal("box");
                    for (int j = 0; j < actorList[i].Length; j++)
                    {
                        GUILayout.Label(actorList[i][j], labelStyle, colWidth[j]);
                        if (Event.current.type == EventType.Repaint)
                        {
                            // 记录各列位置和大小
                            optInstance3.AddColRect(j, GUILayoutUtility.GetLastRect(), mColumns.Count);
                        }
                    }
                    GUILayout.EndHorizontal();
                    if (Event.current.type == EventType.Repaint)
                    {
                        // 记录各行位置和大小
                        optInstance3.AddRowRect(i - (page - 1) * countPerPage, GUILayoutUtility.GetLastRect());
                    }
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.EndScrollView();
                if (Event.current.type == EventType.Repaint)
                {
                    // 记录可显示区域的位置和大小
                    optInstance3.AddViewRect(GUILayoutUtility.GetLastRect());
                }
            }
            else
            {
                // 已经获取各列和各行的位置和大小信息后, 只渲染能在当前窗口里显示的列和行
                optInstance3.GetFirstAndLastColVisible(scrollPosition[1], out int firstColVisible, out int lastColVisible);
                optInstance3.GetFirstAndLastRowVisible(scrollPosition[1], out int firstRowVisible, out int lastRowVisible);
                GUILayout.BeginHorizontal();
                if (firstColVisible > 0)
                {
                    // 用空白填充不能显示的区域
                    GUILayout.Space(optInstance3.GetColEndPosition(firstColVisible - 1));
                }
                GUILayout.BeginVertical("box");
                if (firstRowVisible > 0)
                {
                    // 用空白填充不能显示的区域
                    GUILayout.Space(optInstance3.GetRowEndPosition(firstRowVisible - 1));
                }
                int beginRow = Math.Max((page - 1) * countPerPage + firstRowVisible, 0);
                int endRow = (page - 1) * countPerPage + lastRowVisible + 1;
                // 只渲染可以显示的行
                for (int i = beginRow; i < endRow; i++)
                {
                    GUILayout.BeginHorizontal("box", GUILayout.Height(optInstance3.GetRowHeight(i - (page - 1) * countPerPage)));
                    // 只渲染可以显示的列
                    for (int j = Math.Max(firstColVisible, 0); j <= lastColVisible; j++)
                    {
                        GUILayout.Label(actorList[i][j], labelStyle, colWidth[j]);
                    }
                    GUILayout.EndHorizontal();
                }
                int lastRow = (page - 1) * countPerPage + optInstance3.RowCount;
                if (endRow < lastRow)
                {
                    // 用空白填充不能显示的区域
                    GUILayout.Space(optInstance3.GetRowEndPosition(optInstance3.RowCount - 1) - optInstance3.GetRowEndPosition(lastRowVisible));
                }
                GUILayout.EndVertical();
                if (lastColVisible != -1 && lastColVisible < optInstance3.ColCount - 1)
                {
                    // 用空白填充不能显示的区域
                    GUILayout.Space(optInstance3.GetColEndPosition(optInstance3.ColCount - 1) - optInstance3.GetColEndPosition(lastColVisible));
                }
                GUILayout.EndHorizontal();
                GUILayout.EndScrollView();
            }
            GUI.skin.verticalScrollbarThumb.fixedWidth = verticalThumbWidth;
            GUI.skin.horizontalScrollbarThumb.fixedHeight = horizonThumbHeight;
        }


        /// <summary>
        /// 同时添加一个laber和一个textbox
        /// </summary>
        /// <param name="labelName">laber名字</param>
        /// <param name="currentwidth">当前行宽度</param>
        /// <param name="settingInput">textbox的当前值</param>
        /// <param name="settingValue">textbox的设定值</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float AddLabelAndTextField(string labelName, float currentwidth, int settingInput, out int settingValue)
        {
            currentwidth = AddHorizontal(currentwidth, 65 * ratio);
            GUILayout.Label($"{labelName}:", labelStyle, GUILayout.Width(30 * ratio));
            if (!int.TryParse(GUILayout.TextField(settingInput.ToString(), 10, textStyle, GUILayout.Width(30 * ratio)), out settingValue))
            {
                settingValue = 0;
            }
            GUILayout.Space(5 * ratio);
            return currentwidth;
        }

        /// <summary>
        /// 同时添加一个laber和一个textbox
        /// </summary>
        /// <param name="labelName">laber名字</param>
        /// <param name="currentwidth">当前行宽度</param>
        /// <param name="settingInput">textbox的当前内容</param>
        /// <param name="addwidth">添加的宽度</param>
        /// <param name="outString">textbox的输入内容</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float AddLabelAndTextField(string labelName, float currentwidth, float addwidth, float labelWidth, float textWidth, string settingInput, out string outString)
        {
            currentwidth = AddHorizontal(currentwidth, addwidth);
            GUILayout.Label($"{labelName}:", labelStyle, GUILayout.Width(labelWidth));
            outString = GUILayout.TextField(settingInput, textStyle, GUILayout.Width(textWidth));
            return currentwidth;
        }

        /// <summary>
        /// 同时添加一个laber和一个textbox
        /// </summary>
        /// <param name="labelName">laber名字</param>
        /// <param name="currentwidth">当前行宽度</param>
        /// <param name="settingInput">textbox的当前内容</param>
        /// <param name="addwidth">添加的宽度</param>
        /// <param name="outValue">textbox的输入内容</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float AddLabelAndTextField(string labelName, float currentwidth, float addwidth, float labelWidth, float textWidth, int settingInput, out int outValue)
        {

            currentwidth = AddHorizontal(currentwidth, addwidth);
            GUILayout.Label($"{labelName}:", labelStyle, GUILayout.Width(labelWidth));
            string outString = GUILayout.TextField(settingInput.ToString(), 10, textStyle, GUILayout.Width(textWidth));
            int.TryParse(outString, out outValue);
            return currentwidth;
        }

        /// <summary>
        /// 是否换行判断
        /// </summary>
        /// <param name="currentwidth">当前宽</param>
        /// <param name="addWidth">添加宽</param>
        /// <returns>当前宽</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float AddHorizontal(float currentwidth, float addWidth)
        {
            currentwidth += addWidth;
            if (currentwidth >= WindowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = addWidth;
                GUILayout.BeginHorizontal("box");
            }
            return currentwidth;
        }

#if debug
        private readonly Stopwatch stopwatch = new Stopwatch();
#endif

        /// <summary>
        /// 搜索NPC
        /// </summary>
        private void ScanNpc()
        {
#if debug
            stopwatch.Restart();
#endif
            OptimizeScrollView.GetInstance("Content").ResetView();
            actorList.Clear();
            // 重置上一次排序的列为空，这样下次排序时对待所有列都是默认首先降序
            sortIndex = -1;
            Main.isScaningNpc = true;
            foreach (var npcId in DateFile.instance.actorsDate.Keys)
            {
                var addItem = new ActorItem(npcId, this);
                if (addItem.isNeedAdd)
                {
                    actorList.Add(addItem.GetAddItem());
                }
            }
            Main.isScaningNpc = false;
#if debug
            stopwatch.Stop();
            Main.Logger.Log($"ScanNpc: {stopwatch.Elapsed}");
#endif
        }

        /// <summary>
        /// 点击类标题的按钮时触发
        /// </summary>
        /// <param name="colNum">列序号</param>
        private void OnClickColBtn(int colNum)
        {
#if debug
            stopwatch.Restart();
#endif
            // 新的一列作为排序依据时总是默认降序
            if (sortIndex != colNum)
            {
                desc = true;
            }
            sortIndex = colNum;
            actorList.Sort(SortList);
            // 排序完重置到第一页
            if (page != 1)
            {
                page = 1;
            }
            // 顺序打乱后需要重新获取个列和行的位置信息
            OptimizeScrollView.GetInstance("Content").ResetView();
            // 上下滚动条复位
            scrollPosition[1].y = 0;
            desc = !desc;
#if debug
            stopwatch.Stop();
            Main.Logger.Log($"Sort: {stopwatch.Elapsed}");
#endif
        }

        /// <summary>
        /// 获得想要搜索的特性
        /// </summary>
        /// <param name="str">特性搜索条件</param>
        /// <param name="TarFeaure">是否精确查找</param>
        /// <returns></returns>
        private static void GetFeatures(string str, bool TarFeaure)
        {
            Main.findSet.Clear();
            var features = str.Split('|');
            foreach (string s in features)
            {
                if (Main.fNameList.TryGetValue(s, out int key))
                {
                    if (!TarFeaure)
                    {
                        Features f = Main.featuresList[key];
                        int plus = f.Plus;
                        if (plus == 3 || plus == 4)
                        {
                            Main.findSet.Add(f.Group);
                            continue;
                        }
                    }
                    Main.findSet.Add(key);
                }
            }
        }

        /// <summary>
        /// 获得想要搜索的功法
        /// </summary>
        /// <param name="str">搜索词条</param>
        private static void GetGongFaKey(string str)
        {
            Main.gongFaList.Clear();
            var gongFaTemp = str.Split('|');
            foreach (string s in gongFaTemp)
            {
                if (Main.gNameList.TryGetValue(s, out int key))
                {
                    Main.gongFaList.Add(key);
                }
            }
        }

        /// <summary>
        /// 获得想要搜索的技艺
        /// </summary>
        /// <param name="str">搜索词条</param>
        private static void GetSkillKey(string str)
        {
            Main.skillList.Clear();
            var skillTemp = str.Split('|');
            foreach (var s in skillTemp)
            {
                if (Main.bNameList.TryGetValue(s, out int key))
                {
                    Main.skillList.Add(key);
                }
            }
        }

        /// <summary>
        /// 关闭打开窗口
        /// </summary>
        public void ToggleWindow() => ToggleWindow(!Opened);

        /// <summary>
        /// 关闭打开窗口
        /// </summary>
        /// <param name="open">是否打开</param>
        public void ToggleWindow(bool open)
        {
            Opened = open;
            BlockGameUI(open);
            if (open)
            {
                // 自适应游戏窗口变化
                CalculateWindowPos();
                // 调整字体
                labelStyle.fontSize = (int)(baseFontSize * ratio);
                buttonStyle.fontSize = (int)(baseFontSize * ratio);
                textStyle.fontSize = (int)(baseFontSize * ratio);
                toggleStyle.fontSize = (int)(baseFontSize * ratio);
                // 调整滚动条大小
                verticalScrollbarStyle.fixedWidth = verticalScrollBarWidth * ratio;
                horizontalScrollbarStyle.fixedHeight = horizonScollBarHeight * ratio;
                OptimizeScrollView.ResetAllView();
            }
        }

        /// <summary>
        /// 遮蔽游戏界面
        /// </summary>
        private GameObject mCanvas;

        /// <summary>
        /// 遮蔽游戏界面
        /// </summary>
        private void BlockGameUI(bool value)
        {
            if (value)
            {
                mCanvas = new GameObject("", typeof(Canvas), typeof(GraphicRaycaster));
                mCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                mCanvas.GetComponent<Canvas>().sortingOrder = short.MaxValue;
                DontDestroyOnLoad(mCanvas);
                var panel = new GameObject("", typeof(Image));
                panel.transform.SetParent(mCanvas.transform);
                panel.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0);
                panel.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                panel.GetComponent<RectTransform>().offsetMin = Vector2.zero;
                panel.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            }
            else
            {
                Destroy(mCanvas);
            }
        }

        /// <summary>
        /// 调整margin
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static RectOffset RectOffset(int value) => new RectOffset(value, value, value, value);

        /// <summary>
        /// 调整margin
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static RectOffset RectOffset(int x, int y) => new RectOffset(x, x, y, y);

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int SortList(string[] a, string[] b)
        {
            int m, n;
            string s1 = a[sortIndex];
            string s2 = b[sortIndex];

            int index = Rankmode ? sortIndex - 1 : sortIndex;
            // 分情况讨论，主要是为了使用更专门的Regex，提升排序性能
            switch (index)
            {
                case 4:
                    // 魅力只保留数值部分
                    s1 = Regex.Match(s1, @"^(?>\d{1,3})").Value;
                    s2 = Regex.Match(s2, @"^(?>\d{1,3})").Value;
                    int.TryParse(s1, out m);
                    int.TryParse(s2, out n);
                    return CheckMN(m, n, desc);
                case 6:
                    // 身份按照颜色排序模式
                    m = Main.colorText[Regex.Match(s1, @"<color=#[0-9a-fA-F]{8}>").Groups[0].Value];
                    n = Main.colorText[Regex.Match(s2, @"<color=#[0-9a-fA-F]{8}>").Groups[0].Value];
                    return CheckMN(m, n, desc);
                case 12:
                    // 健康，先比最大健康，若一样再比当前健康
                    s1 = colorTagRegex.Replace(s1, "");
                    s2 = colorTagRegex.Replace(s2, "");
                    var a1 = s1.Split('/');
                    var a2 = s2.Split('/');
                    if (a1.Length != 2 && a1.Length != a2.Length)
                        return 0;
                    for (int i = 1; i >= 0; i--)
                    {
                        int.TryParse(a1[i], out m);
                        int.TryParse(a2[i], out n);

                        if (m != 0 || n != 0)
                        {
                            var result = CheckMN(m, n, desc);
                            if (result != 0)
                                return result;
                        }
                    }
                    return 0;
                case 57:
                    // 可学功法，从一品到九品数量加权求和转换成一个正整数比较大小
                    MatchCollection matches;
                    long ml = 0, nl = 0;
                    if (s1 != "")
                    {
                        matches = Regex.Matches(s1, @"(?<=<color=#[0-9a-fA-F]{8}>)\d{2}(?=</color>)");
                        for (int i = 0; i < matches.Count; i++)
                        {
                            var tmp = matches[i].Value;
                            if (tmp != "00")
                                ml += int.Parse(tmp) * power[i];

                        }
                    }
                    if (s2 != "")
                    {
                        matches = Regex.Matches(s2, @"(?<=<color=#[0-9a-fA-F]{8}>)\d{2}(?=</color>)");
                        for (int i = 0; i < matches.Count; i++)
                        {
                            var tmp = matches[i].Value;
                            if (tmp != "00")
                                nl += int.Parse(tmp) * power[i];
                        }
                    }
                    return CheckMN(ml, nl, desc);
                case 58:
                    // 可学技艺，将各可学技艺的品阶做(9-品阶)<<(9-品阶)运算后求和，排序
                    m = n = 0;
                    if (s1 != "")
                    {
                        matches = Regex.Matches(s1, @"\d(?=</color>)");
                        for (int i = 0; i < matches.Count; i++)
                        {
                            var tmp = 10 - int.Parse(matches[i].Value);
                            m += tmp << tmp;
                        }
                    }
                    if (s2 != "")
                    {
                        matches = Regex.Matches(s2, @"\d(?=</color>)");
                        for (int i = 0; i < matches.Count; i++)
                        {
                            var tmp = 10 - int.Parse(matches[i].Value);
                            n += tmp << tmp;
                        }
                    }
                    return CheckMN(m, n, desc);
                case 60:
                    // 特性直接字符串比较
                    s1 = colorTagRegex.Replace(s1, "");
                    s2 = colorTagRegex.Replace(s2, "");
                    return desc ? -s1.CompareTo(s2) : s1.CompareTo(s2);
                default:
                    // 取其中数字排序
                    // 若其中数字相等 或非数字情况下 字符串排序
                    s1 = colorTagRegex.Replace(s1, "");
                    s2 = colorTagRegex.Replace(s2, "");
                    int.TryParse(s1, out m);
                    int.TryParse(s2, out n);
                    if (m != 0 || n != 0)
                        return CheckMN(m, n, desc);
                    else
                        return desc ? -s1.CompareTo(s2) : s1.CompareTo(s2);
            }
        }

        /// <summary>
        /// 比较大小
        /// </summary>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <param name="desc">是否降序</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CheckMN(long m, long n, bool desc)
        {
            int rtn = desc ? -1 : 1;
            if (m > n)
                return rtn;
            else if (m < n)
                return -1 * rtn;
            else
                return 0;
        }

        /// <summary>
        /// 计算每列宽度
        /// </summary>
        private void CalculateColWidth()
        {
            float amountWidth = 0;
            float expandWidth = 0;

            foreach (Column mCol in mColumns)
            {
                amountWidth += mCol.width * ratio;
                if (mCol.expand)
                    expandWidth += mCol.width * ratio;
            }

            float reciprocalexpandWidth = (float)(1 / (double)expandWidth);
            colWidth = new GUILayoutOption[mColumns.Count];
            for (int i = 0; i < colWidth.Length; i++)
            {
                if (mColumns[i].expand)
                    colWidth[i] = GUILayout.Width(mColumns[i].width * ratio * reciprocalexpandWidth * (WindowWidth() - 60 + expandWidth - amountWidth));
                else
                    colWidth[i] = GUILayout.Width(mColumns[i].width * ratio);
            }
        }

        /// <summary>
        /// 窗口宽度
        /// </summary>
        /// <returns></returns>
        private float WindowWidth() => (DateFile.instance.screenWidth * 0.8f);

        /// <summary>
        /// 屏幕宽度
        /// </summary>
        /// <returns></returns>
        private float ScreenWidth() => (DateFile.instance.screenWidth);
    }
}
