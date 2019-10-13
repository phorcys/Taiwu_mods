using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using GameData;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;

namespace LKX_BookMerge
{
    /// <summary>
    /// 设置文件
    /// </summary>
    public class Settings : UnityModManager.ModSettings
    {
        /// <summary>
        /// 合并书籍类型
        /// </summary>
        public List<int> bookMergeType = new List<int>();

        /// <summary>
        /// 合并书籍品级
        /// </summary>
        public List<int> bookLevel = new List<int>();

        /// <summary>
        /// 自动合并
        /// </summary>
        public bool autoMerge;

        /// <summary>
        /// 是否使用系统难度计算成功率
        /// </summary>
        public bool noSystemObbs;

        /// <summary>
        /// 手动难度成功率类型
        /// </summary>
        public uint bookMergeObbsType;

        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="modEntry"></param>
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    public class Main
    {
        /// <summary>
        /// umm日志
        /// </summary>
        public static UnityModManager.ModEntry.ModLogger logger;

        /// <summary>
        /// mod设置
        /// </summary>
        public static Settings settings;

        /// <summary>
        /// 是否开启mod
        /// </summary>
        public static bool enabled;

        /// <summary>
        /// Key是道具分类 Value是对应人物属性index
        /// </summary>
        public static Dictionary<int, int> booksToActorMap = new Dictionary<int, int> {
            //书籍
            { 179, 501 }, { 180, 502 }, { 181, 503 }, { 182, 504 }, { 183, 505 },
            { 184, 506 }, { 185, 507 }, { 186, 508 }, { 187, 509 }, { 188, 510 },
            { 189, 511 }, { 190, 512 }, { 191, 513 }, { 192, 514 }, { 193, 515 },
            { 194, 516 },
            //功法真传与手抄
            { 195, 601 }, { 196, 602 }, { 197, 603 }, { 198, 604 }, { 199, 605 },
            { 200, 606 }, { 201, 607 }, { 202, 608 }, { 203, 609 }, { 204, 610 },
            { 205, 611 }, { 206, 612 }, { 207, 613 }, { 208, 614 },
        };
        
        /// <summary>
        /// enemyBorn战斗等级，xxLevel相怄入侵速度，enemySize外道数量，worldResource世界资源等级，randomHeir是否随机继承人
        /// </summary>
        public static List<int> LevelSon = new List<int>();

        /// <summary>
        /// 载入mod。
        /// </summary>
        /// <param name="modEntry">mod管理器对象</param>
        /// <returns></returns>
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Main.logger = modEntry.Logger;
            Main.settings = Settings.Load<Settings>(modEntry);

            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());

            modEntry.OnToggle = Main.OnToggle;
            modEntry.OnGUI = Main.OnGUI;
            modEntry.OnSaveGUI = Main.OnSaveGUI;

            return true;
        }

