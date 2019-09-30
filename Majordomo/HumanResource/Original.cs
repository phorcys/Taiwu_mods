using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityModManagerNet;


namespace Majordomo
{
    /// <summary>
    /// 本身就是游戏原有逻辑，或者和游戏原有逻辑关联紧密的方法
    /// 每次游戏版本更新，这里的方法都要检查一遍
    /// </summary>
    public class Original
    {
        // 获取当前据点的工作人员列表
        // *** 目前只适用于据点为太吾村的情形 ***
        public static List<int> GetWorkerIds(int partId, int placeId)
        {
            List<int> workerIds = new List<int>();

            List<int> actorIds = DateFile.instance.GetGangActor(16, 9, true);

            List<int> teammates = DateFile.instance.GetFamily(getPrisoner: true);

            foreach (int actorId in actorIds)
            {
                if (teammates.Contains(actorId)) continue;

                int age = int.Parse(DateFile.instance.GetActorDate(actorId, 11, false));
                if (age <= 14) continue;

                workerIds.Add(actorId);
            }

            return workerIds;
        }


        /// <summary>
        /// 清理所有建筑内的工作人员（排除列表中的除外）
        /// 如果某个建筑在排除列表中，那么其中的工作人员也会同时添加到排除列表中
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="excludedBuildings"></param>
        /// <param name="excludedWorkers"></param>
        public static void RemoveWorkersFromBuildings(int partId, int placeId, HashSet<int> excludedBuildings, HashSet<int> excludedWorkers)
        {
            var buildings = DateFile.instance.homeBuildingsDate[partId][placeId];

            foreach (int buildingIndex in buildings.Keys)
            {
                if (excludedBuildings.Contains(buildingIndex))
                {
                    if (DateFile.instance.actorsWorkingDate.ContainsKey(partId) &&
                        DateFile.instance.actorsWorkingDate[partId].ContainsKey(placeId) &&
                        DateFile.instance.actorsWorkingDate[partId][placeId].ContainsKey(buildingIndex))
                    {
                        int workerId = DateFile.instance.actorsWorkingDate[partId][placeId][buildingIndex];
                        excludedWorkers.Add(workerId);
                    }
                }
                else
                {
                    Original.RemoveBuildingWorker(partId, placeId, buildingIndex);
                }
            }
        }


        public static void UpdateAllBuildings(int partId, int placeId)
        {
            var buildings = DateFile.instance.homeBuildingsDate[partId][placeId];

            if (HomeSystemWindow.Exists)
            {
                foreach (int buildingIndex in buildings.Keys)
                    HomeSystemWindow.Instance.UpdateHomePlace(partId, placeId, buildingIndex);
            }
        }


        /// <summary>
        /// 为指定建筑分配指定工作人员
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="buildingIndex"></param>
        /// <param name="workerId"></param>
        public static void SetBuildingWorker(int partId, int placeId, int buildingIndex, int workerId)
        {
            if (!DateFile.instance.actorsWorkingDate.ContainsKey(partId))
                DateFile.instance.actorsWorkingDate[partId] = new Dictionary<int, Dictionary<int, int>>();

            if (!DateFile.instance.actorsWorkingDate[partId].ContainsKey(placeId))
                DateFile.instance.actorsWorkingDate[partId][placeId] = new Dictionary<int, int>();

            DateFile.instance.actorsWorkingDate[partId][placeId][buildingIndex] = workerId;
        }


        /// <summary>
        /// 移除指定建筑内的工作人员
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="buildingIndex"></param>
        public static void RemoveBuildingWorker(int partId, int placeId, int buildingIndex)
        {
            if (DateFile.instance.actorsWorkingDate.ContainsKey(partId) &&
                DateFile.instance.actorsWorkingDate[partId].ContainsKey(placeId) &&
                DateFile.instance.actorsWorkingDate[partId][placeId].ContainsKey(buildingIndex))
            {
                DateFile.instance.actorsWorkingDate[partId][placeId].Remove(buildingIndex);
                if (DateFile.instance.actorsWorkingDate[partId][placeId].Count <= 0)
                {
                    DateFile.instance.actorsWorkingDate[partId].Remove(placeId);
                    if (DateFile.instance.actorsWorkingDate[partId].Count <= 0)
                    {
                        DateFile.instance.actorsWorkingDate.Remove(partId);
                    }
                }
            }
        }


        // 计算建筑工作效率时，对好感等级的缩放平移
        public static int GetScaledFavor(int favorLevel)
        {
            return 40 + favorLevel * 10;
        }


