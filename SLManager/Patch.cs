using Harmony12;
using Ionic.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityModManagerNet;


namespace Sth4nothing.SLManager
{
    [HarmonyPatch(typeof(Encoding), "GetEncoding", new Type[] { typeof(int) })]
    public static class Encoding_GetEncoding_Path
    {
        public static bool Prefix(int codepage, ref Encoding __result)
        {
            if (!Main.Enabled || codepage != 437)
                return true;
            
            __result = new I18N.West.CP437();
            return false;
        }
    }
    [HarmonyPatch(typeof(WorldMapSystem), "Start")]
    public static class WorldMapSystem_Start_Patch
    {
        public static void Postfix()
        {
            if (Main.Enabled)
            {
                UI.Load();

                Transform parent = GameObject.Find("ResourceBack").transform;

                GameObject loadBtn = UnityEngine.Object.Instantiate(
                    GameObject.Find("EncyclopediaButton,609"),
                    new Vector3(1620f, -30f, 0), Quaternion.identity);
                loadBtn.name = "LoadButton2";
                loadBtn.tag = "SystemIcon";
                loadBtn.transform.SetParent(parent, false);
                loadBtn.transform.localPosition = new Vector3(1620f, -30f, 0);
                Selectable loadButton = loadBtn.GetComponent<Selectable>();
                ((Image)loadButton.targetGraphic).sprite =
                    Resources.Load<Sprite>("Graphics/Buttons/StartGameButton_NoColor");
                loadBtn.AddComponent<MyPointerClick>();

                GameObject saveBtn = UnityEngine.Object.Instantiate(
                    GameObject.Find("EncyclopediaButton,609"),
                    new Vector3(1570f, -30f, 0), Quaternion.identity);
                saveBtn.name = "SaveButton";
                saveBtn.tag = "SystemIcon";
                saveBtn.transform.SetParent(parent, false);
                saveBtn.transform.localPosition = new Vector3(1570f, -30f, 0);
                Selectable saveButton = saveBtn.GetComponent<Selectable>();
                ((Image)saveButton.targetGraphic).sprite =
                    Resources.Load<Sprite>("Graphics/Buttons/StartGameButton");
                saveBtn.AddComponent<MyPointerClick>();

                GameObject loadBtn2 = GameObject.Find("EncyclopediaButton,609");
                loadBtn2.name = "LoadButton";
                Selectable loadButton2 = loadBtn2.GetComponent<Selectable>();
                ((Image)loadButton2.targetGraphic).sprite =
                    Resources.Load<Sprite>("Graphics/Buttons/StartGameButton_NoColor");
                loadBtn2.AddComponent<MyPointerClick>();
            }
        }
    }

