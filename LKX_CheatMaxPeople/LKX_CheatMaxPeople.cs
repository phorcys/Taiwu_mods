using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
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

            if (GUILayout.Button("测试传剑"))
            {
                ActorMenu.instance.acotrId = Main.selectActorId;
                ActorMenu.instance.SetNewMianActor();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box", new GUILayoutOption[0]);
            if (GUILayout.Button("测试事件"))
            {
                DateFile.instance.SetEvent(new int[] { 0, -1, 16785 }, false);
            }
            if (GUILayout.Button("测试地图资源"))
            {
                Main.SetStoryForKunLun();
            }
            if (GUILayout.Button("检测地图资源"))
            {
                Main.GetStoryForKunLun();
            }
            if (GUILayout.Button("检测地图资源1"))
            {
                if (DateFile.instance.worldMapState[DateFile.instance.mianPartId].ContainsKey(DateFile.instance.mianPlaceId))
                {
                    Main.logger.Log(DateFile.instance.worldMapState[DateFile.instance.mianPartId][DateFile.instance.mianPlaceId].Length.ToString());
                }
                else
                {
                    Main.logger.Log("不存在的");
                }
            }
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

        public static void SetStoryForKunLun()
        {
            int partId = int.Parse(DateFile.instance.GetGangDate(16, 3));
            int placeId = int.Parse(DateFile.instance.GetGangDate(16, 4));
            List<int> list = new List<int>(GetTaiWuHomePlaceList(partId, placeId, false));

            if (list.Count > 0)
            {
                int placeId2 = list[UnityEngine.Random.Range(0, list.Count)];
                DateFile.instance.SetStory(true, partId, placeId2, 200001, -1);
            }
            else
            {
                Main.logger.Log("添加失败，太吾村所在地图已经没有位置。");
            }
        }

        public static void GetStoryForKunLun()
        {
            int partId = int.Parse(DateFile.instance.GetGangDate(16, 3));
            int placeId = int.Parse(DateFile.instance.GetGangDate(16, 4));
            List<int> list = new List<int>(GetTaiWuHomePlaceList(partId, placeId));

            if (list.Count > 0)
            {
                int i = 0;
                foreach(int storyKey in list)
                {
                    if (DateFile.instance.worldMapState[partId][storyKey][0] == 200001)
                    {
                        DateFile.instance.SetStory(false, partId, storyKey, 0, -1);
                        i++;
                    }
                }

                if (i > 0)
                {
                    Main.logger.Log("完成卸载！卸载了：" + i.ToString() + "个昆仑·镜冢.");
                    return;
                }
            }

            Main.logger.Log("没有找到昆仑·镜冢，可能已经卸载完成过或者该冢并没有出现过，建议备份存档。");
        }

        public static List<int> GetTaiWuHomePlaceList(int partId, int placeId, bool isStory = true)
        {
            List<int> list = new List<int>();
            List<int> list2 = new List<int>(DateFile.instance.GetWorldMapNeighbor(partId, placeId, 1));
            List<int> list3 = new List<int>(DateFile.instance.GetWorldMapNeighbor(partId, placeId, 10));
            for (int j = 0; j < list3.Count; j++)
            {
                int num2 = list3[j];
                bool flag;
                if (isStory)
                {
                    flag = !list2.Contains(num2) && DateFile.instance.HaveStory(partId, num2) && int.Parse(DateFile.instance.GetNewMapDate(partId, num2, 93)) == 0;
                }
                else
                {
                    flag = !list2.Contains(num2) && !DateFile.instance.HaveStory(partId, num2) && int.Parse(DateFile.instance.GetNewMapDate(partId, num2, 93)) == 0;
                }

                if (flag)
                {
                    list.Add(num2);
                }
            }

            return list;
        }

        public static bool KunLunIsExist()
        {
            int partId = int.Parse(DateFile.instance.GetGangDate(16, 3));
            int placeId = int.Parse(DateFile.instance.GetGangDate(16, 4));
            List<int> list = new List<int>(GetTaiWuHomePlaceList(partId, placeId));

            if (list.Count > 0)
            {
                foreach (int storyKey in list)
                {
                    if (DateFile.instance.worldMapState[partId][storyKey][0] == 200001)
                    {
                        return true;
                    }
                }
            }

            return false;
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
                if (DateFile.instance.GetWorldXXLevel() >= 7 && !Main.KunLunIsExist()) DateFile.instance.SetEvent(new int[] { 0, -1, 16785 }, false);
            }
        }

        /// <summary>
        /// 处理蛐蛐年龄始终是0
        /// </summary>
        public static void QuQuCheat()
        {
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
                if (!df.itemsDate.TryGetValue(itemId, out foodData))
                {
                    Main.logger.Log("失败itemId");
                    continue;
                }
                
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
                    df.itemsDate[makeItemId][pair.Key] = pair.Value;
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

    [HarmonyPatch(typeof(Warehouse), "GetWarehouseMaxSize")]
    public class LKXSetWarehouseMaxSize_For_Warehouse_GetWarehouseMaxSize
    {
        static void Postfix(ref int __result)
        {
            if (Main.enabled && Main.settings.cheatWarehouseMaxSize) __result = 100000000;
        }
    }

    [HarmonyPatch(typeof(DateFile), "MakeXXEnemy")]
    public class LKX_Enemy_For_DateFile_MakeXXEnemy
    {
        static void Prefix(int baseActorId, int index)
        {
            if (baseActorId == 18628)
            {
                int num = Mathf.Min(index + 1, 10);
                for(int i = 0; i < 7; i++)
                {
                    int equipId = int.Parse(DateFile.instance.presetActorDate[18628][304 + i]);
                    if(equipId != 0)
                    {
                        DateFile.instance.presetActorDate[18628][304 + i] = (equipId - num / 2).ToString();
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(DateFile), "SetEvent")]
    public class LKX_RegEvent_For_DateFile_SetEvent
    {
        static void Prefix(int[] eventDate)
        {
            if (eventDate[2] == 16781)
            {
                Main.logger.Log("进入对话：" + eventDate[2].ToString());
                LKX_BossBattle_MassageWindow_EndEvent147_1.RunActorData();
            }
        }
    }

    [HarmonyPatch(typeof(MassageWindow), "EndEvent")]
    public class LKX_RegEvent_For_MassageWindow_EndEvent
    {
        static void Prefix()
        {
            if (MassageWindow.instance.eventValue.Count > 0 && MassageWindow.instance.eventValue[0] != 0)
            {
                Main.logger.Log(MassageWindow.instance.eventValue[0].ToString());
                if (MassageWindow.instance.eventValue[0] == 16785)
                {
                    Main.logger.Log("进入16785");
                    Main.SetStoryForKunLun();
                }
            }
        }
    }

    [HarmonyPatch(typeof(MassageWindow), "DoEvent")]
    public class LKX_RegEvent_For_MassageWindow_DoEvent
    {
        static void Prefix(int eventIndex)
        {
            Main.logger.Log("DoEvent:{" + eventIndex.ToString() + "}");
        }
    }

    [HarmonyPatch(typeof(MassageWindow), "SetMassageWindow")]
    public class LKX_RegEvent_For_MassageWindow_SetMassageWindow
    {
        static void Prefix(int[] baseEventDate, int chooseId)
        {
            Main.logger.Log("SetMassageWindow:{");
            Main.logger.Log("  chooseId:" + chooseId.ToString());
            Main.logger.Log("  baseEventDate:{");
            for(int i = 0; i < baseEventDate.Length; i++)
            {
                Main.logger.Log("    " + i.ToString() + ":" + baseEventDate[i].ToString());
            }
            Main.logger.Log("  }");
            Main.logger.Log("}");
        }
    }

    [HarmonyPatch(typeof(MassageWindow), "EndEventChangeMassageWindow")]
    public class LKX_RegEvent_For_MassageWindow_EndEventChangeMassageWindow
    {
        public static bool active = false;
        static void Prefix(int eventId)
        {
            if (eventId == 110)
            {
                active = true;
            }
        }

        static void Postfix()
        {
            if (active)
            {
                DateFile df = DateFile.instance;
                df.SetStory(true, df.mianPartId, df.mianPlaceId, Main.settings.eventId);
            }
        }
    }

    [HarmonyPatch(typeof(MassageWindow), "EndEvent147_1")]
    public class LKX_BossBattle_MassageWindow_EndEvent147_1
    {
        static bool Prefix()
        {
            //长度2，index{0:事件id  1:对战人物id}
            int eventIndex = MassageWindow.instance.eventValue[1];
            if (eventIndex == 18628)
            {
                Event_StartBattleWindow();
                return false;
            }

            return true;
        }

        public static void Event_StartBattleWindow()
        {
            int teamId = 200001;

            StartBattle.instance.ShowStartBattleWindow(teamId, 0, 18, new List<int> { MassageWindow.instance.eventValue[1] });
        }

        public static void RunActorData()
        {
            int id = DateFile.instance.MianActorID();
            Dictionary<int, string> pactor = new Dictionary<int, string>();
            Dictionary<int, string> mianactor = new Dictionary<int, string>();
            if (DateFile.instance.presetActorDate.TryGetValue(18628, out pactor) && DateFile.instance.actorsDate.TryGetValue(DateFile.instance.mianActorId, out mianactor))
            {
                SortedDictionary<int, int[]> mianGongFa = new SortedDictionary<int, int[]>();
                if (DateFile.instance.actorGongFas.TryGetValue(id, out mianGongFa))
                {
                    pactor[906] = GongFaMerge(mianGongFa);
                }
                
                foreach (KeyValuePair<int, string> actor in mianactor)
                {
                    List<int> equip = new List<int> { 304, 305, 306, 307, 308, 309, 310, 311 };
                    if (equip.Contains(actor.Key))
                    {
                        pactor[actor.Key] = DateFile.instance.GetItemDate(int.Parse(actor.Value), 999);
                    }

                    if (pactor.ContainsKey(actor.Key))
                    {

                        List<int> exist = new List<int> {
                            5,11,17,
                            61,62,63,64,65,66,
                            71,72,73,
                            81,82,83,84,85,86,
                            92,93,94,95,96,97,98,
                            101,
                            501,502,503,504,505,506,507,508,509,510,511,512,513,514,515,516,
                            601,602,603,604,605,606,607,608,609,610,611,612,613,614,
                            651,551,
                            995,996
                        };

                        if (!exist.Contains(actor.Key)) continue;
                        
                        if (actor.Key == 101)
                        {
                            pactor[actor.Key] = "10011|" + actor.Value;
                        }
                        else if(actor.Key == 5)
                        {
                            //暂时0是有名字的
                            pactor[0] = "昆仑镜·" + mianactor[actor.Key];
                            if (mianactor[0] != "NoSurname" || mianactor[0] != "无名" || mianactor[0] != "") pactor[0] = pactor[0] + mianactor[0];
                        }
                        else
                        {
                            pactor[actor.Key] = actor.Value;
                        }
                    }
                }
            }
        }

        public static string GongFaMerge(SortedDictionary<int, int[]> gongfa)
        {
            string defaultGongFa = "170010&0";
            foreach(KeyValuePair<int, int[]> item in gongfa)
            {
                if(item.Value[0] >= 25)
                {
                    string addGongFa = "";
                    if(item.Value[2] == 0)
                    {
                        addGongFa = "|" + item.Key.ToString() + "&0";
                    }
                    else if(item.Value[2] <= 5 && item.Value[2] != 0)
                    {
                        addGongFa = "|" + item.Key.ToString() + "&1";
                    }
                    else
                    {
                        addGongFa = "|" + item.Key.ToString() + "&2";
                    }
                    defaultGongFa += addGongFa;
                }
            }

            return defaultGongFa;
        }
    }
}
