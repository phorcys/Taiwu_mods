using Harmony12;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace Evolution
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool addFeature = true;
        public bool addAttr = true;
        public bool expCost = false;
        public int growCount = 0;
        public int changeAttrSpeed = 1;
        public bool[] teammateImprove = { false, false, false, false, false, };

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
            bool cond = (DateFile.instance == null || !GameData.Characters.HasChar(DateFile.instance.MianActorID()));
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
                setting.addFeature = GUILayout.Toggle(setting.addFeature, "易经");

                GUILayout.Label("说明： 取消勾选,将不再增加特质");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("Box");
                setting.addAttr = GUILayout.Toggle(setting.addAttr, "锻骨");

                GUILayout.Label("说明： 取消勾选,将不再增加属性");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("Box");
                setting.expCost = GUILayout.Toggle(setting.expCost, "锻骨之痛");

                GUILayout.Label("说明： 如果需要增加的资质大于熟练度，会消耗熟练度，取消勾选则不消耗");

                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("Box");
                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("属性实际增加数量为 高于标准值 增加低保数字， 低于标准值3点以上获得额外成长\n");
                GUILayout.EndHorizontal();
                GUILayout.Label("标准值; 熟练度 + 心法等级 * 2");
                GUILayout.BeginHorizontal("Box");

                GUILayout.Label("低保：");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("<", GUILayout.Width(30)) && Main.setting.changeAttrSpeed > 1)
                {
                    Main.setting.changeAttrSpeed--;
                }

                GUILayout.Label("  " + Main.setting.changeAttrSpeed + "  ");
                if (GUILayout.Button(">", GUILayout.Width(30)) && Main.setting.changeAttrSpeed < 10)
                {
                    Main.setting.changeAttrSpeed++;
                }

                GUILayout.EndHorizontal();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("消耗历练,强化队友。");
                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("队友：");
                for (int i = 1; i != 5; i++)
                {
                    if (DateFile.instance.acotrTeamDate[i] != -1)
                    {
                        setting.teammateImprove[i] = GUILayout.Toggle(setting.teammateImprove[i], DateFile.instance.GetActorName(DateFile.instance.acotrTeamDate[i]));
                    }
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("灌顶", GUILayout.Width(60)) && DateFile.instance.actorGongFas[DateFile.instance.mianActorId][150370][0] > 15)
                {
                    DateFile.instance.actorGongFas[DateFile.instance.mianActorId][150370][0] =
                        DateFile.instance.actorGongFas[DateFile.instance.mianActorId][150370][0] - 15;
                    for (int k = 1; k != 5; k++)
                    {

                        Main.Logger.Log("强化队友属性"+ k);
                        if (!setting.teammateImprove[k])
                        {
                            Main.Logger.Log("跳过强化");
                            continue;
                        }

                        var actor_id = DateFile.instance.acotrTeamDate[k];
                        for (int i = 0; i < 6; i++)
                        {
                            GameData.Characters.SetCharProperty(actor_id, 61 + i,
                                int.Parse(GameData.Characters.GetCharProperty(actor_id, 61 + i) + 1).ToString());

                        }

                        for (int i = 0; i < 16; i++)
                        {
                            GameData.Characters.SetCharProperty(actor_id, 501 + i,
                                int.Parse(GameData.Characters.GetCharProperty(actor_id, 501 + i) + 1).ToString());
                        }

                        for (int i = 0; i < 14; i++)
                        {
                            GameData.Characters.SetCharProperty(actor_id, 601 + i,
                                int.Parse(GameData.Characters.GetCharProperty(actor_id, 601 + i) + 1).ToString());
                        }
                    }

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

                            // DateFile.instance.SetActorEquipGongFa(key, true, true);
                            // 刷新功法 ？
                            DateFile.instance.SetMianActorEquipGongFa();
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
                    if (Main.setting.growCount > 10)
                    {
                        Main.setting.growCount = Main.setting.growCount - 10;
                        Main.Logger.Log("开始加特质");
                        Dictionary<int, string> array = new Dictionary<int, string>();
                        //Main.Logger.Log("获取特性中");

                        foreach (KeyValuePair<int, Dictionary<int, string>> item in DateFile.instance.actorFeaturesDate)
                        {
                            //Main.Logger.Log(item.Key.ToString());
                            if (item.Value[4] == "3") array[item.Key] = "";
                            // Main.Logger.Log(string.Join(",", item.Value.Select(kv => kv.Key + "=" + kv.Value).ToArray()));

                        }


                        if (GameData.Characters.HasChar(actorId))
                        {
                            var features = DateFile.instance.GetActorFeature(actorId);
                            foreach (int key in features)
                            {
                                if (array.ContainsKey(key)) array.Remove(key);
                            }
                        }

                        if (array.Count > 0)
                        {

                            // 增加特性
                            List<int> keyList = new List<int>(array.Keys);
                            int index = Random.Range(0, keyList.Count - 1);
                            GameData.Characters.SetCharProperty(actorId, 101,
                                GameData.Characters.GetCharProperty(actorId, 101) + "|" + keyList[index]);

                            // 刷新特性缓存，要不然游戏不生效
                            DateFile.instance.ActorFeaturesCacheReset(DateFile.instance.MianActorID());

                            if (DateFile.instance.actorGongFas[actorId][150370][0] >
                                DateFile.instance.GetActorFeature(actorId).Count)
                            {
                                DateFile.instance.actorGongFas[actorId][150370][0] -=
                                    DateFile.instance.GetActorFeature(actorId).Count;
                            }
                            else
                            {
                                DateFile.instance.actorGongFas[actorId][150370][0] = 0;
                            }
                        }
                        else
                        {
                            // 已经获得全部三级特性,自动关闭增加特质
                            Main.Logger.Log("已经获得全部三级特性，自动关闭易经");
                            Main.setting.addFeature = false;

                        }

                    }
                    else
                    {
                        Main.setting.growCount += grow;

                    }
                }

                if (Main.setting.addAttr)
                {
                    int min_grow = Main.setting.changeAttrSpeed;

                    int extra_grow = 0;


                    int num = Random.Range(0, 36);

                    int attrKey = 61;
                    if (num < 6)
                    {
                        attrKey = 61 + num;
                    }
                    else if (num < 20)
                    {
                        attrKey = 601 + num - 6;
                        min_grow++;
                    }
                    else
                    {
                        attrKey = 501 + num - 20;
                        min_grow++;
                    }

                    var tar = GameData.Characters.GetCharProperty(actorId, attrKey);
                    Main.Logger.Log("成长前属性值为" + tar);
                    int p = int.Parse(tar);

                    if (DateFile.instance.actorGongFas[actorId][150370][0] +
                        2 * DateFile.instance.actorGongFas[actorId][150370][1] > p)
                    {
                        int temp = (int)System.Math.Log(DateFile.instance.actorGongFas[actorId][150370][0] +
                                                         2 * DateFile.instance.actorGongFas[actorId][150370][1] - p);
                        if (temp > 0)
                        {
                            extra_grow = temp;
                            Main.Logger.Log("标准值大于需要增加的属性，计算得获得额外加成" + extra_grow);
                        }
                    }

                    int grow_num = min_grow + extra_grow;

                    if (Main.setting.expCost && DateFile.instance.actorGongFas[actorId][150370][0] > p)
                    {
                        DateFile.instance.actorGongFas[actorId][150370][0] =
                            DateFile.instance.actorGongFas[actorId][150370][0] - grow_num;
                    }

                    p = p + grow_num;
                    tar = p.ToString();
                    GameData.Characters.SetCharProperty( actorId, attrKey, tar);
                    Main.Logger.Log("成长后属性值为" + tar);
                }

                if (DateFile.instance.actorGongFas[DateFile.instance.mianActorId][150370][1] < 10)
                {
                    int exp = DateFile.instance.GetActorValue(DateFile.instance.mianActorId, 601, true) - int.Parse(DateFile.instance.GetActorDate(DateFile.instance.mianActorId, 601, true));
                    if (exp > DateFile.instance.actorGongFas[DateFile.instance.mianActorId][150370][1] * (DateFile.instance.actorGongFas[DateFile.instance.mianActorId][150370][1]) * 5)
                    {
                        DateFile.instance.actorGongFas[DateFile.instance.mianActorId][150370][1]++;
                    }
                }



            }
        }
    }




}


