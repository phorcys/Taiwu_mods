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


namespace InventorySort
{

    public class Settings : UnityModManager.ModSettings
    {
        public bool isdesc = false;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

    }


    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());


            settings = Settings.Load<Settings>(modEntry);


            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
                return false;

            enabled = value;

            return true;
        }
        static void OnGUI(UnityModManager.ModEntry modEntry)
        {

            settings.isdesc = GUILayout.Toggle(settings.isdesc, "正序还是逆序，启用=逆序");

        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        public static List<int> sorttypes = new List<int>() { 8, 904, 4, 5, 501 };
    }




    /// <summary>
    ///  商店 获取数据接口拦截，提前修改商店 this.sellItems 排序
    /// </summary>
    [HarmonyPatch(typeof(ShopSystem), "SetItems")]
    public static class ShopSystem_SetItems_Patch
    {


        private static void Prefix(ShopSystem __instance, bool actor, int typ, ref Dictionary<int, int> ___shopItems)
        {
            if (!Main.enabled || DateFile.instance.itemSortLists.Count == 0)
            {
                return;
            }

            var df = DateFile.instance;

            IOrderedEnumerable<KeyValuePair<int, int>> val = null;
            if (Main.settings.isdesc == true)
            {
                val = ___shopItems.OrderByDescending(kv => int.Parse(df.GetItemDate(kv.Key, 999, true))).OrderByDescending(kv => (10000* int.Parse(df.GetItemDate(kv.Key, Main.sorttypes[df.itemSortLists[0]], true)) 
                                                             + int.Parse(df.GetItemDate(kv.Key, Main.sorttypes[df.itemSortLists[1]], true))));
            }
            else
            {
                val = ___shopItems.OrderBy(kv => int.Parse(df.GetItemDate(kv.Key, 999, true))).OrderBy(kv => (10000 * int.Parse(df.GetItemDate(kv.Key, Main.sorttypes[df.itemSortLists[0]], true))
                                                             + int.Parse(df.GetItemDate(kv.Key, Main.sorttypes[df.itemSortLists[1]], true))));
            }


            ___shopItems = val.ToDictionary((KeyValuePair<int, int> o) => o.Key, (KeyValuePair<int, int> p) => p.Value);
            return;
        }

    }

    /// <summary>
    ///  替换排序
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "GetItemSort")]
    public static class DateFile_GetItemSort_Patch
    {
        public static List<int> new_sort(List<int> sort_items)
        {
            var df = DateFile.instance;

            IOrderedEnumerable<int> val = null;
            if (Main.settings.isdesc == true)
            {
                val = sort_items.OrderByDescending(kv =>  int.Parse(df.GetItemDate(kv, 999, true))).OrderByDescending(kv => (10000 * int.Parse(df.GetItemDate(kv, Main.sorttypes[df.itemSortLists[0]], true))
                                                             + int.Parse(df.GetItemDate(kv, Main.sorttypes[df.itemSortLists[1]], true))));
            }
            else
            {
                val = sort_items.OrderBy(kv => int.Parse(df.GetItemDate(kv, 999, true))).OrderBy(kv => (10000 * int.Parse(df.GetItemDate(kv, Main.sorttypes[df.itemSortLists[0]], true))
                                                             + int.Parse(df.GetItemDate(kv, Main.sorttypes[df.itemSortLists[1]], true))));
            }

            return val.ToList();
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Main.Logger.Log(" Transpiler init codes ");
            var codes = new List<CodeInstruction>(instructions);

            var foundtheforend = true;
            int startIndex = 0;


            if (foundtheforend)
            {
                var injectedCodes = new List<CodeInstruction>();

                // 注入 IL code 
                //
                injectedCodes.Add(new CodeInstruction(OpCodes.Ldarg_1));
                injectedCodes.Add(new CodeInstruction(OpCodes.Call, typeof(DateFile_GetItemSort_Patch).GetMethod("new_sort")));
                injectedCodes.Add(new CodeInstruction(OpCodes.Ret));

                codes.InsertRange(startIndex, injectedCodes);
            }
            else
            {
                Main.Logger.Log(" game changed ... this mod failed to find code to patch...");
            }

            //Main.Logger.Log(" dump the patch codes ");

            //for (int i = 0; i < codes.Count; i++)
            //{
            //    Main.Logger.Log(String.Format("{0} : {1}  {2}", i, codes[i].opcode, codes[i].operand));
            //}
            return codes.AsEnumerable();
        }
    }

}