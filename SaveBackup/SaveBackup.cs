using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony12;
using Ionic.Zip;
using UnityModManagerNet;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SaveBackup
{
    using CharId = System.Int32;
    using OpinionValue = System.Int32;


    public class Settings : UnityModManager.ModSettings
    {
        public const uint DEFAULT_MAX_BACKUPS_TO_KEEP = 5;
        public uint maxBackupsToKeep = DEFAULT_MAX_BACKUPS_TO_KEEP;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    public static class Main
    {
        public static bool enabled;
        public static Initializer initializer = new Initializer();
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public class Initializer
        {
            public Initializer()
            {
                string[] assemblyNames = {
                    "DotNetZip.dll",
                    "I18N.dll",
                    "I18N.West.dll",
                };

                string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                foreach (var name in assemblyNames)
                {
                    try { Assembly.LoadFrom(Path.Combine(assemblyFolder, name)); } catch(Exception ex) { Console.WriteLine(ex.ToString());  }
                }
            }
        }

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            settings = Settings.Load<Settings>(modEntry);

            Logger = modEntry.Logger;

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

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
            GUILayout.BeginHorizontal(GUILayout.Width(400));

            GUILayout.Label("每个存档槽最大保留备份数量：");

            var maxBackupsToKeep = GUILayout.TextField(settings.maxBackupsToKeep.ToString(), 3);
            if (GUI.changed && !uint.TryParse(maxBackupsToKeep, out settings.maxBackupsToKeep))
            {
                settings.maxBackupsToKeep = 0;
            }

            GUILayout.EndHorizontal();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

    }

    public static class SaveManager
    {
        public const int BEFORE_SAVE_BACKUP = 0,
                         AFTER_LOADING_BACKUP = 1;

        public static void Backup(int backupType)
        {
            // 获取当前游戏存档的路径
            var dirPathMethod = typeof(SaveDateFile).GetMethod("Dirpath", BindingFlags.Instance | BindingFlags.NonPublic);
            string pathToBackup = (string) dirPathMethod.Invoke(SaveDateFile.instance, new object[1] { -1 });

            // 确保pathToBackup末尾没有"/"或者"\\"
            pathToBackup = Path.GetDirectoryName(pathToBackup + "/");

            switch (backupType)
            {
            case BEFORE_SAVE_BACKUP:
                BackupBeforeSave(pathToBackup);
                return;

            case AFTER_LOADING_BACKUP:
                BackupAfterLoad(pathToBackup);
                return;

            default:
                throw new System.ArgumentException("invalid backupType");
            }
        }

        // 执行存档前备份
        private static void BackupBeforeSave(string pathToBackup)
        {
            if (Main.settings.maxBackupsToKeep == 0)
            {
                return;
            }

            string folderName = Path.GetFileName(pathToBackup);
            string backupStoragePath = GetBackupStoragePath(pathToBackup);

            string backupFilePattern = folderName + ".save.???.zip";
            string backupFileFormat = folderName + ".save.{0:D3}.zip";

            int backupIndex;

            // 获取所有当前存档的备份
            var backupFiles = Directory.GetFiles(backupStoragePath, backupFilePattern, SearchOption.TopDirectoryOnly);
            if (backupFiles.Count() < Main.settings.maxBackupsToKeep)
            {
                // 若数量未超上限，则直接累加计数
                backupIndex = backupFiles.Count();
            }
            else
            {
                // 若数量超过上限，将最早的一个删掉并且平移所有备份
                Array.Sort(backupFiles, StringComparer.InvariantCulture);
                try { File.Delete(backupFiles[0]); } catch (Exception ex) { Console.WriteLine(ex.ToString()); }

                for (int i = 1; i < backupFiles.Count(); i++)
                {
                    try { File.Move(backupFiles[i], backupFiles[i-1]); } catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                }

                backupIndex = (int) Main.settings.maxBackupsToKeep - 1;
            }

            // 保存备份
            var targetFile = backupStoragePath + "/" + string.Format(backupFileFormat, backupIndex);
            BackupFolderToFile(pathToBackup, targetFile);
        }

        // 执行读档后备份
        private static void BackupAfterLoad(string pathToBackup)
        {
            var targetFile = GetBackupStoragePath(pathToBackup) + "/" + Path.GetFileName(pathToBackup) + ".load.zip";
            BackupFolderToFile(pathToBackup, targetFile);
        }

        // 压缩到备份路径
        private static void BackupFolderToFile(string pathToBackup, string targetFile)
        {
            using(var zip = new ZipFile())
            {
                zip.AddDirectory(pathToBackup);
                zip.Save(targetFile);
            }
        }

        // 返回存档备份存储路径
        private static string GetBackupStoragePath(string pathToBackup)
        {
            string path = Directory.GetParent(pathToBackup).FullName + "/SaveBackup";

            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }

            return path;
        }
    }

    /// <summary>
    ///  回合存档前自动备份游戏存档
    /// </summary>
    [HarmonyPatch(typeof(SaveDateFile), "LateUpdate")]
    public static class SaveDateFile_LateUpdate_Patch
    {

        private static void Prefix(SaveDateFile __instance)
        {
            if (!Main.enabled || !__instance.saveSaveDate) { return; }

            try
            {
                SaveManager.Backup(SaveManager.BEFORE_SAVE_BACKUP);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    public static class LFSM
    {
        public enum State {
            NOT_LOADING = 0,    // 没在读
            LOADING,            // 读档
            OTHER_LOADING,      // 读其他东西
        }

        public static State state = State.NOT_LOADING;
    }

    /// <summary>
    ///  成功读档后自动备份游戏存档
    /// </summary>
    [HarmonyPatch(typeof(ui_Loading), "LoadingScene")]
    public static class Loading_LoadingScene_Patch
    {

        private static void Prefix(bool newGame, int teachingId, int loadingDateId)
        {
            if (!Main.enabled) { return; }

            LFSM.state = (teachingId == -1 && !newGame && loadingDateId != 0) ? LFSM.State.LOADING : LFSM.State.OTHER_LOADING;
        }

    }

    [HarmonyPatch(typeof(ui_Loading), "Update")]
    public static class Loading_Update_Patch
    {
        private static void Prefix(bool ___loadingEnd)
        {
            if (!Main.enabled) { return; }

            if (LFSM.state == LFSM.State.LOADING)
            {
                if (___loadingEnd)
                {
                    try
                    {
                        SaveManager.Backup(SaveManager.AFTER_LOADING_BACKUP);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    LFSM.state = LFSM.State.NOT_LOADING;
                }
            }
        }
    }
}