using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using GameData;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;

/// <summary>
/// 作弊用的mod
/// </summary>
namespace LKX_CheatMaxPeople
{
    /// <summary>
    /// 设置文件
    /// </summary>
    public class Settings : UnityModManager.ModSettings
    {
        /// <summary>
        /// 人口
        /// </summary>
        public int maxPeople;

        /// <summary>
        /// 蛐蛐时节开启
        /// </summary>
        public bool ququLife;

        /// <summary>
        /// 食物合并作弊
        /// </summary>
        public bool foodMerge;

        public List<int> foodLevel = new List<int>();

        public List<int> ququList = new List<int>();

        public bool cheatWarehouseMaxSize;

        public int eventId;

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

        public static Dictionary<int, string[]> defaultQuQuVoice = new Dictionary<int, string[]>();

        public static Dictionary<int, string> quQuGuiList = new Dictionary<int, string>();

        public static List<int> teamActorId = new List<int>();

        public static int selectActorId;

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
            redLabelStyle.normal.textColor = new Color(159f / 256f, 20f / 256f, 29f / 256f);
            GUILayout.Label("修改人力上限！", redLabelStyle);

            string maxPeople = GUILayout.TextField(Main.settings.maxPeople.ToString());
            if (GUI.changed)
            {
                if (string.Empty == maxPeople) maxPeople = "0";

                Main.settings.maxPeople = int.Parse(maxPeople);
            }

            Main.settings.ququLife = GUILayout.Toggle(Main.settings.ququLife, "蛐蛐无限寿命，已死的不复活。");
            Main.settings.foodMerge = GUILayout.Toggle(Main.settings.foodMerge, "开启合并食物。");
            Main.settings.cheatWarehouseMaxSize = GUILayout.Toggle(Main.settings.cheatWarehouseMaxSize, "开启无限仓库容量。");
            GUILayout.BeginHorizontal("Box", new GUILayoutOption[0]);
            Main.SetGUIToToggle(1, "九品", ref Main.settings.foodLevel);
            Main.SetGUIToToggle(2, "八品", ref Main.settings.foodLevel);
            Main.SetGUIToToggle(3, "七品", ref Main.settings.foodLevel);
            Main.SetGUIToToggle(4, "六品", ref Main.settings.foodLevel);
            Main.SetGUIToToggle(5, "五品", ref Main.settings.foodLevel);
            Main.SetGUIToToggle(6, "四品", ref Main.settings.foodLevel);
            Main.SetGUIToToggle(7, "三品", ref Main.settings.foodLevel);
            Main.SetGUIToToggle(8, "二品", ref Main.settings.foodLevel);
            Main.SetGUIToToggle(9, "一品", ref Main.settings.foodLevel);
            GUILayout.EndHorizontal();

            

