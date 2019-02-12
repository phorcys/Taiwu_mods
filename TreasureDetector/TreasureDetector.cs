using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;

namespace TreasureDetector
{
    public class Settings : ModSettings
    {
        public override void Save(ModEntry modEntry)
        {
            ModSettings.Save<Settings>(this, modEntry);
        }
        public bool calByDropRate = false;
        public bool riseDropRate = false;
        public bool workerCanGetTreasure = false;
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
            GUILayout.Label("插件功能", UI.h2);
            GUILayout.Label("* 大地图各处添加了座标", new GUIStyle(UI.bold) { fontSize = 16 });
            GUILayout.Label("* 鼠标移到地形,可获得挖掘资讯,包括掉宝等级及机率", new GUIStyle(UI.bold) { fontSize = 16 });
            GUILayout.Label("* 鼠标移到城市,可显示当地最高级物资的座标及最大数值", new GUIStyle(UI.bold) { fontSize = 16 });
            GUILayout.Label("选项设定", UI.h2);
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("* 计算最大资源基于", new GUIStyle(UI.bold) { fontSize = 16 }, GUILayout.Width(150));
            settings.calByDropRate = GUILayout.SelectionGrid(settings.calByDropRate ? 1 : 0, new string[] { "<b>现有资源</b>", "<b>掉极品率</b>" }, 2, GUILayout.Width(150)) == 1;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Label("附加功能 (或会破坏游戏体验, 慎用)", UI.h2);
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("* 提高掉极品机率", new GUIStyle(UI.bold) { fontSize = 16 }, GUILayout.Width(150));
            settings.riseDropRate = GUILayout.SelectionGrid(settings.riseDropRate ? 1 : 0, new string[] { "关闭", "启用" }, 2, GUILayout.Width(150)) == 1;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            //以下內容暫未開發
            //            GUILayout.BeginHorizo​​ntal("Box");
            //            GUILayout.Label("进注工人,每月也有机率挖得宝物 (暂未开发)", new GUIStyle(UI.bold) { fontSize = 16 }, GUILayout.Width(150));
            //            settings.workerCanGetTreasure = GUILayout.SelectionGrid(settings.workerCanGetTreasure ? 1 : 0, new string[] { "关闭", "启用" }, 2, GUILayout.Width(150)) == 1;
            //            GUILayout.FlexibleSpace();
            //            GUILayout.EndHorizo​​ntal();
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

    [HarmonyPatch(typeof(WorldMapSystem), "GetTimeWorkItem")]
    public static class WorldMapSystem_GetTimeWorkItem_Patch
    {
        public static void Prefix(int actorId, int partId, int placeId, int workTyp, int showGetTyp, bool getItem = true, bool actorWork = false)
        {
            if (!Main.settings.riseDropRate) return;
            int key = int.Parse(DateFile.instance.GetNewMapDate(partId, placeId, 13));
            DateFile.instance.timeWorkBootyDate[key][workTyp + 1001]=(int.Parse(DateFile.instance.timeWorkBootyDate[key][workTyp + 1001]) + 50).ToString();
        }
        public static void Postfix(int actorId, int partId, int placeId, int workTyp, int showGetTyp, bool getItem = true, bool actorWork = false)
        {
            if (!Main.settings.riseDropRate) return;
            int key = int.Parse(DateFile.instance.GetNewMapDate(partId, placeId, 13));
            DateFile.instance.timeWorkBootyDate[key][workTyp + 1001]= (int.Parse(DateFile.instance.timeWorkBootyDate[key][workTyp + 1001]) - 50).ToString();
        }
    }

