using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;

namespace BuriedTreasureDetector
{
    public class FindAll
    {
        //private enum ItemKes
        //{
        //    UnKnow = 0,
        //    Food = 1,
        //    Wood = 2,
        //    Jade = 3,
        //    Iron = 4,
        //    Textile = 5,
        //    Poison = 6,
        //    Drug = 7
        //}

        private static FindAll _instance;
        private static object _instance_Lock = new object();
        public static FindAll Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instance_Lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new FindAll();
                        }
                    }

                }
                return _instance;
            }
        }

        public Dictionary<string, PartItem> GetPlace(int partId)
        {
            if (Find.ContainsKey(partId))
            {
                // 取得当前地区信息
                return Find[partId];
            }
            else
            {
                Clear();
                DictionaryPrace = SetNewPrace();

                //循环所有15个大地图
                for (int baseWorldId = 0; baseWorldId < 15; baseWorldId++)
                {
                    //循环所有地图里面的3个地区
                    foreach (int basePartId in DateFile.instance.baseWorldDate[baseWorldId].Keys)
                    {
                        //获取当前地区的地图宽度
                        int mapWidth = int.Parse(DateFile.instance.partWorldMapDate[basePartId][98]);
                        //设置当前地区信息
                        Find.Add(basePartId, SetPlace(basePartId, mapWidth * mapWidth));
                    }
                }

                return Find[partId];
            }
        }

        public void Clear()
        {
            Find.Clear();
            DictionaryPrace.Clear();
        }

        /// <summary>
        /// 45个地区信息汇总
        /// </summary>
        public Dictionary<string, WorldItem> DictionaryPrace = new Dictionary<string, WorldItem>();

        public Dictionary<string, PartItem> GetPlace(int partId, int placeId)
        {
            Dictionary<string, PartItem> dictionary = new Dictionary<string, PartItem>();
            dictionary.Add("食材", new PartItem());
            dictionary.Add("木材", new PartItem());
            dictionary.Add("玉石", new PartItem());
            dictionary.Add("铁石", new PartItem());
            dictionary.Add("织物", new PartItem());
            dictionary.Add("药材", new PartItem());
            dictionary.Add("毒物", new PartItem());
            dictionary.Add("茶酒", new PartItem());


            GetPlaceWorkItem(partId, placeId, dictionary);

            return dictionary;
        }

        private Dictionary<string, WorldItem> SetNewPrace()
        {
            Dictionary<string, WorldItem> praceItem = new Dictionary<string, WorldItem>();
            praceItem.Add("食材", new WorldItem());
            praceItem.Add("木材", new WorldItem());
            praceItem.Add("玉石", new WorldItem());
            praceItem.Add("铁石", new WorldItem());
            praceItem.Add("织物", new WorldItem());
            praceItem.Add("药材", new WorldItem());
            praceItem.Add("毒物", new WorldItem());
            praceItem.Add("茶酒", new WorldItem());
            return praceItem;
        }

        private Dictionary<int, Dictionary<string, PartItem>> Find = new Dictionary<int, Dictionary<string, PartItem>>();

        private Dictionary<string, PartItem> SetPlace(int partId, int mapSize)
        {
            Dictionary<string, PartItem> dictionary = new Dictionary<string, PartItem>();
            dictionary.Add("食材", new PartItem());
            dictionary.Add("木材", new PartItem());
            dictionary.Add("玉石", new PartItem());
            dictionary.Add("铁石", new PartItem());
            dictionary.Add("织物", new PartItem());
            dictionary.Add("药材", new PartItem());
            dictionary.Add("毒物", new PartItem());
            dictionary.Add("茶酒", new PartItem());

            //循环所有地块
            for (int placeId = 0; placeId < mapSize; placeId++)
            {
                GetPlaceWorkItem(partId, placeId, dictionary);
            }
            return dictionary;
        }

        /// <summary>
        /// copy from GetTimeWorkItem WorldMapSystem
        /// 获取锄地概率
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        private void GetPlaceWorkItem(int partId, int placeId, Dictionary<string, PartItem> dictionary)
        {
            int[] nowResourceAll = DateFile.instance.GetPlaceResource(partId, placeId);

            for (int workTyp = 0; workTyp < 6; workTyp++)
            {
                //最大资源
                int maxResource = Mathf.Max(int.Parse(DateFile.instance.GetNewMapDate(partId, placeId, workTyp + 1)), 1);
                //当前资源
                int nowResource = Mathf.Max(nowResourceAll[workTyp], 0);

                //jmswzyk说 如果低于100就歇歇吧
                //最低升级概率 升级到最高品的概率最大为千分之一
                if (maxResource < 100)
                {
                    continue;
                }
                //获得物品的概率
                int probability_0 = nowResource * 100 / maxResource - 25;

                //基础地形
                int key = DateFile.instance.ParseIntWithDefaultValue(DateFile.instance.GetNewMapDate(partId, placeId, 13), 0);
                //可得的基础物品集合
                string[] array = DateFile.instance.timeWorkBootyDate[key][workTyp + 1].Split('|');

                //没有物品可以获取 再见
                if (array == null || array.Length == 0 || array[0] == "0")
                {
                    continue;
                }

                int probability_1 = 100;
                //地形类型
                int mapType = DateFile.instance.ParseIntWithDefaultValue(DateFile.instance.GetNewMapDate(partId, placeId, 83), 0);
                //相邻
                List<int> worldMapNeighbor = DateFile.instance.GetWorldMapNeighbor(partId, placeId);
                for (int i = 0; i < worldMapNeighbor.Count; i++)
                {
                    //判断相邻地块是否和本地块一致
                    if (DateFile.instance.ParseIntWithDefaultValue(DateFile.instance.GetNewMapDate(partId, worldMapNeighbor[i], 83), 0) == mapType)
                    {
                        probability_1 += 200;
                    }
                }
                //升品概率
                Decimal probability_2 = DateFile.instance.ParseIntWithDefaultValue(DateFile.instance.timeWorkBootyDate[key][workTyp + 1001], 0) + Mathf.Max(maxResource - 100, 0) / 10 * probability_1 / 100;

                //提升品级
                int upLevel = DateFile.instance.ParseIntWithDefaultValue(DateFile.instance.timeWorkBootyDate[key][workTyp + 101], 0);
                Decimal probability_3 = probability_2 * (nowResource * 100 / maxResource) / 100;

                //num6/100  乘  (num8/100 的 num7 次方)
                //if (UnityEngine.Random.Range(0, 100) < num6)
                //for (int j = 0; j < num7; j++)
                //if (UnityEngine.Random.Range(0, 100) < num8)
                // 获取最高品的概率
                Decimal probability = (probability_2 / 100) * new Decimal(Math.Pow((Decimal.ToDouble(probability_3) / 100), upLevel));

                // 最终 挖到东西且获得最高品概率为
                probability = probability / 100 * probability_0;

                //probability = Math.Round(probability, Main.settings.decimalPlace);

                // 获取最高品物品的品级
                int maxLevel = int.Parse(DateFile.instance.GetItemDate(int.Parse(array[0]) + upLevel, 8));

                // 循环所有可获得的基础物品
                foreach (var strItemId in array)
                {
                    int itemId = int.Parse(strItemId);
                    string itemName = Getitemename(itemId, workTyp);

                    if (itemName.Equals("unknow"))
                    {
                        continue;
                    }

                    if (dictionary[itemName].maxProbability.CompareTo(0) == 0)
                    {
                        //dictionary[itemName] = new PlaceItem(probability, maxLevel, placeId);
                        dictionary[itemName].maxProbability = probability;
                        dictionary[itemName].maxLevel = maxLevel;
                        dictionary[itemName].placeIdList.Add(placeId);
                    }
                    else
                    {
                        //该地点以及在字典中 不需要进行判断
                        if (dictionary[itemName].placeIdList.Contains(placeId))
                        {
                            continue;
                        }
                        try
                        {
                            //当前地块概率最大值比较  保存概率大的地块
                            int check = probability.CompareTo(dictionary[itemName].maxProbability);
                            if (check > 0)
                            {
                                dictionary[itemName] = new PartItem(probability, maxLevel, placeId);
                            }
                            else if (check == 0)
                            {
                                dictionary[itemName].placeIdList.Add(placeId);
                            }

                            //当前地块与全世界地块比较 保存概率大的地块
                            check = probability.CompareTo(DictionaryPrace[itemName].maxProbability);
                            if (check > 0)
                            {
                                DictionaryPrace[itemName].maxProbability = probability;
                                DictionaryPrace[itemName].PartPlaceDic.Clear();
                                DictionaryPrace[itemName].PartPlaceDic.Add(partId, new List<int>() { placeId });
                            }
                            else if (check == 0)
                            {
                                if (DictionaryPrace[itemName].PartPlaceDic.ContainsKey(partId))
                                {
                                    //if (!DictionaryPrace[itemName].PartPlaceDic[partId].Contains(placeId))
                                    //{
                                    //    DictionaryPrace[itemName].PartPlaceDic[partId].Add(placeId);
                                    //}
                                    DictionaryPrace[itemName].PartPlaceDic[partId].Add(placeId);
                                }
                                else
                                {
                                    DictionaryPrace[itemName].PartPlaceDic.Add(partId, new List<int>() { placeId });
                                }
                            }
                        }
                        catch (Overflow​Exception e)
                        {
                            Main.Logger.Log("==============Overflow​ExceptionS==============");

                            Main.Logger.Log("probability:" + probability.ToString("R"));
                            Main.Logger.Log("dictionary:" + dictionary[itemName].maxProbability.ToString("R"));

                            Main.Logger.Log("probability_1:" + probability_2.ToString());
                            Main.Logger.Log("probability_2:" + probability_3.ToString());
                            Main.Logger.Log("upLevel:" + upLevel.ToString());
                            Main.Logger.Log("==============Overflow​ExceptionE==============");
                        }
                        catch (Exception e)
                        {
                            Main.Logger.Log("==============ExceptionS==============");
                            Main.Logger.Log("Message:" + e.Message);
                            Main.Logger.Log("StackTrace:" + e.StackTrace);

                            Main.Logger.Log("probability:" + probability.ToString("R"));
                            Main.Logger.Log("dictionary:" + dictionary[itemName].maxProbability.ToString("R"));
                            Main.Logger.Log("==============ExceptionE==============");
                        }

                    }
                }

            }
        }

        /// <summary>
        /// 获得物品信息
        /// </summary>
        /// <param name="storyid">物品Id</param>
        /// <param name="key">物品种类Id</param>
        /// <returns>物品信息</returns>
        public string Getitemename(int storyid, int key)
        {
            string str;
            switch (key)
            {
                case 0:
                    str = "食材";
                    break;
                case 1:
                    str = "木材";
                    break;
                case 2:
                    if (storyid > 3114)
                    {
                        str = "玉石";
                    }
                    else
                    {
                        str = "铁石";
                    }
                    break;
                case 3:
                    str = "织物";
                    break;
                case 4:
                    str = ((storyid > 4096) ? "毒物" : "药材");
                    break;
                case 5:
                    str = "茶酒";
                    break;
                default:
                    str = "unknow";
                    break;
            }
            return str;
        }
    }

    /// <summary>
    /// 世界信息
    /// </summary>
    public class WorldItem
    {
        /// <summary>
        /// 最高掉率
        /// </summary>
        public Decimal maxProbability = 0;
        
        /// <summary>
        /// 地区加地块的ID集合
        /// </summary>
        public Dictionary<int,List<int>> PartPlaceDic;

        public WorldItem()
        {
            maxProbability = 0;
            PartPlaceDic = new Dictionary<int, List<int>>();
        }
    }

    /// <summary>
    /// 地区信息
    /// </summary>
    public class PartItem
    {
        /// <summary>
        /// 最高掉率
        /// </summary>
        public Decimal maxProbability = 0;

        /// <summary>
        /// 最高物品
        /// </summary>
        public int maxLevel = 0;

        /// <summary>
        /// 位置
        /// </summary>
        public List<int> placeIdList = new List<int>();

        public PartItem()
        {
            maxProbability = 0;
            maxLevel = 0;
            placeIdList = new List<int>();
        }

        public PartItem(Decimal maxProbability, int maxLevel, int placeId)
        {
            this.maxProbability = maxProbability;
            this.maxLevel = maxLevel;
            placeIdList = new List<int>() { placeId };
        }
    }
}
