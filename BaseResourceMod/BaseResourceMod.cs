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
    public class Settings : UnityModManager.ModSettings
    {
        public bool save_config = true;
        public bool load_custom_config = true;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

    }
    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static string backupdir = "./Backup/txt/";
        public static string resdir = "./Data/";

        public static Dictionary<string, string> mods_res_dict;


        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            
            if (!Directory.Exists(backupdir))
            {
                System.IO.Directory.CreateDirectory(backupdir);
            }

            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            return true;
        }

        public static void registModResDir(UnityModManager.ModEntry modEntry,string respath)
        {
            mods_res_dict[modEntry.Info.DisplayName] = respath;
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
            GUILayout.BeginVertical("Box");
            GUILayout.Label("基础资源框架：");
            settings.save_config = GUILayout.Toggle(settings.save_config, "启动时保存原始配置文件到游戏 根目录下的 Backup/txt 目录下");
            GUILayout.Label("开启后启动时，mod会保存当前版本游戏配置文件到 Backup/txt 目录下，txt为原始csv文件，json为解析后的游戏内配置数据");
            settings.load_custom_config = GUILayout.Toggle(settings.load_custom_config, "启动时增量载入 游戏 根目录下 Data/txt 内的配置文件");
            GUILayout.Label("自定义配置文件命名方式形如 Item_date.txt.001.txt  其中数字如 001 为加载顺序，从001 开始 顺序加载，最后加载 不带数字后缀的文件");
            GUILayout.Label("更多信息参见 https://github.com/phorcys/Taiwu_mods");
            GUILayout.EndVertical();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        public static Dictionary<int, string> textcolor = new Dictionary<int, string>()
        {
            { 10000,"<color=#323232FF>"},
            { 10001,"<color=#4B4B4BFF>"},
            { 10002,"<color=#B97D4BFF>"},
            { 10003,"<color=#9B8773FF>"},
            { 10004,"<color=#AF3737FF>"},
            { 10005,"<color=#FFE100FF>"},
            { 10006,"<color=#FF44A7FF>"},
            { 20001,"<color=#E1CDAAFF>"},
            { 20002,"<color=#8E8E8EFF>"},
            { 20003,"<color=#FBFBFBFF>"},
            { 20004,"<color=#6DB75FFF>"},
            { 20005,"<color=#8FBAE7FF>"},
            { 20006,"<color=#63CED0FF>"},
            { 20007,"<color=#AE5AC8FF>"},
            { 20008,"<color=#E3C66DFF>"},
            { 20009,"<color=#F28234FF>"},
            { 20010,"<color=#E4504DFF>"},
            { 20011,"<color=#EDA723FF>"},
        };

        static public Dictionary<string, string> date_instance_dict = new Dictionary<string, string>()
        {
            {
                "ability_date",
                "abilityDate"
            }, {
                "actorattr_date",
                "actorAttrDate"
            }, {
                "actorfeature_date",
                "actorFeaturesDate"
            }, {
                "actormassage_date",
                "actorMassageDate"
            }, {
                "actorname_date",
                "actorNameDate"
            }, {
                "actorsurname_date",
                "actorSurnameDate"
            }, {
                "age_date",
                "ageDate"
            }, {
                "aishoping_date",
                "aiShopingDate"
            }, {
                "allworldmap_date",
                "allWorldDate"
            }, {
                "attacktyp_date",
                "attackTypDate"
            }, {
                "basemission_date",
                "baseMissionDate"
            }, {
                "baseskill_date",
                "baseSkillDate"
            }, {
                "basetips_date",
                "baseTipsDate"
            }, {
                "battlemap_date",
                "battleMapDate"
            }, {
                "battlerated_date",
                "battleRatedDate"
            }, {
                "battlestate_date",
                "battleStateDate"
            }, {
                "battletyp_date",
                "battleTypDate"
            }, {
                "body_date",
                "bodyInjuryDate"
            }, {
                "buffattr_date",
                "buffAttrDate"
            }, {
                "changeequip_date",
                "changeEquipDate"
            }, {
                "cricketbattle_date",
                "cricketBattleDate"
            }, {
                "cricket_date",
                "cricketDate"
            }, {
                "cricketplace_date",
                "cricketPlaceDate"
            }, {
                "enemyrand_date",
                "enemyRandDate"
            }, {
                "enemyteam_date",
                "enemyTeamDate"
            }, {
                "event_date",
                "eventDate"
            }, {
                "gamelevel_date",
                "gameLevelDate"
            }, {
                "gangequip_date",
                "gangEquipDate"
            }, {
                "ganggroup_date",
                "presetGangGroupDate"
            }, {
                "ganggroupvalue_date",
                "presetGangGroupDateValue"
            }, {
                "generation_date",
                "generationDate"
            }, {
                "gongfa_date",
                "gongFaDate"
            }, {
                "gongfafpower_date",
                "gongFaFPowerDate"
            }, {
                "gongfaotherfpower_date",
                "gongFaFPowerDate"
            }, // otherFPower 合并入 gongFaFPowerDate
            {
                "goodness_date",
                "goodnessDate"
            }, {
                "homeplace_date",
                "basehomePlaceDate"
            }, {
                "homeshopevent_date",
                "homeShopEventDate"
            }, {
                "homeshopeventtyp_date",
                "homeShopEventTypDate"
            }, {
                "hometyp_date",
                "homeTypDate"
            }, {
                "identity_date",
                "identityDate"
            }, {
                "injury_date",
                "injuryDate"
            }, {
                "item_date",
                "presetitemDate"
            }, {
                "itempower_date",
                "itemPowerDate"
            }, {
                "loadingimage_date",
                "loadingImageDate"
            }, {
                "makeitem_date",
                "makeItemDate"
            }, {
                "massage_date",
                "massageDate"
            }, {
                "moodtyp_date",
                "moodTypDate"
            }, {
                "palceworld_date",
                "placeWorldDate"
            }, {
                "partworldmap_date",
                "partWorldMapDate"
            }, {
                "poison_date",
                "poisonDate"
            }, {
                "presetactor_date",
                "presetActorDate"
            }, {
                "presetgang_date",
                "presetGangDate"
            }, {
                "qivaluestate_date",
                "qiValueStateDate"
            }, {
                "readbook_date",
                "readBookDate"
            }, {
                "resource_date",
                "resourceDate"
            }, {
                "scorebooty_date",
                "scoreBootyDate"
            }, {
                "scorevalue_date",
                "scoreValueDate"
            }, {
                "skill_date",
                "skillDate"
            }, {
                "solarterms_date",
                "solarTermsDate"
            }, {
                "storybuff_date",
                "storyBuffDate"
            }, {
                "story_date",
                "baseStoryDate"
            }, {
                "storyevent_date",
                "storyEventDate"
            }, {
                "storyshop_date",
                "storyShopDate"
            }, {
                "storyterrain_date",
                "storyTerrain"
            }, {
                "studydisk_date",
                "studyDiskDate"
            }, {
                "talk_date",
                "talkDate"
            }, {
                "teaching_date",
                "teachingDate"
            }, {
                "timeworkbooty_date",
                "timeWorkBootyDate"
            }, {
                "tipsmassage_date",
                "tipsMassageDate"
            }, {
                "trunevent_date",
                "trunEventDate"
            }, {
                "villagename_date",
                "villageNameDate"
            }
        };

        public static T GetFieldValue<T>(object obj, string fieldName)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            var field = obj.GetType().GetField(fieldName, BindingFlags.Public |
                                                          BindingFlags.NonPublic |
                                                          BindingFlags.Instance);

            if (field == null)
                throw new ArgumentException("fieldName", "No such field was found.");

            if (!typeof(T).IsAssignableFrom(field.FieldType))
                throw new InvalidOperationException("Field type and requested type are not compatible.");

            return (T)field.GetValue(obj);
        }

        static public Dictionary<int, Dictionary<int, string>>  getDictRef(string cate)
        {
            if(cate == "ActorFace_Date")
            {
                return GetSprites.instance.actorFaceDate;
            }
            if(date_instance_dict.ContainsKey(cate))
            {
                return GetFieldValue<Dictionary<int, Dictionary<int, string>>>(DateFile.instance, date_instance_dict[cate]);
            }
            return null;
        }
    }

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
                    
                    if(Main.settings.save_config == true)
                    {
                       
                        string savepath = string.Format("{0}/{1}.txt", Main.backupdir, dateName);

                        var arr = Newtonsoft.Json.JsonConvert.SerializeObject(dateList);
                        System.IO.File.WriteAllText(savepath, text);
                        System.IO.File.WriteAllText(savepath + ".json", arr);
                    }


                }
                catch(Exception e)
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
        public static void do_inject_data_to_dict(string path,  Dictionary<int, Dictionary<int,string>> dict, int passDateIndex =-1)
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
                Main.Logger.Log(String.Format("Found {0} in subdir {1}" , ufname, path));
                try
                {
                    if (File.Exists(ufname))
                    {
                        string ftype = filename.Substring(0, filename.IndexOf('.')).ToLower();
                        int num = 1000;
                        var m = Regex.Match(filename, @"\.txt\.(\d+)\.txt", RegexOptions.IgnoreCase);
                        if (m != null )
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

            foreach(var kv in allfiles)
            {
                Main.Logger.Log("begin cate  : " + kv.Key);
                foreach(var vv in kv.Value.OrderBy(o=> o.Key)
                    .ToDictionary((KeyValuePair<int, string> o) => o.Key, (KeyValuePair<int, string> p) => p.Value) )
                {
                    try {
                        Main.Logger.Log("begin cate  file : " + vv.Value);
                        var dict = Main.getDictRef(kv.Key);
                        if(dict != null)
                        {
                            do_inject_data_to_dict(vv.Value, dict);
                        }
                        else
                        {
                            Main.Logger.Log("Error  cate " + kv.Key+ "  file : " + vv.Value);
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
                    if(Directory.Exists(path))
                    {
                        processDir(path);
                    }
                }
            }
            

            foreach(var kv in Main.mods_res_dict)
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
                if (codes[i].opcode == OpCodes.Ret && codes[i - 1].opcode == OpCodes.Stfld )
                {
                    startIndex = i ;
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