        // 计算建筑工作效率时，心情对好感的影响
        public static int AdjustScaledFavorWithMood(int scaledFavor, int mood)
        {
            if (mood <= 0)
            {
                return scaledFavor - 30;
            }
            else if (mood <= 20)
            {
                return scaledFavor - 20;
            }
            else if (mood <= 40)
            {
                return scaledFavor - 10;
            }
            else if (mood >= 100)
            {
                return scaledFavor + 30;
            }
            else if (mood >= 80)
            {
                return scaledFavor + 20;
            }
            else if (mood >= 60)
            {
                return scaledFavor + 10;
            }
            else
            {
                return scaledFavor;
            }
        }


        // 获取各建筑收获物类型
        // @return:         baseBuildingId -> {harvestType, }
        // harvestType:     1: 资源, 2: 物品, 3: 人才, 4: 蛐蛐
        public static Dictionary<int, HashSet<int>> GetBuildingsHarvestTypes()
        {
            var buildingsHarvestTypes = new Dictionary<int, HashSet<int>>();

            foreach (int baseBuildingId in DateFile.instance.basehomePlaceDate.Keys)
            {
                int harvestWorkPoint = int.Parse(DateFile.instance.basehomePlaceDate[baseBuildingId][91]);
                if (harvestWorkPoint == 0) continue;

                int baseEventId = int.Parse(DateFile.instance.basehomePlaceDate[baseBuildingId][96]);
                if (baseEventId == 0) continue;

                HashSet<int> harvestTypes = new HashSet<int>();

                string[] eventIdsStr = DateFile.instance.homeShopEventTypDate[baseEventId][1].Split('|');
                foreach (string eventIdStr in eventIdsStr)
                {
                    int eventId = int.Parse(eventIdStr);
                    string[] harvestTypesStr = DateFile.instance.homeShopEventDate[eventId][11].Split('|');
                    foreach (string harvestTypeStr in harvestTypesStr)
                    {
                        int harvestType = int.Parse(harvestTypeStr);
                        if (harvestType != 0) harvestTypes.Add(harvestType);
                    }
                }

                buildingsHarvestTypes[baseBuildingId] = harvestTypes;
            }

            return buildingsHarvestTypes;
        }


        /// <summary>
        /// 返回对于指定建筑，在标准状态下，需要多少对应资质以达到 50% / 100% 工作效率
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="buildingIndex"></param>
        /// <param name="withAdjacentBedrooms"></param>
        /// <param name="getStandardAttrValue">是否返回标准化的能力值</param>
        /// <returns></returns>
        public static int[] GetRequiredAttributeValues(int partId, int placeId, int buildingIndex,
            bool withAdjacentBedrooms = true, bool getStandardAttrValue = false)
        {
            int[] building = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int baseBuildingId = building[0];
            int requiredAttrId = int.Parse(DateFile.instance.basehomePlaceDate[baseBuildingId][33]);

            if (requiredAttrId == 0) return new int[] { 0, 0 };

            int mood = HumanResource.STANDARD_MOOD;
            int scaledFavor = Original.GetScaledFavor(HumanResource.STANDARD_FAVOR_LEVEL);
            scaledFavor = Original.AdjustScaledFavorWithMood(scaledFavor, mood);

            int workDifficulty = Original.GetWorkDifficulty(partId, placeId, buildingIndex);

            int adjacentAttrBonus = withAdjacentBedrooms ?
                Original.GetAdjacentAttrBonus(partId, placeId, buildingIndex, requiredAttrId) : 0;

            int requiredHalfAttrValue = Mathf.Max(workDifficulty * 100 / Mathf.Max(scaledFavor, 0) - adjacentAttrBonus, 0);
            int requiredFullAttrValue = Mathf.Max(workDifficulty * 200 / Mathf.Max(scaledFavor, 0) - adjacentAttrBonus, 0);

            if (!getStandardAttrValue)
            {
                requiredHalfAttrValue = Original.FromStandardAttrValue(requiredAttrId, requiredHalfAttrValue);
                requiredFullAttrValue = Original.FromStandardAttrValue(requiredAttrId, requiredFullAttrValue);
            }

            return new int[] { requiredHalfAttrValue, requiredFullAttrValue };
        }


        // 获取指定建筑的工作难度
        // 从当前数据来看，工作难度取值范围 [1, 119]
        public static int GetWorkDifficulty(int partId, int placeId, int buildingIndex)
        {
            int[] building = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int baseBuildingId = building[0];
            int buildingLevel = building[1];

            int baseWorkDifficulty = int.Parse(DateFile.instance.basehomePlaceDate[baseBuildingId][51]);
            int workDifficulty = Mathf.Max(baseWorkDifficulty + (buildingLevel - 1), 1);

            return workDifficulty;
        }


