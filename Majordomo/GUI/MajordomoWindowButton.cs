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

        // 管家按钮
        public static GameObject goButton;
        // 按钮消息 ID
        private static int messageId = -1;


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
            if (!MajordomoWindowButton.goButton)
            {
                var windowButtonHolder = HomeSystem.instance.showQuquBoxButton.transform.parent;

                MajordomoWindowButton.goButton = UnityEngine.Object.Instantiate(HomeSystem.instance.showQuquBoxButton, windowButtonHolder);
                MajordomoWindowButton.goButton.name = $"MajordomoWindowButton,{MajordomoWindowButton.messageId}";

                var image = MajordomoWindowButton.goButton.GetComponent<Image>();
                string buttonImagePath = Path.Combine(Path.Combine(Main.resBasePath, "Texture"), $"{MajordomoWindowButton.IMAGE_NAME}.png");
                image.sprite = ResourceLoader.CreateSpriteFromImage(buttonImagePath);
                if (!image.sprite) throw new Exception($"Failed to create sprite: {buttonImagePath}");

                var texts = MajordomoWindowButton.goButton.GetComponentsInChildren<Text>();
                foreach (var text in texts) text.text = MajordomoWindowButton.MESSAGE_TITLE;

                var button = Common.RemoveComponent<Button>(MajordomoWindowButton.goButton, recreate: true);
                button.onClick.AddListener(() => MajordomoWindow.instance.Open());
            }
        }
    }


    /// <summary>
    /// Patch: 注册管家界面按钮（在其他 mod 之后注册）
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "Start")]
    [HarmonyPriority(Priority.Last)]
    public static class HomeSystem_Start_RegisterMajordomoWindowButton
    {
        static void Postfix()
        {
            if (!Main.enabled) return;

            MajordomoWindowButton.TryRegisterResources();
        }
    }


    /// <summary>
    /// Patch: 显示管家界面按钮
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "GetBuildingMassage")]
    public static class HomeSystem_GetBuildingMassage_ShowMajordomoWindowButton
    {
        static void Postfix(HomeSystem __instance)
        {
            if (!Main.enabled) return;

            int partId = __instance.homeMapPartId;
            int placeId = __instance.homeMapPlaceId;
            int buildingIndex = __instance.homeMapbuildingIndex;

            int[] building = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int baseBuildingId = building[0];
            bool isTaiwuVillage = int.Parse(DateFile.instance.basehomePlaceDate[baseBuildingId][44]) > 0;

            MajordomoWindowButton.goButton.SetActive(isTaiwuVillage);
        }
    }
}
