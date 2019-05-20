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

        public int handWidth = 20;
    }
    public static class Main
    {
        public static Canvas MainCanvas;
        public static int Layer = 0;
        public static bool onOpen = false;//
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public struct HandData
        {
           public Image bg;
            public Image hand;
            public RectTransform bgtf;
            public RectTransform handtf;
            public HandData(Image b,Image h,RectTransform btf, RectTransform htf)
            {
                bg = b;
                hand = h;
                bgtf = btf;
                handtf = htf;
            }
        }
        public static List<HandData> hands = new List<HandData>();


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
            if (MainCanvas != null)
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
            float bgR;
            float bgG;
            float bgB;

            float handR;
            float handG;
            float handB;


        int handWidth;


            GUILayout.Label(title, GUILayout.Width(300));
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("<color=#{0}{1}{2}>■■■■■■■■</color>底色"
                , (Main.settings.bgR * 255 > 16 ? "" : "0") + Convert.ToString((int)(Main.settings.bgR * 255), 16)
                , (Main.settings.bgG * 255 > 16 ? "" : "0") + Convert.ToString((int)(Main.settings.bgG * 255), 16)
                , (Main.settings.bgB * 255 > 16 ? "" : "0") + Convert.ToString((int)(Main.settings.bgB * 255), 16)));
            bgR = GUILayout.HorizontalSlider(Main.settings.bgR, 0, 1); GUILayout.Label("R:" + (Main.settings.bgR * 255).ToString("000").ToUpper());
            bgG = GUILayout.HorizontalSlider(Main.settings.bgG, 0, 1); GUILayout.Label("G:" + (Main.settings.bgG * 255).ToString("000").ToUpper());
            bgB = GUILayout.HorizontalSlider(Main.settings.bgB, 0, 1); GUILayout.Label("B:" + (Main.settings.bgB * 255).ToString("000").ToUpper());
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("<color=#{0}{1}{2}>■■■■■■■■</color>前色"
                , (Main.settings.handR * 255 > 16 ? "" : "0") + Convert.ToString((int)(Main.settings.handR * 255), 16)
                , (Main.settings.handG * 255 > 16 ? "" : "0") + Convert.ToString((int)(Main.settings.handG * 255), 16)
                , (Main.settings.handB * 255 > 16 ? "" : "0") + Convert.ToString((int)(Main.settings.handB * 255), 16)));
            handR = GUILayout.HorizontalSlider(Main.settings.handR, 0, 1); GUILayout.Label("R:" + (Main.settings.handR * 255).ToString("000").ToUpper());
            handG = GUILayout.HorizontalSlider(Main.settings.handG, 0, 1); GUILayout.Label("G:" + (Main.settings.handG * 255).ToString("000").ToUpper());
            handB = GUILayout.HorizontalSlider(Main.settings.handB, 0, 1); GUILayout.Label("B:" + (Main.settings.handB * 255).ToString("000").ToUpper());
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("设置滑动条宽度{0}", Main.settings.handWidth));
            handWidth = (int)GUILayout.HorizontalSlider(Main.settings.handWidth, 5, 30);
            GUILayout.Space(100);
            if (GUILayout.Button("使用鬼的配色"))
            {
                bgR = 0.9490196f;
                bgG = 0.509803951f;
                bgB = 0.503921571f;

                handR = 0.5882353f;
                handG = 0.807843149f;
                handB = 0.8156863f;

                handWidth = 20;
            }
            GUILayout.Space(100);
            if (GUILayout.Button("使用太吾配色"))
            {
                bgR = 0.164705882f;
                bgG = 0.164705882f;
                bgB = 0.164705882f;

                handR = 0.282352941f;
                handG = 0.094117647f;
                handB = 0.094117647f;

                handWidth = 10;
            }
            GUILayout.Space(100);
            GUILayout.EndHorizontal();


            if (
            Main.settings.bgR != bgR ||
            Main.settings.bgG != bgG ||
            Main.settings.bgB != bgB ||
            Main.settings.handR != handR ||
            Main.settings.handG != handG ||
            Main.settings.handB != handB ||
            Main.settings.handWidth != handWidth
                )
            {
                Main.settings.bgR = bgR;
                Main.settings.bgG = bgG;
                Main.settings.bgB = bgB;
                Main.settings.handR = handR;
                Main.settings.handG = handG;
                Main.settings.handB = handB;
                Main.settings.handWidth = handWidth;

                List<HandData> res = new List<HandData>();
                foreach (var item in hands)
                {
                    if (item.bg == null || item.hand == null || item.bgtf == null || item.handtf == null)
                    {
                        res.Add(item);
                    }
                    else
                    {
                        item.bg.color = new Color(bgR, bgG, bgB);
                        item.hand.color = new Color(handR, handG, handB);
                        item.bgtf.sizeDelta = new Vector2(handWidth, 0);
                        item.handtf.sizeDelta = new Vector2(20, 20);
                    }
                }
                foreach (var item in res)
                {
                    hands.Remove(item);
                }
            }

        }



        public static void LogAllChild(Transform tf, bool logSize = false, int idx = 0)
        {
            string s = "";
            for (int i = 0; i < idx; i++)
            {
                s += "-- ";
            }
            s += tf.name + " "+tf.gameObject.activeSelf;
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
                LogAllChild(child, logSize, idx);
            }
        }
    }
}