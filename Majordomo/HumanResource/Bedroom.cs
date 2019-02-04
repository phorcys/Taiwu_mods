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
    public class Bedroom
    {
        // 需要厢房才能满效率的建筑信息
        public class BuildingInfo
        {
            public BuildingWorkInfo buildingWorkInfo;   // 建筑本身信息
            public List<int> adjacentBedrooms;          // 邻接厢房列表
        }


        // 将作为提升效率的厢房的信息
        public class BedroomInfo
        {
            public int buildingIndex;                   // 厢房建筑 ID
            public List<int> buildingsNeedBedroom;      // 邻接的需要厢房帮助的建筑列表
        }


        // 获取增加工作效率的厢房列表
        // 如果某个建筑找不到标准状态下满效率的工作人选，那么就看其邻接区域有没有厢房，有则选择一个当作增加工作效率的厢房。
        public static Dictionary<int, List<BuildingWorkInfo>> GetBedroomsForWork(int partId, int placeId,
            Dictionary<int, BuildingWorkInfo> buildings,
            Dictionary<int, List<int>> attrCandidates,
            Dictionary<int, Dictionary<int, int>> workerAttrs)
        {
            // 对于每个需要厢房才能满效率的建筑，尝试找到邻接厢房，生成需要厢房辅助的建筑列表和候选厢房列表
            // buildingIndex -> BuildingInfo
            var buildingsNeedBedroom = new Dictionary<int, BuildingInfo>();
            // bedroomIndex -> BedroomInfo
            var bedroomCandidates = new Dictionary<int, BedroomInfo>();

            Bedroom.GetBedroomsForWork_PrepareData(partId, placeId,
                buildings, attrCandidates, workerAttrs, buildingsNeedBedroom, bedroomCandidates);

            // 按建筑优先级对需要厢房辅助的建筑排序，然后对于每个建筑，在其候选厢房中选择优先级最高的厢房
            List<int> sortedBuildingsNeedBedroom = buildingsNeedBedroom
                .OrderByDescending(entry => entry.Value.buildingWorkInfo.priority)
                .Select(entry => entry.Key).ToList();

            // 厢房 ID -> 该厢房辅助的建筑信息列表
            // bedroomIndex -> [BuildingWorkInfo, ]
            var bedroomsForWork = new Dictionary<int, List<BuildingWorkInfo>>();

            foreach (int buildingIndex in sortedBuildingsNeedBedroom)
            {
                int bedroomIndex = SelectBedroomForWork(partId, placeId, buildingIndex,
                    buildings, buildingsNeedBedroom, bedroomCandidates);

                if (bedroomIndex < 0) continue;

                if (!bedroomsForWork.ContainsKey(bedroomIndex)) bedroomsForWork[bedroomIndex] = new List<BuildingWorkInfo>();
                bedroomsForWork[bedroomIndex].Add(buildings[buildingIndex]);
            }

            return bedroomsForWork;
        }


        /// <summary>
        /// 对于某个需要厢房辅助的建筑，若有多个邻接厢房，选择一个合适的厢房
        /// 选择厢房的标准是：负责建筑数量多、能力种类少、厢房等级低的优先。
        /// 当某个建筑的辅助厢房确定之后，更新厢房列表，把不确定数据变为确定，再继续选择。
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="buildingIndex"></param>
        /// <param name="buildings"></param>
        /// <param name="buildingsNeedBedroom"></param>
        /// <param name="bedroomCandidates"></param>
        /// <returns>可能返回 -1</returns>
        private static int SelectBedroomForWork(int partId, int placeId, int buildingIndex,
            Dictionary<int, BuildingWorkInfo> buildings,
            Dictionary<int, BuildingInfo> buildingsNeedBedroom,
            Dictionary<int, BedroomInfo> bedroomCandidates)
        {
            var buildingInfo = buildingsNeedBedroom[buildingIndex];

            int selectedBedroomIndex = -1;
            int selectedBedroomPriority = -1;

            foreach (int bedroomIndex in buildingInfo.adjacentBedrooms)
            {
                var bedroomInfo = bedroomCandidates[bedroomIndex];

                int priority = GetBedroomPriority(partId, placeId, bedroomIndex, bedroomInfo, buildings);

                if (priority > selectedBedroomPriority)
                {
                    selectedBedroomIndex = bedroomIndex;
                    selectedBedroomPriority = priority;
                }
            }

            // 某个建筑确定了辅助厢房，那么其他候选厢房的建筑列表里面，就要删掉该建筑
            foreach (int bedroomIndex in buildingInfo.adjacentBedrooms)
            {
                if (bedroomIndex == selectedBedroomIndex) continue;

                var bedroomInfo = bedroomCandidates[bedroomIndex];
                bedroomInfo.buildingsNeedBedroom.Remove(buildingIndex);
            }

            return selectedBedroomIndex;
        }


        // 负责建筑数量多、能力种类少、厢房等级低的优先
        private static int GetBedroomPriority(int partId, int placeId, int bedroomIndex,
            BedroomInfo bedroomInfo, Dictionary<int, BuildingWorkInfo> buildings)
        {
            // nbuildingsNeedBedroom: [1, 4]
            int nbuildingsNeedBedroom = bedroomInfo.buildingsNeedBedroom.Count;

            // nRequiredAttrIds: [1, 4]
            var requiredAttrIds = new HashSet<int>();

            foreach (int buildingIndex in bedroomInfo.buildingsNeedBedroom)
            {
                var info = buildings[buildingIndex];
                requiredAttrIds.Add(info.requiredAttrId);
            }

            int nRequiredAttrIds = requiredAttrIds.Count;

            // bedroomLevel: [1, 20]
            int[] building = DateFile.instance.homeBuildingsDate[partId][placeId][bedroomIndex];
            int bedroomLevel = building[1];

            // 4 buildings: 8+1, 8+2, 8+3, 8+4
            // 3 buildings: 6+2, 6+3, 6+4
            // 2 buildings: 4+3, 4+4
            // 1 buildings: 2+4
            int priority =
                nbuildingsNeedBedroom * 200 +           // [200, 800]
                (5 - nRequiredAttrIds) * 100 +          // [100, 400]
                (21 - bedroomLevel) * 1;                // [1, 20]

            return priority;
        }


        // 对于每个需要厢房才能满效率的建筑，尝试找到邻接厢房，生成建筑列表和候选厢房列表
        private static void GetBedroomsForWork_PrepareData(int partId, int placeId,
            Dictionary<int, BuildingWorkInfo> buildings,
            Dictionary<int, List<int>> attrCandidates,
            Dictionary<int, Dictionary<int, int>> workerAttrs,
            Dictionary<int, BuildingInfo> buildingsNeedBedroom,
            Dictionary<int, BedroomInfo> bedroomCandidates)
        {
            // 遍历所有建筑
            foreach (var info in buildings.Values)
            {
                // 排除厢房
                if (info.requiredAttrId == 0) continue;

                // 如果没有候选人，则不寻找邻接厢房
                var sortedWorkerIds = attrCandidates[info.requiredAttrId];
                if (sortedWorkerIds.Any())
                {
                    int workerId = sortedWorkerIds[0];
                    int attrMaxValue = workerAttrs[workerId][info.requiredAttrId];
                    // 凭单个候选人无法满效率
                    if (attrMaxValue < info.fullWorkingAttrValue)
                    {
                        // 找到邻接厢房，并创建供之后使用的数据结构
                        var adjacentBedrooms = Bedroom.GetAdjacentBedrooms(partId, placeId, info.buildingIndex);

                        // 记录需要厢房的建筑信息
                        buildingsNeedBedroom[info.buildingIndex] = new BuildingInfo
                        {
                            buildingWorkInfo = info,
                            adjacentBedrooms = adjacentBedrooms,
                        };

                        // 记录候选厢房的信息
                        foreach (int bedroomBuildingIndex in adjacentBedrooms)
                        {
                            if (!bedroomCandidates.ContainsKey(bedroomBuildingIndex))
                            {
                                bedroomCandidates[bedroomBuildingIndex] = new BedroomInfo
                                {
                                    buildingIndex = bedroomBuildingIndex,
                                    buildingsNeedBedroom = new List<int>(),
                                };
                            }

                            bedroomCandidates[bedroomBuildingIndex].buildingsNeedBedroom.Add(info.buildingIndex);
                        }
                    }
                }
            }
        }


        public static List<int> GetAdjacentBedrooms(int partId, int placeId, int buildingIndex)
        {
            var adjacentBedrooms = new List<int>();

            int[] adjacentBuildingIndexes = HomeSystem.instance.GetBuildingNeighbor(partId, placeId, buildingIndex);
            foreach (int adjacentBuildingIndex in adjacentBuildingIndexes)
            {
                if (!DateFile.instance.homeBuildingsDate[partId][placeId].ContainsKey(adjacentBuildingIndex)) continue;
                if (!Bedroom.IsBedroom(partId, placeId, adjacentBuildingIndex)) continue;
                adjacentBedrooms.Add(adjacentBuildingIndex);
            }

            return adjacentBedrooms;
        }


        public static bool IsBedroom(int partId, int placeId, int buildingIndex)
        {
            int[] building = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int baseBuildingId = building[0];
            int bedroomValue = int.Parse(DateFile.instance.basehomePlaceDate[baseBuildingId][62]);

            return bedroomValue > 0;
        }
    }
}
