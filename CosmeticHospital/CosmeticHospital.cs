using System;
using System.Collections.Generic;
using System.Reflection;
using GameData;
using Harmony12;
using UnityEngine;
using UnityModManagerNet;

namespace CosmeticHospital

{
    public class Settings : UnityModManager.ModSettings
    {
    }


    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create(modEntry.Info.Id);
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            //settings = Settings.Load<Settings>(modEntry);

            Logger = modEntry.Logger;

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

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            //settings.Save(modEntry);
        }

        public static Vector2 chosenActorScroll;
        public static int targetActor = -1;

        public static string[] targetPartName =
        {
            "体型", "鼻子", "特征", "眼睛", "眉毛", "嘴唇", "衣着", "发型"
        };

        public static string[] targetPartColor =
        {
            "皮肤颜色", "眉毛颜色", "眼睛颜色", "嘴唇颜色", "未知", "特征颜色", "头发颜色", "衣着颜色"
        };

        public static int[] currentFaceDate;
        public static int[] currentFaceColor;
        public static int currentGender;
        public static int currentGenderChange; //男生女相、女生男相
        public static int[] newFaceDate;
        public static int[] newFaceColor;
        public static int newGender;
        public static int newGenderChange;


        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (!enabled)
            {
                return;
            }

            if (DateFile.instance == null || !Characters.HasChar(DateFile.instance.MianActorID()))
            {
                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("存档未载入");
                GUILayout.EndHorizontal();
                return;
            }

            GUILayout.BeginHorizontal("Box");
            GUILayout.BeginVertical(GUILayout.Width(120));
            GUILayout.Label("请选择美容对象：");
            GUILayout.EndVertical();
            GUILayout.BeginVertical();

            chosenActorScroll = GUILayout.BeginScrollView(chosenActorScroll);
            GUILayout.BeginHorizontal();
            Dictionary<int, bool> chosenActorId = new Dictionary<int, bool>();
            foreach (var actorId in DateFile.instance.GetFamily(false, false))
            {
                string actorName = DateFile.instance.GetActorName(actorId);
                GUILayout.BeginVertical();
                chosenActorId[actorId] = GUILayout.Button(actorName,GUILayout.Width(80));
                GUILayout.EndVertical();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            foreach (var key in chosenActorId.Keys)
            {
                if (chosenActorId[key])
                {
                    targetActor = key;
                    string[] rawFaceDate =
                        DateFile.instance.GetActorDate(targetActor, 995, applyBonus: false).Split('|');
                    string[] rawFaceColor =
                        DateFile.instance.GetActorDate(targetActor, 996, applyBonus: false).Split('|');
                    currentFaceDate = new int[rawFaceDate.Length];
                    currentFaceColor = new int[rawFaceColor.Length];
                    for (int i = 0; i < rawFaceDate.Length; i++)
                    {
                        currentFaceDate[i] = int.Parse(rawFaceDate[i]) + 1;
                    }

                    for (int j = 0; j < rawFaceColor.Length; j++)
                    {
                        currentFaceColor[j] = int.Parse(rawFaceColor[j]) + 1;
                    }

                    newFaceDate = new int[currentFaceDate.Length];
                    newFaceColor = new int[currentFaceColor.Length];
                    currentFaceDate.CopyTo(newFaceDate, 0);
                    currentFaceColor.CopyTo(newFaceColor, 0);

                    currentGender = DateFile.instance.GetActorDate(targetActor, 14, applyBonus: false).ParseInt();
                    currentGenderChange = DateFile.instance.GetActorDate(targetActor, 17, applyBonus: false).ParseInt();
                    newGender = currentGender;
                    newGenderChange = currentGenderChange;
                }
            }
            
            if (targetActor != -1)
            {
                GUILayout.BeginHorizontal("Box");
                GUILayout.BeginVertical("Box",GUILayout.Width(150));
                int[] displayOrder = {1, 3, 5, 2, 7, 4, 0, 6};
                for (int i = 0; i < displayOrder.Length-2; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical(GUILayout.Width(60));
                    GUILayout.Label(targetPartName[displayOrder[i]] + "：" + currentFaceDate[displayOrder[i]]);
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical(GUILayout.Width(30));
                    string temp = GUILayout.TextField(newFaceDate[displayOrder[i]].ToString());
                    if (!int.TryParse(temp, out newFaceDate[displayOrder[i]]) || newFaceDate[displayOrder[i]] == 0)
                    {
                        newFaceDate[displayOrder[i]] = 1;
                    }
                    else if (displayOrder[i] == 2 && newFaceDate[displayOrder[i]] > 30)
                    {
                        newFaceDate[displayOrder[i]] = 30;
                    }
                    else if (displayOrder[i] == 7 && newFaceDate[displayOrder[i]] > 55)
                    {
                        newFaceDate[displayOrder[i]] = 55;
                    }
                    else if (displayOrder[i] != 2 && displayOrder[i] != 7 && newFaceDate[displayOrder[i]] > 45)
                    {
                        newFaceDate[displayOrder[i]] = 45;
                    }

                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
                GUILayout.BeginVertical("Box",GUILayout.Width(170));
                displayOrder = new[]{0, 2, 3, 5, 6, 1, 7, 4};
                for (int i = 0; i < displayOrder.Length-2; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical(GUILayout.Width(80));
                    GUILayout.Label(targetPartColor[displayOrder[i]] + "：" + currentFaceColor[displayOrder[i]]);
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical(GUILayout.Width(30));
                    string temp = GUILayout.TextField(newFaceColor[displayOrder[i]].ToString());
                    if (!int.TryParse(temp, out newFaceColor[displayOrder[i]]))
                    {
                        newFaceColor[displayOrder[i]] = currentFaceColor[displayOrder[i]];
                    }
                    else if (newFaceColor[displayOrder[i]] < 1)
                    {
                        newFaceColor[displayOrder[i]] = 1;
                    }
                    else if (newFaceColor[displayOrder[i]] > 10)
                    {
                        newFaceColor[displayOrder[i]] = 10;
                    }

                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
                GUILayout.BeginVertical("Box");
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(GUILayout.Width(130));
                GUILayout.Label("性别：");
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                int temp1 = newGender - 1;
                temp1 = GUILayout.SelectionGrid(temp1, new[] {"男", "女"}, 2, GUILayout.Width(80));
                newGender = temp1 + 1;
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(GUILayout.Width(130));
                GUILayout.Label("男生女相/女生男相：");
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                newGenderChange = GUILayout.SelectionGrid(newGenderChange, new[] {"否", "是"}, 2, GUILayout.Width(80));
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                string faceDate = String.Empty;
                foreach (var value in newFaceDate)
                {
                    faceDate += value - 1 + "|";
                }

                faceDate = faceDate.Remove(faceDate.LastIndexOf('|'));


                string faceColor = String.Empty;
                foreach (var value in newFaceColor)
                {
                    faceColor += value - 1 + "|";
                }

                faceColor = faceColor.Remove(faceColor.LastIndexOf('|'));

                string gender = newGender.ToString();
                string genderChange = newGenderChange.ToString();

                GUILayout.BeginHorizontal("Box");
                if (GUILayout.Button("应用修改"))
                {
                    Characters.SetCharProperty(targetActor, 995, faceDate);
                    Characters.SetCharProperty(targetActor, 996, faceColor);
                    Characters.SetCharProperty(targetActor, 14, gender);
                    Characters.SetCharProperty(targetActor, 17, genderChange);
                    targetActor = -1;
                }

                GUILayout.EndHorizontal();
            }
        }
    }
}