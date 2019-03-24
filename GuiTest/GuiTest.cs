using Harmony12;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Diagnostics;
using UnityEngine.EventSystems;

namespace GuiTest
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
    }
    public static class Main
    {
        public static bool onOpen = false;//
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            #region 基础设置
            settings = Settings.Load<Settings>(modEntry);
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            #endregion

            HarmonyInstance harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());


            return true;
        }

        static string title = "鬼的测试";
        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }



        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(title, GUILayout.Width(300));

            //if (GUILayout.Button("测试"))
            //{
            //    ActorMenu.instance.listActorsHolder.root.gameObject.SetActive(!ActorMenu.instance.listActorsHolder.root.gameObject.activeSelf);
            //    Main.Logger.Log(ActorMenu.instance.listActorsHolder.root.ToString());
            //    Main.Logger.Log(ActorMenu.instance.listActorsHolder.ToString());
            //}

            //if (GUILayout.Button("打印"))
            //{
            //    GuiBaseUI.Main.LogAllChild(ActorMenu.instance.listActorsHolder.root);

            //}
        }

        [HarmonyPatch(typeof(ActorMenu), "CloseActorMenu")]
        public static class ActorMenu_CloseActorMenu_Patch
        {
            public static void Postfix()
            {
                Main.Logger.Log("关闭");
                StackTrace st = new StackTrace(true);
                Main.Logger.Log(st.ToString());
            }
        }

        [HarmonyPatch(typeof(DateFile), "ChangeTwoActorItem")]
        public static class DateFile_ChangeTwoActorItem_Patch
        {
            public static void Postfix(int loseItemActorId, int getItemActorId, int itemId, int itemNumber = 1, int getTyp = -1, int partId = 0, int placeId = 0)
            {
                Main.Logger.Log("massageItemTyp=" + MassageWindow.instance.massageItemTyp);
                Main.Logger.Log(loseItemActorId + " " + getItemActorId + " " + itemId + " " + itemNumber + " " + getTyp + " " + partId + placeId);
                StackTrace st = new StackTrace(true);

                Main.Logger.Log(st.ToString());
            }
        }

        //[HarmonyPatch(typeof(DateFile), "SetActorEquip")]
        //public static class DateFile_SetActorEquip_Patch
        //{
        //    public static void Postfix(int key, int equipIndex, int newEquipId)
        //    {
        //        Main.Logger.Log(key + "穿戴装备 " + equipIndex + " 新装备= " + newEquipId);
        //    }
        //}

        //[HarmonyPatch(typeof(DropObject), "OnDrop")]
        //public static class DropObject_OnDrop_Patch
        //{
        //    public static void Postfix(PointerEventData eventData)
        //    {
        //        try
        //        {
        //            Main.Logger.Log("dropObjectTyp = " + DropUpdate.instance.updateId);
        //            Main.Logger.Log(eventData.ToString());

        //            int id = DateFile.instance.ParseInt(eventData.pointerEnter.gameObject.name.Split(',')[1]);
        //            Dictionary<int, string> data1;
        //            DateFile.instance.itemsDate.TryGetValue(id, out data1);
        //            Main.Logger.Log("打印物品 pointerEnter =============================== ");
        //            foreach (var item in data1)
        //            {
        //                Main.Logger.Log(item.Key + " " + item.Value);
        //            }

        //            int id2 = DateFile.instance.ParseInt(eventData.pointerDrag.gameObject.name.Split(',')[1]);
        //            Dictionary<int, string> data2;
        //            DateFile.instance.itemsDate.TryGetValue(id2, out data2);
        //            Main.Logger.Log("打印物品 pointerDrag =============================== ");
        //            foreach (var item in data2)
        //            {
        //                Main.Logger.Log(item.Key + " " + item.Value);
        //            }
        //            Main.Logger.Log("打印完毕  =============================== ");
        //        }
        //        catch
        //        {

        //        }
        //    }
        //}

        //[HarmonyPatch(typeof(DateFile), "DragObjectEnd")]
        //public static class DateFile_DragObjectEnd_Patch
        //{
        //    public static bool Prefix(int typ)
        //    {
        //        if (!Main.enabled)
        //            return true;

        //            Main.Logger.Log("DateFile_DragObjectEnd_Patch:typ=" + typ);

        //        return true;
        //    }
        //}

        //[HarmonyPatch(typeof(DragObject), "OnBeginDrag")]
        //public static class DragObject_OnBeginDrag_Patch
        //{
        //    public static bool Prefix(PointerEventData eventData)
        //    {
        //        if (!Main.enabled)
        //            return true;

        //        Main.Logger.Log("DateFile_DragObjectEnd_Patch:eventData=" + eventData);

        //        return true;
        //    }
        //}

        //[HarmonyPatch(typeof(DateFile), "ChangeTwoActorItem")]
        //public static class DateFile_ChangeTwoActorItem_Patch
        //{
        //    public static bool Prefix(int loseItemActorId, int getItemActorId, int itemId, int itemNumber = 1, int getTyp = -1, int partId = 0, int placeId = 0)
        //    {
        //        if (!Main.enabled)
        //            return true;

        //        Main.Logger.Log("loseItemActorId=" + loseItemActorId+ " getItemActorId="+ getItemActorId + " itemId=" + itemId + " itemNumber=" + itemNumber + " getTyp=" + getTyp + " partId=" + partId + " placeId=" + placeId);

        //        return true;
        //    }
        //}

    }
}