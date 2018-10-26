using System;
using System.Reflection;
using UnityModManagerNet;
using Harmony12;
using UnityEngine;
using UnityEngine.UI;

namespace IllustratedHandbook
{

    public class Main : MonoBehaviour
    {
        public static bool isEnabled;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static bool windowOpened = false;
        public static IllustratedHandbookUI mWindow;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;

            modEntry.OnToggle = OnToggle;

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
                return false;

            isEnabled = value;

            if (isEnabled)
            {
                try
                {
                    // Create GameObject in the sence for handling actions
                    new GameObject(typeof(Main).FullName, typeof(Main));
                    mWindow = (IllustratedHandbookUI)new GameObject((typeof(IllustratedHandbookUI).FullName), typeof(IllustratedHandbookUI)).GetComponent(typeof(IllustratedHandbookUI));
                }
                catch (Exception e)
                {
                    Logger.Log(e.ToString());
                    return false;
                }

            }

            return true;
        }

        private void Awake()
        {
            // Do not destroy this object across sences
            DontDestroyOnLoad(this);
        }
    }
}