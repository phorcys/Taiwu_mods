using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;

/// <summary>
/// 作弊用的mod
/// </summary>
namespace LKX_CheatMaxPeople
{
    /// <summary>
    /// 设置文件
    /// </summary>
    public class Settings : UnityModManager.ModSettings
    {
        /// <summary>
        /// 人口
        /// </summary>
        public int maxPeople;

        /// <summary>
        /// 蛐蛐时节开启
        /// </summary>
        public bool ququLife;

        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="modEntry"></param>
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    public class Main
    {
        /// <summary>
        /// umm日志
        /// </summary>
        public static UnityModManager.ModEntry.ModLogger logger;

        /// <summary>
        /// mod设置
        /// </summary>
        public static Settings settings;

        /// <summary>
        /// 是否开启mod
        /// </summary>
        public static bool enabled;

        /// <summary>
        /// 载入mod。
        /// </summary>
        /// <param name="modEntry">mod管理器对象</param>
        /// <returns></returns>
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Main.logger = modEntry.Logger;
            Main.settings = Settings.Load<Settings>(modEntry);

            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());

            modEntry.OnToggle = Main.OnToggle;
            modEntry.OnGUI = Main.OnGUI;
            modEntry.OnSaveGUI = Main.OnSaveGUI;

            return true;
        }

        /// <summary>
        /// 确定是否激活mod
        /// </summary>
        /// <param name="modEntry">umm</param>
        /// <param name="value">是否激活</param>
        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Main.enabled = value;
            return true;
        }

        /// <summary>
        /// 展示mod的设置
        /// </summary>
        /// <param name="modEntry">umm</param>
        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUIStyle redLabelStyle = new GUIStyle();
            redLabelStyle.normal.textColor = new Color(159f / 256f, 20f / 256f, 29f / 256f);
            GUILayout.Label("修改人力上限！", redLabelStyle);

            string maxPeople = GUILayout.TextField(Main.settings.maxPeople.ToString());
            if (GUI.changed)
            {
                if (string.Empty == maxPeople) maxPeople = "0";

                Main.settings.maxPeople = int.Parse(maxPeople);
            }

            Main.settings.ququLife = GUILayout.Toggle(Main.settings.ququLife, "蛐蛐无限寿命，已死的不复活。");

            if (GUILayout.Button("测试"))
            {
                Main.TestModValue();
            }
        }

        /// <summary>
        /// 保存mod的设置
        /// </summary>
        /// <param name="modEntry">umm</param>
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Main.settings.Save(modEntry);
        }

        static void TestModValue()
        {
            DateFile df = DateFile.instance;
            int i = 0;
            List<int> boxQuQu = new List<int>();
            foreach (int[] box in df.cricketBoxDate.Values)
            {
                if (box[0] != -97) boxQuQu.Add(box[0]);
            }

            foreach (int actorId in df.acotrTeamDate)
            {
                if (int.Parse(df.GetActorDate(actorId, 302)) != 0) boxQuQu.Add(int.Parse(df.GetActorDate(actorId, 302)));
            }

            foreach (KeyValuePair<int, Dictionary<int, string>> item in DateFile.instance.itemsDate)
            {
                if ((DateFile.instance.actorItemsDate[10001].ContainsKey(item.Key) || DateFile.instance.actorItemsDate[-999].ContainsKey(item.Key) || boxQuQu.Contains(item.Key)) && item.Value[999] == "10000")
                {
                    i++;
                }
            }

            foreach (int aaa in df.acotrTeamDate)
            {
                Main.logger.Log(aaa.ToString());
            }
            Main.logger.Log(i.ToString() + "个");
        }
    }

    [HarmonyPatch(typeof(UIDate), "GetMaxManpower")]
    public class MaxPeople_For_UIDate_GetBaseMaxManpower
    {
        static void Postfix(ref int __result)
        {
            if (Main.enabled && Main.settings.maxPeople >= 0) __result += Main.settings.maxPeople;
        }
    }

    [HarmonyPatch(typeof(SaveDateFile), "LateUpdate")]
    public class QuQuLife_For_SaveDateFile_LateUpdate
    {
        static void Prefix(SaveDateFile __instance)
        {
            if (Main.enabled && Main.settings.ququLife && __instance.saveSaveDate)
            {
                DateFile df = DateFile.instance;
                List<int> boxQuQu = new List<int>();
                foreach (int[] box in df.cricketBoxDate.Values)
                {
                    if (box[0] != -97) boxQuQu.Add(box[0]);
                }

                foreach (int actorId in df.acotrTeamDate)
                {
                    if (int.Parse(df.GetActorDate(actorId, 302)) != 0) boxQuQu.Add(int.Parse(df.GetActorDate(actorId, 302)));
                }

                foreach (KeyValuePair<int, Dictionary<int, string>> item in df.itemsDate)
                {
                    if ((df.actorItemsDate[10001].ContainsKey(item.Key) || df.actorItemsDate[-999].ContainsKey(item.Key) || boxQuQu.Contains(item.Key)) && item.Value[999] == "10000")
                    {
                        if (item.Value[901] != "0" && item.Value[2007] != "0") item.Value[2007] = "0";
                    }
                }
            }
        }
    }
}
