using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public bool useColorOfTeachingSkill = false; // 用颜色及數字 標記 可以请教的技艺最高品階
        public bool showSexuality = false; //显示性向

        public bool showActorStatus = true; // 人物状况
        public bool lifeMessage = false; //人物经历
        public bool showCharacteristic = true; //人物技艺
        public bool showFamilySkill = false; //技艺造诣
        public bool showIV = false; //显示被隐藏了的人物特性
        public bool showResources = false; // 七元賦性
        public bool showBest = true; //显示身上品质最高的物品与功法及可学功法

        public bool deadActor = false;//死亡人物显示信息
        public bool enableMAL = true, enableMAN = false, enableDI = false, enableBW = false, enableTA = false, enableAM = false, enableRI = false, enableMAC = false, enableWNV = false;
        public bool shortMAL = false, shortDI = false, shortTA = false, shortAM = false, shortRI = false;
        public int minWidth = 680;
        public int colorLevelBaseOn = 0;
    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        private static Version gameVersion;
        public static Version GameVersion
        {
            get
            {
                gameVersion = gameVersion ?? new Version(DateFile.instance.gameVersion.Replace("Beta V", "").Replace("[Test]", ""));
                return gameVersion;
            }
        }

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
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
            if (GameVersion < new Version(0, 1, 6, 3))
            {
                enabled = false;
                GUILayout.Label("游戏版本 V" + GameVersion);
                GUILayout.Label("此插件要求 V0.1.6.3 (无法载入)");
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("浮窗显示区域");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                settings.enableMAL = GUILayout.Toggle(settings.enableMAL, "地块人物", new GUILayoutOption[0]);
                settings.enableMAN = GUILayout.Toggle(settings.enableMAN, "地块邻格", new GUILayoutOption[0]);
                settings.enableTA = GUILayout.Toggle(settings.enableTA, "主画面同道", new GUILayoutOption[0]);
                settings.enableDI = GUILayout.Toggle(settings.enableDI, "对话对象", new GUILayoutOption[0]);
                settings.enableMAC = GUILayout.Toggle(settings.enableMAC, "对话选择人物", new GUILayoutOption[0]);
                settings.enableBW = GUILayout.Toggle(settings.enableBW, "村民分配", new GUILayoutOption[0]);
                settings.enableAM = GUILayout.Toggle(settings.enableAM, "同道列表", new GUILayoutOption[0]);
                settings.enableRI = GUILayout.Toggle(settings.enableRI, "人物关系", new GUILayoutOption[0]);
                settings.enableWNV = GUILayout.Toggle(settings.enableWNV, "新村民", new GUILayoutOption[0]);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("展示內容");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                settings.showActorStatus = GUILayout.Toggle(settings.showActorStatus, "人物状况", new GUILayoutOption[0]);
                settings.showCharacteristic = GUILayout.Toggle(settings.showCharacteristic, "人物特性", new GUILayoutOption[0]);
                settings.showLevel = GUILayout.Toggle(settings.showLevel, "人物属性", new GUILayoutOption[0]);
                settings.showFamilySkill = GUILayout.Toggle(settings.showFamilySkill, "技艺造诣", new GUILayoutOption[0]);
                settings.showResources = GUILayout.Toggle(settings.showResources, "七元賦性", new GUILayoutOption[0]);
                settings.showBest = GUILayout.Toggle(settings.showBest, "最佳物品、功法", new GUILayoutOption[0]);
                settings.lifeMessage = GUILayout.Toggle(settings.lifeMessage, "人物经历", new GUILayoutOption[0]);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("简约显示");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                settings.shortMAL = GUILayout.Toggle(settings.shortMAL, "地块人物列表", new GUILayoutOption[0]);
                settings.shortTA = GUILayout.Toggle(settings.shortTA, "主角及同道头像", new GUILayoutOption[0]);
                settings.shortDI = GUILayout.Toggle(settings.shortDI, "对话界面", new GUILayoutOption[0]);
                settings.shortAM = GUILayout.Toggle(settings.shortAM, "人物信息界面", new GUILayoutOption[0]);
                settings.shortRI = GUILayout.Toggle(settings.shortRI, "人物关系界面", new GUILayoutOption[0]);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("其他设定");
                GUILayout.EndHorizontal();
                settings.showSexuality = GUILayout.Toggle(settings.showSexuality, "显示性取向", new GUILayoutOption[0]);
                settings.addonInfo = GUILayout.Toggle(settings.addonInfo, "比对原始信息", new GUILayoutOption[0]);
                settings.deadActor = GUILayout.Toggle(settings.deadActor, "显示已故人物信息", new GUILayoutOption[0]);
                settings.showIV = GUILayout.Toggle(settings.showIV, "显示隐藏的人物特性", new GUILayoutOption[0]);

                settings.useColorOfTeachingSkill = GUILayout.Toggle(settings.useColorOfTeachingSkill, "标记可学技艺的最高品阶", new GUILayoutOption[0]);
                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("颜色计算基于", GUILayout.Width(150));
                settings.colorLevelBaseOn = GUILayout.SelectionGrid(settings.colorLevelBaseOn, new string[] { "门派红字", "门派橙字", "门派黄字", "门派紫字", "大城", "村镇关寨" }, 6);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("窗口最小宽度", GUILayout.Width(150));
                settings.minWidth = int.Parse(GUILayout.TextArea(settings.minWidth.ToString(), GUILayout.Width(50)));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
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

    // 新村民加入
    [HarmonyPatch(typeof(GetActorWindow), "ShowGetActorWindow")]
    public static class GetActorWindow_ShowGetActorWindow_Patch
    {
        static void Postfix(List<int> actorId, GetActorWindow __instance)
        {
            if (!Main.enabled) return;
            //int num = YesOrNoWindow.instance.onMaskWindow[YesOrNoWindow.instance.onMaskWindow.Count - 1].GetSiblingIndex();
            //if (num > YesOrNoWindow.instance.windowMask.transform.GetSiblingIndex()) num--;
            //YesOrNoWindow.instance.windowMask.transform.SetSiblingIndex(num);

            for (int i = 0; i < __instance.actorBack.Length; i++)
            {
                if (i < actorId.Count)
                {
                    GameObject actor = __instance.getActorLight[i].gameObject;
                    actor.name = "NewActorFace," + actorId[i].ToString();
                    if (actor.GetComponents<PointerEnter>().Count() == 0)
                    {
                        actor.AddComponent<PointerEnter>();
                        __instance.getActorLight[i].raycastTarget = true;
                    }
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
            WorkerNewVillager,
        };

        public static bool CheckShort()
        {
            switch (windowType)
            {
                case WindowType.MapActorList: return Main.settings.shortMAL;
                case WindowType.WorkerNewVillager:
                case WindowType.Dialog: return Main.settings.shortDI;
                case WindowType.ActorMenu: return Main.settings.shortAM;
                case WindowType.TeamActor: return Main.settings.shortTA;
                case WindowType.Relationship: return Main.settings.shortRI;
                case WindowType.DialogChooseActors:
                case WindowType.BuildingWindow: return true;
            }
            return false;
        }

        public static void Postfix(bool on, GameObject tips, ref Text ___itemMoneyText, ref Text ___itemLevelText, ref Text ___informationMassage, ref Text ___informationName, ref bool ___anTips, ref int ___tipsW, ref int ___tipsH)
        {
            if (!on || !Main.enabled || ActorMenu.instance == null || tips == null) return;

            bool needShow = false;
            string[] array = tips.name.Split(',');
            int id = array.Length > 1 ? int.Parse(array[1]) : 0;

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
                id = DateFile.instance.acotrTeamDate[id];
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
            else if (array[0] == "FaceHolder")
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
            else // 工人界面的新村民
            if (array[0] == "ActorIcon")
            {
                needShow = Main.settings.enableWNV;
                windowType = WindowType.WorkerNewVillager;
            }
            else
            if (array[0] == "NewActorFace")
            {
                needShow = Main.settings.enableWNV;
                windowType = WindowType.WorkerNewVillager;
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
                    ___tipsW = Main.settings.minWidth;
                    ___itemLevelText.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 0);
                    ___itemMoneyText.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 0);
                }

                ___itemLevelText.text = SetLevelText(id);
                ___itemMoneyText.text = SetMoneyText(id);

                ___informationName.text = SetInfoName(id);
                ___informationMassage.text = SetInfoMessage(id, ref ___tipsW) + (___informationMassage.text.Length > 100 ? "" : "\n" + ___informationMassage.text);
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
                actorWarinessBar1.fillAmount = (float)DateFile.instance.GetActorWariness(actorId) / 400f;
            }
            else
            {
                ClearFavourHolder();
                text += string.Format("{0}{1}{2}", DateFile.instance.massageDate[16][1], DateFile.instance.samsara, DateFile.instance.massageDate[16][2]); // 第X代太吾
                text += string.Format("\n生平遗惠 <color>{0}</color>", GetActorScore(actorId));
            }
            return text;
        }

        public static int GetActorScore(int actorId) // 生平遺惠
        {
            if (!DateFile.instance.actorGetScore.ContainsKey(actorId)) return 0;
            int[] array = new int[7];
            foreach (var num2 in DateFile.instance.actorGetScore[actorId].Keys)
            {
                if (num2 != 0)
                {
                    Dictionary<int, int> dictionary = new Dictionary<int, int>();
                    foreach (var array2 in DateFile.instance.actorGetScore[actorId][num2])
                    {
                        if (dictionary.ContainsKey(array2[0]))
                        {
                            dictionary[array2[0]] += array2[1];
                        }
                        else
                        {
                            dictionary[array2[0]] = array2[1];
                        }
                    }
                    foreach (var key2 in dictionary.Keys)
                    {
                        int num5 = int.Parse(DateFile.instance.scoreValueDate[key2][3]);
                        int num6 = 100;
                        foreach (var str in DateFile.instance.scoreValueDate[key2][4].Split('|'))
                        {
                            switch (int.Parse(str))
                            {
                                case 1:
                                    num6 += DateFile.instance.enemyBorn * DateFile.instance.enemyBorn * 40;
                                    break;
                                case 2:
                                    num6 += DateFile.instance.enemySize * DateFile.instance.enemySize * 10;
                                    break;
                                case 3:
                                    num6 += DateFile.instance.xxLevel * DateFile.instance.xxLevel * 30;
                                    break;
                                case 4:
                                    num6 += DateFile.instance.worldResource * DateFile.instance.worldResource * 10;
                                    break;
                            }
                        }
                        num5 = num5 * num6 / 100;
                        array[num2 - 1] += Mathf.Min(dictionary[key2], num5);
                    }
                }
            }
            return array.Sum();
        }

        //标题栏
        public static string SetInfoName(int id) => DateFile.instance.GetActorName(id, true, false) + ShowActorStatus(id);

        // 狀況
        public static string ShowActorStatus(int id)
        {
            if (!Main.settings.showActorStatus) return "";

            string text = "\n";
            string seperator = "";

            if (windowType == WindowType.Relationship || windowType == WindowType.Dialog)
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
            else if (DateFile.instance.actorInjuryDate.ContainsKey(id))
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
        public static string SetInfoMessage(int id, ref int ___tipsW) => SetInfoMessage1(id) + "\n" + SetInfoMessage2(id, ref ___tipsW) + SetInfoMessage3(id) + SetInfoMessage4(id) + SetInfoMessage5(id);


        public static string SetInfoMessage1(int id) //主要信息
        {
            string text = "";
            text += "魅力:" + GetChame(id);
            text += "\t\t立场:" + GetGoodness(id);
            text += "\t\t名誉:" + GetFame(id);
            text += "\t\t心情:" + GetMood(id);
            text += "\t\t印象:" + GetLifeFace(id);
            text += "\n\n";

            if (!smallerWindow)
            {
                text += string.Format("轮回:<color=white>{0}</color>", GetSamsara(id));

                text += GetSamsara(id) != "0" ? string.Format(" ◆ 前世:<color=white>{0}</color>", GetSamsaraName(id)) : "";

                string item = Gethobby(id, 0);
                text += string.Format("\t\t喜好:<color={1}>{0}</color>", item, item == "未知" ? "gray" : "white");
                item = Gethobby(id, 1);
                text += string.Format("\t\t厌恶:<color={1}>{0}</color>", item, item == "未知" ? "gray" : "white");

                if (Main.settings.showIV)
                {
                    int result1 = GetFertility(id, true);
                    int result2 = GetFertility(id, false);
                    text += string.Format("\t\t生育:<color={1}>{0}%</color>", result1, (result2 > 0 ? "+" : "") + result2, result1 <= 0 ? "red" : "white");
                    if (Main.settings.addonInfo && result2 != 0) text += string.Format("\t<color=#606060FF>{0}%</color>", (result2 > 0 ? "+" : "") + result2);
                }

                if (GetAge(id) > ConstValue.actorMinAge)
                    text += string.Format("\t\t子嗣:<color=white>{0}</color>", DateFile.instance.GetActorSocial(id, 310, false, false).Count);    // todo: 改為顯示所有孩子名

                text += string.Format("\t\t威望:<color=white>{0}</color>", int.Parse(DateFile.instance.GetActorDate(id, 407, false)));
            }
            else if (windowType == WindowType.BuildingWindow)
            {
                if (HomeSystem.instance == null) return null;
                if (!HomeSystem.instance.buildingWindowOpend) return null;
                int partId, placeId, buildingIndex;

                text += "工作岗位:";

                int[] city = DateFile.instance.ActorIsWorking(id);
                if (city == null)
                {
                    text += "<color=#606060FF>无</color>";
                }
                else
                {
                    partId = city[0];
                    placeId = city[1];
                    var aBuilding = DateFile.instance.actorsWorkingDate[partId][placeId].FirstOrDefault(t => t.Value == id);
                    if (aBuilding.Equals(default(KeyValuePair<int, int>)))
                    {
                        text += "<color=red>未知</color>";
                    }
                    else
                    {
                        buildingIndex = aBuilding.Key;
                        text += string.Format("<color=white>{0}</color>", GetWorkPlace(partId, placeId, buildingIndex));
                        text += "\t进度:<color=white>" + GetWorkingProgress(partId, placeId, buildingIndex) + "</color>";
                    }
                }
                partId = HomeSystem.instance.homeMapPartId;
                placeId = HomeSystem.instance.homeMapPlaceId;
                buildingIndex = HomeSystem.instance.homeMapbuildingIndex;
                if (!DateFile.instance.actorsWorkingDate[partId][placeId].TryGetValue(buildingIndex, out int workerId) || workerId != id)
                    text += string.Format("\n村民派遣，预期效率:<color=white>{0}</color>", GetExpectEfficient(id, partId, placeId, buildingIndex));

            }
            else if (windowType == WindowType.DialogChooseActors)
            {
                List<int> martialQuests = new List<int> { 9301, 9303, 9304, 9305, 9307, 9308, 9310, 9312, 9315, 9366, 9321 };
                if (!martialQuests.Contains(MassageWindow.instance.mianEventDate[2])) return text; // 只在門派掌門任務中顯示以下內容.

                int gangId = int.Parse(DateFile.instance.GetActorDate(MassageWindow.instance.eventMianActorId, 19, false));
                text += string.Format("地区恩义:<color=white>{0}%</color>", DateFile.instance.GetBasePartValue(int.Parse(DateFile.instance.GetGangDate(gangId, 11)), int.Parse(DateFile.instance.GetGangDate(gangId, 3))) / 10);
                switch (MassageWindow.instance.mianEventDate[2])
                {
                    case 9301: // 念经忏悔
                        int num = Math.Min(0, GetActorFame(id));
                        text += string.Format("\t\t罪孽:<color={1}>{0}</color>", num, num < 0 ? "red" : "white");
                        break;
                    case 9321: // 治疗伤病
                    case 9303: // 起死回生
                        int Hp = ActorMenu.instance.Hp(id, false);
                        int maxHp = ActorMenu.instance.MaxHp(id);
                        int Sp = ActorMenu.instance.Sp(id, false);
                        int maxSp = ActorMenu.instance.MaxSp(id);
                        text += string.Format("\t\t外傷:<color={4}>{0}/{1}</color>\t\t內傷:<color={5}>{2}/{3}</color>", Hp, maxHp, Sp, maxSp, Hp == 0 ? "white" : "red", Sp == 0 ? "white" : "red");
                        break;
                    case 9307: // 王禅典籍
                        text += string.Format("\t\t处世立场:{0}", GetActorFame(id));
                        break;
                    case 9310: // 秘药延寿
                        int year = ActorMenu.instance.Health(id);
                        text += string.Format("\t\t寿命余下<color={1}>{0}</color>年", year, year > 1 ? "white" : "red");
                        break;
                    case 9366: // 驱除毒素
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
                        if (canDeductCount == 0 && canDeductCount == 0) text += "\t\t<color=#606060FF>不能提升</color>";
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
                Graphic[] componentsInChildren = WindowManage.instance.informationWindow.GetComponentsInChildren<Graphic>();
                foreach (Graphic component2 in componentsInChildren)
                {
                    component2.CrossFadeAlpha(1f, 0.2f, true);
                }
                ___tipsW = Math.Max(Main.settings.minWidth, shown * 95);
            }
            return text;
        }

        // 人物能力值
        public static string SetInfoMessage3(int actorId)
        {
            if (!Main.settings.showLevel) return "";
            string text = "\n";
            foreach (int i in DateFile.instance.baseSkillDate.Keys)
            {
                if (!smallerWindow)
                {
                    int typ = (i < 100 ? 501 : 500) + i;
                    int b = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), actorId, false, false) / 6000;
                    int level = Mathf.Clamp(Mathf.Min(MassageWindow.instance.GetSkillValue(actorId, typ) - 1, b), 0, 8);
                    string[] marks = { "❾", "❽", "❼", "❻", "❺", "❹", "❸", "❷", "❶" };
                    string mark = Main.settings.useColorOfTeachingSkill ? DateFile.instance.SetColoer(20002 + level, marks[level]) : "※";
                    text += CanTeach(actorId, i) ? mark : "　";
                }
                text += GetLevel(actorId, i);
                text += i % 4 == (i < 100 ? 3 : 0) ? "\n" : "\t";
                if (i == 15) text += "\n";
            }
            return text + "\n";
        }
        //资质
        public static string GetLevel(int id, int index)
        {
            int typ = (index < 100 ? 501 : 500) + index;
            int skillValue = GetSkillValue(id, typ);
            int skillDiffer = skillValue - int.Parse(DateFile.instance.GetActorDate(id, typ, false));
            string familySkill = smallerWindow || !Main.settings.showFamilySkill ? "" : GetFamilySkill(id, index) + ",";
            bool shownoadd = !smallerWindow && Main.settings.addonInfo && skillDiffer != 0;
            string text = DateFile.instance.SetColoer(20001 + Mathf.Clamp(GetMaxSkillLevel(id, typ), 1, 9),
            string.Format("{0}<color=orange>{1,3}{2}</color>{3,3}{4}<color=#606060ff>{5,3}{6}</color>",
                DateFile.instance.baseSkillDate[index][0],
                familySkill,
                familySkill.Length > 0 && familySkill.Length <= 3 ? "\u00A0" + (familySkill.Length <= 2 ? "\u00A0\u00A0" : "") : "",
                skillValue.ToString(),
                skillValue < 100 ? "\u00A0" + (skillValue < 10 ? "\u00A0\u00A0" : "") : "",
                smallerWindow ? "" : (shownoadd ? (skillDiffer < 0 ? "\u00A0" : "+") + skillDiffer.ToString() : "\t"),
                smallerWindow ? "" : (shownoadd && Math.Abs(skillDiffer) < 10 ? "\u00A0\u00A0" : "")));
            return text;
        }
        //造詣
        public static string GetFamilySkill(int key, int index)
        {
            int mainActorID = DateFile.instance.MianActorID();
            string text = (key != mainActorID) ? (DateFile.instance.GetActorValue(key, (index < 100 ? 501 : 500) + index, true) - int.Parse(DateFile.instance.GetActorDate(key, (index < 100 ? 501 : 500) + index, true))).ToString() : DateFile.instance.GetFamilySkillLevel(index, false).ToString();
            if (!Main.settings.showIV)
            {
                int[] closeLevel = { 5, 3, 4, 6, 99 };
                int goodness = DateFile.instance.GetActorGoodness(key);
                text = key != mainActorID && DateFile.instance.GetActorFavor(false, mainActorID, key, true, false) < closeLevel[goodness > 4 ? 0 : goodness] ? "???" : "≈" + (int.Parse(text) / 10 * 10).ToString();
            }
            return text;
        }

        //資質現值
        public static int GetSkillValue(int actorId, int typ) => int.Parse(DateFile.instance.GetActorDate(actorId, typ, true));
        //資質原值
        public static int GetSkillBase(int actorId, int typ) => int.Parse(DateFile.instance.GetActorDate(actorId, typ, false));
        //顏色基礎值
        public static int GetGangFamilySkillBase(int typ) => int.Parse(DateFile.instance.presetGangGroupDateValue[Main.settings.colorLevelBaseOn < 4 ? int.Parse("101,102,103,104".Split(',')[Main.settings.colorLevelBaseOn]) : (Main.settings.colorLevelBaseOn == 4 || typ >= 600 ? 10 : 30) + int.Parse("4,4,4,4,4,4,7,7,6,6,7,7,8,8,8,8,0".Split(',')[typ < 600 ? typ - 501 : 16])][typ < 600 ? 815 : 816]);
        //最大造詣
        public static int GetMaxFamilySkill(int actorId, int typ) => GetSkillBase(actorId, typ) * (GetGangFamilySkillBase(typ) + 100) / 100;
        //最大資質
        public static int GetMaxSkillLevel(int actorId, int typ) => GetSkillValue(actorId, typ) / 30 + (GetSkillValue(actorId, typ) + GetMaxFamilySkill(actorId, typ)) / 90;

        // 根據NPC的門派或職業,判斷能否傳授你這生活藝能
        public static bool CanTeach(int actorID, int skillId)
        {
            int eventID;
            List<int> eventIDs;

            if (skillId >= 0 && skillId <= 5)
                eventID = 931900001 + skillId;
            else if (skillId == 6 || skillId == 7)
                eventID = 932200001 + skillId - 6;
            else if (skillId == 8 || skillId == 9)
                eventID = 932900001 + skillId - 8;
            else if (skillId == 10 || skillId == 11)
                eventID = 932200003 + skillId - 10;
            else if (skillId >= 12 && skillId <= 15)
                eventID = 932300001 + skillId - 12;
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
        public static string SetInfoMessage5(int id) => Main.settings.lifeMessage && !smallerWindow ? "\n" + GetLifeMessage(id, 3) : "";

        //心情
        public static string GetMood(int id) => ActorMenu.instance.Color2(DateFile.instance.GetActorDate(id, 4, false));

        //印象
        public static string GetLifeFace(int actorId)
        {
            string text = "<color=#606060FF>无</color>";
            if (DateFile.instance.HaveLifeDate(actorId, 1001))
            {
                int faceId = DateFile.instance.GetLifeDate(actorId, 1001, 0);
                int lifeDate = DateFile.instance.GetLifeDate(actorId, 1001, 1);
                text = String.Format("<color=white>{0}</color><color=lime>{1}%</color>", DateFile.instance.identityDate[faceId][0], lifeDate);
            }
            return text;
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
        public static string GetFame(int id) => ActorMenu.instance.Color7(GetActorFame(id));
        public static int GetActorFame(int id) => DateFile.instance.GetActorFame(id);

        //立场
        public static string GetGoodness(int id) => DateFile.instance.massageDate[9][0].Split('|')[DateFile.instance.GetActorGoodness(id)];

        //生育能力
        public static int GetFertility(int id, bool plus)
        {
            int result = int.Parse(DateFile.instance.GetActorDate(id, 24, plus));
            if (!plus) result = int.Parse(DateFile.instance.GetActorDate(id, 24, true)) - result;
            return result;
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

        //年龄
        public static int GetAge(int id) => int.Parse(DateFile.instance.GetActorDate(id, 11, false));

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
                    //商品等级
                    int level = DateFile.instance.GetActorValue(id, 506, false) * 10;
                    //实际花费
                    int num = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), id, true, false);
                    int shopSellCost = 30 + (num * 5);
                    //花费等级
                    int[] moneyCost = { 250, 200, 225, 275, 300 }; // 此處貎似遊戲的BUG, 本應是 {250, 225, 200, 275, 300}
                    int shopSystemCost = moneyCost[DateFile.instance.GetActorGoodness(id)] - (num * 15);

                    text += "(Lv:" + DateFile.instance.storyShopLevel[int.Parse(DateFile.instance.GetGangDate(typ, 16))].ToString() + "+" + level.ToString() + ",Cost:" + shopSystemCost.ToString() + "/" + shopSellCost.ToString() + ")";
                }
            }
            return text;
        }

        // 所屬地
        public static string GetActorGang(int actorid) => DateFile.instance.GetGangDate(int.Parse(DateFile.instance.GetActorDate(actorid, 19, false)), 0);

        //人物在组织中等级ID
        public static int GetGangLevelId(int actorid) => DateFile.instance.GetGangValueId(int.Parse(DateFile.instance.GetActorDate(actorid, 19, false)), int.Parse(DateFile.instance.GetActorDate(actorid, 20, false)));

        //人物在组织中等级名称
        public static string GetGangLevelText(int actorid) => DateFile.instance.presetGangGroupDateValue[GetGangLevelId(actorid)][int.Parse(DateFile.instance.GetActorDate(actorid, 20, false)) >= 0 ? 1001 : 1001 + int.Parse(DateFile.instance.GetActorDate(actorid, 14, false))];
        public static string GetGangLevelColorText(int id) => DateFile.instance.SetColoer(20011 - Mathf.Abs(int.Parse(DateFile.instance.GetActorDate(id, 20, false))), GetGangLevelText(id));

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

        public static int GetGongfaLevel(int id) => DateFile.instance.gongFaDate.ContainsKey(id) ? int.Parse(DateFile.instance.gongFaDate[id][2]) : 0;

        public static string GetGongfaText(int id) => DateFile.instance.gongFaDate.ContainsKey(id) ? DateFile.instance.gongFaDate[id][0] : "";

        public static string GetGongfaColorText(int id) => DateFile.instance.SetColoer(20001 + GetGongfaLevel(id), GetGongfaText(id));

        public static List<int> GetGongfaList(int id)
        {
            List<int> gongFas = DateFile.instance.actorGongFas.ContainsKey(id) ? new List<int>(DateFile.instance.actorGongFas[id].Keys) : new List<int>(); //避免存取死人資料時引發紅字
            gongFas.RemoveAll(t => t == 0); // 刪除多餘的功法ID=0
            return gongFas;
        }

        //人物身上最高级功法获取
        public static string GetBestGongfa(int id) => (DateFile.instance.actorGongFas.ContainsKey(id) ? "最佳功法: " + GetBestGongfaText(GetGongfaList(id)) : DateFile.instance.SetColoer(20002, GetGenderTA(id) + "还没来得及学")) + "\n";

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

            List<int> list = new List<int>(ActorMenu.instance.GetActorItems(id, 0, false).Keys);
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
        public static string GetWorkPlace(int partId, int placeId, int buildingIndex)
        {
            string text = "";
            int[] buildingData = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int buildType = buildingData[0];
            int buildLv = buildingData[1];
            text += DateFile.instance.basehomePlaceDate[buildType][0];
            text += " - Lv." + buildLv;
            return text;
        }

        //村民工作地点現在收获进度
        public static string GetWorkingProgress(int partId, int placeId, int buildingIndex)
        {
            string text = "";
            int[] buildingData = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int buildingType = buildingData[0];
            Dictionary<int, string> buildingSetting = DateFile.instance.basehomePlaceDate[buildingType];

            int currentXp = buildingData[11];
            int BuildingMaxXp = int.Parse(buildingSetting[91]);
            int efficient = HomeSystem.instance.GetBuildingLevelPct(partId, placeId, buildingIndex);
            text += buildingType == 1003 ? "此人正在厢房摸鱼……" : string.Format("{0:0.#}% (+{1:0.#}%{2})", (float)currentXp / BuildingMaxXp * 100, (float)efficient * 100 / BuildingMaxXp, DateFile.instance.massageDate[7006][1]);

            return text;
        }

        //工作效率
        public static string GetExpectEfficient(int workerId, int partId, int placeId, int buildingIndex)
        {
            int[] buildingData = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int buildingType = buildingData[0];
            int buildingLv = buildingData[1];
            Dictionary<int, string> buildingSetting = DateFile.instance.basehomePlaceDate[buildingType];

            int skillId = int.Parse(buildingSetting[33]);
            int BuildingMaxXp = int.Parse(buildingSetting[91]);
            if (skillId <= 0 || BuildingMaxXp <= 0) return "";

            int mood = int.Parse(DateFile.instance.GetActorDate(workerId, 4, false));
            int favorLvl = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), workerId, true, false);//[0-6]

            int moodBonus = 40 + favorLvl * 10;
            if (mood <= 0) moodBonus -= 30;
            else if (mood <= 20) moodBonus -= 20;
            else if (mood <= 40) moodBonus -= 10;
            else if (mood >= 100) moodBonus += 30;
            else if (mood >= 80) moodBonus += 20;
            else if (mood >= 60) moodBonus += 10;

            int workerSkill = int.Parse(DateFile.instance.GetActorDate(workerId, skillId, true));
            if (skillId == 18) workerSkill += 100;  //镖局,知客亭,炼神峰: 以名譽+100計算

            int NeighborBonus = 0;
            foreach (int key in HomeSystem.instance.GetBuildingNeighbor(partId, placeId, buildingIndex, 1))
            {
                if (DateFile.instance.homeBuildingsDate[partId][placeId].ContainsKey(key) && DateFile.instance.actorsWorkingDate[partId][placeId].ContainsKey(key) && int.Parse(DateFile.instance.basehomePlaceDate[DateFile.instance.homeBuildingsDate[partId][placeId][key][0]][62]) != 0)
                {
                    int skillBonus = int.Parse(DateFile.instance.GetActorDate(DateFile.instance.actorsWorkingDate[partId][placeId][key], skillId, true));
                    if (skillId == 18) skillBonus += 100;  //镖局,知客亭,炼神峰 : 以名譽+100計算
                    NeighborBonus += skillBonus;
                }
            }
            int skillRequirement = Mathf.Max(int.Parse(buildingSetting[51]) + buildingLv - 1, 1);
            workerSkill = (workerSkill + NeighborBonus) * Mathf.Max(moodBonus, 0);

            int efficiency = Mathf.Clamp(workerSkill / skillRequirement, 50, 200);
            return string.Format("+{0:0.#}%{1}", (float)efficiency / BuildingMaxXp * 100, DateFile.instance.massageDate[7006][1]);
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

        //婚姻 / 性取向
        public static string GetSpouse(int id)
        {
            List<int> actorSocial = DateFile.instance.GetActorSocial(id, 309, false, true);
            List<int> actorSocial2 = DateFile.instance.GetActorSocial(id, 309, true, true);
            string result = actorSocial2.Count == 0 ? DateFile.instance.SetColoer(20004, "未婚", false) : (actorSocial.Count == 0 ? DateFile.instance.SetColoer(20007, "丧偶", false) : DateFile.instance.SetColoer(20010, "已婚", false));
            if (Main.settings.showSexuality && !isDead) result += " • " + GetSexuality(id);
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

        public static string GetSexuality(int id) => DateFile.instance.GetActorDate(id, 21, false) == "0" ? "直" : "双";
        public static string GetGenderText(int id) => DateFile.instance.massageDate[7][0].Split('|')[int.Parse(DateFile.instance.GetActorDate(id, 14, false))];
        public static string GetGenderTA(int id) => DateFile.instance.GetActorDate(id, 14, false) == "2" ? "她" : "他";
        public static string PurifyItemName(String name) => name.Split('(')[0].Split('\n')[0];
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

        public static void ClearResourceHolder() => UnityEngine.Object.Destroy(GetResourceHolder().gameObject);

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
                AddText("FavLabel", favourHolder.Find("ActorFavor1"), DateFile.instance.massageDate[7][3] + ":", -48f, -9f, 60f, 30f, Color.yellow);
                AddText("WariLabel", favourHolder.Find("wariness"), DateFile.instance.massageDate[7][4] + ":", -48f, -9f, 60f, 30f, Color.yellow);
            }
            return favourHolder;
        }

        public static void ClearFavourHolder() => UnityEngine.Object.Destroy(GetFavourHolder().gameObject);

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
