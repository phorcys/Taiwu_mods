using System;
using UnityEngine;
using UnityModManagerNet;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GameData;
using Harmony12;
using Random = UnityEngine.Random;

namespace ExchangeStatPoints
{
    public static class Main
    {
        public static int 存档Id = 0;
        public static UnityModManager.ModEntry.ModLogger Logger;

        //{16,"人物立场"},
        ////0刚正，每200一段？
        //{202,"喜爱"},{203,"厌恶"},
        ////无|针匣|对刺|暗器|箫笛|掌套|短杵|拂尘|长鞭|剑|刀|长兵|瑶琴|宝物|冠饰|鞋子|护甲|衣着|代步|促织|图纸|书籍|工具|食材|木材|金铁|宝石|织物|毒物|药材|毒药|丹药|杂物|蛰罐|素食|荤食|神兵|酒|机关|毒器|令符|茶
        //{18,"名誉"},
        //{995,"捏脸部件"},{996,"捏脸颜色"},

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnGUI = OnGUI;
            return true;
        }

        public static void Init()
        {
            bool cond = (DateFile.instance == null || !Characters.HasChar(DateFile.instance.mianActorId));
            if (存档Id != SaveDateFile.instance.dateId || cond) //避免跨存档切换
            {
                自由属性点 = new int[] {0, 0, 0}; //分别为六维属性点、技艺属性点、武学属性点
                for (int i = 0; i < 自由属性点.Length; i++)
                {
                    自由属性点[i] = 0;
                }

                foreach (var item in 修改后的属性.ToList())
                {
                    修改后的属性[item.Key] = 0;
                }

                for (int i = 0; i < 修改后的性别.Length; i++)
                {
                    修改后的性别[i] = false;
                }

                changedMoney = 0;
                changedCharm = 0;

                changedPrestige = 0;
                for (int i = 0; i < changedHeirLoom.Length; i++)
                {
                    changedHeirLoom[i] = 0;
                }

                isRejuvenation = false;
                if (cond)
                {
                    存档Id = 0;
                }
                else
                {
                    存档Id = SaveDateFile.instance.dateId;
                }
            }
        }

        public static int[] 自由属性点 = {0, 0, 0};

        public static int ChangedPoint(int pointId)
        {
            if (pointId == 551 || pointId == 651) return 修改后的属性[pointId];
            if (修改后的属性[pointId] > 0) return 修改后的属性[pointId] / 2;
            return 修改后的属性[pointId];
        }

        public static Dictionary<int, int> 修改后的属性 = new Dictionary<int, int>
        {
            {61, 0}, /* "膂力"*/ {62, 0}, /* "体质"*/ {63, 0}, /* "灵敏"*/ {64, 0}, /* "根骨"*/ {65, 0}, /* "悟性"*/
            {66, 0}, /* "定力"*/

            {501, 0}, /*"音律"*/ {502, 0}, /*"弈棋"*/ {503, 0}, /*"诗书"*/ {504, 0}, /*"绘画"*/ {505, 0}, /*"术数"*/
            {506, 0}, /*"品鉴"*/ {507, 0}, /*"锻造"*/ {508, 0}, /*"制木"*/
            {509, 0}, /*"医术"*/ {510, 0}, /*"毒术"*/ {511, 0}, /*"织锦"*/ {512, 0}, /*"巧匠"*/ {513, 0}, /*"道法"*/
            {514, 0}, /*"佛学"*/ {515, 0}, /*"厨艺"*/ {516, 0}, /*"杂学"*/

            {601, 0}, /*"内功"*/ {602, 0}, /*"身法"*/ {603, 0}, /*"绝技"*/ {604, 0}, /*"拳掌"*/ {605, 0}, /*"指法"*/
            {606, 0}, /*"腿法"*/
            {607, 0}, /*"暗器"*/ {608, 0}, /*"剑法"*/ {609, 0}, /*"刀法"*/ {610, 0}, /*"长兵"*/ {611, 0}, /*"奇门"*/
            {612, 0}, /*"软兵"*/ {613, 0}, /*"御射"*/ {614, 0}, /*"乐器"*/

            {551, 0}, /*"技艺成长"*/ {651, 0}, /*"功法成长"*/ //2均衡3早熟4晚成
        };

        public static Dictionary<int, string> pointDate = new Dictionary<int, string>
        {
            {61, "膂力"}, {62, "体质"}, {63, "灵敏"}, {64, "根骨"}, {65, "悟性"}, {66, "定力"},

            {501, "音律"}, {502, "弈棋"}, {503, "诗书"}, {504, "绘画"}, {505, "术数"}, {506, "品鉴"}, {507, "锻造"}, {508, "制木"},
            {509, "医术"}, {510, "毒术"}, {511, "织锦"}, {512, "巧匠"}, {513, "道法"}, {514, "佛学"}, {515, "厨艺"}, {516, "杂学"},

            {601, "内功"}, {602, "身法"}, {603, "绝技"}, {604, "拳掌"}, {605, "指法"}, {606, "腿法"},
            {607, "暗器"}, {608, "剑法"}, {609, "刀法"}, {610, "长兵"}, {611, "奇门"}, {612, "软兵"}, {613, "御射"}, {614, "乐器"},

            {551, "技艺成长"}, {651, "功法成长"}, //2均衡3早熟4晚成
        };

