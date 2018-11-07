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

namespace GuiBaseUI
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        public float bgR = 0.9490196f;
        public float bgG = 0.509803951f;
        public float bgB = 0.503921571f;

        public float handR = 0.5882353f;
        public float handG = 0.807843149f;
        public float handB = 0.8156863f;
    }
    public static class Main
    {
        public static Canvas MainCanvas;
        public static int Layer = 0;
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

            HarmonyInstance harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            #endregion

            MainCanvas = GameObject.FindObjectOfType<Canvas>();
            if (MainCanvas!=null)
            {
                Layer = MainCanvas.gameObject.layer;
            }

            return true;
        }

        static string title = "鬼的基础界面 1.0.0";
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
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("<color=#{0}{1}{2}>■■■■底色 {3} {4} {5}■■■■</color>", Convert.ToString((int)(Main.settings.bgR * 255), 16)
                , Convert.ToString((int)(Main.settings.bgG * 255), 16), Convert.ToString((int)(Main.settings.bgB * 255), 16), Main.settings.bgR * 255, Main.settings.bgG * 255, Main.settings.bgB * 255));
            Main.settings.bgR = GUILayout.HorizontalSlider(Main.settings.bgR, 0, 1);
            Main.settings.bgG= GUILayout.HorizontalSlider(Main.settings.bgG, 0, 1);
            Main.settings.bgB = GUILayout.HorizontalSlider(Main.settings.bgB, 0, 1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("<color=#{0}{1}{2}>■■■■前色 {3} {4} {5}■■■■</color>", Convert.ToString((int)(Main.settings.handR * 255), 16)
                , Convert.ToString((int)(Main.settings.handG * 255), 16), Convert.ToString((int)(Main.settings.handB * 255), 16), Main.settings.bgR * 255, Main.settings.bgG * 255, Main.settings.bgB * 255));
            Main.settings.handR = GUILayout.HorizontalSlider(Main.settings.handR, 0, 1);
            Main.settings.handG = GUILayout.HorizontalSlider(Main.settings.handG, 0, 1);
            Main.settings.handB = GUILayout.HorizontalSlider(Main.settings.handB, 0, 1);
            GUILayout.EndHorizontal();
        }



        public static void LogAllChild(Transform tf, bool logSize = false, int idx = 0)
        {
            string s = "";
            for (int i = 0; i < idx; i++)
            {
                s += "-- ";
            }
            s += tf.name;
            if (logSize)
            {
                RectTransform rect = tf as RectTransform;
                if (rect == null)
                {
                    s += " scale=" + tf.localScale.ToString();
                }
                else
                {
                    s += " sizeDelta=" + rect.sizeDelta.ToString();
                }
            }
            Main.Logger.Log(s);

            idx++;
            for (int i = 0; i < tf.childCount; i++)
            {
                Transform child = tf.GetChild(i);
                LogAllChild(child,logSize, idx);
            }
        }
    }
}