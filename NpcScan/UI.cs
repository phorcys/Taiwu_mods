using GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


namespace NpcScan
{
    internal class UI : MonoBehaviour
    {
        public static UI Instance { get; private set; }

        #region 样式
        /// <summary>基准字体大小</summary>
        private const int baseFontSize = 12;
        /// <summary>基准字体大小的倒数，减少除法次数</summary>
        private const float reciprocalFontSize = (float)(1 / (double)baseFontSize);
        /// <summary>基准屏宽(pixel)倒数</summary>
        /// <remarks>使用倒数将<see cref="CalculateColWidth"/>中除法变成乘法</remarks>
        private const double reciproBaseScreenWidth = 1 / (1600d * 0.8 + 60d);
        /// <summary>基准屏高(pixel)倒数</summary>
        /// <remarks>使用倒数将<see cref="CalculateColWidth"/>中除法变成乘法</remarks>
        private const double reciproBaseScrrenHeight = 1 / 900d;
        /// <summary>存放GUI.skin.verticalScrollBar.fixWidth, 上下滚动条基准宽度</summary>
        private float verticalScrollBarWidth;
        /// <summary>存放GUI.skin.horizonScrollBar.fixHeight, 左右滚动条基准高度</summary>
        private float horizonScollBarHeight;
        /// <summary>窗口控件样式</summary>
        private GUIStyle window;
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
        /// <summary>空样式</summary>
        private GUIStyle emptyStyle;
        /// <summary>屏幕宽度缩放比例</summary>
        private float widthRatio = 1f;
        /// <summary>屏幕高度缩放比例</summary>
        private float heightRatio = 1f;
        /// <summary>窗口位置和大小</summary>
        private Rect mWindowRect;
        #endregion

        #region 常量
        /// <summary>立场类型</summary>
        private static readonly string[] goodnessName = new string[] { "全部", "中庸", "仁善", "刚正", "叛逆", "唯我" };
        /// <summary>性别选项文字</summary>
        private static readonly string[] genderChoiceText = new[] { "全部", "男", "女", "男生女相", "女生男相" };
        /// <summary>性别选项宽度</summary>
        private static readonly float[] genderChoiceWidth = new float[] { 45, 30, 30, 65, 65 };
        /// <summary>婚姻状况选项文字</summary>
        private static readonly string[] marriageChoiceText = new[] { "全部", "未婚", "已婚", "丧偶", "可说媒" };
        /// <summary>婚姻状况选项宽度</summary>
        private static readonly float[] marriageChoiceWidth = new float[] { 40, 40, 40, 40, 51 };
        /// <summary>功法类型文字</summary>
        private static readonly string[] gongFaTypText = new[] { "内功", "身法", "绝技", "拳掌", "指法", "腿法", "暗器", "剑法", "刀法", "长兵", "奇门", "软兵", "御射", "乐器" };
        /// <summary>技能类型文字</summary>
        private static readonly string[] skillTypText = new[] { "音律", "弈棋", "诗书", "绘画", "术数", "品鉴", "锻造", "制木", "医术", "毒术", "织锦", "巧匠", "道法", "佛学", "厨艺", "杂学" };
        /// <summary>求和时品阶的加权</summary>
        private static readonly long[] power = { (long)1E16, (long)1E14, (long)1E12, (long)1E10, (long)1E8, (long)1E6, (long)1E4, (long)1E2, 1 };
        /// <summary>搜索结果表头</summary>
        private static readonly List<Column> mColumns = new List<Column>
        {
            new Column {name = "姓名", width = 105},
            new Column {name = "年龄", width = 30},
            new Column {name = "性别", width = 30},
            new Column {name = "位置", width = 165},
            new Column {name = "魅力", width = 70},
            new Column {name = "从属", width = 60},
            new Column {name = "身份", width = 70},
            new Column {name = "商会", width = 70},
            new Column {name = "立场", width = 30},
            new Column {name = "婚姻", width = 30},
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
            new Column {name = "细腻", width = 30},
            new Column {name = "聪颖", width = 30},
            new Column {name = "水性", width = 30},
            new Column {name = "勇壮", width = 30},
            new Column {name = "坚毅", width = 30},
            new Column {name = "冷静", width = 30},
            new Column {name = "福缘", width = 30},
            new Column {name = "可学功法(不考虑好感度影响)", width = 210},
            new Column {name = "可学技艺(不考虑好感度影响)", width = 160},
            new Column {name = "最佳物品和装备", width = 300},
            new Column {name = "前世", width = 120},
            new Column {name = "特性", width = 500}
        };
        #endregion

        #region 常用正则表达式
        /// <summary>用于匹配空白和颜色标签</summary>
        public static readonly Regex colorTagRegex = new Regex(@"(?>\s+|<color=#[0-9a-fA-F]{6,8}>|</color>)");
        #endregion

        #region 私有成员
        /// <summary>UI是否已经初始化</summary>
        private bool mInit = false;
        /// <summary>窗口是否已打开</summary>
        private bool opened = false;
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
        /// <summary>分页</summary>
        private int Page
        {
            get => page;
            set
            {
                OptimizeScrollView.GetInstance("Content").ResetView();
                page = value;
            }
        }
        /// <summary>综合评分列已添加</summary>
        private bool rankcolumnadded = false;
        /// <summary>立场选项:全部 中庸 仁善 刚正 叛逆 唯我" </summary>
        private readonly bool[] goodness = new bool[] { true, false, false, false, false, false };
        /// <summary>婚姻状态选项:全部 未婚 已婚 丧偶 可说媒" </summary>
        private readonly bool[] marriage = new bool[] { true, false, false, false, false };
        /// <summary>每页显示条数</summary>
        private int countPerPage = 0;
        /// <summary>每页显示条数的倒数，减少浮点除法的运算次数</summary>
        private float reciprocalCountPerPage;
        /// <summary>性别选项：0 双性 1 男 2 女 3 男生女相 4 女生男相</summary>
        private readonly bool[] genderChoice = new[] { true, false, false, false, false };
        /// <summary>特性搜索条件</summary>
        internal readonly HashSet<int> featureSearchSet = new HashSet<int>();
        /// <summary>功法搜索条件</summary>
        internal readonly List<int> gongFaSearchList = new List<int>();
        /// <summary>技艺搜索条件</summary>
        internal readonly List<int> skillSearchList = new List<int>();
        /// <summary>物品搜索条件</summary>
        internal readonly HashSet<int> itemSearchSet = new HashSet<int>();
        /// <summary>物品搜索条件(相同名字的物品基础ID取反)</summary>
        internal readonly List<int> itemSearchList = new List<int>();
        /// <summary>搜索结果区域当前列宽</summary>
        private GUILayoutOption[] colWidth;
        /// <summary>NPC搜索结果滚动条, 0: 标题行； 1：结果区域</summary>
        private readonly Vector2[] scrollPosition = new Vector2[2];
        /// <summary>鼠标指向NPC详情时提示NPC姓名</summary>
        private bool showNameTip = true;
        /// <summary>鼠标指向引用此GUIContent的GUI控件时显示提示标签</summary>
        private GUIContent tipContent;
        /// <summary>提示标签的位置</summary>
        private Rect tipRect;
        /// <summary>搜索结果</summary>
        private readonly List<ActorItem> actorList = new List<ActorItem>();
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
        public string GangLevelText { get; private set; } = "";
        /// <summary>立场</summary>
        public int Goodness { get; private set; } = -1;
        /// <summary>婚姻状况</summary>
        public int Marriage { get; private set; } = 0;
        /// <summary>商会</summary>
        public string AShopName { get; private set; } = "";
        /// <summary>
        /// 0:内功;1:身法;2:绝技;3:拳掌;4:指法;5:腿法;6:暗器;7:剑法;8:刀法;9:长兵;10:奇门;11:软兵;12:御射;13:乐器;
        /// </summary>
        public int[] Gongfa { get; private set; } = new int[gongFaTypText.Length];
        /// <summary>
        /// 0:音律;1:弈棋;2:诗书;3:绘画;4:术数;5:品鉴;6:锻造;7:制木;8:医术;9:毒术;10:织锦;11:巧匠;12:道法;13:佛学;14:厨艺;15:杂学;
        /// </summary>
        public int[] Skill { get; private set; } = new int[skillTypText.Length];
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
        /// <summary>物品搜索</summary>
        public string ActorItemText { get; private set; } = "";
        /// <summary>物品Or查询，默认AND</summary>
        public bool TarItemOr { get; private set; }
        /// <summary>搜索过世之人的物品</summary>
        public bool ItemDead { get; private set; }
        /// <summary>最高查询品级</summary>
        public int HighestLevel { get; private set; } = 1;
        /// <summary>是否开启门派识别</summary>
        public bool TarIsGang { get; private set; } = false;
        /// <summary>仅搜索门派</summary>
        public bool IsGang { get; private set; } = false;
        /// <summary>姓名(包括前世)</summary>
        public string AName { get; private set; } = "";
        #endregion