        public static void ChangePoint(int 点数类型, int 属性Id)
        {
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label(pointDate[属性Id]);
            GUILayout.Label(Characters.GetCharProperty(targetActorId, 属性Id) + "\t" +
                            (修改后的属性[属性Id] == 0 ? "" : ChangedPoint(属性Id).ToString("+#;-#;0")) + "\t");
            if (GUILayout.Button("-", GUILayout.Width(30)) &&
                int.Parse(Characters.GetCharProperty(targetActorId, 属性Id)) + 修改后的属性[属性Id] > 1)
            {
                修改后的属性[属性Id] -= 2;
                自由属性点[点数类型]++;
            }

            if (GUILayout.Button("+", GUILayout.Width(30)) && 自由属性点[点数类型] > 0 &&
                int.Parse(Characters.GetCharProperty(targetActorId, 属性Id)) + ChangedPoint(属性Id) <
                (属性Id < 67 ? 80 : 100) + 10 * (Characters.HasCharProperty(targetActorId, 901)
                    ? int.Parse(Characters.GetCharProperty(targetActorId, 901))
                    : 0))
            {
                修改后的属性[属性Id] += 2;
                自由属性点[点数类型]--;
            }

            GUILayout.EndHorizontal();
        }

        public static void BuyPoint(int pointType)
        {
            bool clicked = false;
            int currentMoney = Convert.ToInt32(Characters.GetCharProperty(DateFile.instance.mianActorId, 406)) +
                               changedMoney;
            int currentPrestige = Convert.ToInt32(Characters.GetCharProperty(DateFile.instance.mianActorId, 407)) +
                                  changedPrestige;
            if (currentMoney < 5000 || currentPrestige < 5000)
            {
                return;
            }
            else
            {
                GUILayout.BeginVertical();
                clicked = GUILayout.Button("+", GUILayout.Width(30));
                GUILayout.EndVertical();
            }

            if (clicked)
            {
                switch (pointType)
                {
                    case 0:
                        自由属性点[0]++;
                        changedPrestige -= 5000;
                        changedMoney -= 5000;
                        break;
                    case 1:
                        自由属性点[1]++;
                        changedPrestige -= 5000;
                        changedMoney -= 5000;
                        break;
                    case 2:
                        自由属性点[2]++;
                        changedPrestige -= 5000;
                        changedMoney -= 5000;
                        break;
                }
            }
        }

        public static void ChangeGrowth(int index, int pointId)
        {
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label(pointDate[pointId]);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("<", GUILayout.Width(30)) && (自由属性点[index] >= 20 || 修改后的属性[pointId] != 0) &&
                int.Parse(Characters.GetCharProperty(targetActorId, pointId)) + 修改后的属性[pointId] > 2)
            {
                if (修改后的属性[pointId] == 0)
                    自由属性点[index] -= 20;
                修改后的属性[pointId]--;
                if (修改后的属性[pointId] == 0)
                    自由属性点[index] += 20;
            }

            GUILayout.Label(
                int.Parse(Characters.GetCharProperty(targetActorId, pointId)) + 修改后的属性[pointId] == 2
                    ? "均衡"
                    : int.Parse(Characters.GetCharProperty(targetActorId, pointId)) + 修改后的属性[pointId] ==
                      3
                        ? "早熟"
                        : "晚成");
            if (GUILayout.Button(">", GUILayout.Width(30)) && (自由属性点[index] >= 20 || 修改后的属性[pointId] != 0) &&
                int.Parse(Characters.GetCharProperty(targetActorId, pointId)) + 修改后的属性[pointId] < 4)
            {
                if (修改后的属性[pointId] == 0)
                    自由属性点[index] -= 20;
                修改后的属性[pointId]++;
                if (修改后的属性[pointId] == 0)
                    自由属性点[index] += 20;
            }

