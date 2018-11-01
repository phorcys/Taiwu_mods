using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace TaiwuBaseClass
{
    public class Settings : UnityModManager.ModSettings
    {

    }

    public class Main
    {
        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }
        public static Settings settings;
        public static bool Enabled { get; private set; }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
            {
                return false;
            }
            Enabled = true;
            return true;
        }

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;         
            return true;
        }
    }
}