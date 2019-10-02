using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DG.Tweening.Core;
using Harmony12;
using Newtonsoft.Json;
using UnityEngine;
using UnityModManagerNet;

namespace Ju.EventExtension
{
    public class Settings : UnityModManager.ModSettings
    {
    }

    public static class Main
    {
        public static bool Enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static string dataPath = AppDomain.CurrentDomain.BaseDirectory;

        private static bool IsLoaded = false;

        #region Property

        #endregion

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            #region InitBase

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Logger = modEntry.Logger;

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;

            #endregion

            dataPath = Assembly.GetExecutingAssembly().Location;
            dataPath = Path.Combine(dataPath.Substring(0, dataPath.LastIndexOf('\\')), "Event_Date.txt");

            if (File.Exists(dataPath))
            {
                Logger.Log(dataPath);
                DateFile.EventMethodManager.RegisterEventBase(typeof(EventExtentionHandle));
            }
            
            GEvent.AddOneShot(eEvents.LoadedSavedAndBaseData, (args) =>
            {
                LoadDataText(dataPath,ref DateFile.instance.eventDate);
                Logger.Log($"{DateFile.instance.eventDate[50000][0]},{string.Join(",",DateFile.instance.eventDate[50000].SelectMany(t=>t.Value))}");
                Logger.Log($"{DateFile.instance.eventDate[900100004][0]},{string.Join(",",DateFile.instance.eventDate[900100004].SelectMany(t=>t.Value))}");
                Logger.Log($"{DateFile.instance.eventDate[500000001][0]},{string.Join(",",DateFile.instance.eventDate[500000001].SelectMany(t=>t.Value))}");
                Logger.Log($"{DateFile.instance.eventDate[500000002][0]},{string.Join(",",DateFile.instance.eventDate[500000002].SelectMany(t=>t.Value))}");
            });

            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginVertical();

            GUILayout.EndVertical();
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }

        /// <summary>
        /// 读取配置
        /// </summary>
        /// <param name="_datePath"></param>
        /// <param name="dateList"></param>
        public static void LoadDataText(string _datePath,ref  Dictionary<int, Dictionary<int, string>> dateList)
        {
            SortedDictionary<int, Dictionary<int, string>> _newDateList =
                new SortedDictionary<int, Dictionary<int, string>>();
            string _baseDate = null;
            if (File.Exists(_datePath))
            {
                //将字符串反序列化为对象
                _baseDate = File.OpenText(_datePath).ReadToEnd();
                Main.Logger.Log(_baseDate);
            }
            

            string[] lineArray = _baseDate.Replace("\r", "").Split("\n"[0]);
            //获取列的Key值
            string[] lineIndex = lineArray[0].Split(',');

            //将每一行的内容存入字典
            System.Threading.Tasks.Parallel.For(0, lineArray.Length, i =>
                {
                    //将每行数据再分成数组
                    string[] lineDate = lineArray[i].Split(',');
                    //如果该行数据的id不为#或不为空，就进行数据处理
                    if (lineDate[0] != "#" && lineDate[0] != "")
                    {
                        //数据的数据组
                        Dictionary<int, string> date = new Dictionary<int, string>();
                        //遍历所有数据值，将它们加入到数据的数据组
                        for (int a = 0; a < lineIndex.Length; a++)
                        {
                            if (lineIndex[a] != "#"&& lineIndex[a] != "")
                            {
                                try
                                {
                                    Main.Logger.Log($"{int.Parse(lineIndex[a])},{lineDate[a]}");
                                    date.Add(int.Parse(lineIndex[a]), lineDate[a]);
                                }
                                catch (Exception e)
                                {
                                }
                            }
                        }

                        try
                        {
                            //存入最终字典
                            lock (_newDateList)
                            {
                                _newDateList.Add(int.Parse(lineDate[0]), date);
                            }
                        }
                        catch (Exception e)
                        {
                            {
                                Debug.LogError(e.ToString());
                            }
                        }
                    }
                }
            );

            foreach (int a in _newDateList.Keys)
            {
                Debug.Log($"{a},{string.Join(",",_newDateList[a].Values)}");
                if (dateList.ContainsKey(a))continue;
                dateList.Add(a, _newDateList[a]);
            }
        }


        //static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        //{
        //    settings.Save(modEntry);
        //}
    }
}