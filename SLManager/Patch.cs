using Harmony12;
using Ionic.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Sth4nothing.SLManager
{
    [HarmonyPatch(typeof(Encoding), "GetEncoding", new Type[] {typeof(int)})]
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
                //
                float startX = 1520f;
                float size = 40f;
                Vector2 iconSize = new Vector2(size, size);

                //快速存档
                startX += size;
                GameObject saveBtn = UnityEngine.Object.Instantiate(
                    GameObject.Find("EncyclopediaButton,609"),
                    new Vector3(startX, -30f, 0), Quaternion.identity);
                saveBtn.name = "SaveButton";
                saveBtn.tag = "SystemIcon";
                saveBtn.transform.SetParent(parent, false);
                saveBtn.transform.localPosition = new Vector3(startX, -30f, 0);
                Selectable saveButton = saveBtn.GetComponent<Selectable>();
                ((Image) saveButton.targetGraphic).sprite =
                    Resources.Load<Sprite>("Graphics/Buttons/StartGameButton");
                saveBtn.GetComponent<RectTransform>().sizeDelta = iconSize;
                saveBtn.AddComponent<MyPointerClick>();

                // 快速载入
                startX += size;
                GameObject loadBtn = UnityEngine.Object.Instantiate(
                    GameObject.Find("EncyclopediaButton,609"),
                    new Vector3(startX, -30f, 0), Quaternion.identity);
                loadBtn.name = "LoadButton";
                loadBtn.tag = "SystemIcon";
                loadBtn.transform.SetParent(parent, false);
                loadBtn.transform.localPosition = new Vector3(startX, -30f, 0);
                Selectable loadButton = loadBtn.GetComponent<Selectable>();
                ((Image) loadButton.targetGraphic).sprite =
                    Resources.Load<Sprite>("Graphics/Buttons/StartGameButton_NoColor");
                loadBtn.GetComponent<RectTransform>().sizeDelta = iconSize;
                loadBtn.AddComponent<MyPointerClick>();

                // 列表载入
                startX += size;
                GameObject loadBtnForList = UnityEngine.Object.Instantiate(
                    GameObject.Find("EncyclopediaButton,609"),
                    new Vector3(startX, -30f, 0), Quaternion.identity);
                loadBtnForList.name = "LoadButtonList";
                loadBtnForList.tag = "SystemIcon";
                loadBtnForList.transform.SetParent(parent, false);
                loadBtnForList.transform.localPosition = new Vector3(startX, -30f, 0);
                Selectable loadBtnForListSelectable = loadBtnForList.GetComponent<Selectable>();
                ((Image) loadBtnForListSelectable.targetGraphic).sprite =
                    Resources.Load<Sprite>("Graphics/Buttons/StartGameButton_NoColor");
                loadBtnForList.GetComponent<RectTransform>().sizeDelta = iconSize;
                loadBtnForList.AddComponent<MyPointerClick>();

                //产业视图
                //HomeButton,612
                startX += size;
                GameObject homeButton = GameObject.Find("HomeButton,612");
                homeButton.GetComponent<RectTransform>().sizeDelta = iconSize;
                homeButton.transform.localPosition = new Vector3(startX, -30f, 0);
                //时节回顾
                //ReShowTrunEventButton,822
                startX += size;
                GameObject reShowTrunEventButton = GameObject.Find("ReShowTrunEventButton,822");
                reShowTrunEventButton.GetComponent<RectTransform>().sizeDelta = iconSize;
                reShowTrunEventButton.transform.localPosition = new Vector3(startX, -30f, 0);
                //太吾传承
                //ScrollButton,607
                startX += size;
                GameObject scrollButton = GameObject.Find("ScrollButton,607");
                scrollButton.GetComponent<RectTransform>().sizeDelta = iconSize;
                scrollButton.transform.localPosition = new Vector3(startX, -30f, 0);
                //太吾百晓册
                //EncyclopediaButton,609
                startX += size;
                GameObject encyclopediaButton = GameObject.Find("EncyclopediaButton,609");
                encyclopediaButton.GetComponent<RectTransform>().sizeDelta = iconSize;
                encyclopediaButton.transform.localPosition = new Vector3(startX, -30f, 0);
                //铭刻
                //SaveActorsButton,723
                startX += size;
                GameObject saveActorsButton = GameObject.Find("SaveActorsButton,723");
                saveActorsButton.GetComponent<RectTransform>().sizeDelta = iconSize;
                saveActorsButton.transform.localPosition = new Vector3(startX, -30f, 0);
                //系统设置
                //SystemButton,608
                startX += size;
                GameObject systemButton = GameObject.Find("SystemButton,608");
                systemButton.GetComponent<RectTransform>().sizeDelta = iconSize;
                systemButton.transform.localPosition = new Vector3(startX, -30f, 0);
            }
        }
    }

    /// <summary>
    /// 添加的按钮的点击事件
    /// </summary>
    public class MyPointerClick : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Main.Enabled) return;
            switch (gameObject.name)
            {
                case "LoadButtonList":
                    LoadFiles();
                    break;
                case "LoadButton":
                    YesOrNoWindow.instance.SetYesOrNoWindow(4646, "快速载入",
                        DateFile.instance.massageDate[701][2].Replace("返回主菜单", "载入旧存档")
                            .Replace("返回到游戏的主菜单…\n", ""), false, true);
                    break;
                case "SaveButton":
                    Main.ForceSave = true;
                    SaveDateFile.instance.SaveGameDate();
                    break;
            }
        }

        /// <summary>
        /// 加载所有存档
        /// </summary>
        public static void LoadFiles()
        {
            var files = Directory.GetFiles(SaveManager.BackPath,
                $"Date_{SaveManager.DateId}.save.???.zip",
                SearchOption.TopDirectoryOnly);
            Array.Sort(files, StringComparer.InvariantCulture);

            LoadFile.SavedFiles = new List<string>();

            if (File.Exists(Path.Combine(SaveManager.SavePath, "TW_Save_Date_0.twV0"))
                || File.Exists(Path.Combine(SaveManager.SavePath, "TW_Save_Date_0.tw")))
                LoadFile.SavedFiles.Add(SaveManager.SavePath);

            LoadFile.SavedFiles.AddRange(
                Main.settings.maxBackupToLoad > 0
                    ? files.Reverse().Take(Main.settings.maxBackupToLoad)
                    : files);

            LoadFile.ParseFiles();

            UI.Instance.ShowData();
        }
    }

    /// <summary>
    /// 读档
    /// </summary>
    [HarmonyPatch(typeof(OnClick), "Index")]
    public static class OnClick_Index_Patch
    {
        public static void Postfix()
        {
            if (!Main.Enabled) return;
            switch (OnClick.instance.ID)
            {
                case 4646:
                    DateFile.instance.SetEvent(new[] {0, -1, 1001}, true, true);
                    DateFile.instance.Initialize(SaveDateFile.instance.dateId);
                    YesOrNoWindow.instance.CloseYesOrNoWindow();
                    YesOrNoWindow.instance.yesOrNoWindow.sizeDelta = new Vector2(720f, 280f);
                    OnClick.instance.Over = true;
                    break;
            }
        }
    }

    /// <summary>
    /// 存档的摘要信息
    /// </summary>
    public class SaveData
    {
        public string name;
        public int year;
        public int samsara;
        public int trun;
        public string position;
        public string playtime;

        [Obsolete]
        public SaveData()
        {
        }

        public SaveData(string name, int year, int samsara, int turn, string position, DateTime playtime) :
            this(name, year, samsara, turn, position, playtime.ToString("yyyy - MM - dd   [ HH : mm ]"))
        {
        }

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
        public static List<string> SavedFiles;

        /// <summary>
        /// 解析了的存档
        /// </summary>
        public static ConcurrentDictionary<string, SaveData> SavedInfos;

        public const string Format = "yyyy - MM - dd   [ HH : mm ]";

        /// <summary>
        /// 解析压缩存档列表
        /// </summary>
        public static void ParseFiles()
        {
            SavedInfos = new ConcurrentDictionary<string, SaveData>();

            Parallel.ForEach(SavedFiles, ParseThread);

            SavedFiles.Sort((f1, f2) =>
            {
                if (!SavedInfos.ContainsKey(f1))
                    return -1;
                if (!SavedInfos.ContainsKey(f2))
                    return 1;
                var t1 = DateTime.ParseExact(SavedInfos[f1].playtime, Format, null);
                var t2 = DateTime.ParseExact(SavedInfos[f2].playtime, Format, null);
                return -t1.CompareTo(t2);
            });
        }
        /// <summary>
        /// 解析存档
        /// </summary>
        /// <param name="file"></param>
        public static void ParseThread(string path)
        {
            try
            {
                Debug.Log("Parse: " + path);
                var data = Parse(path);
                SavedInfos[path] = data ?? throw new Exception("未能解析存档");
            }
            catch (Exception e)
            {
                Debug.Log("[ERROR]" + e);
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
            SaveData data;
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
                var date = File.Exists(Path.Combine(path, "TW_Save_Date_0.twV0"))
                    ? ReadSaveDate(Path.Combine(path, "TW_Save_Date_0.twV0"), false)
                    : ReadSaveDate(Path.Combine(path, "TW_Save_Date_0.tw"), true);

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
            SaveData data;
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
                        Guid.NewGuid() + ".tw");

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

                    var date = ReadSaveDate(tmp, rijndeal);

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
            if (!File.Exists(path))
            {
                return null;
            }

            return ReflectionMethod.Invoke<SaveDateFile, DateFile.SaveDate>(
                SaveDateFile.instance,
                "GetData",
                path, typeof(DateFile.SaveDate), rijndeal // 参数
            );
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

            DateFile.instance.SetEvent(new int[] {0, -1, 1001}, true, true);
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
        public const int AfterSaveBackup = 0,
            BeforeLoadingBackup = 1;

        /// <summary>
        /// 当前系统存档路径
        /// </summary>

        public static string SavePath
        {
            get => ReflectionMethod.Invoke<SaveDateFile, string>(
                    SaveDateFile.instance,
                    "Dirpath",
                    -1)
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
        public static int DateId => SaveDateFile.instance.dateId;

        public static void Backup(int backupType)
        {
            switch (backupType)
            {
                case AfterSaveBackup:
                    BackupAfterSave();
                    return;

                case BeforeLoadingBackup:
                    BackupBeforeLoad();
                    return;

                default:
                    throw new ArgumentException("invalid backupType");
            }
        }

        /// <summary>
        /// 执行存档后备份
        /// </summary>
        private static void BackupAfterSave()
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

            if (backupFiles.Length < Main.settings.maxBackupsToKeep)
            {
                // 若数量未超上限，则直接累加计数
                backupIndex = backupFiles.Length;
            }
            else
            {
                // 若数量超过上限，将最早的一个删掉并且平移所有备份
                Array.Sort(backupFiles, StringComparer.InvariantCulture);
                try
                {
                    File.Delete(backupFiles[0]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                for (int i = 1; i < backupFiles.Length; i++)
                {
                    try
                    {
                        File.Move(backupFiles[i], backupFiles[i - 1]);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }

                backupIndex = Main.settings.maxBackupsToKeep - 1;
            }

            // 保存备份
            var targetFile = Path.Combine(BackPath, $"Date_{DateId}.save.{backupIndex:D3}.zip");
            Main.Logger.Log("备份路径:" + targetFile);
            var dataPath = Directory.GetParent(SavePath).FullName;
            var path = Path.Combine(dataPath, $"Date_{DateId}.backup");
            Main.Logger.Log("系统自动备份路径:" + path);
            if (File.Exists(path))
            {
                Main.Logger.Log(path + "已存在");
                File.Copy(path, targetFile);
            }
            else
            {
                Main.Logger.Log(path + "不存在");
                BackupFolderToFile(SavePath, targetFile);
            }
        }

        /// <summary>
        /// 执行读档前备份
        /// </summary>
        private static void BackupBeforeLoad()
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
            Task.Run(() =>
            {
                var preDir = Environment.CurrentDirectory;
                Environment.CurrentDirectory = pathToBackup;
                var files = GetFilesToBackup();
                using (var zip = new ZipFile())
                {
                    zip.AddFiles(files, true, "\\");
                    zip.Save(targetFile);
                }

                Environment.CurrentDirectory = preDir;
            });
        }

        /// <summary>
        /// 获取备份文件列表
        /// </summary>
        /// <returns></returns>
        internal static List<string> GetFilesToBackup()
        {
            var files = new List<string>();
            if (File.Exists(Path.Combine(".", "date.json")))
            {
                files.Add(Path.Combine(".", "date.json"));
            }

            files.AddRange(Directory.GetFiles(
                ".",
                "*.tw*",
                SearchOption.AllDirectories));
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

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }
    }

    /// <summary>
    /// 按钮的提示信息
    /// </summary>
    [HarmonyPatch(typeof(WindowManage), "WindowSwitch")]
    public static class WindowManage_WindowSwitch_Patch
    {
        [HarmonyBefore(new[] {"CharacterFloatInfo"})]
        public static void Postfix(bool on, GameObject tips,
            ref Text ___informationMassage, ref Text ___informationName,
            ref int ___tipsW, ref bool ___anTips)
        {
            if (!Main.Enabled || tips == null) return;
            if (tips.name == "SaveButton")
            {
                ___informationName.text = "立即储存";
                ___informationMassage.text = "立即储存当前进度\n";
                ___tipsW = 230;
                ___anTips = true;
            }
            else if (tips.name == "LoadButtonList")
            {
                ___informationName.text = "载入";
                ___informationMassage.text = "显示存档列表，选择存档读取\n";
                ___tipsW = 260;
                ___anTips = true;
            }
            else if (tips.name == "LoadButton")
            {
                ___informationName.text = "快速载入";
                ___informationMassage.text = "放弃当前进度, 重新读档\n";
                ___tipsW = 260;
                ___anTips = true;
            }
        }
    }

    /// <summary>
    /// 判断是否需要存档，同时写入date.json
    /// </summary>
    [HarmonyPatch(typeof(SaveDateFile), "LateUpdate")]
    public class SaveDateFile_LateUpdate_Patch
    {
        private static void Prefix(SaveDateFile __instance)
        {
            if (!Main.Enabled || UIDate.instance == null) return;

            if (__instance.saveSaveDate)
            {
                if (Main.ForceSave)
                {
                    Main.ForceSave = false;
                    UIDate.instance.trunSaveText.text = "手动存档";
                }
                else if (Main.settings.blockAutoSave)
                {
                    UIDate.instance.trunSaveText.text = "由于您的MOD设置，游戏未保存";
                    __instance.saveSaveDate = false;
                    return;
                }

                Main.DoBackup = true;
                // 写入date.json
                WriteSaveSummary();
            }
        }

        /// <summary>
        /// 保存当前摘要到date.json
        /// </summary>
        private static void WriteSaveSummary()
        {
            var df = DateFile.instance;
            var data = new SaveData(
                df.GetActorName(0, false, false),
                df.year,
                df.samsara,
                df.dayTrun,
                df.playerSeatName,
                DateTime.Now);

            File.WriteAllText(
                Path.Combine(SaveManager.SavePath, "date.json"),
                JsonConvert.SerializeObject(data));
        }
    }

    /// <summary>
    /// 不让date.json被清除
    /// </summary>
    [HarmonyPatch(typeof(SaveDateBackuper), "ClearSaveDataDirectory")]
    public class SaveDateBackuper_ClearSaveDataDirectory_Patch
    {
        private static bool Prefix(string ___currentActorSaveDataPath)
        {
            if (!Main.Enabled)
            {
                return true;
            }

            if (!Directory.Exists(___currentActorSaveDataPath))
            {
                Directory.CreateDirectory(___currentActorSaveDataPath);
            }

            string[] files = Directory.GetFiles(___currentActorSaveDataPath);
            foreach (string text in files)
            {
                if (!text.EndsWith(SaveDateFile.instance.saveVersionName) && !text.EndsWith("date.json"))
                {
                    File.Delete(text);
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(SaveDateBackuper), "DoBackup")]
    public class SaveDateBackuper_DoBackup_Patch
    {
        private static void Postfix()
        {
            if (!Main.Enabled || !Main.DoBackup)
                return;

            SaveManager.Backup(SaveManager.AfterSaveBackup);
            Main.DoBackup = false;
        }
    }
}