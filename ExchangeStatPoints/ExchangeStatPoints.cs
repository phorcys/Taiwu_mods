using UnityEngine;
using UnityModManagerNet;
using System.Collections.Generic;
using System.Linq;

namespace ExchangeStatPoints
{
    public static class Main
    {
        private static int dateId = 0;
        private static int[] freePoint = { 0, 0, 0 };
        private static Dictionary<int, int> changedPoint = new Dictionary<int, int> {
            {61,0},/* "膂力"*/{62,0},/* "体质"*/{63,0},/* "灵敏"*/{64,0},/* "根骨"*/{65,0},/* "悟性"*/{66,0},/* "定力"*/

            {501,0},/*"音律"*/{502,0},/*"弈棋"*/{503,0},/*"诗书"*/{504,0},/*"绘画"*/{505,0},/*"术数"*/{506,0},/*"品鉴"*/{507,0},/*"锻造"*/{508,0},/*"制木"*/
            {509,0},/*"医术"*/{510,0},/*"毒术"*/{511,0},/*"织锦"*/{512,0},/*"巧匠"*/{513,0},/*"道法"*/{514,0},/*"佛学"*/{515,0},/*"厨艺"*/{516,0},/*"杂学"*/

            {601,0},/*"内功"*/{602,0},/*"身法"*/{603,0},/*"绝技"*/{604,0},/*"拳掌"*/{605,0},/*"指法"*/{606,0},/*"腿法"*/
            {607,0},/*"暗器"*/{608,0},/*"剑法"*/{609,0},/*"刀法"*/{610,0},/*"长兵"*/{611,0},/*"奇门"*/{612,0},/*"软兵"*/{613,0},/*"御射"*/{614,0},/*"乐器"*/

            {551,0},/*"技艺成长"*/{651,0},/*"功法成长"*/    //2均衡3早熟4晚成
        };
        private static Dictionary<int, string> pointDate = new Dictionary<int, string>
        {
            {61, "膂力"},{62, "体质"},{63, "灵敏"},{64, "根骨"},{65, "悟性"},{66, "定力"},

            {501,"音律"},{502,"弈棋"},{503,"诗书"},{504,"绘画"},{505,"术数"},{506,"品鉴"},{507,"锻造"},{508,"制木"},
            {509,"医术"},{510,"毒术"},{511,"织锦"},{512,"巧匠"},{513,"道法"},{514,"佛学"},{515,"厨艺"},{516,"杂学"},

            {601,"内功"},{602,"身法"},{603,"绝技"},{604,"拳掌"},{605,"指法"},{606,"腿法"},
            {607,"暗器"},{608,"剑法"},{609,"刀法"},{610,"长兵"},{611,"奇门"},{612,"软兵"},{613,"御射"},{614,"乐器"},

            {551,"技艺成长"},{651,"功法成长"},//2均衡3早熟4晚成
        };
        private static int[] changedHeirloom = { 0, 0 };//changedItemId|changedBuffId
        private static int[] heirloomId = { 50809, 50909, 80809, 80818, 80827, 80909, 80918, 80927, 70209, 70309, 74009, 60809, 60909, 61009, 61109, 61209, 61309, 61409, 61509, 61609, 61709, 61809, 61909, 62009, 62109 };
        private static Dictionary<int, int> heirloomIdIndex = new Dictionary<int, int> { { 50809, 0 }, { 50909, 1 }, { 80809, 2 }, { 80818, 3 }, { 80827, 4 }, { 80909, 5 }, { 80918, 6 }, { 80927, 7 }, { 70209, 8 }, { 70309, 9 }, { 74009, 10 }, { 60809, 11 }, { 60909, 12 }, { 61009, 13 }, { 61109, 14 }, { 61209, 15 }, { 61309, 16 }, { 61409, 17 }, { 61509, 18 }, { 61609, 19 }, { 61709, 20 }, { 61809, 21 }, { 61909, 22 }, { 62009, 23 }, { 62109, 24 }, };
        private static Dictionary<int, string> heirloomData = new Dictionary<int, string> {
            {50809,"造成外伤：+20%"},//降龙玄铁戒
            {50909,"内功发挥：-20%"},//玄离金册
            {80809,"烈毒抵抗：+900"},//昊天塔
            {80818,"寒毒抵抗：+900"},//羲和印
            {80827,"腐毒抵抗：+900"},//娲皇图
            {80909,"郁毒抵抗：+900"},//紫皇香炉
            {80918,"赤毒抵抗：+900"},//涅槃珠
            {80927,"幻毒抵抗：+900"},//万法荼糜
            {70209,"架势速度：+20%"},//雷鼓灵旗
            {70309,"提气速度：+20%"},//万象云光帕
            {74009,"行囊大小：+30"},//乾坤叉袋
            {60809,"膂力：+45"},//开天珠
            {60909,"体质：+45"},//上尊金身
            {61009,"灵敏：+45"},//神照镜
            {61109,"内伤上限：+150"},//太真华扃
            {61209,"迅疾：+80"},//三辰旗
            {61309,"精妙：+80"},//九乌眼
            {61409,"力道：+80"},//千均印
            {61509,"悟性：+45"},//物华天宝
            {61609,"根骨：+45"},//太始天元册
            {61709,"定力：+45"},//冰心玉壶
            {61809,"外伤上限：+150"},//聚宝盆
            {61909,"架势&提气消耗：-15%"},//九色胭脂壶
            {62009,"造成内伤：+20%"},//昆仑玉册
            {62109,"内功发挥：+20%"},//四海匣
        };
        private static int featureId = 0;//0抓周，1-7一般
        private static List<int> featureList = new List<int>();
        private static int[,] changedFeature = new int[8, 2] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, };//类别|等级
        private static int changedCharm = 0;
        private static int changedMoney = 0;
        private static int changedPrestige = 0;