    [HarmonyPatch(typeof(WindowManage), "WindowSwitch")]
    public static class WindowManage_WindowSwitch_Patch
    {
        [HarmonyAfter(new string[] { "CharacterFloatInfo" })]
        public static void Postfix(bool on, GameObject tips, ref Text ___itemMoneyText, ref Text ___informationMassage, ref int ___tipsW)
        {
            string[] strings;
            int worldId, partId, placeId;
            if (!Main.enabled || ActorMenu.instance == null || tips == null) return;
            switch (tips.tag)
            {
                case "MianPlace":
                    strings = tips.transform.parent.name.Split(',');
                    worldId = int.Parse(strings[1]);
                    partId = int.Parse(strings[2]);
                    placeId = int.Parse(strings[3]);
                    int width = int.Parse(DateFile.instance.partWorldMapDate[partId][98]);
                    for (int workTyp = 0; workTyp < 5; workTyp++)
                    {
                        GetWorkItemDetail(worldId, partId, placeId, workTyp, out int 现有资源, out int 最大资源, out int 最高品阶, out int 最低品阶, out float 掉宝率, out float 掉极品率, out string 全部可得物品);
                        if (最高品阶 >= 最低品阶 && 最大资源 >= 100)
                        {
                            ___informationMassage.text += String.Format("<color=#FFFF8FFF>{0}</color> {1}/<color=#FFFFFFFF>{2}</color>\t掉宝率{3}\t掉极品率:{4}\n\n可获物品: {5} - {6}\n\n<color=#add8e6ff>包括:</color>{7}\n",
                            DateFile.instance.resourceDate[workTyp][1],
                            DateFile.instance.SetColoer(20002 + Mathf.Clamp(Mathf.FloorToInt(9f * 现有资源 / 最大资源), 0, 8), 现有资源.ToString()),
                            DateFile.instance.SetColoer(20002 + Mathf.Clamp(Mathf.FloorToInt(最大资源 / 10 - 10), 0, 8), 最大资源.ToString()),
                            DateFile.instance.SetColoer(20002 + Mathf.Clamp(Mathf.FloorToInt(掉宝率 / 9), 0, 8), String.Format("{0:0.##}%", 掉宝率)),
                            DateFile.instance.SetColoer(20002 + Mathf.Clamp(Mathf.FloorToInt(掉极品率 * 9), 0, 8), String.Format("{0:0.####}%", 掉极品率)),
                            DateFile.instance.SetColoer(20001 + 最低品阶, DateFile.instance.massageDate[8][3].Split('|')[最低品阶]),
                            DateFile.instance.SetColoer(20001 + 最高品阶, DateFile.instance.massageDate[8][3].Split('|')[最高品阶]),
                            全部可得物品);
                            if (___tipsW < 580) ___tipsW = 580;
                        }
                    }
                    if (___tipsW < 360) ___tipsW = 360;
                    ___itemMoneyText.text = String.Format("({0},{1})", Mathf.FloorToInt(placeId % width), Mathf.FloorToInt(placeId / width));
                    ___informationMassage.text += "\n";
                    break;

                case "PartWorldButton":
                case "MiniPartWorldButton":
                    strings = tips.name.Split(',');
                    worldId = int.Parse(strings[1]);
                    partId = int.Parse(strings[2]);
                    int num = int.Parse(DateFile.instance.partWorldMapDate[partId][98]);
                    int num2 = num * num;
                    for (int workTyp = 0; workTyp < 5; workTyp++)
                    {
                        float 最大值=0;
                        int 最高品阶 = 0;
                        int 最低品阶 = 10;
                        List<int> 位置 = new List<int> { };
                        for (placeId = 0; placeId < num2; placeId++)
                        {
                            GetWorkItemDetail(worldId, partId, placeId, workTyp, out int 现有资源, out int 最大资源, out int 高品阶, out int 低品阶, out float 掉宝率, out float 掉极品率, out string 全部可得物品);
                            if (Main.settings.calByDropRate)
                            {
                                if (掉极品率 > 最大值)
                                {
                                    最大值 = 掉极品率;
                                    最低品阶 = 低品阶;
                                    最高品阶 = 高品阶;
                                    位置 = new List<int> { placeId };
                                }
                                else if (掉极品率 > 0 && 掉极品率 == 最大值)
                                {
                                    位置.Add(placeId);
                                }
                            }
                            else
                            {
                                if (现有资源 > 最大值)
                                {
                                    最大值 = 现有资源;
                                    最低品阶 = 低品阶;
                                    最高品阶 = 高品阶;
                                    位置 = new List<int> { placeId };
                                }
                                else if (现有资源 > 0 && 现有资源 == 最大值)
                                {
                                    位置.Add(placeId);
                                }
                            }
                        }
                        ___informationMassage.text += String.Format("<color=#FFFF8FFF>{0}</color> {1}: {2}\t\t可获: {3} - {4}\n",
                        DateFile.instance.resourceDate[workTyp][1],
                        Main.settings.calByDropRate? "掉极品率(最高)" : "现有资源(最大)",
                        Main.settings.calByDropRate ?
                            DateFile.instance.SetColoer(20002 + Mathf.Clamp(Mathf.FloorToInt(最大值 * 9), 0, 8), String.Format("{0:0.####}%", 最大值)):
                            DateFile.instance.SetColoer(20002 + Mathf.Clamp(Mathf.FloorToInt(最大值 / 10 - 10), 0, 8), 最大值.ToString()),
                        DateFile.instance.SetColoer(20001 + 最低品阶, DateFile.instance.massageDate[8][3].Split('|')[最低品阶]),
                        DateFile.instance.SetColoer(20001 + 最高品阶, DateFile.instance.massageDate[8][3].Split('|')[最高品阶])
                        );
                        ___informationMassage.text += "<color=#add8e6ff>位置: " + String.Join(", ", 位置.Select(p => String.Format("({0},{1})",Mathf.FloorToInt(p % num), Mathf.FloorToInt(p / num))).ToArray())+"</color>\n";
                    }
                    if (___tipsW < 520) ___tipsW = 520;
                    ___informationMassage.text += "\n";
                    break;
            }
        }

