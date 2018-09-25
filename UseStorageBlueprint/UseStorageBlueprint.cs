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

namespace UseStorageBlueprint
{

 

    public static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());


            Logger = modEntry.Logger;

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
    ///  建造界面图纸列表显示，追加仓库图纸
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "GetItem")]
    public static class HomeSystem_GetItem_Patch
    {

        private static void Postfix(HomeSystem __instance, Transform ___itemHolder)
        {
            if (!Main.enabled)
            {
                return;
            }

            int num = -999;
            List<int> list = new List<int>(ActorMenu.instance.GetActorItems(num, 0).Keys);
            for (int i = 0; i < list.Count; i++)
            {
                int num2 = list[i];
                if (int.Parse(DateFile.instance.GetItemDate(num2, 301, true)) > 0)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(ActorMenu.instance.itemIconNoDrag, Vector3.zero, Quaternion.identity);
                    gameObject.name = "Item," + num2;
                    gameObject.transform.SetParent(___itemHolder, false);
                    Image component = gameObject.transform.Find("ItemBack").GetComponent<Image>();
                    component.sprite = GetSprites.instance.itemBackSprites[int.Parse(DateFile.instance.GetItemDate(num2, 4, true))];
                    component.color = ActorMenu.instance.LevelColor(int.Parse(DateFile.instance.GetItemDate(num2, 8, true)));
                    gameObject.GetComponent<Toggle>().group = ___itemHolder.GetComponent<ToggleGroup>();
                    gameObject.transform.Find("ItemNumberText").GetComponent<Text>().text = "×" + DateFile.instance.GetItemNumber(num, num2);
                    GameObject gameObject2 = gameObject.transform.Find("ItemIcon").gameObject;
                    gameObject2.name = "ItemIcon," + num2;
                    gameObject2.GetComponent<Image>().sprite = GetSprites.instance.itemSprites[int.Parse(DateFile.instance.GetItemDate(num2, 98, true))];
                }
            }


            return;
        }

    }


    /// <summary>
    ///  建造界面图纸列表 如果需要，销毁仓库中对应 图纸
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "MakeNewBuilding")]
    public static class HomeSystem_MakeNewBuilding_Patch
    {

        private static void Postfix(HomeSystem __instance, int ___useItemId)
        {
            if (!Main.enabled)
            {
                return;
            }

            if (! DateFile.instance.actorItemsDate[DateFile.instance.MianActorID()].ContainsKey(___useItemId))
            {
                DateFile.instance.LoseItem(-999, ___useItemId, 1, true);
            }


            return;
        }

    }

}