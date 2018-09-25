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

namespace InventorySort
{

    public class Settings : UnityModManager.ModSettings
    {
        public int mainsort_category = 0;
        public int subsort_category = 1;


        public int getmaincate()
        {
            if(mainsort_category >0 && mainsort_category <ALLCATE.Length)
            {
                mainsort_category = 0;
            }
            return ALLCATE[mainsort_category].Key;
        }

        public int getsubcate()
        {
            if (subsort_category > 0 && subsort_category < ALLCATE.Length)
            {
                subsort_category = 1;
            }
            return ALLCATE[subsort_category].Key;
        }

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public static KeyValuePair<int, string>[] ALLCATE = new KeyValuePair<int, string>[]
        {
            new KeyValuePair<int, string>( 4, "主分类" ),
            new KeyValuePair<int, string>(  98, "次分类" ),
            new KeyValuePair<int, string>(  0, "道具名"  ),
            new KeyValuePair<int, string>(   8, "品质/颜色"  ),
            new KeyValuePair<int, string>(  904, "物品价值" ),
        };
    }


    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            settings = Settings.Load<Settings>(modEntry);

            Logger = modEntry.Logger;

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
            GUILayout.BeginVertical("Box");
            GUILayout.Label("主排序方法：");
            settings.mainsort_category = GUILayout.SelectionGrid(settings.mainsort_category, Settings.ALLCATE.Select(o => o.Value).ToArray(),3);
            GUILayout.Label("次排序方法：");
            settings.subsort_category = GUILayout.SelectionGrid(settings.subsort_category, Settings.ALLCATE.Select(o => o.Value).ToArray(), 3);

            GUILayout.Label("说明： 点击选择 主要和次要 排序方法， 会先按照主方法排序，再按照次方法排序。");
            GUILayout.EndVertical();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

    }


    /// <summary>
    ///  个人 背包/仓库 获取数据接口拦截，排序后发送结果
    /// </summary>
    [HarmonyPatch(typeof(ActorMenu), "GetActorItems")]
    public static class ActorMenu_GetActorItems_Patch
    {

        private static void Postfix(ActorMenu __instance, ref Dictionary<int, int> __result, int key, int typ)
        {
            if (!Main.enabled)
            {
                return;
            }

            __result = (from o in __result
                        orderby int.Parse(DateFile.instance.GetItemDate(o.Key, Main.settings.getmaincate(), true)) * 10000 
                        + int.Parse(DateFile.instance.GetItemDate(o.Key, Main.settings.getsubcate(), true))
                        select o).ToDictionary((KeyValuePair<int, int> o) => o.Key, (KeyValuePair<int, int> p) => p.Value);

            return;
        }

    }


    /// <summary>
    ///  商店 获取数据接口拦截，提前修改商店 this.sellItems 排序
    /// </summary>
    [HarmonyPatch(typeof(ShopSystem), "SetItems")]
    public static class ShopSystem_SetItems_Patch
    {

        private static void Prefix(ShopSystem __instance, bool actor, int typ, ref Dictionary<int,int> ___shopItems)
        {
            if (!Main.enabled)
            {
                return;
            }

            ___shopItems = ___shopItems.OrderBy(o => 
            (int.Parse(DateFile.instance.GetItemDate(o.Key, Main.settings.getmaincate(), true)) * 10000 
                + int.Parse(DateFile.instance.GetItemDate(o.Key, Main.settings.getsubcate(), true)))
            ).ToDictionary((KeyValuePair<int, int> o) => o.Key, (KeyValuePair<int, int> p) => p.Value);

            

            return;
        }

    }
}