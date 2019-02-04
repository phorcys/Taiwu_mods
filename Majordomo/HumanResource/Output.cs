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
    public class Output
    {
        public static void LogBuildingAndWorker(BuildingWorkInfo info, int selectedWorkerId,
            int partId, int placeId, Dictionary<int, Dictionary<int, int>> workerAttrs)
        {
            var building = DateFile.instance.homeBuildingsDate[partId][placeId][info.buildingIndex];
            int baseBuildingId = building[0];
            int buildingLevel = building[1];

            var baseBuilding = DateFile.instance.basehomePlaceDate[baseBuildingId];
            string buildingName = baseBuilding[0];

            string attrName = Output.GetRequiredAttrName(info.requiredAttrId);

            string logText = $"{info.priority}\t" +
                    $"{buildingName} ({buildingLevel}): " +
                    $"{attrName} [{info.halfWorkingAttrValue}, {info.fullWorkingAttrValue}] - ";

            if (selectedWorkerId >= 0)
            {
                string workerName = DateFile.instance.GetActorName(selectedWorkerId);
                int attrValue = info.requiredAttrId != 0 ? workerAttrs[selectedWorkerId][info.requiredAttrId] : -1;
                int mood = int.Parse(DateFile.instance.GetActorDate(selectedWorkerId, 4, addValue: false));
                int favor = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), selectedWorkerId, getLevel: true);

                // 这里的工作效率并不一定等于最终工作效率，因为可能还有厢房未分配
                int workEffectiveness = info.requiredAttrId != 0 ?
                    Original.GetWorkEffectiveness(partId, placeId, info.buildingIndex, selectedWorkerId) : -1;
                string workEffectivenessStr = workEffectiveness >= 0 ? workEffectiveness / 2 + "%" : "N/A";

                Main.Logger.Log(logText +
                    $"{workerName}, 资质: {attrValue}, 心情: {mood}, 好感: {favor}, 工作效率: {workEffectivenessStr}");
            }
            else
            {
                Main.Logger.Log(logText + "<无合适人选>");
            }
        }


        public static void LogAuxiliaryBedroomAndWorker(int bedroomIndex, List<BuildingWorkInfo> relatedBuildings,
            int priority, int selectedWorkerId,
            int partId, int placeId, Dictionary<int, Dictionary<int, int>> workerAttrs)
        {
            var building = DateFile.instance.homeBuildingsDate[partId][placeId][bedroomIndex];
            int baseBuildingId = building[0];
            int buildingLevel = building[1];

            var baseBuilding = DateFile.instance.basehomePlaceDate[baseBuildingId];
            string buildingName = baseBuilding[0];

            var attrNames = new List<string>();
            foreach (var info in relatedBuildings)
                attrNames.Add(Output.GetRequiredAttrName(info.requiredAttrId));

            string attrNamesStr = String.Join(", ", attrNames.ToArray());

            string logText = $"{priority}\t{buildingName} ({buildingLevel}): ({attrNamesStr}) - ";

            if (selectedWorkerId >= 0)
            {
                string workerName = DateFile.instance.GetActorName(selectedWorkerId);

                var attrValues = new List<string>();
                foreach (var info in relatedBuildings)
                    attrValues.Add(workerAttrs[selectedWorkerId][info.requiredAttrId].ToString());

                string attrValuesStr = String.Join(", ", attrValues.ToArray());

                int mood = int.Parse(DateFile.instance.GetActorDate(selectedWorkerId, 4, addValue: false));
                int favor = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), selectedWorkerId, getLevel: true);

                Main.Logger.Log(logText + $"{workerName}, 资质: ({attrValuesStr}), 心情: {mood}, 好感: {favor}");
            }
            else
            {
                Main.Logger.Log(logText + "<无合适人选>");
            }
        }


        private static string GetRequiredAttrName(int requiredAttrId)
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
    }
}
