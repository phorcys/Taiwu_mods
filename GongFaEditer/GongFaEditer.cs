using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;
using UnityEngine.UI;
using System.Drawing;
using Newtonsoft.Json;
using System.IO;

namespace Ju.GongFaEditer
{

    public class Settings : UnityModManager.ModSettings
    {
    }

    public static class Main
    {
        public static bool Enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        #region Property

        public static string About = "功法编辑器(by Ju)";

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

            return true;
        }

        //功法id->属性id->0:原内容,现内容
        public static Dictionary<int, Dictionary<int, string[]>> EditHostory = new Dictionary<int, Dictionary<int, string[]>>();
        public static string savePath = Path.Combine(Application.dataPath, "GongFaEditer").Replace('/', '\\');
        public static string saveFilePath = Path.Combine(savePath, "gongfa.dat");
        public static List<GongFaAttribute> gongFaAttributes = new List<GongFaAttribute>();

        public static void LoadAllGongFaAttributes()
        {
            if (gongFaAttributes == null) gongFaAttributes = new List<GongFaAttribute>();
            var props = typeof(GongFa).GetProperties().ToList();
            foreach (var propertyInfo in props)
            {
                var attr = propertyInfo.GetCustomAttribute<GongFaAttribute>();
                if (attr!=null)
                {
                    Logger.Log($"添加属性{attr.DisplayName}({attr.Index})");
                    gongFaAttributes.Add(attr);
                }
            }
            Logger.Log($"总计获取属性{gongFaAttributes.Count}个");
        }

        public static void LoadChangedGongFa()
        {
            Logger.Log($"开始获取mod数据!");
            if (!Directory.Exists(savePath)) return;
            if (!File.Exists(saveFilePath)) return;
            var allChangedSave = File.ReadAllText(saveFilePath);
            if (string.IsNullOrEmpty(allChangedSave)) return;
            try
            {
                 EditHostory = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, string[]>>>(allChangedSave);
            }
            catch (Exception)
            {
                Debug.Log("检测到老版本mod数据,已执行清理!");
                File.Delete(saveFilePath);
                EditHostory = null;
            }
            if (EditHostory == null) EditHostory = new Dictionary<int, Dictionary<int, string[]>>();
            if (DateFile.instance == null)
            {
                Debug.Log("系统功法加载失败!");
                return;
            }
            Debug.Log(DateFile.instance.gongFaDate);
            foreach (var gongfa in EditHostory)
            {
                foreach (var gongfaAtt in gongfa.Value)
                {
                    DateFile.instance.gongFaDate[gongfa.Key][gongfaAtt.Key] = gongfaAtt.Value[1];
                }
            }
        }

        public static void SaveChangedGongFa()
        {
            if (EditHostory.Count == 0) return;
            if (! Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
             File.WriteAllText(saveFilePath, JsonConvert.SerializeObject(EditHostory));
        }
        
        private static Rect windowRect;

        private static Vector2 postion = Vector2.zero;

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginVertical();
            if (EditHostory != null && EditHostory.Count > 0)
            {
                if (MakeGui.Instance.isShow)
                {
                    GUILayout.Label($"当前正在编辑功法中!");
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"当前已更改{EditHostory.Count}套功法!",GUILayout.Width(150));
                    if (GUILayout.Button("重置所有",GUILayout.Width(80)))
                    {
                        foreach (var history in EditHostory)
                        {
                            foreach (var attrValue in history.Value)
                            {
                                DateFile.instance.gongFaDate[history.Key][attrValue.Key] = attrValue.Value[0];
                            }
                        }
                        EditHostory.Clear();
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                    postion = GUILayout.BeginScrollView(postion);
                    var removeGongfaList = new List<int>();
                    foreach (var history in EditHostory)
                    {
                        var removeAttrList = new List<int>();
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(DateFile.instance.gongFaDate[history.Key][0]);
                        if (GUILayout.Button("删除", GUILayout.Width(45)))
                        {
                            removeGongfaList.Add(history.Key);
                        }
                        GUILayout.EndHorizontal();
                        foreach (var attrValue in history.Value)
                        {
                            var attr = gongFaAttributes.FirstOrDefault(t => t.Index == attrValue.Key);
                            if (attr == null) continue;
                            GUILayout.BeginHorizontal();
                            GUILayout.Label($"{attr.DisplayName}({attr.Index}):",GUILayout.Width(160));
                            var oldValue = attrValue.Value[1];
                            attrValue.Value[1] = GUILayout.TextField(attrValue.Value[1]);
                            if (GUILayout.Button("初始化",GUILayout.Width(65)))
                            {
                                attrValue.Value[1] = attrValue.Value[0];
                            }
                            if (oldValue != attrValue.Value[1])
                            {
                                DateFile.instance.gongFaDate[history.Key][attrValue.Key] = attrValue.Value[1];
                            }
                            if (GUILayout.Button("删除", GUILayout.Width(45)))
                            {
                                removeAttrList.Add(attrValue.Key);
                            }
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.Label("----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                        foreach (var attrIndex in removeAttrList)
                        {
                            DateFile.instance.gongFaDate[history.Key][attrIndex] = history.Value[attrIndex][0];
                            history.Value.Remove(attrIndex);
                            if (history.Value.Count == 0)
                            {
                                removeGongfaList.Add(history.Key);
                            }
                        }
                    }
                    GUILayout.EndScrollView();
                    foreach (var gongfaId in removeGongfaList)
                    {
                        foreach (var attr in EditHostory[gongfaId])
                        {
                            DateFile.instance.gongFaDate[gongfaId][attr.Key] = attr.Value[0];
                        }
                        EditHostory.Remove(gongfaId);
                    }
                }
            }
            else
            {
                GUILayout.Label($"当前暂无任何功法更改的数据!");
            }
            GUILayout.EndVertical();
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }


        //static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        //{
        //    settings.Save(modEntry);
        //}
    }
}