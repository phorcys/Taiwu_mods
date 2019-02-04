using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace RobAll
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        public int[] grade = new int[7] { 0, 1, 1, 1, 1, 1, 1 };
    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static string labelText = "";
        public static string[] typeName = new string[7]
        {
            "无法获得",
            "制造",
            "丹药",
            "食物",
            "装备",
            "图书",
            "其他"
        };
        public static string[] gradeName = new string[9]
        {
            "<color=#8E8E8EFF>下·九品</color>",
            "<color=#FBFBFBFF>中·八品</color>",
            "<color=#6DB75FFF>上·七品</color>",
            "<color=#8FBAE7FF>奇·六品</color>",
            "<color=#63CED0FF>秘·五品</color>",
            "<color=#AE5AC8FF>极·四品</color>",
            "<color=#E3C66DFF>超·三品</color>",
            "<color=#F28234FF>绝·二品</color>",
            "<color=#E4504DFF>神·一品</color>"
        };

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
            bool flag = GUILayout.Button("一键搜刮被绑架者符合要求的物品");
            if (flag) labelText = Rob();
            GUILayout.Label(labelText);
            GUILayout.EndHorizontal();
            for(int i = 1; i <= 6; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("不低于", GUILayout.Width(50));
                settings.grade[i] = GUILayout.SelectionGrid(settings.grade[i] - 1, gradeName, 9) + 1;
                GUILayout.Label("的" + typeName[i], GUILayout.Width(50));
                GUILayout.EndHorizontal();
            }

        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        static string Rob()
        {
            if (DateFile.instance == null || DateFile.instance.mianActorId <= 0) return "存档未载入";
            int victimCnt = 0;
            int itemCnt = 0;
            foreach (int actorId in DateFile.instance.actorFamilyDate)
            {
                bool isVictim = DateFile.instance.GetActorDate(actorId, 27) == "1";
                string actorName = DateFile.instance.GetActorName(actorId);
                if (!isVictim) continue;
                Main.Logger.Log(actorName);
                victimCnt++;
                var itemsDate = DateFile.instance.actorItemsDate[actorId];
                List<int> keys = itemsDate.Keys.ToList();
                foreach(int itemId in keys)
                {
                    if (!Match(itemId)) continue;
                    int itemNumber = itemsDate[itemId];
                    itemCnt += itemNumber;
                    string itemName = DateFile.instance.GetItemDate(itemId, 0, true).Replace("\n", " ");
                    Main.Logger.Log($"{itemName}*{itemNumber}");
                    ChangeItem(actorId, itemId, itemNumber);
                }
                for(int i = 301; i <= 312; i++)
                {
                    int itemId = int.Parse(DateFile.instance.GetActorDate(actorId, i, false));
                    if (!Match(itemId)) continue;
                    itemCnt += 1;
                    string itemName = DateFile.instance.GetItemDate(itemId, 0, true).Replace("\n", " ");
                    Main.Logger.Log($"{itemName}[装备]");
                    ChangeItem(actorId, itemId);
                }
            }
            if (itemCnt == 0) return "什么都没搜刮到";
            return $"共搜刮了来自{victimCnt}位受害者的{itemCnt}件物品";
        }

        static void ChangeItem(int actorId, int itemId, int itemNumber = 1)
        {
            int mainId = DateFile.instance.mianActorId;
            //DateFile.instance.ChangeTwoActorItem(actorId, mainId, itemId, itemNumber);
            DateFile.instance.LoseItem(actorId, itemId, itemNumber, false, true);
            DateFile.instance.GetItem(mainId, itemId, itemNumber, false);
            string itemValue5 = DateFile.instance.GetItemDate(itemId, 5);//物品小类
            bool like = DateFile.instance.GetActorDate(actorId, 202, false) == itemValue5;
            bool hate = DateFile.instance.GetActorDate(actorId, 203, false) == itemValue5;
            int rate = 100 + 100;//被劫持额外多100
            if (like) rate += 100;
            if (hate) rate -= 50;
            DateFile.instance.SetActorMood(actorId, -int.Parse(DateFile.instance.GetItemDate(itemId, 103, true)), 100, false);//心情
            int favorChange = int.Parse(DateFile.instance.GetItemDate(itemId, 102, true)) * rate / 100;
            DateFile.instance.actorsDate[actorId][210] = (int.Parse(DateFile.instance.GetActorDate(actorId, 210, false)) + favorChange).ToString();//债
            DateFile.instance.ChangeFavor(actorId, -favorChange, false, false);
        }
        static bool Match(int itemId)
        {
            int type = int.Parse(DateFile.instance.GetItemDate(itemId, 4));
            int grade = int.Parse(DateFile.instance.GetItemDate(itemId, 8));
            int require = Main.settings.grade[type];
            return grade >= require;
        }
    }

    
}
