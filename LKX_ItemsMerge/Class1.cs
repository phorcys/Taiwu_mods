using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;

namespace LKX_ItemsMerge
{
    /// <summary>
    /// 设置文件
    /// </summary>
    public class Settings : UnityModManager.ModSettings
    {
        /// <summary>
        /// 自动时节结束合并道具
        /// </summary>
        public bool autoMerge;

        /// <summary>
        /// 合并道具类型
        /// </summary>
        public List<int> mergeType = new List<int>();

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

        //public static List<int> itemsKey = new List<int>();

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
            GUILayout.Label("如果status亮红灯代表mod失效！游戏过程中修改了mod配置需要重新开启游戏让mod生效！", redLabelStyle);
            Main.settings.autoMerge = GUILayout.Toggle(Main.settings.autoMerge, "开启结束时节时自动合并道具");
            if (GUILayout.Button("手动点击合并"))
            {
                Main.RunningMergeItems();
            }

            Main.SetGUIToToggle(31, "药品");
            Main.SetGUIToToggle(22, "工具");

        }

        /// <summary>
        /// 保存mod的设置
        /// </summary>
        /// <param name="modEntry">umm</param>
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Main.settings.Save(modEntry);
        }

        /// <summary>
        /// 生成GUI开关
        /// </summary>
        /// <param name="index">道具类型index 看item的index 5</param>
        /// <param name="name">开关名称</param>
        static void SetGUIToToggle(int index, string name)
        {
            bool status = GUILayout.Toggle(Main.settings.mergeType.Contains(index), name);
            if (GUI.changed)
            {
                if (status)
                {
                    if (!Main.settings.mergeType.Contains(index)) Main.settings.mergeType.Add(index);
                }
                else
                {
                    if (Main.settings.mergeType.Contains(index)) Main.settings.mergeType.Remove(index);
                }
            }
        }

        public static void RunningMergeItems()
        {
            if (!Main.enabled && Main.settings.mergeType.Count <= 0) return;

            DateFile df = DateFile.instance;
            Dictionary<int, int[]> items = new Dictionary<int, int[]>();
            Dictionary<int, int> itemsId = new Dictionary<int, int>();
            if (df.actorItemsDate.TryGetValue(df.mianActorId, out itemsId))
            {
                List<int> buffer = itemsId.Keys.ToList();
                foreach (int itemId in buffer)
                {
                    if (!Main.settings.mergeType.Contains(int.Parse(df.GetItemDate(itemId, 5)))) continue;
                    Main.logger.Log(itemId.ToString() + "...");
                    string id = df.GetItemDate(itemId, 999), surpluses = df.GetItemDate(itemId, 901), limit = df.GetItemDate(itemId, 902);
                    if (items.ContainsKey(int.Parse(id)))
                    {
                        items[int.Parse(id)][0] += int.Parse(surpluses);
                        items[int.Parse(id)][1] += int.Parse(limit);
                    }
                    else
                    {
                        items.Add(int.Parse(id), new int[] { int.Parse(surpluses), int.Parse(limit) });
                    }

                    df.LoseItem(df.mianActorId, itemId, itemsId[itemId], true);
                }

                foreach (KeyValuePair<int, int[]> item in items)
                {
                    int makeItemId = df.MakeNewItem(item.Key);
                    df.GetItem(df.mianActorId, makeItemId, 1, false, -1, 0);
                    df.itemsDate[makeItemId][901] = item.Value[0].ToString();
                    df.itemsDate[makeItemId][902] = item.Value[1].ToString();
                }
            }
        }
    }

    /*
    [HarmonyPatch(typeof(Loading), "LoadGameDateStart2")]
    public static class GetItemsKeys_For_Loading_LoadScene
    {
        static void Postfix()
        {
            if (Main.enabled)
            {
                List<string> keys = new List<string>(GetSprites.instance.baseGameDate["Item_Date"].Trim().Split(','));
                int indexTypeNum = keys.IndexOf("5");
                int indexId = keys.IndexOf("999");

                foreach (string items in GetSprites.instance.baseGameDate["Item_Date"].Trim().Split(new string[] { "\r\n" }, StringSplitOptions.None))
                {
                    string[] theItemParams = items.Trim().Split(',');
                    if (theItemParams[indexTypeNum] == "31") Main.itemsKey.Add(int.Parse(theItemParams[indexId]));
                }
            }
        }
    }
    */

    [HarmonyPatch(typeof(SaveDateFile), "LateUpdate")]
    public static class MergeItems_For_SaveDateFile_SaveSaveDate
    {
        static void Prefix(SaveDateFile __instance)
        {
            if (Main.enabled && Main.settings.autoMerge && __instance.saveSaveDate) Main.RunningMergeItems();
        }
    }
}