        //{5,"姓"},{0,"名"},{16,"人物立场"},
        ////0刚正，每200一段？

        //{101,"人物特性"},

        //{11,"年龄"},{12,"健康"},{13,"寿命"},

        //{202,"喜爱"},{203,"厌恶"},
        ////无|针匣|对刺|暗器|箫笛|掌套|短杵|拂尘|长鞭|剑|刀|长兵|瑶琴|宝物|冠饰|鞋子|护甲|衣着|代步|促织|图纸|书籍|工具|食材|木材|金铁|宝石|织物|毒物|药材|毒药|丹药|杂物|蛰罐|素食|荤食|神兵|酒|机关|毒器|令符|茶

        //{15,"魅力"},{18,"名誉"},

        //{14,"性别"},{17,"性转外貌"},//可能没有该项
        //{995,"捏脸部件"},{996,"捏脸颜色"},

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnGUI = OnGUI;
            return true;
        }
        private static void CheckDateId()
        {
            var tbl = DateFile.instance;
            bool cond = (tbl == null || tbl.actorsDate == null || !tbl.actorsDate.ContainsKey(tbl.mianActorId));
            if (dateId != SaveDateFile.instance.dateId || cond) //避免跨存档切换
            {
                freePoint = new int[] { 0, 0, 0 };
                changedPoint = new Dictionary<int, int> {
                    {61,0},/* "膂力"*/{62,0},/* "体质"*/{63,0},/* "灵敏"*/{64,0},/* "根骨"*/{65,0},/* "悟性"*/{66,0},/* "定力"*/

                    {501,0},/*"音律"*/{502,0},/*"弈棋"*/{503,0},/*"诗书"*/{504,0},/*"绘画"*/{505,0},/*"术数"*/{506,0},/*"品鉴"*/{507,0},/*"锻造"*/{508,0},/*"制木"*/
                    {509,0},/*"医术"*/{510,0},/*"毒术"*/{511,0},/*"织锦"*/{512,0},/*"巧匠"*/{513,0},/*"道法"*/{514,0},/*"佛学"*/{515,0},/*"厨艺"*/{516,0},/*"杂学"*/

                    {601,0},/*"内功"*/{602,0},/*"身法"*/{603,0},/*"绝技"*/{604,0},/*"拳掌"*/{605,0},/*"指法"*/{606,0},/*"腿法"*/
                    {607,0},/*"暗器"*/{608,0},/*"剑法"*/{609,0},/*"刀法"*/{610,0},/*"长兵"*/{611,0},/*"奇门"*/{612,0},/*"软兵"*/{613,0},/*"御射"*/{614,0},/*"乐器"*/

                    {551,0},/*"技艺成长"*/{651,0},/*"功法成长"*/    //2均衡3早熟4晚成
                };
                changedHeirloom = new int[] { 0, 0 };
                featureId = 0;//0抓周，1-7一般
                changedFeature = new int[8, 2] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, };//类别|等级
                changedCharm = 0;
                changedMoney = 0;
                changedPrestige = 0;
                if (cond)
                {
                    dateId = 0;
                }
                else
                {
                    dateId = SaveDateFile.instance.dateId;
                }
            }
        }
        private static int ChangedPoint(int pointId)
        {
            if (pointId == 551 || pointId == 651) return changedPoint[pointId];
            if (changedPoint[pointId] > 0) return changedPoint[pointId] / 2;
            return changedPoint[pointId];
        }
        private static void ChangePoint(int index, int pointId)
        {
            var tbl = DateFile.instance;
            bool cond = (tbl == null || tbl.actorsDate == null || !tbl.actorsDate.ContainsKey(tbl.mianActorId));
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label(pointDate[pointId]);
            if (cond)
            {
                GUILayout.Label("未读取存档", GUILayout.ExpandWidth(true));
            }
            else
            {
                GUILayout.Label(tbl.GetActorDate(tbl.mianActorId, pointId, false) + "\t" + (changedPoint[pointId] == 0 ? "" : ChangedPoint(pointId).ToString("+#;-#;0")) + "\t");
                //GUILayout.FlexibleSpace();
                if (GUILayout.Button("-", GUILayout.Width(30)) && int.Parse(tbl.actorsDate[tbl.mianActorId][pointId]) + changedPoint[pointId] > 1)
                {
                    changedPoint[pointId] -= 2;
                    freePoint[index]++;
                }
                if (GUILayout.Button("+", GUILayout.Width(30)) && freePoint[index] > 0
                    && int.Parse(tbl.actorsDate[tbl.mianActorId][pointId]) + ChangedPoint(pointId) < (pointId < 67 ? 80 : 100) + 10 * (tbl.actorsDate[tbl.mianActorId].ContainsKey(901) ? int.Parse(tbl.actorsDate[tbl.mianActorId][901]) : 0))
                {
                    changedPoint[pointId] += 2;
                    freePoint[index]--;
                }
            }
            GUILayout.EndHorizontal();
        }
        private static void ChangeGrowth(int index, int pointId)
        {
            var tbl = DateFile.instance;
            bool cond = (tbl == null || tbl.actorsDate == null || !tbl.actorsDate.ContainsKey(tbl.mianActorId));
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label(pointDate[pointId]);
            if (cond)
            {
                GUILayout.Label("未读取存档", GUILayout.ExpandWidth(true));
            }
            else
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("<", GUILayout.Width(30)) && (freePoint[index] >= 20 || changedPoint[pointId] != 0) && int.Parse(tbl.actorsDate[tbl.mianActorId][pointId]) + changedPoint[pointId] > 2)
                {
                    if (changedPoint[pointId] == 0)
                        freePoint[index] -= 20;
                    changedPoint[pointId]--;
                    if (changedPoint[pointId] == 0)
                        freePoint[index] += 20;
                }
                GUILayout.Label(int.Parse(tbl.actorsDate[tbl.mianActorId][pointId]) + changedPoint[pointId] == 2 ? "均衡" : int.Parse(tbl.actorsDate[tbl.mianActorId][pointId]) + changedPoint[pointId] == 3 ? "早熟" : "晚成");
                if (GUILayout.Button(">", GUILayout.Width(30)) && (freePoint[index] >= 20 || changedPoint[pointId] != 0) && int.Parse(tbl.actorsDate[tbl.mianActorId][pointId]) + changedPoint[pointId] < 4)
                {
                    if (changedPoint[pointId] == 0)
                        freePoint[index] -= 20;
                    changedPoint[pointId]++;
                    if (changedPoint[pointId] == 0)
                        freePoint[index] += 20;
                }
            }
            GUILayout.EndHorizontal();
        }
        private static void SetChangedPoint()
        {
            var tbl = DateFile.instance;
            bool cond = (tbl == null || tbl.actorsDate == null || !tbl.actorsDate.ContainsKey(tbl.mianActorId));
            if (cond) return;
            foreach (var key in pointDate.Keys)
            {
                int temp = int.Parse(tbl.actorsDate[tbl.mianActorId][key]);
                tbl.actorsDate[tbl.mianActorId][key] = (temp + ChangedPoint(key)).ToString();
                changedPoint[key] = 0;
            }
        }
        private static int GetHeirloomId()
        {
            var tbl = DateFile.instance;
            bool cond = (tbl == null || tbl.actorsDate == null || !tbl.actorsDate.ContainsKey(tbl.mianActorId));
            if (cond) return -1;
            foreach (var kvp in tbl.itemsChangeDate)
            {
                if (kvp.Value.ContainsKey(92) && kvp.Value.ContainsKey(93))
                {
                    return kvp.Key;
                }
            }
            return -1;
        }
        private static int GetHeirloomBuffId()
        {
            var tbl = DateFile.instance;
            bool cond = (tbl == null || tbl.actorsDate == null || !tbl.actorsDate.ContainsKey(tbl.mianActorId));
            if (cond || GetHeirloomId() == -1) return -1;
            foreach (var key in tbl.itemsDate[GetHeirloomId()].Keys)
            {
                if ((key > 50500 && key < 50517) || (key > 50600 && key < 50615))
                {
                    return key;
                }
            }
            return -1;
        }
        private static void ChangeHeirloom()
        {
            var tbl = DateFile.instance;
            bool cond = (tbl == null || tbl.actorsDate == null || !tbl.actorsDate.ContainsKey(tbl.mianActorId));
            GUILayout.Label("");
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("家传宝物：");
            if (cond)
            {
                GUILayout.Label("未读取存档", GUILayout.ExpandWidth(true));
            }
            else
            {
                bool noHeirloom = !tbl.actorItemsDate[tbl.mianActorId].ContainsKey(GetHeirloomId()) && tbl.GetActorDate(tbl.mianActorId, 308, false) != GetHeirloomId().ToString()
                    && tbl.GetActorDate(tbl.mianActorId, 309, false) != GetHeirloomId().ToString() && tbl.GetActorDate(tbl.mianActorId, 310, false) != GetHeirloomId().ToString();
                GUILayout.Label(GetHeirloomId() == -1 ? "没有家传宝物" : noHeirloom ? "家传宝物已遗失" : tbl.presetitemDate[int.Parse(tbl.itemsDate[GetHeirloomId()][999])][0], GUILayout.ExpandWidth(true));
            }
            GUILayout.EndHorizontal();
            if (cond || GetHeirloomId() == -1) return;
            GUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("<", GUILayout.Width(30)) && (freePoint[0] >= 15 || changedHeirloom[0] != 0) && heirloomIdIndex[int.Parse(tbl.itemsDate[GetHeirloomId()][999])] + changedHeirloom[0] > 0)
            {
                if (changedHeirloom[0] == 0)
                {
                    freePoint[0] -= 15;
                }
                changedHeirloom[0]--;
                if (changedHeirloom[0] == 0)
                {
                    freePoint[0] += 15;
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label(heirloomData[heirloomId[heirloomIdIndex[int.Parse(tbl.itemsDate[GetHeirloomId()][999])] + changedHeirloom[0]]]);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(">", GUILayout.Width(30)) && (freePoint[0] >= 15 || changedHeirloom[0] != 0) && heirloomIdIndex[int.Parse(tbl.itemsDate[GetHeirloomId()][999])] + changedHeirloom[0] < 24)
            {
                if (changedHeirloom[0] == 0)
                {
                    freePoint[0] -= 15;
                }
                changedHeirloom[0]++;
                if (changedHeirloom[0] == 0)
                {
                    freePoint[0] += 15;
                }
            }
            GUILayout.EndHorizontal();
            if (cond || GetHeirloomBuffId() == -1) return;
            GUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("<", GUILayout.Width(30)) && (freePoint[(GetHeirloomBuffId() + changedHeirloom[1]) / 100 - 504] >= 20 || changedHeirloom[1] != 0) && (GetHeirloomBuffId() + changedHeirloom[1]) % 100 > 1)
            {
                if (changedHeirloom[1] == 0)
                {
                    freePoint[(GetHeirloomBuffId() + changedHeirloom[1]) / 100 - 504] -= 20;
                }
                changedHeirloom[1]--;
                if (changedHeirloom[1] == 0)
                {
                    freePoint[(GetHeirloomBuffId() + changedHeirloom[1]) / 100 - 504] += 20;
                }
            }
            if (GUILayout.Button("技艺") && GetHeirloomBuffId() + changedHeirloom[1] > 50600 && (freePoint[1] >= 20 || GetHeirloomBuffId() < 50600))
            {
                if (GetHeirloomBuffId() < 50600)
                {
                    changedHeirloom[1] = 0;
                    freePoint[2] += 20;
                }
                else
                {
                    if (changedHeirloom[1] != 0)
                    {
                        freePoint[2] += 20;
                    }
                    changedHeirloom[1] = 50501 - GetHeirloomBuffId();
                    freePoint[1] -= 20;
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label(pointDate[GetHeirloomBuffId() - 50000 + changedHeirloom[1]] + "：+20", GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("武学") && GetHeirloomBuffId() + changedHeirloom[1] < 50600 && (freePoint[2] >= 20 || GetHeirloomBuffId() > 50600))
            {
                if (GetHeirloomBuffId() > 50600)
                {
                    changedHeirloom[1] = 0;
                    freePoint[1] += 20;
                }
                else
                {
                    if (changedHeirloom[1] != 0)
                    {
                        freePoint[1] += 20;
                    }
                    changedHeirloom[1] = 50601 - GetHeirloomBuffId();
                    freePoint[2] -= 20;
                }
            }
            if (GUILayout.Button(">", GUILayout.Width(30)) && (freePoint[(GetHeirloomBuffId() + changedHeirloom[1]) / 100 - 504] >= 20 || changedHeirloom[1] != 0) && (GetHeirloomBuffId() + changedHeirloom[1]) % 100 < 16 - ((GetHeirloomBuffId() + changedHeirloom[1]) / 100 - 505) * 2)
            {
                if (changedHeirloom[1] == 0)
                {
                    freePoint[(GetHeirloomBuffId() + changedHeirloom[1]) / 100 - 504] -= 20;
                }
                changedHeirloom[1]++;
                if (changedHeirloom[1] == 0)
                {
                    freePoint[(GetHeirloomBuffId() + changedHeirloom[1]) / 100 - 504] += 20;
                }
            }
            GUILayout.EndHorizontal();
        }
        private static void SetChangedHeirloom()
        {
            var tbl = DateFile.instance;
            bool cond = (tbl == null || tbl.actorsDate == null || !tbl.actorsDate.ContainsKey(tbl.mianActorId));
            if (cond) return;
            if (changedHeirloom[0] != 0)
            {
                tbl.itemsDate[GetHeirloomId()][999] = heirloomId[heirloomIdIndex[int.Parse(tbl.itemsDate[GetHeirloomId()][999])] + changedHeirloom[0]].ToString();
                changedHeirloom[0] = 0;
            }
            if (changedHeirloom[1] != 0)
            {
                var newHeirloomBuffId = GetHeirloomBuffId() + changedHeirloom[1];
                tbl.itemsDate[GetHeirloomId()].Remove(GetHeirloomBuffId());
                tbl.itemsDate[GetHeirloomId()].Add(newHeirloomBuffId, "20");
                changedHeirloom[1] = 0;
            }
        }
        private static void ShowFeatureMessage(int featureId)
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            var keys = DateFile.instance.buffAttrDate.Keys;
            foreach (int key in keys)
            {
                if (DateFile.instance.actorFeaturesDate[featureId].ContainsKey(key) && int.Parse(DateFile.instance.buffAttrDate[key][9]) != 0)
                {
                    int num = int.Parse(DateFile.instance.actorFeaturesDate[featureId][key]);
                    if (num != 0 && int.Parse(DateFile.instance.buffAttrDate[key][5]) == 0)
                    {
                        dic.Add(key, ((int.Parse(DateFile.instance.buffAttrDate[key][1]) != 1)
                            ? ((float)num / float.Parse(DateFile.instance.buffAttrDate[key][1])).ToString((int.Parse(DateFile.instance.buffAttrDate[key][1]) != 100) ? "+0.#;-0.#;0" : "+0.##;-0.##;0")
                            : num.ToString("+0;-0;0")) + DateFile.instance.buffAttrDate[key][2]);
                    }
                }
            }
            List<int> list = new List<int>(dic.Keys);
            foreach (int key in list)
            {
                if (DateFile.instance.buffAttrDate[key][6] != "0")
                {
                    if (DateFile.instance.buffAttrDate[key][6] == "50042|50043|50044|50045|50046")
                    {
                        if (dic.ContainsKey(50042) && dic.ContainsKey(50043) && dic.ContainsKey(50044) && dic.ContainsKey(50045) && dic.ContainsKey(50046))
                        {
                            dic[key] = DateFile.instance.buffAttrDate[key][10] + "：" + dic[key];
                            dic.Remove(50042);
                            dic.Remove(50043);
                            dic.Remove(50044);
                            dic.Remove(50045);
                            dic.Remove(50046);
                        }
                    }
                    else
                    {
                        if (int.Parse(DateFile.instance.buffAttrDate[key][6]) != 0)
                        {
                            if (dic.ContainsKey(int.Parse(DateFile.instance.buffAttrDate[key][6])))
                            {
                                dic[key] = DateFile.instance.buffAttrDate[key][10] + "：" + dic[key] + "※" + dic[int.Parse(DateFile.instance.buffAttrDate[key][6])];
                                dic.Remove(int.Parse(DateFile.instance.buffAttrDate[key][6]));
                            }
                            else
                            {
                                dic[key] = DateFile.instance.buffAttrDate[key][0] + "：" + dic[key];
                            }
                        }
                    }
                }
            }
            list = new List<int>(dic.Keys);
            foreach (int key in list)
            {
                if (DateFile.instance.buffAttrDate[key][6] == "0")
                {
                    dic[key] = DateFile.instance.buffAttrDate[key][0] + "：" + dic[key];
                }
                GUILayout.Label(dic[key]);
            }
        }
        private static int GetChangedFeature(int num)
        {
            int class0 = int.Parse(DateFile.instance.actorFeaturesDate[featureList[num]][5]) + changedFeature[num, 0];
            int Level = int.Parse(DateFile.instance.actorFeaturesDate[featureList[num]][4]) + changedFeature[num, 1];
            if (Level == 0 || class0 == 0)
            {
                return 0;
            }
            return class0 - 1 + (Level > 0 ? Level : (Level * -1 + 3));
        }
        private static void ChangeFeature()
        {
            var tbl = DateFile.instance;
            if (DateFile.instance == null || DateFile.instance.actorsDate == null || !DateFile.instance.actorsDate.ContainsKey(DateFile.instance.mianActorId)) return;
            featureList = new List<int>(DateFile.instance.GetActorFeature(DateFile.instance.mianActorId));
            int zhuazhou = 0;
            foreach (int key in featureList)
            {
                if (key >= 2001 && key <= 2012)
                {
                    zhuazhou = key;
                }
            }
            featureList.RemoveAll(n => n == 0);
            featureList.RemoveAll(n => n > 168);
            featureList.Insert(0, zhuazhou);
            while (featureList.Count() <= 8)
            {
                featureList.Add(0);
            }
            GUILayout.BeginHorizontal("Box");
            GUILayout.FlexibleSpace();
            GUILayout.Label("威望余额：" + tbl.actorsDate[tbl.mianActorId][407] + changedPrestige.ToString("+#;-#;"));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            GUILayout.BeginVertical("Box", GUILayout.Width(200));
            GUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("<", GUILayout.Width(30)) && (int.Parse(tbl.actorsDate[tbl.mianActorId][407]) + changedPrestige >= 500 || changedFeature[0, 0] != 0) &&
                (changedFeature[0, 0] + featureList[0] > 2001 || (changedFeature[0, 0] == 2001 && featureList[0] == 0)))
            {
                if (changedFeature[0, 0] == 0)
                {
                    changedPrestige -= 500;
                }
                if (changedFeature[0, 0] + featureList[0] == 2001)
                {
                    changedFeature[0, 0] = 0;
                }
                else
                {
                    changedFeature[0, 0]--;
                }
                if (changedFeature[0, 0] == 0)
                {
                    changedPrestige += 500;
                }
            }
            GUILayout.FlexibleSpace();
            if (changedFeature[0, 0] + featureList[0] == 0)
            {
                GUILayout.Label("无抓周特性");
            }
            else
            {
                GUILayout.Label(DateFile.instance.actorFeaturesDate[changedFeature[0, 0] + featureList[0]][0]);
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(">", GUILayout.Width(30)) && (int.Parse(tbl.actorsDate[tbl.mianActorId][407]) + changedPrestige >= 500 || changedFeature[0, 0] != 0) && changedFeature[0, 0] + featureList[0] < 2012)
            {
                if (changedFeature[0, 0] == 0)
                {
                    changedPrestige -= 500;
                }
                if (changedFeature[0, 0] + featureList[0] == 0)
                {
                    changedFeature[0, 0] += 2001;
                }
                else
                {
                    changedFeature[0, 0]++;
                }
                if (changedFeature[0, 0] == 0)
                {
                    changedPrestige += 500;
                }
            }
            GUILayout.EndHorizontal();
            ShowFeatureMessage(changedFeature[0, 0] + featureList[0]);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            GUILayout.BeginHorizontal("Box");
            if (GUILayout.Button(DateFile.instance.actorFeaturesDate[GetChangedFeature(1)][0])) featureId = 1;
            if (GUILayout.Button(DateFile.instance.actorFeaturesDate[GetChangedFeature(2)][0])) featureId = 2;
            if (GUILayout.Button(DateFile.instance.actorFeaturesDate[GetChangedFeature(3)][0])) featureId = 3;
            if (GUILayout.Button(DateFile.instance.actorFeaturesDate[GetChangedFeature(4)][0])) featureId = 4;
            if (GUILayout.Button(DateFile.instance.actorFeaturesDate[GetChangedFeature(5)][0])) featureId = 5;
            if (GUILayout.Button(DateFile.instance.actorFeaturesDate[GetChangedFeature(6)][0])) featureId = 6;
            if (GUILayout.Button(DateFile.instance.actorFeaturesDate[GetChangedFeature(7)][0])) featureId = 7;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            GUILayout.BeginVertical("Box", GUILayout.Width(200));
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("特性类别：");
            if (GUILayout.Button("<", GUILayout.Width(30)))
            {
                int i = 0;
                bool isCover = true;
                while (isCover)
                {
                    i -= 6;
                    isCover = false;
                    for (int j = 1; j < 8; j++)
                    {
                        if (j != featureId && int.Parse(DateFile.instance.actorFeaturesDate[featureList[j]][5]) + changedFeature[j, 0]
                                                == int.Parse(DateFile.instance.actorFeaturesDate[featureList[featureId]][5]) + changedFeature[featureId, 0] + i)
                        {
                            isCover = true;
                        }
                    }
                }
                if (int.Parse(DateFile.instance.actorFeaturesDate[featureList[featureId]][5]) + changedFeature[featureId, 0] + i < 0)
                {
                    if (featureList[featureId] == 0)
                    {
                        i += 5;
                    }
                    else
                    {
                        i = 0;
                    }
                }
                if (featureId == 0)
                {
                    i = 0;
                }
                if (i != 0)
                {
                    if (changedFeature[featureId, 0] != 0 || int.Parse(tbl.actorsDate[tbl.mianActorId][407]) + changedPrestige >= 500)
                    {
                        if (changedFeature[featureId, 0] == 0)
                        {
                            changedPrestige -= 500;
                        }
                        changedFeature[featureId, 0] += i;
                        if (changedFeature[featureId, 0] == 0)
                        {
                            changedPrestige += 500;
                        }
                    }
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label((int.Parse(DateFile.instance.actorFeaturesDate[featureList[featureId]][5]) + changedFeature[featureId, 0]).ToString());
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(">", GUILayout.Width(30)))
            {
                int i = 0;
                if (int.Parse(DateFile.instance.actorFeaturesDate[featureList[featureId]][5]) + changedFeature[featureId, 0] == 0)
                {
                    i = -5;
                }
                bool isCover = true;
                while (isCover)
                {
                    i += 6;
                    isCover = false;
                    for (int j = 1; j < 8; j++)
                    {
                        if (j != featureId && int.Parse(DateFile.instance.actorFeaturesDate[featureList[j]][5]) + changedFeature[j, 0]
                                                == int.Parse(DateFile.instance.actorFeaturesDate[featureList[featureId]][5]) + changedFeature[featureId, 0] + i)
                        {
                            isCover = true;
                        }
                    }
                }
                if (int.Parse(DateFile.instance.actorFeaturesDate[featureList[featureId]][5]) + changedFeature[featureId, 0] + i > 163 || featureId == 0)
                {
                    i = 0;
                }
                if (i != 0)
                {
                    if (changedFeature[featureId, 0] != 0 || int.Parse(tbl.actorsDate[tbl.mianActorId][407]) + changedPrestige >= 500)
                    {
                        if (changedFeature[featureId, 0] == 0)
                        {
                            changedPrestige -= 500;
                        }
                        changedFeature[featureId, 0] += i;
                        if (changedFeature[featureId, 0] == 0)
                        {
                            changedPrestige += 500;
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("特性等级：");
            if (GUILayout.Button("-", GUILayout.Width(30))
                && ((int.Parse(DateFile.instance.actorFeaturesDate[featureList[featureId]][4]) + changedFeature[featureId, 1]) > -1
                    || (changedFeature[featureId, 0] == 0 && changedFeature[featureId, 1] > 0)))
            {
                changedFeature[featureId, 1]--;
                if (changedFeature[featureId, 1] >= 0)
                {
                    changedPrestige += 500;
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label((int.Parse(DateFile.instance.actorFeaturesDate[featureList[featureId]][4]) + changedFeature[featureId, 1]).ToString("+#;-#;0"));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+", GUILayout.Width(30))
                && (int.Parse(tbl.actorsDate[tbl.mianActorId][407]) + changedPrestige >= 500 || changedFeature[featureId, 1] < 0)
                && ((int.Parse(DateFile.instance.actorFeaturesDate[featureList[featureId]][4]) + changedFeature[featureId, 1]) < 2
                    || (changedFeature[featureId, 0] == 0 && changedFeature[featureId, 1] < 0)))
            {
                if (changedFeature[featureId, 1] >= 0)
                {
                    changedPrestige -= 500;
                }
                changedFeature[featureId, 1]++;
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Reset"))
            {
                if (changedFeature[featureId, 0] != 0)
                {
                    changedPrestige += 500;
                }
                if (changedFeature[featureId, 1] > 0)
                {
                    changedPrestige += 500 * changedFeature[featureId, 1];
                }
                changedFeature[featureId, 0] = 0;
                changedFeature[featureId, 1] = 0;
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical("Box");
            ShowFeatureMessage(GetChangedFeature(featureId));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        private static void SetChangedFeature()
        {
            if (changedPrestige != 0)
            {
                DateFile.instance.actorsDate[DateFile.instance.mianActorId][407]=(int.Parse(DateFile.instance.actorsDate[DateFile.instance.mianActorId][407]) + changedPrestige).ToString();
                changedPrestige=0;
            }
            if (changedFeature[0, 0] != 0)
            {
                if (featureList[0] == 0)
                {
                    DateFile.instance.actorsDate[DateFile.instance.mianActorId][101] += "|" + changedFeature[0, 0].ToString();
                }
                else
                {
                    DateFile.instance.ChangeActorFeature(DateFile.instance.mianActorId, featureList[0], featureList[0] + changedFeature[0, 0]);
                }
                changedFeature[0, 0] = 0;
            }
            for (int i = 1; i < 8; i++)
            {
                if (GetChangedFeature(i) != featureList[i])
                {
                    if (featureList[i] == 0)
                    {
                        DateFile.instance.actorsDate[DateFile.instance.mianActorId][101] += "|" + GetChangedFeature(i).ToString();
                    }
                    else
                    {
                        DateFile.instance.ChangeActorFeature(DateFile.instance.mianActorId, featureList[i], GetChangedFeature(i));
                    }
                    changedFeature[i, 0] = 0;
                    changedFeature[i, 1] = 0;
                }
            }
        }
        private static void ChangeCharm()
        {
            var tbl = DateFile.instance;
            bool cond = (tbl == null || tbl.actorsDate == null || !tbl.actorsDate.ContainsKey(tbl.mianActorId));
            if(cond) return;
            GUILayout.BeginHorizontal("Box");
            GUILayout.FlexibleSpace();
            GUILayout.Label("金钱余额：" + tbl.actorsDate[tbl.mianActorId][406] + changedMoney.ToString("+#;-#;"));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("-10", GUILayout.Width(30)) && int.Parse(tbl.actorsDate[tbl.mianActorId][15]) + changedCharm >= 10)
            {
                if (changedCharm >= 10)
                {
                    changedMoney += (int.Parse(tbl.actorsDate[tbl.mianActorId][15]) + changedCharm) * 10 - 45;
                }
                else if (changedCharm > 0)
                {
                    changedMoney += (int.Parse(tbl.actorsDate[tbl.mianActorId][15]) + changedCharm) * changedCharm - (changedCharm * (changedCharm - 1) / 2);
                }
                changedCharm -= 10;
            }
            if (GUILayout.Button("-1", GUILayout.Width(30)) && int.Parse(tbl.actorsDate[tbl.mianActorId][15]) + changedCharm >= 1)
            {
                if (changedCharm > 0)
                {
                    changedMoney += int.Parse(tbl.actorsDate[tbl.mianActorId][15]) + changedCharm;
                }
                changedCharm--;
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label("魅力：" + tbl.actorsDate[tbl.mianActorId][15] + changedCharm.ToString("+#;-#;"));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+1", GUILayout.Width(30)) && int.Parse(tbl.actorsDate[tbl.mianActorId][15]) + changedCharm <= 899
                && (changedCharm < 0 || int.Parse(tbl.actorsDate[tbl.mianActorId][406]) + changedMoney >= (int.Parse(tbl.actorsDate[tbl.mianActorId][15]) + changedCharm + 1)))
            {
                if (changedCharm >= 0)
                {
                    changedMoney -= int.Parse(tbl.actorsDate[tbl.mianActorId][15]) + changedCharm + 1;
                }
                changedCharm++;
            }
            if (GUILayout.Button("+10", GUILayout.Width(30)) && int.Parse(tbl.actorsDate[tbl.mianActorId][15]) + changedCharm <= 890
                && (changedCharm <= -10 || changedCharm >= 0
                ? ((int.Parse(tbl.actorsDate[tbl.mianActorId][406]) + changedMoney) > ((int.Parse(tbl.actorsDate[tbl.mianActorId][15]) + changedCharm + 1) * 10 + 45))
                : (int.Parse(tbl.actorsDate[tbl.mianActorId][406]) + changedMoney) > ((int.Parse(tbl.actorsDate[tbl.mianActorId][15]) + 1) * (10 + changedCharm) + ((10 + changedCharm) * (9 + changedCharm) / 2))))
            {
                if (changedCharm >= 0)
                {
                    changedMoney -= (int.Parse(tbl.actorsDate[tbl.mianActorId][15]) + changedCharm + 1) * 10 + 45;
                }
                else if (changedCharm > -10)
                {
                    changedMoney -= (int.Parse(tbl.actorsDate[tbl.mianActorId][15]) + 1) * (10 + changedCharm) + ((10 + changedCharm) * (9 + changedCharm) / 2);
                }
                changedCharm += 10;
            }
            GUILayout.EndHorizontal();
        }
        private static void SetChangedCharm()
        {
            if (changedCharm != 0)
            {
                DateFile.instance.actorsDate[DateFile.instance.mianActorId][15]=(int.Parse(DateFile.instance.actorsDate[DateFile.instance.mianActorId][15])+changedCharm).ToString();
            }
            changedCharm = 0;
            if (changedMoney != 0)
            {
                DateFile.instance.actorsDate[DateFile.instance.mianActorId][406] = (int.Parse(DateFile.instance.actorsDate[DateFile.instance.mianActorId][406]) + changedMoney).ToString();
            }
            changedMoney = 0;
        }
        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            var tbl = DateFile.instance;
            bool cond = (tbl == null || tbl.actorsDate == null || !tbl.actorsDate.ContainsKey(tbl.mianActorId));
            CheckDateId();
            GUILayout.BeginHorizontal("Box");
            if (GUILayout.Button((freePoint[0] + freePoint[1] + freePoint[2] == 0) ? "Apply" : "有未使用的可用点数时不能应用更改") && (freePoint[0] + freePoint[1] + freePoint[2] == 0))
            {
                SetChangedPoint();
                SetChangedHeirloom();
                SetChangedFeature();
                SetChangedCharm();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            GUILayout.BeginVertical("Box");
            ChangePoint(0, 61);
            ChangePoint(0, 62);
            ChangePoint(0, 63);
            ChangePoint(0, 64);
            ChangePoint(0, 65);
            ChangePoint(0, 66);
            GUILayout.Label("可用六维点数：" + freePoint[0].ToString());
            ChangeHeirloom();
            GUILayout.Label("变更固有属性消耗15六维可用点数。");
            GUILayout.Label("变更资质加成消耗20目标类可用点数。");
            ChangeCharm();
            GUILayout.EndVertical();
            GUILayout.BeginVertical("Box");
            ChangePoint(1, 501);
            ChangePoint(1, 502);
            ChangePoint(1, 503);
            ChangePoint(1, 504);
            ChangePoint(1, 505);
            ChangePoint(1, 506);
            ChangePoint(1, 507);
            ChangePoint(1, 508);
            ChangePoint(1, 509);
            ChangePoint(1, 510);
            ChangePoint(1, 511);
            ChangePoint(1, 512);
            ChangePoint(1, 513);
            ChangePoint(1, 514);
            ChangePoint(1, 515);
            ChangePoint(1, 516);
            ChangeGrowth(1, 551);
            GUILayout.Label("可用技艺点数：" + freePoint[1].ToString());
            GUILayout.EndVertical();
            GUILayout.BeginVertical("Box");
            ChangePoint(2, 601);
            ChangePoint(2, 602);
            ChangePoint(2, 603);
            ChangePoint(2, 604);
            ChangePoint(2, 605);
            ChangePoint(2, 606);
            ChangePoint(2, 607);
            ChangePoint(2, 608);
            ChangePoint(2, 609);
            ChangePoint(2, 610);
            ChangePoint(2, 611);
            ChangePoint(2, 612);
            ChangePoint(2, 613);
            ChangePoint(2, 614);
            ChangeGrowth(2, 651);
            GUILayout.Label("可用武学点数：" + freePoint[2].ToString());
            GUILayout.Label("变更资质成长需花费20可用点数。");
            GUILayout.Label("从左至右顺序为[均衡|早熟|晚成]。");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            ChangeFeature();
        }
    }
}
