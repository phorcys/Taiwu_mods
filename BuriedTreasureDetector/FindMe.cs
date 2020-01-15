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
    public static class FindMe
    {
        public static Dictionary<int, List<int>> Getplace(int _partId,int mapSize)
        {
            Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
            //循环所有地块
            for (int placeId = 0; placeId < mapSize; placeId++)
            {
                //如果有奇遇
                if (DateFile.instance.HaveStory(_partId, placeId))
                {
                    int storyid = DateFile.instance.worldMapState[_partId][placeId][0];
                    foreach (int key in FindMe.type.Keys)
                    {
                        if (FindMe.type[key].Contains(storyid))
                        {
                            dictionary.Add(placeId, new List<int>
                            {
                                storyid,
                                key
                            });
                            break;
                        }
                    }
                }
            }
            return dictionary;
        }

        /// <summary>
        /// 天灾情况下 获得地块信息
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="num"></param>
        /// <returns>地块信息</returns>
        public static string Getplacename(int partId, int placeId)
        {
            return DateFile.instance.SetColoer(10002, DateFile.instance.GetNewMapDate(partId, placeId, 98) + DateFile.instance.GetNewMapDate(partId, placeId, 0), false);
        }

        /// <summary>
        /// 地区情况下 获得地块信息
        /// </summary>
        /// <param name="partId">地区ID</param>
        /// <param name="placeId">地块ID</param>
        /// <param name="num">地区地块总个数</param>
        /// <returns>地块信息</returns>
        public static string Getplacename(int partId, int placeId, int num)
        {
            return DateFile.instance.SetColoer(10002, DateFile.instance.GetNewMapDate(partId, placeId, 0) + placeId % num + "，" + placeId / num, false);
        }

        /// <summary>
        /// 获得物品信息
        /// </summary>
        /// <param name="storyid">物品Id</param>
        /// <param name="key">物品种类Id</param>
        /// <returns>物品信息</returns>
        public static string Getitemename(int storyid, int key)
        {
            string str = "谜";
            int num = 20009;
            switch (key)
            {
                case 0:
                    num--;
                    str = "食材";
                    break;
                case 1:
                    str = ((storyid > 3007) ? "软木" : "硬木");
                    break;
                case 2:
                    if (storyid > 3207)
                    {
                        str = "软玉";
                    }
                    else if (storyid > 3114)
                    {
                        str = "硬玉";
                    }
                    else if (storyid > 3107)
                    {
                        str = "软铁";
                    }
                    else
                    {
                        str = "硬铁";
                    }
                    break;
                case 3:
                    str = ((storyid > 3307) ? "丝布" : "皮革");
                    break;
                case 4:
                    str = ((storyid > 4096) ? "毒物" : "药材");
                    break;
            }
            return DateFile.instance.SetColoer(num, DateFile.instance.baseStoryDate[storyid][0].Replace("寻找", "（" + str + "）"), false);
        }

        public static Dictionary<int, Dictionary<int, List<int>>> aowu = new Dictionary<int, Dictionary<int, List<int>>>();

        private static FieldInfo disastersStorys = typeof(PeopleLifeAI).GetField("_disasterStoriesInfo", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic);

        private static Dictionary<int, int[]> type = (Dictionary<int, int[]>)FindMe.disastersStorys.GetValue(PeopleLifeAI.instance);
    }
}