        /// <summary>
        /// 获取指定人物在指定建筑内的工作效率
        /// 大部分照抄 HomeSystem::GetBuildingLevelPct 方法
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="buildingIndex"></param>
        /// <param name="actorId"></param>
        /// <param name="withAdjacentBedrooms"></param>
        /// <returns></returns>
        public static int GetWorkEffectiveness(int partId, int placeId, int buildingIndex, int actorId, bool withAdjacentBedrooms = true)
        {
            int[] building = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int baseBuildingId = building[0];
            int requiredAttrId = int.Parse(DateFile.instance.basehomePlaceDate[baseBuildingId][33]);
            int mood = int.Parse(DateFile.instance.GetActorDate(actorId, 4, false));
            int favorLevel = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), actorId, getLevel: true);
            int scaledFavor = Original.GetScaledFavor(favorLevel);
            scaledFavor = Original.AdjustScaledFavorWithMood(scaledFavor, mood);

            int attrValue = (requiredAttrId > 0) ? int.Parse(DateFile.instance.GetActorDate(actorId, requiredAttrId)) : 0;
            attrValue = Original.ToStandardAttrValue(requiredAttrId, attrValue);

            int adjacentAttrBonus = withAdjacentBedrooms ?
                Original.GetAdjacentAttrBonus(partId, placeId, buildingIndex, requiredAttrId) : 0;

            int scaledAttrValue = (attrValue + adjacentAttrBonus) * Mathf.Max(scaledFavor, 0) / 100;
            int workDifficulty = Original.GetWorkDifficulty(partId, placeId, buildingIndex);
            int workEffectiveness = Mathf.Clamp(scaledAttrValue * 100 / workDifficulty, 50, 200);

            return workEffectiveness;
        }


        /// <summary>
        /// 获取邻接厢房对指定建筑的能力加成
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="buildingIndex"></param>
        /// <param name="requiredAttrId"></param>
        /// <returns></returns>
        private static int GetAdjacentAttrBonus(int partId, int placeId, int buildingIndex, int requiredAttrId)
        {
            int totalAdjacentAttrValue = 0;

            foreach (int adjacentBuildingIndex in Bedroom.GetAdjacentBedrooms(partId, placeId, buildingIndex))
            {
                if (!DateFile.instance.actorsWorkingDate.ContainsKey(partId) ||
                    !DateFile.instance.actorsWorkingDate[partId].ContainsKey(placeId) ||
                    !DateFile.instance.actorsWorkingDate[partId][placeId].ContainsKey(adjacentBuildingIndex))
                    continue;

                int adjacentActorId = DateFile.instance.actorsWorkingDate[partId][placeId][adjacentBuildingIndex];

                int adjacentAttrValue = (requiredAttrId > 0) ? int.Parse(DateFile.instance.GetActorDate(adjacentActorId, requiredAttrId)) : 0;
                adjacentAttrValue = Original.ToStandardAttrValue(requiredAttrId, adjacentAttrValue);

                totalAdjacentAttrValue += adjacentAttrValue;
            }

            return totalAdjacentAttrValue;
        }


        /// <summary>
        /// 从原始能力值转换到标准化的（可以和所有其他能力类型比较的）能力值
        /// </summary>
        /// <param name="attrId"></param>
        /// <param name="attrValue"></param>
        /// <returns></returns>
        public static int ToStandardAttrValue(int attrId, int attrValue)
        {
            // 只有名誉需要转换
            return attrId == 18 ? attrValue + 100 : attrValue;
        }


        /// <summary>
        /// 从标准化的（可以和所有其他能力类型比较的）能力值转换到原始能力值
        /// </summary>
        /// <param name="attrId"></param>
        /// <param name="standardAttrValue"></param>
        /// <returns></returns>
        public static int FromStandardAttrValue(int attrId, int standardAttrValue)
        {
            // 只有名誉需要转换
            return attrId == 18 ? standardAttrValue - 100 : standardAttrValue;
        }


        /// <summary>
        /// 判断建筑是否需要工作人员（建筑本身需要工作人员，且不处于新建、拆除状态）
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="buildingIndex"></param>
        /// <returns></returns>
        public static bool BuildingNeedsWorker(int partId, int placeId, int buildingIndex)
        {
            int[] building = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int baseBuildingId = building[0];

            var baseBuilding = DateFile.instance.basehomePlaceDate[baseBuildingId];
            return int.Parse(baseBuilding[3]) == 1 && building[3] <= 0 && building[6] <= 0;
        }
    }
}
