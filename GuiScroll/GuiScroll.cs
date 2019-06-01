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

namespace GuiScroll
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        public float scrollSpeed = 10;
        public bool openActor = true;
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
            //ActorPatch.Init(modEntry);

            ActorMenuActorListPatch.Init(modEntry);

            HarmonyInstance harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
        }

        static string title = "鬼的滚动优化";
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

    }
}