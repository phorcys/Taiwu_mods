using Harmony12;
using Ionic.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


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
        private static readonly string[] btns =
        {
            "SystemButton,608",//系统设置
            "SaveActorsButton,723",//铭刻
            "EncyclopediaButton,609",//太吾百晓册
            "ScrollButton,607",//太吾传承
            "ReShowTrunEventButton,822",//时节回顾
            "HomeButton,612",//产业视图
            "ManageManPower,778",//人力调度
            //"LegendBook,724",//解读奇书, 在故事线进行到激活解读奇书之前按钮不会出现，因而不能用GameObject.Find来处理
        };

        private static readonly ValueTuple<string, string>[] extraBtns =
        {
            ValueTuple.Create("LoadButtonList", "Graphics/Buttons/StartGameButton_NoColor"),//列表读档
            ValueTuple.Create("LoadButton", "Graphics/Buttons/StartGameButton_NoColor"),//快速读档
            ValueTuple.Create("SaveButton", "Graphics/Buttons/StartGameButton"),//快速存档
        };

        private const float Size = 40f;
        private const float StartX = 1920f;

        public static void Postfix()
        {
            if (Main.Enabled)
            {
                UI.Load();

                float startX = StartX;
                Vector2 iconSize = new Vector2(Size, Size);
                Transform parent = GameObject.Find("ResourceBack").transform;

                // 调整其他按钮的位置、大小
                foreach (var btn in btns)
                {
                    var button = GameObject.Find(btn);
                    if (button == null)
                    {
                        Debug.LogWarning(btn + " not exist");
                        continue;
                    }
                    startX -= Size;
                    button.GetComponent<RectTransform>().sizeDelta = iconSize;
                    button.transform.localPosition = new Vector3(startX, -30f, 0f);
                }
                // 解读奇书需要特殊对待
                startX -= Size;
                MissionSystem.instance.legendBookBtn.GetComponent<RectTransform>().sizeDelta = iconSize;
                MissionSystem.instance.legendBookBtn.transform.localPosition = new Vector3(startX, -30f, 0f);
                // 添加按钮
                foreach (var btn in extraBtns)
                {
                    startX -= Size;
                    var obj = UnityEngine.Object.Instantiate(
                        GameObject.Find("EncyclopediaButton,609"),
                        new Vector3(startX, -30f, 0),
                        Quaternion.identity);
                    obj.name = btn.Item1;
                    obj.tag = "SystemIcon";
                    obj.transform.SetParent(parent, false);
                    obj.transform.localPosition = new Vector3(startX, -30f, 0);
                    var objBtn = obj.GetComponent<Button>();
                    // 以便日后游戏实装太吾百晓册
                    objBtn.onClick.RemoveAllListeners();
                    ((Image)objBtn.targetGraphic).sprite = Resources.Load<Sprite>(btn.Item2);
                    obj.GetComponent<RectTransform>().sizeDelta = iconSize;
                    obj.AddComponent<MyPointerClick>();
                    UnityEngine.Object.Destroy(obj.GetComponent<PointerClick>());
                }
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
                    // 防止用户在备份存档线程完成前载入之前的存档覆盖存档造成备份出错
                    if (!Main.isBackuping)
                    {
                        LoadFiles();
                    }
                    else
                    {
                        YesOrNoWindow.instance.SetYesOrNoWindow(-1, "读取存档",
                        DateFile.instance.massageDate[701][2].Replace("确定放弃所有未保存的游戏进度，返回主菜单吗？", "请稍待后台备份存档完成")
                            .Replace("返回到游戏的主菜单…\n", ""), false, true);
                    }
                    break;
                case "LoadButton":
                    YesOrNoWindow.instance.SetYesOrNoWindow(4646, "快速载入",
                        DateFile.instance.massageDate[701][2].Replace("返回主菜单", "载入旧存档")
                            .Replace("返回到游戏的主菜单…\n", ""), false, true);
                    break;
                case "SaveButton":
                    // 防止用户在备份存档线程完成前又点击存档造成备份出错
                    if (!Main.isBackuping)
                    {
                        Main.ForceSave = true;
                        SaveDateFile.instance.SaveGameDate();
                    }
                    else
                    {
                        YesOrNoWindow.instance.SetYesOrNoWindow(-1, "存档",
                        DateFile.instance.massageDate[701][2].Replace("确定放弃所有未保存的游戏进度，返回主菜单吗？", "请稍待后台备份存档完成")
                            .Replace("返回到游戏的主菜单…\n", ""), false, true);
                    }
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
                    LoadFile.DoLoad(SaveDateFile.instance.dateId);
                    break;
            }
        }
    }

    /// <summary>
    /// 读档
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "LoadLegendBook")]
    public static class DateFile_LoadLegendBook_Patch
    {
        /// <summary>
        /// 需要清除的实例
        /// </summary>
        // 不同于从主菜单载入，在游戏内载入存档时很多游戏实例依然存在，它们会不停调用SingletonObject.getInstance<T>()。
        // 若T的实例未创建，则getInstance<T>方法在第一次被调用时会创建T的实例。这导致尽管在载入存档前执行
        // SingletonObject.ClearInstances()清除所有实例，载入存档时还会存在冲突的实例。所以在DateFile.LoadLegendBook()
        // 方法前将有可能还存在的冲突实例清除掉以便正常载入存档。
        private static readonly string[] instancesToRemove =
        {
            "JuniorXiangshuSystem",
            "LegendBookSystem",
            "TaichiDiscSystem",
            // "SpecialEffectSystem" //游戏V0.2.3.x：不再由Singleton载入，改由Subsystem载入
        };

        static private Dictionary<string, object> m_SingletonMap;
        static private List<Type> dontClearList;

        /// <summary>
        /// 清除可能会冲突的实例(尽管禁用UIDate后再载入存档一般可以顺利进行，但游戏特殊情况仍会卡读档，在游戏开发稳定之前双保险)
        /// </summary>
        /// <returns></returns>
        private static bool Prefix(DateFile.LegendBook loadDate)
        {
            if (Main.onLoad)
            {
                Main.onLoad = false;
                m_SingletonMap = m_SingletonMap ?? ReflectionMethod.GetValue<SingletonObject, Dictionary<string, object>>(null, "m_SingletonMap");
                dontClearList = dontClearList ?? ReflectionMethod.GetValue<SingletonObject, List<Type>>(null, "dontClearList");
#if DEBUG
                var m_Container = ReflectionMethod.GetValue<SingletonObject, GameObject>(null, "m_Container");
                Main.Logger.Log($"DateFile_Loadloadlegend: Is m_Container null? {m_Container == null}");
                foreach (string key in m_SingletonMap.Keys)
                {
                    Main.Logger.Log($"DateFile_Loadloadlegend m_SingletonMap keys {key}");
                }
#endif
                // 清除需要清除的实例(仿自 SingletonObject.ClearInstances)
                for (int i = 0; i < instancesToRemove.Length; i++)
                {
                    if (m_SingletonMap.TryGetValue(instancesToRemove[i], out object item) && !dontClearList.Contains(item))
                    {
                        (item as IDisposable)?.Dispose();
                        if (item.GetType().IsSubclassOf(typeof(Component)))
                        {
                            UnityEngine.Object.Destroy(item as Component);
                        }
                        m_SingletonMap.Remove(instancesToRemove[i]);
                    }
                }
                // 清除Subsystem中可能存在冲突的实例
                SubSystems.OnUnloadGameData();
            }
            return true;
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
            return (DateFile.SaveDate)SaveDateFile.instance.GetData(path, typeof(DateFile.SaveDate), rijndeal);
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
            DoLoad(SaveManager.DateId);
        }

        /// <summary>
        /// 执行存档读取操作
        /// </summary>
        public static void DoLoad(int dataId)
        {
            Main.onLoad = true;
            // 防止UI活动生成新的SingletonObject实例
            UIDate.instance.gameObject.SetActive(false);
            // 来自DateFile.BackToStartMenu()方法，载入存档前清空，防止载入存档时载入奇书数据时卡档
            SingletonObject.ClearInstances();
            SubSystems.OnUnloadGameData();
            MainMenu.instance.SetLoadIndex(dataId);
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
        public static SaveDateBackuper backup { get => SaveDateBackuper.GetInstance(); }
        public static int autoBackupRemain
        {
            get => ReflectionMethod.GetValue<SaveDateBackuper, int>(backup, "autoBackupRemain");
            set => ReflectionMethod.SetValue<SaveDateBackuper>(backup, "autoBackupRemain", value);
        }
        public const int AfterSaveBackup = 0,
            BeforeLoadingBackup = 1;

        /// <summary>
        /// 将反射方法与委托挂钩
        /// </summary>
        public static class DelegateCenter
        {
            /// <summary>SaveDateBackuper._extension 获取备份文件扩展名</summary>
            static public string Get_extension { get; private set; }
            /// <summary>SaveDateBackuper.GetDataPathForId(int)</summary>
            static public Func<int, string> GetDataPathForId { get; private set; }
            /// <summary>SaveDateBackuper.BackupItemToBytes()</summary>
            static public Func<byte[]> BackupItemToBytes { get; private set; }
            /// <summary>SaveDateBackuper.CheckBackupCount(int)</summary>
            static public Action<int> CheckBackupCount { get; private set; }
            /// <summary>SaveDateFile.Dirpath(int)</summary>
            public static Func<int, string> Dirpath { get; private set; }

            /// <summary>
            /// 初始化委托
            /// </summary>
            /// <param name="instance"></param>
            static private void Init(SaveDateBackuper instance)
            {
                try
                {
                    Get_extension = ReflectionMethod.GetValue<SaveDateBackuper, string>(instance, "_extension");
                    GetDataPathForId = (Func<int, string>)Delegate.CreateDelegate(
                        typeof(Func<int, string>),
                        instance,
                        ReflectionMethod.GetMethod<SaveDateBackuper>("GetDataPathForId"),
                        true);
                    BackupItemToBytes = (Func<byte[]>)Delegate.CreateDelegate(
                        typeof(Func<byte[]>),
                        instance,
                        ReflectionMethod.GetMethod<SaveDateBackuper>("BackupItemToBytes"),
                        true);
                    CheckBackupCount = (Action<int>)Delegate.CreateDelegate(
                        typeof(Action<int>),
                        instance,
                        ReflectionMethod.GetMethod<SaveDateBackuper>("CheckBackupCount"),
                        true);
                    Dirpath = (Func<int, string>)Delegate.CreateDelegate(
                        typeof(Func<int, string>),
                        SaveDateFile.instance,
                        ReflectionMethod.GetMethod<SaveDateFile>("Dirpath", new[] { typeof(int) }),
                        true);
                }
                catch (Exception e)
                {
                    Main.Logger.Log(e.ToString());
                    throw e;
                }
            }
            /// <summary>
            /// 初始化委托
            /// </summary>
            [HarmonyPatch(typeof(SaveDateBackuper), "Init")]
            private static class SaveDateBackuper_Init_Patch
            {
                private static void Postfix(SaveDateBackuper __instance)
                {
                    Init(__instance);
                }
            }
        }
        /// <summary>
        /// 当前系统存档路径
        /// </summary>
        public static string SavePath
        {
            get => DelegateCenter.Dirpath(-1).Replace('/', '\\');
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

        public static string Backup(int backupType)
        {
            if (!backup.CheckAllSaveFileExist(DateId))
            {
                Debug.Log("存档文件不全");
                return null;
            }
            switch (backupType)
            {
                case AfterSaveBackup:
                    return BackupAfterSave();

                case BeforeLoadingBackup:
                    return BackupBeforeLoad();

                default:
                    throw new ArgumentException("invalid backupType");
            }
        }

        /// <summary>
        /// 执行存档后备份
        /// </summary>
        private static string BackupAfterSave()
        {
            if (Main.settings.maxBackupsToKeep == 0)
            {
                return null;
            }

            // 防止备份存档时进行其他操作会改变存档文件造成错误
            Main.isBackuping = true;
#if DEBUG
            Main.Logger.Log("开始备份存档");
#endif
            // 获取所有当前存档的备份
            var backupFiles = Directory.GetFiles(BackPath, $"Date_{DateId}.save.???.zip",
                SearchOption.TopDirectoryOnly);
#if DEBUG
            Main.Logger.Log("当前存档数:" + backupFiles.Count());
#endif
            Array.Sort(backupFiles, StringComparer.InvariantCulture);

            // 若数量超上限，则删掉较早的存档，并留出多余的一个空档位存新档
            int index = 0;
            while (index < backupFiles.Length - Main.settings.maxBackupsToKeep + 1)
            {
                try
                {
                    File.Delete(backupFiles[index]);
                }
                catch (Exception ex)
                {
                    Main.Logger.Log(ex.ToString());
                }
                index++;
            }

            // 将文件按从旧到新重新命名
            int backupIndex = 0;
            while (index < backupFiles.Length)
            {
                try
                {
                    // 当DateId=1, 正则表达式: (?<=Date_1\.save\.)\d{3}(?=\.zip$) 匹配"D:\abcd\efgh\Date_1.save.000.zip"中的"000"
                    var targetPath = Regex.Replace(backupFiles[index], $"(?<=Date_{DateId}\\.save\\.)\\d{{3}}(?=\\.zip$)", $"{backupIndex:D3}");
                    File.Move(backupFiles[index], targetPath);
                }
                catch (Exception ex)
                {
                    Main.Logger.Log(ex.ToString());
                }
                index++;
                backupIndex++;
            }

            // 保存备份
            var targetFile = Path.Combine(BackPath, $"Date_{DateId}.save.{backupIndex:D3}.zip");
#if DEBUG
            Main.Logger.Log("备份路径:" + targetFile);
#endif
            BackupFolderToFile(SavePath, targetFile);
            // 备份存档完成
            Main.isBackuping = false;
            return targetFile;
        }

        /// <summary>
        /// 执行读档前备份
        /// </summary>
        private static string BackupBeforeLoad()
        {
            var targetFile = Path.Combine(BackPath, $"Date_{DateId}.load.zip");
            Task.Run(() => BackupFolderToFile(SavePath, targetFile));
            return targetFile;
        }

        /// <summary>
        /// 压缩到备份路径
        /// </summary>
        /// <param name="pathToBackup"></param>
        /// <param name="targetFile"></param>
        // 因为DoDefaultBackup方法必须在此方法完成后才能执行，为保证一致性改为在同一线程执行
        internal static void BackupFolderToFile(string pathToBackup, string targetFile)
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
        [HarmonyBefore(new[] { "CharacterFloatInfo" })]
        public static void Postfix(GameObject tips,
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
    public static class SaveDateFile_LateUpdate_Patch
    {
        private static bool Prefix(SaveDateFile __instance, ref bool ___saveSaveDateOK1, ref bool ___saveSaveDateOK2, ref bool ___saveSaveDateOK3)
        {
            if (!Main.Enabled || UIDate.instance == null) return true;

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
                    return true;
                }
                // 写入date.json
                WriteSaveSummary();
            }
            if (___saveSaveDateOK1 && ___saveSaveDateOK2 && ___saveSaveDateOK3)
            {
                ___saveSaveDateOK1 = false;
                ___saveSaveDateOK2 = false;
                ___saveSaveDateOK3 = false;

                // Application.dataPath只能在主线程里使用
                var dirInfo = new DirectoryInfo(
                    Path.Combine(Application.dataPath,
                    "SaveFiles" + SaveDateFile.instance.GetSaveFilesSuffix(),
                    "Backup",
                    $"Date_{SaveManager.DateId}"));

                Task.Run(() => EnsureFiles(dirInfo));

                return false;
            }
            return true;
        }
        /// <summary>
        /// 替换原本的SaveDateFile.EnsureFiles
        /// </summary>
        /// <param name="dirInfo"></param>
        private static void EnsureFiles(DirectoryInfo dirInfo)
        {
            string[] fileNames = new string[9]
            {
                SaveDateFile.instance.GameSettingName,
                SaveDateFile.instance.WorldDateName2,
                SaveDateFile.instance.WorldDateName4,
                SaveDateFile.instance.saveDateName,
                SaveDateFile.instance.homeBuildingName,
                SaveDateFile.instance.WorldDateName3,
                SaveDateFile.instance.PlaceResourceName,
                SaveDateFile.instance.actorLifeName,
                SaveDateFile.instance.legendBookName
            };
            string path = SaveManager.SavePath;
            int num;
            for (int i = 0; i < fileNames.Length; i = num)
            {
                string tmpFile = $"{path}{fileNames[i]}{SaveDateFile.instance.saveVersionName}~";
                string dstFile = $"{path}{fileNames[i]}{SaveDateFile.instance.saveVersionName}";
                if (!File.Exists(tmpFile))
                {
                    Debug.Log("存档异常");
                    break;
                }
                if (File.Exists(dstFile))
                {
                    File.Replace(tmpFile, dstFile, null);
                }
                else
                {
                    File.Move(tmpFile, dstFile);
                }
                num = i + 1;
            }
            Debug.Log("完成保存存档操作,开始执行随档备份...");
            string file = SaveManager.Backup(SaveManager.AfterSaveBackup);
            DoDefaultBackup(SaveManager.DateId, file, dirInfo);
        }
        /// <summary>
        /// 处理过月存档备份
        /// </summary>
        /// <param name="dataId">存档ID</param>
        /// <param name="file">SLMOD存档备份路径</param>
        /// <param name="dirInfo">系统存档备份路径</param>
        /// <returns></returns>
        private static bool DoDefaultBackup(int dataId, string file, DirectoryInfo dirInfo)
        {
            if (!SaveManager.backup.IsOn)
            {
                Debug.Log("过时节备份已禁用!");
                return false;
            }
            if (SaveManager.autoBackupRemain > 1)
            {
                SaveManager.autoBackupRemain--;
                Debug.Log($"未到设定的自动备份时间,等待时节剩余:{SaveManager.autoBackupRemain}");
                return false;
            }
            var dataPathForId = SaveManager.DelegateCenter.GetDataPathForId(dataId);
            if (!dataPathForId.CheckDirectory())
            {
                Debug.Log("当前存档位置不存在!");
                return false;
            }
            if (!SaveManager.backup.CheckAllSaveFileExist(dataId))
            {
                Debug.Log("当前存档不完整,不进行备份操作!");
                return false;
            }
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            var array = SaveManager.DelegateCenter.BackupItemToBytes();
            var path = Path.Combine(
                dirInfo.FullName,
                $"{DateTime.Now.ToString("yyyyMMddhhmmss")}{array.Length}.{SaveManager.DelegateCenter.Get_extension}");
#if DEBUG
            Main.Logger.Log($"存档备份路径: {path}");
#endif
            using (var fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                Debug.Log("写入备份描述!");
                fs.Write(array, 0, array.Length);
                if (File.Exists(file))
                {
                    using (var zip = File.OpenRead(file))
                    {
                        var buff = new byte[1024];
                        while (zip.Position < zip.Length)
                        {
                            var len = zip.Read(buff, 0, buff.Length);
                            fs.Write(buff, 0, len);
                        }
                    }
                }
                else
                {
                    using (var zip = new ZipFile())
                    {
                        zip.AddDirectory(dataPathForId);
                        zip.Save(fs);
                    }
                }
                Debug.Log($"Date_{dataId}备份完成!");
            }
            SaveManager.autoBackupRemain = SaveManager.backup.TickInterval;
            SaveManager.DelegateCenter.CheckBackupCount(dataId);
            return true;
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
}
