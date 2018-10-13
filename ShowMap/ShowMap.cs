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

namespace ShowMap
{
    public static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value) return false;
            enabled = value;
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            DateFile tbl = DateFile.instance;
            if (tbl == null || tbl.actorsDate == null || !tbl.actorsDate.ContainsKey(tbl.mianActorId))
            {
                GUILayout.Label("存档未载入!");
            }
            else
            {
                if (GUILayout.Button("显示未探索区域"))
                {
                    showAllMap();
                }
            }
        }

        public static void showAllMap()
        {
            int bianchang = Int32.Parse(DateFile.instance.partWorldMapDate[DateFile.instance.mianPartId][98]);
            int placeNum = bianchang * bianchang;
            for (int j = 0; j < placeNum; j++)
            {
                DateFile.instance.SetMapPlaceShow(DateFile.instance.mianPartId, j, true);
            }
            logger.Log(DateFile.instance.partWorldMapDate[DateFile.instance.mianPartId][0] + "的" + placeNum.ToString() + "个地块已全部点亮");
        }
    }
}
