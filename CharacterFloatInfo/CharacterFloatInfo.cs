using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace CharacterFloatInfo
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }

        public bool addonInfo = false;
        public bool shopName = false;
        public bool healthStatus = false;
        public bool workPlace = false;
        public bool workerlist = false;
        public bool talkMessage = false;
        public bool enableTalkShortMode = true;//在对话框只显示部分信息
        public bool enableListShortMode = true;//在工作界面只显示部分信息
        public bool showBest = true; //显示身上品质最高的物品与功法
        public bool showMood = false; //显示心情
        public bool workEfficiency = false; //显示工作效率
        public bool hideShopInfo = true; //不显示商店的详细信息
        public bool hideChameOfChildren = true; //不显示儿童的魅力
        public bool useColorOfTeachingSkill = false; //用可以请教的技艺的颜色显示资质(120=红)
        public bool lifeMessage = false; //人物经历
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
            //GUILayout.BeginHorizontal();
            Main.settings.enableTalkShortMode = GUILayout.Toggle(Main.settings.enableTalkShortMode, "在对话界面只显示部分信息", new GUILayoutOption[0]);
            Main.settings.enableListShortMode = GUILayout.Toggle(Main.settings.enableListShortMode, "在分配工作界面只显示部分信息", new GUILayoutOption[0]);
            Main.settings.addonInfo = GUILayout.Toggle(Main.settings.addonInfo, "显示原始信息", new GUILayoutOption[0]);
            Main.settings.showBest = GUILayout.Toggle(Main.settings.showBest, "显示最佳物品与功法", new GUILayoutOption[0]);
            Main.settings.lifeMessage = GUILayout.Toggle(Main.settings.lifeMessage, "显示人物经历", new GUILayoutOption[0]);
            Main.settings.useColorOfTeachingSkill = GUILayout.Toggle(Main.settings.useColorOfTeachingSkill, "使用可请教的技艺的颜色显示资质", new GUILayoutOption[0]);
            Main.settings.workEfficiency = GUILayout.Toggle(Main.settings.workEfficiency, "显示村民工作效率", new GUILayoutOption[0]);
            Main.settings.workerlist = GUILayout.Toggle(Main.settings.workerlist, "村民分配工作界面启用", new GUILayoutOption[0]);
            Main.settings.talkMessage = GUILayout.Toggle(Main.settings.talkMessage, "对话界面启用", new GUILayoutOption[0]);
            //GUILayout.EndHorizontal();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }


    [HarmonyPatch(typeof(WorldMapSystem), "UpdatePlaceActor", new Type[] {typeof(int), typeof(int)})]
    public static class WorldMapSystem_UpdatePlaceActor_Patch
    {
        public static int index = 0;

        static void Postfix(Transform ___actorHolder)
        {
            if (!Main.enabled)
            {
                return;
            }
            int count = ___actorHolder.childCount;
            for (int i = index; i < count; i++)
            {
                ___actorHolder.GetChild(i).gameObject.AddComponent<PointerEnter>();
            }

            index = count;
        }
    }

    //返回主界面从新计数
    [HarmonyPatch(typeof(SystemSetting), "BackToStartMenu")]
    public static class SystemSetting_BackToStartMenu_Patch
    {
        static void Postfix()
        {
            if (!Main.enabled)
            {
                return;
            }

            WorldMapSystem_UpdatePlaceActor_Patch.index = 0;
        }
    }

    //防止今后更新启用这个函数
    [HarmonyPatch(typeof(WorldMapSystem), "RemoveActor")]
    public static class WorldMapSystem_RemoveActor_Patch
    {
        static void Postfix()
        {
            if (!Main.enabled)
            {
                return;
            }

            WorldMapSystem_UpdatePlaceActor_Patch.index = 0;
        }
    }

    //每次打开窗口之前obj已全部销毁
    [HarmonyPatch(typeof(HomeSystem), "GetActor")]
    public static class HomeSystem_GetActor_Patch
    {
        static void Postfix(Transform ___listActorsHolder)
        {
            if (!Main.enabled || !Main.settings.workerlist)
            {
                return;
            }

            int count = ___listActorsHolder.childCount;
            for (int i = 0; i < count; i++)
            {
                ___listActorsHolder.GetChild(i).gameObject.AddComponent<PointerEnter>();
            }
        }
    }

    [HarmonyPatch(typeof(WindowManage), "WindowSwitch")]
    public static class WindowManage_WindowSwitch_Patch
    {
        public static List<string> actorMassage = new List<string>();
        public static int actorId = 0;
        public enum WindowType
        {
            MapActorList,
            Dialog,
            BuildingWindow
        };
        public static WindowType windowType= WindowType.MapActorList;
        public static void Postfix(GameObject tips, bool on, ref Text ___itemMoneyText, ref Text ___itemLevelText, ref Text ___informationMassage, ref Text ___informationName, ref bool ___anTips)
        {
            if (!Main.enabled)
            {
                return;
            }

            if (tips != null && ___anTips == false && on)
            {
                bool needShow = false;
                int id = -1;
                //建筑/地图左边的列表
                string[] array = tips.name.Split(new char[]
                {
                    ','
                });
                if (array[0] == "Actor")
                {
                    int typ = int.Parse(typeof(WorldMapSystem).GetField("showPlaceActorTyp", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(WorldMapSystem.instance).ToString());
                    if (typ == 1 && WorldMapSystem.instance.choosePlaceId == DateFile.instance.mianPlaceId)
                    {
                        id = int.Parse(array[1]);
                        needShow = true;
                        windowType = WindowType.MapActorList;
                    }
                    if (HomeSystem.instance.buildingWindowOpend)
                    {
                        id = int.Parse(array[1]);
                        needShow = true;
                        windowType = WindowType.BuildingWindow;
                    }
                }
                //对话窗口的人物头像
                else if (array[0] == "FaceHolder" && Main.settings.talkMessage)
                {
                    id = MassageWindow.instance.eventMianActorId;
                    needShow = true;
                    windowType = WindowType.Dialog;
                }

                if (needShow)
                {                   
                    ___itemLevelText.text =SetLevelText(id);
                    ___itemMoneyText.text = SetMoneyText(id);
                    ___informationName.text = SetInfoName(id);
                    ___informationMassage.text = SetInfoMassage(id, tips);
                    ___anTips = true;
                }
            }
        }

        //标题栏左侧小字号文本
        public static string SetLevelText(int id)
        {
            return string.Format("\t{0}({1})", GetAge(id), GetHealth(id));
        }

        //标题栏右侧侧小字号文本
        public static string SetMoneyText(int id)
        {
            return GetChame(id, Main.settings.addonInfo);
        }

        //标题栏
        public static string SetInfoName(int id)
        {
            string text = "";
            int age = GetAge(id);
            text = DateFile.instance.GetActorName(id, true, false);
            string samsaraNmae = GetSamsaraName(id);
            text += samsaraNmae.Length > 0 ? " ◆ " + samsaraNmae : "";
            text += "\n" + GetMood(id) + " • " + GetFame(id);
            if (age > 14) //低于14岁不显示婚姻状况
            {
                text += " • " + GetSpouse(id);
            }

            string shopName = GetShopName(id);
            if (shopName.Length != 0)
            {
                text += " • " + shopName;
            }
            string workDate = GetWorkingData(id);
            if (workDate != null&&Main.settings.workEfficiency)
            {
                text += " • " + workDate;
            }
            string workPlace = GetWorkPlace(id);
            if (workPlace.Length != 0)
            {
                text += " • " + workPlace;
            }


            List<int> list = GetHPSP(id);
            List<int> list1 = GetPoison(id);
            int dmg = Math.Max(list[0] * 100 / list[1], list[2] * 100 / list[3]);
            int dmgtyp = 0;
            if (dmg >= 20) dmgtyp = 1;
            for (int i = 0; i < 6; i++)
            {
                if (list1[i] >= 100)
                {
                    dmgtyp += 2;
                    break;
                }
            }
            switch (dmgtyp)
            {
                case 1:
                    text += DateFile.instance.SetColoer(20010, "\n受伤");
                    break;
                case 2:
                    text += DateFile.instance.SetColoer(20007, "\n中毒");
                    break;
                case 3:
                    text += DateFile.instance.SetColoer(20010, "\n受伤") + "/" + DateFile.instance.SetColoer(20007, "中毒");
                    break;
                default:
                    text += DateFile.instance.SetColoer(20004, "\n健康");
                    break;
            }
            //text += "\n烈毒" + list1[0] + "/郁毒" + list1[1] + "/寒毒" + list1[2] + "/赤毒" + list1[3] + "/腐毒" + list1[4] + "/幻毒" + list1[5];
            return text;
        }

        //文本
        public static string SetInfoMassage(int id, GameObject tips)
        {
            string text = "";
            if (windowType == WindowType.Dialog && Main.settings.enableTalkShortMode)
                return text;
            if (windowType == WindowType.BuildingWindow && Main.settings.enableListShortMode)
                return text;
            text += "\t立场：" + GetGoodness(id) + "\t\t\t轮回：" + GetSamsara(id) + "次";
            if (GetAge(id) > 14)
            {
                text += "\t\t\t子嗣：" + DateFile.instance.GetActorSocial(id, 310, false).Count;
            }

            text += "\n\n\t喜好：" + Gethobby(id, 0);
            text += "\t\t\t厌恶：" + Gethobby(id, 1) + "\n\n";

            for (int i = 0; i < 16; i++)
            {
                if (i < 14)
                {
                    text += string.Format("\t{0}\t\t{1}\n", GetLevel(id, i, 0, Main.settings.addonInfo), GetLevel(id, i, 1, Main.settings.addonInfo));
                }
                else
                {
                    text += string.Format("\t{0}\n", GetLevel(id, i, 0, Main.settings.addonInfo));
                }
            }
            if(Main.settings.showBest)
            {
                text += GetBestItems(id);
                text += GetBestGongfa(id);
            }

            if (Main.settings.lifeMessage && windowType == WindowType.MapActorList)//只在大地图显示经历
            {
                text += GetLifeMessage(id, 3);
            }
            return text;
        }


        //心情
        public static string GetMood(int id)
        {
            return ActorMenu.instance.Color2(DateFile.instance.GetActorDate(id, 4, false));
        }

        //魅力
        public static string GetChame(int id, bool shownoadd)
        {
            // 显示未加成数据 true
            string text = ((int.Parse(DateFile.instance.GetActorDate(id, 11, false)) > 14 || !Main.settings.hideChameOfChildren)
                ? ((int.Parse(DateFile.instance.GetActorDate(id, 8, false)) != 1 || int.Parse(DateFile.instance.GetActorDate(id, 305, false)) != 0)
                    ? DateFile.instance.massageDate[25][int.Parse(DateFile.instance.GetActorDate(id, 14, false)) - 1].Split(new char[]
                    {
                        '|'
                    })[Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(id, 15, true)) / 100, 0, 9)]
                    : DateFile.instance.massageDate[25][5].Split(new char[]
                    {
                        '|'
                    })[1])
                : DateFile.instance.massageDate[25][5].Split(new char[]
                {
                    '|'
                })[0]);
            if (shownoadd)
            {
                text += "（" + ((int.Parse(DateFile.instance.GetActorDate(id, 11, false)) > 14 || !Main.settings.hideChameOfChildren)
                            ? ((int.Parse(DateFile.instance.GetActorDate(id, 8, false)) != 1 || int.Parse(DateFile.instance.GetActorDate(id, 305, false)) != 0)
                                ? DateFile.instance.massageDate[25][int.Parse(DateFile.instance.GetActorDate(id, 14, false)) - 1].Split(new char[]
                                {
                                    '|'
                                })[Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(id, 15, false)) / 100, 0, 9)]
                                : DateFile.instance.massageDate[25][5].Split(new char[]
                                {
                                    '|'
                                })[1])
                            : DateFile.instance.massageDate[25][5].Split(new char[]
                            {
                                '|'
                            })[0]) + "）";
            }

            return text;
        }

        //名誉
        public static string GetFame(int id)
        {
            string text = ActorMenu.instance.Color7(int.Parse(DateFile.instance.GetActorDate(id, 18, true)));
            return text;
        }

        //立场
        public static string GetGoodness(int id)
        {
            string text = DateFile.instance.massageDate[9][0].Split(new char[]
            {
                '|'
            })[DateFile.instance.GetActorGoodness(id)];
            return text;
        }

        //喜好
        public static string Gethobby(int id, int hobby)
        {
            //喜欢 0 讨厌 1
            string text = ((int.Parse(DateFile.instance.GetActorDate(id, 207 + hobby, false)) != 1)
                ? DateFile.instance.massageDate[301][1]
                : DateFile.instance.massageDate[301][0].Split(new char[]
                {
                    '|'
                })[int.Parse(DateFile.instance.GetActorDate(id, 202 + hobby, false))]);
            if (text.Length < 2)
            {
                text += "\t";
            }

            return text;
        }

        //资质
        public static string GetLevel(int id, int index, int gongfa, bool shownoadd)
        {
            // 生活技能 0 战斗技能 1
            //显示未加成数据 true
            int colorCorrect = Main.settings.useColorOfTeachingSkill ? 40 : 20;
            int num = int.Parse(DateFile.instance.GetActorDate(id, 501 + index + 100 * gongfa, true));
            int num2 = int.Parse(DateFile.instance.GetActorDate(id, 501 + index + 100 * gongfa, false));
            string text = DateFile.instance.SetColoer(20002 + Mathf.Clamp((num - colorCorrect) / 10, 0, 8), string.Concat(new object[]
            {
                DateFile.instance.baseSkillDate[index + 101 * gongfa][0],
                DateFile.instance.massageDate[7003][4].Split(new char[]
                {
                    '|'
                })[2],
                WindowManage.instance.Mut(),
                num
            }), false);
            text += num < 10 ? "\t\t" : num < 100 ? "\t" : "";
            if (shownoadd)
            {
                if (num != num2)
                {
                    text += "« \t" + DateFile.instance.SetColoer(20002 + Mathf.Clamp((num - colorCorrect) / 10, 0, 8), num2.ToString());
                    text += num2 < 10 ? "\t\t" : num2 < 100 ? "\t" : "";
                }
                else
                {
                    text += "\t\t\t";
                }
            }
            else
            {
                text += "\t\t";
            }

            return text;
        }

        //年龄
        public static int GetAge(int id)
        {
            int age = (int.Parse(DateFile.instance.GetActorDate(id, 11, false)));
            return age;
        }

        //健康值
        public static string GetHealth(int id)
        {
            int health = ActorMenu.instance.Health(id);
            int maxhealth = ActorMenu.instance.MaxHealth(id);
            string peopleHealthText = string.Format("{0}{1}</color> / {2}", ActorMenu.instance.Color3(health, maxhealth), health, maxhealth);
            return peopleHealthText;
        }

        //红蓝条
        public static List<int> GetHPSP(int id)
        {
            int Hp = ActorMenu.instance.Hp(id, false);
            int maxHp = ActorMenu.instance.MaxHp(id);
            int Sp = ActorMenu.instance.Sp(id, false);
            int maxSp = ActorMenu.instance.MaxHp(id);
            List<int> list = new List<int>
            {
                Hp,
                maxHp,
                Sp,
                maxSp
            };
            return list;
        }

        //毒素
        public static List<int> GetPoison(int id)
        {
            List<int> list = new List<int> {};

            for (int i = 0; i < 6; i++)
            {
                int num = int.Parse(DateFile.instance.GetActorDate(id, 51 + i, false));
       
                list.Add(num);
            }

            return list;
        }

        //商会
        public static string GetShopName(int id)
        {
            string text = "";
            if (GetGangLevelText(id) == "商人")
            {
                int typ = int.Parse(DateFile.instance.GetGangDate(int.Parse(DateFile.instance.GetActorDate(id, 9, false)), 16));
                text = string.Format("{0}", DateFile.instance.storyShopDate[typ][0], DateFile.instance.massageDate[11][2]);
                if (!Main.settings.hideShopInfo)
                {
                    //花费等级
                    int moneyCost = 250;
                    switch (DateFile.instance.GetActorGoodness(id))
                    {
                        case 1:
                            moneyCost = 200;
                            break;

                        case 2:
                            moneyCost = 0xe1;
                            break;

                        case 3:
                            moneyCost = 0x113;
                            break;

                        case 4:
                            moneyCost = 300;
                            break;
                    }

                    //商品等级
                    int level = DateFile.instance.GetActorValue(id, 0x1fa, false) * 10;
                    //商队
                    int shopTyp = int.Parse(DateFile.instance.GetGangDate(typ, 0x10));
                    //商品等级Plus
                    int newShopLevel = DateFile.instance.storyShopLevel[shopTyp] + level;
                    //实际花费
                    int num = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), id, true, false);
                    int shopSellCost = 30 + (num * 5);
                    int shopSystemCost = moneyCost - (num * 15);

                    text += "(Lv:" + DateFile.instance.storyShopLevel[shopTyp].ToString() + "+" + level.ToString() + ",Cost:" + shopSystemCost.ToString() + "/" + shopSellCost.ToString() + ")";
                }
            }

            return text;
        }

        //人物在组织中等级ID
        public static int GetGangLevelId(int id)
        {
            int num2 = int.Parse(DateFile.instance.GetActorDate(id, 19, false));
            int num3 = int.Parse(DateFile.instance.GetActorDate(id, 20, false));
            int gangValueId = DateFile.instance.GetGangValueId(num2, num3);
            return gangValueId;
        }
        //人物在组织中等级名称
        public static string GetGangLevelText(int id)
        {
            int num2 = int.Parse(DateFile.instance.GetActorDate(id, 19, false));
            int num3 = int.Parse(DateFile.instance.GetActorDate(id, 20, false));
            int key2 = (num3 >= 0) ? 1001 : (1001 + int.Parse(DateFile.instance.GetActorDate(id, 14, false)));
            int gangValueId = DateFile.instance.GetGangValueId(num2, num3);
            string gang = DateFile.instance.presetGangGroupDateValue[gangValueId][key2];
            return gang;
        }
        //人物身上最高级功法获取
        public static string GetBestGongfa(int id)
        {

            List<int> gongFas = new List<int>(DateFile.instance.actorGongFas[id].Keys);
            if (gongFas.Count == 0)
            {
                return "\n\t最佳功法: 他还没来得及学";
            }
            string bestName = "";
            int bestLevel = 0;
            int count = 1;
            for (int i = 0; i < gongFas.Count; i++)
            {
                int gongFaId = gongFas[i];
                string gongFaName = DateFile.instance.gongFaDate[gongFaId][0];
                string level = DateFile.instance.gongFaDate[gongFaId][2];
                int intLevel = int.Parse(level);
                if (intLevel > bestLevel)
                {
                    bestLevel = intLevel;
                    bestName = gongFaName;
                    count = 1;
                }
                else
                {
                    if(intLevel == bestLevel)
                    {
                        count += 1;
                    }
                }
               // Main.Logger.Log(string.Format("gongfas-{0}-{1}-{2}-{3}", i, gongFaId, gongFaName,level));
            }
            //Main.Logger.Log(string.Format("gongfas-final-{0}-{1}", bestLevel,bestName));
            bestName = DateFile.instance.SetColoer(20001 + bestLevel, bestName);
            string after = "";
            if (count > 1)
            {
                after = " 等" + count + "种."; 
            }
      
            return "\n\t最佳功法: " + bestName + after;
        }

        //人物身上的最佳物品获取
        public static string GetBestItems(int id)
        {
            List<int> list = new List<int>(ActorMenu.instance.GetActorItems(id, 0).Keys);
            if (list.Count == 0)
            {
                return "\n\t最佳物品: 这是个穷光蛋";
            }
            string bestName = "";
            int bestLevel = 0;
            int count = 1;
            for (int i = 0; i < list.Count; i++)
            {
                int itemId = list[i];
                string itemName = DateFile.instance.GetItemDate(itemId, 0, true);
                string level = DateFile.instance.GetItemDate(itemId, 8, true);
                int intLevel = int.Parse(level);
                if (intLevel > bestLevel)
                {
                    bestLevel = intLevel;
                    bestName = itemName;
                    count = 1;
                }
                else
                {
                    if (intLevel == bestLevel)
                    {
                        count += 1;
                    }
                }
            }
            //获得的物品名格式为xx\n下九品，需去掉后缀，改用颜色进行标识。
            int index = bestName.IndexOf("\n");
            bestName = bestName.Substring(0, index);
            bestName = DateFile.instance.SetColoer(20001 + bestLevel, bestName);
            string after = "";
            if (count > 1)
            {
                after = " 等" + count + "种.";
            }

            return "\n\t最佳物品: " + bestName + after;
        }

        //村民工作地点
        public static string GetWorkPlace(int id)
        {
            string text = "";
            if (DateFile.instance.ActorIsWorking(id) != null)
            {
                int[] place = DateFile.instance.ActorIsWorking(id);
                List<int> list = new List<int>(DateFile.instance.actorsWorkingDate[place[0]][place[1]].Keys);
                for (int i = 0; i < list.Count; i++)
                {
                    int key = list[i];
                    if (DateFile.instance.actorsWorkingDate[place[0]][place[1]][key] == id)
                    {
                        int buildid = DateFile.instance.homeBuildingsDate[place[0]][place[1]][key][0];
                        text = DateFile.instance.basehomePlaceDate[buildid][0];
                    }
                }
            }

            return text;
        }

        //转世次数
        public static string GetSamsara(int id)
        {
            if (DateFile.instance.HaveLifeDate(id, 801))
            {
                int count = DateFile.instance.GetLifeDateList(id, 801).Count;
                int coloer = (count < 1) ? 20002 : 20003;
                return DateFile.instance.SetColoer(coloer, count.ToString(), false);
            }

            return "0";
        }

        //前世名字
        public static string GetSamsaraName(int id)
        {
            string text = "";
            if (DateFile.instance.HaveLifeDate(id, 801))
            {
                List<int> list = DateFile.instance.GetLifeDateList(id, 801);
                int samsaraId = list[list.Count - 1];
                int levelId = GetGangLevelId(samsaraId);
                text = string.Format("{0}", DateFile.instance.GetActorName(samsaraId, true, false));
                //1为太吾村村民为特殊。2-10为无名邪道不会转世。太吾村村民不再特殊处理，跟人物姓名同一颜色即为村民
                if (levelId > 10)
                {
                    if (levelId == 99) //99为太吾传人，给予特殊的明黄色。
                    {
                        text = DateFile.instance.SetColoer(10005, text);
                    }
                    else   //其他的按照品级给予颜色
                    {
                        text = DateFile.instance.SetColoer(20011 - levelId % 10, text);
                    }
                }
            }
            return text;
        }

        //婚姻状况
        public static string GetSpouse(int id)
        {
            List<int> actorSocial = DateFile.instance.GetActorSocial(id, 309, false);
            List<int> actorSocial2 = DateFile.instance.GetActorSocial(id, 309, true);
            bool flag = actorSocial2.Count == 0;
            string result;
            if (flag)
            {
                result = DateFile.instance.SetColoer(20004, "未婚", false);
            }
            else
            {
                bool flag2 = actorSocial.Count == 0;
                if (flag2)
                {
                    result = DateFile.instance.SetColoer(20007, "丧偶", false);
                }
                else
                {
                    result = DateFile.instance.SetColoer(20010, "已婚", false);
                }
            }

            return result;
        }

        //人物经历
        public static string GetLifeMessage(int id, int shownum) //shownum控制显示几条信息
        {
            string text = "";
            int index = 0;
            int count = 0;
            if (id != actorId)
            {
                actorMassage.Clear();
                int num = DateFile.instance.MianActorID();
                actorMassage.Add(string.Format("\n{0}{1}{2}{3}{4}\n", new object[]
                {
                    DateFile.instance.massageDate[8010][1].Split(new char[]
                    {
                        '|'
                    })[0],
                    DateFile.instance.SetColoer(10002, DateFile.instance.solarTermsDate[int.Parse(DateFile.instance.GetActorDate(id, 25, false))][102], false),
                    DateFile.instance.massageDate[8010][1].Split(new char[]
                    {
                        '|'
                    })[1],
                    DateFile.instance.GetActorName(id, false, true),
                    DateFile.instance.massageDate[8010][1].Split(new char[]
                    {
                        '|'
                    })[2]
                }));
                if (DateFile.instance.actorLifeMassage.ContainsKey(id))
                {
                    count = DateFile.instance.actorLifeMassage[id].Count;
                    index = count >= shownum ? count - shownum : 0;
                    for (int i = index; i < count; i++)
                    {
                        int[] array = DateFile.instance.actorLifeMassage[id][i];
                        int key2 = array[0];
                        string[] array2 = DateFile.instance.actorMassageDate[key2][2].Split(new char[]
                        {
                            '|'
                        });
                        string[] array3 = DateFile.instance.actorMassageDate[key2][99].Split(new char[]
                        {
                            '|'
                        });
                        List<string> list = new List<string>
                        {
                            DateFile.instance.massageDate[16][1] + DateFile.instance.SetColoer(10002, array[1].ToString(), false) + DateFile.instance.massageDate[16][3],
                            DateFile.instance.SetColoer(20002, DateFile.instance.solarTermsDate[array[2]][0], false)
                        };

                        list.Add(DateFile.instance.SetColoer(10001, DateFile.instance.GetNewMapDate(array[3], array[4], 98) + DateFile.instance.GetNewMapDate(array[3], array[4], 0), false));
                        for (int j = 0; j < array3.Length; j++)
                        {
                            list.Add(array3[j]);
                        }

                        for (int k = 5; k < array.Length; k++)
                        {
                            int num2 = array[k];
                            switch (int.Parse(array2[k - 5]))
                            {
                                case 0:
                                    list.Add(DateFile.instance.SetColoer((int.Parse(DateFile.instance.GetActorDate(num2, 26, false)) <= 0) ? 10002 : 20010, DateFile.instance.GetActorName(num2, false, false), false));
                                    break;
                                case 1:
                                    list.Add(DateFile.instance.massageDate[10][0].Split(new char[]
                                    {
                                        '|'
                                    })[0] + DateFile.instance.SetColoer(20001 + int.Parse(DateFile.instance.GetItemDate(num2, 8, true)), DateFile.instance.GetItemDate(num2, 0, false), false) + DateFile.instance.massageDate[10][0].Split(new char[]
                                    {
                                        '|'
                                    })[1]);
                                    break;
                                case 2:
                                    list.Add(DateFile.instance.SetColoer(20001 + int.Parse(DateFile.instance.gongFaDate[num2][2]), DateFile.instance.massageDate[10][0].Split(new char[]
                                    {
                                        '|'
                                    })[0] + DateFile.instance.gongFaDate[num2][0] + DateFile.instance.massageDate[10][0].Split(new char[]
                                    {
                                        '|'
                                    })[1], false));
                                    break;
                                case 3:
                                    list.Add(DateFile.instance.SetColoer(20008, DateFile.instance.resourceDate[num2][0], false));
                                    break;
                                case 4:
                                    list.Add(DateFile.instance.SetColoer(20008, DateFile.instance.GetGangDate(num2, 0), false));
                                    break;
                                case 5:
                                    list.Add(DateFile.instance.SetColoer(20011 - Mathf.Abs(int.Parse(DateFile.instance.GetActorDate(id, 20, false))), DateFile.instance.presetGangGroupDateValue[Mathf.Abs(num2)][(num2 <= 0) ? (1001 + int.Parse(DateFile.instance.GetActorDate(id, 14, false))) : 1001], false));
                                    break;
                            }
                        }

                        actorMassage.Add(string.Format(" {0}{1}：" + DateFile.instance.actorMassageDate[key2][1] + "\n", list.ToArray()));
                    }
                }

                int num3 = int.Parse(DateFile.instance.GetActorDate(id, 26, false));
                if (num3 > 0)
                {
                    actorMassage.Add(string.Format("■ {0}{1}{2}\n", DateFile.instance.massageDate[8010][2].Split(new char[]
                    {
                        '|'
                    })[0], DateFile.instance.SetColoer(10002, DateFile.instance.GetActorDate(id, 11, false), false), DateFile.instance.massageDate[8010][2].Split(new char[]
                    {
                        '|'
                    })[1]));
                }

                actorId = id;
            }

            count = actorMassage.Count;
            index = count >= shownum ? count - shownum : 0;
            for (int i = index; i < count; i++)
            {
                text += actorMassage[i];
            }

            return text;
        }
        //工作效率,null代表无法获得
        public static string GetWorkingData(int workerId)
        {
            if (HomeSystem.instance == null)
                return null;
            if (!HomeSystem.instance.buildingWindowOpend)
                return null;
            int buildingIndex = HomeSystem.instance.homeMapbuildingIndex;
            int partId = -1;
            int placeId = -1;
            List<int> list = new List<int>(DateFile.instance.baseHomeDate.Keys);
            foreach (var x_pair in DateFile.instance.baseHomeDate)
            {
                int x = x_pair.Key;
                foreach (var y_pair in x_pair.Value)
                {
                    int y = y_pair.Key;
                    if (DateFile.instance.baseHomeDate[x][y] != 0)
                    {
                        partId = x;
                        placeId = y;
                        break;
                    }
                }
                if (partId >= 0)
                    break;
            }
            if (partId < 0 || placeId < 0)
                return null;
            int[] array = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int unknown = int.Parse(DateFile.instance.basehomePlaceDate[array[0]][33]);//所需资质的序号
            int mood = int.Parse(DateFile.instance.GetActorDate(workerId, 4, false));
            int favorLvl = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), workerId, true, false);//[0-6]
            int moodFavorAddup = 40 + favorLvl * 10;
            if (mood <= 0)
            {
                moodFavorAddup -= 30;
            }
            else if (mood <= 20)
            {
                moodFavorAddup -= 20;
            }
            else if (mood <= 40)
            {
                moodFavorAddup -= 10;
            }
            else if (mood >= 100)
            {
                moodFavorAddup += 30;
            }
            else if (mood >= 80)
            {
                moodFavorAddup += 20;
            }
            else if (mood >= 60)
            {
                moodFavorAddup += 10;
            }
            int num5 = (unknown <= 0) ? 0 : int.Parse(DateFile.instance.GetActorDate(workerId, unknown, true));
            if (unknown == 18)
            {
                num5 += 100;
            }
            int num6 = Mathf.Max(int.Parse(DateFile.instance.basehomePlaceDate[array[0]][51]) + (array[1] - 1), 1);
            num5 = num5 * Mathf.Max(moodFavorAddup, 0) / 100;

            int efficiency = Mathf.Clamp(num5 * 100 / num6, 50, 200);
            int total = int.Parse(DateFile.instance.basehomePlaceDate[array[0]][91]);
            if (total > 0)
                return string.Format("{0}%", efficiency * 100 / total);
            else
                return null;
        }
    }
}
