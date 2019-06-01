using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using System.Text.RegularExpressions;


namespace NpcScan
{
    public class UI : MonoBehaviour
    {
        public static UnityModManager.ModEntry.ModLogger logger;

        private bool desc = true;
        private int sortIndex = 0;
        private int page = 1;
        private bool rankcolumnadded = false;
        private bool showlistshrinked = false;
        private bool showlistadded = false;
        //立场goodnessText
        private bool[] goodness = new bool[] { true, false, false, false, false, false };
        private string[] goodnessValue = new string[] { "全部", "刚正", "仁善", "中庸", "叛逆", "唯我" };

        public int minage = 0;
        public int maxage = 0;
        public int strValue = 0;
        public int conValue = 0;
        public int agiValue = 0;
        public int bonValue = 0;
        public int intValue = 0;
        public int patValue = 0;
        public int genderValue = 0;
        public int charmValue = 0;
        public int samsaraCount = 0;
        public int healthValue = 1;
        public bool getreal = true;
        public bool rankmode = false;
        //从属gangText
        public string gangValue = "";
        //身份gangLevelText
        public string gangLevelValue = "";
        public string goodnessText = "";
        //商会
        public string aShopName = "";

        /// <summary>
        /// 0:内功;1:身法;2:绝技;3:拳掌;4:指法;5:腿法;6:暗器;7:剑法;8:刀法;9:长兵;10:奇门;11:软兵;12:御射;13:乐器;
        /// </summary>
        public int[] gongfa = new int[14];
        public static KeyCode key;

        /// <summary>
        /// 0:音律;1:弈棋;2:诗书;3:绘画;4:术数;5:品鉴;6:锻造;7:制木;8:医术;9:毒术;10:织锦;11:巧匠;12:道法;13:佛学;14:厨艺;15:杂学;
        /// </summary>
        public int[] life = new int[16];
        public string actorFeatureText = "";
        public bool tarFeature = false;
        public bool tarFeatureOr = false;

        public string actorGongFaText = "";
        public bool tarGongFaOr = false;

        public int highestLevel = 1;
        public bool tarIsGang = false;
        public bool isGang = false;

        float windowWidth()
        {
            return (DateFile.instance.screenWidth * 0.8f);
        }
        float screenWidth()
        {
            return (DateFile.instance.screenWidth);
        }

        public string aName = "";

        List<string[]> actorList = new List<string[]>();

        Vector2 scrollPosition = new Vector2(0, 0);
        Vector2 scrollPosition2 = new Vector2(0, 0);

        bool isall = true;
        bool isman = false;
        bool iswoman = false;

        internal static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            //logger.Log(windowWidth.ToString);
            try
            {
                new GameObject(typeof(UI).FullName, typeof(UI));
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return false;
            //GUILayout.Button("");
        }

        private static UI mInstance = null;

        public static UI Instance
        {
            get { return mInstance; }
        }

        public static GUIStyle window = null;
        public static GUIStyle h1 = null;
        public static GUIStyle h2 = null;
        public static GUIStyle bold = null;
        private static GUIStyle settings = null;
        private static GUIStyle status = null;
        private static GUIStyle www = null;
        private static GUIStyle updates = null;
        private GUIStyle featureStyle = null;

        private bool mInit = false;

        private bool mOpened = false;
        public bool Opened { get { return mOpened; } }

        private Rect mWindowRect = new Rect(0, 0, 0, 0);

        private void Awake()
        {
            mInstance = this;
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            CalculateWindowPos();
        }

