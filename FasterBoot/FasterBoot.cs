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

namespace FasterBoot
{

    public static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            modEntry.OnToggle = OnToggle;
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
                return false;

            enabled = value;

            return true;
        }

    }

    /// <summary>
    ///  基础数据CSV读取加速
    /// </summary>
    [HarmonyPatch(typeof(GetSprites), "GetDate")]
    public static class GetSprites_GetDate_Patch
    {

        static public void new_GetData(string dateName, ref Dictionary<int, Dictionary<int, string>> dateList, int passDateIndex)
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
            ns = Regex.Replace(ns, @"C_\d+.*?C_D", delegate (Match match)
            {
                string v = match.ToString();
                return "<color=#"  + v.Substring(2,5)+ ">" + v.Substring(7,v.Length-10) + "</color>";
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
                        if(csv[0]==null || csv[0]=="")
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
                }catch(Exception e)
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
                injectedCodes.Add(new CodeInstruction(OpCodes.Ldarga_S, 2));
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


}