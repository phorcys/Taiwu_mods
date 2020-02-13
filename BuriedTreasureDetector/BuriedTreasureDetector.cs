using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;

namespace BuriedTreasureDetector
{
    public class Settings : ModSettings
    {
        public override void Save(ModEntry modEntry)
        {
            ModSettings.Save<Settings>(this, modEntry);
        }

        /// <summary>
        /// 掉率精确到小数点后几位
        /// </summary>
        public int decimalPlace = 4;
        /// <summary>
        /// 是否显示掉率
        /// </summary>
        public bool isShowProbabilityInMap = false;
        /// <summary>
        /// 是否开启工人自动采集
        /// </summary>
        public bool workerCanGetTreasure = false;
        /// <summary>
        /// 在地图中显示寻宝奇遇
        /// </summary>
        public bool isShowStoryInMap = false;
        /// <summary>
        /// 在过月天灾中显示奇遇
        /// </summary>
        public bool isShowStory = false;
        /// <summary>
        /// 在地块中显示锄地掉率
        /// </summary>
        public bool isShowProbability = false;
    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static ModEntry.ModLogger Logger;

        public static bool Load(ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = ModSettings.Load<Settings>(modEntry);
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            return true;
        }

        static void OnGUI(ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            settings.isShowStory = GUILayout.Toggle(settings.isShowStory, "在过月天灾中显示奇遇", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            settings.isShowProbability = GUILayout.Toggle(settings.isShowProbability, "在地块中显示锄地掉率及坐标", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            settings.isShowStoryInMap = GUILayout.Toggle(settings.isShowStoryInMap, "在地图中显示寻宝奇遇", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            settings.isShowProbabilityInMap = GUILayout.Toggle(settings.isShowProbabilityInMap, "在地图中显示锄地掉率", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("掉率显示小数点后位数：", GUILayout.Width(150));
            string strDecimalPlace = GUILayout.TextArea(Main.settings.decimalPlace.ToString(), GUILayout.Width(60));
            if (int.TryParse(strDecimalPlace, out int number))
            {
                Main.settings.decimalPlace = number;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            settings.workerCanGetTreasure = GUILayout.Toggle(settings.workerCanGetTreasure, "开启过月自动采集", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
        }

        public static bool OnToggle(ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        static void OnSaveGUI(ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }

    /// <summary>
    /// Patch: 设置浮窗文字
    /// </summary>
    [HarmonyPatch(typeof(WindowManage), "WindowSwitch")]
    public static class WindowManage_WindowSwitch_Patch
    {
        public static void Postfix(bool on, GameObject tips, ref int ___tipsW)
        {
            if (!Main.enabled || ActorMenu.instance == null || tips == null || !on) return;

            string[] array;
            int worldId;
            int partId;
            int placeId;
            int mapWidth;

            switch (tips.tag)
            {
                case "MianPlace":
                    //地块
                    //___informationMassage.text == "\n";
                    if (Main.settings.isShowProbability)
                    {
                        array = tips.transform.parent.name.Split(',');
                        partId = array.Length > 2 ? int.Parse(array[2]) : -1;
                        placeId = array.Length > 3 ? int.Parse(array[3]) : -1;
                        mapWidth = partId != -1 ? int.Parse(DateFile.instance.partWorldMapDate[partId][98]) : 0;

                        //WindowManage.instance.informationName.text += String.Format("({0},{1})", Mathf.FloorToInt(placeId % mapWidth), Mathf.FloorToInt(placeId / mapWidth));
                        WindowManage.instance.informationMassage.text += String.Format("位置：({0},{1})", Mathf.FloorToInt(placeId % mapWidth), Mathf.FloorToInt(placeId / mapWidth));
                        WindowManage.instance.informationMassage.text += GetTreasureMessageByPlace(partId, placeId, mapWidth);

                    }
                    break;
                case "PartWorldButton":
                    //地区
                    if (Main.settings.isShowProbabilityInMap || Main.settings.isShowStoryInMap)
                    {
                        array = tips.name.Split(',');
                        partId = array.Length > 2 ? int.Parse(array[2]) : -1;
                        placeId = array.Length > 3 ? int.Parse(array[3]) : -1;
                        mapWidth = partId != -1 ? int.Parse(DateFile.instance.partWorldMapDate[partId][98]) : 0;
                        if (Main.settings.isShowProbabilityInMap)
                        {
                            WindowManage.instance.informationMassage.text += GetTreasureMessageByPlace(partId, 0, mapWidth);

                        }
                        if (Main.settings.isShowStoryInMap)
                        {
                            WindowManage.instance.informationMassage.text += GetTreasureMessageByBuried(partId, mapWidth);
                        }
                    }
                    break;
                case "TrunEventIcon":
                    //天灾
                    if (Main.settings.isShowStory)
                    {
                        array = tips.name.Split(',');
                        if (((array.Length > 1) ? DateFile.instance.ParseIntWithDefaultValue(array[1], 0) : 0) != 256)
                        {
                            break;
                        }
                        if (array[4] == "0")
                        {
                            break;
                        }
                        partId = array.Length > 2 ? int.Parse(array[2]) : -1;
                        mapWidth = partId != -1 ? int.Parse(DateFile.instance.partWorldMapDate[partId][98]) : 0;
                        WindowManage.instance.informationMassage.text += GetTreasureMessageByBuried(partId, mapWidth, false);
                    }
                    break;
            }
        }

        private static string GetTreasureMessageByBuried(int partId, int mapWidth, bool isInMap = true)
        {

            if (FindMe.aowu == null || !FindMe.aowu.ContainsKey(partId))
            {
                FindMe.aowu = new Dictionary<int, Dictionary<int, List<int>>>();
                Dictionary<int, List<int>> dictionary = FindMe.Getplace(partId, mapWidth * mapWidth);
                if (dictionary.Keys.Count > 0)
                {
                    FindMe.aowu.Add(partId, dictionary);
                }
            }

            string text = "";
            if (!FindMe.aowu.ContainsKey(partId))
            {
                text += "此地似乎并无宝藏出现……\n";
            }
            else
            {
                text += "传闻：\n在";
                int num2 = FindMe.aowu[partId].Count;
                foreach (int placeId in FindMe.aowu[partId].Keys)
                {
                    num2--;

                    text += isInMap ? FindMe.Getplacename(partId, placeId, mapWidth) : FindMe.Getplacename(partId, placeId);

                    text += "出现了宝物";
                    text += FindMe.Getitemename(FindMe.aowu[partId][placeId][0], FindMe.aowu[partId][placeId][1]);
                    if (num2 == 0)
                    {
                        text += "。\n";
                    }
                    else
                    {
                        text += "，\n在";
                    }
                }
            }
            return text;
        }

        private static string GetTreasureMessageByPlace(int partId, int placeId, int mapWidth)
        {
            List<string> informationMassageText = new List<string>();
            informationMassageText.Add("\n");

            Dictionary<string, PartItem> dic = placeId.Equals(0) ? FindAll.Instance.GetPlace(partId) : FindAll.Instance.GetPlace(partId, placeId);

            foreach (var item in dic)
            {
                if (item.Value.maxProbability.CompareTo(0) == 0) continue;

                int addColoer = Decimal.ToInt32(Decimal.Floor(item.Value.maxProbability * 900));
                addColoer = Math.Max(addColoer, 0);
                addColoer = Math.Min(addColoer, 8);

                string probability = item.Value.maxProbability.ToString("P" + Main.settings.decimalPlace.ToString());

                informationMassageText.Add(DateFile.instance.SetColoer(20001 + item.Value.maxLevel, item.Key + "："));
                informationMassageText.Add(DateFile.instance.SetColoer(20002 + addColoer, probability));

                if (!placeId.Equals(0)) continue;

                List<string> placeList = new List<string>();
                bool isWordMax = false;

                foreach (var p in item.Value.placeIdList)
                {
                    placeList.Add(string.Format("({0},{1})", Mathf.FloorToInt(p % mapWidth), Mathf.FloorToInt(p / mapWidth)));

                    if (!isWordMax && FindAll.Instance.DictionaryPrace[item.Key].maxProbability.CompareTo(item.Value.maxProbability) == 0)
                    {
                        isWordMax = true;
                    }
                }

                if (isWordMax)
                {
                    informationMassageText.Add(DateFile.instance.SetColoer(20010, " * "));
                }
                else
                {
                    informationMassageText.Add("    ");
                }

                informationMassageText.Add("<color=#add8e6ff>位置： " + String.Join(" ", placeList.ToArray()) + "</color>\n");
            }

            informationMassageText.Add("\n");

            return string.Concat(informationMassageText);
        }

    }

    /// <summary>
    /// Patch: 展示过月事件
    /// </summary>
    [HarmonyPatch(typeof(UIDate), "SaveTurnChangeEvent")]
    public static class UIDate_SaveTurnChangeEvent_OnChangeTurn
    {
        public static void Prefix()
        {
            if (!Main.enabled) return;
            FindAll.Instance.Clear();
            FindMe.aowu.Clear();
            return;
        }

        public static void Postfix()
        {
            if (!Main.enabled || !Main.settings.workerCanGetTreasure) return;
            Work.Instance.ChooseTimeWork();
            return;
        }
    }


    /// <summary>
    /// Patch: 推恩释义
    /// </summary>
    [HarmonyPatch(typeof(MessageEventManager), "EndEvent9013_1")]
    public static class MessageEventManager_EndEvent9013_1_patch
    {
        private static void Prefix()
        {
            if (!Main.enabled || MessageEventManager.Instance.EventValue == null || MessageEventManager.Instance.EventValue.Count < 2) return;

            switch (MessageEventManager.Instance.EventValue[1])
            {
                case 1:
                    FindAll.Instance.Clear();
                    break;
            }
        }
    }
}
