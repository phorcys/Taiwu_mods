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


namespace Sth4nothing.SLManager
{
    [HarmonyPatch(typeof(WorldMapSystem), "Start")]
    public static class WorldMapSystem_Start_Patch
    {
        public static void Postfix()
        {
            if (Main.Enabled)
            {
                UI.Load();

                Transform parent = GameObject.Find("ResourceBack").transform;

                GameObject loadBtn = Object.Instantiate(GameObject.Find("EncyclopediaButton,609"), new Vector3(1620f, -30f, 0), Quaternion.identity);
                loadBtn.name = "LoadButton2";
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

                GameObject loadBtn2 = GameObject.Find("EncyclopediaButton,609");
                loadBtn2.name = "LoadButton";
                Selectable loadButton2 = loadBtn2.GetComponent<Selectable>();
                ((Image)loadButton2.targetGraphic).sprite = Resources.Load<Sprite>("Graphics/Buttons/StartGameButton_NoColor");
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
                    DateFile.instance.massageDate[701][2].Replace("返回主菜单", "载入旧进度")
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
            LoadFile.dateId = SaveDateFile.instance.dateId;

            LoadFile.dirPath = typeof(SaveDateFile)
                .GetMethod("Dirpath", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(SaveDateFile.instance, new object[] { LoadFile.dateId }) as string;

            LoadFile.backPath = (typeof(SaveBackup.SaveManager)
                .GetMethod("GetBackupStoragePath", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, new object[] { LoadFile.dirPath }) as string).Replace("/", "\\");

            var files = Directory.GetFiles(LoadFile.backPath,
                $"Date_{LoadFile.dateId}.save.???.zip",
                SearchOption.TopDirectoryOnly);

            System.Func<string, int> GetSaveIndex = (fname) =>
            {
                var match = new Regex(@"Date_\d\.save\.(\d+)\.zip").Match(fname);
                return int.Parse(match.Groups[1].Value);
            };
            System.Array.Sort(files,
                (str1, str2) => GetSaveIndex(str1) > GetSaveIndex(str2) ? 1 : 0);

            LoadFile.savedFiles = new List<string>();
            if (File.Exists(Path.Combine(LoadFile.dirPath, "TW_Save_Date_0.twV0"))
                || File.Exists(Path.Combine(LoadFile.dirPath, "TW_Save_Date_0.tw")))
                LoadFile.savedFiles.Add(LoadFile.dirPath);
            if (Main.settings.maxSave > 0)
            {
                LoadFile.savedFiles.AddRange(files.Take(Main.settings.maxSave - 1));
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
        public SaveData(string name, int year, int samsara, int turn, string position, System.DateTime playtime) :
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
        /// 当前系统存档路径
        /// </summary>
        public static string dirPath;
        /// <summary>
        /// SaveBackup的备份路径
        /// </summary>
        public static string backPath;
        /// <summary>
        /// 当前存档槽位置
        /// </summary>
        public static int dateId;
        /// <summary>
        /// 需要解析的压缩存档路径
        /// </summary>
        public static List<string> savedFiles;
        /// <summary>
        /// 解析了的存档
        /// </summary>
        public static Dictionary<string, SaveData> savedInfos;

        public static readonly string format = "yyyy - MM - dd   [ HH : mm ]";

        /// <summary>
        /// 解析压缩存档列表
        /// </summary>
        public static void ParseFiles()
        {
            savedInfos = new Dictionary<string, SaveData>();
            foreach (var file in savedFiles)
            {
                try
                {
                    Main.Logger.Log(file);
                    savedInfos.Add(file, Parse(file));
                }
                catch (System.Exception e)
                {
                    Main.Logger.Log("[ERROR]" + e.Message);
                }
            }
            savedFiles.Sort((f1, f2) =>
            {
                if (!savedInfos.ContainsKey(f1))
                    return -1;
                if (!savedInfos.ContainsKey(f2))
                    return 1;
                var t1 = System.DateTime.ParseExact(savedInfos[f1].playtime, format, null);
                var t2 = System.DateTime.ParseExact(savedInfos[f2].playtime, format, null);
                return -t1.CompareTo(t2);
            });
        }
        /// <summary>
        /// 解析指定压缩存档
        /// </summary>
        /// <param name="path">压缩存档路径</param>
        /// <returns></returns>
        public static SaveData Parse(string path)
        {
            SaveData ans = null;
            if (!path.EndsWith(".zip"))
            {
                if (File.Exists(Path.Combine(path, "date.json")))
                {
                    var content = File.ReadAllText(Path.Combine(path, "date.json"));
                    ans = JsonConvert.DeserializeObject(content, typeof(SaveData)) as SaveData;
                }
                else if (!File.Exists(Path.Combine(path, "TW_Save_Date_0.twV0"))
                          && !File.Exists(Path.Combine(path, "TW_Save_Date_0.tw")))
                {
                    throw new System.Exception(path);
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
                    DateFile.SaveDate date = null;
                    if (MainMenu.instance.gameVersionText.text.EndsWith("[Test]"))
                    {
                        date = typeof(SaveDateFile)
                            .GetMethod("GetData", BindingFlags.Public | BindingFlags.Instance)
                            .Invoke(SaveDateFile.instance,
                                new object[] { file, typeof(DateFile.SaveDate), rijndeal })
                            as DateFile.SaveDate;
                    }
                    else
                    {
                        date = typeof(SaveDateFile)
                            .GetMethod("GetData", BindingFlags.Public | BindingFlags.Instance)
                            .Invoke(SaveDateFile.instance,
                                new object[] { file, typeof(DateFile.SaveDate) })
                            as DateFile.SaveDate;
                    }
                    ans = new SaveData(date._mainActorName, date._year, date._samsara,
                        date._dayTrun, date._playerSeatName, date._playTime);
                    File.WriteAllText(Path.Combine(path, "date.json"),
                        JsonConvert.SerializeObject(ans));
                }
            }
            else
            {
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
                                    typeof(SaveData)) as SaveData;
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
                        DateFile.SaveDate date = null;
                        if (MainMenu.instance.gameVersionText.text.EndsWith("[Test]"))
                        {
                            date = typeof(SaveDateFile)
                                .GetMethod("GetData", BindingFlags.Public | BindingFlags.Instance)
                                .Invoke(SaveDateFile.instance,
                                    new object[] { tmp, typeof(DateFile.SaveDate), rijndeal })
                                as DateFile.SaveDate;
                        }
                        else
                        {
                            date = typeof(SaveDateFile)
                                .GetMethod("GetData", BindingFlags.Public | BindingFlags.Instance)
                                .Invoke(SaveDateFile.instance,
                                    new object[] { tmp, typeof(DateFile.SaveDate) })
                                as DateFile.SaveDate;
                        }
                        ans = new SaveData(date._mainActorName, date._year, date._samsara,
                            date._dayTrun, date._playerSeatName, date._playTime);
                        //  添加加速文件
                        zip.AddEntry("date.json", JsonConvert.SerializeObject(ans));
                        zip.Save();
                    }
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
            if (file.EndsWith(".zip"))
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
            Debug.Log("version: " + MainMenu.instance.gameVersionText.text);
            Debug.Log("dateId: " + dateId);
            Debug.Log("dirpath: " + dirPath);
            Debug.Log("backpath: " + backPath);

            Debug.Log("savedFiles: ");
            if (savedFiles != null)
            {
                foreach (var file in savedFiles)
                {
                    Debug.Log("\t" + file);
                }
            }
            Debug.Log("savedInfos: ");
            if (savedInfos != null)
            {
                foreach (var pair in savedInfos)
                {
                    Debug.Log("\t" + pair.Key + ": " + JsonConvert.SerializeObject(pair.Value));
                }
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
            if (!Main.Enabled) return;
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
                ___informationMassage.text = "显示存档列表，选择存档读取\n";
                ___tipsW = 260;
                ___anTips = true;
            }
            else if (tips.name == "LoadButton2")
            {
                ___informationName.text = "快速载入";
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
    [HarmonyPatch(typeof(SaveDateFile), "SaveSaveDate")]
    public class SaveDateFile_SaveSaveDate_Patch
    {
        static void Prefix(SaveDateFile __instance)
        {
            if (!Main.Enabled) return;

#region 加速文件解析，添加额外文件date.json
            var df = DateFile.instance;
            var savedate = new SaveData(df.GetActorName(), df.year, df.samsara, df.dayTrun,
                df.playerSeatName, System.DateTime.Now);

            var dirpath = typeof(SaveDateFile)
                .GetMethod("Dirpath", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(__instance, new object[] { -1 }) as string;
            var fpath = Path.Combine(dirpath, "date.json");

            File.WriteAllText(fpath, JsonConvert.SerializeObject(savedate));
#endregion
        }
    }
}