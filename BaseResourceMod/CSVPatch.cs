#define debug
using Harmony12;
using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace BaseResourceMod
{
    internal static class CSVPatch
    {
        /// <summary>正在加载自定义配置</summary>
        // 目前的作用只用于预防propAllText还未载入完时被读取
        private static int isLoading;

        /// <summary>
        /// 处理CSV格式文本
        /// </summary>
        /// <param name="dataList">目标数据字典</param>
        /// <param name="text">要处理的csv文本</param>
        /// <param name="passDateIndex">需要跳过的数据字段</param>
        /// <param name="path">文本来源路径，仅用于输出Debug信息</param>
        /// <returns>在目标字典中增加的记录数</returns>
        private static int TextProcessor(Dictionary<int, Dictionary<int, string>> dataList, string text, int passDateIndex = -1, string path = "")
        {
            if (dataList == null && text == null)
                return 0;
            // 统计新增多少条信息
            int counter = 0;
            // 转换为Unix风格的换行符
            string ns = text.Replace("\r", "");
            // 添加字体颜色格式，<color=AABBCCFF>文字</color>
            ns = ns.Replace("C_D", "</color>");
            ns = Regex.Replace(ns, @"C_(\d{5})", delegate (Match match)
            {
                string v = match.Groups[0].Value;
                int colorkey = int.Parse(match.Groups[1].Value);
                return Main.textcolor.TryGetValue(colorkey, out var colorTag) ? colorTag : v;
            });
            ns = "index" + ns.Substring(1);

            // 解析csv
            using (var csv = new CsvReader(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(ns))), true))
            {
                int fieldCount = csv.FieldCount;

                string[] headers = csv.GetFieldHeaders();
#if debug
                Main.Logger.Log("processing  header " + string.Join("", headers));
#endif
                headers[0] = "#";
                
                int[] headerint = new int[headers.Length];
                for (int z = 1; z < headers.Length; z++)
                {
                    if (headers[z] != "#" && headers[z] != "" && int.TryParse(headers[z], out var value))
                    {
                        headerint[z] = value;
                    }
                    else
                    {
                        headerint[z] = -1000;
                    }
                }

                while (csv.ReadNextRecord())
                {
                    if (csv[0] == null || csv[0] == "")
                    {
                        continue;
                    }
                    if (!int.TryParse(csv[0], out var dkey))
                    {
                        Main.Logger.Log($"Illegal Primary Key, FileName: {path} Key: {csv[0]}");
                        continue;
                    }
                    var dictionary = new Dictionary<int, string>();
                    for (int j = 0; j < fieldCount; j++)
                    {
                        if (headers[j] != "#" && headers[j] != "" && headerint[j] != passDateIndex)
                        {
                            dictionary[headerint[j]] = Regex.Unescape(csv[j]);
                        }
                    }

                    
                    dataList[dkey] = dictionary;
                    counter++;
                }
            }
            return counter;
        }

        /// <summary>
        ///  基础数据CSV读取Hook
        /// </summary>
        [HarmonyPatch(typeof(GetSprites), "GetDate")]
        private static class GetSprites_GetDate_Patch
        {
            private static void NewGetData(string dateName, Dictionary<int, Dictionary<int, string>> dateList, int passDateIndex)
            {
                dateList.Clear();
                // 载入路径
                string path = $"{"."}/Data/{dateName}.txt";

                //打开文件
                string text;
                // 开启MOD增量功能时屏蔽系统自带的加载功能
                if (!Main.settings.load_custom_config && File.Exists(path))
                {
                    if (Main.settings.detailed_custom_config_log)
                        Main.Logger.Log("processing " + path);
                    using (var fs = File.OpenText(path))
                    {
                        text = fs.ReadToEnd();
                    }
                }
                else
                {
                    // 此处更改路径仅用于在出现bug时输出问题路径
                    path = dateName;
                    if (Main.settings.detailed_custom_config_log)
                        Main.Logger.Log("processing " + dateName);
                    text = GetSprites.instance.baseGameDate[dateName];
                }

                try
                {
                    TextProcessor(dateList, text, passDateIndex, path);
                    // 保存设置
                    if (Main.settings.save_config)
                    {
                        string savepath = $"{Main.backupdir}/{dateName}.txt";
                        var arr = Newtonsoft.Json.JsonConvert.SerializeObject(dateList);
                        File.WriteAllText(savepath, text);
                        File.WriteAllText(savepath + ".json", arr);
                    }
                }
                catch (Exception e)
                {
                    Main.Logger.Log(e.Message);
                    Main.Logger.Log(e.StackTrace);
                }

            }

            private static bool Prefix(string dateName, Dictionary<int, Dictionary<int, string>> dateList, int passDateIndex)
            {
                if (!Main.enabled)
                    return true;
                NewGetData(dateName, dateList, passDateIndex);
                return false;
            }
        }

        /// <summary>
        /// 读取基础数据完毕后，读取自定义数据
        /// </summary>
        [HarmonyPatch(typeof(ArchiveSystem.LoadGame), "LoadReadonlyData")]
        private static class ArchiveSystem_LoadGame_LoadReadonlyData_Patch
        {
            /// <summary>
            /// 将自定义数据文件信息载入字典中
            /// </summary>
            /// <param name="path">自定义数据文件路径</param>
            /// <param name="dict">目标字典</param>
            /// <param name="passDateIndex">跳过的字段</param>
            private static void DoInjectDataToDict(string path, Dictionary<int, Dictionary<int, string>> dict, int passDateIndex = -1)
            {
                if (Main.settings.detailed_custom_config_log)
                    Main.Logger.Log("Injecting processing " + path);
                //打开文件
                if (!File.Exists(path))
                    return;

                string text;
                using (var fs = File.OpenText(path))
                {
                    text = fs.ReadToEnd();
                }

                int counter = 0;
                try
                {
                    counter = TextProcessor(dict, text, passDateIndex, path);
                }
                catch (Exception e)
                {
                    Main.Logger.Log(e.Message);
                    Main.Logger.Log(e.StackTrace);
                }
                // 将所有通过基本资源框架成功加载的txt文件计数
                Interlocked.Increment(ref Main.csvFilesCounter[1]);
                if (Main.settings.detailed_custom_config_log)
                    Main.Logger.Log($"Ijected  {counter} line from  {path} ");
            }

            /// <summary>
            /// 处理自定义数据文件
            /// </summary>
            /// <param name="path"></param>
            private static void ProcessDir(string path)
            {
                var allfiles = new Dictionary<string, SortedDictionary<int, string>>();
                // 获取该目录及子目录下所有 txt 文件
                var fnames = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
                // 将所有即将通过基本资源框架加载的txt文件计数（包括其他的mod的加载请求）
                Interlocked.Add(ref Main.csvFilesCounter[0], fnames.Length);
                // 遍历该目录及子目录下所有 txt 文件
                foreach (string fname in fnames)
                {
                    string filename = Path.GetFileName(fname);
                    string ufname = fname.Replace("\\", "/");
                    if (Main.settings.detailed_custom_config_log)
                        Main.Logger.Log($"Found {ufname} in subdir {path}");
                    try
                    {
                        if (File.Exists(ufname))
                        {
                            // 获取数据类型，如ItemDate.txt.123.txt中的ItemDate
                            string ftype = filename.Substring(0, filename.IndexOf('.')).ToLower();
                            int num = 1000;
                            // 获得文件名中的序号, 如ItemDate.txt.123.txt中的123
                            var m = Regex.Match(filename, @"(?<=\.txt\.)\d{3}(?=\.txt)", RegexOptions.IgnoreCase);
                            if (m != null && m.Length > 0 && int.TryParse(m.Value, out var value) && value > 0)
                            {
                                num = value;
                                Main.Logger.Log($"num: {num}");
                            }
                            // 是否有该类型数据被添加
                            if (!allfiles.TryGetValue(ftype, out var file))
                            {
                                file = new SortedDictionary<int, string>();
                                allfiles[ftype] = file;
                            }
                            // 已经存在的文件不再重复加,在日志中记录
                            if (file.Count > 0 && file.ContainsKey(num))
                            {
                                Main.Logger.Log($"{ufname}: serial number {num} of data type {ftype} already exists");
                                continue;
                            }
                            file.Add(num, ufname);
                            if (Main.settings.detailed_custom_config_log)
                                Main.Logger.Log($"add {ufname} in subdir {path} to  parsing queue");
                        }
                        else
                        {
                            Main.Logger.Log($"file not exsit {ufname} in subdir {path}");
                        }
                    }
                    catch (Exception e)
                    {
                        Main.Logger.Log(e.Message);
                        Main.Logger.Log(e.StackTrace);
                    }
                }

                // 处理添加好的文件
                foreach (var kv in allfiles)
                {
                    if (Main.settings.detailed_custom_config_log)
                        Main.Logger.Log("begin cate  : " + kv.Key);
                    try
                    {
                        // 获取数据文件对应的游戏数据字典
                        var dict = Main.GetCSVDictRef(kv.Key);
                        foreach (var vv in kv.Value)
                        {
                            if (Main.settings.detailed_custom_config_log)
                                Main.Logger.Log("begin cate  file : " + vv.Value);
                            if (dict != null)
                            {
                                DoInjectDataToDict(vv.Value, dict);
                            }
                            else if (kv.Key.Equals("propalltext"))
                            {
                                // 单独处理"propAllText"
                                StringCenter_GetValue_Patch.DoInjectStringData(vv.Value);
                            }
                            else
                            {
                                Main.Logger.Log("Error  cate " + kv.Key + "  file : " + vv.Value);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Main.Logger.Log(e.Message);
                        Main.Logger.Log(e.StackTrace);
                    }
                }
            }

            /// <summary>
            /// 添加自定义数据
            /// </summary>
            private static void InjectData()
            {
                try
                {
                    Interlocked.Exchange(ref isLoading, 1);
                    if (Directory.Exists(Main.resdir))
                    {
                        //遍历 Date目录子目录
                        foreach (string path in Directory.GetDirectories(Main.resdir))
                        {
                            Main.Logger.Log("Found subdir : " + path);
                            if (Directory.Exists(path))
                            {
                                ProcessDir(path);
                            }
                        }
                    }

                    // 处理其他MOD的请求
                    foreach (var kv in Main.mods_res_dict)
                    {
                        Main.Logger.Log("Found Mod subdir : " + kv.Value);
                        if (Directory.Exists(kv.Value))
                        {
                            ProcessDir(kv.Value);
                        }
                        else
                        {
                            Main.Logger.Log("subdir not exsit : " + kv.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Main.Logger.Log(ex.ToString());
                }
                Interlocked.Exchange(ref isLoading, 0);
            }

            public static void Postfix()
            {
                if (!Main.enabled)
                    return;

                if (Main.settings.load_custom_config)
                    InjectData();
            }
        }

        /// <summary>
        /// 加载自定义propAllText配置
        /// </summary>
        [HarmonyPatch(typeof(StringCenter), "GetValue")]
        internal static class StringCenter_GetValue_Patch
        {
            /// <summary>外部propAllText配置</summary>
            private static Dictionary<string, string> allStrings;

            /// <summary>
            /// 加载外部propAllText配置
            /// </summary>
            /// <param name="path"></param>
            /// <remarks><see cref="StringCenter.Init()"/></remarks>
            public static void DoInjectStringData(string path)
            {
                if (!File.Exists(path))
                    return;

                if (Main.settings.detailed_custom_config_log)
                    Main.Logger.Log("Injecting processing " + path);
                try
                {
                    string text;
                    using (var fs = File.OpenText(path))
                    {
                        text = fs.ReadToEnd();
                    }
                    if (!text.IsNullOrEmpty())
                    {
                        allStrings = allStrings ?? new Dictionary<string, string>();
                        var list = new List<string>(text.Split('\n'));
                        int i = 0;
                        string[] array = new string[2];
                        for (int count = list.Count; i < count; i++)
                        {
                            list[i] = list[i].Replace("\r", "");
                            if (!list[i].IsNullOrEmpty() && !list[i].StartsWith("//"))
                            {
                                int num = list[i].IndexOf('=');
                                if (num > -1)
                                {
                                    array[0] = list[i].Substring(0, num).Trim();
                                    array[1] = list[i].Substring(num + 1, list[i].Length - num - 1);
                                    array[1] = Regex.Replace(array[1], @"(?><hc=(\d+)>)", (Match match) =>
                                    {
                                        if (int.TryParse(match.Groups[1].Value, out int result))
                                        {
                                            return new string('\n', result);
                                        }
                                        return "";
                                    });
                                    if (!array[0].IsNullOrEmpty() && !array[1].IsNullOrEmpty())
                                    {
                                        allStrings[array[0]] = array[1];
                                    }
                                }
                            }
                        }
                        Interlocked.Increment(ref Main.csvFilesCounter[1]);
                    }
                    else
                    {
                        Main.Logger.Log($"File {path} is empty!");
                    }
                }
                catch (Exception ex)
                {
                    Main.Logger.Log(ex.ToString());
                }
            }

            private static bool Prefix(string key, ref string __result)
            {
                // 若还未加载完数据则不进行替换, 预防出现race condition
                if (!Main.enabled || isLoading == 1 || allStrings == null || allStrings.Count == 0)
                    return true;

                key = key.Trim();
                // 用外部配置文件的设置替换
                if (allStrings.TryGetValue(key, out var text))
                {
                    __result = text;
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 导出propAllText
        /// </summary>
        [HarmonyPatch(typeof(StringCenter), "Init")]
        private static class StringCenter_Init_Patch
        {
            /// <summary>是否已经导出propAllText</summary>
            private static bool isDumped;

            /// <summary>
            /// 导出propAllText
            /// </summary>
            /// <remarks><see cref="StringCenter.Init()"/></remarks>
            private static void DoDumpStringData()
            {
                if (Main.settings.detailed_custom_config_log)
                    Main.Logger.Log("processing propAllText");
                TextAsset textAsset = Resources.Load<TextAsset>("Data/propAllText");
                string savepath = $"{Main.backupdir}/propAllText.txt";
                File.WriteAllText(savepath, textAsset.text);
            }

            public static void Postfix()
            {
                if (Main.enabled && Main.settings.save_config && !isDumped)
                {
                    DoDumpStringData();
                    isDumped = true;
                }
            }
        }
    }
}
