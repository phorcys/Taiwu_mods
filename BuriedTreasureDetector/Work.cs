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
    public class Work
    {
        private static Work _instance;
        private static object _instance_Lock = new object();
        public static Work Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instance_Lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Work();
                        }
                    }

                }
                return _instance;
            }
        }


        public void ChooseTimeWork()
        {
            //循环所有15个大地图
            for (int baseWorldId = 0; baseWorldId < 15; baseWorldId++)
            {
                //循环所有地图里面的3个地区
                foreach (int basePartId in DateFile.instance.baseWorldDate[baseWorldId].Keys)
                {
                    // 判断当前地图是否有人力
                    //if (DateFile.instance.manpowerUseList.ContainsKey(basePartId))
                    if (DateFile.instance.worldMapWorkState.ContainsKey(basePartId))
                    {
                        //获取当前地区的地图宽度
                        int mapWidth = int.Parse(DateFile.instance.partWorldMapDate[basePartId][98]);

                        //循环所有地块
                        for (int placeId = 0; placeId < mapWidth * mapWidth; placeId++)
                        {
                            // 判断当前地块是否有人力
                            //if (DateFile.instance.manpowerUseList[basePartId].ContainsKey(placeId))
                            if (DateFile.instance.worldMapWorkState[basePartId].ContainsKey(placeId))
                            {
                                // 采集类型
                                int workTyp = DateFile.instance.worldMapWorkState[basePartId][placeId] - 1;

                                //最大资源
                                int maxResource = Mathf.Max(int.Parse(DateFile.instance.GetNewMapDate(basePartId, placeId, workTyp + 1)), 1);
                                //当前资源
                                int nowResource = Mathf.Max(DateFile.instance.GetPlaceResource(basePartId, placeId)[workTyp], 0);

                                if (maxResource >= 100 && maxResource.Equals(nowResource))
                                {
                                    ChooseTimeWork(basePartId, placeId, workTyp);
                                }
                            }
                        }
                    }
                }
            }
        }


        private void ChooseTimeWork(int choosePartId, int choosePlaceId, int chooseWorkTyp)
        {
            int num = DateFile.instance.GetPlaceResource(choosePartId, choosePlaceId)[chooseWorkTyp];
            int num2 = (num >= 100) ? (num * 60 / 100) : (num * 40 / 100);
            num2 += UnityEngine.Random.Range(-num2 * 20 / 100, num2 * 20 / 100 + 1);
            int getItemId = ChoosePlaceWindow.Instance.GetTimeWorkItem(DateFile.instance.MianActorID(), choosePartId, choosePlaceId, chooseWorkTyp, -1, getItem: false, actorWork: true);
            int addResource = Mathf.Max(num2, 0);
            TimeWorkEnd(addResource, getItemId, choosePartId, choosePlaceId, chooseWorkTyp);
        }
        private static void TimeWorkEnd(int addResource, int getItemId, int choosePartId, int choosePlaceId, int chooseWorkTyp)
        {
            // 更新物品
            if (getItemId != 0)
            {
                DateFile.instance.GetItem(DateFile.instance.MianActorID(), getItemId, 1, true, 0);
            }
            //更新地块信息
            UIDate.instance.ChangePlaceResource(false, -25, choosePartId, choosePlaceId, chooseWorkTyp);
            int num = (DateFile.instance.worldResource == 0) ? 10 : 5;
            int num2 = (DateFile.instance.worldResource == 0) ? 4 : 2;
            // 更新太吾资源
            UIDate.instance.ChangeResource(DateFile.instance.MianActorID(), chooseWorkTyp, (chooseWorkTyp == 5) ? (addResource * num) : (addResource * num2));
        }
    }
}