            GUILayout.BeginHorizontal("Box", new GUILayoutOption[0]);
            if (GUILayout.Button("获取蛐蛐列表"))
            {
                if (DateFile.instance == null)
                {
                    GUILayout.Label("获取失败：未进入存档");
                }
                else
                {
                    Main.quQuGuiList = Main.GetQuQuData();
                }
            }
            if (GUILayout.Button("修改蛐蛐叫声"))
            {
                if (DateFile.instance == null)
                {
                    GUILayout.Label("修改失败：未进入存档");
                }
                else
                {
                    Main.TestModValue();
                }
            }
            if (GUILayout.Button("恢复全部蛐蛐叫声"))
            {
                Main.ResetQuQuData();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box", new GUILayoutOption[0]);
            if (GUILayout.Button("列出蛐蛐叫声"))
            {
                foreach (KeyValuePair<int, Dictionary<int, string>> jiaosheng in DateFile.instance.cricketDate)
                {
                    Main.logger.Log(jiaosheng.Key.ToString() + ":{" + jiaosheng.Value[0] + ":" + jiaosheng.Value[9] + "}");
                }
            }
            if (GUILayout.Button("蛐蛐buff为1000"))
            {
                Main.logger.Log(DateFile.instance.storyBuffs[-7].ToString() + "前");
                DateFile.instance.storyBuffs[-7] += 1000;
                Main.logger.Log(DateFile.instance.storyBuffs[-7].ToString() + "后");
            }

            if (GUILayout.Button("启动奇遇"))
            {
                DateFile df = DateFile.instance;
                df.SetStory(true, df.mianPartId, df.mianPlaceId, Main.settings.eventId);
            }

            if (GUILayout.Button("传剑选人"))
            {
                foreach (int family in DateFile.instance.actorFamilyDate)
                {
                    if(!Main.teamActorId.Contains(family)) Main.teamActorId.Add(family);
                }
            }
            
            if (GUILayout.Button("测试状态"))
            {
                
            }

            /*
            if (GUILayout.Button("测试传剑"))
            {
                ActorMenu.instance.acotrId = Main.selectActorId;
                ActorMenu.instance.SetNewMianActor();
            }

            if (GUILayout.Button("测试遗惠"))
            {
                ActorScore.instance.ShowActorScoreWindow(DateFile.instance.mianActorId);
            }
            */
            GUILayout.EndHorizontal();

            if (Main.teamActorId.Count > 0)
            {
                string actorId = "";
                actorId = GUILayout.TextField(Main.selectActorId.ToString());
                if (GUI.changed)
                {
                    if (actorId == "" || actorId == null) actorId = "0";
                    Main.selectActorId = int.Parse(actorId);
                }

                foreach (int actor in Main.teamActorId)
                {
                    GUILayout.Label(actor.ToString() + ":" + DateFile.instance.GetActorDate(actor, 5) + DateFile.instance.GetActorDate(actor, 0));
                }
            }

            string shijianId = GUILayout.TextField(Main.settings.eventId.ToString());
            if (GUI.changed)
            {
                if (shijianId == "" || shijianId == null) shijianId = "0";
                Main.settings.eventId = int.Parse(shijianId);
            }

            if (Main.quQuGuiList.Count > 0)
            {
                GUILayout.Label("蛐蛐列表");
                GUILayout.BeginHorizontal("Box", new GUILayoutOption[0]);
                int i = 0;
                foreach (KeyValuePair<int, string> ququ in Main.quQuGuiList)
                {
                    if (i % 29 == 0 && i != 0 && i != quQuGuiList.Count)
                    {
                        GUILayout.EndVertical();
                        GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
                    }
                    if (i == 0) GUILayout.BeginVertical("Box", new GUILayoutOption[0]);

                    Main.SetGUIToToggle(ququ.Key, ququ.Value, ref Main.settings.ququList);

                    i++;
                    if (i == quQuGuiList.Count) GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 保存mod的设置
        /// </summary>
        /// <param name="modEntry">umm</param>
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Main.settings.Save(modEntry);
        }

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

        static void TestModValue()
        {
            DateFile df = DateFile.instance;
            if (Main.settings.ququList.Count > 0)
            {
                foreach (KeyValuePair<int, Dictionary<int, string>> ququ in DateFile.instance.cricketDate)
                {
                    if (Main.settings.ququList.Contains(ququ.Key))
                    {
                        ququ.Value[9] = "-20";
                        ququ.Value[6] = "100";
                    }
                    else
                    {
                        ququ.Value[9] = "50";
                        ququ.Value[6] = "1";
                    }
                }
            }
            else
            {
                Main.logger.Log("没有选择蛐蛐");
            }
        }

        public static Dictionary<int, string> GetQuQuData()
        {
            Dictionary<int, string> items = new Dictionary<int, string>();
            foreach (KeyValuePair<int, Dictionary<int, string>> quQu in DateFile.instance.cricketDate)
            {
                if (!Main.defaultQuQuVoice.ContainsKey(quQu.Key)) Main.defaultQuQuVoice.Add(quQu.Key, new string[] { quQu.Value[9], quQu.Value[6] });
                items.Add(quQu.Key, DateFile.instance.SetColoer(int.Parse(DateFile.instance.cricketDate[quQu.Key][1]) + 20001, quQu.Value[0]));
            }

            return items;
        }

        public static void ResetQuQuData()
        {
            if (Main.defaultQuQuVoice.Count > 0)
            {
                foreach (KeyValuePair<int, string[]> ququData in Main.defaultQuQuVoice)
                {
                    if (DateFile.instance.cricketDate.ContainsKey(ququData.Key))
                    {
                        DateFile.instance.cricketDate[ququData.Key][9] = ququData.Value[0];
                        DateFile.instance.cricketDate[ququData.Key][6] = ququData.Value[1];
                    }
                }
            }
            else
            {
                Main.logger.Log("恢复失败，请重新退出游戏。");
            }
        }
    }

    [HarmonyPatch(typeof(NewGame), "SetNewGameDate")]
    public class CeshiNewGame_SetNewGameDate
    {
        static void Prefix()
        {

        }
    }

    [HarmonyPatch(typeof(NewGame), "Start")]
    public class CeshiUiLoading_Show
    {
        static void Prefix()
        {

        }
    }

    [HarmonyPatch(typeof(UIDate), "GetMaxManpower")]
    public class MaxPeople_For_UIDate_GetBaseMaxManpower
    {
        static void Postfix(ref int __result)
        {
            if (Main.enabled && Main.settings.maxPeople >= 0) __result += Main.settings.maxPeople;
        }
    }

    [HarmonyPatch(typeof(UIDate), "DoChangeTrun")]
    public class QuQuLifeAndFood_For_UIDate_DoChangeTrun
    {
        static List<int> foodType = new List<int> { 84, 85 };

        static void Prefix()
        {
            if (Main.enabled)
            {
                if (Main.settings.ququLife) QuQuCheat();
                if (Main.settings.foodMerge) FoodMergeCheat();
            }
        }

        /// <summary>
        /// 处理蛐蛐年龄始终是0
        /// </summary>
        public static void QuQuCheat()
        {
            /*
            DateFile df = DateFile.instance;
            List<int> boxQuQu = new List<int>();

            foreach (int[] box in df.cricketBoxDate.Values)
            {
                if (box[0] != -97) boxQuQu.Add(box[0]);
            }

            foreach (int actorId in df.actorFamilyDate)
            {
                if (int.Parse(df.GetActorDate(actorId, 312)) != 0) boxQuQu.Add(int.Parse(df.GetActorDate(actorId, 312)));
            }

            foreach (KeyValuePair<int, Dictionary<int, string>> item in df.itemsDate)
            {
                if ((df.actorItemsDate[10001].ContainsKey(item.Key) || df.actorItemsDate[-999].ContainsKey(item.Key) || boxQuQu.Contains(item.Key)) && item.Value[999] == "10000")
                {
                    if (item.Value[901] != "0" && item.Value[2007] != "0") item.Value[2007] = "0";
                }
            }
            */

            int[] ququIds = Items.GetAllAliveCricketIds();
            for (int i = 0; i < ququIds.Length; i++)
            {
                Dictionary<int, string> item = Items.GetItem(ququIds[i]);
                if (item[901] != "0" && item[2007] != "0")
                {
                    item[2007] = "0";
                    Items.SetItem(ququIds[i], item);
                }
            }
        }

        /// <summary>
        /// 作弊的合并食物
        /// </summary>
        public static void FoodMergeCheat()
        {
            DateFile df = DateFile.instance;
            Dictionary<int, Dictionary<int, string>> foods = new Dictionary<int, Dictionary<int, string>>();
            Dictionary<int, int> itemsId = new Dictionary<int, int>();
            if (!df.actorItemsDate.TryGetValue(df.mianActorId, out itemsId))
            {
                Main.logger.Log("失败itemsId");
                return;
            }

            List<int> buffer = itemsId.Keys.ToList();
            foreach (int itemId in buffer)
            {
                string id = df.GetItemDate(itemId, 999), level = df.GetItemDate(itemId, 8), type = df.GetItemDate(itemId, 98);

                if (!Main.settings.foodLevel.Contains(int.Parse(level))) continue;
                if (!foodType.Contains(int.Parse(type))) continue;
                
                Dictionary<int, string> foodData = new Dictionary<int, string>();
                /*
                if (!df.itemsDate.TryGetValue(itemId, out foodData))
                {
                    Main.logger.Log("失败itemId");
                    continue;
                }
                */
                foodData = Items.GetItem(itemId);
                
                CompareFoodsParams(id, foodData, ref foods);

                //删掉这个item
                df.LoseItem(df.mianActorId, itemId, itemsId[itemId], true);
            }
            Main.logger.Log("合并了" + foods.Count.ToString());
            if (foods.Count > 0) MakeFoods(foods);
        }

        /// <summary>
        /// 根据id对比食物并合并参数
        /// </summary>
        /// <param name="id">食物item的id</param>
        /// <param name="foodData">对比的当前食物参数</param>
        /// <param name="foods">需要合并的食物字典</param>
        static void CompareFoodsParams(string id, Dictionary<int, string> foodData, ref Dictionary<int, Dictionary<int, string>> foods)
        {
            if (foods.ContainsKey(int.Parse(id)))
            {
                foreach (KeyValuePair<int, string> pair in foodData)
                {
                    if (!foods[int.Parse(id)].ContainsKey(pair.Key)) foods[int.Parse(id)].Add(pair.Key, pair.Value);

                    if (pair.Key == 901 || pair.Key == 902)
                    {
                        foods[int.Parse(id)][pair.Key] = (int.Parse(foods[int.Parse(id)][pair.Key]) + int.Parse(pair.Value)).ToString();
                    }
                }
            }
            else
            {
                foods.Add(int.Parse(id), new Dictionary<int, string>());
                foods[int.Parse(id)] = foodData;
            }
        }

        /// <summary>
        /// 创建食物
        /// </summary>
        /// <param name="foods">合并好的食物字典</param>
        static void MakeFoods(Dictionary<int, Dictionary<int, string>> foods)
        {
            DateFile df = DateFile.instance;
            foreach (KeyValuePair<int, Dictionary<int, string>> food in foods)
            {
                int makeItemId = df.MakeNewItem(food.Key);
                df.GetItem(df.mianActorId, makeItemId, 1, false, -1, 0);
                foreach (KeyValuePair<int, string> pair in food.Value)
                {
                    Items.SetItemProperty(makeItemId, pair.Key, pair.Value);
                    //df.itemsDate[makeItemId][pair.Key] = pair.Value;
                }
            }
        }
    }

    /// <summary>
    /// 把选择的蛐蛐加到可以抓的地方。
    /// </summary>
    [HarmonyPatch(typeof(GetQuquWindow), "SetGetQuquWindow")]
    public class LKXTestQUQU_For_GetQuquWindow_SetGetQuquWindow
    {

        static void Postfix()
        {
            if (!Main.enabled) return;
            
            Dictionary<int, int[]> data = new Dictionary<int, int[]>();
            data = GetQuquWindow.instance.cricketDate;

            foreach (KeyValuePair<int, int[]> item in data)
            {
                int randomNum1 = UnityEngine.Random.Range(0, Main.settings.ququList.Count - 1);
                int randomNum2 = UnityEngine.Random.Range(0, Main.settings.ququList.Count - 1);
                item.Value[1] = Main.settings.ququList[randomNum1];
                item.Value[2] = Main.settings.ququList[randomNum2];//这条是后缀
                item.Value[3] = 200;//机率？
            }
        }
    }
    
    [HarmonyPatch(typeof(DateFile), "GetMaxItemSize")]
    public class LKXSetGetMaxItemSize_For_DateFile_GetMaxItemSize
    {
        static void Postfix(ref int __result)
        {
            if (Main.enabled && Main.settings.cheatWarehouseMaxSize) __result = 100000000;
        }
    }

    [HarmonyPatch(typeof(DateFile), "GetWarehouseMaxSize")]
    public class LKXSetWarehouseMaxSize_For_DateFile_GetWarehouseMaxSize
    {
        static void Postfix(ref int __result)
        {
            if (Main.enabled && Main.settings.cheatWarehouseMaxSize) __result = 100000000;
        }
    }
}
