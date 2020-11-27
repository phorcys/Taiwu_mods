using System;
using System.Reflection;
using UnityModManagerNet;
using Harmony12;
using UnityEngine;
using UnityEngine.UI;

namespace IllustratedHandbook
{

    public class Main
    {
        public static bool isEnabled;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static bool windowOpened = false;
        public static IllustratedHandbookUI mWindow;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            isEnabled = value;
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("请在正式游戏场景使用。第一次使用需点击下面的打开窗口按钮，之后可以用Crtl+F11来快速关闭打开");
            if (GUILayout.Button("打开窗口"))
            {
                Logger.Log("打开窗口");
                if (mWindow == null)
                {
                    mWindow = (IllustratedHandbookUI)new GameObject((typeof(IllustratedHandbookUI).FullName), typeof(IllustratedHandbookUI)).GetComponent(typeof(IllustratedHandbookUI));
                }
                mWindow.Show();
                //关闭manager窗口
                UnityModManager.UI.Instance.ToggleWindow(false);
            }
        }
    }

}