    public class MyPointerClick : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Main.Enabled) return;
            if (gameObject.name == "LoadButton")
            {
                LoadFiles();
            }
            else if (gameObject.name == "LoadButton2")
            {
                YesOrNoWindow.instance.SetYesOrNoWindow(4646, "快速载入",
                    DateFile.instance.massageDate[701][2].Replace("返回主菜单", "载入旧存档")
                    .Replace("返回到游戏的主菜单…\n", ""), false, true);
            }
            else if (gameObject.name == "SaveButton")
            {
                Main.forceSave = true;
                SaveDateFile.instance.SaveGameDate();
            }
        }

        public static void LoadFiles()
        {
            var files = Directory.GetFiles(SaveManager.BackPath,
                $"Date_{SaveManager.DateId}.save.???.zip",
                SearchOption.TopDirectoryOnly);
            Array.Sort(files, StringComparer.InvariantCulture);

            LoadFile.savedFiles = new List<string>();

            //var saveZip = Path.Combine(SaveManager.BackPath,
            //    $"Date_{SaveManager.DateId}.save.zip");
            if (File.Exists(Path.Combine(SaveManager.SavePath, "TW_Save_Date_0.twV0"))
                || File.Exists(Path.Combine(SaveManager.SavePath, "TW_Save_Date_0.tw")))
                LoadFile.savedFiles.Add(SaveManager.SavePath);

            if (Main.settings.maxBackupToLoad > 0)
            {
                LoadFile.savedFiles.AddRange(files.Reverse().Take(Main.settings.maxBackupToLoad));
            }
            else
            {
                LoadFile.savedFiles.AddRange(files);
            }

            LoadFile.ParseFiles();

            UI.Instance.ShowData();
        }
    }

    [HarmonyPatch(typeof(OnClick), "Index")]
    public static class OnClick_Index_Patch
    {
        public static void Postfix()
        {
            if (!Main.Enabled) return;
            if (OnClick.instance.ID == 4646)
            {
                DateFile.instance.SetEvent(new int[] { 0, -1, 1001 }, true, true);
                DateFile.instance.Initialize(SaveDateFile.instance.dateId);
                YesOrNoWindow.instance.CloseYesOrNoWindow();
                YesOrNoWindow.instance.yesOrNoWindow.sizeDelta = new Vector2(720f, 280f);
                OnClick.instance.Over = true;
            }
        }
    }

    public class SaveData
    {
        public string name;
        public int year;
        public int samsara;
        public int trun;
        public string position;
        public string playtime;

        public SaveData() : this(null, -1, -1, -1, null, null)
        { }
        public SaveData(string name, int year, int samsara, int turn, string position, DateTime playtime) :
            this(name, year, samsara, turn, position, playtime.ToString("yyyy - MM - dd   [ HH : mm ]"))
        { }
        public SaveData(string name, int year, int samsara, int turn, string position,
            string playtime)
        {
            this.name = name;
            this.year = year;
            this.samsara = samsara;
            this.trun = turn;
            this.position = position;
            this.playtime = playtime;
        }

        public int GetTurn()
        {
            return Mathf.Max(DateFile.instance.GetDayTrun(trun), 0) / 2;
        }
    }

    public static class LoadFile
    {
        /// <summary>
        /// 需要解析的压缩存档路径
        /// </summary>
        public static List<string> savedFiles;
        /// <summary>
        /// 解析了的存档
        /// </summary>
        public static Dictionary<string, SaveData> savedInfos;

        public const string format = "yyyy - MM - dd   [ HH : mm ]";

        private static object lockObj = new object();

        /// <summary>
        /// 解析压缩存档列表
        /// </summary>
        public static void ParseFiles()
        {
            savedInfos = new Dictionary<string, SaveData>();
            var threads = new Queue<System.Threading.Thread>();
            foreach (var file in savedFiles)
            {
                var thread = new System.Threading.Thread(
                    new System.Threading.ParameterizedThreadStart(ParseThread))
                {
                    IsBackground = true
                };
                threads.Enqueue(thread);
                thread.Start(file);
            }
            while (threads.Count > 0)
            {
                threads.Dequeue().Join();
            }

            savedFiles.Sort((f1, f2) =>
            {
                if (!savedInfos.ContainsKey(f1))
                    return -1;
                if (!savedInfos.ContainsKey(f2))
                    return 1;
                var t1 = DateTime.ParseExact(savedInfos[f1].playtime, format, null);
                var t2 = DateTime.ParseExact(savedInfos[f2].playtime, format, null);
                return -t1.CompareTo(t2);
            });
        }

        public static void ParseThread(object file)
        {
            var path = file as string;
            try
            {
                Debug.Log("Parse: " + file);
                var data = Parse(path);
                if (data == null)
                    throw new Exception();

                lock (lockObj)
                {
                    savedInfos.Add(path, data);
                }
            }
            catch (Exception e)
            {
                Debug.Log("[ERROR]" + e.ToString());
            }
        }

        /// <summary>
        /// 解析指定压缩存档
        /// </summary>
        /// <param name="path">压缩存档路径</param>
        /// <returns></returns>
        public static SaveData Parse(string path)
        {
            return path.EndsWith(".zip") ? ParseZip(path) : ParseDirectory(path);
        }

        /// <summary>
        /// 存档为游戏存档
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static SaveData ParseDirectory(string path)
        {
            SaveData data = null;
            if (File.Exists(Path.Combine(path, "date.json")))
            {
                var content = File.ReadAllText(Path.Combine(path, "date.json"));
                data = JsonConvert.DeserializeObject(content, typeof(SaveData)) as SaveData;
            }
            else if (!File.Exists(Path.Combine(path, "TW_Save_Date_0.twV0"))
                      && !File.Exists(Path.Combine(path, "TW_Save_Date_0.tw")))
            {
                throw new Exception(path);
            }
            else
            {
                string file = null;
                bool rijndeal = true;
                if (File.Exists(Path.Combine(path, "TW_Save_Date_0.twV0")))
                {
                    file = Path.Combine(path, "TW_Save_Date_0.twV0");
                    rijndeal = false;
                }
                else
                {
                    file = Path.Combine(path, "TW_Save_Date_0.tw");
                    rijndeal = true;
                }
                DateFile.SaveDate date = ReadSaveDate(file, rijndeal);

                data = new SaveData(date._mainActorName, date._year, date._samsara,
                    date._dayTrun, date._playerSeatName, date._playTime);
                File.WriteAllText(Path.Combine(path, "date.json"),
                    JsonConvert.SerializeObject(data));
            }
            return data;
        }

        /// <summary>
        /// 存档为压缩包
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static SaveData ParseZip(string path)
        {
            SaveData data = null;
            using (var zip = new ZipFile(path))
            {
                if (zip.ContainsEntry("date.json"))
                {
                    using (var stream = new MemoryStream())
                    {
                        zip.SelectEntries("date.json").First().Extract(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                        using (var reader = new StreamReader(stream))
                        {
                            var serializer = JsonSerializer.Create();
                            data = serializer.Deserialize(reader,
                                typeof(SaveData)) as SaveData;
                        }
                    }
                }
                else if (!zip.ContainsEntry("TW_Save_Date_0.twV0")
                      && !zip.ContainsEntry("TW_Save_Date_0.tw"))
                {
                    throw new Exception(path); // 错误存档
                }
                else // 不含加速文件
                {
                    var tmp = Path.Combine(
                        Environment.GetEnvironmentVariable("TEMP"),
                        Guid.NewGuid().ToString() + ".tw");

                    bool rijndeal = true;
                    using (var stream = File.OpenWrite(tmp))
                    {
                        if (zip.ContainsEntry("TW_Save_Date_0.twV0"))
                        {
                            zip.SelectEntries("TW_Save_Date_0.twV0").First().Extract(stream);
                            rijndeal = false;
                        }
                        else if (zip.ContainsEntry("TW_Save_Date_0.tw"))
                        {
                            zip.SelectEntries("TW_Save_Date_0.tw").First().Extract(stream);
                            rijndeal = true;
                        }
                    }
                    DateFile.SaveDate date = ReadSaveDate(tmp, rijndeal);

                    File.Delete(tmp);

                    data = new SaveData(date._mainActorName, date._year, date._samsara,
                        date._dayTrun, date._playerSeatName, date._playTime);
                    //  添加加速文件
                    zip.AddEntry("date.json", JsonConvert.SerializeObject(data));
                    zip.Save();
                }
            }
            return data;
        }

        /// <summary>
        /// 获取存档内容
        /// </summary>
        /// <param name="path"></param>
        /// <param name="rijndeal"></param>
        /// <returns></returns>
        private static DateFile.SaveDate ReadSaveDate(string path, bool rijndeal)
        {
            var method = (typeof(SaveDateFile)
                .GetMethod("GetData", BindingFlags.Public | BindingFlags.Instance));
            try
            {
                return method.Invoke(SaveDateFile.instance,
                        new object[] { path, typeof(DateFile.SaveDate), rijndeal })
                    as DateFile.SaveDate;
            }
            catch (AmbiguousMatchException)
            {
                return method.Invoke(SaveDateFile.instance,
                        new object[] { path, typeof(DateFile.SaveDate) })
                    as DateFile.SaveDate;
            }
        }

        /// <summary>
        /// 读取指定压缩存档
        /// </summary>
        /// <param name="file"></param>
        public static IEnumerator<object> Load(string file)
        {
            yield return new WaitForSeconds(0.01f);
            if (file.EndsWith(".zip"))
                Unzip(file);

            DateFile.instance.SetEvent(new int[] { 0, -1, 1001 }, true, true);
            DateFile.instance.Initialize(SaveManager.DateId);
        }

        /// <summary>
        /// 解压存档到游戏存档目录
        /// </summary>
        /// <param name="file"></param>
        public static void Unzip(string file)
        {
            using (var zip = new ZipFile(file))
            {
                Directory.GetFiles(SaveManager.SavePath, "*.tw*", SearchOption.TopDirectoryOnly)
                    .Do(File.Delete);

                zip.ExtractAll(SaveManager.SavePath, ExtractExistingFileAction.OverwriteSilently);
            }
        }

    }

    public static class SaveManager
    {
        public const int AFTER_SAVE_BACKUP = 0,
                         BEFORE_LOADING_BACKUP = 1;

        /// <summary>
        /// 当前系统存档路径
        /// </summary>

        public static string SavePath
        {
            get => (typeof(SaveDateFile)
                        .GetMethod("Dirpath", BindingFlags.NonPublic | BindingFlags.Instance)
                        .Invoke(SaveDateFile.instance, new object[] { -1 }) as string)
                .Replace('/', '\\');
        }

        /// <summary>
        /// SaveBackup的备份路径
        /// </summary>
        public static string BackPath
        {
            get => GetBackupStoragePath(SavePath);
        }

        /// <summary>
        /// 当前存档槽位置
        /// </summary>
        public static int DateId { get => SaveDateFile.instance.dateId; }

        public static void Backup(int backupType)
        {
            switch (backupType)
            {
                case AFTER_SAVE_BACKUP:
                    BackupBeforeSave();
                    return;

                case BEFORE_LOADING_BACKUP:
                    BackupBeforerLoad();
                    return;

                default:
                    throw new ArgumentException("invalid backupType");
            }
        }

        /// <summary>
        /// 执行存档后备份
        /// </summary>
        private static void BackupBeforeSave()
        {
            if (Main.settings.maxBackupsToKeep == 0)
            {
                return;
            }
            Main.Logger.Log("开始备份存档");

            int backupIndex;

            // 获取所有当前存档的备份
            var backupFiles = Directory.GetFiles(BackPath, $"Date_{DateId}.save.???.zip",
                SearchOption.TopDirectoryOnly);
            Main.Logger.Log("当前存档数:" + backupFiles.Count());

            if (backupFiles.Count() < Main.settings.maxBackupsToKeep)
            {
                // 若数量未超上限，则直接累加计数
                backupIndex = backupFiles.Count();
            }
            else
            {
                // 若数量超过上限，将最早的一个删掉并且平移所有备份
                Array.Sort(backupFiles, StringComparer.InvariantCulture);
                try
                {
                    File.Delete(backupFiles[0]);
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }

                for (int i = 1; i < backupFiles.Count(); i++)
                {
                    try
                    {
                        File.Move(backupFiles[i], backupFiles[i - 1]);
                    }
                    catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                }

                backupIndex = Main.settings.maxBackupsToKeep - 1;
            }

            // 保存备份
            var targetFile = Path.Combine(BackPath, $"Date_{DateId}.save.{backupIndex:D3}.zip");
            BackupFolderToFile(SavePath, targetFile);
        }

        /// <summary>
        /// 执行读档前备份
        /// </summary>
        private static void BackupBeforerLoad()
        {
            var targetFile = Path.Combine(BackPath, $"Date_{DateId}.load.zip");
            BackupFolderToFile(SavePath, targetFile);
        }

        /// <summary>
        /// 压缩到备份路径
        /// </summary>
        /// <param name="pathToBackup"></param>
        /// <param name="targetFile"></param>
        internal static void BackupFolderToFile(string pathToBackup, string targetFile)
        {
            using (var zip = new ZipFile())
            {
                zip.AddFiles(GetFilesToBackup(pathToBackup), "/");
                zip.Save(targetFile);
            }
        }

        /// <summary>
        /// 获取备份文件列表
        /// </summary>
        /// <param name="pathToBackup"></param>
        /// <returns></returns>
        internal static List<string> GetFilesToBackup(string pathToBackup)
        {
            var files = new List<string>();
            if (File.Exists(Path.Combine(pathToBackup, "date.json")))
            {
                files.Add(Path.Combine(pathToBackup, "date.json"));
            }
            files.AddRange(Directory.GetFiles(pathToBackup, "TW_Save_Date_?.tw*"));
            return files;
        }

        /// <summary>
        /// 返回存档备份存储路径
        /// </summary>
        /// <param name="pathToBackup"></param>
        /// <returns></returns>
        internal static string GetBackupStoragePath(string pathToBackup)
        {
            string path = Path.Combine(Directory.GetParent(pathToBackup).FullName, "SaveBackup");

            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }

            return path;
        }
    }

    [HarmonyPatch(typeof(WindowManage), "WindowSwitch")]
    public static class WindowManage_WindowSwitch_Patch
    {
        [HarmonyBefore(new string[] { "CharacterFloatInfo" })]
        public static void Postfix(bool on, GameObject tips,
            ref Text ___informationMassage, ref Text ___informationName,
            ref int ___tipsW, ref bool ___anTips)
        {
            if (tips == null) return;
            if (!Main.Enabled) return;
            if (tips.name == "SaveButton")
            {
                ___informationName.text = "立即储存";
                ___informationMassage.text = "立即储存当前进度\n";
                ___tipsW = 230;
                ___anTips = true;
            }
            else if (tips.name == "LoadButton")
            {
                ___informationName.text = "载入";
                ___informationMassage.text = "显示存档列表，选择存档读取\n";
                ___tipsW = 260;
                ___anTips = true;
            }
            else if (tips.name == "LoadButton2")
            {
                ___informationName.text = "快速载入";
                ___informationMassage.text = "放弃当前进度, 重新读档\n";
                ___tipsW = 260;
                ___anTips = true;
            }
        }
    }

    [HarmonyPatch(typeof(SaveDateFile), "LateUpdate")]
    public class SaveDateFile_LateUpdate_Patch
    {
        static void Prefix(SaveDateFile __instance)
        {
            if (!Main.Enabled || UIDate.instance == null) return;

            if (__instance.saveSaveDate)
            {
                if (Main.forceSave)
                {
                    Main.forceSave = false;
                    UIDate.instance.trunSaveText.text = "手动存档";
                }
                else if (Main.settings.blockAutoSave)
                {
                    UIDate.instance.trunSaveText.text = "由于您的MOD设置，游戏未保存";
                    __instance.saveSaveDate = false;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 存档后备份当前存档
    /// </summary>
    [HarmonyPatch(typeof(SaveDateFile), "EnsureFiles")]
    public class SaveDateFile_EnsureFiles_Patch
    {
        static void Postfix()
        {
            try
            {
                SaveManager.Backup(SaveManager.AFTER_SAVE_BACKUP);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }

    /// <summary>
    /// 加速文件解析，添加额外文件date.json
    /// </summary>
    [HarmonyPatch(typeof(SaveDateFile), "SaveSaveDate")]
    public class SaveDateFile_SaveSaveDate_Patch
    {
        static void Prefix(SaveDateFile __instance)
        {
            if (!Main.Enabled) return;

            var df = DateFile.instance;
            var savedate = new SaveData(df.GetActorName(), df.year, df.samsara, df.dayTrun,
                df.playerSeatName, DateTime.Now);

            var dirpath = typeof(SaveDateFile)
                .GetMethod("Dirpath", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(__instance, new object[] { -1 }) as string;
            var fpath = Path.Combine(dirpath, "date.json");

            File.WriteAllText(fpath, JsonConvert.SerializeObject(savedate));
        }
    }

#if false
    /// <summary>
    ///  成功读档后自动备份游戏存档
    /// </summary>
    [HarmonyPatch(typeof(Loading), "LoadingScene")]
    public static class Loading_LoadingScene_Patch
    {

        private static void Prefix(bool newGame, int teachingId, int loadingDateId)
        {
            if (!Main.Enabled) { return; }

            if ((teachingId == -1 && !newGame && loadingDateId != 0))
                SaveManager.Backup(SaveManager.BEFORE_LOADING_BACKUP);
        }
    }
#endif
}