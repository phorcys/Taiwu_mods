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
            GUILayout.BeginHorizontal();
            Main.settings.addonInfo = GUILayout.Toggle(Main.settings.addonInfo, "显示未加成信息", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Main.settings.shopName = GUILayout.Toggle(Main.settings.shopName, "显示商人商会", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Main.settings.healthStatus = GUILayout.Toggle(Main.settings.healthStatus, "显示健康状态", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Main.settings.workPlace = GUILayout.Toggle(Main.settings.workPlace, "显示太吾村民工作地点", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();


        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }


    [HarmonyPatch(typeof(WorldMapSystem), "UpdatePlaceActor", new Type[] { typeof(int), typeof(int) })]
    public static class WorldMapSystem_UpdatePlaceActor_Patch
    {

        static void Postfix(Transform ___actorHolder, ref int ___showPlaceActorTyp)
        {
            if (!Main.enabled)
            {
                return;
            }

            int count = ___actorHolder.childCount;
            for (int i = 0; i < count; i++)
            {
               GameObject obj = ___actorHolder.GetChild(i).gameObject;
                if (obj.GetComponent<PointerEnter>() == null && ___showPlaceActorTyp==1) {
                    obj.AddComponent<PointerEnter>();
                }
            }

        }
    }


    [HarmonyPatch(typeof(WindowManage), "WindowSwitch")]
    public static class WindowManage_WindowSwitch_Patch
    {
        static void Postfix(WorldMapSystem __instance, ref GameObject tips, ref bool on, ref Text ___itemMoneyText, ref Text ___itemLevelText, ref Text ___informationMassage, ref Text ___informationName, ref bool ___anTips)
        {
            if (!Main.enabled)
            {
                return;
            }

            if (tips != null && ___anTips == false && on)
            {

                string[] array = tips.name.Split(new char[]
{
                                        ','
});
                if (array[0] == "Actor")
                {
                    int typ = int.Parse(typeof(WorldMapSystem).GetField("showPlaceActorTyp", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(WorldMapSystem.instance).ToString());
                    if (typ == 1 && WorldMapSystem.instance.choosePlaceId == DateFile.instance.mianPlaceId)
                    {
                        int id = int.Parse(array[1]);
                        ___informationMassage.text = "";
                        ___informationName.text = DateFile.instance.GetActorName(id, true, false) + "（" + GetFame(id) + "）\n";
                        if (Main.settings.shopName)
                        {
                            string shopName = GetShopName(id);
                            if (Main.settings.healthStatus && shopName.Length > 0)
                            {
                                ___informationName.text += shopName + "\n";
                            }
                            else
                            {
                                ___informationName.text += shopName;
                            }
                        }
                        if (Main.settings.workPlace)
                        {
                            string workPlace = GetWorkPlace(id);
                            if (Main.settings.healthStatus && workPlace.Length != 0)
                            {
                            ___informationName.text += workPlace + "\n";
                            }
                            else
                            {
                                ___informationName.text += GetWorkPlace(id);
                            }
                        }
                        if (Main.settings.healthStatus)
                        {
                            List<int> list = GetHPSP(id);
                            List<int> list1 = GetPoison(id);
                            if (list[0] != 0 || list[2] != 0 || GetPoison(id)[0] == 1)
                            {
                                if (GetPoison(id)[0] == 1)
                                {
                                    if (list[0] != 0 || list[2] != 0)
                                    {
                                        ___informationName.text += DateFile.instance.SetColoer(20010, "受伤") + "/" + DateFile.instance.SetColoer(20007, "中毒");

                                    }
                                    else
                                    {
                                        ___informationName.text += DateFile.instance.SetColoer(20007, "中毒");
                                    }

                                }
                                else
                                {
                                    ___informationName.text += DateFile.instance.SetColoer(20010, "受伤");
                                }

                            }
                            else { ___informationName.text += DateFile.instance.SetColoer(20004, "健康"); }
                        }
                        ___itemLevelText.text = string.Format("\t{0}({1})", GetAge(id), GetHealth(id));
                        ___itemMoneyText.text = GetChame(id, Main.settings.addonInfo);
                        Text text = ___informationMassage;
                        text.text += "\t立场：" + GetGoodness(id);
                        text.text += "\t\t\t喜好：" + Gethobby(id, 0);
                        text.text += "\t\t\t厌恶：" + Gethobby(id, 1) + "\n\n";
                        string text1 = Main.settings.addonInfo ? "\t\t\t\t" : "\t";

                        for (int i = 0; i < 16; i++)
                        {
                            if (i < 14)
                            {

                                text.text += string.Format("\t{0}\t\t\t\t{1}\n", GetLevel(id, i, 0, Main.settings.addonInfo), GetLevel(id, i, 1, Main.settings.addonInfo));


                            }
                            else
                            {
                                text.text += string.Format("\t{0}\n", GetLevel(id, i, 0, Main.settings.addonInfo));
                            }
                        }


                        ___anTips = true;
                    }
                }
            }
        }

        static string GetChame(int id, bool shownoadd)
        {
            // 显示未加成数据 true
            string text = ((int.Parse(DateFile.instance.GetActorDate(id, 11, false)) > 14) ? ((int.Parse(DateFile.instance.GetActorDate(id, 8, false)) != 1 || int.Parse(DateFile.instance.GetActorDate(id, 305, false)) != 0) ? DateFile.instance.massageDate[25][int.Parse(DateFile.instance.GetActorDate(id, 14, false)) - 1].Split(new char[]
{
                                        '|'
})[Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(id, 15, true)) / 100, 0, 9)] : DateFile.instance.massageDate[25][5].Split(new char[]
{
                                        '|'
})[1]) : DateFile.instance.massageDate[25][5].Split(new char[]
{
                                        '|'
})[0]);
            if (shownoadd)
            {
                text += "（" + ((int.Parse(DateFile.instance.GetActorDate(id, 11, false)) > 14) ? ((int.Parse(DateFile.instance.GetActorDate(id, 8, false)) != 1 || int.Parse(DateFile.instance.GetActorDate(id, 305, false)) != 0) ? DateFile.instance.massageDate[25][int.Parse(DateFile.instance.GetActorDate(id, 14, false)) - 1].Split(new char[]
{
                                        '|'
})[Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(id, 15, false)) / 100, 0, 9)] : DateFile.instance.massageDate[25][5].Split(new char[]
{
                                        '|'
})[1]) : DateFile.instance.massageDate[25][5].Split(new char[]
{
                                        '|'
})[0]) + "）";
            }
            return text;
        }

        static string GetFame(int id)
        {
            string text = ActorMenu.instance.Color7(int.Parse(DateFile.instance.GetActorDate(id, 18, false)));
            return text;
        }

        static string GetGoodness(int id)
        {
            string text = DateFile.instance.massageDate[9][0].Split(new char[]
{
                                                    '|'
})[DateFile.instance.GetActorGoodness(id)];
            return text;
        }

        static string Gethobby(int id, int hobby)
        {
            //喜欢 0 讨厌 1
            string text = ((int.Parse(DateFile.instance.GetActorDate(id, 207 + hobby, false)) != 1) ? DateFile.instance.massageDate[301][1] : DateFile.instance.massageDate[301][0].Split(new char[]
{
                                                        '|'
})[int.Parse(DateFile.instance.GetActorDate(id, 202 + hobby, false))]);
            return text;

        }

        static string GetLevel(int id, int index, int gongfa, bool shownoadd)
        {
            // 生活技能 0 战斗技能 1
            //显示未加成数据 true
            int num = int.Parse(DateFile.instance.GetActorDate(id, 501 + index + 100 * gongfa, true));
            string text = DateFile.instance.SetColoer(20002 + Mathf.Clamp((num - 20) / 10, 0, 8), string.Concat(new object[]
    {
                                                                DateFile.instance.baseSkillDate[index+101*gongfa][0],
                                                                DateFile.instance.massageDate[7003][4].Split(new char[]
                                                                {
                                                                    '|'
                                                                })[2],
                                                                WindowManage.instance.Mut(),
                                                                num
    }), false);

            text += gongfa == 1 ? "" : num < 10 ? "\t\t" : num < 100 ? "\t" : "";
            if (shownoadd)
            {
                num = int.Parse(DateFile.instance.GetActorDate(id, 501 + index + 100 * gongfa, false));
                text += "(" + DateFile.instance.SetColoer(20002 + Mathf.Clamp((num - 20) / 10, 0, 8), num.ToString()) + ")";
                text += gongfa == 1 ? "" : num < 10 ? "\t" : num < 100 ? "\t" : "";
            }

            return text;


        }

        static string GetAge(int id)
        {
            int age = (int.Parse(DateFile.instance.GetActorDate(id, 11, false)));
            return age.ToString();
        }
        static string GetHealth(int id)
        {
            int health = ActorMenu.instance.Health(id);
            int maxhealth = ActorMenu.instance.MaxHealth(id);
            string peopleHealthText = string.Format("{0}{1}</color> / {2}", ActorMenu.instance.Color3(health, maxhealth), health, maxhealth);
            return peopleHealthText;
        }

        static List<int> GetHPSP(int id)
        {
            int Hp = ActorMenu.instance.Hp(id, false);
            int maxHp = ActorMenu.instance.MaxHp(id);
            int Sp = ActorMenu.instance.Sp(id, false);
            int maxSp = ActorMenu.instance.MaxHp(id);
            List<int> list = new List<int>
            {Hp,
            maxHp,
            Sp,
            maxSp
            };
            return list;
        }

        static List<int> GetPoison(int id)
        {
            List<int> list = new List<int> { 0 };

            for (int i = 0; i < 6; i++)
            {
                int num = int.Parse(DateFile.instance.GetActorDate(id, 51 + i, false));
                list[0] = num > 0 ? 1 : list[0];
                list.Add(num);
            }
            return list;

        }

        static string GetShopName(int id)
        {
            string text = "";
            if (GetGangLevelText(id) == "商人")
            {
                int typ = int.Parse(DateFile.instance.GetGangDate(int.Parse(DateFile.instance.GetActorDate(id, 9, false)), 16));
                text = string.Format("{0}", DateFile.instance.storyShopDate[typ][0], DateFile.instance.massageDate[11][2]);
            }
            return text;
        }

        static string GetGangLevelText(int id)
        {
            int num2 = int.Parse(DateFile.instance.GetActorDate(id, 19, false));
            int num3 = int.Parse(DateFile.instance.GetActorDate(id, 20, false));
            int key2 = (num3 >= 0) ? 1001 : (1001 + int.Parse(DateFile.instance.GetActorDate(id, 14, false)));
            int gangValueId = DateFile.instance.GetGangValueId(num2, num3);
            string gang = DateFile.instance.presetGangGroupDateValue[gangValueId][key2];
            return gang;
        }

        static string GetWorkPlace(int id)
        {
            // 返回 int[]{partid,placeid}
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
    }
}
