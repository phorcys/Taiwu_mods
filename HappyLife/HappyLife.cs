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

namespace HappyLife
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        public int numChild = 1;
        public bool npcUpLimit = false;
        public int numBuild = 0;
        public int numFavor = 0;
        public bool skip = false;
        public bool unBuildLimit = false;
        public bool unlockAll = false;
        public bool simpleMake = false;
        public bool forcechange = false;


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
            if (!value)
                return false;

            enabled = value;

            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
            Main.settings.skip = GUILayout.Toggle(Main.settings.skip, "跳过测试版说明。", new GUILayoutOption[0]);
            Main.settings.unBuildLimit = GUILayout.Toggle(Main.settings.unBuildLimit, "取消新建建筑相邻限制", new GUILayoutOption[0]);
            Main.settings.unlockAll = GUILayout.Toggle(Main.settings.unlockAll, "空地全开", new GUILayoutOption[0]);
            Main.settings.simpleMake = GUILayout.Toggle(Main.settings.simpleMake, "简化制造系统操作", new GUILayoutOption[0]);
            Main.settings.forcechange = GUILayout.Toggle(Main.settings.forcechange, "强制更换村民工作地点", new GUILayoutOption[0]);
            GUILayout.Label("人物可生育孩子总数量", new GUILayoutOption[0]);
            Main.settings.npcUpLimit = GUILayout.Toggle(Main.settings.npcUpLimit, "对NPC生效", new GUILayoutOption[0]);
            Main.settings.numChild = GUILayout.SelectionGrid(Main.settings.numChild, new string[]
            {
        "少生",
        "正常",
        "2倍",
        "3倍",
        "4倍"
            }, 5, new GUILayoutOption[0]);
            GUILayout.Label("建筑每次升级", new GUILayoutOption[0]);
            Main.settings.numBuild = GUILayout.SelectionGrid(Main.settings.numBuild, new string[]
            {
        "正常",
        "2级",
        "3级",
        "4级",
        "5级"
            }, 5, new GUILayoutOption[0]);
            GUILayout.Label("好感增加时倍率", new GUILayoutOption[0]);
            Main.settings.numFavor = GUILayout.SelectionGrid(Main.settings.numFavor, new string[]
            {
        "正常",
        "2倍",
        "3倍",
        "4倍",
        "5倍"
            }, 5, new GUILayoutOption[0]);
            GUILayout.EndVertical();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }

    [HarmonyPatch(typeof(PeopleLifeAI), "AISetChildren")]
    public static class PeopleLifeAI_AISetChildren_Patch
    {
        static bool Prefix(PeopleLifeAI __instance, ref int fatherId, ref int motherId, ref int setFather, ref int setMother)
        {
            if (!Main.enabled || Main.settings.numChild==1)
            {
                return true;
            }
            //Main.Logger.Log("生孩子倍率：" + Main.settings.numChild);
            int num = Main.settings.numChild == 0?50:100 * Main.settings.numChild;
            int num4 = num;
            if (int.Parse(DateFile.instance.GetActorDate(motherId, 14, false)) == 2)
            {
                if (!DateFile.instance.HaveLifeDate(motherId, 901) && UnityEngine.Random.Range(0, 15000) < int.Parse(DateFile.instance.GetActorDate(fatherId, 24, true)) * int.Parse(DateFile.instance.GetActorDate(motherId, 24, true)))
                {

                    int num2 = DateFile.instance.MianActorID();
                    bool flag = fatherId == num2 || motherId == num2;
                    int num3 = (!flag) ? Main.settings.npcUpLimit || Main.settings.numChild == 0 ? 50 : 50 * Main.settings.numChild : 25;
                    num -= DateFile.instance.GetActorSocial(fatherId, 310, false).Count * num3;
                    num -= DateFile.instance.GetActorSocial(motherId, 310, false).Count * num3;
                    if (UnityEngine.Random.Range(0, num4) < num)
                    {
                        DateFile.instance.ChangeActorFeature(motherId, 4002, 4003);
                        if (flag && UnityEngine.Random.Range(0, 100) < (DateFile.instance.getQuquTrun - 100) / 10)
                        {
                            DateFile.instance.getQuquTrun = 0;
                            DateFile.instance.actorLife[motherId].Add(901, new List<int>
                    {
                        1042,
                        fatherId,
                        motherId,
                        setFather,
                        setMother
                    });
                        }
                        else
                        {
                            DateFile.instance.actorLife[motherId].Add(901, new List<int>
                    {
                        UnityEngine.Random.Range(6, 10),
                        fatherId,
                        motherId,
                        setFather,
                        setMother
                    });
                        }
                    }
                }
            }
            return false;
        }

    }

    [HarmonyPatch(typeof(DateFile), "SetHomeBuildingValue")]
    public static class DateFile_SetHomeBuildingValue_Patch
    {
        static void Prefix(DateFile __instance, ref int partId, ref int placeId, ref int buildingIndex, ref int dateIndex, ref int dateValue)
        {

            if (!Main.enabled || Main.settings.numBuild == 0)
            {
                return;
            }
            //Main.Logger.Log("建筑升级倍率：" + Main.settings.numBuild+1);
            if (dateIndex == 1)
            {

                if (dateValue != 1)
                {
                    dateValue += Main.settings.numBuild;
                    int max = int.Parse(DateFile.instance.basehomePlaceDate[DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex][0]][1]);
                    if (dateValue > max)
                    {
                        dateValue = max;
                    }


                }
            }
        }
    }

    [HarmonyPatch(typeof(DateFile), "ChangeFavor")]
    public static class DateFile_ChangeFavor_Patch
    {
        static void Prefix(DateFile __instance, ref int value)
        {
            if (!Main.enabled || Main.settings.numFavor==0)
            {
                return;
            }
            //Main.Logger.Log("好感倍率：" + Main.settings.numFavor+1);
            if (value > 0)
            {
                value *= (Main.settings.numFavor + 1);
            }
        }
    }

    [HarmonyPatch(typeof(MainMenu), "CloseStartMask")]
    public static class MainMenu_CloseStartMask_Patch
    {
        static void Prefix(MainMenu __instance, ref bool ___showStartMassage)
        {
            if (!Main.enabled || !Main.settings.skip)
            {
                return;
            }
            ___showStartMassage = false;
        }
    }


    [HarmonyPatch(typeof(HomeSystem), "GetBuildingNeighbor")]
    public static class HomeSystem_GetBuildingNeighbor_Patch
    {
        static void Postfix(HomeSystem __instance, ref int partId, ref int placeId, ref int buildingIndex, int ___buildingId, ref int[] __result)
        {
            if (!Main.enabled || !Main.settings.unBuildLimit)
            {
                return;
            }
            int[] buildData = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int exnum = int.Parse(DateFile.instance.basehomePlaceDate[buildData[0]][42]);
            if (exnum != 0)
            {
                return;
            }
            Checkchange(partId, placeId);
            List<int> list = __result.ToList();
            if (___buildingId != 0) //物品建筑不为0
            {
                bool buildon = false;
                int length = __result.Length;
                for (int i = 0; i < length; i++)
                {
                    if (DateFile.instance.homeBuildingsDate[partId][placeId].ContainsKey(__result[i]))
                    {
                        //Main.Logger.Log(DateFile.instance.homeBuildingsDate[partId][placeId][__result[i]][0].ToString());
                        if (DateFile.instance.homeBuildingsDate[partId][placeId][__result[i]][0] > 200 && DateFile.instance.homeBuildingsDate[partId][placeId][__result[i]][0] < 20000)
                        {
                            buildon = true;
                            break;
                        }
                    }
                }
                if (buildon || Main.settings.unlockAll || length == 4)
                {
                    string[] needBuild = DateFile.instance.basehomePlaceDate[___buildingId][5].Split(new char[] { '|' }); //需要建筑列表
                    for (int i = 0; i < needBuild.Length; i++)
                    {
                        int buildId = int.Parse(needBuild[i]);
                        foreach (int key in dictionary.Keys)//枚举当前地图列表
                        {
                            if (buildId == dictionary[key][0])
                            {
                                list.Add(key);
                                break;
                            }
                        }

                    }
                }

            }
            if (buildData[0] != 0 || Main.settings.unlockAll) //判断当前建筑是否为空地且不是可扩张地块
            {

                list = list.Union(addList).ToList(); //合并列表

            }
            __result = list.ToArray();

        }

        static void Checkchange(int partId, int placeId)
        {

            if (part != partId || place != placeId)
            {

                addList.Clear();
                dictionary = DateFile.instance.homeBuildingsDate[partId][placeId];
                part = partId;
                place = placeId;

                foreach (int key in dictionary.Keys)
                {
                    if (dictionary[key][0] == 0)
                    {
                        continue;
                    }
                    if (dictionary[key][0] == 1001) //设置建筑永远靠近太吾村
                    {
                        addList.Add(key);
                        continue;
                    }
                    Dictionary<int, string> baseHome = DateFile.instance.basehomePlaceDate[dictionary[key][0]];
                    //需要编号66,67,68,79,80
                    if (int.Parse(baseHome[66]) != 0 || int.Parse(baseHome[67]) != 0 || int.Parse(baseHome[68]) != 0 || int.Parse(baseHome[79]) != 0 || int.Parse(baseHome[80]) != 0)
                    {
                        addList.Add(key);
                    }
                }
            }
        }

        private static int part = -1;
        private static int place = -1;
        private static Dictionary<int, int[]> dictionary;
        private static List<int> addList = new List<int>();
    }

    [HarmonyPatch(typeof(MakeSystem), "SetMakeWindow")]
    public static class MakeSystem_SetMakeWindow_Patch
    {
        static bool Prefix(MakeSystem __instance, ref int typ, ref int ___makeTyp, ref int ___baseMakeTyp, ref int ___mianItemId, ref int ___secondItemId, ref int[] ___thirdItemId)
        {
            if (!Main.enabled || !Main.settings.simpleMake)
            {
                return true;
            }
            //Main.Logger.Log("typ:" + typ);
            //Main.Logger.Log("___baseMakeTyp" + ___baseMakeTyp);
            if (makeType != ___baseMakeTyp)
            {
                makeType = ___baseMakeTyp;
                return true;
            }
            ___makeTyp = typ;
            ___secondItemId = 0;
            ___thirdItemId[0] = 0;
            ___thirdItemId[1] = 0;
            ___thirdItemId[2] = 0;
            __instance.UpdateMakeWindow();
            return false;
        }
        private static int makeType = -111;
        //private static MethodInfo MakeSystem_RemoveAllUseResourceLevel = typeof(MakeSystem).GetMethod("RemoveAllUseResourceLevel", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    [HarmonyPatch(typeof(HomeSystem), "SetWorkingActor")]
    public static class HomeSystem_SetWorkingActor_Patch
    {
        static bool Prefix(HomeSystem __instance,ref int key,ref int ___workingActorId,ref Button ___canChanageActorButton)
        {
            if (!Main.enabled || !Main.settings.forcechange)
            {
                return true;
            }
            ___workingActorId = key;
            ___canChanageActorButton.interactable = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(HomeSystem), "ChanageWorkingAcotr")]
    public static class HomeSystem_ChanageWorkingAcotr_Patch
    {
        static void Prefix(HomeSystem __instance,int ___workingActorId)
        {
            if (!Main.enabled || !Main.settings.forcechange)
            {
                return;
            }
            var array = DateFile.instance.ActorIsWorking(___workingActorId);
            if (array != null)
            {
                var dic=DateFile.instance.actorsWorkingDate[array[0]][array[1]];
                foreach (int key in dic.Keys)
                {
                    if (dic[key]== ___workingActorId)
                    {
                        __instance.RemoveWorkingActor(array[0], array[1], key);
                        break;
                    }
                } 
            }
        }

    }
}

