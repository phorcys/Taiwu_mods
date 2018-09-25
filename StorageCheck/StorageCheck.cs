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

namespace StorageCheck
{

    public class Settings : UnityModManager.ModSettings
    {
        public bool displaybag = true;
        public bool displaywarehouse = true;



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
            settings.displaybag = GUILayout.Toggle(settings.displaybag, "是否显示 背包内是否有此物品 ");
            settings.displaywarehouse = GUILayout.Toggle(settings.displaywarehouse, "是否显示 仓库内是否有此物品 ");

            GUILayout.Label("说明： 点击是否启用对应功能。");
            GUILayout.EndVertical();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

    }


    /// <summary>
    ///  物品tips 显示，查询是否需要插入 背包/仓库是否有此物品显示
    /// </summary>
    [HarmonyPatch(typeof(WindowManage), "ShowItemMassage")]
    public static class WindowManage_ShowItemMassage_Patch
    {

        private static void Postfix(WindowManage __instance, int itemId, ref string ___baseWeaponMassage, ref Text ___informationMassage)
        {
            if (!Main.enabled)
            {
                return;
            }
            string text = ___baseWeaponMassage;
            if (Main.settings.displaybag  && DateFile.instance.actorItemsDate[DateFile.instance.MianActorID()].ContainsKey(itemId))
            {
                text += DateFile.instance.SetColoer(20008, string.Format("\n 背包数量: {0} ", DateFile.instance.GetItemNumber(DateFile.instance.MianActorID(), itemId)), false);
            }
            if (Main.settings.displaywarehouse &&  DateFile.instance.actorItemsDate[-999].ContainsKey(itemId))
            {
                text += DateFile.instance.SetColoer(20008, string.Format("\n 仓库数量: {0} \n", DateFile.instance.GetItemNumber(-999, itemId)), false);
            }

            ___baseWeaponMassage = text;
            ___informationMassage.text = text;

            return;
        }

    }



}