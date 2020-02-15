using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityModManagerNet;

namespace ShowBiographyOfTheDead
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        public bool HolmesMode = false;//随机扰乱
        public bool saveToFile = true;//将生平(全部)保存到文件
        public int logCount = 20;//将生平的最后几行显示在log
    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            Main.settings.HolmesMode = GUILayout.Toggle(Main.settings.HolmesMode, "侦探模式", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Main.settings.saveToFile = GUILayout.Toggle(Main.settings.saveToFile, "保存到文件", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("将最后:", GUILayout.Width(30));
            int.TryParse(GUILayout.TextField(Main.settings.logCount.ToString(), 2, GUILayout.Width(30)), out Main.settings.logCount);
            GUILayout.Label("条输出到log", GUILayout.Width(30));
            GUILayout.EndHorizontal();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }


    [HarmonyPatch(typeof(DateFile), "RemoveActor")]
    public static class DateFile_RemoveActor_Patch
    {
        public static int index = 0;
        public static List<string> actorMessages = new List<string>();//Qu式传参法:设成静态成员
        public const string outputDir = ".\\Revenge\\";
        public const int minBlurLength = 3;
        public const int maxBlurLength = 10;

        //Modify by inpay start
        //static void Prefix(DateFile __instance, List<int> actorId, bool die, bool showRemove, bool make9Level = true)
        static void Prefix(List<int> actorId, bool die, bool showRemove = true)
        //Modify by inpay end
        {
            if (!Main.enabled)
                return;
            if (!die)
                return;
            List<int> deadFollower = new List<int>();//死亡村民id
            foreach (var id in actorId)
                if (id >= 0)//负的为临时对象
                {
                    if (GameData.Characters.HasChar(id))
                        if (GetGangLevelText(id) == "村民")
                            deadFollower.Add(id);
                }
            if (deadFollower.Count() <= 0)
                return;
            Main.Logger.Log(string.Format("{0}人死亡，其中{1}人为村民", actorId.Count(), deadFollower.Count()));

            if (!System.IO.Directory.Exists(outputDir))//创建目录
                System.IO.Directory.CreateDirectory(outputDir);
            foreach (var id in deadFollower)//依次输出
            {
                GetAllMessage(id);
                //去掉颜色标签,添加混淆
                for (int i = 0; i < actorMessages.Count(); ++i)
                {
                    actorMessages[i] = Regex.Replace(actorMessages[i], @"<[^>]*>", "", RegexOptions.IgnoreCase);
                    if (!Main.settings.HolmesMode)
                        continue;
                    if (UnityEngine.Random.Range(0, 100) > 70)
                        continue;
                    int length = UnityEngine.Random.Range(minBlurLength, maxBlurLength);
                    int start = UnityEngine.Random.Range(0, actorMessages[i].Count() - length);
                    if (start < 0)
                    {
                        length -= start;
                        start = 0;
                    }
                    actorMessages[i] = actorMessages[i].Substring(0, start)
                                    + new String('?', length)
                                    + (start + length < actorMessages[i].Count() ?
                                                actorMessages[i].Substring(start + length, actorMessages[i].Count() - start - length)
                                                : "");
                }
                PrintToLog(id);
                SaveToFile(id);
            }
        }
        private static string GetGangLevelText(int id)
        {
            int num2 = int.Parse(DateFile.instance.GetActorDate(id, 19, false));
            int num3 = int.Parse(DateFile.instance.GetActorDate(id, 20, false));
            int key2 = (num3 >= 0) ? 1001 : (1001 + int.Parse(DateFile.instance.GetActorDate(id, 14, false)));
            int gangValueId = DateFile.instance.GetGangValueId(num2, num3);
            string gang = DateFile.instance.presetGangGroupDateValue[gangValueId][key2];
            return gang;
        }
        private static void PrintToLog(int id)
        {
            Main.Logger.Log(DateFile.instance.GetActorName(id) + ":");
            for (int i = Math.Max(actorMessages.Count() - Main.settings.logCount, 0); i < actorMessages.Count(); ++i)
                Main.Logger.Log(actorMessages[i]);
        }
        private static void SaveToFile(int id)
        {
            string filename = string.Format("{0}{1}{2}年{3}月.txt",//用名字加年月命名
                                                DateFile.instance.GetActorName(id),
                                                UnityEngine.Random.Range(0, 100) < 5 ? "墨鱼" : "没于",//为了符合游戏特色加入错别字
                                                DateFile.instance.year.ToString(),
                                                DateFile.instance.solarTermsDate[DateFile.instance.GetDayTrun(0)][99]);

            FileStream file = new FileStream(outputDir + filename, FileMode.OpenOrCreate);
            if (file != null)
            {
                foreach (var str in actorMessages)
                {
                    byte[] data = System.Text.Encoding.Default.GetBytes(str + "\r\n");
                    file.Write(data, 0, data.Length);
                }
                file.Flush();
                file.Close();
            }
        }

        /// <summary>
        /// copy from ActorMenu.ShowActorMassage(int key)
        /// </summary>
        /// <param name="id">npcId</param>
        private static void GetAllMessage(int id)
        {
            actorMessages.Clear();
            int mianActorId = DateFile.instance.MianActorID();
            actorMessages.Add(string.Format(DateFile.instance.SetColoer(20002, "·") + " {0}{1}{2}{3}{4}\n", DateFile.instance.massageDate[8010][1].Split('|')[0], DateFile.instance.SetColoer(10002, DateFile.instance.solarTermsDate[int.Parse(DateFile.instance.GetActorDate(id, 25, applyBonus: false))][102]), DateFile.instance.massageDate[8010][1].Split('|')[1], DateFile.instance.GetActorName(id, realName: false, baseName: true), DateFile.instance.massageDate[8010][1].Split('|')[2]));
            if (DateFile.instance.actorLife.ContainsKey(id))
            {
                int num2 = Mathf.Max(DateFile.instance.GetActorFavor(false, mianActorId, id), 0);
                for (int i = 0; i < DateFile.instance.actorLife[id].Count; i++)
                {
                    int[] array = DateFile.instance.actorLife[id][i].ToArray();
                    int key2 = array[0];
                    if (DateFile.instance.actorMassageDate.ContainsKey(key2))
                    {
                        if (id != mianActorId && num2 < 30000 * int.Parse(DateFile.instance.actorMassageDate[key2][4]) / 100)
                        {
                            actorMessages.Add(string.Format(DateFile.instance.SetColoer(20002, "·") + " {0}{1}：{2}\n", DateFile.instance.massageDate[16][1] + DateFile.instance.SetColoer(10002, array[1].ToString()) + DateFile.instance.massageDate[16][3], DateFile.instance.SetColoer(20002, DateFile.instance.solarTermsDate[array[2]][0]), DateFile.instance.SetColoer(10001, DateFile.instance.massageDate[12][2])));
                        }
                        else
                        {
                            List<string> list = actorMessages;
                            string format = DateFile.instance.SetColoer(20002, "·") + " {0}{1}：" + DateFile.instance.actorMassageDate[key2][1] + "\n";
                            int massageId = array[0];
                            object[] args = SetMassageText(id, array).ToArray();
                            list.Add(string.Format(format, args));
                        }
                    }
                }
            }
            //死亡
            int num3 = int.Parse(DateFile.instance.GetActorDate(id, 26, false));
            if (num3 > 0)
            {
                actorMessages.Add(string.Format("■ {0}{1}{2}\n", DateFile.instance.massageDate[8010][2].Split(new char[]
                {
                '|'
                })[0], DateFile.instance.SetColoer(10002, DateFile.instance.GetActorDate(id, 11, false), false), DateFile.instance.massageDate[8010][2].Split(new char[]
                {
                '|'
                })[1]));
            }

            // 哈哈，把非静态函数直接挪进来搞成内置的模块，我真是个天才！！
            List<string> SetMassageText(int actorId, int[] massageDate)
            {
                int num = DateFile.instance.MianActorID();
                int key = massageDate[0];
                string[] array = DateFile.instance.actorMassageDate[key][2].Split('|');
                string[] array2 = DateFile.instance.actorMassageDate[key][99].Split('|');
                List<string> list = new List<string>
                {
                    DateFile.instance.massageDate[16][1] + DateFile.instance.SetColoer(10002, massageDate[1].ToString()) + DateFile.instance.massageDate[16][3],
                    DateFile.instance.SetColoer(20002, DateFile.instance.solarTermsDate[massageDate[2]][0])
                };
                list.Add(DateFile.instance.SetColoer(10001, DateFile.instance.GetNewMapDate(massageDate[3], massageDate[4], 98) + DateFile.instance.GetNewMapDate(massageDate[3], massageDate[4], 0)));
                for (int i = 0; i < array2.Length; i++)
                {
                    list.Add(array2[i]);
                }
                for (int j = 5; j < massageDate.Length; j++)
                {
                    int num2 = massageDate[j];
                    switch (int.Parse(array[j - 5]))
                    {
                        case 0:
                            list.Add(DateFile.instance.SetColoer((int.Parse(DateFile.instance.GetActorDate(num2, 26, applyBonus: false)) > 0) ? 20010 : 10002, DateFile.instance.GetActorName(num2)));
                            break;
                        case 1:
                            list.Add(DateFile.instance.massageDate[10][0].Split('|')[0] + DateFile.instance.SetColoer(20001 + int.Parse(DateFile.instance.GetItemDate(num2, 8)), DateFile.instance.GetItemDate(num2, 0, otherMassage: false)) + DateFile.instance.massageDate[10][0].Split('|')[1]);
                            break;
                        case 2:
                            list.Add(DateFile.instance.SetColoer(20001 + int.Parse(DateFile.instance.gongFaDate[num2][2]), DateFile.instance.massageDate[10][0].Split('|')[0] + DateFile.instance.gongFaDate[num2][0] + DateFile.instance.massageDate[10][0].Split('|')[1]));
                            break;
                        case 3:
                            list.Add(DateFile.instance.SetColoer(20008, DateFile.instance.resourceDate[num2][0]));
                            break;
                        case 4:
                            list.Add(DateFile.instance.SetColoer(20008, DateFile.instance.GetGangDate(num2, 0)));
                            break;
                        case 5:
                            {
                                int key2 = Mathf.Abs(num2);
                                list.Add(DateFile.instance.SetColoer(20011 - int.Parse(DateFile.instance.presetGangGroupDateValue[key2][2]), DateFile.instance.presetGangGroupDateValue[key2][(num2 > 0) ? 1001 : (1001 + int.Parse(DateFile.instance.GetActorDate(actorId, 14, applyBonus: false)))]));
                                break;
                            }
                    }
                }
                return list;
            }
        }

    }
}
