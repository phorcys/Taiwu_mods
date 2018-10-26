using Harmony12;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityModManagerNet;

namespace Samsara1k
{
    [HarmonyPatch(typeof(ActorMenu), "NewMianActor")]
    public class ActorMenu_NewMainActor_Patch
    {
        public static void Prefix()
        {
            if (Main.Enabled)
            {
                DateFile.instance.samsara += Main.Setting.samsara - 1;
            }
        }
    }

    [HarmonyPatch(typeof(MassageWindow), "EndEvent11_1")]
    public class MassageWindow_EndEvent11_1_Patch
    {
        public static void Prefix()
        {
            if (Main.Enabled)
            {
                if (MassageWindow.instance.eventValue[1] == 1)
                {
                    DateFile.instance.samsara += Main.Setting.samsara - 1;
                }
            }
        }
    }

    public class Main
    {
        public static UnityModManager.ModEntry.ModLogger logger;

        public static bool Enabled { get; private set; }

        public static Settings Setting { get; private set; }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            Setting = Settings.Load<Settings>(modEntry);
            modEntry.OnToggle = new Func<UnityModManager.ModEntry, bool, bool>(OnToggle);
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }
        
        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("每次传剑增加轮回次数:", GUILayout.Width(180));
            int samsara;
            if (int.TryParse(GUILayout.TextArea(Setting.samsara.ToString(), GUILayout.Width(60)), out samsara))
            {
                if (samsara >= 1)
                {
                    Setting.samsara = samsara;
                }
            }
            GUILayout.EndHorizontal();
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Setting.Save(modEntry);
        }
    }

    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        public int samsara = 100;
    }
}