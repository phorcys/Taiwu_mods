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

            
            if (GUILayout.Button("测试"))
            {
                QuQuLifeAndFood_For_UIDate_DoChangeTrun.FoodMergeCheat();
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

        /*
        static void TestModValue()
        {
            DateFile df = DateFile.instance;
            int i = 0;
            List<int> boxQuQu = new List<int>();
            foreach (int[] box in df.cricketBoxDate.Values)
            {
                if (box[0] != -97) boxQuQu.Add(box[0]);
            }

            foreach (int actorId in df.acotrTeamDate)
            {
                if (int.Parse(df.GetActorDate(actorId, 312)) != 0) boxQuQu.Add(int.Parse(df.GetActorDate(actorId, 312)));
            }

            foreach (KeyValuePair<int, Dictionary<int, string>> item in DateFile.instance.itemsDate)
            {
                if ((DateFile.instance.actorItemsDate[10001].ContainsKey(item.Key) || DateFile.instance.actorItemsDate[-999].ContainsKey(item.Key) || boxQuQu.Contains(item.Key)) && item.Value[999] == "10000")
                {
                    i++;
                }
            }

            foreach (int aaa in df.acotrTeamDate)
            {
                Main.logger.Log(aaa.ToString());
            }
            Main.logger.Log(i.ToString() + "个");
        }
        */
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

        static void Prefix(SaveDateFile __instance)
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
            DateFile df = DateFile.instance;
            List<int> boxQuQu = new List<int>();
            foreach (int[] box in df.cricketBoxDate.Values)
            {
                if (box[0] != -97) boxQuQu.Add(box[0]);
            }

            foreach (int actorId in df.acotrTeamDate)
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
}
