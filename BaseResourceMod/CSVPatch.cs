using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using LumenWorks.Framework.IO.Csv;
namespace BaseResourceMod
{
    /// <summary>
    ///  基础数据CSV读取Hook
    /// </summary>
    [HarmonyPatch(typeof(GetSprites), "GetDate")]
    public static class GetSprites_GetDate_Patch
    {

        static public void new_GetData(string dateName, Dictionary<int, Dictionary<int, string>> dateList, int passDateIndex)
        {
            dateList.Clear();

            string path = string.Format("{0}/Data/{1}.txt", ".", dateName);

            Debug.Log("processing " + path);
            //打开文件
            string text = "";
            if (File.Exists(path))
            {
                text = File.OpenText(path).ReadToEnd();
            }
            else
            {
                text = GetSprites.instance.baseGameDate[dateName];
            }
            //处理替换
            string ns = Regex.Replace(text, "\r", "");
            ns = Regex.Replace(ns, "C_D", "</color>");
            ns = Regex.Replace(ns, @"C_\d\d\d\d\d", delegate (Match match)
            {
                string v = match.ToString();
                int colorkey = int.Parse(v.Substring(2));
                return Main.textcolor.ContainsKey(colorkey) ? Main.textcolor[colorkey] : v;
            });
            ns = "index" + ns.Substring(1);

            //解析csv
            using (CsvReader csv = new CsvReader(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(ns))), true))
            {
                int fieldCount = csv.FieldCount;

                string[] headers = csv.GetFieldHeaders();
                headers[0] = "#";

                //Debug.Log("processing  header " + string.Join("", headers));
                int[] headerint = new int[headers.Length];
                for (int z = 1; z < headers.Length; z++)
                {
                    if (headers[z] != "#" && headers[z] != "")
                    {
                        headerint[z] = int.Parse(headers[z]);
                    }
                    else
                    {
                        headerint[z] = -1000;
                    }
                }
                try
                {
                    while (csv.ReadNextRecord())
                    {
                        if (csv[0] == null || csv[0] == "")
                        {
                            //Debug.Log(" empty line :"+ csv );
                            continue;
                        }
                        Dictionary<int, string> dictionary = new Dictionary<int, string>();
                        for (int j = 0; j < fieldCount; j++)
                        {
                            if (headers[j] != "#" && headers[j] != "" && headerint[j] != passDateIndex)
                            {
                                dictionary.Add(headerint[j], Regex.Unescape(csv[j]));
                            }
                        }
                        dateList.Add(int.Parse(csv[0]), dictionary);
                    }

                    if (Main.settings.save_config == true)
                    {

                        string savepath = string.Format("{0}/{1}.txt", Main.backupdir, dateName);

                        var arr = Newtonsoft.Json.JsonConvert.SerializeObject(dateList);
                        System.IO.File.WriteAllText(savepath, text);
                        System.IO.File.WriteAllText(savepath + ".json", arr);
                    }


                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    Debug.Log(e.StackTrace);
                }

            }
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Main.Logger.Log(" Transpiler init codes ");
            var codes = new List<CodeInstruction>(instructions);

            var foundtheforend = true;
            int startIndex = 0;


            if (foundtheforend)
            {
                var injectedCodes = new List<CodeInstruction>();

                // 注入 IL code 
                //
                injectedCodes.Add(new CodeInstruction(OpCodes.Ldarg_1));
                injectedCodes.Add(new CodeInstruction(OpCodes.Ldarg_2));
                injectedCodes.Add(new CodeInstruction(OpCodes.Ldarg_3));
                injectedCodes.Add(new CodeInstruction(OpCodes.Call, typeof(GetSprites_GetDate_Patch).GetMethod("new_GetData")));
                injectedCodes.Add(new CodeInstruction(OpCodes.Ret));

                codes.InsertRange(startIndex, injectedCodes);
            }
            else
            {
                Main.Logger.Log(" game changed ... this mod failed to find code to patch...");
            }

            //Main.Logger.Log(" dump the patch codes ");

