using Harmony12;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace TaiwuBaseClass
{
    public class Main
    {
        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }
        public static bool Enabled { get; private set; }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
            {
                return false;
            }
            enabled = true;
            return true;
        }

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            Setting = Settings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
            return true;
        }
    }
}