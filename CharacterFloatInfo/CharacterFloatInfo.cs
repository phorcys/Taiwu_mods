using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public bool showCharacteristic = true; //人物特性
        public bool showIV = false; //显示被隐藏了的人物特性
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
            Main.settings.addonInfo = GUILayout.Toggle(Main.settings.addonInfo, "显示原始信息的差异", new GUILayoutOption[0]);
            Main.settings.showBest = GUILayout.Toggle(Main.settings.showBest, "显示最佳物品与功法", new GUILayoutOption[0]);
            Main.settings.lifeMessage = GUILayout.Toggle(Main.settings.lifeMessage, "显示人物经历", new GUILayoutOption[0]);
            Main.settings.showCharacteristic = GUILayout.Toggle(Main.settings.showCharacteristic, "显示人物特性", new GUILayoutOption[0]);
            Main.settings.showIV = GUILayout.Toggle(Main.settings.showIV, "显示被隐藏了的人物特性", new GUILayoutOption[0]);
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


    [HarmonyPatch(typeof(WorldMapSystem), "UpdatePlaceActor", new Type[] { typeof(int), typeof(int) })]
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
        public static int lastActorID = 0;
        public enum WindowType
        {
            MapActorList,
            Dialog,
            BuildingWindow,
            TeamActor,
        };
        public static WindowType windowType = WindowType.MapActorList;
        public static void Postfix(bool on, GameObject tips, ref Text ___itemMoneyText, ref Text ___itemLevelText, ref Text ___informationMassage, ref Text ___informationName, ref bool ___anTips, ref int ___tipsW)
        {
            if (!on || !Main.enabled || ActorMenu.instance == null || tips == null) return;

            bool needShow = false;
            int id = -1;

            string[] array = tips.name.Split(',');

            //大地圖下面的太吾自己的頭像
            if (array[0] == "PlayerFaceButton")
            {
                id = DateFile.instance.mianActorId;
                needShow = true;
                windowType = WindowType.TeamActor;
            }
            else
            //大地圖下面的隊友頭像
            if (tips.tag == "TeamActor")
            {
                id = DateFile.instance.acotrTeamDate[array[1].Length > 0 ? int.Parse(array[1]) : 0];
                needShow = id>0?true:false;
                windowType = WindowType.TeamActor;
            }
            else
            //建筑/地图左边的列表
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
                ___tipsW = 680;
                ___itemLevelText.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 0);
                ___itemMoneyText.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 0);

                ___itemLevelText.text = SetLevelText(id);
                ___itemMoneyText.text = SetMoneyText(id);
                ___informationName.text = SetInfoName(id);
                ___informationMassage.text = SetInfoMessage(id, ref ___tipsW);
                ___anTips = true;
            }
            else
            {
                Transform resourceHolder = WindowManage.instance.informationWindow.transform.Find("ResourceHolder");
                if (resourceHolder != null)
                {
                    UnityEngine.Object.Destroy(resourceHolder.gameObject);
                }
                Transform actorFeatureHolder = WindowManage.instance.informationWindow.transform.Find("ActorFeatureHolder");
                if (actorFeatureHolder != null)
                {
                    if (actorFeatureHolder.childCount > 0)
                    {
                        for (int i = 0; i < actorFeatureHolder.childCount; i++)
                            UnityEngine.Object.Destroy(actorFeatureHolder.GetChild(i).gameObject);
                    }
                    UnityEngine.Object.Destroy(actorFeatureHolder.gameObject);
                }
            }
        }

        //标题栏左侧小字号文本
        public static string SetLevelText(int id)
        {
            return string.Format("岁数: {0}\n寿命: {1}", GetAge(id), GetHealth(id));
        }

        //标题栏右侧侧小字号文本
        public static string SetMoneyText(int id)
        {
            return GetActorGang(id) + GetGangLevelColorText(id) + "\n" + GetChame(id);
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
            if (age > ConstValue.actorMinAge) //低于14岁不显示婚姻状况
            {
                text += " • " + GetSpouse(id);
            }

            string shopName = GetShopName(id);
            if (shopName.Length != 0)
            {
                text += " • " + shopName;
            }
            string workDate = GetWorkingData(id);
            if (workDate != null && Main.settings.workEfficiency)
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
                if (list1[i] >= 50)
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

            return text;
        }

        //文本
        public static string SetInfoMessage(int id, ref int ___tipsW)
        {
            return SetInfoMessage1(id) + "\n" + SetInfoMessage2(id, ref ___tipsW) + SetInfoMessage3(id) + SetInfoMessage4(id) + SetInfoMessage5(id);
        }

        public static string SetInfoMessage1(int id)
        {
            string text = "";
            if (windowType == WindowType.Dialog && Main.settings.enableTalkShortMode)
                return text;
            if (windowType == WindowType.BuildingWindow && Main.settings.enableListShortMode)
                return text;
            text += "立场：" + GetGoodness(id) + "\t\t\t轮回：" + GetSamsara(id);
            if (GetAge(id) > ConstValue.actorMinAge)
            {
                text += "\t\t\t子嗣：" + DateFile.instance.GetActorSocial(id, 310, false).Count;
            }

            text += "\t\t\t喜好：" + Gethobby(id, 0);
            text += "\t\t\t厌恶：" + Gethobby(id, 1);
            return text;
        }

        // 個人特性
        public static string SetInfoMessage2(int id, ref int ___tipsW)
        {
            string text = "";
            if (Main.settings.showCharacteristic)
            {
                Transform actorFeatureHolder = WindowManage.instance.informationWindow.transform.Find("ActorFeatureHolder");
                if (actorFeatureHolder == null)
                {
                    actorFeatureHolder = UnityEngine.Object.Instantiate(ActorMenu.instance.actorFeatureHolder, new Vector3(22f, -125f, 1), Quaternion.identity);
                    actorFeatureHolder.name = "ActorFeatureHolder";
                    actorFeatureHolder.SetParent(WindowManage.instance.informationWindow.GetComponent<RectTransform>(), false);
                    actorFeatureHolder.localScale = new Vector3(.73f, .73f, 1);
                    actorFeatureHolder.GetComponent<RectTransform>().sizeDelta = new Vector2(800f, 0);
                }
                else if (actorFeatureHolder.childCount > 0)
                {
                        for (int i = 0; i < actorFeatureHolder.childCount; i++)
                        {
                            actorFeatureHolder.GetChild(i).gameObject.SetActive(false);//destroy不会立即销毁，原子节点全部禁用
                            UnityEngine.Object.Destroy(actorFeatureHolder.GetChild(i).gameObject);
                        }
                }

                text += "\n\n\n";
                List<int> featureIDs = DateFile.instance.GetActorFeature(id);
                int shown = 0;
                foreach (int featureID in featureIDs)
                    if (DateFile.instance.actorFeaturesDate[featureID][95] != "1" || Main.settings.showIV) //判斷是否顯示隱藏的特性
                    {

                        shown++;
                        GameObject actorFeature = UnityEngine.Object.Instantiate(ActorMenu.instance.actorFeature, Vector3.zero, Quaternion.identity);
                        actorFeature.transform.SetParent(actorFeatureHolder, false);
                        actorFeature.name = "ActorFeatureIcon," + featureID;
                        actorFeature.transform.Find("ActorFeatureNameText").GetComponent<Text>().text = DateFile.instance.actorFeaturesDate[featureID][0];
                        Transform ActorFeatureStarHolder = actorFeature.transform.Find("ActorFeatureStarHolder");

                        string att = DateFile.instance.actorFeaturesDate[featureID][1];
                        if (att.IndexOf('|') > -1 || att != "0") foreach (string j in att.Split('|'))
                            {
                                GameObject gameObject2 = UnityEngine.Object.Instantiate(
                                    ActorMenu.instance.actorAttackFeatureStarIcons[int.Parse(j) > 0 ? int.Parse(j) - 1 : 3], Vector3.zero, Quaternion.identity);
                                gameObject2.transform.SetParent(ActorFeatureStarHolder, false);
                            }
                        string def = DateFile.instance.actorFeaturesDate[featureID][2];
                        if (def.IndexOf('|') > -1 || def != "0") foreach (string j in def.Split('|'))
                            {
                                GameObject gameObject2 = UnityEngine.Object.Instantiate(
                                    ActorMenu.instance.actorDefFeatureStarIcons[int.Parse(j) > 0 ? int.Parse(j) - 1 : 3], Vector3.zero, Quaternion.identity);
                                gameObject2.transform.SetParent(ActorFeatureStarHolder, false);
                            }
                        string spd = DateFile.instance.actorFeaturesDate[featureID][3];
                        if (spd.IndexOf('|') > -1 || spd != "0") foreach (string j in spd.Split('|'))
                            {
                                GameObject gameObject2 = UnityEngine.Object.Instantiate(
                                    ActorMenu.instance.actorPlanFeatureStarIcons[int.Parse(j) > 0 ? int.Parse(j) - 1 : 3], Vector3.zero, Quaternion.identity);
                                gameObject2.transform.SetParent(ActorFeatureStarHolder, false);
                            }
                        if (att == "0" && def == "0" && spd == "0")
                        {
                            UnityEngine.Object.Instantiate(ActorMenu.instance.actorAttackFeatureStarIcons[3], Vector3.zero, Quaternion.identity).transform.SetParent(ActorFeatureStarHolder, false);
                            UnityEngine.Object.Instantiate(ActorMenu.instance.actorDefFeatureStarIcons[3], Vector3.zero, Quaternion.identity).transform.SetParent(ActorFeatureStarHolder, false);
                            UnityEngine.Object.Instantiate (ActorMenu.instance.actorPlanFeatureStarIcons[3], Vector3.zero, Quaternion.identity).transform.SetParent(ActorFeatureStarHolder, false);
                        }
                    }
                    Component[] componentsInChildren = WindowManage.instance.informationWindow.GetComponentsInChildren<Component>();
                    foreach (Component component2 in componentsInChildren)
                    {
                        if (component2 is Graphic)
                        {
                            (component2 as Graphic).CrossFadeAlpha(1f, 0.2f, true);
                        }
                    }
                    if (shown > 7) ___tipsW += (shown - 7) * 90;
            }
            return text;
        }

        // 才能
        public static string SetInfoMessage3(int id)
        {
            string text = "\n";
            foreach (int i in DateFile.instance.baseSkillDate.Keys)
            {
                var canStudy =
                text += CanTeach(id, i) ? "※" : "　";
                text += GetLevel(id, i, Main.settings.addonInfo);
                text += i % 4 == (i < 100 ? 3 : 0) ? "\n" : "\t\t";
                if (i == 15) text += "\n";
            }
            return text + "\n";
        }

        // 根據NPC的門派或職業,配斷能否傳授你這生活藝能
        public static bool CanTeach(int actorID, int skillID)
        {
            int eventID;
            List<int> eventIDs;

            if (skillID >= 0 && skillID <= 5)
                eventID = 931900001 + skillID;
            else if (skillID == 6 || skillID == 7)
                eventID = 932200001 + skillID - 6;
            else if (skillID == 8 || skillID == 9)
                eventID = 932900001 + skillID - 8;
            else if (skillID == 10 || skillID == 11)
                eventID = 932200003 + skillID - 10;
            else if (skillID >= 12 && skillID <= 15)
                eventID = 932300001 + skillID - 12;
            else
                return false;

            int gangValueId = GetGangLevelId(actorID);
            bool isVillager = gangValueId < 100;

            if (isVillager)
            {
                eventIDs = DateFile.instance.presetGangGroupDateValue[gangValueId][818].Split('|').Select(int.Parse).ToList();
                if (eventIDs.Count() > 1)  //大夫
                { }
                else if (eventIDs[0] == 0)  // 無教技藝
                { }
                else
                {
                    int messageID = int.Parse(DateFile.instance.eventDate[eventIDs[0]][7]);
                    eventIDs = DateFile.instance.eventDate[messageID][5].Split('|').Select(int.Parse).ToList();
                }
            }
            else
            {
                eventIDs = DateFile.instance.presetGangGroupDateValue[gangValueId][813].Split('|').Select(int.Parse).ToList();
            }
            return eventIDs.Contains(eventID);
        }

        // 最佳 裝備 & 物品 & 武功
        public static string SetInfoMessage4(int id)
        {
            string text = "";

            text += "\n" + GetResource(id);

            if (Main.settings.showBest)
            {
                text += "\n" + GetEquipments(id);
                text += "\n" + GetBestItems(id);
                text += "\n" + GetBestGongfa(id);
            }
            return text;
        }

        // 近期事件
        public static string SetInfoMessage5(int id)
        {
            string text = "";
            if (Main.settings.lifeMessage) // 只要用者設定了就顯示. 
            {
                text += "\n" + GetLifeMessage(id, 3) + "\n";
            }
            return text;
        }

        //心情
        public static string GetMood(int id)
        {
            return ActorMenu.instance.Color2(DateFile.instance.GetActorDate(id, 4, false));
        }

        //魅力
        public static string GetChame(int id)
        {
            int actorChame = Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(id, 15, true)) / 100, 0, 9);
            int actorChameDiff = int.Parse(DateFile.instance.GetActorDate(id, 15, true)) - int.Parse(DateFile.instance.GetActorDate(id, 15, false));
            bool isChild = int.Parse(DateFile.instance.GetActorDate(id, 11, false)) <= ConstValue.actorMinAge;
            bool hasWearing = int.Parse(DateFile.instance.GetActorDate(id, 8, false)) != 1 || int.Parse(DateFile.instance.GetActorDate(id, 305, false)) != 0;
            string text = isChild && Main.settings.hideChameOfChildren ?
                DateFile.instance.massageDate[25][5].Split('|')[0] : (hasWearing ?
                    DateFile.instance.massageDate[25][int.Parse(DateFile.instance.GetActorDate(id, 14, false)) - 1].Split('|')[actorChame] :
                    DateFile.instance.massageDate[25][5].Split('|')[1]
                );
            if (Main.settings.addonInfo && !isChild && actorChameDiff != 0)
            { // 显示未加成数据 true
                text += " <color=#606060FF>" + (actorChameDiff > 0 ? "+" : "") + actorChameDiff + "</color>";
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
            string text = DateFile.instance.massageDate[9][0].Split('|')[DateFile.instance.GetActorGoodness(id)];
            return text;
        }

        //喜好
        public static string Gethobby(int id, int hobby)
        {
            //喜欢 0 讨厌 1
            string text = ((int.Parse(DateFile.instance.GetActorDate(id, 207 + hobby, false)) != 1)
                ? DateFile.instance.massageDate[301][1]
                : DateFile.instance.massageDate[301][0].Split('|')[int.Parse(DateFile.instance.GetActorDate(id, 202 + hobby, false))]);
            if (text.Length < 2)
            {
                text += "\t";
            }

            return text;
        }

        //资质
        public static string GetLevel(int id, int index, bool shownoadd)
        {
            int colorCorrect = Main.settings.useColorOfTeachingSkill ? 40 : 20;
            int num = int.Parse(DateFile.instance.GetActorDate(id, 501 + index, true));
            int num2 = num - int.Parse(DateFile.instance.GetActorDate(id, 501 + index, false));
            shownoadd = shownoadd && num2 != 0;
            string text = DateFile.instance.SetColoer(20002 + Mathf.Clamp((num - colorCorrect) / 10, 0, 8),
                string.Format("{0}{1,3}{2}<color=#606060ff>{3,3}</color>{4}",
                    DateFile.instance.baseSkillDate[index][0],
                    num.ToString(), (num < 10 ? " " : "") + (num < 100 ? "  " : ""),
                    shownoadd ? (num2 < 0 ? "" : "+") + num2.ToString() : "    ", shownoadd && Math.Abs(num2) < 10 ? " " : ""));
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
            int totalAge = GetAge(id) + maxhealth;
            int ageReduced = health - maxhealth;
            int ageGrade = Mathf.Clamp((int)Math.Floor((decimal)totalAge / 10), 0, 8);
            return string.Format("{0}<color=red>{1}</color>",
                DateFile.instance.SetColoer(20001 + ageGrade, totalAge.ToString()),
                ageReduced != 0 ? ageReduced.ToString() : ""
                );
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

            List<int> list = new List<int> { };

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

        // 所屬地
        public static string GetActorGang(int key)
        {
            string text = "";
            int num = int.Parse(DateFile.instance.GetActorDate(key, 19, false));
            text += DateFile.instance.GetGangDate(num, 0);
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

        public static string GetGangLevelColorText(int id)
        {
            int grade = Mathf.Abs(int.Parse(DateFile.instance.GetActorDate(id, 20, false)));
            return DateFile.instance.SetColoer(20011 - grade, GetGangLevelText(id));
        }

        //人物身上裝備
        public static string GetEquipments(int id)
        {
            List<string> autorEquipments = new List<string> { };
            for (int i = 301; i <= 310; i++)
            {
                int itemID = int.Parse(DateFile.instance.GetActorDate(id, i, false));
                if (itemID != 0)
                {
                    autorEquipments.Add(GetItemColorName(itemID));
                }
            }
            return "人物裝備: " + (autorEquipments.Count() == 0 ?
                DateFile.instance.SetColoer(20002, GetGenderTA(id) + "赤身裸体在你眼前") :
                string.Join(" / ", autorEquipments.ToArray())) + "\n";
        }

        //人物身上最高级功法获取
        public static string GetBestGongfa(int id)
        {
            List<int> gongFas = new List<int>(DateFile.instance.actorGongFas[id].Keys);
            if (gongFas.Count == 0)
            {
                return "最佳功法: " + GetGenderTA(id) + "还没来得及学";
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
                    if (intLevel == bestLevel)
                    {
                        bestName += " / " + gongFaName;
                        count += 1;
                    }
                }
            }
            return "最佳功法: " + DateFile.instance.SetColoer(20001 + bestLevel, bestName) + "\n";
        }

        //人物身上的最佳物品获取
        public static string GetBestItems(int id)
        {
            List<string> bestItems = new List<string> { };
            int bestGrade = 0;

            List<int> list = new List<int>(ActorMenu.instance.GetActorItems(id, 0).Keys);
            foreach (int itemID in list)
            {
                string itemName = GetItemColorName(itemID);
                int itemGrade = int.Parse(DateFile.instance.GetItemDate(itemID, 8, false));
                if (itemGrade > bestGrade)
                {
                    bestGrade = itemGrade;
                    bestItems = new List<string> { itemName };
                }
                else if (itemGrade == bestGrade)
                {
                    if (!bestItems.ToList().Contains(itemName))
                    {
                        bestItems.Add(itemName);
                    }
                }
            }
            return "最佳物品: " + (bestItems.Count() == 0 ?
                DateFile.instance.SetColoer(20002, GetGenderTA(id) + "是个穷光蛋") :
                string.Join(" / ", bestItems.ToArray())) + "\t" + GetItemWeight(id) + "\n";
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
            if (id != lastActorID)
            {
                actorMassage.Clear();
                int num = DateFile.instance.MianActorID();
                actorMassage.Add(string.Format("{0}{1}{2}{3}{4}", new object[]
                {
                    DateFile.instance.massageDate[8010][1].Split('|')[0],
                    DateFile.instance.SetColoer(10002, DateFile.instance.solarTermsDate[int.Parse(DateFile.instance.GetActorDate(id, 25, false))][102], false),
                    DateFile.instance.massageDate[8010][1].Split('|')[1],
                    DateFile.instance.GetActorName(id, false, true),
                    DateFile.instance.massageDate[8010][1].Split('|')[2]
                }));
                if (DateFile.instance.actorLifeMassage.ContainsKey(id))
                {
                    count = DateFile.instance.actorLifeMassage[id].Count;
                    index = count >= shownum ? count - shownum : 0;
                    for (int i = index; i < count; i++)
                    {
                        int[] array = DateFile.instance.actorLifeMassage[id][i];
                        int key2 = array[0];
                        string[] array2 = DateFile.instance.actorMassageDate[key2][2].Split('|');
                        string[] array3 = DateFile.instance.actorMassageDate[key2][99].Split('|');
                        List<string> list = new List<string>
                        {
                            DateFile.instance.massageDate[16][1] + DateFile.instance.SetColoer(10002, array[1].ToString(), false) + DateFile.instance.massageDate[16][3],
                            DateFile.instance.SetColoer(20002, DateFile.instance.solarTermsDate[array[2]][0], false)
                        };

                        list.Add(DateFile.instance.SetColoer(10004, DateFile.instance.GetNewMapDate(array[3], array[4], 98) + DateFile.instance.GetNewMapDate(array[3], array[4], 0), false));
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
                                    list.Add(DateFile.instance.massageDate[10][0].Split('|')[0] + DateFile.instance.SetColoer(20001 + int.Parse(DateFile.instance.GetItemDate(num2, 8, true)), DateFile.instance.GetItemDate(num2, 0, false), false) + DateFile.instance.massageDate[10][0].Split('|')[1]);
                                    break;
                                case 2:
                                    list.Add(DateFile.instance.SetColoer(20001 + int.Parse(DateFile.instance.gongFaDate[num2][2]), DateFile.instance.massageDate[10][0].Split('|')[0] + DateFile.instance.gongFaDate[num2][0] + DateFile.instance.massageDate[10][0].Split('|')[1], false));
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
                        // 避免使用空格,要用 non-break space 字元 (i.e. ALT+SPACE in Mac), 否則unity有機會無故在Space位置加插空行
                        actorMassage.Add(string.Format("{0}{1}: " + DateFile.instance.actorMassageDate[key2][1] + "\n", list.ToArray()));
                    }
                }

                int num3 = int.Parse(DateFile.instance.GetActorDate(id, 26, false));
                if (num3 > 0)
                {
                    actorMassage.Add(string.Format("■ {0}{1}{2}\n", DateFile.instance.massageDate[8010][2].Split('|')[0], DateFile.instance.SetColoer(10002, DateFile.instance.GetActorDate(id, 11, false), false), DateFile.instance.massageDate[8010][2].Split('|')[1]));
                }

                lastActorID = id;
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

        //人物賦性
        public static string GetResource(int id)
        {
            if (!Main.settings.showCharacteristic)
                return "";
            int[] actorResources = ActorMenu.instance.GetActorResources(id);  //401~407

            Transform resourceHolder = WindowManage.instance.informationWindow.transform.Find("ResourceHolder");
            if (resourceHolder == null)
            {
                resourceHolder = UnityEngine.Object.Instantiate(ActorMenu.instance.teamResourcesText[0].transform.parent.transform, new Vector3(58f, -380f, 1), Quaternion.identity);
                //resourceHolder.position = Main.settings.showCharacteristic ? new Vector3(58f, -380f, 1) : new Vector3(58f, -340f, 1);
                resourceHolder.name = "ResourceHolder";
                resourceHolder.SetParent(WindowManage.instance.informationWindow.transform, false);
                
            }

            Text[] resourcesText = resourceHolder.GetComponentsInChildren<Text>();

            for (int j = 0; j < resourcesText.Length; j++)
            {
                resourcesText[j].text = actorResources[j].ToString();
            }
            Component[] componentsInChildren = WindowManage.instance.informationWindow.GetComponentsInChildren<Component>();
            foreach (Component component2 in componentsInChildren)
            {
                if (component2 is Graphic)
                {
                    (component2 as Graphic).CrossFadeAlpha(1f, 0.2f, true);
                }
            }

            return "\n\n";
        }


        // 負重資料
        public static string GetItemWeight(int key)
        {
            string text = "";
            int maxItemSize = ActorMenu.instance.GetMaxItemSize(key);
            int useItemSize = ActorMenu.instance.GetUseItemSize(key);
            text += string.Format("{0}{3}{1} / {2}</color>", new object[] {
                ActorMenu.instance.Color8(useItemSize, maxItemSize),
                ((float)useItemSize / 100f).ToString("f1"),
                ((float)maxItemSize / 100f).ToString("f1"),
                DateFile.instance.massageDate[807][2].Split('|')[0]
            });
            return text;
        }

        public static string GetGenderTA(int id)
        {
            return DateFile.instance.GetActorDate(id, 14, false) == "2" ? "她" : "他";
        }
        public static string PurifyItemName(String name)
        {   //获得的物品名格式为xx\n下九品，需去掉后缀。
            return name.Split('(')[0].Split('\n')[0];
        }
        public static string GetItemColorName(int itemID)
        {
            string itemName = PurifyItemName(DateFile.instance.GetItemDate(itemID, 0, false));
            int itemGrade = int.Parse(DateFile.instance.GetItemDate(itemID, 8, false));
            return DateFile.instance.SetColoer(20001 + itemGrade, itemName);
        }

    }
}