        /// <summary>
        /// 确定是否激活mod
        /// </summary>
        /// <param name="modEntry">umm</param>
        /// <param name="value">是否激活</param>
        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Main.enabled = value;
            return true;
        }

        /// <summary>
        /// 展示mod的设置
        /// </summary>
        /// <param name="modEntry">umm</param>
        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUIStyle redLabelStyle = new GUIStyle();
            redLabelStyle.normal.textColor = new Color(204f / 256f, 0f / 256f, 1f / 256f);
            GUILayout.Label("如果status亮红灯代表mod失效！", redLabelStyle);

            GUILayout.Label("选择书籍合并的成功的难度");
            int systemObbsNum = GUILayout.SelectionGrid(Convert.ToUInt16(Main.settings.noSystemObbs), new string[] { "系统难度", "手动设置" }, 2);
            if (GUI.changed) Main.settings.noSystemObbs = Convert.ToBoolean(systemObbsNum);
            
            if (Main.settings.noSystemObbs)
            {
                Main.settings.bookMergeObbsType = (uint) Mathf.Clamp(int.Parse(Main.settings.bookMergeObbsType.ToString()), 0, 3);
                Main.settings.bookMergeObbsType = (uint) GUILayout.SelectionGrid((int) Main.settings.bookMergeObbsType, new string[] { "偏向作弊", "简单", "普通", "困难"}, 4);
                List<string> obbsLabels = new List<string> { "只要2本对应页码都不是残页就能成功合成", "有一点的失败几率一品成功率最高为", "可以接受的失败几率一品成功率最高为", "几乎都是失败的几率一品成功率最高为" };
                GUILayout.Label(obbsLabels[(int) Main.settings.bookMergeObbsType]);
            }

            if (GUILayout.Button("点击合并"))
            {
                Main.RunningMergeItems();
            }
            Main.settings.autoMerge = GUILayout.Toggle(Main.settings.autoMerge, "开启更换时节自动合并");

            GUILayout.Label("请选择允许合并的书籍类型，必选");
            GUILayout.BeginHorizontal("Box", new GUILayoutOption[0]);
            GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
            GUILayout.Label("技艺书籍");
            Main.SetGUIToToggle(179, "音律", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(180, "弈棋", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(181, "诗书", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(182, "绘画", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(183, "术数", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(184, "品鉴", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(185, "锻造", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(186, "制木", ref Main.settings.bookMergeType);
            GUILayout.EndVertical();
            GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
            GUILayout.Label("技艺书籍");
            Main.SetGUIToToggle(187, "医术", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(188, "毒术", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(189, "织锦", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(190, "巧匠", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(191, "道法", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(192, "佛学", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(193, "厨艺", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(194, "杂学", ref Main.settings.bookMergeType);
            GUILayout.EndVertical();
            GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
            GUILayout.Label("功法书籍");
            Main.SetGUIToToggle(195, "内功", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(196, "身法", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(197, "绝技", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(198, "拳法", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(199, "指法", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(200, "腿法", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(201, "暗器", ref Main.settings.bookMergeType);
            GUILayout.EndVertical();
            GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
            GUILayout.Label("功法书籍");
            Main.SetGUIToToggle(202, "剑法", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(203, "刀法", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(204, "长兵", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(205, "奇门", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(206, "软兵", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(207, "御射", ref Main.settings.bookMergeType);
            Main.SetGUIToToggle(208, "乐器", ref Main.settings.bookMergeType);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Label("选择允许合并的品级。必选");
            GUILayout.BeginHorizontal("Box", new GUILayoutOption[0]);
            Main.SetGUIToToggle(1, "九品", ref Main.settings.bookLevel);
            Main.SetGUIToToggle(2, "八品", ref Main.settings.bookLevel);
            Main.SetGUIToToggle(3, "七品", ref Main.settings.bookLevel);
            Main.SetGUIToToggle(4, "六品", ref Main.settings.bookLevel);
            Main.SetGUIToToggle(5, "五品", ref Main.settings.bookLevel);
            Main.SetGUIToToggle(6, "四品", ref Main.settings.bookLevel);
            Main.SetGUIToToggle(7, "三品", ref Main.settings.bookLevel);
            Main.SetGUIToToggle(8, "二品", ref Main.settings.bookLevel);
            Main.SetGUIToToggle(9, "一品", ref Main.settings.bookLevel);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 保存mod的设置
        /// </summary>
        /// <param name="modEntry">umm</param>
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Main.settings.Save(modEntry);
        }

        /// <summary>
        /// 生成GUI开关
        /// </summary>
        /// <param name="index">道具类型index 看item的index 5</param>
        /// <param name="name">开关名称</param>
        static void SetGUIToToggle(int index, string name, ref List<int> field)
        {
            bool status = GUILayout.Toggle(field.Contains(index), name);
            if (GUI.changed)
            {
                if (status)
                {
                    if (!field.Contains(index)) field.Add(index);
                }
                else
                {
                    if (field.Contains(index)) field.Remove(index);
                }
            }
        }

        /// <summary>
        /// 执行合并道具
        /// </summary>
        public static void RunningMergeItems()
        {
            if (!Main.enabled || Main.settings.bookLevel.Count == 0 || Main.settings.bookMergeType.Count == 0) return;
            
            DateFile df = DateFile.instance;
            Dictionary<int, string[]> items = new Dictionary<int, string[]>();
            Dictionary<int, int> itemsId = new Dictionary<int, int>();
            if (df.actorItemsDate.TryGetValue(df.mianActorId, out itemsId))
            {
                Main.LevelSon = new List<int> {
                    DateFile.instance.enemyBorn, DateFile.instance.xxLevel, DateFile.instance.enemySize,
                    DateFile.instance.worldResource, DateFile.instance.randomHeir
                };

                //处理人物身上所有书籍
                List<int> buffer = itemsId.Keys.ToList();
                foreach (int itemId in buffer)
                {
                    string id = df.GetItemDate(itemId, 999), surpluses = df.GetItemDate(itemId, 901), limit = df.GetItemDate(itemId, 902),
                        level = df.GetItemDate(itemId, 8), type = df.GetItemDate(itemId, 5), group = df.GetItemDate(itemId, 98);
                    if (type != "21" || !Main.settings.bookMergeType.Contains(int.Parse(group))) continue;
                    if (!Main.settings.bookLevel.Contains(int.Parse(level)) || id == "5005") continue;
                    
                    if (items.ContainsKey(int.Parse(id)))
                    {
                        items[int.Parse(id)][0] = (int.Parse(surpluses) + int.Parse(items[int.Parse(id)][0])).ToString();
                        items[int.Parse(id)][1] = (int.Parse(limit) + int.Parse(items[int.Parse(id)][1])).ToString();
                        items[int.Parse(id)][2] = ProcessingPages(df.GetItemDate(itemId, 33), items[int.Parse(id)][2], int.Parse(group), int.Parse(level));
                        //items[int.Parse(id)][2] = ProcessingPages(df.itemsDate[itemId][33], items[int.Parse(id)][2], int.Parse(group), int.Parse(level));
                    }
                    else
                    {
                        //items.Add(int.Parse(id), new string[] { surpluses, limit, df.itemsDate[itemId][33] });
                        items.Add(int.Parse(id), new string[] { surpluses, limit, df.GetItemDate(itemId, 33) });
                    }

                    df.LoseItem(df.mianActorId, itemId, itemsId[itemId], true);
                }

                //生成合并后的书籍
                foreach (KeyValuePair<int, string[]> item in items)
                {
                    int makeItemId = df.MakeNewItem(item.Key);
                    df.GetItem(df.mianActorId, makeItemId, 1, false, -1, 0);
                    Items.SetItemProperty(makeItemId, 901, item.Value[0]);
                    Items.SetItemProperty(makeItemId, 902, item.Value[1]);
                    Items.SetItemProperty(makeItemId, 33, item.Value[2]);
                    //df.itemsDate[makeItemId][901] = item.Value[0];
                    //df.itemsDate[makeItemId][902] = item.Value[1];
                    //df.itemsDate[makeItemId][33] = item.Value[2];
                }
            }
        }

        /// <summary>
        /// 处理残页，计算成功率，成功的话该页就不再是残章。
        /// </summary>
        /// <param name="newData">新的功法书籍</param>
        /// <param name="oldData">旧的功法书籍</param>
        /// <param name="group">类型组</param>
        /// <param name="level">书籍品级</param>
        /// <returns>处理好的残页字符串</returns>
        public static string ProcessingPages(string newData, string oldData, int group, int level)
        {
            List<string> res = new List<string>();
            string[] isNew = newData.Split('|'), isOld = oldData.Split('|');
            int baseObbs, offsetObbs, obbsSum;

            baseObbs = GetBaseObbs(group, level);
            offsetObbs = GetOffsetObbs(level);
            obbsSum = baseObbs + offsetObbs;
            for (int i = 0; i < isOld.Length; i++)
            {
                if (isOld[i] == "0" && isNew[i] != "0")
                {
                    if (Main.settings.bookMergeObbsType == 0 && Main.settings.noSystemObbs)
                    {
                        res.Add("1");
                        continue;
                    }
                    int randomCount = UnityEngine.Random.Range(1, 100);
                    res.Add(obbsSum >= randomCount ? "1" : "0");
                }
                else
                {
                    res.Add(isOld[i]);
                }
            }
            
            return string.Join("|", res.ToArray());
        }

        /// <summary>
        /// 获得合并难度减免的倍数
        /// </summary>
        /// <returns>返回减免的倍数</returns>
        public static int SwitchObbsType()
        {
            int obbsType, selectNum;
            if (Main.settings.noSystemObbs)
            {
                selectNum = (int)Main.settings.bookMergeObbsType;
            }
            else
            {
                selectNum = (int)Main.LevelSon.Average();
            }

            switch (selectNum)
            {
                case 0:
                case 1:
                    obbsType = 3;
                    break;
                case 2:
                    obbsType = 2;
                    break;
                case 3:
                default:
                    obbsType = 1;
                    break;
            }

            return obbsType;
        }

        /// <summary>
        /// 获得补偿的成功率
        /// </summary>
        /// <param name="level">书籍品级</param>
        /// <returns>计算好的值</returns>
        public static int GetOffsetObbs(int level)
        {
            int multipleCount = SwitchObbsType();
            double x = level, y = multipleCount;
            
            return (int)(100 / (x / y));
        }

        /// <summary>
        /// 获得基础成功率
        /// </summary>
        /// <param name="group">书籍类型</param>
        /// <param name="level">书籍品级</param>
        /// <returns>计算好的成功率</returns>
        public static int GetBaseObbs(int group, int level)
        {
            int zhiZhi, wuXing, multipleCount;
            zhiZhi = wuXing = multipleCount = 0;

            multipleCount = SwitchObbsType();
            if (multipleCount <= 2)
            {
                //后天属性
                zhiZhi = int.Parse(DateFile.instance.GetActorDate(DateFile.instance.mianActorId, Main.booksToActorMap[group], true));
                wuXing = DateFile.instance.BaseAttr(DateFile.instance.mianActorId, 4, 0);
            }
            else
            {
                //先天属性
                //zhiZhi = int.Parse(DateFile.instance.actorsDate[DateFile.instance.mianActorId][Main.booksToActorMap[group]]);
                //wuXing = int.Parse(DateFile.instance.actorsDate[DateFile.instance.mianActorId][65]);
                zhiZhi = int.Parse(Characters.GetCharProperty(DateFile.instance.mianActorId, Main.booksToActorMap[group]));
                wuXing = int.Parse(Characters.GetCharProperty(DateFile.instance.mianActorId, 65));
            }

            double x = level, y = multipleCount;

            return (int) ((zhiZhi + wuXing) / (x / y));
        }
    }

    /// <summary>
    /// 时节更换
    /// </summary>
    [HarmonyPatch(typeof(UIDate), "DoChangeTrun")]
    public class AutoMergeBook_For_UIDate_DoChangeTrun
    {
        static void Prefix()
        {
            if (Main.enabled && Main.settings.autoMerge) Main.RunningMergeItems();
        }
    }
}
