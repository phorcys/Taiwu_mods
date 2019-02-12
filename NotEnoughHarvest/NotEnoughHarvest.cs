
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
        public bool greedy = false;
        public List<Coord> coords = new List<Coord>();
        public HashSet<int> valid_coords = new HashSet<int>();
        public string L = "13";
        public int _L = 13;
        //public int TAIWU = (_L * _L - 1) / 2;
        public int TAIWU = -1;
        public bool debug = false;

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
            GUILayout.BeginHorizontal();
            string s;
            if (settings.greedy)
                s = "on";
            else
                s = "off";
            settings.greedy = GUILayout.Toggle(settings.greedy, " 贪婪模式");
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.Label("说明：选择后对所有建筑生效，下方坐标转变为除外的坐标 (当前: " + s + ")");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(200));
            if (GUILayout.Button("add", GUILayout.Width(100)))
                settings.coords.Add(new Coord());
            if (GUILayout.Button("delete", GUILayout.Width(100)))
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

            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            settings.debug = GUILayout.Toggle(settings.debug, " debug");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("说明：选择后在log里打印一些额外信息");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(150));
            GUILayout.Label("太吾村边长");
            settings.L = GUILayout.TextField(settings.L);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("说明：默认13，该设置未测试");
            GUILayout.EndHorizontal();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            int L;
            if (!int.TryParse(settings.L, out L)) {
                if (settings.debug)
                    Main.logger.Log("OnSaveGUI: invalid taiwu length, setting to 13 (default)...");
                settings.L = "13";
                L = 13;
            }

            settings._L = L;
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
            // HomeSystem.IsTaiWu
            if (DateFile.instance.GetNewMapDate(partId, placeId, 96) != "1") {
                return;
            }
            bool pass = Main.settings.greedy ^ Main.settings.valid_coords.Contains(buildingIndex);
            if (pass)
            {
                int[] infos = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
                Dictionary<int, string> base_infos = DateFile.instance.basehomePlaceDate[infos[0]];
                int full = int.Parse(base_infos[91]);
                if (full > 0)
                {
                    int x = (buildingIndex - Main.settings.TAIWU) % Main.settings._L;
                    int y = -(buildingIndex - Main.settings.TAIWU) / Main.settings._L;
                    if (Main.settings.debug)
                        Main.logger.Log("boosting " + base_infos[0] +
                            " at coordinate (" + x + ", " + y + ")" +
                            ", buildingIndex " + buildingIndex);
                    DateFile.instance.SetHomeBuildingValue(partId, placeId,
                                                           buildingIndex, 11, full);
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
            if (Main.settings.debug)
                Main.logger.Log("entering next month, perparing...");
            Main.settings.valid_coords.Clear();
            int L;
            if (!int.TryParse(Main.settings.L, out L)) {
                if (Main.settings.debug)
                    Main.logger.Log("invalid taiwu length, setting to 13 (default)...");
                Main.settings.L = "13";
                L = 13;
            }
            if (Main.settings.debug)
                Main.logger.Log("final taiwu length: " + L);
            Main.settings._L = L;
            int R = (L - 1) / 2;
            Main.settings.TAIWU = (L * L - 1) / 2;
            foreach (Coord coord in Main.settings.coords)
            {
                int x;
                int y;
                if (!int.TryParse(coord.x, out x) || !int.TryParse(coord.y, out y))
                {
                    if (Main.settings.debug)
                        Main.logger.Log("skipping invalid input(parse error): (" +
                            coord.x + ", " + coord.y + ")");
                    continue;
                }
                if (x > R || x < -R || y > R || y < -R)
                {
                    if (Main.settings.debug)
                        Main.logger.Log("skipping invalid input(range error): (" +
                            coord.x + ", " + coord.y + ")");
                    continue;
                }
                int index = Main.settings.TAIWU - y * L + x;
                Main.settings.valid_coords.Add(index);
            }
            if (Main.settings.debug) {
                Main.logger.Log("done, got " + Main.settings.valid_coords.Count + " coordinates, ");
                // Main.logger.Log("corresponding buildingIndex: ");
            }
        }
    }
}