            GUILayout.EndHorizontal();
        }

        public static void SetChangedPoint()
        {
            foreach (var key in pointDate.Keys)
            {
                int temp = int.Parse(Characters.GetCharProperty(targetActorId, key));
                Characters.SetCharProperty(targetActorId, key, (temp + ChangedPoint(key)).ToString());
                修改后的属性[key] = 0;
            }
        }

        //public static void ChangeName()
        //{
        //    //{5,"姓"},{0,"名"},
        //}

        public static bool[] 修改后的性别 = {false, false};

        public static void ChangeSexy()
        {
            //{14,"性别"},{17,"性转外貌"},//可能没有该项
            GUILayout.BeginHorizontal("Box");
            if (GUILayout.Button(
                "性别:" + ((DateFile.instance.GetActorDate(targetActorId, 14, false) == "1" ^ 修改后的性别[0])
                    ? "男"
                    : "女")))
            {
                修改后的性别[0] = !修改后的性别[0];
            }

            if (GUILayout.Button(
                "外貌：" + ((DateFile.instance.GetActorDate(targetActorId, 14, false) == "1" ^ 修改后的性别[0]) ^
                         (DateFile.instance.GetActorDate(targetActorId, 17, false) != "0" ^ 修改后的性别[1])
                    ? "男"
                    : "女")))
            {
                修改后的性别[1] = !修改后的性别[1];
            }

            GUILayout.EndHorizontal();
        }

        public static void SetChangedSexy()
        {
            if (!Characters.HasCharProperty(targetActorId, 14))
            {
                Characters.SetCharProperty(targetActorId, 14,
                    DateFile.instance.GetActorDate(targetActorId, 14, false));
            }

            if (DateFile.instance.GetActorDate(targetActorId, 14, false) == "1" ^ 修改后的性别[0])
            {
                Characters.SetCharProperty(targetActorId, 14, "1");
            }
            else
            {
                Characters.SetCharProperty(targetActorId, 14, "2");
            }

            if (!Characters.HasCharProperty(targetActorId, 17))
            {
                Characters.SetCharProperty(targetActorId, 17,
                    DateFile.instance.GetActorDate(targetActorId, 17, false));
            }

            if (DateFile.instance.GetActorDate(targetActorId, 17, false) != "0" ^ 修改后的性别[1])
            {
                Characters.SetCharProperty(targetActorId, 17, "1");
            }
            else
            {
                Characters.SetCharProperty(targetActorId, 17, "0");
            }

            修改后的性别 = new bool[] {false, false};
        }

        public static int changedMoney = 0;
        public static int changedCharm = 0;

        public static void ChangeCharm()
        {
            //{15,"魅力"},
            GUILayout.BeginHorizontal("Box");
            GUILayout.FlexibleSpace();
            GUILayout.Label("主角金钱：" + Characters.GetCharProperty(DateFile.instance.mianActorId, 406) + " " +
                            changedMoney.ToString("+#;-#;"));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("-10", GUILayout.Width(40)) &&
                int.Parse(Characters.GetCharProperty(targetActorId, 15)) + changedCharm >= 10)
            {
                changedCharm -= 10;
                changedMoney += 5000;
            }

            if (GUILayout.Button("-1", GUILayout.Width(30)) &&
                int.Parse(Characters.GetCharProperty(targetActorId, 15)) + changedCharm >= 1)
            {
                changedCharm--;
                changedMoney += 500;
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label("目标魅力：" + Characters.GetCharProperty(targetActorId, 15) +
                            changedCharm.ToString("+#;-#;"));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+1", GUILayout.Width(30)) &&
                int.Parse(Characters.GetCharProperty(targetActorId, 15)) + changedCharm <= 899 &&
                int.Parse(Characters.GetCharProperty(DateFile.instance.mianActorId, 406)) + changedMoney >= 500)
            {
                changedCharm++;
                changedMoney -= 500;
            }

            if (GUILayout.Button("+10", GUILayout.Width(40)) &&
                int.Parse(Characters.GetCharProperty(targetActorId, 15)) + changedCharm <= 890 &&
                int.Parse(Characters.GetCharProperty(DateFile.instance.mianActorId, 406)) + changedMoney >= 5000)
            {
                changedCharm += 10;
                changedMoney -= 5000;
            }

            GUILayout.EndHorizontal();
        }

        public static void SetChangedCharm()
        {
            if (changedCharm != 0)
            {
                Characters.SetCharProperty(targetActorId, 15,
                    (int.Parse(Characters.GetCharProperty(targetActorId, 15)) + changedCharm)
                    .ToString());
                changedCharm = 0;
            }
        }

        public static int changedPrestige = 0;
        public static int[] changedHeirLoom = {0, 0}; //changedItemId|changedBuffId

        private static readonly int[] HeirLoomId =
        {
            50809, 50909, 80809, 80818, 80827, 80909, 80918, 80927, 70209, 70309, 74009, 60809, 60909, 61009, 61109,
            61209, 61309, 61409, 61509, 61609, 61709, 61809, 61909, 62009, 62109
        };

        private static readonly Dictionary<int, int> HeirLoomIdIndex = new Dictionary<int, int>
        {
            {50809, 0}, {50909, 1}, {80809, 2}, {80818, 3}, {80827, 4}, {80909, 5}, {80918, 6}, {80927, 7}, {70209, 8},
            {70309, 9}, {74009, 10}, {60809, 11}, {60909, 12}, {61009, 13}, {61109, 14}, {61209, 15}, {61309, 16},
            {61409, 17}, {61509, 18}, {61609, 19}, {61709, 20}, {61809, 21}, {61909, 22}, {62009, 23}, {62109, 24},
        };

        private static readonly Dictionary<int, string> HeirLoomData = new Dictionary<int, string>
        {
            {50809, "造成外伤：+20%"}, //降龙玄铁戒
            {50909, "内功发挥：-20%"}, //玄离金册
            {80809, "烈毒抵抗：+900"}, //昊天塔
            {80818, "寒毒抵抗：+900"}, //羲和印
            {80827, "腐毒抵抗：+900"}, //娲皇图
            {80909, "郁毒抵抗：+900"}, //紫皇香炉
            {80918, "赤毒抵抗：+900"}, //涅槃珠
            {80927, "幻毒抵抗：+900"}, //万法荼糜
            {70209, "架势速度：+20%"}, //雷鼓灵旗
            {70309, "提气速度：+20%"}, //万象云光帕
            {74009, "行囊大小：+30"}, //乾坤叉袋
            {60809, "膂力：+45"}, //开天珠
            {60909, "体质：+45"}, //上尊金身
            {61009, "灵敏：+45"}, //神照镜
            {61109, "内伤上限：+150"}, //太真华扃
            {61209, "迅疾：+80"}, //三辰旗
            {61309, "精妙：+80"}, //九乌眼
            {61409, "力道：+80"}, //千均印
            {61509, "悟性：+45"}, //物华天宝
            {61609, "根骨：+45"}, //太始天元册
            {61709, "定力：+45"}, //冰心玉壶
            {61809, "外伤上限：+150"}, //聚宝盆
            {61909, "架势&提气消耗：-15%"}, //九色胭脂壶
            {62009, "造成内伤：+20%"}, //昆仑玉册
            {62109, "内功发挥：+20%"}, //四海匣
        };

        public static int GetHeirloomId()
        {
            foreach (KeyValuePair<int, Dictionary<int, int>> item in DateFile.instance.itemsChangeDate)
            {
                if (item.Value.ContainsKey(92) && item.Value.ContainsKey(93) &&
                    HeirLoomIdIndex.ContainsKey(item.Key)) // 增加条件
                {
                    return item.Key;
                }
            }

            return -1;
        }

        public static int GetHeirloomBuffId()
        {
            foreach (var key in Items.GetItem(GetHeirloomId()).Keys)
            {
                if ((key > 50500 && key < 50517) || (key > 50600 && key < 50615))
                {
                    return key;
                }
            }

            return -1;
        }

        public static void ChangeHeirloom()
        {
            GUILayout.BeginHorizontal("Box");
            GUILayout.FlexibleSpace();
            GUILayout.Label("主角威望：" + Characters.GetCharProperty(DateFile.instance.mianActorId, 407) + " " +
                            changedPrestige.ToString("+#;-#;"));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (targetActorId != DateFile.instance.mianActorId)
            {
                return;
            }

            GUILayout.BeginHorizontal("Box");
            GUILayout.FlexibleSpace();
            GUILayout.Label("家传宝物：");
            bool noHeirloom =
                !DateFile.instance.actorItemsDate[DateFile.instance.mianActorId]
                    .ContainsKey(GetHeirloomId()) && DateFile.instance.GetActorDate(DateFile.instance.mianActorId, 308,
                                                      false) != GetHeirloomId().ToString()
                                                  && DateFile.instance.GetActorDate(DateFile.instance.mianActorId, 309,
                                                      false) != GetHeirloomId().ToString() &&
                                                  DateFile.instance.GetActorDate(DateFile.instance.mianActorId, 310,
                                                      false) != GetHeirloomId().ToString();
            GUILayout.Label(GetHeirloomId() == -1 ? "没有家传宝物" :
                noHeirloom ? "家传宝物已遗失" :
                DateFile.instance.presetitemDate[int.Parse(Items.GetItemProperty(GetHeirloomId(), 999))][0]);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (GetHeirloomId() == -1)
            {
                return;
            }

            GUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("<", GUILayout.Width(30)) &&
                (int.Parse(Characters.GetCharProperty(DateFile.instance.mianActorId, 407)) + changedPrestige >= 5000 ||
                 changedHeirLoom[0] != 0) && HeirLoomIdIndex[int.Parse(Items.GetItemProperty(GetHeirloomId(), 999))] +
                changedHeirLoom[0] > 0)
            {
                if (changedHeirLoom[0] == 0)
                {
                    changedPrestige -= 5000;
                }

                changedHeirLoom[0]--;
                if (changedHeirLoom[0] == 0)
                {
                    changedPrestige += 5000;
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label(HeirLoomData[
                HeirLoomId[
                    HeirLoomIdIndex[int.Parse(Items.GetItemProperty(GetHeirloomId(), 999))] + changedHeirLoom[0]]]);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(">", GUILayout.Width(30)) &&
                (int.Parse(Characters.GetCharProperty(DateFile.instance.mianActorId, 407)) + changedPrestige >= 5000 ||
                 changedHeirLoom[0] != 0) && HeirLoomIdIndex[int.Parse(Items.GetItemProperty(GetHeirloomId(), 999))] +
                changedHeirLoom[0] < 24)
            {
                if (changedHeirLoom[0] == 0)
                {
                    changedPrestige -= 5000;
                }

                changedHeirLoom[0]++;
                if (changedHeirLoom[0] == 0)
                {
                    changedPrestige += 5000;
                }
            }

            GUILayout.EndHorizontal();
            if (GetHeirloomBuffId() == -1)
            {
                return;
            }

            GUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("<", GUILayout.Width(30)) &&
                (int.Parse(Characters.GetCharProperty(DateFile.instance.mianActorId, 407)) + changedPrestige >= 5000 ||
                 changedHeirLoom[1] != 0) && (GetHeirloomBuffId() + changedHeirLoom[1]) % 100 > 1)
            {
                if (changedHeirLoom[1] == 0)
                {
                    changedPrestige -= 5000;
                }

                changedHeirLoom[1]--;
                if (changedHeirLoom[1] == 0)
                {
                    changedPrestige += 5000;
                }
            }

            if (GUILayout.Button("技艺") && GetHeirloomBuffId() + changedHeirLoom[1] > 50600 &&
                (int.Parse(Characters.GetCharProperty(DateFile.instance.mianActorId, 407)) + changedPrestige >= 5000 ||
                 changedHeirLoom[1] != 0))
            {
                if (GetHeirloomBuffId() < 50600)
                {
                    changedHeirLoom[1] = 0;
                    changedPrestige += 5000;
                }
                else
                {
                    if (changedHeirLoom[1] == 0)
                    {
                        changedPrestige -= 5000;
                    }

                    changedHeirLoom[1] = 50501 - GetHeirloomBuffId();
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label(pointDate[GetHeirloomBuffId() - 50000 + changedHeirLoom[1]] + "：+20");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("武学") && GetHeirloomBuffId() + changedHeirLoom[1] < 50600 &&
                (int.Parse(Characters.GetCharProperty(DateFile.instance.mianActorId, 407)) + changedPrestige >= 5000 ||
                 changedHeirLoom[1] != 0))
            {
                if (GetHeirloomBuffId() > 50600)
                {
                    changedHeirLoom[1] = 0;
                    changedPrestige += 5000;
                }
                else
                {
                    if (changedHeirLoom[1] == 0)
                    {
                        changedPrestige -= 5000;
                    }

                    changedHeirLoom[1] = 50601 - GetHeirloomBuffId();
                }
            }

            if (GUILayout.Button(">", GUILayout.Width(30)) &&
                (int.Parse(Characters.GetCharProperty(DateFile.instance.mianActorId, 407)) + changedPrestige >= 5000 ||
                 changedHeirLoom[1] != 0) && (GetHeirloomBuffId() + changedHeirLoom[1]) % 100 <
                16 - ((GetHeirloomBuffId() + changedHeirLoom[1]) / 100 - 505) * 2)
            {
                if (changedHeirLoom[1] == 0)
                {
                    changedPrestige -= 5000;
                }

                changedHeirLoom[1]++;
                if (changedHeirLoom[1] == 0)
                {
                    changedPrestige += 5000;
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            GUILayout.FlexibleSpace();
            GUILayout.Label("单项属性变更消耗5000威望");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public static void SetChangedHeirloom()
        {
            bool cond = (DateFile.instance == null || !Characters.HasChar(DateFile.instance.mianActorId));
            if (cond) return;

            if (changedHeirLoom[0] != 0)
            {
                Items.SetItemProperty(GetHeirloomId(), 999,
                    HeirLoomId[
                            HeirLoomIdIndex[int.Parse(Items.GetItemProperty(GetHeirloomId(), 999))] +
                            changedHeirLoom[0]]
                        .ToString());
                changedHeirLoom[0] = 0;
            }

            if (changedHeirLoom[1] != 0)
            {
                var newHeirloomBuffId = GetHeirloomBuffId() + changedHeirLoom[1];
                Dictionary<int, string> HeirloomProperties =
                    new Dictionary<int, string>(Items.GetItem(GetHeirloomId()));
                HeirloomProperties.Remove(GetHeirloomBuffId());
                HeirloomProperties.Add(newHeirloomBuffId, "20");
                Items.SetItem(GetHeirloomId(), HeirloomProperties);
                Logger.Log("传家宝id为" + GetHeirloomId().ToString());
                changedHeirLoom[1] = 0;
            }
        }

        public static bool isRejuvenation = false;

        public static void Rejuvenation()
        {
            //{11,"年龄"},{12,"健康"},{13,"寿命"},
            if (targetActorId != DateFile.instance.mianActorId)
            {
                return;
            }

            GUILayout.BeginHorizontal("Box");
            GUILayout.FlexibleSpace();
            GUILayout.Label("历练：" + DateFile.instance.gongFaExperienceP + " " + (isRejuvenation ? "-10000" : ""));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("逆运回魂仙梦(10000历练)") && !isRejuvenation &&
                int.Parse(Characters.GetCharProperty(DateFile.instance.mianActorId, 11)) >= 10 &&
                DateFile.instance.gongFaExperienceP >= 10000)
            {
                isRejuvenation = true;
            }

            if (GUILayout.Button("散功") && isRejuvenation)
            {
                isRejuvenation = false;
            }

            GUILayout.EndHorizontal();
        }

        public static void SetRejuvenation()
        {
            bool cond = (DateFile.instance == null || !Characters.HasChar(DateFile.instance.mianActorId));
            if (cond) return;
            if (isRejuvenation)
            {
                for (int i = 0; i < 6; i++)
                {
                    int num = Random.Range(0, 6);
                    Characters.SetCharProperty(DateFile.instance.mianActorId, 61 + num,
                        (int.Parse(Characters.GetCharProperty(DateFile.instance.mianActorId, 61 + num)) + 1)
                        .ToString());
                }

                for (int i = 0; i < 16; i++)
                {
                    int num = Random.Range(0, 16);
                    Characters.SetCharProperty(DateFile.instance.mianActorId, 501 + num,
                        (int.Parse(Characters.GetCharProperty(DateFile.instance.mianActorId, 501 + num)) + 1)
                        .ToString());
                }

                for (int i = 0; i < 14; i++)
                {
                    int num = Random.Range(0, 14);
                    Characters.SetCharProperty(DateFile.instance.mianActorId, 601 + num,
                        (int.Parse(Characters.GetCharProperty(DateFile.instance.mianActorId, 601 + num)) + 1)
                        .ToString());
                }

                Characters.SetCharProperty(DateFile.instance.mianActorId, 11,
                    (int.Parse(Characters.GetCharProperty(DateFile.instance.mianActorId, 11)) - 10).ToString());
                Characters.SetCharProperty(DateFile.instance.mianActorId, 13,
                    (int.Parse(Characters.GetCharProperty(DateFile.instance.mianActorId, 13)) - 6).ToString());
                DateFile.instance.gongFaExperienceP -= 10000;
                Characters.SetCharProperty(DateFile.instance.mianActorId, 39, "8000");
                DateFile.instance.samsara++;
                isRejuvenation = false;
            }
        }

        public static void ShowFeaturesMessage(int featuresId)
        {
            string text = "";
            Dictionary<int, string> dictionary = new Dictionary<int, string>();
            List<int> list = new List<int>(DateFile.instance.buffAttrDate.Keys);
            for (int i = 0; i < list.Count; i++)
            {
                int key = list[i];
                if (!DateFile.instance.actorFeaturesDate[featuresId].ContainsKey(key) ||
                    int.Parse(DateFile.instance.buffAttrDate[key][9]) == 0)
                {
                    continue;
                }

                int num = int.Parse(DateFile.instance.actorFeaturesDate[featuresId][key]);
                if (num != 0)
                {
                    string str = "+";
                    int num2 = 20008;
                    if (num > 0)
                    {
                        num2 = int.Parse(DateFile.instance.buffAttrDate[key][3]);
                    }
                    else
                    {
                        str = "-";
                        num2 = int.Parse(DateFile.instance.buffAttrDate[key][4]);
                    }

                    if (int.Parse(DateFile.instance.buffAttrDate[key][5]) == 0)
                    {
                        string str2 = (int.Parse(DateFile.instance.buffAttrDate[key][1]) == 1)
                            ? Mathf.Abs(num).ToString()
                            : ((float) Mathf.Abs(num) / float.Parse(DateFile.instance.buffAttrDate[key][1])).ToString(
                                (int.Parse(DateFile.instance.buffAttrDate[key][1]) == 100) ? "f2" : "f1");
                        dictionary.Add(key,
                            DateFile.instance.buffAttrDate[key][0] + DateFile.instance.massageDate[10][3] +
                            DateFile.instance.SetColoer(num2,
                                str + str2 + DateFile.instance.buffAttrDate[key][2]));
                    }
                }
            }

            if (dictionary.Count > 0)
            {
                List<int> list2 = new List<int>();
                List<int> list3 = new List<int>(dictionary.Keys);
                for (int j = 0; j < list3.Count; j++)
                {
                    int num3 = list3[j];
                    bool flag = int.Parse(DateFile.instance.buffAttrDate[num3][11]) == 0;
                    string[] array = DateFile.instance.buffAttrDate[num3][6].Split('|');
                    for (int k = 0; k < array.Length; k++)
                    {
                        int num4 = int.Parse(array[k]);
                        if (list3.Contains(num4))
                        {
                            list2.Add(num4);
                            dictionary[num3] = dictionary[num3].Replace(DateFile.instance.buffAttrDate[num3][0],
                                DateFile.instance.buffAttrDate[num3][10]);
                            if (flag)
                            {
                                Dictionary<int, string> dictionary2 = dictionary;
                                int key2 = num3;
                                dictionary2[key2] =
                                    dictionary2[key2] +
                                    DateFile.instance.SetColoer(20002, DateFile.instance.massageDate[10][4]) +
                                    dictionary[num4]
                                        .Replace(DateFile.instance.buffAttrDate[num4][0], "")
                                        .Replace(DateFile.instance.massageDate[10][3], "");
                            }
                        }
                    }
                }

                for (int l = 0; l < list2.Count; l++)
                {
                    dictionary.Remove(list2[l]);
                }

                list3 = new List<int>(dictionary.Keys);
                for (int m = 0; m < list3.Count; m++)
                {
                    text += $"{DateFile.instance.massageDate[10][2]}{dictionary[list3[m]]}\n";
                }

                text += "\n";
            }

            string featureMessage = text;
            GUILayout.Label(featureMessage);
        }

        public static List<int> featureList = new List<int>();
        public static int currentSelectedFeatureId = -1;
        public static string currentSelectedFeatureName = String.Empty;
        public static Vector2 showFeaturesScroll;
        public static Dictionary<string, string> changeableText = new Dictionary<string, string>();

        public static void ShowFeatures(int actorId, int currentFeatureId)
        {
            Dictionary<int, bool> newFeatureClicked = new Dictionary<int, bool>();

            GUILayout.BeginVertical("Box");
            showFeaturesScroll =
                GUILayout.BeginScrollView((Vector2) showFeaturesScroll, GUILayout.Width(120));
            foreach (var key in DateFile.instance.actorFeaturesDate.Keys)
            {
                if (!featureList.Contains(key))
                {
                    newFeatureClicked[key] = GUILayout.Button(DateFile.instance.actorFeaturesDate[key][0]);
                }
            }

            foreach (var key in newFeatureClicked.Keys)
            {
                if (newFeatureClicked[key])
                {
                    int actorCurrentMoney = Characters.GetCharProperty(DateFile.instance.mianActorId, 406).ParseInt();
                    int actorCurrentPrestige =
                        Characters.GetCharProperty(DateFile.instance.mianActorId, 407).ParseInt();
                    if (actorCurrentMoney + changedMoney < 10000 || actorCurrentPrestige + changedPrestige < 10000)
                    {
                        changeableText["特性"] = "银钱或威望不足";
                        break;
                    }
                    else
                    {
                        changedMoney -= 10000;
                        changedPrestige -= 10000;
                        ApplyResourceChange();
                    }

                    if (currentFeatureId != 0)
                    {
                        DateFile.instance.RemoveActorFeature(actorId, currentFeatureId);
                    }

                    if (key != 0)
                    {
                        DateFile.instance.AddActorFeature(actorId, key);
                    }

                    currentSelectedFeatureId = key;
                    currentSelectedFeatureName = DateFile.instance.actorFeaturesDate[currentSelectedFeatureId][0];
                    showFeatures = false;
                    break;
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            GUILayout.Label("使用10000银钱和10000威望来更换或者新增（删除）特性");
            if (changeableText.ContainsKey("特性"))
            {
                GUILayout.Label(changeableText["特性"], new GUIStyle() {normal = {textColor = Color.red}});
            }

            GUILayout.EndVertical();
        }

        public static bool showFeatures;
        public static Vector2 changeFeatureScroll;

        public static void ChangeFeature()
        {
            int actorId = targetActorId;
            Dictionary<string, bool> clicked = new Dictionary<string, bool>();
            featureList = DateFile.instance.GetActorFeature(actorId);
            GUILayout.BeginHorizontal("Box");
            GUILayout.BeginVertical("Box", GUILayout.Width(120));
            GUILayout.Label("个人特性：");
            changeFeatureScroll =
                GUILayout.BeginScrollView((Vector2) changeFeatureScroll, GUILayout.Width(120), GUILayout.Height(250));
            for (int i = 0; i < featureList.Count; i++)
            {
                string featureName = DateFile.instance.actorFeaturesDate[featureList[i]][0];
                clicked[featureList[i].ToString()] = GUILayout.Button(featureName);
            }

            clicked["新增特性"] = GUILayout.Button("+");
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            if (currentSelectedFeatureId != -1)
            {
                GUILayout.BeginVertical("Box", GUILayout.Width(250));

                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("特质名：" + currentSelectedFeatureName);
                if (currentSelectedFeatureId != 0)
                {
                    clicked["更换特性"] = GUILayout.Button("Change");
                }
                else
                {
                    clicked["添加特性"] = GUILayout.Button("New");
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("特质Id：" + currentSelectedFeatureId);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("Box");
                GUILayout.BeginVertical();
                GUILayout.Label("特质属性：");
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                if (currentSelectedFeatureId != -1)
                {
                    ShowFeaturesMessage(currentSelectedFeatureId);
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                if (showFeatures)
                {
                    ShowFeatures(actorId, currentSelectedFeatureId);
                }
            }

            GUILayout.EndHorizontal();


            foreach (var content in clicked.Keys)
            {
                if (clicked[content])
                {
                    if (Regex.IsMatch(content, @"^[+-]?\d*[.]?\d*$"))
                    {
                        currentSelectedFeatureId = content.ParseInt();
                        currentSelectedFeatureName = DateFile.instance.actorFeaturesDate[currentSelectedFeatureId][0];
                        break;
                    }

                    switch (content)
                    {
                        case "新增特性":
                            currentSelectedFeatureId = 0;
                            currentSelectedFeatureName = "新特质";
                            break;
                        case "更换特性":
                            showFeatures = !showFeatures;
                            break;
                        case "添加特性":
                            showFeatures = !showFeatures;
                            break;
                    }

                    break;
                }
            }
        }

        public static void ApplyResourceChange()
        {
            if (changedMoney != 0)
            {
                Characters.SetCharProperty(DateFile.instance.mianActorId, 406,
                    (int.Parse(Characters.GetCharProperty(DateFile.instance.mianActorId, 406)) + changedMoney)
                    .ToString());
                changedMoney = 0;
            }

            if (changedPrestige != 0)
            {
                Characters.SetCharProperty(DateFile.instance.mianActorId, 407,
                    (int.Parse(Characters.GetCharProperty(DateFile.instance.mianActorId, 407)) + changedPrestige)
                    .ToString());
                changedPrestige = 0;
            }
        }

        public static Vector2 scrollPosition;
        public static int targetActorId = -1;

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            bool cond = (DateFile.instance == null || !Characters.HasChar(DateFile.instance.mianActorId));
            if (cond)
            {
                GUILayout.BeginHorizontal("Box");
                GUILayout.Button("未读取存档", GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
                return;
            }

            Dictionary<int, bool> selectedActorId = new Dictionary<int, bool>();
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("请选择要转换的对象：", GUILayout.Width(150));
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginHorizontal();
            foreach (var actorId in DateFile.instance.GetFamily(false, false))
            {
                string actorName = DateFile.instance.GetActorName(actorId);
                GUILayout.BeginVertical(GUILayout.Width(80));
                selectedActorId[actorId] = GUILayout.Button(actorName, GUILayout.Width(80));
                if (selectedActorId[actorId])
                {
                    targetActorId = actorId;
                    Init();
                }

                GUILayout.EndVertical();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();


            if (targetActorId != -1)
            {
                GUILayout.BeginHorizontal("Box");
                GUILayout.BeginVertical("Box");
                GUILayout.BeginHorizontal("Box");
                GUILayout.BeginVertical("Box");
                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("年龄：" + Characters.GetCharProperty(targetActorId, 11) +
                                (isRejuvenation ? " -10" : ""));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("剩余寿命：" + Characters.GetCharProperty(targetActorId, 12) +
                                (isRejuvenation ? " -6" : ""));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("使用5000威望\n和5000金钱\n来购买点数,\n按钮将在威望和\n金钱达标后出现",
                    new GUIStyle() {normal = {textColor = Color.blue}});
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("显示的数值为\n不加特性等修正的数值", new GUIStyle() {normal = {textColor = Color.red}});
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                GUILayout.BeginVertical("Box", GUILayout.Width(250));
                GUILayout.BeginHorizontal("Box");
                GUILayout.BeginVertical(GUILayout.Width(120));
                GUILayout.Label("可用六维点数：" + 自由属性点[0].ToString());
                GUILayout.EndVertical();
                BuyPoint(0);
                GUILayout.EndHorizontal();
                for (int i = 0; i < 6; i++)
                {
                    ChangePoint(0, 61 + i);
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                Rejuvenation();
                ChangeSexy();
                ChangeCharm();
                ChangeHeirloom();
                GUILayout.EndVertical();
                GUILayout.BeginVertical("Box", GUILayout.Width(250));
                GUILayout.BeginHorizontal("Box");
                GUILayout.BeginVertical(GUILayout.Width(120));
                GUILayout.Label("可用技艺点数：" + 自由属性点[1].ToString());
                GUILayout.EndVertical();
                BuyPoint(1);
                GUILayout.EndHorizontal();
                for (int i = 0; i < 16; i++)
                {
                    ChangePoint(1, 501 + i);
                }

                ChangeGrowth(1, 551);
                GUILayout.EndVertical();
                GUILayout.BeginVertical("Box", GUILayout.Width(250));
                GUILayout.BeginHorizontal("Box");
                GUILayout.BeginVertical(GUILayout.Width(120));
                GUILayout.Label("可用武学点数：" + 自由属性点[2].ToString());
                GUILayout.EndVertical();
                BuyPoint(2);
                GUILayout.EndHorizontal();
                for (int i = 0; i < 14; i++)
                {
                    ChangePoint(2, 601 + i);
                }

                ChangeGrowth(2, 651);
                GUILayout.BeginHorizontal("Box");
                GUILayout.FlexibleSpace();
                GUILayout.Label("变更资质成长需花费20可用点数");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("Box");
                GUILayout.FlexibleSpace();
                GUILayout.Label("从左至右顺序为[均衡|早熟|晚成]");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                ChangeFeature();

                GUILayout.BeginHorizontal("Box");
                if (GUILayout.Button((自由属性点[0] + 自由属性点[1] + 自由属性点[2] == 0) ? "应用修改" : "有未使用的可用点数时不能应用更改") &&
                    (自由属性点[0] + 自由属性点[1] + 自由属性点[2] == 0))
                {
                    SetChangedPoint();
                    SetRejuvenation();
                    SetChangedSexy();
                    SetChangedCharm();
                    SetChangedHeirloom();
                    ApplyResourceChange();
                    targetActorId = -1;
                }

                GUILayout.EndHorizontal();
            }
        }
    }
}