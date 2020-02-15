using System;
using UnityEngine;
using UnityModManagerNet;

namespace ShowMap
{
    public static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger logger;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool enable)
        {
            Main.enabled = enable;
            logger.Log("显示地图MOD已" + (enable ? "开启" : "关闭"));
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (!Main.enabled)
            {
                GUILayout.Label("Mod已关闭!");
                return;
            }
            DateFile tbl = DateFile.instance;
            if (tbl == null || !GameData.Characters.HasChar(tbl.MianActorID()))
            {
                GUILayout.Label("存档未载入!");
            }
            else
            {
                if (GUILayout.Button("显示未探索区域"))
                {
                    ShowAllMap();
                }
            }
        }

        public static void ShowAllMap()
        {
            if (!Main.enabled) return;
            int length = Int32.Parse(DateFile.instance.partWorldMapDate[DateFile.instance.mianPartId][98]);
            int num = length * length;
            for (int i = 0; i < num; i++)
            {
                DateFile.instance.SetMapPlaceShow(DateFile.instance.mianPartId, i, true);
            }
            logger.Log(DateFile.instance.partWorldMapDate[DateFile.instance.mianPartId][0] + "的" + num.ToString() + "个地块已全部点亮");
        }
    }
}
