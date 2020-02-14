using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using System.Text.RegularExpressions;
using System.IO;

namespace GuiltyNature
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        public bool needNoImpression = true;
        public bool ququEnabled = true;
        public bool rapedEnabled = true;
    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static readonly char[] Separator = { '|' };
        public static string resBasePath;
        public static string folderName = "Texture";

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            resBasePath = modEntry.Path;
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
            GUILayout.Label("<color=#F28234FF>本mod要求太吾版本1.7.5及以上</color>");
            GUILayout.Label("<color=#F28234FF>注意：不要在已积有被欺侮事件的情况下卸载本mod</color>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("功能：相虫失败时可以选择将对方变成蛐蛐", new GUILayoutOption[0]);
            settings.needNoImpression = GUILayout.Toggle(settings.needNoImpression, "无需印象", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            bool flag = GUILayout.Button("切换", GUILayout.Width(60));
            if (flag) QuquEvent.Switch();
            GUILayout.Label("将【取其性命】改为【化为促织】：" + (settings.ququEnabled ? "开启" : "关闭"));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            bool flag2 = GUILayout.Button("切换", GUILayout.Width(60));
            if (flag2) RapedEvent.Switch();
            GUILayout.Label("允许触发欺侮太吾事件&提高欺侮触发率：" + (settings.rapedEnabled ? "开启" : "关闭"));
            GUILayout.EndHorizontal();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        public static bool GetModDate(out Dictionary<string, string> myModDict, string suffix = "")
        {
            bool exist = true;
            var modDict = DateFile.instance.modDate;
            string key = ModDate.name + suffix;
            if (!modDict.ContainsKey(key))
            {
                modDict.Add(key, new Dictionary<string, string> { });
                exist = false;
            }
            myModDict = modDict[key];
            return exist;
        }
        
    }

    public static class ModDate
    {
        public static string name = "GuiltyNature";
        public static string ququ = "ququ";
        public static string actor = "actor";
        public static string itemNamePlus = "itemName+";
    }
    public static class QuquEvent
    {
        public const int actorLifeKey = 98701;
        public static int eventId = 957400009;
        public static void Switch()
        {
            Main.settings.ququEnabled = !Main.settings.ququEnabled;
            if (DateFile.instance == null) return;
            ChangeKill(Main.settings.ququEnabled);
        }
        static void ChangeKill(bool change)
        {
            var rows = DateFile.instance.eventDate;
            if (change)
            {
                foreach (int i in new int[] { 917800001, 12400001, 17700001 })
                {
                    rows[i][3] = "（将其化为促织……）";
                    rows[i][8] = $"END&{eventId}";
                }//袭击获胜，被袭击获胜，失心人救治失败
                foreach (int i in new int[] { 25200005 })
                {
                    rows[i][3] = "（将其化为促织……）";
                    rows[i][7] = "-1";
                    rows[i][8] = "GN&4|" + $"END&{eventId}";
                }//失心人救治唯我选项
            }
            else
            {
                foreach (int i in new int[] { 917800001, 12400001 })
                {
                    rows[i][3] = "（取其性命！）";
                }
                rows[917800001][8] = "END&91781&1";
                rows[12400001][8] = "END&1241&1";
                rows[17700001][3] = "（放弃救治，取其性命！）";
                rows[17700001][8] = "END&1241&1";
                foreach (int i in new int[] { 25200005 })
                {
                    rows[i][3] = "（为其了断……）";
                    rows[i][7] = "257";
                    rows[i][8] = "GN&4|END&2521&4";
                }//失心人救治唯我选项
            }
        }
        public static void Add()
        {
            var rows = DateFile.instance.eventDate;
            int newRowId = QuquEvent.eventId;
            while (rows.ContainsKey(newRowId)) newRowId++;
            QuquEvent.eventId = newRowId;
            Dictionary<int, string> interpolatedData = new Dictionary<int, string>
            {
                {0,"相虫·相人" },
                {1,"" },
                {2,"-1" },
                {3,"我看你倒是神采非凡，不如…？ <color=#8E8E8EFF>（掏出剑柄……）</color>" },
                {4,"1" },
                {5,"" },
                {6,"" },
                {7,"-1" },
                {8,$"END&{newRowId}" },
                {9,"0" },
                {10,"" },
                {11,"0" }
            };//相人选项
            rows.Add(newRowId, interpolatedData);
            foreach (int i in new int[] { 9574, 9575, 9576 })
            {
                rows[i][5] = $"{newRowId}|" + rows[i][5];
            }//三种相虫失败
            if (Main.settings.ququEnabled) ChangeKill(true);
            Main.Logger.Log($"新增相虫事件，id为{newRowId}");
        }
        public static void EndEvent(int actorId)
        {
            //蛐蛐
            int grade = 10 - int.Parse(DateFile.instance.GetActorDate(actorId, 20));//根据人物品级决定蛐蛐品级
            int quality = DateFile.instance.GetActorValue(actorId, 516, false);//杂学面板资质，决定蛐蛐稀有度
            int quality2 = DateFile.instance.GetActorValue(actorId, 601, false);//内功面板资质，决定蛐蛐另一半的品级
            bool isMale = DateFile.instance.GetActorDate(actorId, 14) == "1";//根据性别决定[颜色和部位哪个品级更高]
            int colorId = 0;
            int partId = 0;
            const int colorLimit = 6;
            const int partLimit = 7;
            List<int> allQuqu = new List<int>(DateFile.instance.cricketDate.Keys);
            //促织王
            if (grade >= 8)
            {
                Dictionary<int, List<int>> colorPools = new Dictionary<int, List<int>> { };
                for (int i = 0; i < allQuqu.Count; i++)
                {
                    int ququId = allQuqu[i];
                    bool match = int.Parse(DateFile.instance.cricketDate[ququId][4]) == 0 &&
                        int.Parse(DateFile.instance.cricketDate[ququId][1]) == grade;//一二品的颜色
                    if (match)//根据稀有度分配奖池
                    {
                        int rarity = int.Parse(DateFile.instance.cricketDate[ququId][6]);
                        if (ququId == 20) rarity = 5;//三太子和梅花翅一档
                        else if (ququId == 18 || ququId == 19) rarity = 1;//八败、三段棉、天蓝青一档
                        ManageToAdd(colorPools, rarity, ququId);
                    }
                }
                colorId = IdInPools(colorPools, quality, actorId);
            }
            //普通蛐蛐
            else
            {
                Dictionary<int, List<int>> colorPools = new Dictionary<int, List<int>> { };
                Dictionary<int, List<int>> partPools = new Dictionary<int, List<int>> { };
                bool colorBetter = (grade == 7 || isMale) ? false : true;
                int grade2 = IndexForValue(quality2, Math.Min(grade, colorBetter ? partLimit : colorLimit)) + 1;
                for (int i = 0; i < allQuqu.Count; i++)
                {
                    int ququId = allQuqu[i];
                    if (ququId == 0) continue;//排除呆物
                    bool isColor = int.Parse(DateFile.instance.cricketDate[ququId][4]) == 0;
                    bool isBetter = isColor == colorBetter;
                    int grade0 = isBetter ? grade : grade2;
                    var pools0 = isColor ? colorPools : partPools;
                    bool match = int.Parse(DateFile.instance.cricketDate[ququId][1]) == grade0;
                    if (match)
                    {
                        int rarity = int.Parse(DateFile.instance.cricketDate[ququId][6]);
                        ManageToAdd(pools0, rarity, ququId);
                    }
                }
                colorId = IdInPools(colorPools, quality, actorId);
                partId = IdInPools(partPools, quality, actorId);
            }
            //生成物品
            int itemId = DateFile.instance.MakeNewItem(10000, 0, 0, 50, 20);
            var itemDate = DateFile.instance.itemsDate[itemId];
            GetQuquWindow.instance.MakeQuqu(itemId, colorId, partId);
            //名字信息
            string ququName = DateFile.instance.GetItemDate(itemId, 0, false);
            string actorName = DateFile.instance.GetActorName(actorId);
            int gang = int.Parse(DateFile.instance.GetActorDate(actorId, 19, false));
            string gangName = DateFile.instance.GetGangDate(gang, 0);
            int status = int.Parse(DateFile.instance.GetActorDate(actorId, 20, false));
            string statusName = DateFile.instance.presetGangGroupDateValue[DateFile.instance.GetGangValueId(gang, status)][1001];
            statusName = DateFile.instance.SetColoer(20001 + grade, statusName, false);
            string age = DateFile.instance.GetActorDate(actorId, 11, true);
            itemDate[2007] = age;
            string life = DateFile.instance.GetActorDate(actorId, 13, true);
            string[] ququInfo =
            {
                gangName + statusName + " " + actorName,
                life,
                actorId.ToString(),
            };
            Main.GetModDate(out var ququModDate, ModDate.ququ);
            ququModDate.Add(itemId.ToString(), string.Join(Main.Separator[0].ToString(), ququInfo));
            //itemDate[99] = Main.ModName + Main.Separator[0] + gangName + statusName + " " + actorName + Main.Separator[0] + life;
            Main.Logger.Log($"{actorName}>>>{ququName}");
            //给予物品
            int mainId = DateFile.instance.mianActorId;
            DateFile.instance.GetItem(mainId, itemId, 1, false, 0, 0);
            //人
            DateFile.instance.actorsDate[actorId][12] = "0";
            DateFile.instance.RemoveActor(new List<int> { actorId }, true, false);
            DateFile.instance.MoveOutPlace(actorId);
            string[] actorInfo =
            {
                colorId.ToString(),
                partId.ToString(),
                itemId.ToString(),
            };
            Main.GetModDate(out var actorModDate, ModDate.actor);
            actorModDate.Add(actorId.ToString(), string.Join(Main.Separator[0].ToString(), actorInfo));
            /*var actorLife = DateFile.instance.actorLife[actorId];
            var key1 = Main.ActorLifeKey;
            if (actorLife.ContainsKey(key1))
            {
                string log = "";
                foreach (int i in actorLife[key1]) log += $"{i} ";
                Main.Logger.Log($"{actorName} 已被记录了 {log}");
                //actorLife[key1].Add(itemId);
            }
            else
            {
                actorLife.Add(key1, new List<int> { colorId, partId });
            }*/
            UIDate.instance.UpdateManpower();
            WorldMapSystem.instance.UpdatePlaceActor(WorldMapSystem.instance.choosePartId, WorldMapSystem.instance.choosePlaceId);
        }
        public static int IndexForValue(int value, int size, int upLimit = 120)
        {
            int div = upLimit * 100 / size;
            int index = Mathf.Clamp(value * 100 / div, 0, size - 1);
            return index;
        }
        public static void ManageToAdd(Dictionary<int, List<int>> dict, int key, int item)
        {
            if (dict.ContainsKey(key))
            {
                dict[key].Add(item);
            }
            else
            {
                dict.Add(key, new List<int> { item });
            }
        }
        public static int IdInPools(Dictionary<int, List<int>> pools, int value, int actorId)
        {
            List<int> keys = pools.Keys.ToList();
            keys.Sort((a, b) => (a > b ? -1 : 1));
            int poolKey = keys[IndexForValue(value, keys.Count)];
            List<int> pool = pools[poolKey];
            //匹配七元
            var ququsDate = DateFile.instance.cricketDate;
            int[] actorQiYuan = ActorMenu.instance.GetActorResources(actorId);
            string log = "人物七元";
            foreach (int i in actorQiYuan) log += " " + i.ToString();
            //Main.Logger.Log(log);
            Dictionary<int, float> similarity = new Dictionary<int, float> { };
            float max = 0;
            int best = 0;
            foreach (int ququId in pool)
            {
                int[] ququQiYuan = new int[7];
                var ququDate = ququsDate[ququId];
                log = ququDate[0] + ":";
                for (int i = 0; i < 7; i++)
                {
                    ququQiYuan[i] = int.Parse(ququDate[52001 + i]);
                    log += ' ' + ququDate[52001 + i];
                }
                similarity[ququId] = GetSimilarity(actorQiYuan, ququQiYuan);
                //Main.Logger.Log(log);
                //Main.Logger.Log($"similarity:{similarity[ququId]}");
                if (similarity[ququId] > max)
                {
                    max = similarity[ququId];
                    best = ququId;
                }
            }
            int result = best;
            return result;
        }
        public static float GetSimilarity(int[] actor, int[] ququ)
        {
            float sum1 = actor.Sum();
            float bonus1 = sum1 / 30f;
            sum1 += 7 * bonus1;
            float[] array1 = actor.Select(i => (i + bonus1) / sum1).ToArray();
            float sum2 = ququ.Sum();
            float bonus2 = sum2 / 30f;
            sum2 += 7 * bonus2;
            float[] array2 = ququ.Select(i => (i + bonus2) / sum2).ToArray();
            float similarity = 1;
            for (int i = 0; i < 7; i++)
            {
                float n1 = array1[i];
                float n2 = array2[i];
                similarity *= n1 > n2 ? n2 / n1 : n1 / n2;
            }
            return similarity;
        }
        
    }
    public static class RapedEvent
    {
        public static class ID
        {
            public static int menu0 = 999;
            public static int option1 = 99900001;
            public static int option2 = 99900002;
            public static int menu1 = 998;
            public static int menu2 = 997;
            public static int option21 = 99700001;
            public static int option22 = 99700002;
            public static int enemyTeam = 170;
        }
        public static bool isAdded = false;
        public static Sprite sprite;
        public static string imageName = "EventBack_GuiltyNature.png";
        public static void Switch()
        {
            Main.settings.rapedEnabled = !Main.settings.rapedEnabled;
            if (DateFile.instance == null) return;
            ChangeProbability(Main.settings.rapedEnabled);
        }
        static void ChangeProbability(bool add = true)
        {
            var goodnessDate = DateFile.instance.goodnessDate;
            if (add)
            {
                DateFile.instance.goodnessDate[0][25] = "25";
                DateFile.instance.goodnessDate[3][25] = "75";
                DateFile.instance.goodnessDate[4][25] = "50";
            }
            else
            {
                DateFile.instance.goodnessDate[0][25] = "1";
                DateFile.instance.goodnessDate[3][25] = "3";
                DateFile.instance.goodnessDate[4][25] = "2";
            }
        }
        public static void DoRape(int raperId, int victimId, bool reverse = false)
        {
            int raperGender = int.Parse(DateFile.instance.GetActorDate(raperId, 14));
            int raperGoodness = DateFile.instance.GetActorGoodness(raperId);
            int raperMoodChange = int.Parse(DateFile.instance.goodnessDate[raperGoodness][102]) * 10;
            int victimGender = int.Parse(DateFile.instance.GetActorDate(victimId, 14));

            DateFile.instance.SetActorMood(raperId, raperMoodChange);
            bool love = DateFile.instance.GetActorSocial(victimId, 312, false, false).Contains(raperId)//倾心爱慕
                || DateFile.instance.GetActorSocial(victimId, 306, false, false).Contains(raperId)//两情相悦
                || DateFile.instance.GetActorSocial(victimId, 309, false, false).Contains(raperId);//结发夫妻
            if (love)
            {
                DateFile.instance.SetActorMood(victimId, UnityEngine.Random.Range(-10, 11));
                bool hate = UnityEngine.Random.Range(0, 100) < 50;
                if (hate)
                {
                    DateFile.instance.AddSocial(victimId, raperId, 402);
                }
                PeopleLifeAI.instance.AISetMassage(97, victimId, DateFile.instance.mianPartId, DateFile.instance.mianPlaceId, new int[1], raperId, true);
            }
            else
            {
                //Main.Logger.Log("心中悲痛欲绝，只觉得再无面目活于世上");
                DateFile.instance.SetActorMood(victimId, -50);
                DateFile.instance.AddSocial(victimId, raperId, 402);
                PeopleLifeAI.instance.AISetMassage(96, victimId, DateFile.instance.mianPartId, DateFile.instance.mianPlaceId, new int[1], raperId, true);
            }
            DateFile.instance.ChangeActorFeature(raperId, 4001, 4002);
            DateFile.instance.ChangeActorFeature(victimId, 4001, 4002);
            bool sameGender = raperGender == victimGender;
            if (!sameGender)
            {
                bool preg = PeopleLifeAI.instance.AISetChildren((raperGender == 1) ? raperId : victimId, (raperGender == 1) ? victimId : raperId,
                    (raperGender == 1) ? 0 : 1, (raperGender == 1) ? 1 : 0);
                //if (preg) Main.Logger.Log("身怀六甲");
            }
        }
        public static void Spay(int actorId)
        {
            bool isMale = int.Parse(DateFile.instance.GetActorDate(actorId, 14)) == 1;
            string actorName = DateFile.instance.GetActorName(actorId);
            int itemId = isMale ? 3413 : 3427;
            int featureId = isMale ? 1001 : 1002;
            int mainId = DateFile.instance.mianActorId;
            DateFile.instance.AddActorFeature(actorId, featureId);
            DateFile.instance.GetItem(DateFile.instance.mianActorId, itemId, 1, true, 0);
        }
        public static void Add()
        {
            var eventDate = DateFile.instance.eventDate;
            while (eventDate.ContainsKey(ID.menu0)) ID.menu0--;
            ID.option1 = ID.menu0 * 100000 + 1;
            while (eventDate.ContainsKey(ID.option1)) ID.option1++;
            ID.option2 = ID.option1 + 1;
            while (eventDate.ContainsKey(ID.option2)) ID.option2++;
            ID.menu1 = ID.menu0 - 1;
            while (eventDate.ContainsKey(ID.menu1)) ID.menu1--;
            Dictionary<int, string> menu0_Date = new Dictionary<int, string>
            {
                {0,"遭遇欺侮" },
                {1,"201" },
                {2,"0" },
                {3,"当MN行至偏僻无人之处时，D0忽然面色不善的拦住了MN的去路……\n在三言两语的交谈之后，D0终于言明：已无法抑制对MN的爱欲……" },
                {4,"1" },
                {5,$"{ID.option1}|{ID.option2}" },
                {6,"" },
                {7,"" },
                {8,"" },
                {9,"0" },
                {10,"" },
                {11,"0" }
            };//菜单0
            eventDate.Add(ID.menu0, menu0_Date);
            Dictionary<int, string> option1_Date = new Dictionary<int, string>
            {
                {0,"" },
                {1,"" },
                {2,"-1" },
                {3,"（任其宣泄……）" },
                {4,"1" },
                {5,"" },
                {6,"" },
                {7,$"{ID.menu1}" },
                {8,$"END&{ID.option1}" },
                {9,"0" },
                {10,"" },
                {11,"0" }
            };//选项1
            eventDate.Add(ID.option1, option1_Date);
            Dictionary<int, string> option2_Date = new Dictionary<int, string>
            {
                {0,"" },
                {1,"" },
                {2,"-1" },
                {3,"（恕难从命……）" },
                {4,"1" },
                {5,"" },
                {6,"" },
                {7,"-1" },
                {8,"BAT&102&0" },
                {9,"0" },
                {10,"" },
                {11,"0" }
            };//选项2
            eventDate.Add(ID.option2, option2_Date);
            Dictionary<int, string> menu1_Date = new Dictionary<int, string>
            {
                {0,"被欺侮" },
                {1,"201" },
                {2,"-99" },
                {3,"D0以卑劣手段欺侮了MN……" },
                {4,"1" },
                {5,"20300001|20300002|20300003|20300004|20300005" },
                {6,"" },
                {7,"" },
                {8,"" },
                {9,"0" },
                {10,"" },
                {11,"0" }
            };//菜单1
            eventDate.Add(ID.menu1, menu1_Date);
            isAdded = true;
            Main.Logger.Log($"新增被欺侮事件，id为{ID.menu0}、{ID.option1}、{ID.option2}、{ID.menu1}");
            if (Main.settings.rapedEnabled) ChangeProbability();

            ID.menu2 = ID.menu1 - 1;
            while (eventDate.ContainsKey(ID.menu2)) ID.menu2--;
            ID.option21 = ID.menu2 * 100000 + 1;
            while (eventDate.ContainsKey(ID.option21)) ID.option21++;
            /*ID.option22 = ID.option21 + 1;
            while (eventDate.ContainsKey(ID.option22)) ID.option22++;*/
            Dictionary<int, string> menu2_Date = new Dictionary<int, string>(eventDate[124]);//菜单2
            menu2_Date[0] = "反抗欺侮获胜";
            menu2_Date[5] = $"{ID.option21}|12400002";
            eventDate.Add(ID.menu2, menu2_Date);
            Dictionary<int, string> option21_Date = new Dictionary<int, string>(eventDate[900700001]);
            option21_Date[0] = "一劳永逸";
            option21_Date[3] = "（没收工具......）";
            option21_Date[8] = $"END&{ID.option21}";
            eventDate.Add(ID.option21, option21_Date);

            var enemyTeamDate = DateFile.instance.enemyTeamDate;
            while (enemyTeamDate.ContainsKey(ID.enemyTeam)) ID.enemyTeam++;
            Dictionary<int, string> newRow = new Dictionary<int, string>(enemyTeamDate[102]);
            newRow[23] = "75";
            newRow[98] = "1";
            newRow[101] = $"{ID.menu2}&1";
            newRow[102] = $"{ID.menu1}&1";
            enemyTeamDate.Add(ID.enemyTeam, newRow);
            Main.Logger.Log($"新增被欺侮战斗，id为{ID.enemyTeam}");
            eventDate[ID.option2][8] = $"BAT&{ID.enemyTeam}&0";

        }
    }

    //Event条件
    [HarmonyPatch(typeof(MassageWindow), "GetEventIF")]
    public static class GuiltyNature_GetEventIF_Patch
    {
        static bool Prefix(MassageWindow __instance, ref bool __result, ref int actorId, ref int eventActor, ref int eventId)
        {
            if (!Main.enabled || !Main.settings.needNoImpression) return true;
            string require = DateFile.instance.eventDate[eventId][6];
            bool flag = require == "" || require == "0";
            if (flag) return true;
            string[] array = DateFile.instance.eventDate[eventId][6].Split(new char[]
            {
                '|'
            });
            bool[] array2 = new bool[array.Length];
            bool flag_or;
            bool flag_and = true;
            bool isLIFEF = false;
            for (int i = 0; i < array.Length; i++)
            {
                string[] array3 = array[i].Split(new char[]
                {
                    '#'
                });
                flag_or = false;
                for (int j = 0; j < array3.Length; j++)
                {
                    string[] array5 = array3[j].Split(new char[]
                    {
                        '&'
                    });
                    string text = array5[0];
                    if (text == "LIFEF")
                    {
                        if (DateFile.instance.GetLifeDate(eventActor, 1001, 0) == int.Parse(array5[1]))
                            flag_or = true;
                        isLIFEF = true;
                    }
                    else if (text == "NOF")
                    {
                        if (!DateFile.instance.GetFamily(true, true).Contains(eventActor))
                            flag_or = true;
                    }
                    else flag_or = true;
                }
                if (!flag_or)
                {
                    flag_and = false;
                    break;
                }
            }
            if (isLIFEF && flag_and)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
    //加载图片
    [HarmonyPatch(typeof(ui_Loading), "LoadScene")]
    [HarmonyPriority(Priority.Last)]
    public static class Loading_LoadScene_DynamicallyLoadResources
    {
        static void Postfix()
        {
            if (RapedEvent.sprite != null)
            {
                Main.Logger.Log("图片已加载");
                return;
            }
            string path = Path.Combine(Path.Combine(Main.resBasePath, Main.folderName), RapedEvent.imageName);
            if (!File.Exists(path))
            {
                Main.Logger.Log("图片路径不存在：" + path);
                return;
            }
            var fileData = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 100);
            RapedEvent.sprite = sprite;
            Main.Logger.Log($"成功加载图片：{path}\n大小为{texture.width}*{texture.height}");
        }
    }
    //加载eventDate
    [HarmonyPatch(typeof(ui_Loading), "LoadBaseDate")]
    [HarmonyPriority(Priority.Last)]
    public static class Loading_LoadBaseDate_Patch
    {
        static void Postfix()
        {
            RapedEvent.isAdded = false;
            //if (!Main.enabled) return;
            QuquEvent.Add();
            RapedEvent.Add();

        }
    }
    //处理Event
    [HarmonyPatch(typeof(MassageWindow), "EndEvent")]
    public static class GuiltyNature_EndEvent_Patch
    {
        static bool Prefix(MassageWindow __instance)
        {
            if (__instance.eventValue.Count > 0 && __instance.eventValue[0] != 0)
            {
                int num = __instance.eventValue[0];
                if (num == QuquEvent.eventId)
                {
                    Main.Logger.Log("相虫事件跳转");
                    QuquEvent.EndEvent(__instance.mianEventDate[1]);
                }
                else if (num == RapedEvent.ID.option1)
                {
                    Main.Logger.Log("被欺侮事件跳转");
                    RapedEvent.DoRape(__instance.mianEventDate[1], DateFile.instance.mianActorId);
                }
                else if (num == RapedEvent.ID.option21)
                {
                    Main.Logger.Log("没收工具事件跳转");
                    RapedEvent.Spay(__instance.mianEventDate[1]);
                }
                else return true;
                __instance.eventValue = new List<int>();
                return false;
            }
            return true;
        }

    }
    //物品名称后缀
    [HarmonyPatch(typeof(DateFile), "GetItemDate")]
    public static class GuiltyNature_GetItemDate_Patch
    {
        static void Postfix(DateFile __instance, ref string __result, ref int id, ref int index, ref bool otherMassage)
        {
            if (index != 0 || !otherMassage) return;
            if (__result.Length < 2) return;
            if (!DateFile.instance.itemsDate.TryGetValue(id, out var dict)) return;
            int presetId = int.Parse(dict[999]);
            if (!DateFile.instance.presetitemDate.TryGetValue(presetId, out var dict2)) return;
            if (dict2[2001] != "1") return;
            //获取数据
            if (Main.GetModDate(out var modDate, ModDate.ququ) && modDate.ContainsKey(id.ToString()))
            {
                var actorInfo = modDate[id.ToString()].Split(Main.Separator);
                __result += "\n" + actorInfo[0];
            }
            else
            {
                if (!dict.ContainsKey(99)) return;
                var actorInfo = dict[99].Split(Main.Separator);
                if (actorInfo.Length < 3) return;
                if (actorInfo[0] != ModDate.name) return;
                __result += "\n" + actorInfo[1];
            }
        }
    }
    //蛐蛐寿命
    [HarmonyPatch(typeof(GetQuquWindow), "GetQuquDate")]
    public static class GuiltyNature_GetQuquDate_Patch
    {
        static bool Prefix(GetQuquWindow __instance, ref int __result, ref int itemId, ref int index, ref bool injurys)
        {
            if (index != 98) return true;
            bool flag = DateFile.instance.itemsDate.TryGetValue(itemId, out var dict);
            if (!flag) return true;
            //获取数据
            if (Main.GetModDate(out var modDate, ModDate.ququ) && modDate.ContainsKey(itemId.ToString()))
            {
                var actorInfo = modDate[itemId.ToString()].Split(Main.Separator);
                __result = int.Parse(actorInfo[1]);
            }
            else
            {
                if (!dict.ContainsKey(99)) return true;
                var actorInfoPair = dict[99].Split(Main.Separator);
                if (actorInfoPair.Length < 3) return true;
                if (actorInfoPair[0] != ModDate.name) return true;
                __result = int.Parse(actorInfoPair[2]);
            }
            return false;
        }
    }
    //化虫者头像显示为蛐蛐
    [HarmonyPatch(typeof(ActorFace), "UpdateFace")]
    public static class GuiltyNature_UpdateFace_Patch
    {
        static bool Prefix(ActorFace __instance, ref int actorId, ref int[] faceDate)
        {
            int colorId;
            int partId;
            if (Main.GetModDate(out var modDate, ModDate.ququ) && modDate.ContainsKey(actorId.ToString()))
            {
                var info = modDate[actorId.ToString()].Split(Main.Separator);
                colorId = int.Parse(info[0]);
                partId = int.Parse(info[1]);
            }
            else
            {
                if (!DateFile.instance.actorLife.TryGetValue(actorId, out var actorLife)) return true;
                int key = QuquEvent.actorLifeKey;
                if (!actorLife.ContainsKey(key)) return true;
                colorId = actorLife[key][0];
                partId = actorLife[key][1];
            }
            __instance.ageImage.gameObject.SetActive(false);
            __instance.nose.gameObject.SetActive(false);
            __instance.faceOther.gameObject.SetActive(false);
            __instance.eye.gameObject.SetActive(false);
            __instance.eyePupil.gameObject.SetActive(false);
            __instance.eyebrows.gameObject.SetActive(false);
            __instance.mouth.gameObject.SetActive(false);
            __instance.beard.gameObject.SetActive(false);
            __instance.hair1.gameObject.SetActive(false);
            __instance.hair2.gameObject.SetActive(false);
            __instance.hairOther.gameObject.SetActive(false);
            __instance.clothes.gameObject.SetActive(false);
            __instance.clothesColor.gameObject.SetActive(false);
            __instance.body.gameObject.SetActive(true);
            var ququDate = DateFile.instance.cricketDate;
            int key1 = partId > 0 ? int.Parse(ququDate[colorId][96]) : 36;
            int key2 = int.Parse(ququDate[partId > 0 ? partId : colorId][96]);
            __instance.body.sprite = GetSprites.instance.cricketImage[key1][key2];
            __instance.body.color = new Color(1f, 1f, 1f, 1f);

            return false;
        }
    }
    //被欺侮，将必定失败的换季事件变为回合开始事件
    [HarmonyPatch(typeof(PeopleLifeAI), "DoTrunAIChange")]
    public static class GuiltyNature_DoTrunAIChange_Patch
    {
        static void Postfix(PeopleLifeAI __instance, ref int actorId)
        {
            if (!Main.enabled || !Main.settings.rapedEnabled || !RapedEvent.isAdded) return;
            int length = __instance.aiTrunEvents.Count;
            if (length <= 0) return;
            int[] aiTurnEvent = __instance.aiTrunEvents[length - 1];
            if (aiTurnEvent[0] != 233) return;
            int actorId2 = aiTurnEvent[3];
            if (actorId != actorId2) return;
            var method = typeof(PeopleLifeAI).GetMethod("AISetEvent",
                        BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(__instance, new object[] { 0, new int[]
            {
                0,
                actorId,
                RapedEvent.ID.menu0,
                actorId
            } });
            __instance.aiTrunEvents.RemoveAt(length - 1);
            string actorName = DateFile.instance.GetActorName(actorId);
            Main.Logger.Log($"添加雷普事件，发起者为：{actorName}");
        }
    }
    //修改Event图片
    [HarmonyPatch(typeof(MassageWindow), "SetMassageWindow")]
    public static class GuiltyNature_SetMassageWindow_Patch
    {
        static void Postfix(MassageWindow __instance)
        {
            if (!Main.enabled || !Main.settings.rapedEnabled || !RapedEvent.isAdded) return;
            int eventId = __instance.mianEventDate[2];
            bool isMenu0 = eventId == RapedEvent.ID.menu0;
            bool isMenu1 = eventId == RapedEvent.ID.menu1;
            if (isMenu0 || isMenu1)
            {
                __instance.eventBackImage.sprite = RapedEvent.sprite;
            }
        }
    }
}