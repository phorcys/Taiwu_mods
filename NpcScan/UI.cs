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
        int minage = 0;
        int maxage = 0;
        int strValue = 0;
        int conValue = 0;
        int agiValue = 0;
        int bonValue = 0;
        int intValue = 0;
        int patValue = 0;
        int genderValue = 0;
        int charmValue = 0;
        int samsaraCount = 0;
        int healthValue = 1;
        bool desc = true;
        int sortIndex = 0;
        int page = 1;
        bool getreal = true;
        bool rankmode = false;
        bool rankcolumnadded = false;
        bool showlistshrinked = false;
        bool showlistadded = false;
        //从属gangText
        string gangValue = "";
        //身份gangLevelText
        string gangLevelValue = "";
        //立场goodnessText
        bool[] goodness = new bool[] { true, false, false, false, false, false };
        string[] goodnessValue = new string[] { "全部", "刚正", "仁善", "中庸", "叛逆", "唯我" };
        string goodnessText = "";
        //内功
        //身法
        //绝技
        //拳掌
        //指法
        //腿法
        //暗器
        //剑法
        //刀法
        //长兵
        //奇门
        //软兵
        //御射
        //乐器
        int[] gongfa = new int[14];
        public static KeyCode key;
        //音律
        //弈棋
        //诗书
        //绘画
        //术数
        //品鉴
        //锻造
        //制木
        //医术
        //毒术
        //织锦
        //巧匠
        //道法
        //佛学
        //厨艺
        //杂学
        int[] life = new int[16];
        string actorFeatureText = "";
        bool tarFeature = false;
        bool tarFeatureOr = false;


        string actorGongFaText = "";
        bool tarGongFaOr = false;

        int highestLevel = 1;
        bool tarIsGang = false;
        bool isGang = false;

        float windowWidth()
        {
            return (DateFile.instance.screenWidth * 0.8f);
        }
        float screenWidth()
        {
            return (DateFile.instance.screenWidth);
        }

        string aName = "";

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
            logger.Log(screenWidth().ToString()+" "+ windowWidth().ToString());
            mWindowRect = new Rect(screenWidth()*0.05f, 50f, windowWidth(), 0);
        }

        private void WindowFunction(int windowId)
        {
            if (Input.GetKey(KeyCode.LeftControl))
                GUI.DragWindow(mWindowRect);

            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("NPC查找器");
            if (GUILayout.Button("关闭", GUILayout.Width(150)))
            {
                ToggleWindow();
            }
            GUILayout.EndHorizontal();
            int currentwidth = 0;
            GUILayout.BeginHorizontal("box");
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
            GUILayout.Label("膂力:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(strValue.ToString(), 10, GUILayout.Width(30)), out strValue);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("体质:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(conValue.ToString(), 10, GUILayout.Width(30)), out conValue);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("灵敏:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(agiValue.ToString(), 10, GUILayout.Width(30)), out agiValue);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("根骨:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(bonValue.ToString(), 10, GUILayout.Width(30)), out bonValue);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("悟性:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(intValue.ToString(), 10, GUILayout.Width(30)), out intValue);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("定力:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(patValue.ToString(), 10, GUILayout.Width(30)), out patValue);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("魅力:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(charmValue.ToString(), 10, GUILayout.Width(30)), out charmValue);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("健康:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(healthValue.ToString(), 10, GUILayout.Width(30)), out healthValue);
            GUILayout.Space(5);
            currentwidth += 130;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("轮回次数:", GUILayout.Width(60));
            int.TryParse(GUILayout.TextField(samsaraCount.ToString(), 10, GUILayout.Width(30)), out samsaraCount);
            GUILayout.Label(string.Format("{0}/{1}:", page, (int)Math.Ceiling((double)actorList.Count / 50d)), GUILayout.Width(40));
            currentwidth += 60;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
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
            GUILayout.EndHorizontal();
            currentwidth = 0;
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("内功:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(gongfa[0].ToString(), 10, GUILayout.Width(30)), out gongfa[0]);
            GUILayout.Space(5);
            currentwidth += 65;
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("身法:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(gongfa[1].ToString(), 10, GUILayout.Width(30)), out gongfa[1]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("绝技:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(gongfa[2].ToString(), 10, GUILayout.Width(30)), out gongfa[2]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("拳掌:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(gongfa[3].ToString(), 10, GUILayout.Width(30)), out gongfa[3]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("指法:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(gongfa[4].ToString(), 10, GUILayout.Width(30)), out gongfa[4]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("腿法:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(gongfa[5].ToString(), 10, GUILayout.Width(30)), out gongfa[5]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("暗器:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(gongfa[6].ToString(), 10, GUILayout.Width(30)), out gongfa[6]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("剑法:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(gongfa[7].ToString(), 10, GUILayout.Width(30)), out gongfa[7]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("刀法:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(gongfa[8].ToString(), 10, GUILayout.Width(30)), out gongfa[8]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("长兵:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(gongfa[9].ToString(), 10, GUILayout.Width(30)), out gongfa[9]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("奇门:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(gongfa[10].ToString(), 10, GUILayout.Width(30)), out gongfa[10]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("软兵:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(gongfa[11].ToString(), 10, GUILayout.Width(30)), out gongfa[11]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("御射:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(gongfa[12].ToString(), 10, GUILayout.Width(30)), out gongfa[12]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("乐器:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(gongfa[13].ToString(), 10, GUILayout.Width(30)), out gongfa[13]);
            GUILayout.Space(10);
            currentwidth += 160;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 160;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("取值:", GUILayout.Width(30));
	        GUILayout.Space(5);
	        getreal = GUILayout.Toggle(getreal, "基础值", GUILayout.Width(55));
            GUILayout.Space(5);
            rankmode = GUILayout.Toggle(rankmode, "排行模式", GUILayout.Width(65));
            GUILayout.EndHorizontal();
            currentwidth = 0;
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("音律:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(life[0].ToString(), 10, GUILayout.Width(30)), out life[0]);
            GUILayout.Space(5);
            currentwidth += 65;
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("弈棋:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(life[1].ToString(), 10, GUILayout.Width(30)), out life[1]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("诗书:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(life[2].ToString(), 10, GUILayout.Width(30)), out life[2]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("绘画:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(life[3].ToString(), 10, GUILayout.Width(30)), out life[3]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("术数:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(life[4].ToString(), 10, GUILayout.Width(30)), out life[4]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("品鉴:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(life[5].ToString(), 10, GUILayout.Width(30)), out life[5]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("锻造:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(life[6].ToString(), 10, GUILayout.Width(30)), out life[6]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("制木:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(life[7].ToString(), 10, GUILayout.Width(30)), out life[7]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("医术:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(life[8].ToString(), 10, GUILayout.Width(30)), out life[8]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("毒术:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(life[9].ToString(), 10, GUILayout.Width(30)), out life[9]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("织锦:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(life[10].ToString(), 10, GUILayout.Width(30)), out life[10]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("巧匠:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(life[11].ToString(), 10, GUILayout.Width(30)), out life[11]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("道法:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(life[12].ToString(), 10, GUILayout.Width(30)), out life[12]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("佛学:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(life[13].ToString(), 10, GUILayout.Width(30)), out life[13]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("厨艺:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(life[14].ToString(), 10, GUILayout.Width(30)), out life[14]);
            GUILayout.Space(5);
            currentwidth += 65;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 65;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("杂学:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(life[15].ToString(), 10, GUILayout.Width(30)), out life[15]);
            GUILayout.EndHorizontal();
            currentwidth = 0;
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("姓名（包括前世）:", GUILayout.Width(120));
            aName = GUILayout.TextField(aName, 10, GUILayout.Width(80));
            currentwidth += 200;
            currentwidth += 110;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 110;
                GUILayout.BeginHorizontal("box");
            }
            //从属gangText
            GUILayout.Label("从属:", GUILayout.Width(50));
            gangValue = GUILayout.TextField(gangValue, 10, GUILayout.Width(60));
            currentwidth += 110;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 110;
                GUILayout.BeginHorizontal("box");
            }
            //身份gangLevelText
            GUILayout.Label("身份:", GUILayout.Width(50));
            gangLevelValue = GUILayout.TextField(gangLevelValue, 10, GUILayout.Width(60));
            currentwidth += 300;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 300;
                GUILayout.BeginHorizontal("box");
            }
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
            currentwidth += 255;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 255;
                GUILayout.BeginHorizontal("box");
            }
            GUILayout.Label("人物特性:", GUILayout.Width(60));
            actorFeatureText = GUILayout.TextField(actorFeatureText, 60, GUILayout.Width(120));
            tarFeature = GUILayout.Toggle(tarFeature, "精确特性", GUILayout.Width(75));//是否精确查找,精确查找的情况下,特性用'|'分隔
            tarFeatureOr = GUILayout.Toggle(tarFeatureOr, "OR查询", new GUILayoutOption[0]);//默认AND查询方式
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal("box");
            GUILayout.Label("可教功法:", GUILayout.Width(60));
            actorGongFaText = GUILayout.TextField(actorGongFaText, 60, GUILayout.Width(120));
            tarGongFaOr = GUILayout.Toggle(tarGongFaOr, "OR查询", new GUILayoutOption[0]);//默认AND查询方式
            currentwidth += 180;
            currentwidth += 210;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 180;
                GUILayout.BeginHorizontal("box");
            }

            GUILayout.Label("最高查询品级:", GUILayout.Width(120));
            int.TryParse(GUILayout.TextField(highestLevel.ToString(), 1, GUILayout.Width(60)), out highestLevel);
            tarIsGang = GUILayout.Toggle(tarIsGang, "是否开启识别门派", new GUILayoutOption[0]);
            isGang = GUILayout.Toggle(isGang, "仅搜索门派", new GUILayoutOption[0]);

            //Main.Logger.Log(tarFeature.ToString());
            GUILayout.Space(30);
            currentwidth += 150;
            if (currentwidth >= windowWidth())
            {
                GUILayout.EndHorizontal();
                currentwidth = 150;
                GUILayout.BeginHorizontal("box");
            }
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
                if (!rankmode)
                {
                    ScanNpc();
                    showlistshrinked = true;
                    showlistadded = false;
                }
                else
                {
                    GetRank();
                    showlistadded = true;
                    showlistshrinked = false;
                }
            }
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

        internal bool GameCursorLocked { get; set; }

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

        private int GetLevelValue(int id, int index, int gongfa)
        {
            int num;
            if (getreal)
            {
                num = int.Parse(DateFile.instance.GetActorDate(id, 501 + index + 100 * gongfa, false));
                int age = int.Parse(DateFile.instance.GetActorDate(id, 11, false));
                if (age < 14 && age > 0)
                {
                    num = num * (1400 / age) / 100;
                }
            }
            else
            {
                num = int.Parse(DateFile.instance.GetActorDate(id, 501 + index + 100 * gongfa, true));
            }
            return num;
        }

        private string GetLevel(int id, int index, int gongfa)
        {
            int colorCorrect = 40;
            int num = GetLevelValue(id, index, gongfa);
            string text = DateFile.instance.SetColoer(20002 + Mathf.Clamp((num - colorCorrect) / 10, 0, 8), num.ToString(), false);
            return text;
        }


        static string GetHealth(int key, int value = 0)
        {
            int num = ActorMenu.instance.Health(key);
            int num2 = ActorMenu.instance.MaxHealth(key);
            if (value > 0 && num2 - num > value)
            {
                value = Mathf.Max(value / 5, 1);
            }
            int num3 = Mathf.Clamp(num + value, 0, num2);
            if (int.Parse(DateFile.instance.GetActorDate(key, 26, false)) != 0)
            {
                num2 = num3 = 0;
            }
            DateFile.instance.actorsDate[key][12] = num3.ToString();
            if (int.Parse(DateFile.instance.GetActorDate(key, 8, false)) != 1)
            {
                return "??? / ???";
            }
            else
            {
                return string.Format("{0}{1}</color> / {2}", ActorMenu.instance.Color3(num3, num2), num3, num2);
            }
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
                    if (s1.Contains("("))
                    {
                        s1 = System.Text.RegularExpressions.Regex.Replace(s1, "\\(.*?\\)", "");
                        s2 = System.Text.RegularExpressions.Regex.Replace(s2, "\\(.*?\\)", "");
                    }
                    else
                    {
                        s1 = System.Text.RegularExpressions.Regex.Replace(s1, "<.*?>", "");
                        s2 = System.Text.RegularExpressions.Regex.Replace(s2, "<.*?>", "");
                    }
                }
                int.TryParse(s1, out m);
                int.TryParse(s2, out n);
                if (desc)
                {
                    if (m > n)
                    {
                        return -1;
                    }
                    else if (m < n)
                    {
                        return 1;
                    }
                }
                else
                {
                    if (m > n)
                    {
                        return 1;
                    }
                    else if (m < n)
                    {
                        return -1;
                    }
                }
                if (desc)
                {
                    return -s1.CompareTo(s2);
                }
                else
                {
                    return s1.CompareTo(s2);
                }
            }
            else
            {
                Regex reg = new Regex("<(.+?)>");
                Match m1 = reg.Match(s1);
                Match m2 = reg.Match(s2);
                s1 = m1.Groups[0].Value;
                s2 = m2.Groups[0].Value;
                Main.Logger.Log(s1 + "," + s2);
                m = Main.colorText[s1];
                n = Main.colorText[s2];
                if (desc)
                {
                    if (m > n)
                    {
                        return -1;
                    }
                    else if (m < n)
                    {
                        return 1;
                    }
                }
                else
                {
                    if (m > n)
                    {
                        return 1;
                    }
                    else if (m < n)
                    {
                        return -1;
                    }
                }
                return 0;
            }
        }

        void ScanNpc()
        {
            actorList.Clear();
            DateFile dateFile = DateFile.instance;
            Dictionary<int, Dictionary<int, string>> actors = dateFile.actorsDate;
            foreach (int index in actors.Keys)
            {
                Dictionary<int, string> actor = actors[index];
                int str = int.Parse(DateFile.instance.GetActorDate(index, 61, !getreal)); //dateFile.BaseAttr(index, 0, 0);
                int con = int.Parse(DateFile.instance.GetActorDate(index, 62, !getreal)); //dateFile.BaseAttr(index, 1, 0);
                int agi = int.Parse(DateFile.instance.GetActorDate(index, 63, !getreal)); //dateFile.BaseAttr(index, 2, 0);
                int bon = int.Parse(DateFile.instance.GetActorDate(index, 64, !getreal)); //dateFile.BaseAttr(index, 3, 0);
                int inv = int.Parse(DateFile.instance.GetActorDate(index, 65, !getreal)); //dateFile.BaseAttr(index, 4, 0);
                int pat = int.Parse(DateFile.instance.GetActorDate(index, 66, !getreal)); //dateFile.BaseAttr(index, 5, 0);

                int age = int.Parse(dateFile.GetActorDate(index, 11, false));
                int gender = int.Parse(dateFile.GetActorDate(index, 14, false));
                int charm = int.Parse(DateFile.instance.GetActorDate(index, 15, !getreal));
                int samsara = dateFile.GetLifeDateList(index, 801, false).Count;
                int health = int.Parse(DateFile.instance.GetActorDate(index, 26, false)) == 0 ? ActorMenu.instance.Health(index) : 0;
                int cv = charmValue;
                if (charmValue == 0)
                {
                    cv = -999;
                }

                if (inv >= intValue
                    && str >= strValue
                    && con >= conValue
                    && agi >= agiValue
                    && bon >= bonValue
                    && pat >= patValue
                    && charm >= cv
                    && age >= minage
                    && health >= healthValue
                    && samsara >= samsaraCount
                    && (maxage == 0 || age <= maxage)
                    && (genderValue == 0 || gender == genderValue)
                    )
                {
                    string genderText;
                    if (gender == 1)
                    {
                        genderText = "男";
                    }
                    else
                    {
                        genderText = "女";

                    }
                    string place;
                    if (int.Parse(dateFile.GetActorDate(index, 8, false)) != 1)
                    {
                        place = dateFile.massageDate[8010][3].Split(new char[] { '|' })[1];
                    }
                    else
                    {
                        List<int> list = new List<int>(dateFile.GetActorAtPlace(index));
                        place = string.Format("{0}{1}", new object[]
                        {
                            dateFile.GetNewMapDate(list[0], list[1], 98),
                            dateFile.GetNewMapDate(list[0], list[1], 0)
                        });
                    }

                    string actorName = dateFile.GetActorName(index);

                    string charmText = ((int.Parse(DateFile.instance.GetActorDate(index, 11, false)) > 14) ? ((int.Parse(DateFile.instance.GetActorDate(index, 8, false)) != 1 || int.Parse(DateFile.instance.GetActorDate(index, 305, false)) != 0) ? DateFile.instance.massageDate[25][int.Parse(DateFile.instance.GetActorDate(index, 14, false)) - 1].Split(new char[]
                    {
                            '|'
                    })[Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(index, 15, true)) / 100, 0, 9)] : DateFile.instance.massageDate[25][5].Split(new char[]
                    {
                            '|'
                    })[1]) : DateFile.instance.massageDate[25][5].Split(new char[]
                    {
                            '|'
                    })[0]);

                    List<int> samsaraList = dateFile.GetLifeDateList(index, 801, false);
                    string samsaraNames = "";
                    foreach (int samsaraId in samsaraList)
                    {
                        samsaraNames = samsaraNames + " " + dateFile.GetActorName(samsaraId);
                    }

                    int[] actorResources = ActorMenu.instance.GetActorResources(index);

                    if (GetLevelValue(index, 0, 1) >= gongfa[0]
                        && GetLevelValue(index, 1, 1) >= gongfa[1]
                        && GetLevelValue(index, 1, 1) >= gongfa[1]
                        && GetLevelValue(index, 2, 1) >= gongfa[2]
                        && GetLevelValue(index, 3, 1) >= gongfa[3]
                        && GetLevelValue(index, 4, 1) >= gongfa[4]
                        && GetLevelValue(index, 5, 1) >= gongfa[5]
                        && GetLevelValue(index, 6, 1) >= gongfa[6]
                        && GetLevelValue(index, 7, 1) >= gongfa[7]
                        && GetLevelValue(index, 8, 1) >= gongfa[8]
                        && GetLevelValue(index, 9, 1) >= gongfa[9]
                        && GetLevelValue(index, 10, 1) >= gongfa[10]
                        && GetLevelValue(index, 11, 1) >= gongfa[11]
                        && GetLevelValue(index, 12, 1) >= gongfa[12]
                        && GetLevelValue(index, 13, 1) >= gongfa[13]
                        && GetLevelValue(index, 0, 0) >= life[0]
                        && GetLevelValue(index, 1, 0) >= life[1]
                        && GetLevelValue(index, 2, 0) >= life[2]
                        && GetLevelValue(index, 3, 0) >= life[3]
                        && GetLevelValue(index, 4, 0) >= life[4]
                        && GetLevelValue(index, 5, 0) >= life[5]
                        && GetLevelValue(index, 6, 0) >= life[6]
                        && GetLevelValue(index, 7, 0) >= life[7]
                        && GetLevelValue(index, 8, 0) >= life[8]
                        && GetLevelValue(index, 9, 0) >= life[9]
                        && GetLevelValue(index, 10, 0) >= life[10]
                        && GetLevelValue(index, 11, 0) >= life[11]
                        && GetLevelValue(index, 12, 0) >= life[12]
                        && GetLevelValue(index, 13, 0) >= life[13]
                        && GetLevelValue(index, 14, 0) >= life[14]
                        && GetLevelValue(index, 15, 0) >= life[15]
                        )
                    {
                        string gn = dateFile.massageDate[9][0].Split(new char[] { '|' })[DateFile.instance.GetActorGoodness(index)];
                        int groupid = int.Parse(DateFile.instance.GetActorDate(index, 19, false));//身份组ID
                        int gangLevel = int.Parse(DateFile.instance.GetActorDate(index, 20, false));//身份等级
                        int gangValueId = DateFile.instance.GetGangValueId(groupid, gangLevel);
                        int key2 = (gangLevel >= 0) ? 1001 : (1001 + int.Parse(DateFile.instance.GetActorDate(index, 14, false)));//性别标识
                        string gangLevelText = dateFile.SetColoer((gangValueId != 0) ? (20011 - Mathf.Abs(gangLevel)) : 20002, DateFile.instance.presetGangGroupDateValue[gangValueId][key2], false);//身份gangLevelText

                        if (ScanFeature(index, Main.findList, tarFeature, tarFeatureOr) || actorFeatureText == "")
                        {
                            if (ScanGongFa(index, Main.GongFaList, tarGongFaOr) || actorGongFaText == "")
                            {
                                if (gangLevel >= highestLevel)
                                {
                                    if ((!tarIsGang || (isGang == (groupid >= 1 && groupid <= 15))) && gangValueId != 0)
                                    {
                                        if (goodnessText.Equals("全部") || gn.Contains(goodnessText))
                                        {
                                            if ((actorName.Contains(aName) || samsaraNames.Contains(aName)) && (dateFile.GetGangDate(groupid, 0).Contains(gangValue)) && (gangLevelText.Contains(gangLevelValue)))
                                            {
                                                actorList.Add(new string[] { actorName ,age.ToString(), genderText, place,
                                charm + "(" + charmText + ")" ,//魅力
                                dateFile.GetGangDate(groupid, 0),//从属gangText
                                gangLevelText,//身份gangLevelText
                                gn,//立场goodnessText
                                GetSpouse(index),//
                                GetSkillDevelopText(index),
                                GetGongFaDevelopText(index),
                                GetHealth(index),//健康
                                str.ToString(), con.ToString(), agi.ToString(), bon.ToString(), inv.ToString(), pat.ToString(),
                                GetLevel(index, 0, 1),
                                GetLevel(index, 1, 1) ,
                                GetLevel(index, 2, 1),
                                GetLevel(index, 3, 1) ,
                                GetLevel(index, 4, 1) ,
                                GetLevel(index, 5, 1) ,
                                GetLevel(index, 6, 1) ,
                                GetLevel(index, 7, 1) ,
                                GetLevel(index, 8, 1) ,
                                GetLevel(index, 9, 1) ,
                                GetLevel(index, 10, 1) ,
                                GetLevel(index, 11, 1) ,
                                GetLevel(index, 12, 1) ,
                                GetLevel(index, 13, 1),
                                GetLevel(index, 0, 0),
                                GetLevel(index, 1, 0) ,
                                GetLevel(index, 2, 0),
                                GetLevel(index, 3, 0) ,
                                GetLevel(index, 4, 0) ,
                                GetLevel(index, 5, 0) ,
                                GetLevel(index, 6, 0) ,
                                GetLevel(index, 7, 0) ,
                                GetLevel(index, 8, 0) ,
                                GetLevel(index, 9, 0) ,
                                GetLevel(index, 10, 0) ,
                                GetLevel(index, 11, 0) ,
                                GetLevel(index, 12, 0) ,
                                GetLevel(index, 13, 0),
                                GetLevel(index, 14, 0),
                                GetLevel(index, 15, 0),
                                actorResources[0].ToString(),
                                actorResources[1].ToString(),
                                actorResources[2].ToString(),
                                actorResources[3].ToString(),
                                actorResources[4].ToString(),
                                actorResources[5].ToString(),
                                actorResources[6].ToString(),
                                GetGongFaListText(index),
                                samsaraNames,
                                GetActorFeatureNameText(index, tarFeature)});
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        void GetRank()
        {
            actorList.Clear();
            DateFile dateFile = DateFile.instance;
            Dictionary<int, Dictionary<int, string>> actors = dateFile.actorsDate;
            foreach (int index in actors.Keys)
            {
                Dictionary<int, string> actor = actors[index];
                int age = int.Parse(dateFile.GetActorDate(index, 11, false));
                int gender = int.Parse(dateFile.GetActorDate(index, 14, false));
                int samsara = dateFile.GetLifeDateList(index, 801, false).Count;
                int health = ActorMenu.instance.Health(index);
                if (age >= minage
                    && health >= healthValue
                    && samsara >= samsaraCount
                    && (maxage == 0 || age <= maxage)
                    && (genderValue == 0 || gender == genderValue)
                    )
                {
                    int str = int.Parse(DateFile.instance.GetActorDate(index, 61, !getreal)); //dateFile.BaseAttr(index, 0, 0);
                    int con = int.Parse(DateFile.instance.GetActorDate(index, 62, !getreal)); //dateFile.BaseAttr(index, 1, 0);
                    int agi = int.Parse(DateFile.instance.GetActorDate(index, 63, !getreal)); //dateFile.BaseAttr(index, 2, 0);
                    int bon = int.Parse(DateFile.instance.GetActorDate(index, 64, !getreal)); //dateFile.BaseAttr(index, 3, 0);
                    int inv = int.Parse(DateFile.instance.GetActorDate(index, 65, !getreal)); //dateFile.BaseAttr(index, 4, 0);
                    int pat = int.Parse(DateFile.instance.GetActorDate(index, 66, !getreal)); //dateFile.BaseAttr(index, 5, 0);
                    int totalrank = str * strValue + con * conValue + agi * agiValue + bon * bonValue + inv * intValue + pat * patValue;
                    int charm = int.Parse(DateFile.instance.GetActorDate(index, 15, !getreal));
                    totalrank += charm * charmValue;
                    string genderText;
                    if (gender == 1)
                    {
                        genderText = "男";
                    }
                    else
                    {
                        genderText = "女";

                    }
                    string place;
                    if (int.Parse(dateFile.GetActorDate(index, 8, false)) != 1)
                    {
                        place = dateFile.massageDate[8010][3].Split(new char[] { '|' })[1];
                    }
                    else
                    {
                        List<int> list = new List<int>(dateFile.GetActorAtPlace(index));
                        place = string.Format("{0}{1}", new object[]
                        {
                            dateFile.GetNewMapDate(list[0], list[1], 98),
                            dateFile.GetNewMapDate(list[0], list[1], 0)
                        });
                    }

                    string actorName = dateFile.GetActorName(index);

                    string charmText = ((int.Parse(DateFile.instance.GetActorDate(index, 11, false)) > 14) ? ((int.Parse(DateFile.instance.GetActorDate(index, 8, false)) != 1 || int.Parse(DateFile.instance.GetActorDate(index, 305, false)) != 0) ? DateFile.instance.massageDate[25][int.Parse(DateFile.instance.GetActorDate(index, 14, false)) - 1].Split(new char[]
                    {
                            '|'
                    })[Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(index, 15, true)) / 100, 0, 9)] : DateFile.instance.massageDate[25][5].Split(new char[]
                    {
                            '|'
                    })[1]) : DateFile.instance.massageDate[25][5].Split(new char[]
                    {
                            '|'
                    })[0]);

                    List<int> samsaraList = dateFile.GetLifeDateList(index, 801, false);
                    string samsaraNames = "";
                    foreach (int samsaraId in samsaraList)
                    {
                        samsaraNames = samsaraNames + " " + dateFile.GetActorName(samsaraId);
                    }

                    int[] actorResources = ActorMenu.instance.GetActorResources(index);
                    for (int tmpi = 0; tmpi < 14; tmpi++)
                        totalrank += GetLevelValue(index, tmpi, 1) * gongfa[tmpi];
                    for (int tmpi = 0; tmpi < 16; tmpi++)
                        totalrank += GetLevelValue(index, tmpi, 0) * life[tmpi];
                    string gn = dateFile.massageDate[9][0].Split(new char[] { '|' })[DateFile.instance.GetActorGoodness(index)];
                    int groupid = int.Parse(DateFile.instance.GetActorDate(index, 19, false));//身份组ID
                    int gangLevel = int.Parse(DateFile.instance.GetActorDate(index, 20, false));//身份等级
                    int gangValueId = DateFile.instance.GetGangValueId(groupid, gangLevel);
                    int key2 = (gangLevel >= 0) ? 1001 : (1001 + int.Parse(DateFile.instance.GetActorDate(index, 14, false)));//性别标识
                    string gangLevelText = dateFile.SetColoer((gangValueId != 0) ? (20011 - Mathf.Abs(gangLevel)) : 20002, DateFile.instance.presetGangGroupDateValue[gangValueId][key2], false);//身份gangLevelText

                    if (ScanFeature(index, Main.findList, tarFeature, tarFeatureOr) || actorFeatureText == "")
                    {
                        if (ScanGongFa(index, Main.GongFaList, tarGongFaOr) || actorGongFaText == "")
                        {
                            if (gangLevel >= highestLevel)
                            {
                                if ((!tarIsGang || (isGang == (groupid >= 1 && groupid <= 15))) && gangValueId != 0)
                                {
                                    if (goodnessText.Equals("全部") || gn.Contains(goodnessText))
                                    {
                                        if ((actorName.Contains(aName) || samsaraNames.Contains(aName)) && (dateFile.GetGangDate(groupid, 0).Contains(gangValue)) && (gangLevelText.Contains(gangLevelValue)))
                                        {
                                            actorList.Add(new string[] { totalrank.ToString(), actorName ,age.ToString(), genderText, place,
                                charm + "(" + charmText + ")" ,//魅力
                                dateFile.GetGangDate(groupid, 0),//从属gangText
                                gangLevelText,//身份gangLevelText
                                gn,//立场goodnessText
                                GetSpouse(index),//
                                GetSkillDevelopText(index),
                                GetGongFaDevelopText(index),
                                GetHealth(index),//健康
                                str.ToString(), con.ToString(), agi.ToString(), bon.ToString(), inv.ToString(), pat.ToString(),
                                GetLevel(index, 0, 1),
                                GetLevel(index, 1, 1) ,
                                GetLevel(index, 2, 1),
                                GetLevel(index, 3, 1) ,
                                GetLevel(index, 4, 1) ,
                                GetLevel(index, 5, 1) ,
                                GetLevel(index, 6, 1) ,
                                GetLevel(index, 7, 1) ,
                                GetLevel(index, 8, 1) ,
                                GetLevel(index, 9, 1) ,
                                GetLevel(index, 10, 1) ,
                                GetLevel(index, 11, 1) ,
                                GetLevel(index, 12, 1) ,
                                GetLevel(index, 13, 1),
                                GetLevel(index, 0, 0),
                                GetLevel(index, 1, 0) ,
                                GetLevel(index, 2, 0),
                                GetLevel(index, 3, 0) ,
                                GetLevel(index, 4, 0) ,
                                GetLevel(index, 5, 0) ,
                                GetLevel(index, 6, 0) ,
                                GetLevel(index, 7, 0) ,
                                GetLevel(index, 8, 0) ,
                                GetLevel(index, 9, 0) ,
                                GetLevel(index, 10, 0) ,
                                GetLevel(index, 11, 0) ,
                                GetLevel(index, 12, 0) ,
                                GetLevel(index, 13, 0),
                                GetLevel(index, 14, 0),
                                GetLevel(index, 15, 0),
                                actorResources[0].ToString(),
                                actorResources[1].ToString(),
                                actorResources[2].ToString(),
                                actorResources[3].ToString(),
                                actorResources[4].ToString(),
                                actorResources[5].ToString(),
                                actorResources[6].ToString(),
                                GetGongFaListText(index),
                                samsaraNames,
                                GetActorFeatureNameText(index, tarFeature) });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        //婚姻状况
        public static string GetSpouse(int id)
        {
            List<int> actorSocial = DateFile.instance.GetActorSocial(id, 309, false, false);
            List<int> actorSocial2 = DateFile.instance.GetActorSocial(id, 309, true, false);
            bool flag = actorSocial2.Count == 0;
            string result;
            if (flag)
            {
                result = DateFile.instance.SetColoer(20004, "未婚", false);
            }
            else
            {
                bool flag2 = actorSocial.Count == 0;
                if (flag2)
                {
                    result = DateFile.instance.SetColoer(20007, "丧偶", false);
                }
                else
                {
                    result = DateFile.instance.SetColoer(20010, "已婚", false);
                }
            }

            return result;
        }

        private static string GetSkillDevelopText(int key)
        {
            int num = DateFile.instance.MianActorID();
            int num2 = Mathf.Max(int.Parse(DateFile.instance.GetActorDate(key, 551, false)), 2);
            int num3 = int.Parse(DateFile.instance.ageDate[Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(key, 11, false)), 0, 100)][num2]);
            string text = ((num2 == 0) ? DateFile.instance.massageDate[7006][0] : string.Format("{0} {1}", DateFile.instance.massageDate[2002][2].Split(new char[]
            {
            '|'
            })[num2], (num3 <= 0) ? ((num3 != 0) ? DateFile.instance.SetColoer(20010, "-" + Mathf.Abs(num3), false) : DateFile.instance.SetColoer(20002, "+" + num3, false)) : DateFile.instance.SetColoer(20005, "+" + num3, false)));
            return text;
        }
        private static string GetGongFaDevelopText(int key)
        {
            int num = DateFile.instance.MianActorID();
            int num2 = Mathf.Max(int.Parse(DateFile.instance.GetActorDate(key, 651, false)), 2);
            int num3 = int.Parse(DateFile.instance.ageDate[Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(key, 11, false)), 0, 100)][num2 + 3]);
            string text = ((num2 == 0) ? DateFile.instance.massageDate[7006][0] : string.Format("{0} {1}", DateFile.instance.massageDate[2002][2].Split(new char[]
            {
            '|'
            })[num2], (num3 <= 0) ? ((num3 != 0) ? DateFile.instance.SetColoer(20010, "-" + Mathf.Abs(num3), false) : DateFile.instance.SetColoer(20002, "+" + num3, false)) : DateFile.instance.SetColoer(20005, "+" + num3, false)));
            return text;
        }
        private static string GetActorFeatureNameText(int key, bool tarFeature)
        {
            List<int> list = new List<int>(DateFile.instance.GetActorFeature(key));
            string text = "";
            for (int i = 0; i < list.Count; i++)
            {
                int j = list[i];
                Features f = Main.featuresList[j];
                if (f.Group == 2001 || f.Group == 3024) continue;
                string s = f.Level.ToString();
                if (tarFeature)
                {
                    text += (Main.findList.Contains(f) ? f.tarColor : f.Color) + f.Name + "(" + s + ")</color>";
                }
                else
                {
                    text += (Main.findList.Contains(Main.featuresList[f.Group]) ? f.tarColor : f.Color) + f.Name + "(" + s + ")</color>";
                }             
            }
            //Main.Logger.Log(text);
            return text;
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

        private static bool ScanGongFa(int key, List<int> slist, bool tarGongFaOr)
        {
            List<int> list = new List<int>(DateFile.instance.actorGongFas[key].Keys);
            bool result = false;
            if (slist.Count == 0)
            {
                return true;
            }
            //List<int> actorGongFa = new List<int>();
            //foreach (int i in list)
            //{
               // actorGongFa.Add(Main.GongFaList[i]);
            //}

            if (!tarGongFaOr)   //与查找
            {
                if (slist.All(t => list.Any(b => b == t)))
                {
                    result = true;
                }
            }
            else                //或查找
            {
                foreach (int i in list)
                {
                    if (slist.Contains(i))
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        private static String GetGongFaListText(int key)
        {
            String result = "";
            List<int> myList = new List<int>(DateFile.instance.actorGongFas[DateFile.instance.MianActorID()].Keys);
            List<int> list = new List<int>(DateFile.instance.actorGongFas[key].Keys);
            int[] resultList = {0, 0, 0, 0, 0, 0, 0, 0, 0};
            foreach(int i in list)
            {
                if(myList.All(t => t != i))
                {
                    resultList[9 - int.Parse(DateFile.instance.gongFaDate[i][2])]++;
                }
            }
            for(int i = 0; i < 9; i++)
            {
                result += DateFile.instance.SetColoer(20010 - i, String.Format("{0:D2}", resultList[i]));
                if (i != 8)
                    result += " | ";
            }
            return result;
        }

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
