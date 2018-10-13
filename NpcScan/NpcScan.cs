using System.Reflection;
using Harmony12;
using UnityModManagerNet;

namespace NpcScan
{
    public static class Main
    {
        public static bool enabled;
        public static bool uiIsShow = false;

        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            if (!Main.uiIsShow)
            {
                UI.Load();
                Main.uiIsShow = true;
            }
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
        }
    }
}