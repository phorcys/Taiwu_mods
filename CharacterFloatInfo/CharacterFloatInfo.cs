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
        public bool workPlace = false;
        public bool showMood = false; //显示心情
        public bool showLevel = true;
        public bool hideShopInfo = true; //不显示商店的详细信息
        public bool hideChameOfChildren = true; //不显示儿童的魅力
        public bool useColorOfTeachingSkill = false; //用可以请教的技艺的颜色显示资质(120=红)

        public bool showActorStatus = true; // 人物状况
        public bool lifeMessage = false; //人物经历
        public bool showCharacteristic = true; //人物特性
        public bool showIV = false; //显示被隐藏了的人物特性
        public bool showResources = false; // 七元賦性
        public bool showBest = true; //显示身上品质最高的物品与功法及可学功法

        public bool deadActor = false;//死亡人物显示信息
        public bool enableMAL = true;
        public bool enableMAN = false;
        public bool enableDI = false;
        public bool enableBW = false;
        public bool enableTA = false;
        public bool enableAM = false;
        public bool enableRI = false;
        public bool enableMAC = false;
        public bool shortMAL = false;
        public bool shortDI = false;
        public bool shortTA = false;
        public bool shortAM = false;
        public bool shortRI = false;
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
            GUILayout.BeginHorizontal();
            GUILayout.Label("浮窗显示区域");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Main.settings.enableMAL = GUILayout.Toggle(Main.settings.enableMAL, "地块人物", new GUILayoutOption[0]);
            Main.settings.enableMAN = GUILayout.Toggle(Main.settings.enableMAN, "地块邻格", new GUILayoutOption[0]);
            Main.settings.enableTA = GUILayout.Toggle(Main.settings.enableTA, "主画面同道", new GUILayoutOption[0]);
            Main.settings.enableDI = GUILayout.Toggle(Main.settings.enableDI, "对话对象", new GUILayoutOption[0]);
            Main.settings.enableMAC = GUILayout.Toggle(Main.settings.enableMAC, "对话选择人物", new GUILayoutOption[0]);
            Main.settings.enableBW = GUILayout.Toggle(Main.settings.enableBW, "村民分配", new GUILayoutOption[0]);
            Main.settings.enableAM = GUILayout.Toggle(Main.settings.enableAM, "同道列表", new GUILayoutOption[0]);
            Main.settings.enableRI = GUILayout.Toggle(Main.settings.enableRI, "人物关系", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("展示內容");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Main.settings.showActorStatus = GUILayout.Toggle(Main.settings.showActorStatus, "人物状况", new GUILayoutOption[0]);
            Main.settings.showCharacteristic = GUILayout.Toggle(Main.settings.showCharacteristic, "人物特性", new GUILayoutOption[0]);
            Main.settings.showLevel = GUILayout.Toggle(Main.settings.showLevel, "人物属性", new GUILayoutOption[0]);
            Main.settings.showResources = GUILayout.Toggle(Main.settings.showResources, "七元賦性", new GUILayoutOption[0]);
            Main.settings.showBest = GUILayout.Toggle(Main.settings.showBest, "最佳物品、功法", new GUILayoutOption[0]);
            Main.settings.lifeMessage = GUILayout.Toggle(Main.settings.lifeMessage, "人物经历", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("简约显示");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Main.settings.shortMAL = GUILayout.Toggle(Main.settings.shortMAL, "地块人物列表", new GUILayoutOption[0]);
            Main.settings.shortTA = GUILayout.Toggle(Main.settings.shortTA, "主角及同道头像", new GUILayoutOption[0]);
            Main.settings.shortDI = GUILayout.Toggle(Main.settings.shortDI, "对话界面", new GUILayoutOption[0]);
            Main.settings.shortAM = GUILayout.Toggle(Main.settings.shortAM, "人物信息界面", new GUILayoutOption[0]);
            Main.settings.shortRI = GUILayout.Toggle(Main.settings.shortRI, "人物关系界面", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("其他设定");
            GUILayout.EndHorizontal();
            Main.settings.addonInfo = GUILayout.Toggle(Main.settings.addonInfo, "比对原始信息", new GUILayoutOption[0]);
            Main.settings.deadActor = GUILayout.Toggle(Main.settings.deadActor, "显示已故人物信息", new GUILayoutOption[0]);
            Main.settings.showIV = GUILayout.Toggle(Main.settings.showIV, "显示隐藏的人物特性", new GUILayoutOption[0]);
            Main.settings.useColorOfTeachingSkill = GUILayout.Toggle(Main.settings.useColorOfTeachingSkill, "以请教阈值显示资质", new GUILayoutOption[0]);
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }

    // 輪迴NPC列表
    [HarmonyPatch(typeof(SetSamsaraActor), "SetActor")]
    public static class SetSamsaraActor_SetActor_Patch
    {
        static void Postfix(int key, ActorFace ___mianActorFace)
        {
            if (!Main.enabled) return;
            GameObject actor = ___mianActorFace.transform.parent.parent.parent.gameObject;
            if (actor.GetComponents<PointerEnter>().Count() == 0)
            {
                actor.AddComponent<PointerEnter>();
            }
        }
    }

    // 同道人列表(點擊主角時)
    [HarmonyPatch(typeof(SetListActor), "SetActor")]
    public static class SetListActor_SetActor_Patch
    {
        static void Postfix(int key, ActorFace ___mianActorFace)
        {
            if (!Main.enabled) return;
            GameObject actor = ___mianActorFace.transform.parent.parent.parent.gameObject;
            if (actor.GetComponents<PointerEnter>().Count() == 0)
            {
                actor.AddComponent<PointerEnter>();
            }
        }
    }

    // 大地圖上的主要NPC列表
    [HarmonyPatch(typeof(SetPlaceActor), "SetActor")]
    public static class SetPlaceActor_SetActor_Patch
    {
        static void Postfix(int key, bool show, ActorFace ___mianActorFace)
        {
            if (!Main.enabled) return;
            if (show)
            { // 只顯示本格及邻格的NPC
                GameObject actor = ___mianActorFace.transform.parent.parent.parent.gameObject;
                if (actor.GetComponents<PointerEnter>().Count() == 0)
                {
                    actor.AddComponent<PointerEnter>();
                }
            }
        }
    }

    // 村內建築中選擇NPC時
    [HarmonyPatch(typeof(SetWorkActorIcon), "SetActor")]
    public static class SetWorkActorIcon_SetActor_Patch
    {
        static void Postfix(int key, int skillTyp, bool favorChange, ActorFace ___mianActorFace)
        {
            if (!Main.enabled) return;
            GameObject actor = ___mianActorFace.transform.parent.parent.parent.gameObject;
            if (actor.GetComponents<PointerEnter>().Count() == 0)
            {
                actor.AddComponent<PointerEnter>();
            }
        }
    }

    // 對話時,彈出的NPC選擇窗
    [HarmonyPatch(typeof(MassageWindow), "GetActor")]
    public static class MassageWindow_GetActor_Patch
    {
        static void Postfix(Transform ___actorHolder)
        {
            if (!Main.enabled) return;
            for (int i = 0; i < ___actorHolder.childCount; i++)
            {
                GameObject actor = ___actorHolder.GetChild(i).gameObject;
                if (actor.GetComponents<PointerEnter>().Count() == 0)
                {
                    actor.AddComponent<PointerEnter>();
                }
            }
        }
    }

    [HarmonyPatch(typeof(WindowManage), "WindowSwitch")]
    public static class WindowManage_WindowSwitch_Patch
    {
        public static List<string> actorMassage = new List<string>();
        public static int lastActorID = 0;
        public static bool isDead;
        public static bool smallerWindow;
        public static WindowType windowType;

        public enum WindowType
        {
            MapActorList,
            Dialog,
            BuildingWindow,
            TeamActor,
            ActorMenu,
            Relationship,
            DialogChooseActors,
        };

        public static bool CheckShort()
        {
            switch (windowType)
            {
                case WindowType.MapActorList:
                    return Main.settings.shortMAL;
                case WindowType.Dialog:
                    return Main.settings.shortTA;
                case WindowType.ActorMenu:
                    return Main.settings.shortDI;
                case WindowType.TeamActor:
                    return Main.settings.shortAM;
                case WindowType.Relationship:
                    return Main.settings.shortRI;
                case WindowType.DialogChooseActors:  //這兩種情況並不能出現太多資訊,影響UI,選擇困難
                case WindowType.BuildingWindow:
                    return true;
            }
            return false;
        }

        public static void Postfix(bool on, GameObject tips, ref Text ___itemMoneyText, ref Text ___itemLevelText, ref Text ___informationMassage, ref Text ___informationName, ref bool ___anTips, ref int ___tipsW, ref int ___tipsH)
        {
            if (!on || !Main.enabled || ActorMenu.instance == null || tips == null) return;

            bool needShow = false;
            string[] array = tips.name.Split(',');
            int id = array.Length>1 ? int.Parse(array[1]) : 0;

            //大地圖下面的太吾自己的頭像
            if (array[0] == "PlayerFaceButton")
            {
                id = DateFile.instance.mianActorId;
                needShow = Main.settings.enableTA;
                windowType = WindowType.TeamActor;
            }
            else
            //大地圖下面的隊友頭像
            if (tips.tag == "TeamActor")
            {
                id = DateFile.instance.acotrTeamDate[ id ];
                needShow = id > 0 && Main.settings.enableTA;
                windowType = WindowType.TeamActor;
            }
            else
            //同道列表
            if (tips.transform.parent.name == "ActorListMianHolder")
            {
                needShow = id > 0 && Main.settings.enableAM;
                windowType = WindowType.ActorMenu;
            }
            else
            //人物訊息內的關係頁及輪迴前世
            if (tips.transform.parent.name == "ActorListHolder" || (ActorMenu.instance.actorMenu.activeSelf && tips.transform.parent.name == "ActorHolder"))
            {
                needShow = Main.settings.enableRI;
                windowType = WindowType.Relationship;
            }
            else
            //太吾村內工作人員選單
            if (HomeSystem.instance.buildingWindow.gameObject.activeSelf && tips.transform.parent.name == "ActorHolder")
            {
                needShow = Main.settings.enableBW;
                windowType = WindowType.BuildingWindow;
            }
            else
            //对话界面的人物选择
            if (MassageWindow.instance.actorWindow.activeInHierarchy && tips.transform.parent.name == "ActorHolder")
            {
                needShow = Main.settings.enableMAC;
                windowType = WindowType.DialogChooseActors;
            }
            //对话窗口的人物头像
            else if (array[0] == "FaceHolder" )
            {
                id = MassageWindow.instance.eventMianActorId;
                needShow = Main.settings.enableDI;
                windowType = WindowType.Dialog;
            }
            else
            //建筑/地图左边的列表
            if (array[0] == "Actor" && DateFile.instance.actorsDate.ContainsKey(id))
            {
                if (WorldMapSystem.instance.choosePlaceId == DateFile.instance.mianPlaceId) //当前格显示
                {
                    needShow = Main.settings.enableMAL;
                    windowType = WindowType.MapActorList;
                }
                else if (WorldMapSystem.instance.choosePlaceId != DateFile.instance.mianPlaceId && WorldMapSystem.instance.playerNeighbor.Contains(WorldMapSystem.instance.choosePlaceId)) //邻格显示
                {
                    needShow = Main.settings.enableMAN;
                    windowType = WindowType.MapActorList;
                }
            }

            isDead = int.Parse(DateFile.instance.GetActorDate(id, 26, false)) > 0;
            if (isDead && !Main.settings.deadActor)
            {
                needShow = false;
            }

            if (needShow)
            {
                smallerWindow = CheckShort();
                ___tipsW = 500;
                ___tipsH = 50;
                if (!smallerWindow)
                {
                    ___tipsW = 680;
                    ___itemLevelText.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 0);
                    ___itemMoneyText.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 0);
                }

                ___itemLevelText.text = SetLevelText(id);
                ___itemMoneyText.text = SetMoneyText(id);

                ___informationName.text = SetInfoName(id);
                ___informationMassage.text = SetInfoMessage(id, ref ___tipsW);
                ___anTips = true;
            }
            else
            {
                if (___informationMassage.text.Length < ___informationName.text.Length)
                {
                    ___tipsW = 200;
                }

                ClearResourceHolder();
                ClearActorFeatureHolder();
                ClearFavourHolder();
            }
        }

        //标题栏左侧小字号文本
        public static string SetLevelText(int id)
        {
            string text = "";
            text += string.Format((isDead ? "享年" : "岁数") + ":\u00A0{0}\n", GetAge(id));
            text += string.Format("寿命:\u00A0{0}", GetHealth(id));
            return text;
        }

        //标题栏右侧侧小字号文本
        public static string SetMoneyText(int actorId)
        {
            string text = "";
            if (actorId != DateFile.instance.MianActorID())
            {
                Transform favourHolder = GetFavourHolder();
                Transform actorFavorHolder = favourHolder.Find("ActorFavor1");
                Image actorFavorBar1 = actorFavorHolder.Find("ActorFavorBar1").GetComponent<Image>();
                Image actorFavorBar2 = actorFavorHolder.Find("ActorFavorBar2").GetComponent<Image>();
                Image actorWarinessBar1 = favourHolder.Find("wariness").Find("WarinessFavorBar").GetComponent<Image>();

                int actorFavor = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), actorId, false, false);
                actorFavorBar1.fillAmount = (float)actorFavor / 30000f;
                actorFavorBar2.fillAmount = (float)(actorFavor - 0x7530) / 60000f;
                int actorWariness = DateFile.instance.GetActorWariness(actorId);
                actorWarinessBar1.fillAmount = (float)DateFile.instance.GetActorWariness(actorId) / 400f;
            }
            else
            {
                //todo: 第X代太吾
                //todo: 遗惠: XXXXX
            }
            return text;
        }

        //标题栏
        public static string SetInfoName(int id)
        {
            string text = "";
            text = DateFile.instance.GetActorName(id, true, false);
            text += ShowActorStatus(id);
            return text;
        }

        // 狀況
        public static string ShowActorStatus(int id)
        {
            if (!Main.settings.showActorStatus) return "";

            string text = "\n";
            string seperator = "";

            if (windowType == WindowType.Relationship)
            {
                text += GetActorGang(id); // 所屬地
                text += GetGangLevelColorText(id); // 地位
                seperator = " • ";
            }

            if (GetGangLevelText(id) == "商人")
            {
                text += seperator + DateFile.instance.SetColoer(20006, GetShopName(id));
                seperator = " • ";
            }

            text += seperator + DateFile.instance.SetColoer(20003, GetGenderText(id));
            seperator = " • ";
            // todo: show if 男生女相, 女生男相

            if (GetAge(id) > ConstValue.actorMinAge) text += seperator + GetSpouse(id);

            if (isDead)
            {
                text += seperator + DateFile.instance.SetColoer(20009, "过世");
                // todo: 投胎至:XX村，人名
            }
            else
            {
                int Hp = ActorMenu.instance.Hp(id, false);
                int maxHp = ActorMenu.instance.MaxHp(id);
                int Sp = ActorMenu.instance.Sp(id, false);
                int maxSp = ActorMenu.instance.MaxSp(id);
                int dmg = Math.Max(Hp * 100 / maxHp, Sp * 100 / maxSp);
                int dmgtyp = 0;
                if (dmg >= 20) dmgtyp = 1;

                for (int i = 51; i <= 56; i++)
                {
                    int num = int.Parse(DateFile.instance.GetActorDate(id, i, false));
                    if (num >= 50)
                    {
                        dmgtyp += 2;
                        break;
                    }
                }

                switch (dmgtyp)
                {
                    case 1:
                        text += seperator + DateFile.instance.SetColoer(20010, "受伤");
                        break;
                    case 2:
                        text += seperator + DateFile.instance.SetColoer(20007, "中毒");
                        break;
                    case 3:
                        text += seperator + DateFile.instance.SetColoer(20010, "受伤") + "&" + DateFile.instance.SetColoer(20007, "中毒");
                        break;
                    default:
                        text += seperator + DateFile.instance.SetColoer(20004, "健康");
                        break;
                }
            }
            return text;
        }

        //文本
        public static string SetInfoMessage(int id, ref int ___tipsW)
        {
            return SetInfoMessage1(id) + "\n" + SetInfoMessage2(id, ref ___tipsW) + SetInfoMessage3(id) + SetInfoMessage4(id) + SetInfoMessage5(id);
        }


        public static string SetInfoMessage1(int id) //主要信息
        {
            string text = "";
            text += "魅力:" + GetChame(id);
            text += "\t\t立场:" + GetGoodness(id);
            text += "\t\t名誉:" + GetFame(id);
            text += "\t\t心情:" + GetMood(id);
            text += "\n\n";

            if (!smallerWindow)
            {
                text += string.Format("轮回:<color=white>{0}</color>", GetSamsara(id));

                text += GetSamsara(id) != "0" ? string.Format(" ◆ 前世:<color=white>{0}</color>", GetSamsaraName(id)) : "";

                string item = Gethobby(id, 0);
                text += string.Format("\t\t喜好:<color={1}>{0}</color>", item, item == "未知" ? "gray" : "white");
                item = Gethobby(id, 1);
                text += string.Format("\t\t厌恶:<color={1}>{0}</color>", item, item == "未知" ? "gray" : "white");

                if (GetAge(id) > ConstValue.actorMinAge)
                    text += string.Format("\t\t子嗣:<color=white>{0}</color>", DateFile.instance.GetActorSocial(id, 310, false).Count);    // todo: 改為顯示所有孩子名

            }
            else if (windowType == WindowType.BuildingWindow)
            {
                text += DateFile.instance.ActorIsWorking(id) == null ?
                    "工作岗位:<color=gray>无</color>" :
                    string.Format("工作岗位:<color=white>{0}</color>\t\t進度:<color=white>{1}</color>", GetWorkPlace(id), GetWorkingData(id));
            }
            else if (windowType == WindowType.DialogChooseActors)
            {
                List<int> martialQuests = new List<int> { 9301, 9303, 9304, 9305, 9307, 9308, 9310, 9312, 9315 };

                if (!martialQuests.Contains(MassageWindow.instance.mianEventDate[2])) return text; // 只在門派掌門任務中顯示以下內容. 

                int gangId = int.Parse(DateFile.instance.GetActorDate(MassageWindow.instance.eventMianActorId, 19, false));
                text += string.Format("地区恩义:<color=white>{0}%</color>", DateFile.instance.GetBasePartValue(int.Parse(DateFile.instance.GetGangDate(gangId, 11)), int.Parse(DateFile.instance.GetGangDate(gangId, 3))) / 10);
                switch (MassageWindow.instance.mianEventDate[2])
                {
                    case 9301: // 念经忏悔
                        int num = Math.Min(0, int.Parse(DateFile.instance.GetActorDate(id, 18, false)));
                        text += string.Format("\t\t罪孽:<color={1}>{0}</color>", num, num < 0 ? "red" : "white");
                        break;
                    case 9303: // 起死回生
                        int Hp = ActorMenu.instance.Hp(id, false);
                        int maxHp = ActorMenu.instance.MaxHp(id);
                        int Sp = ActorMenu.instance.Sp(id, false);
                        int maxSp = ActorMenu.instance.MaxSp(id);
                        text += string.Format("\t\t外傷:<color={4}>{0}/{1}</color>\t\t內傷:<color={5}>{2}/{3}</color>", Hp, maxHp, Sp, maxSp, Hp == 0 ? "white" : "red", Sp == 0 ? "white" : "red");
                        break;
                    case 9307: // 王禅典籍
                        text += string.Format("\t\t处世立场:{0}", DateFile.instance.GetActorDate(id, 18, false));
                        break;
                    case 9310: // 秘药延寿
                        int year = ActorMenu.instance.Health(id);
                        text += string.Format("\t\t寿命余下<color={1}>{0}</color>年", year, year > 1 ? "white" : "red");
                        break;
                    case 9312: // 五圣秘浴
                        string p = "";
                        for (int i = 51; i <= 56; i++)
                        {
                            if (DateFile.instance.GetActorDate(id, i) != "0") p += DateFile.instance.poisonDate[i - 51][0] + ":" + DateFile.instance.GetActorDate(id, i);
                        }
                        text += p == "" ? "\t\t无中毒" : p;
                        break;
                    case 9304: // 七星调元
                        float hurt = int.Parse(DateFile.instance.GetActorDate(id, 39)) / 10;
                        text += string.Format("\t\t內息:<color={1}>{0:0.#}</color>", hurt, hurt == 0 ? "white" : "red");
                        break;
                    case 9308: // 玉镜沉思
                        int canDeductCount = 0, canEnchanceCount = 0;
                        foreach (int featureID in DateFile.instance.GetActorFeature(id))
                        {
                            if (int.Parse(DateFile.instance.actorFeaturesDate[featureID][8]) == 3 && int.Parse(DateFile.instance.actorFeaturesDate[featureID][4]) < 3) canEnchanceCount++;
                            if (int.Parse(DateFile.instance.actorFeaturesDate[featureID][8]) == 4 && Mathf.Abs(int.Parse(DateFile.instance.actorFeaturesDate[featureID][4])) > 1) canDeductCount++;
                        }
                        if (canEnchanceCount > 0) text += "\t\t<color=white>可舞劍:" + canEnchanceCount + "</color>";
                        if (canDeductCount > 0) text += "\t\t<color=white>可撫琴:" + canDeductCount + "</color>";
                        break;
                    case 9305: // 石牢静坐
                    case 9315: // 血池秘法
                        int[] specialFeatures = { 9997, 9998, 9999 };
                        if (DateFile.instance.GetActorFeature(id).Count() > 0)
                        {
                            foreach (int featureID in specialFeatures)
                            {
                                if (DateFile.instance.GetActorFeature(id).Contains(featureID))
                                {
                                    text += string.Format("\t\t状态:<color={0}>{1}</color>", featureID == 9997 ? "white" : "red", DateFile.instance.actorFeaturesDate[featureID][0]);
                                    break;
                                }
                            }
                        }
                        break;
                }
            }
            return text;
        }

        // 個人特性
        public static string SetInfoMessage2(int id, ref int ___tipsW)
        {
            string text = "";
            ClearActorFeatureHolder();
            if (Main.settings.showCharacteristic && !smallerWindow)
            {
                text += "\n\n\n";
                int shown = 0;
                if (DateFile.instance.GetActorFeature(id).Count() == 0) return ""; // 死人

                foreach (int featureID in DateFile.instance.GetActorFeature(id))
                    if (DateFile.instance.actorFeaturesDate[featureID][95] != "1" || Main.settings.showIV) //判斷是否顯示隱藏的特性
                    {
                        Transform actorFeature = UnityEngine.Object.Instantiate(ActorMenu.instance.actorFeature, Vector3.zero, Quaternion.identity).transform;
                        actorFeature.Find("ActorFeatureNameText").GetComponent<Text>().text = DateFile.instance.actorFeaturesDate[featureID][0];
                        Transform actorFeatureStarHolder = actorFeature.Find("ActorFeatureStarHolder");

                        string att = DateFile.instance.actorFeaturesDate[featureID][1];
                        if (att.IndexOf('|') > -1 || att != "0") foreach (string j in att.Split('|'))
                            {
                                UnityEngine.Object.Instantiate(ActorMenu.instance.actorAttackFeatureStarIcons[int.Parse(j) > 0 ? int.Parse(j) - 1 : 3], Vector3.zero, Quaternion.identity).transform.SetParent(actorFeatureStarHolder, false);
                            }
                        string def = DateFile.instance.actorFeaturesDate[featureID][2];
                        if (def.IndexOf('|') > -1 || def != "0") foreach (string j in def.Split('|'))
                            {
                                UnityEngine.Object.Instantiate(ActorMenu.instance.actorDefFeatureStarIcons[int.Parse(j) > 0 ? int.Parse(j) - 1 : 3], Vector3.zero, Quaternion.identity).transform.SetParent(actorFeatureStarHolder, false);
                            }
                        string spd = DateFile.instance.actorFeaturesDate[featureID][3];
                        if (spd.IndexOf('|') > -1 || spd != "0") foreach (string j in spd.Split('|'))
                            {
                                UnityEngine.Object.Instantiate(ActorMenu.instance.actorPlanFeatureStarIcons[int.Parse(j) > 0 ? int.Parse(j) - 1 : 3], Vector3.zero, Quaternion.identity).transform.SetParent(actorFeatureStarHolder, false);
                            }
                        if (att == "0" && def == "0" && spd == "0")
                        {
                            UnityEngine.Object.Instantiate(ActorMenu.instance.actorAttackFeatureStarIcons[3], Vector3.zero, Quaternion.identity).transform.SetParent(actorFeatureStarHolder, false);
                            UnityEngine.Object.Instantiate(ActorMenu.instance.actorDefFeatureStarIcons[3], Vector3.zero, Quaternion.identity).transform.SetParent(actorFeatureStarHolder, false);
                            UnityEngine.Object.Instantiate(ActorMenu.instance.actorPlanFeatureStarIcons[3], Vector3.zero, Quaternion.identity).transform.SetParent(actorFeatureStarHolder, false);
                        }

                        actorFeature.SetParent(GetActorFeatureHolder(), false);
                        shown++;
                    }
                // UpdateActorFeatureHolder();
                Graphic[] componentsInChildren = WindowManage.instance.informationWindow.GetComponentsInChildren<Graphic>();
                foreach (Graphic component2 in componentsInChildren)
                {
                    component2.CrossFadeAlpha(1f, 0.2f, true);
                }
                if (shown > 7) ___tipsW += (shown - 7) * 90;
            }
            return text;
        }

        // 人物能力值
        public static string SetInfoMessage3(int id)
        {
            if (!Main.settings.showLevel) return "";
            string text = "\n";
            foreach (int i in DateFile.instance.baseSkillDate.Keys)
            {
                if (!smallerWindow) text += CanTeach(id, i) ? "※" : "　";
                text += GetLevel(id, i);
                text += i % 4 == (i < 100 ? 3 : 0) ? "\n" : (smallerWindow ? "\t" : "\t\t");
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

            string taughtId = DateFile.instance.presetGangGroupDateValue[gangValueId][818];
            switch (taughtId)
            {
                case "0": //不教
                    eventIDs = new List<int>();
                    break;
                case "901000006": // 学习复合技艺
                    eventIDs = DateFile.instance.presetGangGroupDateValue[gangValueId][813].Split('|').Select(int.Parse).ToList();
                    break;
                case "901300004": // 请教才艺
                case "901300007": // 请教手艺
                case "901300008": // 请教杂艺
                    int messageID = int.Parse(DateFile.instance.eventDate[int.Parse(taughtId)][7]);
                    eventIDs = DateFile.instance.eventDate[messageID][5].Split('|').Select(int.Parse).ToList();
                    break;
                default: // 学习單一技艺 or 大夫
                    eventIDs = taughtId.Split('|').Select(int.Parse).ToList();
                    break;
            }
            return eventIDs.Contains(eventID);
        }

        // 最佳 裝備 & 物品 & 武功
        public static string SetInfoMessage4(int id)
        {
            string text = "";
            text += GetResource(id);
            if (Main.settings.showBest && !smallerWindow)
            {
                text += "\n" + GetEquipments(id);
                text += "\n" + GetBestItems(id);
                if (DateFile.instance.actorGongFas.ContainsKey(id))
                {
                    text += "\n" + GetBestGongfa(id);
                    if (id != DateFile.instance.MianActorID())
                    {
                        text += "\n" + GetLearnableGongfa(id);
                    }
                }

            }
            return text;
        }

        // 近期事件
        public static string SetInfoMessage5(int id)
        {
            string text = "";

            if (Main.settings.lifeMessage && !smallerWindow)
            {
                text += "\n" + GetLifeMessage(id, 3);
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
            if (Main.settings.addonInfo && !isChild && actorChameDiff != 0 && !smallerWindow)
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
        public static string GetLevel(int id, int index)
        {
            int colorCorrect = Main.settings.useColorOfTeachingSkill ? 40 : 20;
            int num = int.Parse(DateFile.instance.GetActorDate(id, (index < 100 ? 501 : 500) + index, true));
            int num2 = num - int.Parse(DateFile.instance.GetActorDate(id, (index < 100 ? 501 : 500) + index, false));
            bool shownoadd = !smallerWindow && Main.settings.addonInfo && num2 != 0;
            string text = DateFile.instance.SetColoer(20002 + Mathf.Clamp((num - colorCorrect) / 10, 0, 8),
                string.Format("{0}{1,3}{2}<color=#606060ff>{3,4}</color>{4}",
                    DateFile.instance.baseSkillDate[index][0],
                    num.ToString(),
                    (num < 10 ? "\u00A0" : "") + (num < 100 ? "\u00A0\u00A0" : "\u00A0"),
                    shownoadd ? (num2 < 0 ? "\u00A0" : "+") + num2.ToString() : (smallerWindow ? "" : "\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0"),
                    shownoadd && Math.Abs(num2) < 10 ? "\u00A0" + (Math.Abs(num2) < 100 ? "\u00A0\u00A0" : "") : (smallerWindow ? "\u00A0" : "\u00A0\u00A0")));

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
            return "人物装备: " + (autorEquipments.Count() == 0 ?
                DateFile.instance.SetColoer(20002, GetGenderTA(id) + "赤身裸体在你眼前") :
                string.Join(", ", autorEquipments.ToArray())) + "\n";
        }


        //获取列表中品级最高功法的名字与数量
        public static string GetBestGongfaText(List<int> gongFas)
        {
            string bestName = "";
            int bestLevel = -1;
            foreach (int id in gongFas)
            {
                if (GetGongfaLevel(id) == bestLevel)
                {
                    bestName += ", " + GetGongfaColorText(id);
                }
                else if (GetGongfaLevel(id) > bestLevel)
                {
                    bestName = GetGongfaColorText(id);
                    bestLevel = GetGongfaLevel(id);
                }
            }
            return bestName;
        }

        public static int GetGongfaLevel(int id)
        {
            return DateFile.instance.gongFaDate.ContainsKey(id) ? int.Parse(DateFile.instance.gongFaDate[id][2]) : 0;
        }

        public static string GetGongfaText(int id)
        {
            return DateFile.instance.gongFaDate.ContainsKey(id) ? DateFile.instance.gongFaDate[id][0] : "";
        }

        public static string GetGongfaColorText(int id)
        {
            return DateFile.instance.SetColoer(20001 + GetGongfaLevel(id), GetGongfaText(id));
        }

        public static List<int> GetGongfaList(int id)
        {
            List<int> gongFas = DateFile.instance.actorGongFas.ContainsKey(id) ? new List<int>(DateFile.instance.actorGongFas[id].Keys) : new List<int>(); //避免存取死人資料時引發紅字
            gongFas.RemoveAll(t => t == 0); // 刪除多餘的功法ID=0
            return gongFas;
        }

        //人物身上最高级功法获取
        public static string GetBestGongfa(int id)
        {
            string bestName = GetBestGongfaText(GetGongfaList(id));
            bestName = (bestName == "") ? DateFile.instance.SetColoer(20002, GetGenderTA(id) + "还没来得及学") : bestName;
            return "最佳功法: " + bestName + "\n";
        }

        //人物身上可被太吾修习的功法获取
        public static string GetLearnableGongfa(int id)
        {
            List<int> myGongFas = GetGongfaList(DateFile.instance.MianActorID());
            List<int> taGongFas = GetGongfaList(id);

            //挑出目标人物身上太吾未学会的功法
            List<int> nGongFas = new List<int> { };
            foreach (int gongFaId in taGongFas)
                if (!myGongFas.Contains(gongFaId)) nGongFas.Add(gongFaId);

            string bestName = GetBestGongfaText(nGongFas);
            int bestGongfaCount = (bestName == "") ? 0 : bestName.Split(',').Count();

            bestName = bestName == "" ? DateFile.instance.SetColoer(20002, DateFile.instance.eventDate[9159][3].Replace("我", GetGenderTA(id))) : bestName;
            if (bestGongfaCount < nGongFas.Count) bestName = string.Format("{0} 及 {1} 种低阶功法", bestName, nGongFas.Count - bestGongfaCount);
            return "可学功法: " + bestName + "\n";
        }

        //人物身上的最佳物品获取
        public static string GetBestItems(int id)
        {
            if (!DateFile.instance.actorItemsDate.ContainsKey(id)) return ""; // 無物品/ 已死之人
            List<string> bestItems = new List<string> { };
            int bestGrade = 0;

            List<int> list = new List<int>(ActorMenu.instance.GetActorItems(id, 0).Keys);
            foreach (int itemID in list)
            {
                string itemName = GetItemColorName(itemID);
                int itemGrade = int.Parse(DateFile.instance.GetItemDate(itemID, 8, false));
                if (int.Parse(DateFile.instance.GetItemDate(itemID, 98, false)) == 86) continue;//跳过碎片
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
                string.Join(", ", bestItems.ToArray())) + " \t" + GetItemWeight(id) + "\n";
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
                        actorMassage.Add(string.Format("{0}{1}:\u00A0" + DateFile.instance.actorMassageDate[key2][1], list.ToArray()));
                    }
                }

                int num3 = int.Parse(DateFile.instance.GetActorDate(id, 26, false));
                if (num3 > 0)
                {
                    actorMassage.Add(string.Format("■\u00A0{0}{1}{2}",
                        DateFile.instance.massageDate[8010][2].Split('|')[0],
                        DateFile.instance.SetColoer(10002, DateFile.instance.GetActorDate(id, 11, false), false),
                        DateFile.instance.massageDate[8010][2].Split('|')[1]
                        ));
                }

                lastActorID = id;
            }

            count = actorMassage.Count;
            index = count >= shownum ? count - shownum : 0;
            for (int i = index; i < count; i++)
            {
                text += actorMassage[i].Trim('\n') + "\n";
            }

            return text;
        }

        //工作效率,null代表无法获得
        public static string GetWorkingData(int workerId)
        {
            if (HomeSystem.instance == null) return null;
            if (!HomeSystem.instance.buildingWindowOpend) return null;
            int buildingIndex = HomeSystem.instance.homeMapbuildingIndex;
            int partId = HomeSystem.instance.homeMapPartId;
            int placeId = HomeSystem.instance.homeMapPlaceId;
            if ((!DateFile.instance.baseHomeDate.ContainsKey(partId))
                || (!DateFile.instance.baseHomeDate[partId].ContainsKey(placeId))
                || DateFile.instance.baseHomeDate[partId][placeId] == 0)
                return null;
            if (partId < 0 || placeId < 0) return null;
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
            int xiangFangAddup = 0;
            foreach (int key in HomeSystem.instance.GetBuildingNeighbor(partId, placeId, buildingIndex, 1))
            {
                if (DateFile.instance.homeBuildingsDate[partId][placeId].ContainsKey(key) && DateFile.instance.actorsWorkingDate[partId][placeId].ContainsKey(key) && int.Parse(DateFile.instance.basehomePlaceDate[DateFile.instance.homeBuildingsDate[partId][placeId][key][0]][62]) != 0)
                {
                    int singleAddup = (unknown <= 0) ? 0 : int.Parse(DateFile.instance.GetActorDate(DateFile.instance.actorsWorkingDate[partId][placeId][key], unknown, true));
                    if (unknown == 18)
                        singleAddup += 100;
                    xiangFangAddup += singleAddup;
                }
            }
            int num6 = Mathf.Max(int.Parse(DateFile.instance.basehomePlaceDate[array[0]][51]) + (array[1] - 1), 1);
            num5 = (num5 + xiangFangAddup) * Mathf.Max(moodFavorAddup, 0) / 100;

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
            if (!Main.settings.showResources) return "";

            int[] actorResources = ActorMenu.instance.GetActorResources(id);  //401~407
            if (DateFile.instance.deadActors.Contains(id) || actorResources.Sum() == 0 || smallerWindow)
            {
                ClearResourceHolder();
                return "";
            }

            Text[] resourcesText = GetResourceHolder().gameObject.GetComponentsInChildren<Text>();

            for (int j = 0; j < resourcesText.Length; j++)
            {
                resourcesText[j].text = actorResources[j].ToString();
            }
            Graphic[] componentsInChildren = WindowManage.instance.informationWindow.GetComponentsInChildren<Graphic>();
            foreach (Graphic component2 in componentsInChildren)
            {
                component2.CrossFadeAlpha(1f, 0.2f, true);
            }

            return "\n\n\n";
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

        public static string GetGenderText(int id)
        {
            return DateFile.instance.massageDate[7][0].Split('|')[int.Parse(DateFile.instance.GetActorDate(id, 14, false))];
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

        public static Transform GetActorFeatureHolder()
        {
            Transform actorFeatureHolder = WindowManage.instance.informationMassage.transform.Find("ActorFeatureHolder");
            if (actorFeatureHolder == null)
            {
                actorFeatureHolder = UnityEngine.Object.Instantiate(ActorMenu.instance.actorFeatureHolder, new Vector3(-20f, -60f, 1), Quaternion.identity);
                actorFeatureHolder.name = "ActorFeatureHolder";
                actorFeatureHolder.localScale = new Vector3(.73f, .73f, 1);
                actorFeatureHolder.GetComponent<RectTransform>().sizeDelta = new Vector2(800f, 0);
                actorFeatureHolder.SetParent(WindowManage.instance.informationMassage.GetComponent<RectTransform>(), false);
            }
            return actorFeatureHolder;
        }
        public static void ClearActorFeatureHolder()
        {
            Transform actorFeatureHolder = GetActorFeatureHolder();
            if (actorFeatureHolder.childCount > 0)
            {
                for (int i = 0; i < actorFeatureHolder.childCount; i++)
                {
                    UnityEngine.Object.Destroy(actorFeatureHolder.GetChild(i).gameObject);
                }
            }
        }

        public static Transform GetResourceHolder()
        {
            float y = 65; //主要信息
            if (Main.settings.showCharacteristic) y += 55; // 人物特性
            if (Main.settings.showLevel) y += 195; // 人物屬性
            Transform resourceHolder = WindowManage.instance.informationMassage.transform.Find("ResourceHolder");
            if (resourceHolder == null)
            {
                resourceHolder = UnityEngine.Object.Instantiate(ActorMenu.instance.teamResourcesText[0].transform.parent, new Vector3(0, -y, 1), Quaternion.identity);
                resourceHolder.name = "ResourceHolder";
                resourceHolder.SetParent(WindowManage.instance.informationMassage.GetComponent<RectTransform>(), false);
            }
            return resourceHolder;
        }

        public static void ClearResourceHolder()
        {
            UnityEngine.Object.Destroy(GetResourceHolder().gameObject);
        }

        public static Transform GetFavourHolder()
        {
            Transform favourHolder = WindowManage.instance.itemMoneyText.transform.Find("FavourHolder");
            if (favourHolder == null)
            {
                favourHolder = UnityEngine.Object.Instantiate(MassageWindow.instance.actorFavor.transform);
                UnityEngine.Object.Destroy(favourHolder.Find("PartGoodIcon").gameObject);
                favourHolder.name = "FavourHolder";
                favourHolder.Rotate(new Vector3(0, 180f, 0));
                favourHolder.localPosition = new Vector3(20f, 37f, 0);
                favourHolder.SetParent(WindowManage.instance.itemMoneyText.transform, false);
                favourHolder.Find("wariness").GetComponent<RectTransform>().localPosition = new Vector3(-50f, -38f, 0);
                AddText("FavLabel", favourHolder.Find("ActorFavor1"), "感情:", -48f, -9f, 60f, 30f, Color.yellow);
                AddText("WariLabel", favourHolder.Find("wariness"), "戒心:", -48f, -9f, 60f, 30f, Color.yellow);
            }
            return favourHolder;
        }

        public static void ClearFavourHolder()
        {
            UnityEngine.Object.Destroy(GetFavourHolder().gameObject);
        }

        public static void AddText(string name, Transform parent, string initText = "", float x = 0, float y = 0, float width = 200f, float height = 200f, Color? color = null, int fontsize = 15, TextAnchor align = TextAnchor.MiddleLeft)
        {
            var text = new GameObject(name, typeof(Text));
            text.transform.SetParent(parent, false);
            text.GetComponent<Text>().color = color ?? Color.white;
            text.GetComponent<Text>().text = initText;
            text.GetComponent<Text>().font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text.GetComponent<Text>().alignment = align;
            text.GetComponent<Text>().fontSize = fontsize;
            var textTransform = text.GetComponent<RectTransform>();
            textTransform.sizeDelta = new Vector2(width, height);
            textTransform.localScale = new Vector3(1, 1, 1);
            textTransform.anchorMax = new Vector2(0.5f, 1);
            textTransform.anchorMin = new Vector2(0.5f, 1);
            textTransform.anchoredPosition3D = new Vector2(x, y);
        }
    }
}
