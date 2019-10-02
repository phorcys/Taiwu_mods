using Harmony12;
using System;
using System.Collections.Generic;
using System.IO;
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
    public enum BuildingType
    {
        Bedroom = 0,
        Hospital = 1,
        Recruitment = 2,
        GettingResource = 3,
        GettingItem = 4,
        GettingCricket = 5,
        Unknown = 6,
    }


    public class BuildingWorkInfo
    {
        public int buildingIndex;           // 建筑在所在地域中的 ID
        public int requiredAttrId;          // 工作所需资质类型
        public float priority;              // 安排工作人员的优先级
        public int halfWorkingAttrValue;    // 半进度工作时所需资质（标准心情好感、无邻接厢房）
        public int fullWorkingAttrValue;    // 满进度工作时所需资质（标准心情好感、无邻接厢房）


        public bool IsBedroom()
        {
            return this.requiredAttrId <= 0;
        }
    }


    public class AuxiliaryBedroomWorker
    {
        public int actorId;                 // 工作人员 ID
        public float integratedAttrValue;   // 此工作人员对于当前厢房来说的统合能力值
        public int lackedMoodAndFavor;      // 此工作人员的心情和好感相对标准状态的差值
    }


    public class HumanResource
    {
        public const int STANDARD_MOOD = 80;            // 心情：欢喜
        public const int STANDARD_FAVOR_LEVEL = 5;      // 好感：亲密左右

        public const int WORK_EFFECTIVENESS_HALF = 100;
        public const int WORK_EFFECTIVENESS_FULL = 200;

        // 计算厢房工作人员的统合能力值时，单项能力百分比的阈值。只要达到这个值，单项能力就是满分。
        public const float ATTR_INTEGRATION_THRESHOLD = 0.6f;

        private readonly int partId;
        private readonly int placeId;
        private readonly TaiwuDate currDate;

        // baseBuildingId -> {harvestType, }
        private static readonly Dictionary<int, HashSet<int>> buildingsHarvestTypes = Original.GetBuildingsHarvestTypes();

        private readonly HashSet<int> excludedBuildings;
        private readonly HashSet<int> excludedWorkers;

        // buildingIndex -> BuildingWorkInfo
        private Dictionary<int, BuildingWorkInfo> buildings;
        // workerId -> {attrId -> attrValue}
        private Dictionary<int, Dictionary<int, int>> workerAttrs;
        // requiredAttrId -> sortedWorkerIds
        private Dictionary<int, List<int>> attrCandidates;


        public HumanResource(int partId, int placeId)
        {
            this.partId = partId;
            this.placeId = placeId;
            this.currDate = new TaiwuDate();

            if (Main.settings.excludedBuildings.ContainsKey(partId) &&
                Main.settings.excludedBuildings[partId].ContainsKey(placeId))
                this.excludedBuildings = new HashSet<int>(Main.settings.excludedBuildings[partId][placeId]);
            else
                this.excludedBuildings = new HashSet<int>();

            this.excludedWorkers = new HashSet<int>();
        }


        /// <summary>
        /// 为太吾村指派工作人员（静态方法）
        /// </summary>
        public static void AssignBuildingWorkersForTaiwuVillage()
        {
            int mainPartId = int.Parse(DateFile.instance.GetGangDate(16, 3));
            int mainPlaceId = int.Parse(DateFile.instance.GetGangDate(16, 4));
            HumanResource hr = new HumanResource(mainPartId, mainPlaceId);
            hr.AssignBuildingWorkers();
        }


        /// <summary>
        /// 指派工作人员
        /// </summary>
        public void AssignBuildingWorkers()
        {
            var workingStats = HumanResource.GetWorkingStats(this.partId, this.placeId);
            MajordomoWindow.instance.AppendMessage(this.currDate, Message.IMPORTANCE_NORMAL,
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_BLUE, "开始指派工作人员") + "　-　" +
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_DARK_BROWN, "综合工作指数") + ": " +
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_WHITE, workingStats.compositeWorkIndex.ToString()));

            Original.RemoveWorkersFromBuildings(this.partId, this.placeId, this.excludedBuildings, this.excludedWorkers);

            MajordomoWindow.instance.AppendMessage(this.currDate, Message.IMPORTANCE_LOWEST,
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_DARK_GRAY, "开始第一轮指派……"));

            this.AssignBuildingWorkers_PrepareData();

            this.AssignBedroomWorkers();

            MajordomoWindow.instance.AppendMessage(this.currDate, Message.IMPORTANCE_LOWEST,
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_DARK_GRAY, "开始第二轮指派……"));

            // 指派完厢房后，重新计算
            this.AssignBuildingWorkers_PrepareData();

            this.AssignLeftBuildings();

            Original.UpdateAllBuildings(this.partId, this.placeId);

            var workerStats = HumanResource.GetWorkerStats(this.partId, this.placeId);
            MajordomoWindow.instance.SetWorkerStats(this.currDate, workerStats);

            workingStats = HumanResource.GetWorkingStats(this.partId, this.placeId);
            MajordomoWindow.instance.AppendMessage(this.currDate, Message.IMPORTANCE_NORMAL,
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_BLUE, "结束指派工作人员") + "　-　" +
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_DARK_BROWN, "综合工作指数") + ": " +
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_WHITE, workingStats.compositeWorkIndex.ToString()));
            MajordomoWindow.instance.SetWorkingStats(this.currDate, workingStats);
        }


        /// <summary>
        /// 计算指定地区的工作人员统计信息
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <returns></returns>
        public static WorkerStats GetWorkerStats(int partId, int placeId)
        {
            int mainActorId = DateFile.instance.MianActorID();
            List<int> workerIds = Original.GetWorkerIds(partId, placeId);
            var stats = new WorkerStats();

            foreach (int workerId in workerIds)
            {
                stats.avgHealthInjury += 1f - TaiwuCommon.GetInjuryRate(workerId);
                stats.avgHealthCirculating += 1f - TaiwuCommon.GetCirculatingBlockingRate(workerId);
                stats.avgHealthPoison += 1f - TaiwuCommon.GetPoisoningRate(workerId);
                stats.avgHealthLifespan += 1f - TaiwuCommon.GetLifespanDamageRate(workerId);

                int mood = int.Parse(DateFile.instance.GetActorDate(workerId, 4, false));
                int favor = DateFile.instance.GetActorFavor(false, mainActorId, workerId);
                int favorLevel = DateFile.instance.GetActorFavor(false, mainActorId, workerId, getLevel: true);
                int scaledFavor = Original.GetScaledFavor(favorLevel);
                scaledFavor = Original.AdjustScaledFavorWithMood(scaledFavor, mood);
                stats.avgMood += mood;
                stats.avgFriendliness += favor;
                stats.avgWorkMotivation += Mathf.Max(scaledFavor, 0) / 100f;
            }

            if (workerIds.Count > 0)
            {
                stats.nWorkers = workerIds.Count;
                stats.avgHealthInjury /= workerIds.Count;
                stats.avgHealthCirculating /= workerIds.Count;
                stats.avgHealthPoison /= workerIds.Count;
                stats.avgHealthLifespan /= workerIds.Count;
                stats.avgCompositeHealth = (stats.avgHealthInjury + stats.avgHealthCirculating + stats.avgHealthPoison + stats.avgHealthLifespan) / 4;
                stats.avgMood /= workerIds.Count;
                stats.avgFriendliness /= workerIds.Count;
                stats.avgWorkMotivation /= workerIds.Count;
            }
            return stats;
        }


        /// <summary>
        /// 计算指定地区的工作统计信息
        /// 
        /// 综合工作指数 = SUM(缩放平移过的建筑工作效率 * 建筑优先级)
        /// 厢房效率不计入统计
        /// 
        /// 由于必须在最开始执行，所以无法使用 prepared data
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <returns></returns>
        private static WorkingStats GetWorkingStats(int partId, int placeId)
        {
            var stats = new WorkingStats();

            // 统计所有建筑的工作效率（厢房效率不计入统计）
            var buildings = DateFile.instance.homeBuildingsDate[partId][placeId];
            foreach (int buildingIndex in buildings.Keys)
            {
                if (!Original.BuildingNeedsWorker(partId, placeId, buildingIndex)) continue;

                if (Bedroom.IsBedroom(partId, placeId, buildingIndex)) continue;

                if (DateFile.instance.actorsWorkingDate.ContainsKey(partId) &&
                    DateFile.instance.actorsWorkingDate[partId].ContainsKey(placeId) &&
                    DateFile.instance.actorsWorkingDate[partId][placeId].ContainsKey(buildingIndex))
                {
                    int workerId = DateFile.instance.actorsWorkingDate[partId][placeId][buildingIndex];
                    int workEffectiveness = Original.GetWorkEffectiveness(partId, placeId, buildingIndex, workerId);
                    float scaledWorkEffectiveness = (workEffectiveness - 100f) / 100f;
                    float priority = HumanResource.GetBuildingWorkingPriority(partId, placeId, buildingIndex, withAdjacentBedrooms: false);

                    ++stats.nProductiveBuildings;
                    stats.avgWorkEffectiveness += workEffectiveness / 200f;
                    stats.compositeWorkIndex += scaledWorkEffectiveness * priority;
                }
            }

            if (stats.nProductiveBuildings > 0) stats.avgWorkEffectiveness /= stats.nProductiveBuildings;
            return stats;
        }


        private void AssignBuildingWorkers_PrepareData()
        {
            List<int> workerIds = Original.GetWorkerIds(this.partId, this.placeId);
            workerIds = workerIds.Where(workerId => !this.excludedWorkers.Contains(workerId)).ToList();

            Dictionary<int, List<BuildingWorkInfo>> attrBuildings = this.GetBuildingsNeedWorker();

            //
            this.buildings = new Dictionary<int, BuildingWorkInfo>();
            foreach (var currBuildingWorkInfos in attrBuildings.Values)
                foreach (var info in currBuildingWorkInfos)
                    this.buildings[info.buildingIndex] = info;

            //
            this.workerAttrs = new Dictionary<int, Dictionary<int, int>>();
            this.attrCandidates = new Dictionary<int, List<int>>();

            foreach (int workerId in workerIds) this.workerAttrs[workerId] = new Dictionary<int, int>();

            foreach (int requiredAttrId in attrBuildings.Keys)
            {
                foreach (int workerId in workerIds)
                {
                    // 不需要资质时（厢房），放入好感和心情相对标准状态的差值
                    int attrValue = requiredAttrId != 0 ?
                        int.Parse(DateFile.instance.GetActorDate(workerId, requiredAttrId)) :
                        HumanResource.GetLackedMoodAndFavor(workerId);
                    this.workerAttrs[workerId][requiredAttrId] = attrValue;
                }

                List<int> sortedWorkerIds = this.workerAttrs
                    .OrderByDescending(elem => elem.Value[requiredAttrId])
                    .Select(elem => elem.Key)
                    .ToList();
                this.attrCandidates[requiredAttrId] = sortedWorkerIds;
            }
        }


        /// <summary>
        /// 为所有厢房安排工作人员
        /// 
        /// 建筑类型优先级因子中的厢房因子不能控制此处的厢房指派优先级，此处所有厢房必定优先于其他建筑指派。
        /// </summary>
        private void AssignBedroomWorkers()
        {
            // 厢房 ID -> 该厢房辅助的建筑信息列表
            // bedroomIndex -> [BuildingWorkInfo, ]
            var bedroomsForWork = Bedroom.GetBedroomsForWork(this.partId, this.placeId,
                this.buildings, this.attrCandidates, this.workerAttrs);

            // 更新辅助类厢房的优先级
            // 辅助类厢房优先级 = SUM(相关建筑优先级)
            // bedroomIndex -> priority
            var auxiliaryBedroomsPriorities = new Dictionary<int, float>();

            foreach (var entry in bedroomsForWork)
            {
                int bedroomIndex = entry.Key;
                var relatedBuildings = entry.Value;

                float priority = relatedBuildings.Select(info => info.priority).Sum();

                auxiliaryBedroomsPriorities[bedroomIndex] = priority;
            }

            // 对于辅助类厢房，按优先级依次分配合适的人选
            MajordomoWindow.instance.AppendMessage(this.currDate, Message.IMPORTANCE_LOWEST,
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_DARK_GRAY, "开始指派辅助类厢房……"));

            var sortedAuxiliaryBedrooms = auxiliaryBedroomsPriorities.OrderByDescending(entry => entry.Value).Select(entry => entry.Key);
            foreach (int bedroomIndex in sortedAuxiliaryBedrooms)
            {
                int selectedWorkerId = this.SelectAuxiliaryBedroomWorker(bedroomIndex, bedroomsForWork[bedroomIndex]);
                if (selectedWorkerId >= 0) Original.SetBuildingWorker(this.partId, this.placeId, bedroomIndex, selectedWorkerId);

                Output.LogAuxiliaryBedroomAndWorker(bedroomIndex, bedroomsForWork[bedroomIndex],
                    auxiliaryBedroomsPriorities[bedroomIndex], selectedWorkerId, this.partId, this.placeId, this.currDate, this.workerAttrs);
            }

            // 对于一般厢房，按优先级依次分配合适的人选
            MajordomoWindow.instance.AppendMessage(this.currDate, Message.IMPORTANCE_LOWEST,
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_DARK_GRAY, "开始指派一般厢房……"));

            var sortedBedrooms = this.buildings.Where(entry => entry.Value.IsBedroom())
                .OrderByDescending(entry => entry.Value.priority).Select(entry => entry.Value);
            foreach (var info in sortedBedrooms)
            {
                if (this.excludedBuildings.Contains(info.buildingIndex)) continue;

                int selectedWorkerId = this.SelectBuildingWorker(info.buildingIndex, info.requiredAttrId);
                if (selectedWorkerId >= 0) Original.SetBuildingWorker(this.partId, this.placeId, info.buildingIndex, selectedWorkerId);

                Output.LogBuildingAndWorker(info, selectedWorkerId, this.partId, this.placeId, this.currDate, this.workerAttrs,
                    suppressNoWorkerWarnning: true);
            }
        }


        /// <summary>
        /// 为辅助厢房选择工作人员
        /// 
        /// 按厢房所需的统合能力值对工作人员进行排序
        /// 如果存在统合能力值为最大值的候选人，则从其中选择心情好感低的
        /// 如果不存在统合能力值为最大值的候选人，则选择统合能力值最大的
        /// 
        /// 统合能力值最大值 = ATTR_INTEGRATION_THRESHOLD * 辅助建筑数
        /// </summary>
        /// <param name="bedroomIndex">厢房的建筑 ID</param>
        /// <param name="relatedBuildings">该厢房辅助的建筑信息列表</param>
        /// <returns></returns>
        private int SelectAuxiliaryBedroomWorker(int bedroomIndex, List<BuildingWorkInfo> relatedBuildings)
        {
            float integratedAttrMaxValue = relatedBuildings.Count * ATTR_INTEGRATION_THRESHOLD;

            var workers = this.GetAuxiliaryBedroomWorkers(bedroomIndex, relatedBuildings);

            if (!workers.Any()) return -1;

            var maxAttrWorkers = workers.Where(worker => worker.integratedAttrValue >= integratedAttrMaxValue).ToList();

            var selectedWorker = maxAttrWorkers.Any() ?
                maxAttrWorkers.OrderByDescending(worker => worker.lackedMoodAndFavor).First() :
                workers.OrderByDescending(worker => worker.integratedAttrValue).First();

            this.excludedBuildings.Add(bedroomIndex);
            this.excludedWorkers.Add(selectedWorker.actorId);

            return selectedWorker.actorId;
        }


        /// <summary>
        /// 对于指定辅助厢房，生成候选人列表
        /// </summary>
        /// <param name="bedroomIndex">厢房的建筑 ID</param>
        /// <param name="relatedBuildings">该厢房辅助的建筑信息列表</param>
        /// <returns></returns>
        private List<AuxiliaryBedroomWorker> GetAuxiliaryBedroomWorkers(int bedroomIndex, List<BuildingWorkInfo> relatedBuildings)
        {
            var workers = new List<AuxiliaryBedroomWorker>();

            foreach (var entry in this.workerAttrs)
            {
                int workerId = entry.Key;

                if (this.excludedWorkers.Contains(workerId)) continue;

                var attrs = entry.Value;

                workers.Add(new AuxiliaryBedroomWorker
                {
                    actorId = workerId,
                    integratedAttrValue = HumanResource.GetIntegratedAttrValue(attrs, relatedBuildings),
                    lackedMoodAndFavor = HumanResource.GetLackedMoodAndFavor(workerId),
                });
            }

            return workers;
        }


        /// <summary>
        /// 计算指定人员对于指定厢房的统合能力值
        /// 
        /// 统合能力值 = SUM(MIN(标准化的能力值 / 标准化的标准状态满效率能力值, ATTR_INTEGRATION_THRESHOLD))
        /// </summary>
        /// <param name="attrs">指定人员的能力字典</param>
        /// <param name="relatedBuildings">指定厢房的辅助建筑列表，不能含有厢房</param>
        /// <returns></returns>
        private static float GetIntegratedAttrValue(Dictionary<int, int> attrs, List<BuildingWorkInfo> relatedBuildings)
        {
            float integratedAttrValue = 0.0f;

            foreach (var info in relatedBuildings)
            {
                int standardAttrValue = Original.ToStandardAttrValue(info.requiredAttrId, attrs[info.requiredAttrId]);
                int standardFullWorkingAttrValue = Original.ToStandardAttrValue(info.requiredAttrId, info.fullWorkingAttrValue);
                float attrRatio = (float)standardAttrValue / standardFullWorkingAttrValue;
                integratedAttrValue += Mathf.Min(attrRatio, ATTR_INTEGRATION_THRESHOLD);
            }

            return integratedAttrValue;
        }


        /// <summary>
        /// 计算心情和好感相对标准状态的工作效率差值（差值大的优先休息）
        /// 注意，此处只关心心情和好感，不关心资质
        /// 返回值小于 0 时，说明工作效率已经超过标准状态
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        private static int GetLackedMoodAndFavor(int actorId)
        {
            int currMood = int.Parse(DateFile.instance.GetActorDate(actorId, 4, false));
            int currFavorLevel = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), actorId, getLevel: true);
            int currScaledFavor = Original.GetScaledFavor(currFavorLevel);
            currScaledFavor = Original.AdjustScaledFavorWithMood(currScaledFavor, currMood);

            int standardMood = STANDARD_MOOD;
            int standardScaledFavor = Original.GetScaledFavor(STANDARD_FAVOR_LEVEL);
            standardScaledFavor = Original.AdjustScaledFavorWithMood(standardScaledFavor, standardMood);

            return standardScaledFavor - currScaledFavor;
        }


        /// <summary>
        /// 为指定建筑选择合适的工作人员
        /// 按资质顺序检查实际效率是否达到及格线（或心情好感未达到标准），若达到则选择
        /// 若没有任何人达到及格线（或心情好感皆达到标准），或者没有任何工作人员，则返回 -1
        /// </summary>
        /// <param name="buildingIndex"></param>
        /// <param name="requiredAttrId"></param>
        /// <returns></returns>
        private int SelectBuildingWorker(int buildingIndex, int requiredAttrId)
        {
            int selectedWorkerId = -1;

            List<int> candidates = this.attrCandidates[requiredAttrId];
            candidates = candidates.Where(workerId => !this.excludedWorkers.Contains(workerId)).ToList();
            if (!candidates.Any()) return -1;

            if (requiredAttrId != 0)
            {
                foreach (int workerId in candidates)
                {
                    int effect = Original.GetWorkEffectiveness(this.partId, this.placeId, buildingIndex, workerId);
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
                    int lackedMoodAndFavor = this.workerAttrs[workerId][requiredAttrId];
                    if (lackedMoodAndFavor > 0)
                    {
                        selectedWorkerId = workerId;
                        break;
                    }
                }
            }

            if (selectedWorkerId >= 0)
            {
                this.excludedBuildings.Add(buildingIndex);
                this.excludedWorkers.Add(selectedWorkerId);
            }

            return selectedWorkerId;
        }


        /// <summary>
        /// 获取当前据点需要工作人员的建筑，以及该建筑的工作相关信息
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, List<BuildingWorkInfo>> GetBuildingsNeedWorker()
        {
            // requiredAttrId -> [BuildingWorkInfo, ]
            var attrBuildings = new Dictionary<int, List<BuildingWorkInfo>>();

            var buildings = DateFile.instance.homeBuildingsDate[this.partId][this.placeId];
            var validBuildings = buildings.Where(entry => !this.excludedBuildings.Contains(entry.Key));

            foreach (var entry in validBuildings)
            {
                int buildingIndex = entry.Key;
                int[] building = entry.Value;

                if (!Original.BuildingNeedsWorker(this.partId, this.placeId, buildingIndex)) continue;
                
                int baseBuildingId = building[0];
                var baseBuilding = DateFile.instance.basehomePlaceDate[baseBuildingId];
                int requiredAttrId = int.Parse(baseBuilding[33]);

                int[] requiredAttrValues = Original.GetRequiredAttributeValues(this.partId, this.placeId, buildingIndex);

                BuildingWorkInfo info = new BuildingWorkInfo
                {
                    buildingIndex = buildingIndex,
                    requiredAttrId = requiredAttrId,
                    priority = HumanResource.GetBuildingWorkingPriority(this.partId, this.placeId, buildingIndex, withAdjacentBedrooms: true),
                    halfWorkingAttrValue = requiredAttrValues[0],
                    fullWorkingAttrValue = requiredAttrValues[1],
                };

                if (!attrBuildings.ContainsKey(requiredAttrId)) attrBuildings[requiredAttrId] = new List<BuildingWorkInfo>();
                attrBuildings[requiredAttrId].Add(info);
            }

            return attrBuildings;
        }


        /// <summary>
        /// 返回建筑指派工作人员的优先级
        /// 
        /// 优先级 = 建筑种类因子 * 标准状态下满效率需求的标准化能力值
        /// 对于厢房，“标准状态下满效率需求的标准化能力值” 即为其等级
        /// 虽然这会导致方向的优先级和其他建筑没有可比性，但是厢房优先级本来就不会与其他建筑比较，所以没有问题
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="buildingIndex"></param>
        /// <param name="withAdjacentBedrooms">是否考虑邻接厢房的影响</param>
        /// <returns></returns>
        private static float GetBuildingWorkingPriority(int partId, int placeId, int buildingIndex, bool withAdjacentBedrooms)
        {
            int[] requiredAttrValues = Original.GetRequiredAttributeValues(partId, placeId, buildingIndex,
                withAdjacentBedrooms, getStandardAttrValue: true);
            int fullWorkingAttrValue = requiredAttrValues[1];

            // 对于厢房，“标准状态下满效率需求的标准化能力值” 即为其等级
            int[] building = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int baseBuildingId = building[0];
            int buildingLevel = building[1];
            int requiredAttrId = int.Parse(DateFile.instance.basehomePlaceDate[baseBuildingId][33]);
            if (requiredAttrId <= 0)
                fullWorkingAttrValue = buildingLevel;

            float typeFactor = HumanResource.GetBuildingPriorityFactor(baseBuildingId);
            return typeFactor * fullWorkingAttrValue;
        }


        /// <summary>
        /// 获取指定建筑的优先级因子
        /// 
        /// 根据不同的建筑类型优先级因子排序，同一建筑的因子会产生变化，
        /// 比如某个建筑同时产出物品和金钱，如果物品的优先级因子更大，那么该建筑就被认定为产出物品，返回物品优先级因子。
        /// </summary>
        /// <param name="baseBuildingId"></param>
        /// <returns></returns>
        private static float GetBuildingPriorityFactor(int baseBuildingId)
        {
            var sortedFactors = Main.settings.buildingTypePriorityFactors.OrderByDescending(entry => entry.Value);

            HashSet<int> harvestTypes = null;
            if (HumanResource.buildingsHarvestTypes.ContainsKey(baseBuildingId))
                harvestTypes = HumanResource.buildingsHarvestTypes[baseBuildingId];

            foreach (var entry in sortedFactors)
            {
                var type = entry.Key;
                var factor = entry.Value;

                switch (type)
                {
                    case BuildingType.Bedroom:
                        // 厢房
                        if (baseBuildingId == 1003) return factor;
                        break;
                    case BuildingType.Hospital:
                        // 病坊，密医
                        if (baseBuildingId == 2904 || baseBuildingId == 3004) return factor;
                        break;
                    case BuildingType.Recruitment:
                        // 收获类型包含人才
                        if (harvestTypes != null && harvestTypes.Contains(3)) return factor;
                        break;
                    case BuildingType.GettingResource:
                        // 收获类型包含资源
                        if (harvestTypes != null && harvestTypes.Contains(1)) return factor;
                        break;
                    case BuildingType.GettingItem:
                        // 收获类型包含物品
                        if (harvestTypes != null && harvestTypes.Contains(2)) return factor;
                        break;
                    case BuildingType.GettingCricket:
                        // 收获类型包含蛐蛐
                        if (harvestTypes != null && harvestTypes.Contains(4)) return factor;
                        break;
                }
            }

            // 建筑无法被归为任意有效类时，即为未知类
            return Main.settings.buildingTypePriorityFactors[BuildingType.Unknown];
        }


        /// <summary>
        /// 指派完厢房之后，继续指派其他建筑
        /// 
        /// 其实本轮指派开始时仍有可能包含厢房，本轮临近结束时也仍有可能尚未指派厢房。
        /// 本轮最后厢房指派，不考虑是否达到心情好感阈值，只要还有人就往里放。
        /// 上次分配后还有厢房剩下的原因：人太少，辅助性厢房装不满；心情都挺好，一般厢房装不满。
        /// 本次分配后还有厢房剩下的原因：人太少。
        /// 
        /// 建筑类型优先级因子中的厢房因子依然不能控制此处的厢房指派优先级，所有厢房在最后阶段指派。
        /// </summary>
        private void AssignLeftBuildings()
        {
            MajordomoWindow.instance.AppendMessage(this.currDate, Message.IMPORTANCE_LOWEST,
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_DARK_GRAY, "开始指派主要建筑……"));

            var sortedBuildings = this.buildings.OrderByDescending(entry => entry.Value.priority).Select(entry => entry.Value);
            foreach (var info in sortedBuildings)
            {
                if (this.excludedBuildings.Contains(info.buildingIndex)) continue;
                if (info.IsBedroom()) continue;

                int selectedWorkerId = this.SelectBuildingWorker(info.buildingIndex, info.requiredAttrId);
                if (selectedWorkerId >= 0) Original.SetBuildingWorker(this.partId, this.placeId, info.buildingIndex, selectedWorkerId);

                Output.LogBuildingAndWorker(info, selectedWorkerId, this.partId, this.placeId, this.currDate, this.workerAttrs,
                    suppressNoWorkerWarnning: false);
            }

            // 最后指派尚未指派的厢房
            MajordomoWindow.instance.AppendMessage(this.currDate, Message.IMPORTANCE_LOWEST,
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_DARK_GRAY, "开始指派尚未指派的厢房……"));

            sortedBuildings = this.buildings.OrderByDescending(entry => entry.Value.priority).Select(entry => entry.Value);
            foreach (var info in sortedBuildings)
            {
                if (this.excludedBuildings.Contains(info.buildingIndex)) continue;
                if (!info.IsBedroom()) continue;

                int selectedWorkerId = this.SelectLeftBedroomWorker(info.buildingIndex);
                if (selectedWorkerId >= 0) Original.SetBuildingWorker(this.partId, this.placeId, info.buildingIndex, selectedWorkerId);

                Output.LogBuildingAndWorker(info, selectedWorkerId, this.partId, this.placeId, this.currDate, this.workerAttrs,
                    suppressNoWorkerWarnning: false);
            }
        }


        /// <summary>
        /// 为最后剩下的厢房指派工作人员
        /// 按心情好感排序，低的优先指派
        /// </summary>
        /// <param name="buildingIndex"></param>
        /// <returns></returns>
        private int SelectLeftBedroomWorker(int buildingIndex)
        {
            const int requiredAttrId = 0;

            var candidates = this.workerAttrs.Where(entry => !this.excludedWorkers.Contains(entry.Key))
                .OrderByDescending(entry => entry.Value[requiredAttrId])
                .Select(entry => entry.Key).ToList();

            if (!candidates.Any()) return -1;

            int selectedWorkerId = candidates[0];

            this.excludedBuildings.Add(buildingIndex);
            this.excludedWorkers.Add(selectedWorkerId);

            return selectedWorkerId;
        }
    }
}
