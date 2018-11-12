
using UnityEngine;
using Harmony12;
using UnityModManagerNet;
using System.Reflection;
using System.Collections.Generic;
using NotEnoughHarvest;

namespace NotEnoughHarvest
{
    public class Coord
    {
        public string x = "";
        public string y = "";
    }
    public class Settings : UnityModManager.ModSettings
    {
        public List<Coord> coords = new List<Coord>();
        public HashSet<int> valid_coords = new HashSet<int>();
        public static int R = 6;
        public static int L = 2 * R + 1;
        public static int TAIWU = (L * L - 1) / 2;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save<Settings>(this, modEntry);
        }


    }

    public static class Main
    {
        public static UnityModManager.ModEntry.ModLogger logger;
        public static Settings settings;
        public static bool enabled;
        
        static public bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            logger.Log("hello.");
            settings = Settings.Load<Settings>(modEntry);
            var harmony = HarmonyInstance.Create("nz.carso.taiwu_mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal(GUILayout.Width(200));
            if (GUILayout.Button("add"))
                settings.coords.Add(new Coord());
            if (GUILayout.Button("delete"))
            {
                int len = settings.coords.Count;
                if (len > 0)
                    settings.coords.RemoveAt(len - 1);
            }
            GUILayout.EndHorizontal();
            foreach (Coord coord in settings.coords)
            {
                GUILayout.BeginHorizontal(GUILayout.Width(150));
                GUILayout.Label("x");
                coord.x = GUILayout.TextField(coord.x);
                GUILayout.Label("y");
                coord.y = GUILayout.TextField(coord.y);
                GUILayout.EndHorizontal();
            }
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
                return false;
            enabled = value;
            return true;
        }     
    }
    [HarmonyPatch(typeof(HomeSystem), "UpdateShop")]
    public static class PatchUpdateShop
    {
        static void Prefix(HomeSystem __instance, ref int partId, ref int placeId, ref int buildingIndex)
        {
            // HomeSystem.isTaiWu
            if (DateFile.instance.GetNewMapDate(partId, placeId, 96) != "1") {
                return;
            }
            if (Main.settings.valid_coords.Contains(buildingIndex))
            {
                int[] infos = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
                Dictionary<int, string> base_infos = DateFile.instance.basehomePlaceDate[infos[0]];
                int full = int.Parse(base_infos[91]);
                if (full > 0)
                {
                    int x = (buildingIndex - Settings.TAIWU) % Settings.L;
                    int y = -(buildingIndex - Settings.TAIWU) / Settings.L;
                    Main.logger.Log("boosting " + base_infos[0] + " at coordinate (" + x + ", " + y + ")");
                    DateFile.instance.SetHomeBuildingValue(partId, placeId, buildingIndex, 11, full);
                }
            }
        }
    }
    [HarmonyPatch(typeof(UIDate), "TrunChange")]  // Trun...
    public static class PatchTrunChange
    {
        // prune coordinates
        static void Prefix()
        {
            Main.settings.valid_coords.Clear();
            int R = Settings.R;
            int L = Settings.L;
            int CENTER = Settings.TAIWU;
            foreach (Coord coord in Main.settings.coords)
            {
                int x;
                int y;
                if (!int.TryParse(coord.x, out x) || !int.TryParse(coord.y, out y))
                {
                    Main.logger.Log("skipping invalid input(parse error): (" + coord.x + ", " + coord.y + ")");
                    continue;
                }
                if (x > R || x < -R || y > R || y < -R)
                {
                    Main.logger.Log("skipping invalid input(range error): (" + coord.x + ", " + coord.y + ")");
                    continue;
                }
                int index = CENTER - y * L + x;
                Main.settings.valid_coords.Add(index);
            }
        }
    }
}