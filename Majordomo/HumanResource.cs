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
    public class BuildingWorkInfo
    {
        public int buildingIndex;           // 建筑在所在地域的 ID
        public int requiredAttrId;          // 工作所需资质类型
        public int priority;                // 安排工作人员的优先级
        public int halfWorkingAttrValue;    // 半进度工作时所需资质（标准心情好感、无邻接厢房）
        public int fullWorkingAttrValue;    // 满进度工作时所需资质（标准心情好感、无邻接厢房）
    }


    public class HumanResource
    {
        public const int BASE_BUILDING_ID_BEDROOM = 1003;
        public const int BASE_BUILDING_ID_HOSPITAL = 2904;
        public const int BASE_BUILDING_ID_DETOXIFICATION = 3004;

        public const int HARVEST_TYPE_RESOURCE = 1;
        public const int HARVEST_TYPE_ITEM = 2;
        public const int HARVEST_TYPE_CHARACTER = 3;
        public const int HARVEST_TYPE_CRICKET = 4;

        public const int STANDARD_MOOD = 80;            // 心情：欢喜
        public const int STANDARD_FAVOR_LEVEL = 5;      // 好感：喜爱左右

        public const int WORK_EFFECTIVENESS_HALF = 100;
        public const int WORK_EFFECTIVENESS_FULL = 200;


        // 自动指派工作人员
        public void AssignBuildingWorkers(int partId, int placeId)
        {
            List<int> workerIds = GetWorkerIds();

            // workerId -> {attrId -> attrValue}
            Dictionary<int, Dictionary<int, int>> workerAttrs = new Dictionary<int, Dictionary<int, int>>();
            foreach (int workerId in workerIds) workerAttrs[workerId] = new Dictionary<int, int>();

            // requiredAttrId -> sortedWorkerIds
            Dictionary<int, List<int>> attrCandidates = new Dictionary<int, List<int>>();

            var attrBuildings = GetBuildingsNeedWorker(partId, placeId);

            foreach (int requiredAttrId in attrBuildings.Keys)
            {
                foreach (int workerId in workerIds)
                {
                    // 不需要资质时（厢房），放入好感和心情相对标准状态的差值
                    int attrValue = requiredAttrId != 0 ?
                        int.Parse(DateFile.instance.GetActorDate(workerId, requiredAttrId)) :
                        GetLackedMoodAndFavor(workerId);
                    workerAttrs[workerId][requiredAttrId] = attrValue;
                }

                List<int> sortedWorkerIds = workerAttrs.OrderByDescending(elem => elem.Value[requiredAttrId]).Select(elem => elem.Key).ToList();
                attrCandidates[requiredAttrId] = sortedWorkerIds;
            }

            // 按建筑优先级排序
            List<BuildingWorkInfo> buildingWorkInfos = new List<BuildingWorkInfo>();
            foreach (var currBuildingWorkInfos in attrBuildings.Values) buildingWorkInfos.AddRange(currBuildingWorkInfos);
            var sortedBuildingWorkInfos = buildingWorkInfos.Where(elem => elem.priority >= 0).OrderByDescending(elem => elem.priority);

            foreach (var info in sortedBuildingWorkInfos)
            {
                var building = DateFile.instance.homeBuildingsDate[partId][placeId][info.buildingIndex];
                int baseBuildingId = building[0];
                int buildingLevel = building[1];

                var baseBuilding = DateFile.instance.basehomePlaceDate[baseBuildingId];
                string buildingName = baseBuilding[0];

                string attrName = GetRequiredAttrName(info.requiredAttrId);

                // 从候选列表中选出最合适人选
                // *** 暂不考虑厢房的加成效果 ***
                int selectedWorkerId = SelectBuildingWorker(partId, placeId, info.buildingIndex, attrCandidates, info.requiredAttrId, workerAttrs);
                if (selectedWorkerId >= 0)
                {
                    SetBuildingWorker(partId, placeId, info.buildingIndex, selectedWorkerId);

                    string workerName = DateFile.instance.GetActorName(selectedWorkerId);
                    int attrValue = info.requiredAttrId != 0 ? workerAttrs[selectedWorkerId][info.requiredAttrId] : -1;
                    int mood = int.Parse(DateFile.instance.GetActorDate(selectedWorkerId, 4, addValue: false));
                    int favor = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), selectedWorkerId, getLevel: true);
                    int baseWorkEffectiveness = info.requiredAttrId != 0 ?
                        GetWorkEffectiveness(partId, placeId, info.buildingIndex, selectedWorkerId) : -1;
                    string baseWorkEffectivenessStr = baseWorkEffectiveness >= 0 ? baseWorkEffectiveness / 2 + "%" : "N/A";

                    Main.Logger.Log($"{info.priority}\t" +
                        $"{buildingName} ({buildingLevel}): " +
                        $"{attrName} [{info.halfWorkingAttrValue}, {info.fullWorkingAttrValue}] - " +
                        $"{workerName}, 资质: {attrValue}, 心情: {mood}, 好感: {favor}, 基础效率: {baseWorkEffectivenessStr}");
                }
                else
                {
                    Main.Logger.Log($"{info.priority}\t" +
                        $"{buildingName} ({buildingLevel}): " +
                        $"{attrName} [{info.halfWorkingAttrValue}, {info.fullWorkingAttrValue}] - <无合适人选>");
                }
            }
        }


        private List<int> GetWorkerIds()
        {
            List<int> workerIds = new List<int>();

            List<int> actorIds = DateFile.instance.GetGangActor(16, 9);

            List<int> teammates = DateFile.instance.GetFamily(getPrisoner: true);

            foreach (int actorId in actorIds)
            {
                if (teammates.Contains(actorId)) continue;

                int age = int.Parse(DateFile.instance.GetActorDate(actorId, 11, addValue: false));
                if (age <= 14) continue;

                workerIds.Add(actorId);
            }

            return workerIds;
        }


        private void SetBuildingWorker(int partId, int placeId, int buildingIndex, int workerId)
        {
            if (!DateFile.instance.actorsWorkingDate.ContainsKey(partId))
            {
                DateFile.instance.actorsWorkingDate[partId] = new Dictionary<int, Dictionary<int, int>>();
            }

            if (!DateFile.instance.actorsWorkingDate[partId].ContainsKey(placeId))
            {
                DateFile.instance.actorsWorkingDate[partId][placeId] = new Dictionary<int, int>();
            }

            DateFile.instance.actorsWorkingDate[partId][placeId][buildingIndex] = workerId;

            HomeSystem.instance.UpdateHomePlace(partId, placeId, buildingIndex);
        }


        // 计算心情和好感相对标准状态的工作效率差值（差值大的优先休息）
        // 注意，此处只关心心情和好感，不关心资质
        // 返回值小于 0 时，说明工作效率已经超过标准状态
        private int GetLackedMoodAndFavor(int actorId)
        {
            int currMood = int.Parse(DateFile.instance.GetActorDate(actorId, 4, addValue: false));
            int currScaledFavor = 40 + DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), actorId, getLevel: true) * 10;
            currScaledFavor = AdjustScaledFavorWithMood(currScaledFavor, currMood);

            int standardMood = STANDARD_MOOD;
            int standardScaledFavor = 40 + STANDARD_FAVOR_LEVEL * 10;
            standardScaledFavor = AdjustScaledFavorWithMood(standardScaledFavor, standardMood);

            return standardScaledFavor - currScaledFavor;
        }


        // 用于计算建筑工作效率
        private int AdjustScaledFavorWithMood(int scaledFavor, int mood)
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


        // 按资质顺序检查实际效率是否达到及格线（或心情好感未达到标准），若达到则选择
        // 若没有任何人达到及格线（或心情好感皆达到标准），或者没有任何工作人员，则返回 -1
        private int SelectBuildingWorker(int partId, int placeId, int buildingIndex,
            Dictionary<int, List<int>> attrCandidates, int requiredAttrId,
            Dictionary<int, Dictionary<int, int>> workerAttrs)
        {
            int selectedWorkerId = -1;

            List<int> candidates = attrCandidates[requiredAttrId];
            if (candidates.Count <= 0) return selectedWorkerId;

            if (requiredAttrId != 0)
            {
                foreach (int workerId in candidates)
                {
                    int effect = GetWorkEffectiveness(partId, placeId, buildingIndex, workerId);
                    if (effect >= WORK_EFFECTIVENESS_HALF)
                    {
                        selectedWorkerId = workerId;
                        break;
                    }
                }
            }
            else
            {
                foreach (int workerId in candidates)
                {
                    int lackedMoodAndFavor = workerAttrs[workerId][requiredAttrId];
                    if (lackedMoodAndFavor > 0)
                    {
                        selectedWorkerId = workerId;
                        break;
                    }
                }
            }

            if (selectedWorkerId >= 0)
            {
                foreach (var currCandidates in attrCandidates.Values)
                {
                    currCandidates.Remove(selectedWorkerId);
                }
            }

            return selectedWorkerId;
        }

        
        // 获取需要工作人员的建筑，以及该建筑的工作相关信息
        private Dictionary<int, List<BuildingWorkInfo>> GetBuildingsNeedWorker(int partId, int placeId)
        {
            // requiredAttrId -> [BuildingWorkInfo, ]
            Dictionary<int, List<BuildingWorkInfo>> attrBuildings = new Dictionary<int, List<BuildingWorkInfo>>();

            var buildingsHarvestTypes = GetBuildingsHarvestTypes();

            var buildings = DateFile.instance.homeBuildingsDate[partId][placeId];

            foreach (var entry in buildings)
            {
                int buildingIndex = entry.Key;
                int[] building = entry.Value;
                int baseBuildingId = building[0];

                var baseBuilding = DateFile.instance.basehomePlaceDate[baseBuildingId];
                bool needWorker = int.Parse(baseBuilding[3]) == 1;
                if (needWorker)
                {
                    int requiredAttrId = int.Parse(baseBuilding[33]);

                    BuildingWorkInfo info = new BuildingWorkInfo
                    {
                        buildingIndex = buildingIndex,
                        requiredAttrId = requiredAttrId,
                        priority = GetBuildingWorkingPriority(partId, placeId, buildingIndex, buildingsHarvestTypes),
                        halfWorkingAttrValue = 0,
                        fullWorkingAttrValue = 0,
                    };

                    if (requiredAttrId != 0)
                    {
                        int[] requiredAttrValues = GetRequiredAttributeValues(partId, placeId, buildingIndex);
                        info.halfWorkingAttrValue = requiredAttrValues[0];
                        info.fullWorkingAttrValue = requiredAttrValues[1];
                    }

                    if (!attrBuildings.ContainsKey(requiredAttrId)) attrBuildings[requiredAttrId] = new List<BuildingWorkInfo>();
                    attrBuildings[requiredAttrId].Add(info);
                }
            }

            return attrBuildings;
        }


        // 返回建筑安排工作人员的优先级。优先级越高，越优先安排高能力的工作人员。
        // 返回值越大，优先级越高；为负数时（因用户配置而排除），不参与人员分配。
        // 目前固定优先级为：厢房 > 病坊、密医 > 收获人才建筑 > 收获资源建筑 > 收获物品建筑 > 收获蛐蛐建筑
        // 同一类别中，工作难度越高，优先级越高
        private int GetBuildingWorkingPriority(int partId, int placeId, int buildingIndex,
            Dictionary<int, HashSet<int>> buildingsHarvestTypes)
        {
            int[] building = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int baseBuildingId = building[0];

            int priority;

            if (baseBuildingId == BASE_BUILDING_ID_BEDROOM)
            {
                priority = 5000;
            }
            else if (baseBuildingId == BASE_BUILDING_ID_HOSPITAL || baseBuildingId == BASE_BUILDING_ID_DETOXIFICATION)
            {
                priority = 4000;
            }
            else
            {
                var harvestTypes = buildingsHarvestTypes[baseBuildingId];
                if (harvestTypes.Contains(HARVEST_TYPE_CHARACTER))
                {
                    priority = 3000;
                }
                else if (harvestTypes.Contains(HARVEST_TYPE_RESOURCE))
                {
                    priority = 2000;
                }
                else if (harvestTypes.Contains(HARVEST_TYPE_ITEM))
                {
                    priority = 1000;
                }
                else
                {
                    priority = 0;
                }
            }

            // 从当前数据来看，工作难度取值范围 [1, 119]
            int baseWorkDifficulty = int.Parse(DateFile.instance.basehomePlaceDate[baseBuildingId][51]);
            int buildingLevel = building[1];
            int workDifficulty = Mathf.Max(baseWorkDifficulty + (buildingLevel - 1), 1);
            priority += workDifficulty;

            return priority;
        }


        // 获取各建筑收获物类型
        // @return:         baseBuildingId -> {harvestType, }
        // harvestType:     1: 资源, 2: 物品, 3: 人才, 4: 蛐蛐
        private Dictionary<int, HashSet<int>> GetBuildingsHarvestTypes()
        {
            Dictionary<int, HashSet<int>> buildingsHarvestTypes = new Dictionary<int, HashSet<int>>();

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


        private string GetRequiredAttrName(int requiredAttrId)
        {
            switch (requiredAttrId)
            {
                case 0:
                    return "<无>";
                case 18:
                    return "名誉";
                default:
                    return DateFile.instance.actorAttrDate[requiredAttrId][0];
            }
        }


        // 返回对于指定建筑，在标准状态下，需要多少对应资质以达到 50% / 100% 工作效率
        private int[] GetRequiredAttributeValues(int partId, int placeId, int buildingIndex, int adjacentAttrBonus = 0)
        {
            int[] building = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int baseBuildingId = building[0];
            int requiredAttrId = int.Parse(DateFile.instance.basehomePlaceDate[baseBuildingId][33]);
            int mood = STANDARD_MOOD;
            int scaledFavor = 40 + STANDARD_FAVOR_LEVEL * 10;

            scaledFavor = AdjustScaledFavorWithMood(scaledFavor, mood);

            int buildingLevel = building[1];
            int baseWorkDifficulty = int.Parse(DateFile.instance.basehomePlaceDate[baseBuildingId][51]);
            int workDifficulty = Mathf.Max(baseWorkDifficulty + (buildingLevel - 1), 1);

            int requiredHalfAttrValue = Mathf.Max(workDifficulty * 100 / Mathf.Max(scaledFavor, 0) - adjacentAttrBonus, 0);
            int requiredFullAttrValue = Mathf.Max(workDifficulty * 200 / Mathf.Max(scaledFavor, 0) - adjacentAttrBonus, 0);

            // 需要威望时，要从以 0 为中间值换算为以 0 为最小值
            if (requiredAttrId == 18)
            {
                requiredHalfAttrValue -= 100;
                requiredFullAttrValue -= 100;
            }

            return new int[] { requiredHalfAttrValue, requiredFullAttrValue };
        }


        // 获取指定人物在指定建筑内的工作效率
        // 大部分照抄 HomeSystem::GetBuildingLevelPct 方法，改动主要在邻接厢房方面
        private int GetWorkEffectiveness(int partId, int placeId, int buildingIndex, int actorId, bool withAdjacentBedrooms = false)
        {
            int[] building = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int baseBuildingId = building[0];
            int requiredAttrId = int.Parse(DateFile.instance.basehomePlaceDate[baseBuildingId][33]);
            int mood = int.Parse(DateFile.instance.GetActorDate(actorId, 4, addValue: false));
            int scaledFavor = 40 + DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), actorId, getLevel: true) * 10;

            scaledFavor = AdjustScaledFavorWithMood(scaledFavor, mood);

            int attrValue = (requiredAttrId > 0) ? int.Parse(DateFile.instance.GetActorDate(actorId, requiredAttrId)) : 0;
            // 需要威望时，要从以 0 为中间值换算为以 0 为最小值
            if (requiredAttrId == 18) attrValue += 100;

            int totalAdjacentAttrValue = 0;
            if (withAdjacentBedrooms)
            {
                int[] adjacentBuildingIndexes = HomeSystem.instance.GetBuildingNeighbor(partId, placeId, buildingIndex);
                foreach (int adjacentBuildingIndex in adjacentBuildingIndexes)
                {
                    if (!DateFile.instance.homeBuildingsDate[partId][placeId].ContainsKey(adjacentBuildingIndex)) continue;
                    if (!DateFile.instance.actorsWorkingDate[partId][placeId].ContainsKey(adjacentBuildingIndex)) continue;

                    int[] adjacentBuilding = DateFile.instance.homeBuildingsDate[partId][placeId][adjacentBuildingIndex];
                    int adjacentBaseBuildingId = adjacentBuilding[0];
                    int bedroomValue = int.Parse(DateFile.instance.basehomePlaceDate[adjacentBaseBuildingId][62]);
                    if (bedroomValue == 0) continue;

                    int adjacentActorId = DateFile.instance.actorsWorkingDate[partId][placeId][adjacentBuildingIndex];

                    int adjacentAttrValue = (requiredAttrId > 0) ? int.Parse(DateFile.instance.GetActorDate(adjacentActorId, requiredAttrId)) : 0;
                    // 需要威望时，要从以 0 为中间值换算为以 0 为最小值
                    if (requiredAttrId == 18) adjacentAttrValue += 100;

                    totalAdjacentAttrValue += adjacentAttrValue;
                }
            }

            attrValue = (attrValue + totalAdjacentAttrValue) * Mathf.Max(scaledFavor, 0) / 100;
            int buildingLevel = building[1];
            int baseWorkDifficulty = int.Parse(DateFile.instance.basehomePlaceDate[baseBuildingId][51]);
            int workDifficulty = Mathf.Max(baseWorkDifficulty + (buildingLevel - 1), 1);
            int workEffectiveness = Mathf.Clamp(attrValue * 100 / workDifficulty, 50, 200);

            return workEffectiveness;
        }
    }
}
