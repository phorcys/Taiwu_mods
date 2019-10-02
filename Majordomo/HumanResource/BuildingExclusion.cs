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
    public class BuildingExclusion
    {
        public static void Toggle(int partId, int placeId, int buildingIndex)
        {
            if (!Main.settings.excludedBuildings.ContainsKey(partId))
                Main.settings.excludedBuildings[partId] = new SerializableDictionary<int, HashSet<int>>();

            if (!Main.settings.excludedBuildings[partId].ContainsKey(placeId))
                Main.settings.excludedBuildings[partId][placeId] = new HashSet<int>();

            if (Main.settings.excludedBuildings[partId][placeId].Contains(buildingIndex))
            {
                Main.settings.excludedBuildings[partId][placeId].Remove(buildingIndex);
                AudioManager.instance.PlaySE("SE_1");
            }
            else
            {
                Main.settings.excludedBuildings[partId][placeId].Add(buildingIndex);
                AudioManager.instance.PlaySE("SE_7");
            }
            
            HomeSystemWindow.Instance.UpdateHomePlace(partId, placeId, buildingIndex);
        }


        /// <summary>
        /// 获取指定据点指定建筑的自动指派排除状态
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="buildingIndex"></param>
        /// <returns>true: 被排除, false: 未被排除</returns>
        public static bool GetState(int partId, int placeId, int buildingIndex)
        {
            return Main.settings.excludedBuildings.ContainsKey(partId) &&
                Main.settings.excludedBuildings[partId].ContainsKey(placeId) &&
                Main.settings.excludedBuildings[partId][placeId].Contains(buildingIndex);
        }
    }


    /// <summary>
    /// Patch: 在建筑界面添加鼠标右键事件
    /// </summary>
    [HarmonyPatch(typeof(HomeSystemWindow), "MakeHomeMap")]
    public static class HomeSystemWindow_MakeHomeMap_RegisterMouseEvent
    {
        static void Postfix(HomeSystemWindow __instance)
        {
            if (!Main.enabled) return;

            int partId = HomeSystem.instance.homeMapPartId;
            int placeId = HomeSystem.instance.homeMapPlaceId;
            int mapSideLength = int.Parse(DateFile.instance.GetNewMapDate(partId, placeId, 32));
            int maxBuildings = mapSideLength * mapSideLength;

            for (int buildingIndex = 0; buildingIndex < __instance.allHomeBulding.Length && buildingIndex < maxBuildings; ++buildingIndex)
            {
                HomeBuilding building = __instance.allHomeBulding[buildingIndex];
                var handler = building.buildingButton.GetComponent<BuildingPointerHandler>();
                if (!handler) handler = building.buildingButton.AddComponent<BuildingPointerHandler>();
                handler.SetLocation(partId, placeId, buildingIndex);
            }
        }
    }


    /// <summary>
    /// Patch: 在建筑图标上面加上排除标记
    /// </summary>
    [HarmonyPatch(typeof(HomeBuilding), "UpdateBuilding")]
    public static class HomeBuilding_UpdateBuilding_AddExclusionIcon
    {
        static void Postfix(HomeBuilding __instance)
        {
            if (!Main.enabled) return;

            string[] array = __instance.name.Split(new char[] { ',' });
            int partId = int.Parse(array[1]);
            int placeId = int.Parse(array[2]);
            int buildingIndex = int.Parse(array[3]);

            if (BuildingExclusion.GetState(partId, placeId, buildingIndex))
            {
                __instance.placeName.text += "[锁]";
            }
        }
    }


    public class BuildingPointerHandler : MonoBehaviour, IPointerUpHandler
    {
        public int partId;
        public int placeId;
        public int buildingIndex;


        public void SetLocation(int partId, int placeId, int buildingIndex)
        {
            this.partId = partId;
            this.placeId = placeId;
            this.buildingIndex = buildingIndex;
        }


        public void OnPointerUp(PointerEventData eventData)
        {
            if (!Main.enabled) return;

            if (!Original.BuildingNeedsWorker(this.partId, this.placeId, this.buildingIndex)) return;

            // 1: 右键, 2: 中键
            var button = (PointerEventData.InputButton)(Main.settings.exclusionMouseButton + 1);

            if (eventData.button == button && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            {
                BuildingExclusion.Toggle(partId, placeId, buildingIndex);
            }
        }
    }
}
