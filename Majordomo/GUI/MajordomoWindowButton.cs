using DG.Tweening;
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
    /// <summary>
    /// 管家界面入口按钮
    /// </summary>
    public class MajordomoWindowButton
    {
        private const string IMAGE_NAME = "SystemIcon_majordomo";
        private const string MESSAGE_TITLE = "管家";
        private const string MESSAGE_CONTENT = "查看管家相关信息…";

        // 管家按钮图片
        private static Sprite buttonSprite;
        // 按钮消息 ID
        public static int messageId = -1;


        /// <summary>
        /// 检查资源是否存在，若不存在则载入并注册
        /// </summary>
        public static void TryRegisterResources()
        {
            // 注册入口按钮浮窗信息
            if (MajordomoWindowButton.messageId < 0 ||
                !DateFile.instance.massageDate.ContainsKey(MajordomoWindowButton.messageId) ||
                DateFile.instance.massageDate[MajordomoWindowButton.messageId][0] != MajordomoWindowButton.MESSAGE_TITLE)
            {
                MajordomoWindowButton.messageId = ResourceLoader.AppendRow(DateFile.instance.massageDate,
                    new Dictionary<int, string>
                    {
                        [0] = MESSAGE_TITLE,
                        [1] = MESSAGE_CONTENT,
                    });
            }

            // 载入并注册管家入口按钮
            var windowButtonHolder = BuildingWindow.instance.showQuquBoxButton.transform.parent;

            if (buttonSprite == null)
            {
                string buttonImagePath = Path.Combine(Path.Combine(Main.resBasePath, "Texture"), $"{MajordomoWindowButton.IMAGE_NAME}.png");
                buttonSprite = ResourceLoader.CreateSpriteFromImage(buttonImagePath);
                if (buttonSprite == null) throw new Exception($"Failed to create sprite: {buttonImagePath}");
            }

            var goButton = UnityEngine.Object.Instantiate(BuildingWindow.instance.showQuquBoxButton, windowButtonHolder);
            goButton.name = $"MajordomoWindowButton,{MajordomoWindowButton.messageId}";

            var image = goButton.GetComponent<Image>();
            image.sprite = buttonSprite;
            var texts = goButton.GetComponentsInChildren<Text>();
            foreach (var text in texts) text.text = MajordomoWindowButton.MESSAGE_TITLE;

            var button = Common.RemoveComponent<Button>(goButton, recreate: true);
            button.onClick.AddListener(() => MajordomoWindow.instance.Open());

            UnityEngine.Debug.Log("Resources of MajordomoWindowButton registered.");
        }
    }


    /// <summary>
    /// Patch: 注册管家界面按钮（在其他 mod 之后注册）
    /// </summary>
    [HarmonyPatch(typeof(BuildingWindow), "instance", MethodType.Getter)]
    [HarmonyPriority(Priority.Last)]
    public static class BuildingWindow_instance_Getter_RegisterMajordomoWindowButton
    {
        static void Prefix(ref bool __state, BuildingWindow ____inst)
        {
            if (!Main.enabled) return;

            __state = ____inst == null;

            if (__state) UnityEngine.Debug.Log($"BuildingWindow.instance: initializing...");
        }


        static void Postfix(bool __state)
        {
            if (!Main.enabled) return;

            if (__state)
            {
                UnityEngine.Debug.Log($"BuildingWindow.instance: initialized.");
                MajordomoWindowButton.TryRegisterResources();
            }
        }
    }


    /// <summary>
    /// Patch: 显示管家界面按钮
    /// </summary>
    [HarmonyPatch(typeof(BuildingWindow), "GetBuildingMassage")]
    public static class BuildingWindow_GetBuildingMassage_ShowMajordomoWindowButton
    {
        static void Postfix()
        {
            if (!Main.enabled) return;

            int partId = HomeSystem.instance.homeMapPartId;
            int placeId = HomeSystem.instance.homeMapPlaceId;
            int buildingIndex = HomeSystem.instance.homeMapbuildingIndex;

            int[] building = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int baseBuildingId = building[0];
            bool isTaiwuVillage = int.Parse(DateFile.instance.basehomePlaceDate[baseBuildingId][44]) > 0;

            var windowButtonHolder = BuildingWindow.instance.showQuquBoxButton.transform.parent;
            var goButton = windowButtonHolder.Find($"MajordomoWindowButton,{MajordomoWindowButton.messageId}").gameObject;

            goButton.SetActive(isTaiwuVillage);
        }
    }
}
