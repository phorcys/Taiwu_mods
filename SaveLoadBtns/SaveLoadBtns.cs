using Harmony12;
using Newtonsoft.Json;
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

namespace SaveLoadBtns
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry) { Save(this, modEntry); }
        public bool blockAutoSave = false;
        public int maxSave = 8;
    }
    public static class Main
    {
        public static bool enabled;
        public static bool forceSave = false;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                Assembly.LoadFrom(Path.Combine(modEntry.Path, "DotNetZip.dll"));
            }
            catch (System.Exception) { }
            Logger = modEntry.Logger;
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            return true;
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("阻止游戏自动储存", GUILayout.Width(200));
            settings.blockAutoSave = GUILayout.SelectionGrid(settings.blockAutoSave ? 1 : 0, new string[] { "关闭", "启用" }, 2, GUILayout.Width(150)) == 1;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("存档列表的最大存档数(0表示不受限制)", GUILayout.Width(300));
            var num = -1;
            if (int.TryParse(GUILayout.TextField(Main.settings.maxSave.ToString(), GUILayout.Width(60)), out num))
            {
                if (num >= 0)
                    Main.settings.maxSave = num;
            }
            GUILayout.EndHorizontal();
        }
        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }
    }

    [HarmonyPatch(typeof(WorldMapSystem), "Start")]
    public static class WorldMapSystem_Start_Patch
    {
        public static void Postfix()
        {
            if (Main.enabled)
            {
                UI.Load();

                Transform parent = GameObject.Find("ResourceBack").transform;

                GameObject loadBtn = Object.Instantiate(GameObject.Find("EncyclopediaButton,609"), new Vector3(1620f, -30f, 0), Quaternion.identity);
                loadBtn.name = "LoadButton";
                loadBtn.tag = "SystemIcon";
                loadBtn.transform.SetParent(parent, false);
                loadBtn.transform.localPosition = new Vector3(1620f, -30f, 0);
                Selectable loadButton = loadBtn.GetComponent<Selectable>();
                ((Image)loadButton.targetGraphic).sprite = Resources.Load<Sprite>("Graphics/Buttons/StartGameButton_NoColor");
                loadBtn.AddComponent<MyPointerClick>();

                GameObject saveBtn = Object.Instantiate(GameObject.Find("EncyclopediaButton,609"), new Vector3(1570f, -30f, 0), Quaternion.identity);
                saveBtn.name = "SaveButton";
                saveBtn.tag = "SystemIcon";
                saveBtn.transform.SetParent(parent, false);
                saveBtn.transform.localPosition = new Vector3(1570f, -30f, 0);
                Selectable saveButton = saveBtn.GetComponent<Selectable>();
                ((Image)saveButton.targetGraphic).sprite = Resources.Load<Sprite>("Graphics/Buttons/StartGameButton");
                saveBtn.AddComponent<MyPointerClick>();
            }
        }
    }

    public class MyPointerClick : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Main.enabled) return;
            if (gameObject.name == "LoadButton")
            {
                Fuck();
            }
            else if (gameObject.name == "SaveButton")
            {
                Main.forceSave = true;
                SaveDateFile.instance.SaveGameDate();
            }
        }

        public static void Fuck()
        {
            LoadSave.dateId = SaveDateFile.instance.dateId;

            LoadSave.dirPath = typeof(SaveDateFile)
                .GetMethod("Dirpath", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(SaveDateFile.instance, new object[] { LoadSave.dateId }) as string;

            LoadSave.backPath = (typeof(SaveBackup.SaveManager)
                .GetMethod("GetBackupStoragePath", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, new object[] { LoadSave.dirPath }) as string).Replace("/", "\\");

            var files = Directory.GetFiles(LoadSave.backPath,
                $"Date_{LoadSave.dateId}.save.???.zip",
                SearchOption.TopDirectoryOnly);

            System.Func<string, int> GetSaveIndex = (fname) =>
            {
                var match = new Regex(@"Date_\d\.save\.(\d+)\.zip").Match(fname);
                return int.Parse(match.Groups[1].Value);
            };
            System.Array.Sort(files,
                (str1, str2) => GetSaveIndex(str1) > GetSaveIndex(str2) ? 1 : 0);

            LoadSave.savedFiles = new List<string>();
            var fpath = Path.Combine(LoadSave.backPath, $"Date_{LoadSave.dateId}.load.zip");
            var load = 0;
            if (File.Exists(fpath))
            {
                LoadSave.savedFiles.Add(fpath);
                load = 1;
            }
            if (Main.settings.maxSave > 0)
            {
                LoadSave.savedFiles.AddRange(files.Take(Main.settings.maxSave - load));
            }
            else
            {
                LoadSave.savedFiles.AddRange(files);
            }

            LoadSave.ParseFiles();

            UI.Instance.ShowSave();
        }
    }

    public class SaveDate
    {
        public string name;
        public int year;
        public int samsara;
        public int trun;
        public string position;
        public string playtime;

        public SaveDate() : this(null, -1, -1, -1, null, null)
        { }
        public SaveDate(string name, int year, int samsara, int turn, string position, System.DateTime playtime) :
            this(name, year, samsara, turn, position, playtime.ToString("yyyy - MM - dd   [ HH : mm ]"))
        { }
        public SaveDate(string name, int year, int samsara, int turn, string position,
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

    class StringTimeCompare : IComparer<string>
    {
        public static readonly string format = "yyyy - MM - dd   [ HH : mm ]";
        private Dictionary<string, SaveDate> dict;
        public StringTimeCompare(Dictionary<string, SaveDate> dict)
        {
            this.dict = dict;
        }
        public int Compare(string key1, string key2)
        {
            var time1 = System.DateTime.ParseExact(dict[key1].playtime, format, null);
            var time2 = System.DateTime.ParseExact(dict[key2].playtime, format, null);
            return -time1.CompareTo(time2);
        }
    }

    public static class LoadSave
    {
        public static string dirPath;
        public static string backPath;
        public static int dateId;
        /// <summary>
        /// 需要解析的压缩存档路径
        /// </summary>
        public static List<string> savedFiles;
        public static SortedDictionary<string, SaveDate> savedInfos;
        /// <summary>
        /// 解析压缩存档
        /// </summary>
        public static void ParseFiles()
        {
            var dict = new Dictionary<string, SaveDate>();
            foreach (var file in savedFiles)
            {
                try
                {
                    Main.Logger.Log(file);
                    dict.Add(file, Parse(file));
                }
                catch (System.Exception e)
                {
                    Debug.Log("[ERROR]" + e.Message);
                }
            }
            var comparer = new StringTimeCompare(dict);
            savedInfos = new SortedDictionary<string, SaveDate>(dict, comparer);
        }
        /// <summary>
        /// 解析指定压缩存档
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static SaveDate Parse(string path)
        {
            SaveDate ans = null;
            using (var zip = new Ionic.Zip.ZipFile(path))
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
                            ans = serializer.Deserialize(reader,
                                typeof(SaveDate)) as SaveDate;
                        }
                    }
                }
                else if (!zip.ContainsEntry("TW_Save_Date_0.twV0")
                      && !zip.ContainsEntry("TW_Save_Date_0.tw"))
                {
                    throw new System.Exception(path); // 错误存档
                }
                else // 不含加速文件
                {
                    var tmp = Path.Combine(
                        System.Environment.GetEnvironmentVariable("TEMP"),
                        "SaveDate.tw");

                    if (File.Exists(tmp))
                        File.Delete(tmp);

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
                    var date = SaveDateFile.instance.GetData(tmp,
                        typeof(DateFile.SaveDate), rijndeal) as DateFile.SaveDate;
                    ans = new SaveDate(date._mainActorName, date._year, date._samsara,
                        date._dayTrun, date._playerSeatName, date._playTime);
                    //  添加加速文件
                    zip.AddEntry("date.json", JsonConvert.SerializeObject(ans));
                    zip.Save();
                }
            }
            return ans;
        }
        /// <summary>
        /// 读取指定压缩存档
        /// </summary>
        /// <param name="file"></param>
        public static IEnumerator<object> Load(string file)
        {
            yield return new WaitForSeconds(0.01f);
            Unzip(file);

            DateFile.instance.SetEvent(new int[] { 0, -1, 1001 }, true, true);
            DateFile.instance.Initialize(dateId);
        }
        /// <summary>
        /// 解压存档到游戏存档目录
        /// </summary>
        /// <param name="file"></param>
        public static void Unzip(string file)
        {
            using (var zip = new Ionic.Zip.ZipFile(file))
            {
                Directory.GetFiles(dirPath, "*.tw*", SearchOption.TopDirectoryOnly).Do(File.Delete);
                zip.ExtractAll(dirPath, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
            }
        }

        /// <summary>
        /// 用于调试
        /// </summary>
        public static void Log()
        {
            Debug.Log("dateId: " + dateId);
            Debug.Log("dirpath: " + dirPath);
            Debug.Log("backpath: " + backPath);

            Debug.Log("savedFiles: ");
            foreach (var file in savedFiles)
            {
                Debug.Log("\t" + file);
            }
            Debug.Log("savedInfos: ");
            foreach (var pair in savedInfos)
            {
                Debug.Log("\t" + pair.Key + ": " + JsonConvert.SerializeObject(pair.Value));
            }
        }
    }

    [HarmonyPatch(typeof(WindowManage), "WindowSwitch")]
    public static class WindowManage_WindowSwitch_Patch
    {
        [HarmonyBefore(new string[] { "CharacterFloatInfo" })]
        public static void Postfix(bool on, GameObject tips, ref Text ___informationMassage, ref Text ___informationName, ref int ___tipsW, ref bool ___anTips)
        {
            if (tips == null) return;
            if (!Main.enabled) return;
            if (tips.name == "SaveButton")
            {
                ___informationName.text = "储存";
                ___informationMassage.text = "立即储存当前进度\n";
                ___tipsW = 230;
                ___anTips = true;
            }
            else if (tips.name == "LoadButton")
            {
                ___informationName.text = "载入";
                ___informationMassage.text = "放弃现行进度, 由上一次记录重新开始\n";
                ___tipsW = 260;
                ___anTips = true;
            }
        }
    }

    [HarmonyPatch(typeof(SaveDateFile), "LateUpdate")]
    public class SaveDateFile_LateUpdate_Patch
    {
        [HarmonyBefore(new string[] { "SaveBackup" })]
        static void Prefix(SaveDateFile __instance)
        {
            if (!Main.enabled || !Main.settings.blockAutoSave || UIDate.instance == null) return;

            #region 加速文件解析，添加额外文件date.json
            var df = DateFile.instance;
            var savedate = new SaveDate(df.GetActorName(), df.year, df.samsara, df.dayTrun,
                df.playerSeatName, System.DateTime.Now);

            var dirpath = typeof(SaveDateFile)
                .GetMethod("Dirpath", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(__instance, new object[] { -1 }) as string;
            var fpath = Path.Combine(dirpath, "date.json");

            using (var writer = new StreamWriter(fpath, false, Encoding.UTF8))
            {
                var serializer = JsonSerializer.Create();
                serializer.Serialize(writer, savedate);
            }
            #endregion

            SaveDateFile.instance.saveSaveDate = Main.forceSave;
            UIDate.instance.trunSaveText.text = "";
            Main.forceSave = false;
        }
    }
}