        private class Column
        {
            public string name;
            public float width;
            public bool expand = false;
        }

        internal static bool Load()
        {
            try
            {
                if (Instance == null)
                {
                    new GameObject(typeof(UI).FullName, typeof(UI));
                    return true;
                }
            }
            catch (Exception e)
            {
                Main.Logger.Log("UI: \n" + e.ToString());
                var inner = e.InnerException;
                while (inner != null)
                {
                    Main.Logger.Log(inner.ToString());
                    inner = inner.InnerException;
                }
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
            if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyUp(KeyCode.F12)
                || Input.GetKeyUp(Main.settings.keys[0])
                // 当UI文字输入框激活时，只能用如下方式获取键盘事件
                || opened // 在窗口打开时要侦听另外一类键盘事件，避免输入框激活时无法关闭窗口
                && ((Event.current.type == EventType.KeyUp && Event.current.keyCode == Main.settings.keys[0])
                    || (Event.current.type == EventType.KeyUp && Event.current.control && Event.current.keyCode == KeyCode.F12)))
            {
                ToggleWindow();
            }

            if (opened)
            {
                // 执行热键                
                // 查找NPC
                if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter)
                    // 当UI文字输入框激活时，只能用如下方式获取键盘事件
                    || (Event.current.type == EventType.KeyUp
                        && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)))
                {
                    OnClickSearchBtn();
                }
                // 向上翻页
                if (Input.GetKeyUp(Main.settings.keys[1])
                    // 当UI文字输入框激活时，只能用如下方式获取键盘事件
                    || (Event.current.type == EventType.KeyUp && Event.current.keyCode == Main.settings.keys[1]))
                {
                    if (Page > 1)
                        Page--;
                }
                // 向下翻页
                if (Input.GetKeyUp(Main.settings.keys[2])
                    // 当UI文字输入框激活时，只能用如下方式获取键盘事件
                    || (Event.current.type == EventType.KeyUp && Event.current.keyCode == Main.settings.keys[2]))
                {
                    if (actorList.Count > Page * countPerPage)
                        Page++;
                }
            }
        }

        /// <summary>
        /// 初始化UI
        /// </summary>
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
                fontSize = (int)(baseFontSize * widthRatio),
            };
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                name = "button",
                fontSize = (int)(baseFontSize * widthRatio)
            };
            toggleStyle = new GUIStyle(GUI.skin.toggle)
            {
                name = "toggle",
                fontSize = (int)(baseFontSize * widthRatio)
            };
            textStyle = new GUIStyle(GUI.skin.textField)
            {
                name = "text",
                fontSize = (int)(baseFontSize * widthRatio)
            };
            verticalScrollBarWidth = GUI.skin.verticalScrollbar.fixedWidth;
            horizonScollBarHeight = GUI.skin.horizontalScrollbar.fixedHeight;
            verticalScrollbarStyle = new GUIStyle(GUI.skin.verticalScrollbar)
            {
                fixedWidth = verticalScrollBarWidth * widthRatio
            };
            horizontalScrollbarStyle = new GUIStyle(GUI.skin.horizontalScrollbar)
            {
                fixedHeight = horizonScollBarHeight * Math.Max(Screen.height / 900, 1)
            };
            emptyStyle = new GUIStyle();

            tipContent = new GUIContent("", GUI.tooltip);
        }

        /// <summary>
        /// 渲染UI
        /// </summary>
        private void OnGUI()
        {
            if (!mInit)
            {
                mInit = true;
                PrepareGUI();
            }

            if (opened)
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

        /// <summary>
        /// 调整窗口规格
        /// </summary>
        private void CalculateWindowPos()
        {
            mWindowRect = new Rect(ScreenWidth() * 0.05f, 40f, WindowWidth() + 60f, 0);
            // 按照1600*(WindowWidth() + 60f)/(1600*0.8+60)的乘法形式
            widthRatio = (float)((WindowWidth() + 60f) * reciproBaseScreenWidth);
            heightRatio = (float)(Screen.height * reciproBaseScrrenHeight);
            CalculateColWidth();
        }

        /// <summary>
        ///  搜索条件窗口
        /// </summary>
        /// <param name="windowId">没用</param>
        private void WindowFunction(int windowId)
        {
            GUILayout.BeginVertical("box");
            var labelColor = labelStyle.normal.textColor;
            var toggleColor = toggleStyle.normal.textColor;
            labelStyle.normal.textColor = new Color32(200, 255, 255, 255);
            toggleStyle.normal.textColor = new Color32(255, 226, 198, 255);
            SetNo1Windows();
            SetNo2Windows();
            SetNo3Windows();
            SetNo4Windows();
            SetNo5Windows();
            SetNo6Windows();
            SetNo7Windows();
            labelStyle.normal.textColor = labelColor;
            toggleStyle.normal.textColor = toggleColor;

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
        /// 设置第一行内容
        /// </summary>
        private void SetNo1Windows()
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("NPC查找器(按住 左Ctrl 可以用鼠标拖动窗口)", labelStyle);
            GUILayout.Label($"当前热键(Ctrl+F10打开UMM修改)： {Main.textColor[20009]}回车</color>---开始查找 " +
                $"{Main.textColor[20009]}{Main.settings.keys[0]}</color>---打开/关闭窗口 " +
                $"{Main.textColor[20009]}{Main.settings.keys[1]}</color>---向上翻页 " +
                $"{Main.textColor[20009]}{Main.settings.keys[2]}</color>---向下翻页");
            if (GUILayout.Button("关闭", buttonStyle, GUILayout.Width(150 * widthRatio)))
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
            // 记录当前已被控件占用的长途，用于换行判断
            float currentwidth = 0;
            GUILayout.BeginHorizontal("box");
            #region add 年龄 性别
            GUILayout.Label("年龄:", labelStyle, GUILayout.Width(30 * widthRatio));
            int.TryParse(GUILayout.TextField(Minage.ToString(), 3, textStyle, GUILayout.Width(30 * widthRatio)), out int value);
            Minage = value;
            GUILayout.Label("--", labelStyle, GUILayout.Width(10 * widthRatio));
            int.TryParse(GUILayout.TextField(Maxage.ToString(), 3, textStyle, GUILayout.Width(30 * widthRatio)), out value);
            Maxage = value;
            GUILayout.Space(10 * widthRatio);
            // 记录年龄控件的宽度
            currentwidth += (30 + 30 + 10 + 30 + 10) * widthRatio;
            AddHorizontal(ref currentwidth, (30 + 5 + genderChoiceWidth.Sum() + 10) * widthRatio);
            // 性别
            GUILayout.Label("性别:", labelStyle, GUILayout.Width(30 * widthRatio));
            GUILayout.Space(5 * widthRatio);
            GenderValue = MultiChoices(genderChoice, genderChoiceText, GenderValue, genderChoiceWidth);
            GUILayout.Space(10 * widthRatio);
            #endregion

            #region add 基础属性
            StrValue = AddLabelAndTextField("膂力", ref currentwidth, StrValue);
            ConValue = AddLabelAndTextField("体质", ref currentwidth, ConValue);
            AgiValue = AddLabelAndTextField("灵敏", ref currentwidth, AgiValue);
            BonValue = AddLabelAndTextField("根骨", ref currentwidth, BonValue);
            IntValue = AddLabelAndTextField("悟性", ref currentwidth, IntValue);
            PatValue = AddLabelAndTextField("定力", ref currentwidth, PatValue);
            CharmValue = AddLabelAndTextField("魅力", ref currentwidth, CharmValue);
            HealthValue = AddLabelAndTextField("健康", ref currentwidth, HealthValue);
            SamsaraCount = AddLabelAndTextField("轮回次数", ref currentwidth, (55 + 30 + 10) * widthRatio, 55 * widthRatio, 30 * widthRatio, SamsaraCount);
            #endregion
            GUILayout.Space(10 * widthRatio);
            #region add 翻页
            AddHorizontal(ref currentwidth, (80 + 60 + 60) * widthRatio);
            GUILayout.Label($"页码:{Page}/{Mathf.Max((int)Math.Ceiling(actorList.Count * reciprocalCountPerPage), 1)}", labelStyle, GUILayout.Width(80 * widthRatio));
            if (GUILayout.Button("上页", buttonStyle, GUILayout.Width(60 * widthRatio)))
            {
                if (Page > 1)
                    Page--;
            }
            if (GUILayout.Button("下页", buttonStyle, GUILayout.Width(60 * widthRatio)))
            {
                if (actorList.Count > Page * countPerPage)
                    Page++;
            }
            #endregion
            GUILayout.EndHorizontal();
        }
        /// <summary>
        /// 设置第三行内容
        /// </summary>
        private void SetNo3Windows()
        {
            // 记录当前已被控件占用的长途，用于换行判断
            float currentwidth = 0;
            GUILayout.BeginHorizontal("box");
            // 功法属性
            for (int i = 0; i < gongFaTypText.Length; i++)
            {
                Gongfa[i] = AddLabelAndTextField(gongFaTypText[i], ref currentwidth, Gongfa[i]);
            }
            GUILayout.Label("取值:", labelStyle, GUILayout.Width(30 * widthRatio));
            GUILayout.Space(5 * widthRatio);
            IsGetReal = GUILayout.Toggle(IsGetReal, "基础值", toggleStyle, GUILayout.Width(55 * widthRatio));
            GUILayout.Space(5 * widthRatio);
            Rankmode = GUILayout.Toggle(Rankmode, "排行模式", toggleStyle, GUILayout.Width(65 * widthRatio));
            GUILayout.Space(5 * widthRatio);
            HighestLevel = AddLabelAndTextField("最高查询品级", ref currentwidth, (80 + 30 + 5) * widthRatio, 80 * widthRatio, 30 * widthRatio, HighestLevel);
            GUILayout.EndHorizontal();
        }
        /// <summary>
        /// 设置第四行内容
        /// </summary>
        private void SetNo4Windows()
        {
            // 记录当前已被控件占用的长途，用于换行判断
            float currentwidth = 0;
            GUILayout.BeginHorizontal("box");
            #region add 技艺属性
            for (int i = 0; i < skillTypText.Length; i++)
            {
                Skill[i] = AddLabelAndTextField(skillTypText[i], ref currentwidth, Skill[i]);
            }
            #endregion
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("重置所有条件", buttonStyle, GUILayout.Width(150 * widthRatio)))
            {
                OnClickResetBtn();
            }
            GUILayout.EndHorizontal();
        }
        /// <summary>
        /// 设置第五行内容
        /// </summary>
        private void SetNo5Windows()
        {
            // 记录当前已被控件占用的长途，用于换行判断
            float currentwidth = 0;
            GUILayout.BeginHorizontal("box");
            AName = AddLabelAndTextField("姓名（包括前世）", ref currentwidth, 180 * widthRatio, 100 * widthRatio, 80 * widthRatio, AName);
            //从属gangText
            GangValue = AddLabelAndTextField("从属", ref currentwidth, 90 * widthRatio, 30 * widthRatio, 60 * widthRatio, GangValue);
            //身份gangLevelText
            GangLevelText = AddLabelAndTextField("身份", ref currentwidth, 90 * widthRatio, 30 * widthRatio, 60 * widthRatio, GangLevelText);
            //商会
            AShopName = AddLabelAndTextField("商会", ref currentwidth, 90 * widthRatio, 30 * widthRatio, 60 * widthRatio, AShopName);
            AddHorizontal(ref currentwidth, (100) * widthRatio);
            TarIsGang = GUILayout.Toggle(TarIsGang, "开启识别门派", toggleStyle, GUILayout.Width(100 * widthRatio));
            AddHorizontal(ref currentwidth, (65 + 20) * widthRatio);
            IsGang = GUILayout.Toggle(IsGang, "仅搜门派", toggleStyle, GUILayout.Width(65 * widthRatio));
            GUILayout.Space(20);
            AddHorizontal(ref currentwidth, (30 + 40 * goodness.Length + 20) * widthRatio);
            //立场goodnessText
            GUILayout.Label("立场:", labelStyle, GUILayout.Width(30 * widthRatio));
            Goodness = MultiChoices(goodness, goodnessName, Goodness + 1, 40) - 1;
            GUILayout.Space(20);
            AddHorizontal(ref currentwidth, (30 + marriageChoiceWidth.Sum() + 20) * widthRatio);
            // 婚姻
            GUILayout.Label("婚姻:", labelStyle, GUILayout.Width(30 * widthRatio));
            Marriage = MultiChoices(marriage, marriageChoiceText, Marriage, marriageChoiceWidth);
            //GUILayout.Space(20 * widthRatio);            
            GUILayout.EndHorizontal();
        }
        /// <summary>
        /// 设置第六行内容
        /// </summary>
        private void SetNo6Windows()
        {
            // 记录当前已被控件占用的长途，用于换行判断
            float currentwidth = 0;
            GUILayout.BeginHorizontal("box");
            ActorFeatureText = AddLabelAndTextField("人物特性(多个用'|'分隔)", ref currentwidth, (130 + 120 + 65 + 145) * widthRatio, 130 * widthRatio, 120 * widthRatio, ActorFeatureText);
            TarFeature = GUILayout.Toggle(TarFeature, "精确特性", toggleStyle, GUILayout.Width(65 * widthRatio));//是否精确查找,精确查找的情况下,特性用'|'分隔
            TarFeatureOr = GUILayout.Toggle(TarFeatureOr, "OR查询(否则默认AND)", toggleStyle, GUILayout.Width(145 * widthRatio));//默认AND查询方式
            ActorGongFaText = AddLabelAndTextField("可教功法(多个功法用'|'分隔)", ref currentwidth, (160 + 150 + 145) * widthRatio, 150 * widthRatio, 150 * widthRatio, ActorGongFaText);
            TarGongFaOr = GUILayout.Toggle(TarGongFaOr, "OR查询(否则默认AND)", toggleStyle, GUILayout.Width(145 * widthRatio));//默认AND查询方式
            AddHorizontal(ref currentwidth, 175 * widthRatio);
            showNameTip = GUILayout.Toggle(showNameTip, "搜索结果区域提示NPC姓名", toggleStyle, GUILayout.Width(175 * widthRatio));
            AddHorizontal(ref currentwidth, 175 * widthRatio);
            GUILayout.Label($"{Main.textColor[20009]}点击任意NPC信息打开NPC窗口</color>", labelStyle, GUILayout.Width(175 * widthRatio));
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 设置第七行内容
        /// </summary>
        private void SetNo7Windows()
        {
            // 记录当前已被控件占用的长途，用于换行判断
            float currentwidth = 0;
            GUILayout.BeginHorizontal("box");
            ActorSkillText = AddLabelAndTextField("可学技艺(多个用'|'分隔)", ref currentwidth, (130 + 120 + 150) * widthRatio, 130 * widthRatio, 120 * widthRatio, ActorSkillText);
            TarSkillOr = GUILayout.Toggle(TarSkillOr, "OR查询(否则默认AND)", toggleStyle, GUILayout.Width(150 * widthRatio));//默认AND查询方式
            ActorItemText = AddLabelAndTextField("物品(多个用'|'分隔,去掉书名号,不支持促织)", ref currentwidth, (230 + 100 + 150 + 150) * widthRatio, 230 * widthRatio, 150 * widthRatio, ActorItemText);
            TarItemOr = GUILayout.Toggle(TarItemOr, "OR查询(否则默认AND)", toggleStyle, GUILayout.Width(150 * widthRatio));//默认AND查询方式
            ItemDead = GUILayout.Toggle(ItemDead, "搜索过世之人的物品", toggleStyle, GUILayout.Width(150 * widthRatio));//默认AND查询方式
            GUILayout.FlexibleSpace();
            AddHorizontal(ref currentwidth, 150 * widthRatio);
            if (GUILayout.Button("查找(或回车键)", buttonStyle, GUILayout.Width(150 * widthRatio)))
            {
                // 查找NPC
                OnClickSearchBtn();
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
            scrollPosition[0] = GUILayout.BeginScrollView(scrollPosition[0], emptyStyle, emptyStyle,
                                                          // 以12pt字体为基准，对应 16 pixels, 两行文字则加倍32pixel, 若字体乘以ratio转成int后并没有实际增大则不增加宽度
                                                          // 先碱法后“除法”(虽然转换成乘法了)，减少浮点型运算积累误差
                                                          GUILayout.Height(70 + ((int)(widthRatio * baseFontSize) - baseFontSize) * reciprocalFontSize * 32));
            // 获取滚动视图优化类实例
            var optInstance1 = OptimizeScrollView.GetInstance("TitleName", OptimizeScrollView.OptType.ColOnly);
            var optInstance2 = OptimizeScrollView.GetInstance("TitleBtn", OptimizeScrollView.OptType.ColOnly);
            // 首次渲染时记录各列的位置和大小信息
            if (!optInstance1.IsReady() || !optInstance2.IsReady())
            {
                GUILayout.BeginHorizontal("box");
                // 渲染标题中名称
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
                // 渲染标题中按钮
                for (int i = 0; i < mColumns.Count; i++)
                {
                    if (GUILayout.Button("O", buttonStyle, colWidth[i]))
                    {
                        OnClickTitleColBtn(i);
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
                // 再次渲染时，因已经获取各列的位置和大小信息后, 可以根据记录只渲染能在当前窗口里可以显示出来的列
                optInstance1.GetFirstAndLastColVisible(scrollPosition[0], out int firstColVisible, out int lastColVisible);
                GUILayout.BeginHorizontal("box");
                if (firstColVisible > 0)
                {
                    // 用一块空白填充不能显示的左侧区域, 以便滚动条的长度可以正确反应总数据量
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
                        // 用一块空白填充不能显示的右侧区域, 以便滚动条的长度可以正确反应总数据量
                        GUILayout.Space(optInstance1.GetColEndPosition(optInstance1.ColCount - 1) - optInstance1.GetColEndPosition(lastColVisible));
                    }
                    else
                    {
                        // 多填充两个上下滚动条的宽度，防止搜索结果显示区域出现上下滚动条时，左右滚动条滚到最右时，标题栏宽度不够而溢出报错
                        GUILayout.Space(2f * GUI.skin.verticalScrollbar.fixedWidth * widthRatio);
                    }
                }
                GUILayout.EndHorizontal();
                optInstance2.GetFirstAndLastColVisible(scrollPosition[0], out firstColVisible, out lastColVisible);
                GUILayout.BeginHorizontal("box");
                if (firstColVisible > 0)
                {
                    // 用一块空白填充不能显示的左侧区域, 以便滚动条的长度可以正确反应总数据量
                    GUILayout.Space(optInstance2.GetColEndPosition(firstColVisible - 1));
                }
                // 只渲染可以显示的内容
                for (int i = Math.Max(firstColVisible, 0); i <= lastColVisible; i++)
                {
                    if (GUILayout.Button("O", buttonStyle, colWidth[i]))
                    {
                        OnClickTitleColBtn(i);
                    }
                }
                if (lastColVisible != -1)
                {
                    if (lastColVisible < optInstance2.ColCount - 1)
                    {
                        // 用一块空白填充不能显示的右侧区域, 以便滚动条的长度可以正确反应总数据量
                        GUILayout.Space(optInstance2.GetColEndPosition(optInstance2.ColCount - 1) - optInstance2.GetColEndPosition(lastColVisible));
                    }
                    else
                    {
                        // 多填充一个上下滚动条的宽度，防止搜索结果显示区域出现上下滚动条时，左右滚动条滚到最右时，标题栏宽度不够而溢出报错
                        GUILayout.Space(2f * GUI.skin.verticalScrollbar.fixedWidth * widthRatio);
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
            int c = actorList.Count > countPerPage * Page ? countPerPage * Page : actorList.Count;
            // 让滚动条的大小随窗口的大小缩放
            float verticalThumbWidth = GUI.skin.verticalScrollbarThumb.fixedWidth;
            float horizonThumbHeight = GUI.skin.horizontalScrollbarThumb.fixedHeight;
            GUI.skin.verticalScrollbarThumb.fixedWidth = verticalThumbWidth * widthRatio;
            GUI.skin.horizontalScrollbarThumb.fixedHeight = horizonThumbHeight * heightRatio;
            scrollPosition[1] = GUILayout.BeginScrollView(scrollPosition[1], false, false, horizontalScrollbarStyle,
                                                          verticalScrollbarStyle, GUILayout.ExpandHeight(false));
            scrollPosition[0].x = scrollPosition[1].x; // 标题栏列随着结果的列一起移动
            scrollPosition[0].y = 0; // 冻结标题栏
            // 获取滚动视图优化类实例
            var optInstance3 = OptimizeScrollView.GetInstance("Content");
            // 首次渲染时记录各列和各行的位置和大小信息
            if (!optInstance3.IsReady())
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical("box");
                for (int i = (Page - 1) * countPerPage; i < c; i++)
                {
                    // 获取该行展示的内容
                    string[] additem = actorList[i].GetAddedItem();
                    GUILayout.BeginHorizontal("box");
                    for (int j = 0; j < additem.Length; j++)
                    {
                        if (GUILayout.Button(additem[j], labelStyle, colWidth[j]))
                        {
                            OnClickContentArea(i);
                        }
                        if (Event.current.type == EventType.Repaint)
                        {
                            // 记录各列位置和大小信息，只有repaint事件时才能获取LastRect
                            var rect = GUILayoutUtility.GetLastRect();
                            optInstance3.AddColRect(j, rect, mColumns.Count);
                        }
                    }
                    GUILayout.EndHorizontal();
                    if (Event.current.type == EventType.Repaint)
                    {
                        // 记录各行位置和大小，只有repaint事件时才能获取LastRect
                        optInstance3.AddRowRect(i - (Page - 1) * countPerPage, GUILayoutUtility.GetLastRect());
                    }
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.EndScrollView();
                if (Event.current.type == EventType.Repaint)
                {
                    // 记录滚动视图可现实区域的位置和大小，只有repaint事件时才能获取LastRect
                    optInstance3.AddViewRect(GUILayoutUtility.GetLastRect());
                }
            }
            else
            {
                // 再次渲染时，因已经获取各列和各行的位置和大小信息后, 可以根据记录只渲染能在当前窗口里可以显示出来的列和行
                optInstance3.GetFirstAndLastColVisible(scrollPosition[1], out int firstColVisible, out int lastColVisible);
                optInstance3.GetFirstAndLastRowVisible(scrollPosition[1], out int firstRowVisible, out int lastRowVisible);
                GUILayout.BeginHorizontal();
                if (firstColVisible > 0)
                {
                    // 用一块空白填充不能显示的左侧区域, 以便滚动条的长度可以正确反应总数据量
                    GUILayout.Space(optInstance3.GetColEndPosition(firstColVisible - 1));
                }
                GUILayout.BeginVertical("box");
                if (firstRowVisible > 0)
                {
                    // 用空白填充不能显示的上方区域, 以便滚动条的长度可以正确反应总数据量
                    GUILayout.Space(optInstance3.GetRowEndPosition(firstRowVisible - 1));
                }
                // 根据起、止行号, 算出起、止记录的序号
                int beginIndex = Math.Max((Page - 1) * countPerPage + firstRowVisible, 0);
                int endIndex = (Page - 1) * countPerPage + lastRowVisible + 1;
                // 只渲染可以显示的行
                for (int i = beginIndex; i < endIndex; i++)
                {
                    // 获取该行展示的内容
                    var additem = actorList[i].GetAddedItem();
                    GUILayout.BeginHorizontal("box", GUILayout.Height(optInstance3.GetRowHeight(i - (Page - 1) * countPerPage)));
                    // 只渲染可以显示的列
                    for (int j = Math.Max(firstColVisible, 0); j <= lastColVisible; j++)
                    {
                        tipContent.text = additem[j];
                        tipContent.tooltip = $"{actorList[i].ActorName}({actorList[i].npcId})";
                        if (GUILayout.Button(tipContent, labelStyle, colWidth[j]))
                        {
                            OnClickContentArea(i);
                        }
                    }
                    GUILayout.EndHorizontal();
                    // 鼠标指向NPC详情时提示NPC姓名
                    if (showNameTip && Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                    {
                        float x = (Event.current.mousePosition.x <= optInstance3.GetColEndPosition(firstColVisible + 1)) ? (Event.current.mousePosition.x + 10) : (Event.current.mousePosition.x - 100 * widthRatio);
                        float y = (i < firstRowVisible + 1) ? (Event.current.mousePosition.y + 10) : (Event.current.mousePosition.y - 30 * widthRatio);
                        tipRect = new Rect(x, y, 100 * widthRatio, 30 * widthRatio);
                    }
                }
                // 当前页的最后一个记录的序号
                int lastIndex = (Page - 1) * countPerPage + optInstance3.RowCount;
                if (endIndex < lastIndex)
                {
                    // 用空白填充不能显示的下方区域, 以便滚动条的长度可以正确反应总数据量
                    GUILayout.Space(optInstance3.GetRowEndPosition(optInstance3.RowCount - 1) - optInstance3.GetRowEndPosition(lastRowVisible));
                }
                GUILayout.EndVertical();
                if (lastColVisible != -1 && lastColVisible < optInstance3.ColCount - 1)
                {
                    // 用空白填充不能显示的右侧区域, 以便滚动条的长度可以正确反应总数据量
                    GUILayout.Space(optInstance3.GetColEndPosition(optInstance3.ColCount - 1) - optInstance3.GetColEndPosition(lastColVisible));
                }
                GUILayout.EndHorizontal();
                // 鼠标指向NPC详情时提示NPC姓名
                if (showNameTip && tipRect != default)
                {
                    var color = labelStyle.normal.textColor;
                    labelStyle.normal.textColor = new Color32(255, 226, 198, 255);
                    GUI.Label(tipRect, GUI.tooltip, labelStyle);
                    labelStyle.normal.textColor = color;
                }
                GUILayout.EndScrollView();
            }
            GUI.skin.verticalScrollbarThumb.fixedWidth = verticalThumbWidth;
            GUI.skin.horizontalScrollbarThumb.fixedHeight = horizonThumbHeight;
        }


        /// <summary>
        /// 判断是否换行同时添加一个label和一个textbox
        /// </summary>
        /// <param name="labelName">label名字</param>
        /// <param name="currentwidth">当前行宽度</param>
        /// <param name="settingInput">textbox的当前值</param>
        /// <returns>textbox的输入内容</returns>
        private int AddLabelAndTextField(string labelName, ref float currentwidth, int settingInput)
        {
            AddHorizontal(ref currentwidth, 65 * widthRatio);
            GUILayout.Label($"{labelName}:", labelStyle, GUILayout.Width(30 * widthRatio));
            if (!int.TryParse(GUILayout.TextField(settingInput.ToString(), 10, textStyle, GUILayout.Width(30 * widthRatio)), out var settingValue))
            {
                settingValue = 0;
            }
            GUILayout.Space(5 * widthRatio);
            return settingValue;
        }

        /// <summary>
        /// 判断是否换行同时添加一个label和一个textbox
        /// </summary>
        /// <param name="labelName">label名字</param>
        /// <param name="currentwidth">当前行宽度</param>
        /// <param name="settingInput">textbox的当前内容</param>
        /// <param name="addwidth">添加的宽度</param>
        /// <returns>textbox的输入内容</returns>
        private string AddLabelAndTextField(string labelName, ref float currentwidth, float addwidth, float labelWidth, float textWidth, string settingInput)
        {
            AddHorizontal(ref currentwidth, addwidth);
            GUILayout.Label($"{labelName}:", labelStyle, GUILayout.Width(labelWidth));
            return GUILayout.TextField(settingInput, textStyle, GUILayout.Width(textWidth));
        }

        /// <summary>
        /// 判断是否换行同时添加一个label和一个textbox
        /// </summary>
        /// <param name="labelName">label名字</param>
        /// <param name="currentwidth">当前行宽度</param>
        /// <param name="settingInput">textbox的当前内容</param>
        /// <param name="addwidth">添加的宽度</param>
        /// <returns>textbox的输入内容</returns>
        private int AddLabelAndTextField(string labelName, ref float currentwidth, float addwidth, float labelWidth, float textWidth, int settingInput)
        {

            AddHorizontal(ref currentwidth, addwidth);
            GUILayout.Label($"{labelName}:", labelStyle, GUILayout.Width(labelWidth));
            string outString = GUILayout.TextField(settingInput.ToString(), 10, textStyle, GUILayout.Width(textWidth));
            int.TryParse(outString, out var outValue);
            return outValue;
        }

        /// <summary>
        /// 是否换行判断
        /// </summary>
        /// <param name="currentwidth">当前宽</param>
        /// <param name="addWidth">添加宽</param>
        private void AddHorizontal(ref float currentwidth, float addWidth)
        {
            currentwidth += addWidth;
            if (currentwidth >= WindowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = addWidth;
                GUILayout.BeginHorizontal("box");
            }
        }

        /// <summary>
        /// 多选项控件(有且仅一个选项被选中)
        /// </summary>
        /// <param name="choiceStatus">选项的选择状态</param>
        /// <param name="choiceNames">选项的描述</param>
        /// <param name="originalChoiceIndex">原先被选中的选项的序号</param>
        /// <param name="choiceWidths">各选项的宽度，若只填一个数字，则所有选项均为此宽度</param>
        /// <returns>新选中的选项的序号(若选项不变则返回原值)</returns>
        private int MultiChoices(bool[] choiceStatus, string[] choiceNames, int originalChoiceIndex, params float[] choiceWidths)
        {
            int newIndex = originalChoiceIndex;
            for (int i = 0; i < choiceStatus.Length; i++)
            {
                bool tmp = choiceStatus[i];
                if (choiceWidths.Length == 1)
                {
                    choiceStatus[i] = GUILayout.Toggle(choiceStatus[i], choiceNames[i], toggleStyle, GUILayout.Width(choiceWidths[0] * widthRatio));
                }
                else
                {
                    choiceStatus[i] = GUILayout.Toggle(choiceStatus[i], choiceNames[i], toggleStyle, GUILayout.Width(choiceWidths[i] * widthRatio));
                }
                // 选项状态发生变化时，要保持有且仅有一个选项被选中，即至多一个选择被选中且至少一个被选中
                if (tmp != choiceStatus[i])
                {
                    if (choiceStatus[i])
                    {
                        newIndex = i;
                        // 为了保持至多一个选项被选中
                        for (int j = 0; j < choiceStatus.Length; j++)
                        {
                            if (i != j) choiceStatus[j] = false;
                        }
                    }
                    else
                    {
                        // 为了保持至少一个选项被选中
                        choiceStatus[i] = true;
                    }
                }
            }
            return newIndex;
        }

#if debug
        private readonly Stopwatch stopwatch = new Stopwatch();
#endif
        /// <summary>
        /// 执行搜索准备工作
        /// </summary>
        private void PrepareScan()
        {
#if debug
            stopwatch.Restart();
#endif
            // 每次点击查找。获取当前每页显示页数。
            countPerPage = Main.settings.countPerPage;
            reciprocalCountPerPage = (float)(1 / (double)countPerPage);

            Page = 1;
            // 首次搜索时，载入特性信息
            if (Main.featuresList.Count == 0)
            {
                foreach (var pair in DateFile.instance.actorFeaturesDate)
                {
                    var feature = new Features(pair.Key, pair.Value);
                    Main.featuresList.Add(pair.Key, feature);
                    var featureName = colorTagRegex.Replace(feature.Name, "");
                    if (Main.featureNameList.TryGetValue(featureName, out int value))
                    {
                        Main.multinameFeatureGroupIdSet.Add(feature.Group);
                        if (value != feature.Group)
                            Main.featureNameList[featureName] = feature.Group;
                    }
                    else
                    {
                        Main.featureNameList.Add(featureName, pair.Key);
                    }
                }
            }

            GetFeatures(ActorFeatureText, TarFeature);

            // 首次搜索时，载入功法信息
            if (Main.gongFaNameList.Count == 0)
            {
                foreach (var pair in DateFile.instance.gongFaDate)
                {
                    // 去掉颜色和空白
                    var tem = colorTagRegex.Replace(pair.Value[0], "");
                    if (tem != "")
                        Main.gongFaNameList.Add(tem, pair.Key);
                }
            }

            GetGongFaKey(ActorGongFaText);

            // 首次搜索时，载入技艺信息
            if (Main.skillNameList.Count == 0)
            {
                foreach (var pair in DateFile.instance.baseSkillDate)
                {
                    if (pair.Key < 100)
                    {
                        Main.skillNameList.Add(pair.Value[0].Substring(0, 1), pair.Key);
                    }
                }
            }

            GetSkillKey(ActorSkillText);

            // 首次搜索时，载入物品信息
            if (Main.itemNameList.Count == 0)
            {
                foreach (var pair in DateFile.instance.presetitemDate)
                {
                    // NPC一般不持有促织，不需要支持搜索促织
                    if (pair.Key <= 0 || int.Parse(pair.Value[2001]) == 1)
                        continue;

                    string name = Item.PurifyItemName(pair.Value[0]).Trim('《', '》');

                    if (!Main.itemNameList.TryGetValue(name, out var itemList))
                    {
                        itemList = new List<int>(2);
                        Main.itemNameList[name] = itemList;
                    }
                    itemList.Add(pair.Key);
                }
            }

            GetItemKey(ActorItemText);
#if debug
            stopwatch.Stop();
            Main.Logger.Log($"PrepareScan: {stopwatch.Elapsed}");
#endif
        }

        /// <summary>
        /// 搜索NPC
        /// </summary>
        private void ScanNpc()
        {
#if debug
            stopwatch.Restart();
#endif
            actorList.Clear();
            // 重置上一次排序的列为空，这样下次排序时对待所有列都是默认首先降序
            sortIndex = -1;
            // 多线程搜索的锁, 只有该方法中使用多线程，因此只需要用本地变量
            int locker = 0;
            // 启用线程安全补丁
            Patch.StartGetFeaturePatch();
            Parallel.ForEach(Characters.GetAllCharIds(), (npcId) =>
            {
                try
                {
                    var addItem = new ActorItem(npcId, this);
                    if (addItem.NeededAdd)
                    {
                        // 加锁
                        while (Interlocked.CompareExchange(ref locker, 1, 0) == 1) ;
                        try
                        {
                            actorList.Add(addItem);
                        }
                        finally
                        {
                            // 解锁，使用try finally避免抛出异常造成死锁
                            Interlocked.Exchange(ref locker, 0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Main.Logger.Log(ex.ToString());
                }
            });
            // 搜索完毕, 结束关闭多线程补丁
            Patch.StopGetFeaturePatch();
            actorList.Sort();
#if debug
            stopwatch.Stop();
            Main.Logger.Log($"ScanNpc: {stopwatch.Elapsed}");
#endif
        }

        /// <summary>
        /// 点击查找按钮或回车时触发
        /// </summary>
        /// <param name="colNum">列序号</param>
        private void OnClickSearchBtn()
        {
            PrepareScan();
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

        /// <summary>
        /// 点击搜索结果标题的按钮时触发
        /// </summary>
        /// <param name="colNum">列序号</param>
        private void OnClickTitleColBtn(int colNum)
        {
#if debug
            stopwatch.Restart();
#endif
            // TextAssets/SoundTrackInfos.txt
            AudioManager.instance.PlaySE("SE_BUTTONDEFAULT");
            // 新的一列作为排序依据时总是默认降序
            if (sortIndex != colNum)
            {
                desc = true;
            }
            sortIndex = colNum;
            actorList.Sort(SortList);
            // 排序完重置到第一页
            Page = 1;
            // 上下滚动条复位
            scrollPosition[1].y = 0;
            desc = !desc;
#if debug
            stopwatch.Stop();
            Main.Logger.Log($"Sort: {stopwatch.Elapsed}");
#endif
        }

        /// <summary>
        /// 点击搜索结果区域时触发
        /// </summary>
        /// <param name="itemIndex">记录序号</param>
        /// <param name="colNum">列序号</param>
        /// <remarks>点击NPC名字打开NPC窗口</remarks>
        private void OnClickContentArea(int itemIndex)
        {
            if (DateFile.instance.battleStart) /// 战斗中无效 <see cref="ActorMenu.ShowActorMenu"/>
                return;
            if (itemIndex < actorList.Count)
            {
                var npcId = actorList[itemIndex].npcId;
                // 为防止游戏npc数据改变之后而玩家没有重新搜索导致出错，做几步验证，而且过世的NPC的窗口没法打开
                if (Characters.HasChar(npcId)
                    && int.Parse(DateFile.instance.GetActorDate(npcId, 26, false)) == 0
                    && actorList[itemIndex].ActorName == DateFile.instance.GetActorName(npcId))
                {
                    // TextAssets/SoundTrackInfos.txt
                    AudioManager.instance.PlaySE("SE_BUTTONDEFAULT");

                    if (ActorMenu.Exists)
                        ActorMenu.instance.CloseActorMenu();

                    GEvent.AddOneShot(eEvents.ActorMenuOpened, args => ToggleWindow(false));
                    GEvent.AddOneShot(eEvents.ActorMenuClosed, args =>
                    {
                        if (npcId != DateFile.instance.MianActorID())
                            ui_NpcSearch.npcSearchActorID = -1;
                        ToggleWindow(true);
                    });
                    if (npcId == DateFile.instance.MianActorID())
                    {
                        UIManager.Instance.StackState();
                        ActorMenu.cantChangeTeam = true;
                        ActorMenu.instance.ShowActorMenu(false);
                    }
                    else
                    {
                        UIManager.Instance.StackState();
                        ActorMenu.cantChangeTeam = true;
                        ui_NpcSearch.npcSearchActorID = npcId;
                        ActorMenu.instance.ShowActorMenu(true);
                    }
                }
            }
        }

        /// <summary>
        /// 重置搜索条件
        /// </summary>
        private void OnClickResetBtn()
        {
            var properties = typeof(UI).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var fields = typeof(UI).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (field.FieldType.Name == "Boolean[]")
                {
                    bool[] array = (bool[])field.GetValue(this);
#if debug
                    Main.Logger.Log($"0 {field.Name} {array}");
#endif
                    Array.Clear(array, 0, array.Length);
                    array[0] = true;
                }
            }

            foreach (var property in properties)
            {
                if (property.PropertyType.Name == "Int32")
                {
#if debug
                    Main.Logger.Log($"1 {property.Name} {property.GetValue(this)}");
#endif
                    if (property.Name.StartsWith("H"))
                        property.SetValue(this, 1);
                    else if (property.Name == "Goodness")
                        property.SetValue(this, -1);
                    else
                        property.SetValue(this, 0);
                    continue;
                }

                if (property.PropertyType.Name == "Int32[]")
                {
#if debug
                    Main.Logger.Log($"2 {property.Name} {property.GetValue(this)}");
#endif
                    int[] array = (int[])property.GetValue(this);
                    Array.Clear(array, 0, array.Length);
                    continue;
                }

                if (property.PropertyType.Name == "String" && Regex.IsMatch(property.Name, @"^(?>[A-Z]\w*)"))
                {
#if debug
                    Main.Logger.Log($"3 {property.Name} {property.GetValue(this)}");
#endif
                    property.SetValue(this, "");
                    continue;
                }

                if (property.PropertyType.Name == "Boolean" && Regex.IsMatch(property.Name, @"^(?>[A-Z]\w*)"))
                {
#if debug
                    Main.Logger.Log($"4 {property.Name} {property.GetValue(this)}");
#endif
                    if (property.Name == "IsGetReal")
                        property.SetValue(this, true);
                    else
                        property.SetValue(this, false);
                    continue;
                }
            }
            showNameTip = true;
        }

        /// <summary>
        /// 获得想要搜索的特性
        /// </summary>
        /// <param name="str">特性搜索条件</param>
        /// <param name="TarFeaure">是否精确查找</param>
        /// <returns></returns>
        private void GetFeatures(string str, bool TarFeaure)
        {
            featureSearchSet.Clear();
            if (str.IsNullOrEmpty())
                return;

            var features = str.Split('|');
            foreach (string s in features)
            {
                if (Main.featureNameList.TryGetValue(s.Trim(), out int key))
                {
                    Features f = Main.featuresList[key];
                    // 如果该名称对应多个特性ID, 则只添加特性组ID
                    if (Main.multinameFeatureGroupIdSet.Contains(f.Group))
                    {
                        featureSearchSet.Add(f.Group);
                    }
                    else
                    {
                        if (!TarFeaure)
                        {
                            int Category = f.Category;
                            if (Category == 3 || Category == 4)
                            {
                                featureSearchSet.Add(f.Group);
                                continue;
                            }
                        }
                        featureSearchSet.Add(key);
                    }
                }
            }
        }

        /// <summary>
        /// 获得想要搜索的功法
        /// </summary>
        /// <param name="str">搜索词条</param>
        private void GetGongFaKey(string str)
        {
            gongFaSearchList.Clear();
            if (str.IsNullOrEmpty())
                return;

            var gongFaTemp = str.Split('|');
            foreach (string s in gongFaTemp)
            {
                if (Main.gongFaNameList.TryGetValue(s.Trim(), out int key))
                {
                    gongFaSearchList.Add(key);
                }
            }
        }

        /// <summary>
        /// 获得想要搜索的技艺
        /// </summary>
        /// <param name="str">搜索词条</param>
        private void GetSkillKey(string str)
        {
            skillSearchList.Clear();
            if (str.IsNullOrEmpty())
                return;

            var skillTemp = str.Split('|');
            foreach (var s in skillTemp)
            {
                var name = s.Trim();
                if (name.Length == 0) continue;

                if (Main.skillNameList.TryGetValue(name.Substring(0, 1), out int key))
                {
                    Main.Logger.Log($"{name},{key}");
                    skillSearchList.Add(key);
                }
            }
        }

        /// <summary>
        /// 获得想要搜索的物品
        /// </summary>
        /// <param name="str">搜索词条</param>
        private void GetItemKey(string str)
        {
            itemSearchSet.Clear();
            itemSearchList.Clear();
            if (str.IsNullOrEmpty())
                return;

            var itemTemp = str.Split('|');
            foreach (var s in itemTemp)
            {
                if (Main.itemNameList.TryGetValue(s.Trim(), out var keyList))
                {
                    // 一个名字对应多个ID时，将ID取反，以示区分，用零隔开
                    itemSearchSet.UnionWith(keyList);
                    itemSearchList.AddRange(keyList.Count > 1 ? keyList.Select(key => -key).Append(0) : keyList);
                }
            }
        }

        /// <summary>
        /// 关闭打开窗口
        /// </summary>
        public void ToggleWindow() => ToggleWindow(!opened);

        /// <summary>
        /// 关闭打开窗口
        /// </summary>
        /// <param name="open">是否打开</param>
        public void ToggleWindow(bool open)
        {
            if (opened == open)
                return;

            opened = open;
            BlockGameUI(open);
            if (open)
            {
                // 自适应游戏窗口变化
                CalculateWindowPos();
                // 调整字体
                labelStyle.fontSize = (int)(baseFontSize * widthRatio);
                buttonStyle.fontSize = (int)(baseFontSize * widthRatio);
                textStyle.fontSize = (int)(baseFontSize * widthRatio);
                toggleStyle.fontSize = (int)(baseFontSize * widthRatio);
                // 调整滚动条大小
                verticalScrollbarStyle.fixedWidth = verticalScrollBarWidth * widthRatio;
                horizontalScrollbarStyle.fixedHeight = horizonScollBarHeight * heightRatio;
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
        /// 排序
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int SortList(ActorItem a, ActorItem b)
        {
            int index = rankcolumnadded ? sortIndex - 1 : sortIndex;
            switch (index)
            {
                case -1:
                    return CheckMN(a.Totalrank, b.Totalrank);
                case 0:
                    return CheckMN(a.ActorName, b.ActorName);
                case 1:
                    return CheckMN(a.Age, b.Age);
                case 2:
                    return CheckMN(a.Gender, b.Gender);
                case 3:
                    return CheckMN(a.Place, b.Place);
                case 4:
                    return CheckMN(a.Charm, b.Charm);
                case 5:
                    return CheckMN(a.Groupid, b.Groupid);
                case 6:
                    return CheckMN(9 - Math.Abs(a.GangLevel), 9 - Math.Abs(b.GangLevel));
                case 7:
                    return CheckMN(a.GetShopName(), b.GetShopName());
                case 8:
                    return CheckMN(a.Goodness, b.Goodness);
                case 9:
                    return CheckMN(a.GetMarriage(), b.GetMarriage());
                case 10:
                    return CheckMN(a.GetSkillDevelopText(), b.GetSkillDevelopText());
                case 11:
                    return CheckMN(a.GetGongFaDevelopText(), b.GetGongFaDevelopText());
                case 12:
                    var array1 = a.GetHealthValue();
                    var array2 = b.GetHealthValue();
                    if (array1.Length != 2 || array1.Length != array2.Length)
                        return 0;
                    for (int i = 1; i > -1; i--)
                    {
                        var result = CheckMN(array1[i], array2[i]);
                        if (result != 0)
                            return result;
                    }
                    return 0;
                case 13:
                    return CheckMN(a.Str, b.Str);
                case 14:
                    return CheckMN(a.Con, b.Con);
                case 15:
                    return CheckMN(a.Agi, b.Agi);
                case 16:
                    return CheckMN(a.Bon, b.Bon);
                case 17:
                    return CheckMN(a.Inv, b.Inv);
                case 18:
                    return CheckMN(a.Pat, b.Pat);
                case 19:
                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                case 27:
                case 28:
                case 29:
                case 30:
                case 31:
                case 32:
                    return CheckMN(a.GetLevelValue(index - 19, 1), b.GetLevelValue(index - 19, 1));
                case 33:
                case 34:
                case 35:
                case 36:
                case 37:
                case 38:
                case 39:
                case 40:
                case 41:
                case 42:
                case 43:
                case 44:
                case 45:
                case 46:
                case 47:
                case 48:
                    return CheckMN(a.GetLevelValue(index - 33, 0), b.GetLevelValue(index - 33, 0));
                case 49:
                    return CheckMN(a.Money, b.Money);
                case 50:
                case 51:
                case 52:
                case 53:
                case 54:
                case 55:
                case 56:
                    return CheckMN(a.ActorResources[index - 50], b.ActorResources[index - 50]);
                case 57:
                    array1 = a.GetGongFaList();
                    array2 = b.GetGongFaList();
                    if (array1.Length == 0 || array1.Length != array2.Length)
                        return 0;
                    long l1 = 0, l2 = 0;
                    for (int i = 0; i < array1.Length; i++)
                    {
                        if (array1[i] != 0)
                            l1 += array1[i] * power[i];
                        if (array2[i] != 0)
                            l2 += array2[i] * power[i];
                    }
                    return CheckMN(l1, l2);
                case 58:
                    return CheckMN(a.GetMaxSkillGrade(), b.GetMaxSkillGrade());
                case 59:
                    int count1 = a.GetItems().Length;
                    int count2 = b.GetItems().Length;
                    int count = Math.Min(count1, count2);
                    for (int i = 0; i < count; i++)
                    {
                        var result = CheckMN(a.GetItems()[i], b.GetItems()[i]);
                        if (result != 0)
                            return result;
                    }
                    if (count1 > count)
                        return desc ? -1 : 1;
                    if (count2 > count)
                        return desc ? 1 : -1;
                    return 0;
                case 60:
                    return CheckMN(a.SamsaraNames, b.SamsaraNames);
                case 61:
                    count1 = a.ActorFeatures.Count;
                    count2 = b.ActorFeatures.Count;
                    count = Math.Min(count1, count2);
                    for (int i = 0; i < count; i++)
                    {
                        var result = CheckMN(a.ActorFeatures[i], b.ActorFeatures[i]);
                        if (result != 0)
                            return result;
                    }
                    if (count1 > count)
                        return desc ? -1 : 1;
                    if (count2 > count)
                        return desc ? 1 : -1;
                    return 0;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// 比较大小
        /// </summary>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CheckMN<T>(T m, T n) where T : IComparable<T> => (desc ? -1 : 1) * m.CompareTo(n);

        /// <summary>
        /// 计算每列宽度
        /// </summary>
        private void CalculateColWidth()
        {
            float amountWidth = 0;
            float expandWidth = 0;

            foreach (Column mCol in mColumns)
            {
                amountWidth += mCol.width * widthRatio;
                if (mCol.expand)
                    expandWidth += mCol.width * widthRatio;
            }

            float reciprocalexpandWidth = (float)(1 / (double)expandWidth);
            colWidth = new GUILayoutOption[mColumns.Count];
            for (int i = 0; i < colWidth.Length; i++)
            {
                if (mColumns[i].expand)
                    colWidth[i] = GUILayout.Width(mColumns[i].width * widthRatio * reciprocalexpandWidth * (WindowWidth() - 60 + expandWidth - amountWidth));
                else
                    colWidth[i] = GUILayout.Width(mColumns[i].width * widthRatio);
            }
        }

        /// <summary>
        /// 调整margin
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static RectOffset RectOffset(int value) => new RectOffset(value, value, value, value);

        /// <summary>
        /// 窗口宽度
        /// </summary>
        /// <returns></returns>
        private static float WindowWidth() => (DateFile.instance.screenWidth * 0.8f);

        /// <summary>
        /// 屏幕宽度
        /// </summary>
        /// <returns></returns>
        private static float ScreenWidth() => (DateFile.instance.screenWidth);
    }
}