            //for (int i = 0; i < codes.Count; i++)
            //{
            //    Main.Logger.Log(String.Format("{0} : {1}  {2}", i, codes[i].opcode, codes[i].operand));
            //}
            return codes.AsEnumerable();
        }
    }


    /// <summary>
    ///  读取基础数据完毕后，读取自定义数据
    /// </summary>
    [HarmonyPatch(typeof(Loading), "LoadBaseDate")]
    public static class Loading_LoadBaseDate_Patch
    {
        public static void do_inject_data_to_dict(string path, Dictionary<int, Dictionary<int, string>> dict, int passDateIndex = -1)
        {
            Debug.Log("Injecting processing " + path);
            int counter = 0;
            //打开文件
            string text = "";
            if (File.Exists(path))
            {
                text = File.OpenText(path).ReadToEnd();
            }
            else
            {
                return;
            }
            //处理替换
            string ns = Regex.Replace(text, "\r", "");
            ns = Regex.Replace(ns, "C_D", "</color>");
            ns = Regex.Replace(ns, @"C_\d\d\d\d\d", delegate (Match match)
            {
                string v = match.ToString();
                int colorkey = int.Parse(v.Substring(2));
                return Main.textcolor.ContainsKey(colorkey) ? Main.textcolor[colorkey] : v;
            });
            ns = "index" + ns.Substring(1);

            //解析csv
            using (CsvReader csv = new CsvReader(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(ns))), true))
            {
                int fieldCount = csv.FieldCount;

                string[] headers = csv.GetFieldHeaders();
                headers[0] = "#";

                //Debug.Log("processing  header " + string.Join("", headers));
                int[] headerint = new int[headers.Length];
                for (int z = 1; z < headers.Length; z++)
                {
                    if (headers[z] != "#" && headers[z] != "")
                    {
                        headerint[z] = int.Parse(headers[z]);
                    }
                    else
                    {
                        headerint[z] = -1000;
                    }
                }
                try
                {
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
                                dictionary.Add(headerint[j], Regex.Unescape(csv[j]));
                            }
                        }

                        int dkey = int.Parse(csv[0]);
                        dict[dkey] = dictionary;
                        counter = counter + 1;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    Debug.Log(e.StackTrace);
                }
                Main.Logger.Log(String.Format("Ijected  {0} line from  {1} ", counter, path));
            }
        }

        public static void processDir(string path)
        {
            Dictionary<string, Dictionary<int, string>> allfiles = new Dictionary<string, Dictionary<int, string>>();
            //遍历 子目录下所有txt
            foreach (string fname in Directory.GetFiles(path, "*.txt"))
            {

                string filename = Path.GetFileName(fname);
                string ufname = fname.Replace("\\", "/");
                Main.Logger.Log(String.Format("Found {0} in subdir {1}", ufname, path));
                try
                {
                    if (File.Exists(ufname))
                    {
                        string ftype = filename.Substring(0, filename.IndexOf('.')).ToLower();
                        int num = 1000;
                        var m = Regex.Match(filename, @"\.txt\.(\d+)\.txt", RegexOptions.IgnoreCase);
                        if (m != null && m.Length > 0)
                        {
                            string snum = m.Value.Substring(5, m.Value.Length - 9);
                            num = int.Parse(snum);
                        }
                        if (!allfiles.ContainsKey(ftype))
                        {
                            allfiles[ftype] = new Dictionary<int, string>();
                        }
                        allfiles[ftype].Add(num, ufname);
                        Main.Logger.Log(String.Format("add {0} in subdir {1} to  parsing queue", ufname, path));
                    }
                    else
                    {
                        Main.Logger.Log(String.Format("file not exsit {0} in subdir {1}", ufname, path));
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    Debug.Log(e.StackTrace);
                }

            }

            foreach (var kv in allfiles)
            {
                Main.Logger.Log("begin cate  : " + kv.Key);
                foreach (var vv in kv.Value.OrderBy(o => o.Key)
                    .ToDictionary((KeyValuePair<int, string> o) => o.Key, (KeyValuePair<int, string> p) => p.Value))
                {
                    try
                    {
                        Main.Logger.Log("begin cate  file : " + vv.Value);
                        var dict = Main.getCSVDictRef(kv.Key);
                        if (dict != null)
                        {
                            do_inject_data_to_dict(vv.Value, dict);
                        }
                        else
                        {
                            Main.Logger.Log("Error  cate " + kv.Key + "  file : " + vv.Value);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message);
                        Debug.Log(e.StackTrace);
                    }
                }
            }
        }

        public static void post_InjectData()
        {
            if (Directory.Exists(Main.resdir))
            {
                //遍历 Date目录子目录
                foreach (string path in Directory.GetDirectories(Main.resdir))
                {
                    Main.Logger.Log("Found subdir : " + path);
                    if (Directory.Exists(path))
                    {
                        processDir(path);
                    }
                }
            }


            foreach (var kv in Main.mods_res_dict)
            {
                Main.Logger.Log("Found Mod subdir : " + kv.Value);
                if (Directory.Exists(kv.Value))
                {
                    processDir(kv.Value);
                }
                else
                {
                    Main.Logger.Log("subdir not exsit : " + kv.Value);
                }
            }
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Main.Logger.Log(" Transpiler init codes ");
            var codes = new List<CodeInstruction>(instructions);

            var foundtheforend = false;
            int startIndex = -1;

            //寻找注入点
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ret && codes[i - 1].opcode == OpCodes.Stfld)
                {
                    startIndex = i;
                    foundtheforend = true;
                    Main.Logger.Log(" found the end of the ret , at index: " + i);
                }

            }


            if (foundtheforend)
            {
                var injectedCodes = new List<CodeInstruction>();

                // 注入 IL code 
                //
                injectedCodes.Add(new CodeInstruction(OpCodes.Call, typeof(Loading_LoadBaseDate_Patch).GetMethod("post_InjectData")));
                injectedCodes.Add(new CodeInstruction(OpCodes.Ret));

                codes.InsertRange(startIndex, injectedCodes);
            }
            else
            {
                Main.Logger.Log(" game changed ... this mod failed to find code to patch...");
            }

            //Main.Logger.Log(" dump the patch codes ");

            //for (int i = 0; i < codes.Count; i++)
            //{
            //    Main.Logger.Log(String.Format("{0} : {1}  {2}", i, codes[i].opcode, codes[i].operand));
            //}
            return codes.AsEnumerable();
        }
    }
}
