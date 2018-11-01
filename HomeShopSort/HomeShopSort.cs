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
//建筑选人界面排序
namespace HomeShopSort
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        public static int sortModeNone = 0;
        public static int sortModeReal = 1;
        public static int sortModeAssume = 2;
        public bool showEfficiencyAsMyWay = false;//显示效率占理论效率的最大值
        public bool showEfficienctInBottom = true;//显示在正下方
        public bool showEfficienctUsingDifferentColor = true;//用不同颜色显示
        public int sortMode = 0;//排序模式
        public int assumeTalent = 80;//假想数值
    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static String[] toolBarText = { "不排序","按实际效率排序","按假想效率排序" };

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
            Main.settings.showEfficiencyAsMyWay = GUILayout.Toggle(Main.settings.showEfficiencyAsMyWay, "用占最大效率的百分比显示效率", new GUILayoutOption[0]);
            Main.settings.showEfficienctInBottom = GUILayout.Toggle(Main.settings.showEfficienctInBottom, "显示在建筑名上而非左上角", new GUILayoutOption[0]);
            Main.settings.showEfficienctUsingDifferentColor = GUILayout.Toggle(Main.settings.showEfficienctUsingDifferentColor, "用不同颜色显示效率", new GUILayoutOption[0]);
            Main.settings.sortMode = GUILayout.Toolbar(Main.settings.sortMode, toolBarText);
            GUILayout.EndHorizontal();
            if (Main.settings.sortMode == Settings.sortModeAssume)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("假定所有厢房里的npc资质为:");
                int.TryParse(GUILayout.TextField(Main.settings.assumeTalent.ToString(), 3, GUILayout.Width(90)), out Main.settings.assumeTalent);
                GUILayout.EndHorizontal();
            }
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }
    //立即移除上次显示的物体，防止和这次的弄混
    [HarmonyPatch(typeof(HomeSystem), "RemoveActor", new Type[] { })]
    public static class HomeSystem_Remove_Patch
    {
        static void Prefix()
        {
            if (!Main.enabled)
                return;
            while (HomeSystem.instance.listActorsHolder.childCount > 0)
                UnityEngine.Object.DestroyImmediate(HomeSystem.instance.listActorsHolder.GetChild(0).gameObject);
        }
    }
    //显示工作信息
    [HarmonyPatch(typeof(HomeBuilding), "UpdateBuilding", new Type[] { })]
    public static class HomeBuilding_UpdateBuilding_Patch
    {
        static void Postfix(HomeBuilding __instance)
        {
            if (!Main.enabled)
                return;
            string[] array = __instance.name.Split(new char[] { ',' });
            int partId = int.Parse(array[1]);
            int placeId = int.Parse(array[2]);
            int buildingId = int.Parse(array[3]);
            int[] buildingDate = DateFile.instance.homeBuildingsDate[partId][placeId][buildingId];
            if (buildingDate[0] > 0 && int.Parse(DateFile.instance.basehomePlaceDate[buildingDate[0]][3]) == 1 && buildingDate[3] <= 0)
            {
                if (DateFile.instance.actorsWorkingDate.ContainsKey(partId) && DateFile.instance.actorsWorkingDate[partId].ContainsKey(placeId) && DateFile.instance.actorsWorkingDate[partId][placeId].ContainsKey(buildingId))
                {
                    int eff = HomeSystem.instance.GetBuildingLevelPct(partId, placeId, buildingId);
                    int total = int.Parse(DateFile.instance.basehomePlaceDate[buildingDate[0]][91]);
                    if (total > 0)
                    {
                        string text = "";
                        int color = 20002 + 6;
                        if (Main.settings.showEfficienctUsingDifferentColor)
                        {
                            int percent = eff / 2;
                            if (percent >= 90)
                                color = 20002 + 2;
                            else if (percent >= 70)
                                color = 20002 + 1;
                            else if (percent >= 50)
                                color = 20002 + 7;
                            else
                                color = 20002 + 8;
                        }
                        if (Main.settings.showEfficiencyAsMyWay)
                            text = "(" + (eff / 2).ToString() + "%)";
                        else
                            text = "(" + (100 * eff / total).ToString() + "%)";
                        if (!Main.settings.showEfficienctInBottom)
                            __instance.pctText.text += DateFile.instance.SetColoer(color,
                                                     text, false);
                        else
                            __instance.placeName.text += DateFile.instance.SetColoer(color,
                                                     text, false);
                    }
                }
                else
                {
                    string text = "(空闲)";
                    int color = 20002 + 8;
                    if (!Main.settings.showEfficienctInBottom)
                        __instance.pctText.text += DateFile.instance.SetColoer(color,
                                                 text, false);
                    else
                        __instance.placeName.text += DateFile.instance.SetColoer(color,
                                                 text, false);
                }
            }
        }
    }
    //将显示人物的物体重排序(实际上是对每个物体重新设置了内容)
    [HarmonyPatch(typeof(HomeSystem), "ShowWorkingAcotrWindow", new Type[] { })]
    public static class HomeSystem_ShowWorkingAcotrWindow_Patch
    {
        public static void Postfix()
        {
            if (!Main.enabled)
                return;
            if (!HomeSystem.instance.buildingWindowOpend)
                return;
            if (Main.settings.sortMode == Settings.sortModeNone)
                return;
            int key = HomeSystem.instance.homeMapPartId;
            int key2 = HomeSystem.instance.homeMapPlaceId;
            int key3 = HomeSystem.instance.homeMapbuildingIndex;
            int[] array = DateFile.instance.homeBuildingsDate[key][key2][key3];
            int _skillTyp=0;
            bool favorChange=false;
            int num = int.Parse(DateFile.instance.basehomePlaceDate[array[0]][33]);
            if (num > 0)
            {
                _skillTyp = num;
                favorChange = false;
            }
            else if (float.Parse(DateFile.instance.basehomePlaceDate[array[0]][62]) > 0f)
            {
                _skillTyp = 3;
                favorChange = true;
            }

            var sort_list =new List<KeyValuePair<int, int>>();
            for (int i = 0; i < HomeSystem.instance.listActorsHolder.childCount; i++)
            {
                string name = HomeSystem.instance.listActorsHolder.GetChild(i).name;
                var cont = name.Split(new char[] { ',' });
                if(cont.Count()==2&&cont[0]=="Actor")
                {
                    int id = int.Parse(cont[1]);
                    sort_list.Add(new KeyValuePair<int, int>(id, GetWorkingData(id)));
                }
            }                
            sort_list.Sort((KeyValuePair<int, int> s1, KeyValuePair<int, int> s2) => s2.Value.CompareTo(s1.Value) );

            for (int i = 0; i < sort_list.Count(); i++)
            {
                int id = sort_list[i].Key;
                HomeSystem.instance.listActorsHolder.GetChild(i).name = "Actor," + id;
                HomeSystem.instance.listActorsHolder.GetChild(i).GetComponent<SetWorkActorIcon>().SetActor(id, _skillTyp, favorChange);
                HomeSystem.instance.listActorsHolder.GetChild(i).gameObject.AddComponent<PointerEnter>();
            }
        }

        //工作效率，厢房按心情取负
        public static int GetWorkingData(int workerId)
        {
            if (HomeSystem.instance == null)
                return -1;
            if (!HomeSystem.instance.buildingWindowOpend)
                return -1;
            int buildingIndex = HomeSystem.instance.homeMapbuildingIndex;
            int partId = HomeSystem.instance.homeMapPartId;
            int placeId = HomeSystem.instance.homeMapPlaceId;
            if ((!DateFile.instance.baseHomeDate.ContainsKey(partId))
                || (!DateFile.instance.baseHomeDate[partId].ContainsKey(placeId))
                || DateFile.instance.baseHomeDate[partId][placeId] == 0)
                return -1;
            if (partId < 0 || placeId < 0)
                return -1;
            int[] array = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int requiredTalentIndex = int.Parse(DateFile.instance.basehomePlaceDate[array[0]][33]);//所需资质的序号
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
            int talent = (requiredTalentIndex <= 0) ? 0 : int.Parse(DateFile.instance.GetActorDate(workerId, requiredTalentIndex, true));
            if (requiredTalentIndex == 18)//18=声望
            {
                talent += 100;
            }
            int xiangFangAddup = 0;
            foreach (int key in HomeSystem.instance.GetBuildingNeighbor(partId, placeId, buildingIndex,1))
            {
                if (DateFile.instance.homeBuildingsDate[partId][placeId].ContainsKey(key) && DateFile.instance.actorsWorkingDate[partId][placeId].ContainsKey(key) && int.Parse(DateFile.instance.basehomePlaceDate[DateFile.instance.homeBuildingsDate[partId][placeId][key][0]][62]) != 0)
                {
                    int singleAddup = 0;
                    if (Main.settings.sortMode == Settings.sortModeReal)
                    {
                        singleAddup = (requiredTalentIndex <= 0) ? 0 : int.Parse(DateFile.instance.GetActorDate(DateFile.instance.actorsWorkingDate[partId][placeId][key], requiredTalentIndex, true));
                        if (requiredTalentIndex == 18)
                            singleAddup += 100;
                    }
                    else
                        singleAddup = Main.settings.assumeTalent;
                    xiangFangAddup += singleAddup;
                }
            }

            talent = (talent+xiangFangAddup) * Mathf.Max(moodFavorAddup, 0) / 100;
            int num6 = Mathf.Max(int.Parse(DateFile.instance.basehomePlaceDate[array[0]][51]) + (array[1] - 1), 1);
            int efficiency = Mathf.Clamp(talent * 100 / num6, 50, 200);
            int total = int.Parse(DateFile.instance.basehomePlaceDate[array[0]][91]);
            if (total > 0)
                return efficiency;
            else
                return 1000-mood;
        }
    }
}