        private static void GetWorkItemDetail(int worldId, int partId, int placeId, int workTyp, out int 现有资源, out int 最大资源, out int 最高品阶, out int 最低品阶, out float 掉宝率, out float 掉极品率, out string 全部可得物品)
        {
            现有资源 = Mathf.Max(DateFile.instance.GetPlaceResource(partId, placeId)[workTyp],0);
            最大资源 = Mathf.Max(int.Parse(DateFile.instance.GetNewMapDate(partId, placeId, workTyp + 1)), 1);
            int 库存率 = 现有资源 * 100 / 最大资源;

            最高品阶 = 0;
            最低品阶 = 10;
            int key = int.Parse(DateFile.instance.GetNewMapDate(partId, placeId, 13));
            int[] 可得物品IDs = DateFile.instance.timeWorkBootyDate[key][workTyp + 1].Split('|').Select(int.Parse).ToArray();
            int 提升品阶 = int.Parse(DateFile.instance.timeWorkBootyDate[key][workTyp + 101]);

            List<string> 可得物品 = new List<string> { };
            for (int i = 0; i < 可得物品IDs.Length; i++)
            {
                for (int lv = 0; lv <= 提升品阶; lv++)
                {
                    if (DateFile.instance.presetitemDate.ContainsKey(可得物品IDs[i] + lv))
                    {
                        可得物品.Add(DateFile.instance.SetColoer(20001 + int.Parse(DateFile.instance.presetitemDate[可得物品IDs[i] + lv][8]), DateFile.instance.presetitemDate[可得物品IDs[i] + lv][0]));
                        最高品阶 = Math.Max(最高品阶, int.Parse(DateFile.instance.presetitemDate[可得物品IDs[i] + lv][8]));
                        最低品阶 = Math.Min(最低品阶, int.Parse(DateFile.instance.presetitemDate[可得物品IDs[i] + lv][8]));
                    }
                }
            }
            全部可得物品 = String.Join(", ", 可得物品.ToArray());

            掉宝率 = 可得物品IDs.Length == 1 && 可得物品IDs[0] == 0 ? 0 : 库存率 - 25;

            int 人力 = 1;
            List<int> worldMapNeighbor = DateFile.instance.GetWorldMapNeighbor(partId, placeId, 0);
            for (int i = 0; i < worldMapNeighbor.Count; i++)
            { // 这里可能是游戏错误, 凡是邻格是相同名稱都加人力, 并非邻格有工人在开采相同物资才加人力, 日后游戏改版或会修正
                if (int.Parse(DateFile.instance.GetNewMapDate(partId, worldMapNeighbor[i], 83)) == int.Parse(DateFile.instance.GetNewMapDate(partId, placeId, 83))) 人力 += 2;
            }
            int 地区助力 = int.Parse(DateFile.instance.timeWorkBootyDate[key][workTyp + 1001]) + (Main.settings.riseDropRate?43:0);
            float 極品門檻 = 0.01f * (地区助力 + Mathf.Max(最大资源 / 10 - 10, 0) * 人力);
            掉极品率 = 掉宝率 * Mathf.Min(1,極品門檻) * Mathf.Min(1,Mathf.Pow(極品門檻 * 库存率 / 100, 提升品阶));
        }
    }

}
