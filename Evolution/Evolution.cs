using Harmony12;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityModManagerNet;

namespace Evolution
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool addFeature = true;
        public bool expCost = true;
        public int changeAttrSpeed = 1;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

    }

    public static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static Settings setting;
        public static Dictionary<int, string> teammateAge;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            setting = Settings.Load<Settings>(modEntry);
            teammateAge = new Dictionary<int, string>();

            Logger = modEntry.Logger;

            modEntry.OnToggle = OnToggle;

            modEntry.OnGUI = OnGUI;

            //if (enabled)
            {
                string resdir = System.IO.Path.Combine(modEntry.Path, "Data");
                Logger.Log(" resdir :" + resdir);
                BaseResourceMod.Main.registModResDir(modEntry, resdir);
            }

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            bool cond = (DateFile.instance == null || DateFile.instance.actorsDate == null ||
                         !DateFile.instance.actorsDate.ContainsKey(DateFile.instance.mianActorId));
            if (cond)
            {
                GUILayout.Label("未加载存档！");
                return;
            }

            if (!DateFile.instance.gongFaDate.ContainsKey(150370))
            {
                GUILayout.Label("增量数据未正常加载！");
                return;
            }

            if (!enabled)
            {
                if (DateFile.instance.actorGongFas[DateFile.instance.mianActorId].ContainsKey(150370))
                {
                    GUILayout.Label("结束当前时节后MOD将被卸载。");
                }
                else
                {
                    GUILayout.Label("MOD当前未生效，可放心移除。");
                }

                return;
            }


            if (!DateFile.instance.actorGongFas[DateFile.instance.mianActorId].ContainsKey(150370))
            {
                //DateFile.instance.ChangeActorGongFa(DateFile.instance.mianActorId, 150370, 0, 0, 0, true);
            }
            else
            {
                GUILayout.Label("洗脉伐髓，脱胎换骨");
                GUILayout.BeginHorizontal("Box");
                setting.addFeature = GUILayout.Toggle(setting.addFeature, "特质成长模式");

                GUILayout.Label("说明： 取消勾选,将不再增加特质，只增加属性，并且以下设置有效");
                GUILayout.EndHorizontal();

                GUILayout.Label("心法等级越高，成长上限越高");
                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("研读进度："
                                + "正" + DateFile.instance.actorGongFas[DateFile.instance.mianActorId][150370][1]);
                GUILayout.BeginHorizontal("Box", GUILayout.Width(300));
                GUILayout.FlexibleSpace();
                setting.expCost = GUILayout.Toggle(setting.expCost, "心法成长消耗熟练度");
                GUILayout.FlexibleSpace();
                GUILayout.Label("属性增长速度：");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("<", GUILayout.Width(30)) && Main.setting.changeAttrSpeed > 1)
                {
                    Main.setting.changeAttrSpeed--;
                }

                GUILayout.Label("  " + Main.setting.changeAttrSpeed + "  ");
                if (GUILayout.Button(">", GUILayout.Width(30)) && Main.setting.changeAttrSpeed <DateFile.instance.actorGongFas[DateFile.instance.mianActorId][150370][1])
                {
                    Main.setting.changeAttrSpeed++;
                }

                GUILayout.EndHorizontal();
                GUILayout.EndHorizontal();

            }
        }
    }


    [HarmonyPatch(typeof(UIDate), "TrunChange")]
    public static class UIDate_TrunChange_Patch
    {
        private static void Prefix()
        {
            if (!DateFile.instance.gongFaDate.ContainsKey(150370))
            {
                return;
            }

            if (!Main.enabled)
            {
                if (DateFile.instance.actorGongFas[DateFile.instance.mianActorId].ContainsKey(150370))
                {
                    DateFile.instance.RemoveMainActorEquipGongFa(150370);

                    foreach (var key in DateFile.instance.actorEquipGongFas.Keys)
                    {
                        if (DateFile.instance.actorEquipGongFas[key][0][0] == 150370 ||
                            DateFile.instance.actorEquipGongFas[key][0][1] == 150370)
                        {
                            if (DateFile.instance.actorGongFas[key].ContainsKey(150370))
                            {
                                DateFile.instance.actorGongFas[key].Remove(150370);
                            }

                            DateFile.instance.SetActorEquipGongFa(key, true, true);
                        }
                    }

                    foreach (var key in DateFile.instance.actorGongFas.Keys)
                    {
                        if (DateFile.instance.actorGongFas[key].ContainsKey(150370))
                        {
                            DateFile.instance.actorGongFas[key].Remove(150370);
                        }
                    }

                    if (DateFile.instance.gongFaBookPages.ContainsKey(150370))
                    {
                        DateFile.instance.gongFaBookPages.Remove(150370);
                    }
                }

                return;
            }

            if (!DateFile.instance.actorGongFas[DateFile.instance.mianActorId].ContainsKey(150370))
            {

                int[] allQi = DateFile.instance.GetActorAllQi(DateFile.instance.mianActorId);
                if (allQi[0] + allQi[1] + allQi[2] + allQi[3] + allQi[4] >= 500 &&
                    DateFile.instance.GetActorValue(DateFile.instance.mianActorId, 601, true) -
                    int.Parse(DateFile.instance.GetActorDate(DateFile.instance.mianActorId, 601, true)) >= 100)
                {
                    DateFile.instance.ChangeActorGongFa(DateFile.instance.mianActorId, 150370, 50, 0, 0, false);
                }

                if (DateFile.instance.actorGongFas[DateFile.instance.mianActorId].ContainsKey(150002))
                {
                    int[] bookPages = (!DateFile.instance.gongFaBookPages.ContainsKey(150002))
                        ? new int[10]
                        : DateFile.instance.gongFaBookPages[150002];
                    for (int i = 0; i < 10; i++)
                    {
                        if (bookPages[i] == 0)
                        {
                            return;
                        }
                    }

                    if (DateFile.instance.actorGongFas[DateFile.instance.mianActorId][150002][0] == 100)
                    {
                        DateFile.instance.ChangeActorGongFa(DateFile.instance.mianActorId, 150370, 0, 1, 0, true);
                    }
                }

            }
            else
            {
                var actorId = DateFile.instance.mianActorId;
                if (Main.setting.addFeature)
                {
                    int grow = DateFile.instance.actorGongFas[actorId][150370][0] / 25;
                    if (DateFile.instance.actorGongFas[actorId][150370][1] == 10)
                    {
                        Main.Logger.Log("开始加特质");
                        Dictionary<int, string> actor, array = new Dictionary<int, string>();
                        //Main.Logger.Log("获取特性中");

                        foreach (KeyValuePair<int, Dictionary<int, string>> item in DateFile.instance.actorFeaturesDate)
                        {
                            //Main.Logger.Log(item.Key.ToString());
                            if (item.Value[4] == "3") array[item.Key] = "";
                            // Main.Logger.Log(string.Join(",", item.Value.Select(kv => kv.Key + "=" + kv.Value).ToArray()));

                        }


                        if (DateFile.instance.actorsDate.TryGetValue(actorId, out actor))
                        {
                            string[] features = actor[101].Split('|');
                            foreach (string feature in features)
                            {
                                int key = int.Parse(feature.Trim());
                                if (array.ContainsKey(key)) array.Remove(key);
                            }
                        }

                        if (array.Count > 0)
                        {
                            DateFile.instance.actorGongFas[actorId][150370][1] = 0;
                            // 增加特性
                            List<int> keyList = new List<int>(array.Keys);
                            int index = Random.Range(0, keyList.Count - 1);
                            actor[101] += "|" + keyList[index];

                            // 刷新特性缓存，要不然游戏不生效
                            DateFile.instance.actorsFeatureCache.Remove(DateFile.instance.mianActorId);

                            if (DateFile.instance.actorGongFas[actorId][150370][0] >
                                actor[101].Split('|').Length)
                            {
                                DateFile.instance.actorGongFas[actorId][150370][0] -=
                                    actor[101].Split('|').Length;
                            }
                            else
                            {
                                DateFile.instance.actorGongFas[actorId][150370][0] = 0;
                            }
                        }
                        else
                        {
                            if (DateFile.instance.actorGongFas[actorId][150370][0] == 100)
                            {

                                int num = Random.Range(0, 22);

                                if (num < 6)
                                {
                                    DateFile.instance.actorsDate[actorId][61 + num] =
                                        (int.Parse(DateFile.instance.actorsDate[actorId][61 + num]) + 1).ToString();
                                }
                                else
                                {
                                    DateFile.instance.actorsDate[actorId][501 + num - 6] =
                                        (int.Parse(DateFile.instance.actorsDate[actorId][501 + num - 6]) + 1)
                                        .ToString();
                                }
                            }

                        }


                       
                    }
                    else
                    {
                        DateFile.instance.actorGongFas[actorId][150370][1] += grow;
                        if (DateFile.instance.actorGongFas[actorId][150370][1] > 10)
                        {
                            DateFile.instance.actorGongFas[actorId][150370][1] = 10;
                        }

                    }

                if (DateFile.instance.actorGongFas[actorId][150370][0] == 100)
                {

                    int num = Random.Range(0, 20);

                    if (num < 6)
                    {
                        DateFile.instance.actorsDate[actorId][61 + num] =
                            (int.Parse(DateFile.instance.actorsDate[actorId][61 + num]) + 1).ToString();
                    }
                    else
                    {
                        int p = int.Parse(DateFile.instance.actorsDate[actorId][601 + num - 6]);
                        int n = 0;
                        if (p >= 100)
                        {
                            n = p + 1;
                        }
                        else
                        {
                            n = p + (int) System.Math.Log(119 - p);
                        }

                        DateFile.instance.actorsDate[actorId][601 + num - 6] = n.ToString();
                    }
                }
                }
                else
                {
                    int grow = Main.setting.changeAttrSpeed;
                    int endnum = DateFile.instance.actorGongFas[actorId][150370][0] / (100 / grow);

                    int num = Random.Range(0, 36);

                    int attrkey = 61 ;
                    if (num < 6)
                    {
                       attrkey = 61 + num;
                    }
                    else if (num < 20)
                    {
                       attrkey = 601 + num - 6;
                    }
                    else
                    {
                       attrkey = 501 + num - 20;
                    }

                    var tar = DateFile.instance.actorsDate[actorId][attrkey];
                    Main.Logger.Log("成长前属性值为"+ tar);
                    int p = int.Parse(tar);
                    int n = p + endnum;
                    tar = n.ToString();
                    DateFile.instance.actorsDate[actorId][attrkey] = tar;
                    Main.Logger.Log("成长后属性值为" + tar);

                    if (DateFile.instance.actorGongFas[actorId][150370][0] == 100 && DateFile.instance.actorGongFas[actorId][150370][1] < 10)
                    {
                        DateFile.instance.actorGongFas[actorId][150370][1]++;
                        if (Main.setting.expCost)
                        {
                            DateFile.instance.actorGongFas[actorId][150370][0] = 0;
                        }
                    }

                    if (p > DateFile.instance.actorGongFas[actorId][150370][0])
                    {
                        if (DateFile.instance.actorGongFas[actorId][150370][0] > endnum)
                        {
                          DateFile.instance.actorGongFas[actorId][150370][0] -= endnum;
                        }
                        else
                        {
                          DateFile.instance.actorGongFas[actorId][150370][0] = 0;
                        }
                    }
                   
                }
            }
        }
    }




}


