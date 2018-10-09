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
        public bool floatInfo = true;
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
            Main.settings.floatInfo = GUILayout.Toggle(Main.settings.floatInfo, "开启人物浮动信息", new GUILayoutOption[0]);
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
        static void Postfix(WorldMapSystem __instance, Transform ___actorHolder)
        {
            if (!Main.enabled || !Main.settings.floatInfo)
            {
                return;
            }
            int child = ___actorHolder.childCount;
            for (int i = 0; i < child; i++)
            {
                ___actorHolder.GetChild(i).gameObject.AddComponent<PointerEnter>();
            }
        }
    }

    [HarmonyPatch(typeof(WindowManage), "WindowSwitch")]
    public static class WindowManage_WindowSwitch_Patch
    {
        static void Postfix(WorldMapSystem __instance, ref GameObject tips,ref bool on, ref Text ___informationMassage, ref Text ___informationName, ref bool ___anTips)
        {
            if (!Main.enabled || !Main.settings.floatInfo)
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
                    int id = int.Parse(array[1]);
                    ___informationMassage.text = "";

                    string chame = ((int.Parse(DateFile.instance.GetActorDate(id, 11, false)) > 14) ? ((int.Parse(DateFile.instance.GetActorDate(id, 8, false)) != 1 || int.Parse(DateFile.instance.GetActorDate(id, 305, false)) != 0) ? DateFile.instance.massageDate[25][int.Parse(DateFile.instance.GetActorDate(id, 14, false)) - 1].Split(new char[]
        {
                        '|'
        })[Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(id, 15, false)) / 100, 0, 9)] : DateFile.instance.massageDate[25][5].Split(new char[]
        {
                        '|'
        })[1]) : DateFile.instance.massageDate[25][5].Split(new char[]
        {
                        '|'
        })[0]);

                    string fame = ActorMenu.instance.Color7(int.Parse(DateFile.instance.GetActorDate(id, 18, false)));
                    string goodnessText = DateFile.instance.massageDate[9][0].Split(new char[]
        {
                                    '|'
        })[DateFile.instance.GetActorGoodness(id)];

                    string love = ((int.Parse(DateFile.instance.GetActorDate(id, 207, false)) != 1) ? DateFile.instance.massageDate[301][1] : DateFile.instance.massageDate[301][0].Split(new char[]
            {
                                        '|'
            })[int.Parse(DateFile.instance.GetActorDate(id, 202, false))]);
                    string hate = ((int.Parse(DateFile.instance.GetActorDate(id, 208, false)) != 1) ? DateFile.instance.massageDate[301][1] : DateFile.instance.massageDate[301][0].Split(new char[]
            {
                                        '|'
            })[int.Parse(DateFile.instance.GetActorDate(id, 203, false))]);
                    ___informationName.text = DateFile.instance.GetActorName(id, true, false)+ "（" + fame +"）" ;
                    Text text = ___informationMassage;
                    text.text += "\t魅力：" + chame;
                    text.text += "\t\t\t\t\t\t\t立场：" + goodnessText + "\n";
                    text.text += "\t喜爱：" + love;
                    text.text += "\t\t\t\t\t\t\t讨厌：" + hate + "\n\n";

  
                    for (int i = 0; i < 16; i++)
                    {
                        int num60 = int.Parse(DateFile.instance.GetActorDate(id, 501 + i, false));
                        int num62 = int.Parse(DateFile.instance.GetActorDate(id, 601 + i, false));
                        string text1 = "\t";
                        if (num60 < 10)
                        {
                            text1 = "\t\t";

                        }
                        else if (num60 > 99)
                        {
                            text1 = "";
                        }

                        if (i < 14)
                        {
                            text.text += string.Format("{0}{1}\t\t{2}\t\t{3}{4}\n", WindowManage.instance.Dit(), DateFile.instance.SetColoer(20002 + Mathf.Clamp((num60 - 20) / 10, 0, 8), string.Concat(new object[]
                            {
                                                DateFile.instance.baseSkillDate[i][0],
                                                DateFile.instance.massageDate[7003][4].Split(new char[]
                                                {
                                                    '|'
                                                })[2],
                                                WindowManage.instance.Mut(),
                                                num60
                            }), false), text1, WindowManage.instance.Dit(), DateFile.instance.SetColoer(20002 + Mathf.Clamp((num62 - 20) / 10, 0, 8), string.Concat(new object[]
                                {
                                                DateFile.instance.baseSkillDate[i + 101][0],
                                                DateFile.instance.massageDate[7003][4].Split(new char[]
                                                {
                                                    '|'
                                                })[2],
                                                WindowManage.instance.Mut(),
                                                num62
                                }), false));

                        }
                        else
                        {
                            text.text += string.Format("{0}{1}\n", WindowManage.instance.Dit(), DateFile.instance.SetColoer(20002 + Mathf.Clamp((num60 - 20) / 10, 0, 8), string.Concat(new object[]
    {
                                                DateFile.instance.baseSkillDate[i][0],
                                                DateFile.instance.massageDate[7003][4].Split(new char[]
                                                {
                                                    '|'
                                                })[2],
                                                WindowManage.instance.Mut(),
                                                num60
    }), false), WindowManage.instance.Dit());
                        }
                    }
                    ___anTips = true;

                }

            }
        }
    }
}
