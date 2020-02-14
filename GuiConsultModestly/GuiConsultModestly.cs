using Harmony12;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace GuiConsultModestly
{
    public class Settings : UnityModManager.ModSettings
    {
        public int AddWind = 0;
        public int ReadLevel = 1;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }

    }
    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;


        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            #region 基础设置
            settings = Settings.Load<Settings>(modEntry);
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            HarmonyInstance harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            #endregion

            return true;
        }

        static string title = "鬼的虚心请教";
        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(title, GUILayout.Width(300));

            GUILayout.BeginHorizontal();
            Main.settings.AddWind = (int)GUILayout.HorizontalSlider(Main.settings.AddWind, 0, 30);
            GUILayout.Label(string.Format("作弊增加成功率：{0}%", Main.settings.AddWind));
            float v = Main.settings.AddWind / 100f;
            GUILayout.Label("<color=#" + (v * 255 > 16 ? "" : "0") + Convert.ToString((int)(v * 255), 16) + "0000>此举有伤天和，请酌情使用</color>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Main.settings.ReadLevel = (int)GUILayout.HorizontalSlider(Main.settings.ReadLevel, 1, 3);
            GUILayout.Label(string.Format("作弊增加研读页数：{0}倍", Main.settings.ReadLevel));
            v = (Main.settings.ReadLevel - 1) / 9f;
            GUILayout.Label("<color=#" + (v * 255 > 16 ? "" : "0") + Convert.ToString((int)(v * 255), 16) + "0000>此举有伤天和，请酌情使用</color>");
            GUILayout.EndHorizontal();
        }

        [HarmonyPatch(typeof(SkillBattleSystem), "AnQusetion")]
        public static class SkillBattleSystem_AnQusetion_Patch
        {
            static FieldInfo m_actorSkillBattleValue;
            public static int[] actorSkillBattleValue
            {
                get
                {
                    //if (m_actorSkillBattleValue == null)
                    //{
                    m_actorSkillBattleValue = typeof(SkillBattleSystem).GetField("actorSkillBattleValue", BindingFlags.NonPublic | BindingFlags.Instance);
                    //}
                    int[] value;
                    try
                    {
                        value = (int[])m_actorSkillBattleValue.GetValue(SkillBattleSystem.instance);
                    }
                    catch
                    {
                        value = new int[9];
                        Main.Logger.Log("反射出错");
                    }

                    return value;
                }
            }
            static FieldInfo m_enemySkillBattleValue;
            public static int[] enemySkillBattleValue
            {
                get
                {
                    //if (m_enemySkillBattleValue == null)
                    //{
                    m_enemySkillBattleValue = typeof(SkillBattleSystem).GetField("enemySkillBattleValue", BindingFlags.NonPublic | BindingFlags.Instance);
                    //}
                    int[] value;
                    try
                    {
                        value = (int[])m_enemySkillBattleValue.GetValue(SkillBattleSystem.instance);
                    }
                    catch
                    {
                        value = new int[9];
                        Main.Logger.Log("反射出错");
                    }

                    return value;
                }
            }

            static FieldInfo m_nowSkillIndex;
            public static int nowSkillIndex
            {
                get
                {
                    //if (m_nowSkillIndex == null)
                    //{
                    m_nowSkillIndex = typeof(SkillBattleSystem).GetField("nowSkillIndex", BindingFlags.NonPublic | BindingFlags.Instance);
                    //}
                    int value;
                    try
                    {
                        value = (int)m_nowSkillIndex.GetValue(SkillBattleSystem.instance);
                    }
                    catch
                    {
                        value = 0;
                        Main.Logger.Log("反射出错");
                    }

                    return value;
                }
            }
            static FieldInfo m_questionPower;
            public static int questionPower
            {
                get
                {
                    //if (m_questionPower == null)
                    //{
                    m_questionPower = typeof(SkillBattleSystem).GetField("questionPower", BindingFlags.NonPublic | BindingFlags.Instance);
                    //}
                    int value;
                    try
                    {
                        value = (int)m_questionPower.GetValue(SkillBattleSystem.instance);
                    }
                    catch
                    {
                        value = 0;
                        Main.Logger.Log("反射出错");
                    }

                    return value;
                }
            }
            static FieldInfo m_mianEnemyId;
            public static int mianEnemyId
            {
                get
                {
                    //if (m_mianEnemyId == null)
                    //{
                    m_mianEnemyId = typeof(SkillBattleSystem).GetField("mianEnemyId", BindingFlags.NonPublic | BindingFlags.Instance);
                    //}
                    int value;
                    try
                    {
                        value = (int)m_mianEnemyId.GetValue(SkillBattleSystem.instance);
                    }
                    catch
                    {
                        value = 0;
                        Main.Logger.Log("反射出错");
                    }

                    return value;
                }
            }
            static SkillBattleSystem _this = SkillBattleSystem.instance;
            static void Postfix(bool isActor)
            {
                if (!Main.enabled)
                    return;
                _this = SkillBattleSystem.instance;

                try
                {
                    string[] conten = SetSkillBatterGains(isActor);
                    if (conten != null && conten[0] != null)
                    {
                        YesOrNoWindow.instance.SetYesOrNoWindow(-1, conten[0], conten[1], false, true);
                    }
                }
                catch (Exception e)
                {
                    YesOrNoWindow.instance.SetYesOrNoWindow(-1, "鬼的虚心请教 MOD出错", e.Message, false, true);
                }
            }

            /// <summary>
            /// 设置技艺比拼收获
            /// </summary>
            static string[] SetSkillBatterGains(bool isActor)
            {
                Main.Logger.Log("设置技艺比拼收获");
                if (isActor)
                    return null;


                string[] result = new string[2];
                int typ = nowSkillIndex;
                int power = questionPower;
                int battleValue = 0;
                int opponentBattleValue = 0;
                string selfName = "";
                string opponentName = "";
                int _actorId = DateFile.instance.MianActorID();
                int _mianEnemyId = mianEnemyId;

                battleValue = actorSkillBattleValue[typ];
                opponentBattleValue = enemySkillBattleValue[typ];
                selfName = "<color=#8E8E8EFF>" + DateFile.instance.GetActorName(_actorId, false, false) + "</color>";
                opponentName = "<color=#8E8E8EFF>" + DateFile.instance.GetActorName(_mianEnemyId, false, false) + "</color>";

                //int _skillId = DateFile.instance.ParseInt(DateFile.instance.baseSkillDate[_this.battleSkillTyp][typ + 1]);
                int _skillId = int.Parse(DateFile.instance.baseSkillDate[_this.battleSkillTyp][typ + 1], System.Globalization.CultureInfo.InvariantCulture);
                //string book_name = DateFile.instance.skillDate[_skillId][typ];// 书籍名
                var data = DateFile.instance.skillDate[int.Parse(DateFile.instance.baseSkillDate[_this.battleSkillTyp][nowSkillIndex + 1], System.Globalization.CultureInfo.InvariantCulture)];
                string book_name = "<color=#8E8E8EFF>《" + data[0] + "》</color>" + data[1001];
                string bname = "<color=#8E8E8EFF>《" + data[0] + "》</color>";



                int value = 0;
                int num = 0;// 上一阶书籍读的页数
                int num2 = 0;// 当前阶书籍读的页数

                // 判断是否一阶书籍
                int stage = int.Parse(data[2]);

                // 计算当前阶书籍读了几页
                bool flagB = DateFile.instance.skillBookPages.ContainsKey(_skillId);
                if (flagB)
                {
                    for (int pageIndex = 0; pageIndex < 10; pageIndex++)
                    {
                        int num8 = DateFile.instance.skillBookPages[_skillId][pageIndex];
                        bool flag9 = num8 == 1;
                        if (flag9)
                        {
                            num2++;
                        }
                    }
                }

                // 计算上一阶书籍读了几页
                bool flagC = DateFile.instance.skillBookPages.ContainsKey(_skillId - 1);
                if (flagC)
                {
                    for (int pageIndex = 0; pageIndex < 10; pageIndex++)
                    {
                        int num8 = DateFile.instance.skillBookPages[_skillId - 1][pageIndex];
                        bool flag9 = num8 == 1;
                        if (flag9)
                        {
                            num++;
                        }
                    }
                }

                // （当前比拼是第一阶 或者 上一阶书已读满五页）并且当前比拼的书籍尚未读满10页
                if (num2 != 10 && (stage == 1 || num >= 5))
                {
                    int exceed = power - battleValue;
                    if (exceed < 1)
                    {
                        if (opponentBattleValue > (battleValue + 100)) // 深刻理解：虽然《"+book_name+"》已经熟读于心，但是听了"+opponentName+"的讲解之后，有了更深刻的认识。
                        {
                            result[0] = "理解加深";
                            result[1] = "虽然" + selfName + "已然知晓" + book_name + "，但是听了" + opponentName + "的讲解之后，有了更深刻的认识。";
                            value = 20;
                        }
                        else if (battleValue > (opponentBattleValue + 100)) // 指点江山：《"+book_name+"》心中已经有了深刻认识，觉得"+opponentName+"讲解颇有不足，为其讲解了此中深意。
                        {
                            result[0] = "指点江山";
                            result[1] = selfName + "觉得" + opponentName + "对" + book_name + "理解不够深刻，为其讲解了此中深意，" + opponentName + "感激不尽。\n<color=#8E8E8EFF>【需要对方水平比自己高100】"+
                                opponentBattleValue +":"+ battleValue + "</color>";
                        }
                        else
                        {
                            result[0] = "各抒己见";
                            result[1] = selfName + "和" + opponentName + "对" + book_name + "均有不同见解，双方吵得不可开交，差点就打起来了\n<color=#8E8E8EFF>【需要对方水平比自己高100】" +
                                opponentBattleValue + ":" + battleValue + "</color>";
                        }
                    }
                    else if (exceed >= 1 && exceed <= 100) // 茅塞顿开：《"+book_name+"》已经困扰自己已久，此时听了"+opponentName+"的讲解之后茅塞顿开，心中有了更深刻的认识。
                    {
                        result[0] = "茅塞顿开";
                        result[1] = book_name + "中的内容已经困扰" + selfName + "已久，此时听了" + opponentName + "的讲解之后茅塞顿开，对" + book_name + "有了更深刻的认识。";
                        value = 90;
                    }
                    else if (exceed >= 101 && exceed <= 200) // 一知半解：听了"+opponentName+"讲解受益良多，但《"+book_name+"》颇有难度，心中仍有多处不解。
                    {
                        result[0] = "受益良多";
                        result[1] = selfName + "听了" + opponentName + "讲解受益良多，但" + book_name + "颇有难度，心中多处不解变得明朗起来。";
                        value = 40;
                    }
                    else if (exceed >= 201) // 一头雾水：听着"+opponentName+"滔滔不绝指点江山，心中毫无头绪，对《"+book_name+"》完全无法理解。
                    {
                        result[0] = "一头雾水";
                        result[1] = selfName + "听着" + opponentName + "滔滔不绝指点江山，心中毫无头绪，对" + book_name + "始终无法理解。";
                    }

                    if (value > 0)
                    {
                        var seven = DateFile.instance.GetActorResources(_actorId)[0]; // 七元赋性: 0 = 细腻  1 = 聪颖  2 = 水性  3 = 勇壮  4 = 坚毅  5 = 冷静  6 = 机缘
                        var rand = UnityEngine.Random.Range(0, 100);
                        var need = (value + seven + Main.settings.AddWind);
                        result[1] += "\n<color=#8E8E8EFF>【细腻影响领悟成功率】粗心点数越小越好" + rand + "/" + need+ "</color>";
                        value = rand <= need ? 1 : 0;

                        if (value > 0)
                        {
                            seven = DateFile.instance.GetActorResources(_actorId)[1];
                            rand = UnityEngine.Random.Range(0, 95) + seven + Main.settings.AddWind; // 七元赋性: 0 = 细腻  1 = 聪颖  2 = 水性  3 = 勇壮  4 = 坚毅  5 = 冷静  6 = 机缘
                            need = (stage * 10);
                            result[1] += "\n<color=#8E8E8EFF>【聪颖影响研读成功率】研读点数越大越好" + rand + "/" + need + "</color>";
                            //Logger.Log(stage + "阶书籍判断几率 " + rand + "<=" + (stage * 10) + " " + (rand <= (stage * 10)));
                            if (rand <= need) // 书籍阶数 研读成功率越低
                                value = 0;
                        }

                        if (value > 0)
                        {
                            //Logger.Log(stage + "阶书籍 下级将研读 " + (num2+1) + "<=" + (power / 90) + "   问题难度 " + power + "   " + (num2 > (power / 90)));
                            //Logger.Log(stage + "阶书籍 下级将研读 " + (num2+1) + "<=" + (power / 90) + "   问题难度 " + power + "   " + (num2 > (power / 90)));
                            if ((num2*90) > power)
                            {
                                value = 0;
                                result[1] += "\n<color=#8E8E8EFF>【书籍页数相关】题目难度不足" + power + "/" + (num2 * 90)+ "</color>";
                            }
                        }

                        if (value > 0 && result[0] == "受益良多" && (num2 + 1) <= (power / 90))
                        {
                            value = 2;
                        }


                        // 如果大于0则增加技艺等级
                        if (value > 0)
                        {
                            value = value * Main.settings.ReadLevel;
                            value = SkillFLevel(_skillId, value);
                        }
                    }

                    if (value > 0)
                    {
                        result[1] += "\n\n\n【" + selfName + "抓住了一丝明悟，" + book_name + "研习进度增加了" + value + "页】";
                    }
                    else if(value != -100)
                    {
                        int rand = UnityEngine.Random.Range(0, 4);
                        switch (rand)
                        {
                            case 1:
                                result[1] += "\n\n\n【" + selfName + "差点就抓住了那一丝明悟，却没有把握住机会，可惜。】";
                                break;
                            case 2:
                                result[1] += "\n\n\n【" + opponentName + "有意隐瞒了" + book_name + "的诀窍，" + selfName + "虽有所思却没有收获。】";
                                break;
                            case 3:
                                result[1] += "\n\n\n【" + selfName + "陷入了顿悟之中，却被" + opponentName + "催促赶紧继续比拼而被打断。】";
                                break;
                            default:
                                result[1] += "\n\n\n【" + selfName + "似乎抓住了一丝明悟，但却不甚明朗，始终无法突破。】";
                                break;
                        }
                    }
                    else
                    {
                        result[0] = "领悟技艺";
                        result[1] = selfName + "在比拼中有所领悟，赶紧将心中领悟记下" + "\n\n\n【获得残页书籍" + bname + "】";
                    }
                }
                else
                {
                    if (num2 == 10)
                    {
                        if (opponentBattleValue > (battleValue + 100)) // 深刻理解：虽然《"+book_name+"》已经熟读于心，但是听了"+opponentName+"的讲解之后，有了更深刻的认识。
                        {
                            result[0] = "浅尝辄止";
                            result[1] = selfName + "已读过" + book_name + "，断断续续答上了" + opponentName + "的问题，运用了一些伎俩避过了一些刁钻的问题。";
                        }
                        else if (battleValue > (opponentBattleValue + 100)) // 指点江山：《"+book_name+"》心中已经有了深刻认识，觉得"+opponentName+"讲解颇有不足，为其讲解了此中深意。
                        {
                            result[0] = "对答如流";
                            result[1] = selfName + "已然熟读" + book_name + "，" + opponentName + "听了" + selfName + "的讲解之后频频点头。";
                        }
                        else
                        {
                            result[0] = "谈笑风生";
                            result[1] = selfName + "与" + opponentName + "对答如流，谈笑风生，都对" + book_name + "有着深刻的认识。";
                        }
                    }
                    else if (num < 5)
                    {
                        result[0] = "一窍不通";
                        result[1] = selfName + "听着" + opponentName + "出口成章，侃侃而谈，但" + book_name + "太过高深，完全不知道讲的是啥。";
                    }
                    else
                    {
                        result[0] = "相谈甚欢";
                        result[1] = selfName + "与" + opponentName + "对" + book_name + "达成一致看法，相谈甚欢。";
                    }
                    //if (num2 != 10 && (stage == 1 || num >= 5))
                    if(num2 == 10)
                    {
                        result[1]+= "<color=#8E8E8EFF>【书籍研习进度已满】</color>";
                    }else if(!(stage == 1 || num >= 5))
                    {
                        result[1] += "<color=#8E8E8EFF>【需要"+
                            DateFile.instance.skillDate[int.Parse(DateFile.instance.baseSkillDate[_this.battleSkillTyp][nowSkillIndex], System.Globalization.CultureInfo.InvariantCulture)][0]
                            + "研习至5页以上】</color>";
                    }
                }


                #region 打印能力值

                ////// 打印能力值
                //Logger.Log("书页判断结果： " + stage + " " + num + " " + num2);
                //string de = "actorSkillBattleValue:";
                //foreach (var item in actorSkillBattleValue)
                //{
                //    de += " " + item + " ";
                //}
                //Logger.Log(de);

                //de = "enemySkillBattleValue:";
                //foreach (var item in enemySkillBattleValue)
                //{
                //    de += " " + item + " ";
                //}
                //Logger.Log(de);

                //de = _skillId + " actorSkills:";
                //bool flag = DateFile.instance.actorSkills.ContainsKey(_skillId);
                //if (flag)
                //{
                //    var vvv = DateFile.instance.actorSkills[_skillId];
                //    foreach (var item in vvv)
                //    {
                //        de += " " + item + " ";
                //    }
                //    Logger.Log(de);
                //}

                //de = _skillId + " skillBookPages:";
                //bool flag3 = DateFile.instance.skillBookPages.ContainsKey(_skillId);
                //if (flag3)
                //{
                //    var vvv = DateFile.instance.skillBookPages[_skillId];
                //    foreach (var item in vvv)
                //    {
                //        de += " " + item + " ";
                //    }
                //    Logger.Log(de);
                //}

                //de = _skillId + " skillDate:";
                //bool flag4 = DateFile.instance.skillDate.ContainsKey(_skillId);
                //if (flag4)
                //{
                //    var vvv = DateFile.instance.skillDate[_skillId];
                //    foreach (var item in vvv)
                //    {
                //        de += " " + item.Key + "=" + item.Value + " ";
                //    }
                //    Logger.Log(de);
                //}

                //// 打印七元赋性
                //var seven = ActorMenu.instance.GetActorResources(_actorId);
                //de = " 七元赋性:";
                //for (int i = 0; i < 7; i++)
                //{
                //    de += " " + i + "=" + seven[i] + " ";
                //}
                //Logger.Log(de);

                #endregion
                return result;
            }

            // 提升技艺研读
            static int SkillFLevel(int skillId, int value)
            {
                bool flag8 = !DateFile.instance.skillBookPages.ContainsKey(skillId);
                if (flag8)
                {
                    // 习得新的书籍
                    DateFile.instance.skillBookPages.Add(skillId, new int[10]);

                    AddSkillBook(skillId);
                    return -100;
                }
                if (value > 0)
                {
                    int[] pages = new int[value];
                    int idx = 0;
                    for (int i = 0; i < 10; i++)
                    {
                        if (DateFile.instance.skillBookPages[skillId][i] == 0 && idx < value)
                        {
                            pages[idx++] = i;
                        }
                    }

                    foreach (var pageIndex in pages)
                    {
                        int num8 = DateFile.instance.skillBookPages[skillId][pageIndex];
                        bool flag9 = num8 != 1 && num8 > -100;
                        if (flag9)
                        {
                            int num9 = int.Parse(DateFile.instance.skillDate[skillId][2], System.Globalization.CultureInfo.InvariantCulture);
                            bool flag10 = !DateFile.instance.actorSkills.ContainsKey(skillId);
                            if (flag10)
                            {
                                DateFile.instance.ChangeMianSkill(skillId, 0, 0, true);
                            }
                            DateFile.instance.skillBookPages[skillId][pageIndex] = 1;
                            DateFile.instance.AddActorScore(203, num9 * 100);
                            bool flag11 = DateFile.instance.GetSkillLevel(skillId) >= 100 && DateFile.instance.GetSkillFLevel(skillId) >= 10;
                            if (flag11)
                            {
                                DateFile.instance.AddActorScore(204, num9 * 100);
                            }
                        }
                    }
                    return idx;
                }
                return 0;
            }

            // 提升技艺修习
            static int SkillLevel(int skillId, int value)
            {
                
                int idx = 0;
                return idx;
            }
        }


        static void AddSkillBook(int skillId)
        {
            int itemId = 0;
            foreach (var item in DateFile.instance.presetitemDate)
            {
                if (item.Value.ContainsKey(32))
                {
                    if (int.Parse(item.Value[32]) == skillId) // 32 是对应的功法   35是是否手抄
                    {
                        itemId = item.Key;
                        break;
                    }
                }
            }

            if (itemId > 0)
            {
                int item = DateFile.instance.GetItem(DateFile.instance.MianActorID(), itemId, 1, true);
                if (item > 0)
                {
                    GameData.Items.SetItemProperty(item, 33, "0|0|0|0|0|0|0|0|0|0");
                    GameData.Items.SetItemProperty(item, 901, "3");
                    GameData.Items.SetItemProperty(item, 902, "3");
                }
            }
        }
    }
}