        private void Update()
        {
            if (mOpened)
                mLogTimer += Time.unscaledDeltaTime;
            if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl))
                && Input.GetKeyUp(Main.settings.key))
            {
                ToggleWindow();
            }
        }

        private void OnDestroy()
        {
            //SaveSettingsAndParams();
        }

        private void PrepareGUI()
        {
            window = new GUIStyle();
            window.name = "umm window";
            //Texture2D pic = new Texture2D(200, 200);
            //byte[] data = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAIAAAEACAYAAACZCaebAAAAnElEQVRIS63MtQHDQADAwPdEZmaG/fdJCq2g7qqLvu/7hRBCZOF9X0ILz/MQWrjvm1DHdV3MFs7zJLRwHAehhX3fCS1s20ZoYV1XQgvLshDqmOeZ2cI0TYQWxnEktDAMA6GFvu8JLXRdR2ihbVtCHU3TMFuo65rQQlVVhBbKsiS0UBQFoYU8zwktZFlGqCNNU2YLSZIQWojjmFDCH22GtZAncD8TAAAAAElFTkSuQmCC");
            //pic.LoadImage(data);
            //window.normal.background = pic;
            //window.normal.background.wrapMode = TextureWrapMode.Repeat;
            window.padding = RectOffset(5);

            h1 = new GUIStyle();
            h1.name = "umm h1";
            h1.normal.textColor = Color.white;
            h1.fontSize = 16;
            h1.fontStyle = FontStyle.Bold;
            h1.alignment = TextAnchor.MiddleCenter;
            h1.margin = RectOffset(0, 5);

            h2 = new GUIStyle();
            h2.name = "umm h2";
            h2.normal.textColor = new Color(0.6f, 0.91f, 1f);
            h2.fontSize = 13;
            h2.fontStyle = FontStyle.Bold;
            //                h2.alignment = TextAnchor.MiddleCenter;
            h2.margin = RectOffset(0, 3);

            bold = new GUIStyle(GUI.skin.label);
            bold.name = "umm bold";
            bold.normal.textColor = Color.white;
            bold.fontStyle = FontStyle.Bold;

            int iconHeight = 28;
            settings = new GUIStyle();
            settings.alignment = TextAnchor.MiddleCenter;
            settings.stretchHeight = true;
            settings.fixedWidth = 24;
            settings.fixedHeight = iconHeight;

            status = new GUIStyle();
            status.alignment = TextAnchor.MiddleCenter;
            status.stretchHeight = true;
            status.fixedWidth = 12;
            status.fixedHeight = iconHeight;

            www = new GUIStyle();
            www.alignment = TextAnchor.MiddleCenter;
            www.stretchHeight = true;
            www.fixedWidth = 24;
            www.fixedHeight = iconHeight;

            updates = new GUIStyle();
            updates.alignment = TextAnchor.MiddleCenter;
            updates.stretchHeight = true;
            updates.fixedWidth = 26;
            updates.fixedHeight = iconHeight;

            featureStyle = new GUIStyle
            {
                richText = true,
            };


        }

        private void OnGUI()
        {
            if (!mInit)
            {
                mInit = true;
                PrepareGUI();
            }

            if (mOpened)
            {
                var backgroundColor = GUI.backgroundColor;
                var color = GUI.color;
                GUI.backgroundColor = Color.white;
                GUI.color = Color.white;
                mWindowRect = GUILayout.Window(952, mWindowRect, WindowFunction, "", window, GUILayout.Height(Screen.height - 200));
                GUI.backgroundColor = backgroundColor;
                GUI.color = color;
            }
        }

        class Column
        {
            public string name;
            public float width;
            public bool expand = false;
            public bool skip = false;
        }

        private List<Column> mColumns = new List<Column>    //remove readonly as the skip value may change
            {
                new Column {name = "姓名", width = 60},
                new Column {name = "年龄", width = 30},
                new Column {name = "性别", width = 30},
                new Column {name = "位置", width = 130},
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
                new Column {name = "可学功法", width = 240},
                new Column {name = "前世", width = 120},
                new Column {name = "特性", width = 500}
            };

        private float mLogTimer = 0;

        private void CalculateWindowPos()
        {
            logger.Log(screenWidth().ToString() + " " + windowWidth().ToString());
            mWindowRect = new Rect(screenWidth() * 0.05f, 50f, windowWidth(), 0);
        }

        /// <summary>
        ///  搜索条件窗口
        /// </summary>
        /// <param name="windowId">没用</param>
        private void WindowFunction(int windowId)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                GUI.DragWindow(mWindowRect);
            }

            GUILayout.BeginVertical("box");

            _setNo1Windows();
            _setNo2Windows();
            _setNo3Windows();
            _setNo4Windows();
            _setNo5Windows();
            _setNo6Windows();

            GUILayout.EndHorizontal();


            if (actorList.Count > 0)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, new GUIStyle(), new GUIStyle(), GUILayout.Height(70), GUILayout.Width(windowWidth() + 60f));
                GUILayout.BeginVertical("Box");
                GUILayout.BeginHorizontal("box");
                if (rankmode && !rankcolumnadded && showlistadded)
                {
                    mColumns.Insert(0, new Column { name = "综合评分", width = 80 });
                    rankcolumnadded = true;
                }
                if (!rankmode && rankcolumnadded && showlistshrinked)
                {
                    mColumns.RemoveAt(0);
                    rankcolumnadded = false;
                }
                var amountWidth = mColumns.Where(x => !x.skip).Sum(x => x.width);
                var expandWidth = mColumns.Where(x => x.expand && !x.skip).Sum(x => x.width);
                var mods = actorList;
                var colWidth = mColumns.Select(x =>
                    x.expand
                        ? GUILayout.Width(x.width / expandWidth * (windowWidth() - 60 + expandWidth - amountWidth))
                        : GUILayout.Width(x.width)).ToArray();
                for (int i = 0; i < mColumns.Count; i++)
                {
                    if (mColumns[i].skip)
                        continue;
                    GUILayout.Label(mColumns[i].name, colWidth[i]);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("box");
                for (int i = 0; i < mColumns.Count; i++)
                {
                    if (mColumns[i].skip)
                        continue;
                    if (GUILayout.Button("O", colWidth[i]))
                    {
                        Debug.Log("Clicked the " + mColumns[i].name);
                        sortIndex = i;
                        actorList.Sort(SortList);
                        desc = !desc;
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
                scrollPosition2 = GUILayout.BeginScrollView(scrollPosition2, GUILayout.ExpandHeight(false));
                scrollPosition = new Vector2(scrollPosition2.x, 0);
                GUILayout.BeginVertical("box");
                int c = mods.Count;
                c = c > 50 * page ? 50 * page : c;
                for (int i = (page - 1) * 50; i < c; i++)
                {
                    GUILayout.BeginVertical("box");
                    GUILayout.BeginHorizontal();
                    for (int j = 0; j < mods[i].Count(); j++)
                    {
                        GUILayout.BeginHorizontal(colWidth[j]);
                        int itemsList = 56;
                        if (rankmode) itemsList++;
                        if (j == itemsList)
                        {
                            GUILayout.Label(mods[i][j].ToString(), featureStyle);
                        }
                        else
                        {
                            GUILayout.Label(mods[i][j].ToString());
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                }

                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
            GUILayout.Space(3);
        }

        /// <summary>
        /// 设置第一行内容
        /// </summary>
        private void _setNo1Windows()
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("NPC查找器");
            if (GUILayout.Button("关闭", GUILayout.Width(150)))
            {
                ToggleWindow();
            }
            GUILayout.EndHorizontal();
        }
        /// <summary>
        /// 设置第二行内容
        /// </summary>
        private void _setNo2Windows()
        {
            int currentwidth = 0;

            GUILayout.BeginHorizontal("box");
            #region add 年龄 性别
            GUILayout.Label("年龄:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(minage.ToString(), 3, GUILayout.Width(30)), out minage);
            GUILayout.Label("--", GUILayout.Width(10));
            int.TryParse(GUILayout.TextField(maxage.ToString(), 3, GUILayout.Width(30)), out maxage);
            GUILayout.Space(10);
            currentwidth += 110;
            currentwidth += 150;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 150;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("性别:", GUILayout.Width(30));
            GUILayout.Space(5);
            isall = GUILayout.Toggle(isall, "全部", GUILayout.Width(45));
            if (isall)
            {
                iswoman = false;
                isman = false;
                genderValue = 0;
            }

            isman = GUILayout.Toggle(isman, "男", GUILayout.Width(30));
            if (isman)
            {
                isall = false;
                iswoman = false;
                genderValue = 1;
            }
            iswoman = GUILayout.Toggle(iswoman, "女", GUILayout.Width(30));
            if (iswoman)
            {
                isall = false;
                isman = false;
                genderValue = 2;
            }
            GUILayout.Space(10);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            #endregion

            #region add 基础属性
            currentwidth = _addLabelAndTextField("膂力", currentwidth, strValue, out strValue);
            currentwidth = _addLabelAndTextField("体质", currentwidth, conValue, out conValue);
            currentwidth = _addLabelAndTextField("灵敏", currentwidth, agiValue, out agiValue);
            currentwidth = _addLabelAndTextField("根骨", currentwidth, bonValue, out bonValue);
            currentwidth = _addLabelAndTextField("悟性", currentwidth, intValue, out intValue);
            currentwidth = _addLabelAndTextField("定力", currentwidth, patValue, out patValue);
            currentwidth = _addLabelAndTextField("魅力", currentwidth, charmValue, out charmValue);
            currentwidth = _addLabelAndTextField("健康", currentwidth, healthValue, out healthValue);
            currentwidth = _addLabelAndTextField("轮回次数", currentwidth, samsaraCount, out samsaraCount);
            #endregion

            #region add 翻页
            GUILayout.Label(string.Format("{0}/{1}:", page, (int)Math.Ceiling((double)actorList.Count / 50d)), GUILayout.Width(40));
            if (GUILayout.Button("上页", GUILayout.Width(60)))
            {
                if (page > 1)
                    page = page - 1;
            }
            currentwidth += 60;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 60;
                GUILayout.BeginHorizontal("box");
            }
            if (GUILayout.Button("下页", GUILayout.Width(60)))
            {
                if (actorList.Count > page * 50)
                    page = page + 1;
            }
            #endregion
            GUILayout.EndHorizontal();
        }
        /// <summary>
        /// 设置第三行内容
        /// </summary>
        private void _setNo3Windows()
        {
            int currentwidth = 0;
            GUILayout.BeginHorizontal("box");

            #region add 功法属性

            currentwidth = _addLabelAndTextField("内功", currentwidth, gongfa[0], out gongfa[0]);
            currentwidth = _addLabelAndTextField("身法", currentwidth, gongfa[1], out gongfa[1]);
            currentwidth = _addLabelAndTextField("绝技", currentwidth, gongfa[2], out gongfa[2]);
            currentwidth = _addLabelAndTextField("拳掌", currentwidth, gongfa[3], out gongfa[3]);
            currentwidth = _addLabelAndTextField("指法", currentwidth, gongfa[4], out gongfa[4]);
            currentwidth = _addLabelAndTextField("腿法", currentwidth, gongfa[5], out gongfa[5]);
            currentwidth = _addLabelAndTextField("暗器", currentwidth, gongfa[6], out gongfa[6]);
            currentwidth = _addLabelAndTextField("剑法", currentwidth, gongfa[7], out gongfa[7]);
            currentwidth = _addLabelAndTextField("刀法", currentwidth, gongfa[8], out gongfa[8]);
            currentwidth = _addLabelAndTextField("长兵", currentwidth, gongfa[9], out gongfa[9]);
            currentwidth = _addLabelAndTextField("奇门", currentwidth, gongfa[10], out gongfa[10]);
            currentwidth = _addLabelAndTextField("软兵", currentwidth, gongfa[11], out gongfa[11]);
            currentwidth = _addLabelAndTextField("御射", currentwidth, gongfa[12], out gongfa[12]);
            currentwidth = _addLabelAndTextField("乐器", currentwidth, gongfa[13], out gongfa[13]);

            #endregion

            GUILayout.Label("取值:", GUILayout.Width(30));
            GUILayout.Space(5);
            getreal = GUILayout.Toggle(getreal, "基础值", GUILayout.Width(55));
            GUILayout.Space(5);
            rankmode = GUILayout.Toggle(rankmode, "排行模式", GUILayout.Width(65));

            GUILayout.EndHorizontal();
        }
        /// <summary>
        /// 设置第四行内容
        /// </summary>
        private void _setNo4Windows()
        {
            int currentwidth = 0;
            GUILayout.BeginHorizontal("box");

            #region add 功法属性

            currentwidth = _addLabelAndTextField("音律", currentwidth, life[0], out life[0]);
            currentwidth = _addLabelAndTextField("弈棋", currentwidth, life[1], out life[1]);
            currentwidth = _addLabelAndTextField("诗书", currentwidth, life[2], out life[2]);
            currentwidth = _addLabelAndTextField("绘画", currentwidth, life[3], out life[3]);
            currentwidth = _addLabelAndTextField("术数", currentwidth, life[4], out life[4]);
            currentwidth = _addLabelAndTextField("品鉴", currentwidth, life[5], out life[5]);
            currentwidth = _addLabelAndTextField("锻造", currentwidth, life[6], out life[6]);
            currentwidth = _addLabelAndTextField("制木", currentwidth, life[7], out life[7]);
            currentwidth = _addLabelAndTextField("医术", currentwidth, life[8], out life[8]);
            currentwidth = _addLabelAndTextField("毒术", currentwidth, life[9], out life[9]);
            currentwidth = _addLabelAndTextField("织锦", currentwidth, life[10], out life[10]);
            currentwidth = _addLabelAndTextField("巧匠", currentwidth, life[11], out life[11]);
            currentwidth = _addLabelAndTextField("道法", currentwidth, life[12], out life[12]);
            currentwidth = _addLabelAndTextField("佛学", currentwidth, life[13], out life[13]);
            currentwidth = _addLabelAndTextField("厨艺", currentwidth, life[14], out life[14]);
            currentwidth = _addLabelAndTextField("杂学", currentwidth, life[15], out life[15]);

            #endregion

            GUILayout.EndHorizontal();
        }
        /// <summary>
        /// 设置第五行内容
        /// </summary>
        private void _setNo5Windows()
        {
            int currentwidth = 0;
            GUILayout.BeginHorizontal("box");

            currentwidth = _addLabelAndTextField("姓名（包括前世）", currentwidth, 110, 120, 80, aName, out aName);

            //从属gangText
            currentwidth = _addLabelAndTextField("从属", currentwidth, 110, 50, 60, gangValue, out gangValue);
            //身份gangLevelText
            currentwidth = _addLabelAndTextField("身份", currentwidth, 300, 50, 60, gangLevelValue, out gangLevelValue);
            //商会
            currentwidth = _addLabelAndTextField("商会", currentwidth, 110, 50, 60, aShopName, out aShopName);
            //立场goodnessText
            GUILayout.Label("立场:", GUILayout.Width(30));
            for (int i = 0; i < goodness.Length; i++)
            {
                goodness[i] = GUILayout.Toggle(goodness[i], goodnessValue[i].ToString(), GUILayout.Width(45));
                if (goodness[i])
                {
                    for (int j = 0; j < goodness.Length; j++)
                    {
                        if (i != j)
                        {
                            goodness[j] = false;
                            goodnessText = goodnessValue[i].ToString();
                        }
                    }
                }
            }
            _addHorizontal(currentwidth, 255);
            GUILayout.Label("人物特性:", GUILayout.Width(60));
            actorFeatureText = GUILayout.TextField(actorFeatureText, 60, GUILayout.Width(120));
            tarFeature = GUILayout.Toggle(tarFeature, "精确特性", GUILayout.Width(75));//是否精确查找,精确查找的情况下,特性用'|'分隔
            tarFeatureOr = GUILayout.Toggle(tarFeatureOr, "OR查询", new GUILayoutOption[0]);//默认AND查询方式
            GUILayout.EndHorizontal();
        }
        /// <summary>
        /// 设置第六行内容
        /// </summary>
        private void _setNo6Windows()
        {
            int currentwidth = 0;
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("可教功法:", GUILayout.Width(60));
            actorGongFaText = GUILayout.TextField(actorGongFaText, 60, GUILayout.Width(120));
            tarGongFaOr = GUILayout.Toggle(tarGongFaOr, "OR查询", new GUILayoutOption[0]);//默认AND查询方式
            currentwidth += 180;
            currentwidth = _addHorizontal(currentwidth, 210);

            GUILayout.Label("最高查询品级:", GUILayout.Width(120));
            int.TryParse(GUILayout.TextField(highestLevel.ToString(), 1, GUILayout.Width(60)), out highestLevel);
            tarIsGang = GUILayout.Toggle(tarIsGang, "是否开启识别门派", new GUILayoutOption[0]);
            isGang = GUILayout.Toggle(isGang, "仅搜索门派", new GUILayoutOption[0]);

            //Main.Logger.Log(tarFeature.ToString());
            GUILayout.Space(30);
            currentwidth = _addHorizontal(currentwidth, 150);
            if (GUILayout.Button("查找", GUILayout.Width(150)))
            {
                page = 1;
                string s = Main.featuresList.Count.ToString();
                //Main.Logger.Log("测试对象字典长度:" + s);
                if (Main.featuresList.Count == 0)
                {
                    Features aFeature = new Features();
                    foreach (int i in DateFile.instance.actorFeaturesDate.Keys)
                    {
                        aFeature = new Features(i);
                        Main.featuresList.Add(aFeature.Key, aFeature);
                        Main.fNameList.Add(aFeature.Name, aFeature.Key);
                    }
                }
                s = Main.findList.Count.ToString();
                //Main.Logger.Log("测试查找列表:" + s);
                if (actorFeatureText != "")
                {
                    Main.findList = GetFeatureKey(actorFeatureText, tarFeature);
                }

                if (Main.gNameList.Count == 0)
                {
                    foreach (int i in DateFile.instance.gongFaDate.Keys)
                    {
                        //logger.Log(DateFile.instance.gongFaDate[i][0]);
                        if (DateFile.instance.gongFaDate[i][0].LastIndexOf('<') - DateFile.instance.gongFaDate[i][0].IndexOf('>') - 1 > 0)
                        {
                            String tem = DateFile.instance.gongFaDate[i][0].Substring(DateFile.instance.gongFaDate[i][0].IndexOf('>') + 1, DateFile.instance.gongFaDate[i][0].LastIndexOf('<') - DateFile.instance.gongFaDate[i][0].IndexOf('>') - 1);
                            Main.gNameList.Add(tem, i);
                        }
                        else
                        {
                            Main.gNameList.Add(DateFile.instance.gongFaDate[i][0], i);
                        }

                    }
                }
                if (actorGongFaText != "")
                {
                    Main.GongFaList = GetGongFaKey(actorGongFaText);
                }
                //s = Main.findList.Count.ToString();
                //Main.Logger.Log("测试查找列表:" + s);
                ScanNpc();
                if (!rankmode)
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
        }

        /// <summary>
        /// 同时添加一个laber和一个textbox
        /// </summary>
        /// <param name="labelName">laber名字</param>
        /// <param name="currentwidth">当前行宽度</param>
        /// <param name="settingName">textbox的名字</param>
        /// <param name="settingValue">textbox的内容</param>
        /// <returns></returns>
        private int _addLabelAndTextField(string labelName, int currentwidth, int settingName, out int settingValue)
        {
            GUILayout.Label(labelName + ":", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(settingName.ToString(), 10, GUILayout.Width(30)), out settingValue);
            GUILayout.Space(5);
            currentwidth = _addHorizontal(currentwidth, 65);
            return currentwidth;
        }

        /// <summary>
        /// 同时添加一个laber和一个textbox
        /// </summary>
        /// <param name="labelName">laber名字</param>
        /// <param name="currentwidth">当前行宽度</param>
        /// <param name="settingName">textbox的名字</param>
        /// <param name="add">添加的宽度</param>
        /// <param name="outString">textbox的内容</param>
        /// <returns></returns>
        private int _addLabelAndTextField(string labelName, int currentwidth, int addwidth, int labelWidth, int textWidth, string settingName, out string outString)
        {
            GUILayout.Label(labelName + ":", GUILayout.Width(labelWidth));
            outString = GUILayout.TextField(settingName, 10, GUILayout.Width(textWidth));
            currentwidth = _addHorizontal(currentwidth, addwidth);
            return currentwidth;
        }

        /// <summary>
        /// 是否换行判断
        /// </summary>
        /// <param name="currentwidth">当前宽</param>
        /// <param name="addWidth">添加宽</param>
        /// <returns>当前宽</returns>
        private int _addHorizontal(int currentwidth, int addWidth)
        {
            currentwidth += addWidth;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = addWidth;
                GUILayout.BeginHorizontal("box");
            }
            return currentwidth;
        }

        private void ScanNpc()
        {
            actorList.Clear();
            DateFile dateFile = DateFile.instance;
            Dictionary<int, Dictionary<int, string>> actors = dateFile.actorsDate;
            foreach (int index in actors.Keys)
            {
                ActorItem addItem = new ActorItem(index, this);
                if (addItem.isNeedAdd)
                {
                    actorList.Add(addItem.GetAddItem());
                }
            }
        }

        private static List<Features> GetFeatureKey(string str, bool flag)
        {
            //List<int> fKey = new List<int>(Main.featuresList.Keys);
            List<Features> list = new List<Features>();
            string[] aFeatures = str.Split('|');
            foreach (string s in aFeatures)
            {
                if (Main.fNameList.ContainsKey(s))
                {
                    int i = Main.fNameList[s];
                    Features f = Main.featuresList[i];
                    if (flag)
                    {
                        list.Add(f);
                    }
                    else
                    {
                        int j = f.Group;
                        list.Add(Main.featuresList[j]);
                        //list.Add(Main.featuresList[j + 1]);
                        //list.Add(Main.featuresList[j + 2]);
                        //list.Add(Main.featuresList[j + 3]);
                        //list.Add(Main.featuresList[j + 4]);
                        //list.Add(Main.featuresList[j + 5]);
                    }
                }
            }
            //foreach (int i in Main.featuresList.Keys)
            //{
            //    //if (i == 0) continue;
            //    Features f = Main.featuresList[i];
            //    //Main.Logger.Log(f.Plus.ToString());
            //    if (f.Plus == 3 || f.Plus == 4)
            //    {
            //        foreach (string s in aFeatures)
            //        {
            //            if (f.Name.Equals(s))
            //            {
            //                if (flag)
            //                {
            //                    //Main.Logger.Log("SCAN标记1:" + f.Name);
            //                    list.Add(f);
            //                }
            //                else
            //                {
            //                    int j = f.Group;
            //                    //Main.Logger.Log("SCAN标记2:" + Main.featuresList[j].Name + "|" + Main.featuresList[j + 1].Name + "|" + Main.featuresList[j + 2].Name);
            //                    list.Add(Main.featuresList[j]);
            //                    list.Add(Main.featuresList[j + 1]);
            //                    list.Add(Main.featuresList[j + 2]);
            //                }
            //            }
            //        }
            //    }
            //}
            return list;
        }

        private static bool ScanFeature(int key, List<Features> slist, bool tarFeature, bool tarFeatureOr)
        {
            List<int> list = new List<int>(DateFile.instance.GetActorFeature(key));
            bool result = false;
            if (slist.Count == 0)
            {
                return result;
            }
            List<Features> actorFeature = new List<Features>();
            foreach (int i in list)
            {
                if (tarFeature)  //精确查找记录特性
                {
                    actorFeature.Add(Main.featuresList[i]);
                }
                else            //组查找 记录组ID
                {
                    Features f = Main.featuresList[i];
                    int j = f.Group;
                    actorFeature.Add(Main.featuresList[j]);
                }
            }

            if (!tarFeatureOr)   //与查找
            {
                if (slist.All(t => actorFeature.Any(b => b.Key == t.Key)))
                {
                    result = true;
                }
            }
            else                //或查找
            {
                foreach (Features f in actorFeature)
                {
                    if (slist.Contains(f))
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        private static List<int> GetGongFaKey(string str)
        {
            List<int> list = new List<int>();
            string[] gongFaTemp = str.Split('|');
            foreach (string s in gongFaTemp)
            {
                if (Main.gNameList.ContainsKey(s))
                {
                    int i = Main.gNameList[s];
                    list.Add(i);
                }
            }
            return list;
        }

        public void ToggleWindow()
        {
            ToggleWindow(!mOpened);
        }

        public void ToggleWindow(bool open)
        {
            mOpened = open;
            BlockGameUI(open);
            if (!mOpened)
            {
                //SaveSettingsAndParams();
            }
            if (open)
            {
                GameCursorLocked = Cursor.lockState == CursorLockMode.Locked || !Cursor.visible;
                if (GameCursorLocked)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
            }
            else
            {
                if (GameCursorLocked)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }

        private GameObject mCanvas = null;
        private void BlockGameUI(bool value)
        {
            if (value)
            {
                mCanvas = new GameObject("", typeof(Canvas), typeof(GraphicRaycaster));
                mCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                mCanvas.GetComponent<Canvas>().sortingOrder = Int16.MaxValue;
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

        private static RectOffset RectOffset(int value)
        {
            return new RectOffset(value, value, value, value);
        }

        private static RectOffset RectOffset(int x, int y)
        {
            return new RectOffset(x, x, y, y);
        }

        private int SortList(string[] a, string[] b)
        {
            int m = 0;
            int n = 0;
            string s1 = a[sortIndex];
            string s2 = b[sortIndex];
            if (sortIndex != 6)
            {
                if (s1.Contains("color"))
                {
                    string check = s1.Contains("(") ? "\\(.*?\\)" : "<.*?>";

                    s1 = System.Text.RegularExpressions.Regex.Replace(s1, check, "");
                    s2 = System.Text.RegularExpressions.Regex.Replace(s2, check, "");
                }
                int.TryParse(s1, out m);
                int.TryParse(s2, out n);

                if (m != n)
                {
                    return CheckMN(m, n, desc);
                }
                else
                {
                    return desc ? -s1.CompareTo(s2) : s1.CompareTo(s2);
                }
            }
            else
            {
                Regex reg = new Regex("<(.+?)>");

                Main.Logger.Log(reg.Match(s1).Groups[0].Value + "," + reg.Match(s2).Groups[0].Value);
                m = Main.colorText[reg.Match(s1).Groups[0].Value];
                n = Main.colorText[reg.Match(s2).Groups[0].Value];
                return CheckMN(m, n, desc);
            }
        }

        private int CheckMN(int m, int n, bool desc)
        {
            int rtn = desc ? -1 : 1;
            if (m > n)
            {
                return rtn;
            }
            else if (m < n)
            {
                return -1 * rtn;
            }
            else
            {
                return 0;
            }
        }

        internal bool GameCursorLocked { get; set; }
        //        [HarmonyPatch(typeof(Screen), "lockCursor", MethodType.Setter)]
        static class Screen_lockCursor_Patch
        {
            static bool Prefix(bool value)
            {
                if (UI.Instance != null && UI.Instance.Opened)
                {
                    UI.Instance.GameCursorLocked = value;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    return false;
                }

                return true;
            }
        }
    }
}