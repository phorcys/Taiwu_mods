using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace CharacterFloatInfo
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry) => Save(this, modEntry);

        /// <summary>比对原始信息</summary>
        public bool addonInfo = false;
        /// <summary></summary>
        public bool shopName = false;
        /// <summary></summary>
        public bool workPlace = false;
        /// <summary>显示心情</summary>
        public bool showMood = false;
        /// <summary>人物属性</summary>
        public bool showLevel = true;
        /// <summary>不显示商店的详细信息</summary>
        public bool hideShopInfo = true;
        /// <summary>不显示儿童的魅力</summary>
        public bool hideCharmOfChildren = true;
        /// <summary>用颜色及數字 標記 可以请教的技艺最高品階</summary>
        public bool useColorOfTeachingSkill = false;
        /// <summary>显示性向</summary>
        public bool showSexuality = false;

        /// <summary>人物ID</summary>
        public bool showActorId = false;
        /// <summary>人物状况</summary>
        public bool showActorStatus = true;
        /// <summary>人物经历</summary>
        public bool lifeMessage = false;
        /// <summary>人物技艺</summary>
        public bool showCharacteristic = true;
        /// <summary>技艺功法造诣</summary>
        public bool showFamilySkill = false;
        /// <summary>显示被隐藏了的人物特性</summary>
        public bool showIV = false;
        /// <summary>七元賦性</summary>
        public bool showResources = false;
        /// <summary>显示身上品质最高的物品与功法及可学功法</summary>
        public bool showBest = true;

        /// <summary>死亡人物显示信息</summary>
        public bool deadActor = false;

        /// <summary>地块人物</summary>
        public bool enableMAL = true;
        /// <summary>地块邻格</summary>
        public bool enableMAN = false;
        /// <summary>对话对象</summary>
        public bool enableDI = false;
        /// <summary>村民分配</summary>
        public bool enableBW = false;
        /// <summary>主画面同道</summary>
        public bool enableTA = false;
        /// <summary>同道列表</summary>
        public bool enableAM = false;
        /// <summary>人物关系</summary>
        public bool enableRI = false;
        /// <summary>对话选择人物</summary>
        public bool enableMAC = false;
        /// <summary>新村民</summary>
        public bool enableWNV = false;
        /// <summary>简约地块人物列表</summary>
        public bool shortMAL = false;
        /// <summary>简约对话界面</summary>
        public bool shortDI = false;
        /// <summary>简约主角及同道头像</summary>
        public bool shortTA = false;
        /// <summary>简约人物信息界面</summary>
        public bool shortAM = false;
        /// <summary>简约人物关系界面</summary>
        public bool shortRI = false;
        /// <summary></summary>
        public int minWidth = 850;
        /// <summary>颜色计算基于</summary>
        public int colorLevelBaseOn = 0;

        /// <summary>其他人所属商会</summary>
        public bool showShopid = false;
        /// <summary>工作村民</summary>
        public bool enableWA = false;
        /// <summary></summary>
        public bool showGameObjectMassage = false;
    }

    public static class Main
    {
        internal static bool enabled;
        internal static Settings settings;
        internal static UnityModManager.ModEntry.ModLogger Logger;

        private static Version gameVersion;
        internal static Version GameVersion
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

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("浮窗显示区域");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            settings.enableMAL = GUILayout.Toggle(settings.enableMAL, "地块人物");
            settings.enableMAN = GUILayout.Toggle(settings.enableMAN, "地块邻格");
            settings.enableTA = GUILayout.Toggle(settings.enableTA, "主画面同道");
            settings.enableDI = GUILayout.Toggle(settings.enableDI, "对话对象");
            settings.enableMAC = GUILayout.Toggle(settings.enableMAC, "对话选择人物");
            settings.enableBW = GUILayout.Toggle(settings.enableBW, "村民分配");
            settings.enableAM = GUILayout.Toggle(settings.enableAM, "同道列表");
            settings.enableRI = GUILayout.Toggle(settings.enableRI, "人物关系");
            settings.enableWNV = GUILayout.Toggle(settings.enableWNV, "新村民");
            settings.enableWA = GUILayout.Toggle(settings.enableWA, "工作村民");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("展示內容");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            settings.showActorId = GUILayout.Toggle(settings.showActorId, "人物ID");
            settings.showActorStatus = GUILayout.Toggle(settings.showActorStatus, "人物状况");
            settings.showCharacteristic = GUILayout.Toggle(settings.showCharacteristic, "人物特性");
            settings.showLevel = GUILayout.Toggle(settings.showLevel, "人物属性");
            settings.showFamilySkill = GUILayout.Toggle(settings.showFamilySkill, "技艺造诣");
            settings.showResources = GUILayout.Toggle(settings.showResources, "七元賦性");
            settings.showBest = GUILayout.Toggle(settings.showBest, "最佳物品、功法");
            settings.lifeMessage = GUILayout.Toggle(settings.lifeMessage, "人物经历");
            settings.showShopid = GUILayout.Toggle(settings.showShopid, "其他人所属商会");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("简约显示");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            settings.shortMAL = GUILayout.Toggle(settings.shortMAL, "地块人物列表");
            settings.shortTA = GUILayout.Toggle(settings.shortTA, "主角及同道头像");
            settings.shortDI = GUILayout.Toggle(settings.shortDI, "对话界面");
            settings.shortAM = GUILayout.Toggle(settings.shortAM, "人物信息界面");
            settings.shortRI = GUILayout.Toggle(settings.shortRI, "人物关系界面");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("其他设定");
            GUILayout.EndHorizontal();
            settings.showSexuality = GUILayout.Toggle(settings.showSexuality, "显示性取向");
            settings.addonInfo = GUILayout.Toggle(settings.addonInfo, "比对原始信息");
            settings.deadActor = GUILayout.Toggle(settings.deadActor, "显示已故人物信息");
            settings.showIV = GUILayout.Toggle(settings.showIV, "显示隐藏的人物特性");
            settings.hideShopInfo = GUILayout.Toggle(settings.hideShopInfo, "隐藏商人的详细信息");

            settings.useColorOfTeachingSkill = GUILayout.Toggle(settings.useColorOfTeachingSkill, "标记可学技艺的最高品阶");
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("颜色计算基于", GUILayout.Width(150));
            settings.colorLevelBaseOn = GUILayout.SelectionGrid(settings.colorLevelBaseOn, new string[] { "门派红字", "门派橙字", "门派黄字", "门派紫字", "大城", "村镇关寨" }, 6);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("窗口最小宽度", GUILayout.Width(150));
            if (int.TryParse(GUILayout.TextArea(settings.minWidth.ToString(), GUILayout.Width(50)), out var value))
            {
                settings.minWidth = value;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry) => settings.Save(modEntry);
    }

    /// <summary>
    /// 輪迴NPC列表
    /// </summary>
    [HarmonyPatch(typeof(SetSamsaraActor), "SetActor")]
    internal static class SetSamsaraActor_SetActor_Patch
    {
        public static void Postfix(ActorFace ___mianActorFace)
        {
            if (!Main.enabled) return;
            GameObject actor = ___mianActorFace.transform.parent.parent.parent.gameObject;
            if (actor.GetComponents<PointerEnter>().Count() == 0)
            {
                actor.AddComponent<PointerEnter>();
            }
        }
    }

    /// <summary>
    /// 同道人列表(點擊主角時)
    /// </summary>
    [HarmonyPatch(typeof(SetListActor), "SetActor")]
    internal static class SetListActor_SetActor_Patch
    {
        public static void Postfix(ActorFace ___mianActorFace)
        {
            if (!Main.enabled) return;
            GameObject actor = ___mianActorFace.transform.parent.parent.parent.gameObject;
            if (actor.GetComponents<PointerEnter>().Count() == 0)
            {
                actor.AddComponent<PointerEnter>();
            }
        }
    }

    /// <summary>
    /// 大地圖上的主要NPC列表
    /// </summary>
    [HarmonyPatch(typeof(SetPlaceActor), "SetActor")]
    internal static class SetPlaceActor_SetActor_Patch
    {
        public static void Postfix(bool show, ActorFace ___mianActorFace)
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

    /// <summary>
    /// 村內建築中選擇NPC時
    /// </summary>
    [HarmonyPatch(typeof(SetWorkActorIcon), "SetActor")]
    internal static class SetWorkActorIcon_SetActor_Patch
    {
        public static void Postfix(ActorFace ___mianActorFace)
        {
            if (!Main.enabled) return;
            GameObject actor = ___mianActorFace.transform.parent.parent.parent.gameObject;
            if (actor.GetComponents<PointerEnter>().Count() == 0)
            {
                actor.AddComponent<PointerEnter>();
            }
        }
    }

    /// <summary>
    /// 工作间正在工作的村民
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "ShowWorkingActor")]
    internal static class HomeSystem_ShowWorkingActor_Patch
    {
        public static void Postfix(bool show, ActorFace ___mianActorFace)
        {
            if (!Main.enabled) return;
            if (show)
            {
                GameObject actor = ___mianActorFace.transform.parent.parent.parent.gameObject;
                if (actor.GetComponents<PointerEnter>().Count() == 0)
                {
                    actor.AddComponent<PointerEnter>();
                }
            }
        }
    }

    /// <summary>
    /// 對話時,彈出的NPC選擇窗
    /// </summary>
    [HarmonyPatch(typeof(MassageWindow), "GetActor")]
    internal static class MassageWindow_GetActor_Patch
    {
        public static void Postfix(Transform ___actorHolder)
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

    /// <summary>
    /// 新村民加入
    /// </summary>
    [HarmonyPatch(typeof(GetActorWindow), "ShowGetActorWindow")]
    internal static class GetActorWindow_ShowGetActorWindow_Patch
    {
        public static void Postfix(List<int> actorId, GetActorWindow __instance)
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
    internal static class WindowManage_WindowSwitch_Patch
    {
        /// <summary>门派任务EventIDs</summary>
        private static readonly HashSet<int> martialQuests = new HashSet<int> { 9301, 9303, 9304, 9305, 9307, 9308, 9310, 9312, 9315, 9366, 9321 };
        /// <summary>人物身份附加值</summary>
        private static readonly int[] gangGroupIndexAdd = new int[] { 4, 4, 4, 4, 4, 4, 7, 7, 6, 6, 7, 7, 8, 8, 8, 8, 1 };
        /// <summary>角色经历缓存</summary>
        private static readonly List<string> actorMassageCache = new List<string>();
        /// <summary>浮动信息最近一次显示的角色ID</summary>
        private static int lastActorID = 0;
        /// <summary>角色已死亡</summary>
        private static bool isDead;
        /// <summary>小窗口</summary>
        private static bool smallerWindow;
        /// <summary>窗口类型</summary>
        private static WindowType windowType;

        /// <summary>
        /// 窗口类型
        /// </summary>
        private enum WindowType
        {
            MapActorList,
            Dialog,
            BuildingWindow,
            TeamActor,
            ActorMenu,
            Relationship,
            DialogChooseActors,
            WorkerNewVillager,
            WorkingActor,
        };

        /// <summary>
        /// 是否显示小窗口
        /// </summary>
        /// <returns></returns>
        private static bool CheckShort()
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
                case WindowType.WorkingActor:
                case WindowType.BuildingWindow: return true;
            }
            return false;
        }

        /// <summary>
        /// 人物头像获取人物ID
        /// </summary>
        /// <param name="tips"></param>
        /// <returns></returns>
        private static int GetId(GameObject tips)
        {
            string[] array = tips.name.Split(',');
            int id = array.Length > 1 ? int.Parse(array[1]) : 0;

            if (id == 0)
            {
                var count = tips.transform.childCount;
                for (int i = 0; i < count; i++)
                {
                    var item = tips.transform.GetChild(i);
                    if (item.name.StartsWith("Actor,"))
                    {
                        id = int.Parse(item.name.Split(',')[1]);
                        break;
                    }
                }
            }
            return id;
        }

        private static void Postfix(bool on, GameObject tips, ref Text ___itemMoneyText, ref Text ___itemLevelText, ref Text ___informationMassage, ref Text ___informationName, ref bool ___anTips, ref int ___tipsW, ref int ___tipsH)
        {
            if (!on || !Main.enabled || ActorMenu.instance == null || tips == null) return;

            bool needShow = false;
            string[] array = tips.name.Split(',');
            int id = GetId(tips);

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
            if (array[0] == "PlaceActor(Clone)" && tips.transform.parent.name == "ActorHolder" && DateFile.instance.actorsDate.ContainsKey(id))
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
            else
            if (array[0] == "NameIcon" && !ActorMenu.instance.actorMenu.activeSelf)
            {
                int partId = HomeSystem.instance.homeMapPartId;
                int placeId = HomeSystem.instance.homeMapPlaceId;
                int buildingIndex = HomeSystem.instance.homeMapbuildingIndex;
                if (DateFile.instance.actorsWorkingDate.TryGetValue(partId, out var actorWorkingData) && actorWorkingData.TryGetValue(placeId, out var workingData))
                {
                    if (workingData.TryGetValue(buildingIndex, out id))
                    {
                        needShow = Main.settings.enableWA;
                        windowType = WindowType.WorkingActor;
                    }
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
                    ___tipsW = Main.settings.minWidth;
                    ___itemLevelText.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 0);
                    ___itemMoneyText.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 0);
                }

                ___itemLevelText.text = SetLevelText(id);
                ___itemMoneyText.text = SetFavorText(id);

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

        /// <summary>
        /// 标题栏左侧小字号文本
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string SetLevelText(int id)
        {
            string text = "";
            text += $"{(isDead ? "享年" : "岁数")}:\u00A0{GetAge(id)}\n";
            text += $"寿命:\u00A0{GetHealth(id)}";
            return text;
        }

        /// <summary>
        /// 标题栏右侧侧小字号文本
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        private static string SetFavorText(int actorId)
        {
            string text = "";
            if (actorId != DateFile.instance.MianActorID())
            {
                Transform favourHolder = GetFavourHolder();
                Transform actorFavorHolder = favourHolder.Find("ActorFavor1");
                Image actorFavorBar1 = actorFavorHolder.Find("ActorFavorBar1").GetComponent<Image>();
                Image actorFavorBar2 = actorFavorHolder.Find("ActorFavorBar2").GetComponent<Image>();
                var warness = favourHolder.Find("wariness");
                var warnessBar = warness.Find("WarinessFavorBar");
                Image actorWarinessBar1 = warnessBar.GetComponent<Image>();
                favourHolder.gameObject.SetActive(true);
                actorFavorHolder.gameObject.SetActive(true);
                warness.gameObject.SetActive(true);
                warnessBar.gameObject.SetActive(true);
                int actorFavor = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), actorId, false, false);
                actorFavorBar1.fillAmount = actorFavor / 30000f;
                actorFavorBar2.fillAmount = (actorFavor - 30000) / 30000f;
                actorWarinessBar1.fillAmount = DateFile.instance.GetActorWariness(actorId) / 400f;
            }
            else
            {
                ClearFavourHolder();
                //text += string.Format("{0}{1}{2}", DateFile.instance.massageDate[16][1], DateFile.instance.samsara, DateFile.instance.massageDate[16][2]); // 第X代太吾
                text += $"促织福缘 <color>{DateFile.instance.getQuquTrun}</color>";
                text += $"\n生平遗惠 <color>{GetActorScore()}</color>";
            }
            return text;
        }

        /// <summary>
        /// 生平遺惠
        /// </summary>
        /// <returns></returns>
        /// <remarks><see cref="ActorScore.ShowActorScoreWindow(int)"/></remarks>
        private static int GetActorScore()
        {
            int mainId = DateFile.instance.MianActorID();
            if (!DateFile.instance.actorGetScore.TryGetValue(mainId, out var actorScores))
                return 0;

            int[] array = new int[7];
            foreach (var actorScore in actorScores)
            {
                if (actorScore.Key != 0)
                {
                    var scoreItems = new Dictionary<int, int>();
                    foreach (var scoreItem in actorScore.Value)
                    {
                        scoreItems[scoreItem[0]] = scoreItems.TryGetValue(scoreItem[0], out var score) ? scoreItem[1] + score : scoreItem[1];
                    }
                    foreach (var scoreItem in scoreItems)
                    {
                        int scoreLimit = int.Parse(DateFile.instance.scoreValueDate[scoreItem.Key][3]);
                        int ratio = 100;
                        foreach (var str in DateFile.instance.scoreValueDate[scoreItem.Key][4].Split('|'))
                        {
                            switch (str)
                            {
                                case "1":
                                    ratio += DateFile.instance.enemyBorn * DateFile.instance.enemyBorn * 40;
                                    break;
                                case "2":
                                    ratio += DateFile.instance.enemySize * DateFile.instance.enemySize * 10;
                                    break;
                                case "3":
                                    ratio += DateFile.instance.xxLevel * DateFile.instance.xxLevel * 30;
                                    break;
                                case "4":
                                    ratio += DateFile.instance.worldResource * DateFile.instance.worldResource * 10;
                                    break;
                            }
                        }
                        scoreLimit = scoreLimit * ratio / 100;
                        array[actorScore.Key - 1] += Mathf.Min(scoreItem.Value, scoreLimit);
                    }
                }
            }
            return array.Sum();
        }

        /// <summary>
        /// 标题栏
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string SetInfoName(int id) => $"{DateFile.instance.GetActorName(id, true, false)}{GetIdString(id)}{ShowActorStatus(id)}";

        /// <summary>
        /// 获取角色ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string GetIdString(int id) => Main.settings.showActorId ? $"(No.{id})" : string.Empty;

        /// <summary>
        /// 狀況
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string ShowActorStatus(int id)
        {
            if (!Main.settings.showActorStatus)
                return "";

            string text = "\n";
            string seperator = "";

            if (windowType == WindowType.Relationship || windowType == WindowType.Dialog)
            {
                text += GetActorGang(id); // 所屬地
                text += GetGangLevelColorText(id); // 地位
                seperator = " • ";
            }

            text += seperator + DateFile.instance.SetColoer(20003, GetGenderText(id));
            seperator = " • ";

            if (GetAge(id) > ConstValue.actorMinAge) text += seperator + GetSpouse(id);

            if (isDead)
            {
                text += seperator + DateFile.instance.SetColoer(20009, "过世");
                // todo: 投胎至:XX村，人名
            }
            else if (DateFile.instance.actorInjuryDate.ContainsKey(id))
            {
                int Hp = ActorMenu.instance.Hp(id, false);
                int maxHp = ActorMenu.instance.MaxHp(id, 100);
                int Sp = ActorMenu.instance.Sp(id, false);
                int maxSp = ActorMenu.instance.MaxSp(id, 100);
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

            if (windowType == WindowType.BuildingWindow || windowType == WindowType.WorkingActor)
                return text;
            if (GetGangLevelText(id) == "商人")
            {
                text += seperator + DateFile.instance.SetColoer(20006, GetShopName(id));
            }
            else
            {
                if (Main.settings.showShopid)
                {
                    text += seperator + DateFile.instance.SetColoer(20002, GetShopName(id));
                }
            }
            text += seperator + "战" + DateFile.instance.GetActorDate(id, 993, true);
            return text;
        }

        /// <summary>
        /// 文本
        /// </summary>
        /// <param name="id"></param>
        /// <param name="___tipsW"></param>
        /// <returns></returns>
        private static string SetInfoMessage(int id, ref int ___tipsW) => SetInfoMessage1(id, ref ___tipsW) + "\n" + SetInfoMessage2(id, ref ___tipsW, 10, out var rowCount) + SetInfoMessage3(id) + SetInfoMessage4(id, rowCount) + SetInfoMessage5(id);

        /// <summary>
        /// 主要信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string SetInfoMessage1(int id, ref int ___tipsW)
        {
            var text = new StringBuilder();
            text.Append("魅力:" + GetCharm(id));
            text.Append("\t立场:" + GetGoodness(id));
            text.Append("\t名誉:" + GetFame(id));
            text.Append("\t心情:" + GetMood(id));
            text.Append("\t印象:" + GetLifeFace(id));
            int evil = DateFile.instance.GetLifeDate(id, 501, 0);
            text.Append("\t入魔:" + (evil == -1 ? 0 : evil));
            text.Append("\n\n");

            if (!smallerWindow)
            {
                text.Append($"轮回:<color=white>{GetSamsara(id)}</color>");

                text.Append(GetSamsara(id) != "0" ? $" ◆ 前世:<color=white>{GetSamsaraName(id)}</color>" : "");

                string item = Gethobby(id, 0);
                text.Append($"\t喜好:<color={(item == "未知" ? "gray" : "white")}>{item}</color>");
                item = Gethobby(id, 1);
                text.Append($"\t厌恶:<color={(item == "未知" ? "gray" : "white")}>{item}</color>");

                if (Main.settings.showIV)
                {
                    if (DateFile.instance.HaveLifeDate(id, 901))
                    {
                        List<int> list = DateFile.instance.actorLife[id][901];
                        int rest = list[0];
                        if (rest > 0)
                        {
                            if (rest > 1000)
                            {
                                text.Append($"\t孕期剩余<color=white>{rest - 1000}</color>个月");
                            }
                            else
                            {
                                text.Append($"\t孕期剩余<color=white>{rest}</color>个月");
                                //if (rest == 2 || rest == 4 || rest == 6) text += $",下个月可能出现胎教事件";
                            }
                        }
                        else
                        {
                            rest += 100;
                            text.Append($"\t{rest}个月内无法怀孕");
                        }
                    }
                    else
                    {
                        int result1 = GetFertility(id, true);
                        int result2 = GetFertility(id, false);
                        text.Append($"\t生育:<color={(result2 > 0 ? "+" : "") + result2}>{result1}%</color>");
                        if (Main.settings.addonInfo && result2 != 0)
                            text.Append($"\t<color=#606060FF>{(result2 > 0 ? "+" : "") + result2}%</color>");
                    }
                }

                if (GetAge(id) > ConstValue.actorMinAge)
                    text.Append($"\t子嗣:<color=white>{DateFile.instance.GetActorSocial(id, 310, false, false).Count}</color>");
                // todo: 改為顯示所有孩子名

                text.Append($"\t威望:<color=white>{int.Parse(DateFile.instance.GetActorDate(id, 407, false))}</color>");

                text.Append($"\t银钱:<color=white>{ActorMenu.instance.ActorResource(id)[5]}</color>");
            }
            else if (windowType == WindowType.BuildingWindow || windowType == WindowType.WorkingActor)
            {
                if (HomeSystem.instance == null || !HomeSystem.instance.buildingWindowOpend)
                    return null;

                int partId, placeId, buildingIndex;
                var date = DateFile.instance.actorsWorkingDate;

                text.Append("工作岗位:");

                int[] city = DateFile.instance.ActorIsWorking(id);
                if (city == null)
                {
                    text.Append("<color=#606060FF>无</color>");
                }
                else
                {
                    partId = city[0];
                    placeId = city[1];
                    if (!date.TryGetValue(partId, out var partWorkingData))
                        return "";
                    if (!partWorkingData.TryGetValue(placeId, out var workingData))
                        return "";
                    var aBuilding = workingData.FirstOrDefault(t => t.Value == id);
                    if (aBuilding.Equals(default(KeyValuePair<int, int>)))
                    {
                        text.Append("<color=red>未知</color>");
                    }
                    else
                    {
                        buildingIndex = aBuilding.Key;
                        text.Append($"<color=white>{GetWorkPlace(partId, placeId, buildingIndex)}</color>");
                        text.Append("\t进度:<color=white>" + GetWorkingProgress(partId, placeId, buildingIndex) + "</color>");
                    }
                }
                partId = HomeSystem.instance.homeMapPartId;
                placeId = HomeSystem.instance.homeMapPlaceId;
                if (!date.TryGetValue(partId, out var partWorkingData2))
                    return "";
                if (!partWorkingData2.TryGetValue(placeId, out var workingData2))
                    return "";
                buildingIndex = HomeSystem.instance.homeMapbuildingIndex;
                if (!workingData2.TryGetValue(buildingIndex, out int workerId) || workerId != id)
                    text.Append($"\n村民派遣，预期效率:<color=white>{GetExpectEfficient(id, partId, placeId, buildingIndex)}</color>");

            }
            else if (windowType == WindowType.DialogChooseActors && martialQuests.Contains(MassageWindow.instance.mianEventDate[2]))
            {
                // 只在門派任務中顯示以下內容
                int gangId = int.Parse(DateFile.instance.GetActorDate(MassageWindow.instance.eventMianActorId, 19, false));
                int gangWorldId = int.Parse(DateFile.instance.GetGangDate(gangId, 11));
                int gangPartId = int.Parse(DateFile.instance.GetGangDate(gangId, 3));
                int partValue = DateFile.instance.GetBasePartValue(gangWorldId, gangPartId) / 10;
                text.Append($"地区恩义:<color=white>{partValue}%</color>");
                switch (MassageWindow.instance.mianEventDate[2])
                {
                    case 9301: // 念经忏悔
                        int fame = Math.Min(0, GetActorFame(id));
                        text.Append($"\t\t罪孽:<color={(fame < 0 ? "red" : "white")}>{fame}</color>");
                        break;
                    case 9321: // 治疗伤病
                    case 9303: // 起死回生
                        int Hp = ActorMenu.instance.Hp(id, false);
                        int maxHp = ActorMenu.instance.MaxHp(id, 100);
                        int Sp = ActorMenu.instance.Sp(id, false);
                        int maxSp = ActorMenu.instance.MaxSp(id, 100);
                        text.Append($"\t\t外伤:<color={(Hp == 0 ? "white" : "red")}>{Hp}/{maxHp}</color>\t\t内伤:<color={(Sp == 0 ? "white" : "red")}>{Sp}/{maxSp}</color>");
                        break;
                    case 9307: // 王禅典籍
                        text.Append($"\t\t处世立场:{GetGoodness(id)}");
                        break;
                    case 9310: // 秘药延寿
                        int year = ActorMenu.instance.Health(id);
                        text.Append($"\t\t寿命余下<color={(year > 1 ? "white" : "red")}>{year}</color>年");
                        break;
                    case 9366: // 驱除毒素
                    case 9312: // 五圣秘浴
                        string poise = "";
                        for (int i = 51; i <= 56; i++)
                        {
                            if (DateFile.instance.GetActorDate(id, i) != "0")
                                poise += DateFile.instance.poisonDate[i - 51][0] + ":" + DateFile.instance.GetActorDate(id, i);
                        }
                        text.Append(poise == "" ? "\t\t无中毒" : poise);
                        break;
                    case 9304: // 七星调元
                        float hurt = int.Parse(DateFile.instance.GetActorDate(id, 39)) / 10;
                        text.Append($"\t\t內息:<color={(hurt == 0 ? "white" : "red")}>{hurt:0.#}</color>");
                        break;
                    case 9308: // 玉镜沉思
                        int canDeductCount = 0, canEnchanceCount = 0;
                        foreach (int featureID in DateFile.instance.GetActorFeature(id))
                        {
                            if (int.Parse(DateFile.instance.actorFeaturesDate[featureID][8]) == 3
                                && int.Parse(DateFile.instance.actorFeaturesDate[featureID][4]) < 3)
                            {
                                canEnchanceCount++;
                            }

                            if (int.Parse(DateFile.instance.actorFeaturesDate[featureID][8]) == 4
                                && Mathf.Abs(int.Parse(DateFile.instance.actorFeaturesDate[featureID][4])) > 1)
                            {
                                canDeductCount++;
                            }
                        }
                        if (canEnchanceCount > 0)
                            text.Append("\t\t<color=white>可舞劍:" + canEnchanceCount + "</color>");
                        if (canDeductCount > 0)
                            text.Append("\t\t<color=white>可撫琴:" + canDeductCount + "</color>");
                        if (canDeductCount == 0 && canDeductCount == 0)
                            text.Append("\t\t<color=#606060FF>不能提升</color>");
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
                                    var featureName = DateFile.instance.actorFeaturesDate[featureID][0];
                                    text.Append($"\t\t状态:<color={(featureID == 9997 ? "white" : "red")}>{featureName}</color>");
                                    break;
                                }
                            }
                        }
                        break;
                }
            }
            return text.ToString();
        }

        /// <summary>
        /// 個人特性
        /// </summary>
        /// <param name="id"></param>
        /// <param name="___tipsW"></param>
        /// <param name="maxCountPerRow"></param>
        /// <returns></returns>
        private static string SetInfoMessage2(int id, ref int ___tipsW, int maxCountPerRow, out int rowCount)
        {
            string text = "";
            rowCount = 1;
            ClearActorFeatureHolder();
            if (Main.settings.showCharacteristic && !smallerWindow)
            {
                text += "\n\n\n";
                int shown = 0;
                List<int> features = DateFile.instance.GetActorFeature(id);
                if (features.Count() == 0)
                    return "";

                foreach (int featureID in features)
                {
                    if (shown == maxCountPerRow * rowCount)
                    {
                        text += "\n\n\n";
                        rowCount++;
                    }
                    if (DateFile.instance.actorFeaturesDate[featureID][95] != "1" || Main.settings.showIV) //判斷是否顯示隱藏的特性
                    {
                        Transform actorFeature = UnityEngine.Object.Instantiate(ActorMenu.instance.actorFeature, Vector3.zero, Quaternion.identity).transform;
                        actorFeature.Find("ActorFeatureNameText").GetComponent<Text>().text = DateFile.instance.actorFeaturesDate[featureID][0];
                        Transform actorFeatureStarHolder = actorFeature.Find("ActorFeatureStarHolder");
                        // 攻击倾向
                        string attack = DateFile.instance.actorFeaturesDate[featureID][1];
                        if (attack.IndexOf('|') > -1 || attack != "0")
                        {
                            foreach (string j in attack.Split('|'))
                            {
                                UnityEngine.Object.Instantiate(
                                    ActorMenu.instance.actorAttackFeatureStarIcons[int.Parse(j) > 0 ? int.Parse(j) - 1 : 3],
                                    Vector3.zero,
                                    Quaternion.identity).transform.SetParent(actorFeatureStarHolder, false);
                            }
                        }
                        // 防御倾向
                        string def = DateFile.instance.actorFeaturesDate[featureID][2];
                        if (def.IndexOf('|') > -1 || def != "0")
                        {
                            foreach (string j in def.Split('|'))
                            {
                                UnityEngine.Object.Instantiate(
                                    ActorMenu.instance.actorDefFeatureStarIcons[int.Parse(j) > 0 ? int.Parse(j) - 1 : 3],
                                    Vector3.zero,
                                    Quaternion.identity).transform.SetParent(actorFeatureStarHolder, false);
                            }
                        }
                        // 机略点类型
                        string spd = DateFile.instance.actorFeaturesDate[featureID][3];
                        if (spd.IndexOf('|') > -1 || spd != "0")
                        {
                            foreach (string j in spd.Split('|'))
                            {
                                UnityEngine.Object.Instantiate(
                                    ActorMenu.instance.actorPlanFeatureStarIcons[int.Parse(j) > 0 ? int.Parse(j) - 1 : 3],
                                    Vector3.zero,
                                    Quaternion.identity).transform.SetParent(actorFeatureStarHolder, false);
                            }
                        }

                        if (attack == "0" && def == "0" && spd == "0")
                        {
                            UnityEngine.Object.Instantiate(ActorMenu.instance.actorAttackFeatureStarIcons[3],
                                Vector3.zero,
                                Quaternion.identity).transform.SetParent(actorFeatureStarHolder, false);
                            UnityEngine.Object.Instantiate(
                                ActorMenu.instance.actorDefFeatureStarIcons[3],
                                Vector3.zero,
                                Quaternion.identity).transform.SetParent(actorFeatureStarHolder, false);
                            UnityEngine.Object.Instantiate(ActorMenu.instance.actorPlanFeatureStarIcons[3],
                                Vector3.zero,
                                Quaternion.identity).transform.SetParent(actorFeatureStarHolder, false);
                        }

                        actorFeature.SetParent(GetActorFeatureHolder(), false);
                        shown++;
                    }
                }

                Graphic[] componentsInChildren = WindowManage.instance.informationWindow.GetComponentsInChildren<Graphic>();
                foreach (Graphic component2 in componentsInChildren)
                {
                    component2.CrossFadeAlpha(1f, 0.2f, true);
                }
                ___tipsW = Math.Max(Main.settings.minWidth, (shown > maxCountPerRow ? maxCountPerRow : shown) * 95);
            }
            return text;
        }

        /// <summary>
        /// 人物能力值
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        private static string SetInfoMessage3(int actorId)
        {
            if (!Main.settings.showLevel)
                return "";
            var text = new StringBuilder("\n");
            foreach (int i in DateFile.instance.baseSkillDate.Keys)
            {
                if (!smallerWindow)
                {
                    if (CanTeach(actorId, i))
                    {
                        int typ = (i < 100 ? 501 : 500) + i;
                        // 当前好感等级
                        int favor = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), actorId, false, false) / 6000;
                        // 当前可传授最高等级
                        int maxLevel = Mathf.Min(MassageWindow.instance.GetSkillValue(actorId, typ), 8);
                        // 当前可传授等级
                        int level = Mathf.Clamp(Mathf.Min(maxLevel - 1, favor), 0, 8);
                        string[] marks = { "❾", "❽", "❼", "❻", "❺", "❹", "❸", "❷", "❶" };
                        // 文字为当前可传授等级 颜色为当前可传授最高等级
                        string mark = Main.settings.useColorOfTeachingSkill ? DateFile.instance.SetColoer(20002 + maxLevel, marks[level]) : "※";
                        text.Append(mark);
                    }
                    else
                    {
                        text.Append("　");
                    }
                }
                text.Append(GetLevel(actorId, i));
                text.Append(i % 4 == (i < 100 ? 3 : 0) ? "\n" : "\t");
                if (i == 15)
                    text.Append("\n");
            }
            return text + "\n";
        }

        /// <summary>
        /// 资质
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static string GetLevel(int actorId, int index)
        {
            int typ = (index < 100 ? 501 : 500) + index;
            int skillGongFaValue = GetSkillGongFaValue(actorId, typ);
            int differ = skillGongFaValue - int.Parse(DateFile.instance.GetActorDate(actorId, typ, false));
            string familySkillnGongFa = smallerWindow || !Main.settings.showFamilySkill ? "" : GetFamilySkillnGongFa(actorId, index) + ",";
            bool shownoadd = !smallerWindow && Main.settings.addonInfo && differ != 0;
            string text = DateFile.instance.SetColoer(20001 + Mathf.Clamp(GetMaxSkillLevel(actorId, typ), 1, 9),
            string.Format("{0}<color=orange>{1,4}{2}</color>{3,3}{4}<color=#606060ff>{5,4}{6}</color>",
                DateFile.instance.baseSkillDate[index][0],
                familySkillnGongFa,
                familySkillnGongFa.Length > 0 && familySkillnGongFa.Length <= 3 ? "\u00A0" + (familySkillnGongFa.Length <= 2 ? "\u00A0\u00A0" : "") : "",
                skillGongFaValue.ToString(),
                skillGongFaValue >= 100 ? "" : "\u00A0" + (skillGongFaValue < 10 ? "\u00A0\u00A0" : ""),
                smallerWindow ? "" : (!shownoadd ? "\t" : (differ >= 0 ? "+" : "") + differ.ToString()),
                smallerWindow ? "" : (!shownoadd ? "\t" : Math.Abs(differ) >= 100 ? "" : "\u00A0" + (Math.Abs(differ) < 10 ? "\u00A0\u00A0" : ""))
                ));
            return text;
        }

        /// <summary>
        /// 技艺和功法造诣
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="typ">资质类型</param>
        /// <returns></returns>
        /// <remarks><see cref="ActorMenu.GetGongFaValue(int)"/></remarks>
        private static string GetFamilySkillnGongFa(int actorId, int typ)
        {
            int mainActorID = DateFile.instance.MianActorID();
            int skillId = (typ < 100 ? 501 : 500) + typ;
            string text;
            if (actorId != mainActorID)
            {
                var level = (DateFile.instance.GetActorValue(actorId, skillId, true) - int.Parse(DateFile.instance.GetActorDate(actorId, skillId, true)));
                if (!Main.settings.showIV)
                {
                    // 不同立场显示造诣详情的最低好感度等级
                    int[] closeLevel = { 5, 3, 4, 6, 99 };
                    int goodness = DateFile.instance.GetActorGoodness(actorId);
                    text = DateFile.instance.GetActorFavor(false, mainActorID, actorId, true, false) < closeLevel[goodness] ? "???" : "≈" + (level / 10 * 10).ToString();
                }
                else
                {
                    text = level.ToString();
                }
            }
            else if (typ < 100)
            {
                text = DateFile.instance.GetFamilySkillLevel(typ, false).ToString();
            }
            else
            {
                text = DateFile.instance.GetFamilyGongFaLevel(mainActorID, typ - 101, false).ToString();
            }
            return text;
        }

        /// <summary>
        /// 技艺、功法資質現值
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="typ"></param>
        /// <returns></returns>
        private static int GetSkillGongFaValue(int actorId, int typ) => int.Parse(DateFile.instance.GetActorDate(actorId, typ, true));
        /// <summary>
        /// 技艺、功法資質原值
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="typ"></param>
        /// <returns></returns>
        private static int GetSkillGongFaBase(int actorId, int typ) => int.Parse(DateFile.instance.GetActorDate(actorId, typ, false));
        /// <summary>
        /// 对某一角色的资质用顏色标识的基礎值
        /// </summary>
        /// <param name="typ"></param>
        /// <returns></returns>
        /// <remarks>npc成为某一职业后其资质会获得附加奖励值，这里的出发点是，将任意npc的原始资质加上职业附加资质后评价其优劣，用颜色标识</remarks>
        private static int GetGangFamilySkillBase(int typ)
        {
            int gangGroupId;
            if (Main.settings.colorLevelBaseOn < 4)
            {
                // 门派红字、橙字、黄字、紫字
                gangGroupId = 101 + Main.settings.colorLevelBaseOn;
            }
            else
            {
                if (Main.settings.colorLevelBaseOn == 4)
                    // 城中身份ID以1开头
                    gangGroupId = 10;
                else
                    // 村镇身份ID以3和4开头，不过目前数值一样，无需做进一步细分
                    gangGroupId = 30;

                if (typ < 600)
                    // 技艺资质附加值只考虑可以传授技能的职业, 根据技艺类型不同考虑对应职业，如文人、手艺人、大夫和下九流
                    gangGroupId += gangGroupIndexAdd[typ - 501];
                else
                    // 功法资质都一样，随便挑一个
                    gangGroupId += gangGroupIndexAdd[16];
            }

            return int.Parse(DateFile.instance.presetGangGroupDateValue[gangGroupId][typ < 600 ? 815 : 816]);
        }

        /// <summary>
        /// 最大造詣
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="typ"></param>
        /// <returns></returns>
        private static int GetMaxFamilySkill(int actorId, int typ) => GetSkillGongFaBase(actorId, typ) * (GetGangFamilySkillBase(typ) + 100) / 100;

        /// <summary>
        /// 最大資質
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="typ"></param>
        /// <returns></returns>
        private static int GetMaxSkillLevel(int actorId, int typ) => GetSkillGongFaValue(actorId, typ) / 30 + (GetSkillGongFaValue(actorId, typ) + GetMaxFamilySkill(actorId, typ)) / 90;

        /// <summary>
        /// 根據NPC的門派或職業,判斷能否傳授你這生活藝能
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="skillId"></param>
        /// <returns></returns>
        private static bool CanTeach(int actorID, int skillId)
        {
            int gangValueId = GetGangLevelId(actorID);
            string taughtId = DateFile.instance.presetGangGroupDateValue[gangValueId][818];
            if (taughtId == "0") //不教
                return false;

            string eventID;
            string[] eventIDs;

            if (skillId >= 0 && skillId <= 5)
                eventID = (931900001 + skillId).ToString();
            else if (skillId == 6 || skillId == 7)
                eventID = (932200001 + skillId - 6).ToString();
            else if (skillId == 8 || skillId == 9)
                eventID = (932900001 + skillId - 8).ToString();
            else if (skillId == 10 || skillId == 11)
                eventID = (932200003 + skillId - 10).ToString();
            else if (skillId >= 12 && skillId <= 15)
                eventID = (932300001 + skillId - 12).ToString();
            else
                return false;

            switch (taughtId)
            {
                case "901000006": // 学习复合技艺
                    eventIDs = DateFile.instance.presetGangGroupDateValue[gangValueId][813].Split('|');
                    break;
                case "901300004": // 请教才艺
                case "901300007": // 请教手艺
                case "901300008": // 请教杂艺
                    int messageID = int.Parse(DateFile.instance.eventDate[int.Parse(taughtId)][7]);
                    eventIDs = DateFile.instance.eventDate[messageID][5].Split('|');
                    break;
                default: // 学习單一技艺 or 大夫
                    eventIDs = taughtId.Split('|');
                    break;
            }
            return eventIDs.Contains(eventID);
        }

        /// <summary>
        /// 最佳 裝備 物品 武功
        /// </summary>
        /// <param name="id"></param>
        /// <param name="featureRowCount">人物特性栏有几行</param>
        /// <returns></returns>
        private static string SetInfoMessage4(int id, int featureRowCount = 1)
        {
            string text = GetResource(id, featureRowCount);
            if (Main.settings.showBest && !smallerWindow)
            {
                text += "\n" + GetShopMassage(id) + "\n" + GetEquipments(id) + "\n" + GetBestItems(id);
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

        /// <summary>
        /// 近期事件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string SetInfoMessage5(int id) => Main.settings.lifeMessage && !smallerWindow ? "\n" + GetLifeMessage(id, 3) : "";

        /// <summary>
        /// 心情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string GetMood(int id) => ActorMenu.instance.Color2(DateFile.instance.GetActorDate(id, 4, false));

        /// <summary>
        /// 印象
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        private static string GetLifeFace(int actorId)
        {
            string text = "<color=#606060FF>无</color>";
            if (DateFile.instance.HaveLifeDate(actorId, 1001))
            {
                int faceId = DateFile.instance.GetLifeDate(actorId, 1001, 0);
                int lifeDate = DateFile.instance.GetLifeDate(actorId, 1001, 1);
                text = $"<color=white>{DateFile.instance.identityDate[faceId][0]}</color><color=lime>{lifeDate}%</color>";
            }
            return text;
        }

        /// <summary>
        /// 魅力
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        private static string GetCharm(int actorId)
        {
            int actorCharm = Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(actorId, 15, true)) / 100, 0, 9);
            int actorCharmDiff = int.Parse(DateFile.instance.GetActorDate(actorId, 15, true)) - int.Parse(DateFile.instance.GetActorDate(actorId, 15, false));
            bool isChild = int.Parse(DateFile.instance.GetActorDate(actorId, 11, false)) <= ConstValue.actorMinAge;
            bool hasCloth = int.Parse(DateFile.instance.GetActorDate(actorId, 8, false)) != 1 || int.Parse(DateFile.instance.GetActorDate(actorId, 305, false)) != 0;
            string text = isChild && Main.settings.hideCharmOfChildren ?
                DateFile.instance.massageDate[25][5].Split('|')[0] : (hasCloth ?
                    DateFile.instance.massageDate[25][int.Parse(DateFile.instance.GetActorDate(actorId, 14, false)) - 1].Split('|')[actorCharm] :
                    DateFile.instance.massageDate[25][5].Split('|')[1]
                );
            if (Main.settings.addonInfo && !isChild && actorCharmDiff != 0 && !smallerWindow)
            { // 显示未加成数据 true
                text += " <color=#606060FF>" + (actorCharmDiff > 0 ? "+" : "") + actorCharmDiff + "</color>";
            }

            return text;
        }

        /// <summary>
        /// 名誉文字
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string GetFame(int id) => DateFile.instance.GetActorFameText(id);

        /// <summary>
        /// 名誉值
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static int GetActorFame(int id) => DateFile.instance.GetActorFame(id);

        /// <summary>
        /// 立场
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string GetGoodness(int id) => DateFile.instance.massageDate[9][0].Split('|')[DateFile.instance.GetActorGoodness(id)];

        /// <summary>
        /// 生育能力
        /// </summary>
        /// <param name="id"></param>
        /// <param name="plus"></param>
        /// <returns></returns>
        private static int GetFertility(int id, bool plus)
        {
            int result = int.Parse(DateFile.instance.GetActorDate(id, 24, plus));
            if (!plus) result = int.Parse(DateFile.instance.GetActorDate(id, 24, true)) - result;
            return result;
        }

        /// <summary>
        /// 喜好
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hobby"></param>
        /// <returns></returns>
        private static string Gethobby(int id, int hobby)
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

        /// <summary>
        /// 年龄
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static int GetAge(int id) => int.Parse(DateFile.instance.GetActorDate(id, 11, false));

        /// <summary>
        /// 健康值
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string GetHealth(int id)
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

        /// <summary>
        /// 商会
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string GetShopName(int id)
        {
            string text = "";
            if (GetGangLevelText(id) == "商人" || Main.settings.showShopid)
            {
                int typ = int.Parse(DateFile.instance.GetGangDate(int.Parse(DateFile.instance.GetActorDate(id, 9, false)), 16));
                text = string.Format("{0}", DateFile.instance.storyShopDate[typ][0], DateFile.instance.massageDate[11][2]);
            }

            return text;
        }
        /// <summary>
        /// 商会详细信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <remarks><see cref="MassageWindow.EndEvent9013_1()"/> Case 5</remarks>
        private static string GetShopMassage(int id)
        {
            string text = "";
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
                        moneyCost = 225;
                        break;

                    case 3:
                        moneyCost = 275;
                        break;

                    case 4:
                        moneyCost = 300;
                        break;
                }

                /// 品鉴等级 <seealso cref="ShopSystem.SetShopItems(int, int, int, int, int)"/>
                int addlevel = DateFile.instance.GetActorValue(id, 506, true) * 3;
                //商队
                int typ = int.Parse(DateFile.instance.GetGangDate(int.Parse(DateFile.instance.GetActorDate(id, 9, false)), 16));
                int shopTyp = int.Parse(DateFile.instance.GetGangDate(typ, 16));
                //商品等级Plus
                int newShopLevel = DateFile.instance.storyShopLevel[shopTyp] + addlevel;
                //好感等级
                int actorFavor = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), id, true, false);
                // NPC卖价百分比=30+好感等级*5
                int shopSellCost = 30 + actorFavor * 5;
                // NPC买价百分比=特性基价值-好感等级*15
                int shopSystemCost = moneyCost - actorFavor * 15;
                // 商队好感基数（剑冢相关）
                int storyShopLevel = DateFile.instance.storyShopLevel[shopTyp];

                var bestLevel = Mathf.Clamp((newShopLevel) / 625 + 1, 1, 9);
                var bestText = DateFile.instance.massageDate[8001][2].Split('|')[bestLevel - 1];
                bestText = DateFile.instance.SetColoer(20001 + bestLevel, bestText);
                // 商人详细信息：
                text += "商人信息:  "
                    + $"可售{bestText}" + $"  好感:<color>{storyShopLevel / 50}%+{addlevel / 50}%</color>"
                    + $"  买卖价格:<color>{shopSystemCost}%/{shopSellCost}%</color>"
                    + "\n";
            }
            return text;
        }

        /// <summary>
        /// 所屬地
        /// </summary>
        /// <param name="actorid"></param>
        /// <returns></returns>
        private static string GetActorGang(int actorid) => DateFile.instance.GetGangDate(int.Parse(DateFile.instance.GetActorDate(actorid, 19, false)), 0);

        /// <summary>
        /// 人物在组织中等级ID
        /// </summary>
        /// <param name="actorid"></param>
        /// <returns></returns>
        private static int GetGangLevelId(int actorid)
            => DateFile.instance.GetGangValueId(int.Parse(DateFile.instance.GetActorDate(actorid, 19, false)), int.Parse(DateFile.instance.GetActorDate(actorid, 20, false)));

        /// <summary>
        /// 人物在组织中等级名称
        /// </summary>
        /// <param name="actorid"></param>
        /// <returns></returns>
        private static string GetGangLevelText(int actorid) => DateFile.instance.presetGangGroupDateValue[GetGangLevelId(actorid)][int.Parse(DateFile.instance.GetActorDate(actorid, 20, false)) >= 0 ? 1001 : 1001 + int.Parse(DateFile.instance.GetActorDate(actorid, 14, false))];

        /// <summary>
        /// 地位
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string GetGangLevelColorText(int id) => DateFile.instance.SetColoer(20011 - Mathf.Abs(int.Parse(DateFile.instance.GetActorDate(id, 20, false))), GetGangLevelText(id));

        /// <summary>
        /// 人物身上裝備
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string GetEquipments(int id)
        {
            var actorEquipments = new List<string>(10);
            for (int i = 301; i <= 310; i++)
            {
                int itemID = int.Parse(DateFile.instance.GetActorDate(id, i, false));
                if (itemID != 0)
                {
                    actorEquipments.Add(GetItemColorName(itemID));
                }
            }
            return "人物装备: " + (actorEquipments.Count == 0 ?
                DateFile.instance.SetColoer(20002, GetGenderTA(id) + "赤身裸体在你眼前") :
                string.Join(", ", actorEquipments.ToArray())) + "\n";
        }


        /// <summary>
        /// 获取列表中品级最高功法的名字与数量
        /// </summary>
        /// <param name="gongFas"></param>
        /// <returns></returns>
        private static string GetBestGongfaText(List<int> gongFas)
        {
            var bestName = new StringBuilder();
            int bestLevel = -1;
            foreach (int id in gongFas)
            {
                if (GetGongfaLevel(id) == bestLevel)
                {
                    bestName.Append(", " + GetGongfaColorName(id));
                }
                else if (GetGongfaLevel(id) > bestLevel)
                {
                    bestName.Clear();
                    bestName.Append(GetGongfaColorName(id));
                    bestLevel = GetGongfaLevel(id);
                }
            }
            return bestName.ToString();
        }

        /// <summary>
        /// 获取功法等级
        /// </summary>
        /// <param name="gongFaId"></param>
        /// <returns></returns>
        private static int GetGongfaLevel(int gongFaId) => DateFile.instance.gongFaDate.TryGetValue(gongFaId, out var gongFa) ? int.Parse(gongFa[2]) : 0;

        /// <summary>
        /// 获取功法名称
        /// </summary>
        /// <param name="gongFaId"></param>
        /// <returns></returns>
        private static string GetGongfaName(int gongFaId) => DateFile.instance.gongFaDate.TryGetValue(gongFaId, out var gongFa) ? gongFa[0] : "";

        /// <summary>
        /// 带等级颜色的功法名称
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string GetGongfaColorName(int id) => DateFile.instance.SetColoer(20001 + GetGongfaLevel(id), GetGongfaName(id));

        /// <summary>
        /// 获取角色功法
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        private static List<int> GetGongfaList(int actorId)
        {
            List<int> gongFas = DateFile.instance.actorGongFas.TryGetValue(actorId, out var gongFa) ? new List<int>(gongFa.Keys) : new List<int>(); //避免存取死人資料時引發紅字
            gongFas.RemoveAll(t => t == 0); // 刪除多餘的功法ID=0
            return gongFas;
        }

        /// <summary>
        /// 人物身上最高级功法获取
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string GetBestGongfa(int id) => (DateFile.instance.actorGongFas.ContainsKey(id) ? "最佳功法: " + GetBestGongfaText(GetGongfaList(id)) : DateFile.instance.SetColoer(20002, GetGenderTA(id) + "还没来得及学")) + "\n";

        /// <summary>
        /// 人物身上可被太吾修习的功法获取
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string GetLearnableGongfa(int id)
        {
            var myGongFas = new HashSet<int>(GetGongfaList(DateFile.instance.MianActorID()));
            List<int> taGongFas = GetGongfaList(id);

            //挑出目标人物身上太吾未学会的功法
            var nGongFas = new List<int>();
            var nGongFas2 = new List<int>();
            foreach (int gongFaId in taGongFas)
            {
                if (!myGongFas.Contains(gongFaId))
                {
                    if (DateFile.instance.GetGongFaLevel(id, gongFaId) >= 50)
                    {
                        nGongFas.Add(gongFaId);
                    }
                    else
                    {
                        nGongFas2.Add(gongFaId);
                    }
                }
            }
            string bestName = GetBestGongfaText(nGongFas);
            int bestGongfaCount = (bestName == "") ? 0 : bestName.Split(',').Count();

            bestName = bestName == "" ? DateFile.instance.SetColoer(20002, DateFile.instance.eventDate[9159][3].Replace("我", GetGenderTA(id))) : bestName;
            if (bestGongfaCount < nGongFas.Count) bestName = $"{bestName} 及 {nGongFas.Count - bestGongfaCount} 种低阶功法";
            string result = "可学功法: " + bestName + "\n";
            if (nGongFas2.Count > 0) result += $"\n待熟练功法: {GetBestGongfaText(nGongFas2)}\n";
            return result;
        }

        /// <summary>
        /// 人物身上的最佳物品获取
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string GetBestItems(int id)
        {
            if (!DateFile.instance.actorItemsDate.ContainsKey(id))
                return ""; // 無物品/ 已死之人
            // 使用hashset防止加入相同名称的物品
            var bestItems = new HashSet<string>();
            int bestGrade = 0;

            foreach (int itemID in ActorMenu.instance.GetActorItems(id, 0, false).Keys)
            {
                string itemName = GetItemColorName(itemID);
                int itemGrade = int.Parse(DateFile.instance.GetItemDate(itemID, 8, false));
                if (int.Parse(DateFile.instance.GetItemDate(itemID, 98, false)) == 86)
                    continue; //跳过伏虞剑及其碎片
                if (itemGrade > bestGrade)
                {
                    bestGrade = itemGrade;
                    bestItems.Clear();
                    bestItems.Add(itemName);
                }
                else if (itemGrade == bestGrade)
                {
                    bestItems.Add(itemName);
                }
            }
            return "最佳物品: " + (bestItems.Count == 0 ?
                DateFile.instance.SetColoer(20002, GetGenderTA(id) + "是个穷光蛋") :
                string.Join(", ", bestItems.ToArray())) + " \t" + GetItemWeight(id) + "\n";
        }

        /// <summary>
        /// 村民工作地点
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="buildingIndex"></param>
        /// <returns></returns>
        private static string GetWorkPlace(int partId, int placeId, int buildingIndex)
        {
            string text = "";
            int[] buildingData = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int buildType = buildingData[0];
            int buildLv = buildingData[1];
            text += DateFile.instance.basehomePlaceDate[buildType][0];
            text += " - Lv." + buildLv;
            return text;
        }

        /// <summary>
        /// 村民工作地点現在收获进度
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="buildingIndex"></param>
        /// <returns></returns>
        private static string GetWorkingProgress(int partId, int placeId, int buildingIndex)
        {
            string text = "";
            int[] buildingData = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int buildingType = buildingData[0];
            Dictionary<int, string> buildingSetting = DateFile.instance.basehomePlaceDate[buildingType];

            int currentXp = buildingData[11];
            int BuildingMaxXp = int.Parse(buildingSetting[91]);
            int efficient = HomeSystem.instance.GetBuildingLevelPct(partId, placeId, buildingIndex);
            text += buildingType == 1003 ? "此人正在厢房摸鱼……" : $"{(float)currentXp / BuildingMaxXp * 100:0.#}% (+{(float)efficient * 100 / BuildingMaxXp:0.#}%{DateFile.instance.massageDate[7006][1]})";

            return text;
        }

        /// <summary>
        /// 工作效率
        /// </summary>
        /// <param name="workerId"></param>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="buildingIndex"></param>
        /// <returns></returns>
        /// <remarks><see cref="HomeSystem.GetBuildingLevelPct(int partId, int placeId, int buildingIndex)"/></remarks>
        private static string GetExpectEfficient(int workerId, int partId, int placeId, int buildingIndex)
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

            int workerSkill = (skillId > 0) ? int.Parse(DateFile.instance.GetActorDate(workerId, skillId, true)) : 0;
            if (skillId == 18) workerSkill += 100;  //镖局,知客亭,炼神峰: 以名譽+100計算

            int NeighborBonus = 0;
            foreach (int key in HomeSystem.instance.GetBuildingNeighbor(partId, placeId, buildingIndex, 1))
            {
                if (DateFile.instance.homeBuildingsDate[partId][placeId].ContainsKey(key) && DateFile.instance.actorsWorkingDate[partId][placeId].ContainsKey(key) && int.Parse(DateFile.instance.basehomePlaceDate[DateFile.instance.homeBuildingsDate[partId][placeId][key][0]][62]) != 0)
                {
                    int skillBonus = (skillId > 0) ? int.Parse(DateFile.instance.GetActorDate(DateFile.instance.actorsWorkingDate[partId][placeId][key], skillId, true)) : 0;
                    if (skillId == 18) skillBonus += 100;  //镖局,知客亭,炼神峰 : 以名譽+100計算
                    NeighborBonus += skillBonus;
                }
            }
            int skillRequirement = Mathf.Max(int.Parse(buildingSetting[51]) + buildingLv - 1, 1);
            workerSkill = (workerSkill + NeighborBonus) * Mathf.Max(moodBonus, 0);

            int efficiency = Mathf.Clamp(workerSkill / skillRequirement, 50, 200);
            return $"+{(float)efficiency / BuildingMaxXp * 100:0.#}%{DateFile.instance.massageDate[7006][1]}";
        }

        /// <summary>
        /// 转世次数
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string GetSamsara(int id)
        {
            if (DateFile.instance.HaveLifeDate(id, 801))
            {
                int count = DateFile.instance.GetLifeDateList(id, 801).Count;
                int coloer = (count < 1) ? 20002 : 20003;
                return DateFile.instance.SetColoer(coloer, count.ToString(), false);
            }

            return "0";
        }

        /// <summary>
        /// 前世名字
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string GetSamsaraName(int id)
        {
            string text = "";
            if (DateFile.instance.HaveLifeDate(id, 801))
            {
                List<int> list = DateFile.instance.GetLifeDateList(id, 801);
                int samsaraId = list[list.Count - 1];
                int levelId = GetGangLevelId(samsaraId);
                text = $"{DateFile.instance.GetActorName(samsaraId, true, false)}";
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

        /// <summary>
        /// 婚姻 / 性取向
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string GetSpouse(int id)
        {
            List<int> actorSocial = DateFile.instance.GetActorSocial(id, 309, false, true);
            List<int> actorSocial2 = DateFile.instance.GetActorSocial(id, 309, true, true);
            string result = actorSocial2.Count == 0 ? DateFile.instance.SetColoer(20004, "未婚", false) : (actorSocial.Count == 0 ? DateFile.instance.SetColoer(20007, "丧偶", false) : DateFile.instance.SetColoer(20010, "已婚", false));
            if (Main.settings.showSexuality && !isDead) result += " • " + GetSexuality(id);
            return result;
        }

        /// <summary>
        /// 人物经历
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="shownum"></param>
        /// <returns></returns>
        /// <remarks>参见<see cref="ActorMenu.SetMassageText(int, int[])"/></remarks>
        private static string GetLifeMessage(int actorId, int shownum) //shownum控制显示几条信息
        {
            string text = "";
            int index;
            int count;
            if (actorId != lastActorID)
            {
                actorMassageCache.Clear();
                actorMassageCache.Add(string.Format("{0}{1}{2}{3}{4}", new object[]
                {
                    DateFile.instance.massageDate[8010][1].Split('|')[0],
                    // 出生时节
                    DateFile.instance.SetColoer(10002, DateFile.instance.solarTermsDate[int.Parse(DateFile.instance.GetActorDate(actorId, 25, false))][102], false),
                    DateFile.instance.massageDate[8010][1].Split('|')[1],
                    // 取名
                    DateFile.instance.GetActorName(actorId, false, true),
                    DateFile.instance.massageDate[8010][1].Split('|')[2]
                }));
                if (DateFile.instance.actorLifeMassage.TryGetValue(actorId, out var actorMsg))
                {
                    count = actorMsg.Count;
                    index = count >= shownum ? count - shownum : 0;
                    for (int i = index; i < count; i++)
                    {
                        int[] msgData = actorMsg[i];
                        int msgId = msgData[0];
                        string[] array2 = DateFile.instance.actorMassageDate[msgId][2].Split('|');
                        string[] array3 = DateFile.instance.actorMassageDate[msgId][99].Split('|');
                        var list = new List<string>
                        {
                            DateFile.instance.massageDate[16][1] + DateFile.instance.SetColoer(10002, msgData[1].ToString(), false) + DateFile.instance.massageDate[16][3],
                            DateFile.instance.SetColoer(20002, DateFile.instance.solarTermsDate[msgData[2]][0], false)
                        };

                        list.Add(DateFile.instance.SetColoer(10004, DateFile.instance.GetNewMapDate(msgData[3], msgData[4], 98) + DateFile.instance.GetNewMapDate(msgData[3], msgData[4], 0), false));
                        for (int j = 0; j < array3.Length; j++)
                        {
                            list.Add(array3[j]);
                        }

                        for (int k = 5; k < msgData.Length; k++)
                        {
                            int num = msgData[k];
                            switch (int.Parse(array2[k - 5]))
                            {
                                case 0:
                                    list.Add(DateFile.instance.SetColoer((int.Parse(DateFile.instance.GetActorDate(num, 26, false)) <= 0) ? 10002 : 20010, DateFile.instance.GetActorName(num, false, false), false));
                                    break;
                                case 1:
                                    list.Add(DateFile.instance.massageDate[10][0].Split('|')[0] + DateFile.instance.SetColoer(20001 + int.Parse(DateFile.instance.GetItemDate(num, 8, true)), DateFile.instance.GetItemDate(num, 0, false), false) + DateFile.instance.massageDate[10][0].Split('|')[1]);
                                    break;
                                case 2:
                                    list.Add(DateFile.instance.SetColoer(20001 + int.Parse(DateFile.instance.gongFaDate[num][2]), DateFile.instance.massageDate[10][0].Split('|')[0] + DateFile.instance.gongFaDate[num][0] + DateFile.instance.massageDate[10][0].Split('|')[1], false));
                                    break;
                                case 3:
                                    list.Add(DateFile.instance.SetColoer(20008, DateFile.instance.resourceDate[num][0], false));
                                    break;
                                case 4:
                                    list.Add(DateFile.instance.SetColoer(20008, DateFile.instance.GetGangDate(num, 0), false));
                                    break;
                                case 5:
                                    list.Add(DateFile.instance.SetColoer(20011 - Mathf.Abs(int.Parse(DateFile.instance.GetActorDate(actorId, 20, false))), DateFile.instance.presetGangGroupDateValue[Mathf.Abs(num)][(num <= 0) ? (1001 + int.Parse(DateFile.instance.GetActorDate(actorId, 14, false))) : 1001], false));
                                    break;
                            }
                        }
                        actorMassageCache.Add(string.Format("{0}{1}:\u00A0" + DateFile.instance.actorMassageDate[msgId][1], list.ToArray()));
                    }
                }

                int isDead = int.Parse(DateFile.instance.GetActorDate(actorId, 26, false));
                if (isDead > 0)
                {
                    actorMassageCache.Add(string.Format("■\u00A0{0}{1}{2}",
                        DateFile.instance.massageDate[8010][2].Split('|')[0],
                        DateFile.instance.SetColoer(10002, DateFile.instance.GetActorDate(actorId, 11, false), false),
                        DateFile.instance.massageDate[8010][2].Split('|')[1]
                        ));
                }
                lastActorID = actorId;
            }

            count = actorMassageCache.Count;
            index = count >= shownum ? count - shownum : 0;
            for (int i = index; i < count; i++)
            {
                text += actorMassageCache[i].Trim('\n') + "\n";
            }
            return text;
        }

        /// <summary>
        /// 人物賦性
        /// </summary>
        /// <param name="id"></param>
        /// <param name="featureRowCount">人物特性栏有几行</param>
        /// <returns></returns>
        private static string GetResource(int id, int featureRowCount)
        {
            if (!Main.settings.showResources)
                return "";

            int[] actorResources = ActorMenu.instance.GetActorResources(id);  //401~407
            if (DateFile.instance.deadActors.Contains(id) || actorResources.Sum() == 0 || smallerWindow)
            {
                ClearResourceHolder();
                return "";
            }

            Text[] resourcesText = GetResourceHolder(featureRowCount).gameObject.GetComponentsInChildren<Text>();

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

        /// <summary>
        /// 負重資料
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string GetItemWeight(int key)
        {
            string text = "";
            int maxItemSize = ActorMenu.instance.GetMaxItemSize(key);
            int useItemSize = ActorMenu.instance.GetUseItemSize(key);
            text += string.Format("{3}{0}{1} / {2}</color>", new object[] {
                ActorMenu.instance.Color8(useItemSize, maxItemSize),
                (useItemSize / 100f).ToString("f1"),
                (maxItemSize / 100f).ToString("f1"),
                DateFile.instance.massageDate[807][2].Split('|')[0]
            });
            return text;
        }

        /// <summary>
        /// 获取性取向文字
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string GetSexuality(int id) => DateFile.instance.GetActorDate(id, 21, false) == "0" ? "直" : "双";

        /// <summary>
        /// 获取性别文字
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string GetGenderText(int id)
        {
            int gender = Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(id, 14, false)), 0, 2);
            if (DateFile.instance.GetActorDate(id, 17, false) == "1" && gender > 0)
            {
                // 男生女相/女生男相
                return DateFile.instance.massageDate[7003][0].Split('|')[gender - 1];
            }
            else
                return DateFile.instance.massageDate[7][0].Split('|')[gender];
        }

        /// <summary>
        /// 根据性别确定第三人称称谓
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string GetGenderTA(int id) => DateFile.instance.GetActorDate(id, 14, false) == "2" ? "她" : "他";

        /// <summary>
        /// 只保留物品名称第一行括号前面的内容
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string PurifyItemName(string name) => Regex.Match(name, @"^(?>[^\(\r\n]+)").Value;

        /// <summary>
        /// 获得带品级颜色的物品名称
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        private static string GetItemColorName(int itemID)
        {
            string itemName = PurifyItemName(DateFile.instance.GetItemDate(itemID, 0, false));
            int itemGrade = int.Parse(DateFile.instance.GetItemDate(itemID, 8, false));
            return DateFile.instance.SetColoer(20001 + itemGrade, itemName);
        }

        /// <summary>
        /// 角色特性显示栏
        /// </summary>
        /// <returns></returns>
        private static Transform GetActorFeatureHolder()
        {
            Transform actorFeatureHolder = WindowManage.instance.informationMassage.transform.Find("ActorFeatureHolder");
            if (actorFeatureHolder == null)
            {
                actorFeatureHolder = UnityEngine.Object.Instantiate(ActorMenu.instance.actorFeatureHolder, new Vector3(-20f, -60f, 1), Quaternion.identity);
                actorFeatureHolder.name = "ActorFeatureHolder";
                actorFeatureHolder.localScale = new Vector3(.73f, .73f, 1);
                actorFeatureHolder.GetComponent<RectTransform>().sizeDelta = new Vector2(500f, 0);
                actorFeatureHolder.SetParent(WindowManage.instance.informationMassage.GetComponent<RectTransform>(), false);
            }
            return actorFeatureHolder;
        }

        /// <summary>
        /// 清除角色特性显示
        /// </summary>
        private static void ClearActorFeatureHolder()
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

        /// <summary>
        /// 获取角色七元赋性显示框
        /// </summary>
        /// <param name="featureRowCount">人物特性栏有几行</param>
        /// <returns></returns>
        private static Transform GetResourceHolder(int featureRowCount = 1)
        {
            featureRowCount = Math.Max(featureRowCount, 1);
            float y = 65; //主要信息
            if (Main.settings.showCharacteristic)
                y += 55 * featureRowCount; // 人物特性
            if (Main.settings.showLevel)
                y += 195; // 人物屬性
            Transform resourceHolder = WindowManage.instance.informationMassage.transform.Find("ResourceHolder");
            if (resourceHolder == null)
            {
                resourceHolder = UnityEngine.Object.Instantiate(ActorMenu.instance.teamResourcesText[0].transform.parent, new Vector3(0, -y, 1), Quaternion.identity);
                resourceHolder.name = "ResourceHolder";
                resourceHolder.SetParent(WindowManage.instance.informationMassage.GetComponent<RectTransform>(), false);
            }
            else
                resourceHolder.localPosition = new Vector3(resourceHolder.localPosition.x, -y, resourceHolder.localPosition.z);

            return resourceHolder;
        }

        /// <summary>
        /// 清除角色七元赋性显示框
        /// </summary>
        private static void ClearResourceHolder() => UnityEngine.Object.Destroy(GetResourceHolder().gameObject);

        /// <summary>
        /// 浮动显示框中的好感和戒心
        /// </summary>
        /// <returns></returns>
        private static Transform GetFavourHolder()
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

        /// <summary>
        /// 清除浮动显示框中的好感和戒心
        /// </summary>
        private static void ClearFavourHolder() => UnityEngine.Object.Destroy(GetFavourHolder().gameObject);

        /// <summary>
        /// 添加文字
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="initText"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color"></param>
        /// <param name="fontsize"></param>
        /// <param name="align"></param>
        private static void AddText(string name, Transform parent, string initText = "", float x = 0, float y = 0, float width = 200f, float height = 200f, Color? color = null, int fontsize = 15, TextAnchor align = TextAnchor.MiddleLeft)
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

    /// <summary>
    /// 修复人物关系中浮动信息显示不正常
    /// </summary>
    [HarmonyPatch(typeof(WindowManage), "ShowTips")]
    internal static class WindowManage_ShowTips_Patch
    {
        public static void Postfix(WindowManage __instance, ref Vector3 ___tipsPoint, int ___tipsW, int ___tipsH)
        {
            if (!Main.enabled)
                return;

            if (__instance.tipsDate.CompareTag("PeopleActor"))
            {
                RectTransform component = __instance.informationWindow.GetComponent<RectTransform>();
                //component.sizeDelta = new Vector2(___tipsW, ___tipsH + __instance.informationMassage.rectTransform.sizeDelta.y + __instance.informationName.rectTransform.sizeDelta.y);
                ___tipsPoint += new Vector3(component.sizeDelta.x, -component.sizeDelta.y, 0f);
                component.anchoredPosition = ___tipsPoint;
            }
        }
    }
}
