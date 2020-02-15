using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Harmony12;
using UnityModManagerNet;
using System.Reflection;

namespace ShowRichResources
{

    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }

        public bool ShowMax = true; //false为显示资源，true为显示资源上限
        public int yuZhi = 150;
        public bool[] yuZhiToggle = { true, true, true, true, true, true};
    }
    public static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger logger;
        public static Settings settings;
        public static bool flag = false;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Main.enabled = value;
            if (isInGame())
            {
                RefreshResourcesIcon();
            }
            if (value)
            {
                logger.Log("ShowRichResources已加载");
            }
            else
            {
                logger.Log("ShowRichResources已关闭");
            }
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.ShowMax = GUILayout.Toggle(settings.ShowMax, "显示资源上限");
            GUILayout.Label("勾选时根据资源上限和阈值显示图标，未勾选时，根据资源和阈值显示图标");
            GUILayout.BeginHorizontal();
            settings.yuZhiToggle[0] = GUILayout.Toggle(settings.yuZhiToggle[0], "食材", GUILayout.Width(60));
            settings.yuZhiToggle[1] = GUILayout.Toggle(settings.yuZhiToggle[1], "木材", GUILayout.Width(60));
            settings.yuZhiToggle[2] = GUILayout.Toggle(settings.yuZhiToggle[2], "金石", GUILayout.Width(60));
            settings.yuZhiToggle[3] = GUILayout.Toggle(settings.yuZhiToggle[3], "织物", GUILayout.Width(60));
            settings.yuZhiToggle[4] = GUILayout.Toggle(settings.yuZhiToggle[4], "药材", GUILayout.Width(60));
            settings.yuZhiToggle[5] = GUILayout.Toggle(settings.yuZhiToggle[5], "银钱", GUILayout.Width(60));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("显示阈值:", GUILayout.Width(60));
            int thresh = 0;
            if (int.TryParse(GUILayout.TextArea(settings.yuZhi.ToString(), GUILayout.Width(50)), out thresh) && thresh > 0 && thresh <= 200)
            {
                settings.yuZhi = thresh;
            }
            if (isInGame())
            {
                if (GUILayout.Button("应用", GUILayout.Width(80)))
                {
                    RefreshResourcesIcon();
                }
            }
            else
            {
                GUILayout.Label("存档未载入!");
            }
            GUILayout.EndHorizontal();
        }

        public static bool isInGame()
        {
            DateFile tbl = DateFile.instance;
            if (tbl == null || GameData.Characters.HasChar(tbl.MianActorID()))
            {
                return false;
            }
            else return true;
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        //刷新当前地图全部资源格子
        public static void RefreshResourcesIcon()
        {
            int bianchang = Int32.Parse(DateFile.instance.partWorldMapDate[DateFile.instance.mianPartId][98]);
            int placeNum = bianchang * bianchang;
            
            for (int j = 0; j < placeNum; j++)
            {
                SetResourcesIcon(j);
            }
        }

        //反射获取WorldMapPlace下私有变量
        internal static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindingFlags);
            return field.GetValue(instance);
        }

        //每当资源图标刷新，在其后重新刷新
        [HarmonyPatch(typeof(WorldMapPlace), "UpdatePaceResource")]
        public static class WorldMapPlace_UpdatePaceResource_Patch
        {
            public static void Postfix(WorldMapPlace __instance)
            {
                int placeId = Int32.Parse(GetInstanceField(typeof(WorldMapPlace), __instance, "placeId").ToString());
                SetResourcesIcon(placeId);
            }
        }

        //设置格子图标
        public static void SetResourcesIcon(int placeId)
        {
            WorldMapPlace worldMapPlace = WorldMapSystem.instance.worldMapPlaces[placeId];
            if (DateFile.instance.HaveShow(DateFile.instance.mianPartId, placeId) > 0)
            {
                if (int.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 89)) != 6)
                {
                    worldMapPlace.resourceIconHolder.gameObject.SetActive(true);
                    int[] placeResource = DateFile.instance.GetPlaceResource(DateFile.instance.mianPartId, placeId);
                    int[] placeResourceMax = new int[]
                    {
                            Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 1)),
                            Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 2)),
                            Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 3)),
                            Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 4)),
                            Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 5)),
                            Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 6)),
                    };
                    int[] compare;
                    if (settings.ShowMax)
                    {
                        compare = placeResourceMax;
                    }
                    else
                    {
                        compare = placeResource;
                    }
                    if (!Main.enabled)
                    {
                        compare = placeResource;
                    }
                    for (int i = 0; i < 6; i++)
                    {
                        int yuZhi = Main.enabled ? settings.yuZhi : 100;
                        if (compare[i] >= yuZhi && settings.yuZhiToggle[i])
                        {
                            worldMapPlace.resourceIcon[i].SetActive(true);
                        }
                        else
                        {
                            worldMapPlace.resourceIcon[i].SetActive(false);
                        }
                    }
                }
                else
                {
                    worldMapPlace.resourceIconHolder.gameObject.SetActive(false);
                }
            }
            else
            {
                worldMapPlace.resourceIconHolder.gameObject.SetActive(false);
            }
        }
    }
}
