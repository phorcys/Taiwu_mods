using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace InventorySort
{

  public static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());



            modEntry.OnToggle = OnToggle;
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
                return false;

            enabled = value;

            return true;
        }

    }




    /// <summary>
    ///  商店 获取数据接口拦截，提前修改商店 this.sellItems 排序
    /// </summary>
    [HarmonyPatch(typeof(ShopSystem), "SetItems")]
    public static class ShopSystem_SetItems_Patch
    {
        private static List<int> sorttypes = new List<int>() { 8, 904, 4, 5, 501 };

        private static void Prefix(ShopSystem __instance, bool actor, int typ, ref Dictionary<int, int> ___shopItems)
        {
            if (!Main.enabled || DateFile.instance.itemSortLists.Count ==0)
            {
                return;
            }
            
            var df = DateFile.instance;
            int counter = 0;

            IOrderedEnumerable<KeyValuePair<int, int>> val = ___shopItems.OrderBy(kv => int.Parse(df.GetItemDate(kv.Key, sorttypes[df.itemSortLists[0]], true)));
            
            while (counter <DateFile.instance.itemSortLists.Count -1)
            {
                counter++;
                val = val.ThenBy(kv => int.Parse(df.GetItemDate(kv.Key, sorttypes[df.itemSortLists[counter]], true)));
            }

            ___shopItems =  val.ToDictionary((KeyValuePair<int, int> o) => o.Key, (KeyValuePair<int, int> p) => p.Value);
            return;
        }

    }
}