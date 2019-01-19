using Harmony12;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace Sth4nothing.SLManager
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry) { Save(this, modEntry); }
        public bool blockAutoSave = false;
        public int maxBackupToLoad = 8;
        public int maxBackupsToKeep = 1000;
    }
    public static class Main
    {
        public static bool Enabled { get; private set; }
        public static bool forceSave = false;

        private static string logPath;
        private static string[] autoSaveState = { "关闭", "启用" };

        public static Settings settings;

        public static UnityModManager.ModEntry.ModLogger Logger;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                var userprofile = System.Environment.GetEnvironmentVariable("USERPROFILE");
                logPath = Path.Combine(userprofile,
                    @"AppData\LocalLow\Conch Ship Game\The Scroll Of Taiwu Alpha V1.0\output_log.txt"
                    );
            }
            catch (System.Exception) { }

            Logger = modEntry.Logger;
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            SevenZipHelper.SevenZipPath = Path.Combine(modEntry.Path, "7z.exe");

            return true;
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("每个存档槽最大保留备份数量(0-1000)：");

            if (int.TryParse(GUILayout.TextField(settings.maxBackupsToKeep.ToString()),
                    out int maxBackupsToKeep))
            {
                if (maxBackupsToKeep <= 1000 && maxBackupsToKeep >= 0)
                    settings.maxBackupsToKeep = maxBackupsToKeep;
            }
            GUILayout.Label("读档列表的最大存档数(0表示不受限制)");
            if (int.TryParse(GUILayout.TextField(settings.maxBackupToLoad.ToString()),
                    out int maxBackupToLoad))
            {
                if (maxBackupToLoad >= 0)
                    settings.maxBackupToLoad = maxBackupToLoad;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("禁用游戏换季存档", GUILayout.Width(200));
            settings.blockAutoSave = GUILayout.SelectionGrid(settings.blockAutoSave ? 1 : 0,
                autoSaveState, 2, GUILayout.Width(150)) == 1;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("打印log", GUILayout.Width(100)))
            {
                Log();
            }
            if (GUILayout.Button("显示log路径", GUILayout.Width(100)))
            {
                if (logPath != null && File.Exists(logPath))
                {
                    var p = new System.Diagnostics.Process();
                    p.StartInfo.FileName = "explorer.exe";
                    p.StartInfo.Arguments = "/e,/select,\"" + logPath + "\"";
                    p.Start();
                }
            }
            GUILayout.EndHorizontal();
        }
        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }


        /// <summary>
        /// 用于调试
        /// </summary>
        public static void Log()
        {
            Debug.Log("version: " + MainMenu.instance.gameVersionText.text);
            Debug.Log("dateId: " + SaveManager.DateId);
            Debug.Log("dirpath: " + SaveManager.SavePath);
            Debug.Log("backpath: " + SaveManager.BackPath);

            Debug.Log("savedFiles: ");
            if (LoadFile.savedFiles != null)
            {
                foreach (var file in LoadFile.savedFiles)
                {
                    Debug.Log("\t" + file);
                }
            }
            Debug.Log("savedInfos: ");
            if (LoadFile.savedInfos != null)
            {
                foreach (var pair in LoadFile.savedInfos)
                {
                    Debug.Log("\t" + pair.Key + ": " +
                        Newtonsoft.Json.JsonConvert.SerializeObject(pair.Value));
                }
            }
            using (var stream = new MemoryStream())
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(Settings));
                serializer.Serialize(stream, Main.settings);
                stream.Seek(0, System.IO.SeekOrigin.Begin);
                Debug.Log(System.Text.Encoding.UTF8.GetString(stream.ToArray()));
            }
            Debug.Log(settings);
        }
    }
}
