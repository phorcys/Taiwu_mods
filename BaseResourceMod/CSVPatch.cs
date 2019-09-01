using Harmony12;
using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
namespace BaseResourceMod
{
    public static class CSVPatch
    {
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
            string ns = Regex.Replace(text, "\r", "");
            // 添加字体颜色格式，<color=AABBCCFF>文字</color>
            ns = Regex.Replace(ns, "C_D", "</color>");
            ns = Regex.Replace(ns, @"C_\d\d\d\d\d", delegate (Match match)
            {
                string v = match.ToString();
                int colorkey = int.Parse(v.Substring(2));
                return Main.textcolor.ContainsKey(colorkey) ? Main.textcolor[colorkey] : v;
            });
            ns = "index" + ns.Substring(1);

            // 解析csv
            using (CsvReader csv = new CsvReader(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(ns))), true))
            {
                int fieldCount = csv.FieldCount;

                string[] headers = csv.GetFieldHeaders();
                headers[0] = "#";

                //Debug.Log("processing  header " + string.Join("", headers));
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
                    Dictionary<int, string> dictionary = new Dictionary<int, string>();
                    for (int j = 0; j < fieldCount; j++)
                    {
                        if (headers[j] != "#" && headers[j] != "" && headerint[j] != passDateIndex)
                        {
                            dictionary[headerint[j]] = Regex.Unescape(csv[j]);
                        }
                    }

                    if (!int.TryParse(csv[0], out var dkey))
                    {
                        Main.Logger.Log($"Illegal Primary Key, FileName: {path} Key: {csv[0]}");
                        continue;
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
            static private void NewGetData(string dateName, Dictionary<int, Dictionary<int, string>> dateList, int passDateIndex)
            {
                dateList.Clear();
                // 载入路径
                string path = $"{"."}/Data/{dateName}.txt";

                //打开文件
                string text;
                if (File.Exists(path))
                {
                    Main.Logger.Log("processing " + path);
                    text = File.OpenText(path).ReadToEnd();
                }
                else
                {
                    // 此处更改路径仅用于在出现bug时输出问题路径
                    path = dateName;
                    Main.Logger.Log("processing " + dateName);
                    text = GetSprites.instance.baseGameDate[dateName];
                }

                try
                {
                    TextProcessor(dateList, text, passDateIndex, path);
                    // 保存设置
                    if (Main.settings.save_config == true)
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

            static private bool Prefix(string dateName, Dictionary<int, Dictionary<int, string>> dateList, int passDateIndex)
            {
                if (!Main.enabled)
                    return true;
                NewGetData(dateName, dateList, passDateIndex);
                return false;
            }
        }

        /// <summary>
        ///  读取基础数据完毕后，读取自定义数据
        /// </summary>
        [HarmonyPatch(typeof(DateFile), "LoadGameConfigs")]
        private static class Loading_LoadBaseDate_Patch
        {
            /// <summary>
            /// 将自定义数据文件信息载入字典中
            /// </summary>
            /// <param name="path">自定义数据文件路径</param>
            /// <param name="dict">目标字典</param>
            /// <param name="passDateIndex">跳过的字段</param>
            private static void DoInjectDataToDict(string path, Dictionary<int, Dictionary<int, string>> dict, int passDateIndex = -1)
            {
                Main.Logger.Log("Injecting processing " + path);
                //打开文件
                if (!File.Exists(path))
                    return;

                var text = File.OpenText(path).ReadToEnd();
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
                Main.Logger.Log($"Ijected  {counter} line from  {path} ");
            }

            /// <summary>
            /// 处理自定义数据文件
            /// </summary>
            /// <param name="path"></param>
            private static void ProcessDir(string path)
            {
                Dictionary<string, SortedDictionary<int, string>> allfiles = new Dictionary<string, SortedDictionary<int, string>>();
                // 遍历该目录及子目录下所有 txt 文件
                foreach (string fname in Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories))
                {
                    string filename = Path.GetFileName(fname);
                    string ufname = fname.Replace("\\", "/");
                    Main.Logger.Log($"Found {ufname} in subdir {path}");
                    try
                    {
                        if (File.Exists(ufname))
                        {
                            // 获取数据类型，如ItemDate.txt.123.txt中的ItemDate
                            string ftype = filename.Substring(0, filename.IndexOf('.')).ToLower();
                            int num = 1000;
                            var m = Regex.Match(filename, @"\.txt\.(\d+)\.txt", RegexOptions.IgnoreCase);
                            if (m != null && m.Length > 0)
                            {
                                // 获得文件名中的序号, 如ItemDate.txt.123.txt中的123
                                string snum = m.Value.Substring(5, m.Value.Length - 9);
                                num = int.Parse(snum);
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
                    Main.Logger.Log("begin cate  : " + kv.Key);
                    try
                    {
                        // 获取数据文件对应的游戏数据字典
                        var dict = Main.GetCSVDictRef(kv.Key);
                        foreach (var vv in kv.Value)
                        {
                            Main.Logger.Log("begin cate  file : " + vv.Value);
                            if (dict != null)
                            {
                                DoInjectDataToDict(vv.Value, dict);
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
            private static void PostInjectData()
            {
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

            private static void Postfix()
            {
                if (Main.enabled)
                    PostInjectData();
            }
        }
    }
}
