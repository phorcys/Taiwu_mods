using Harmony12;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace Sth4nothing.UseStorageBook
{
    [HarmonyPatch(typeof(HomeSystem), "SetBook")]
    public static class HomeSystem_SetBook_Patch
    {
        /// <summary>
        /// 将HomeSystem.SetBook 修改为 NewSetBook
        /// </summary>
        /// <returns></returns>
        static bool Prefix()
        {
            if (!Main.Enabled)
                return true;
            NewSetBook();
            return false;
        }

        /// <summary>
        /// 在加载主角背包中的书同时加载仓库中的书
        /// </summary>
        public static void NewSetBook()
        {
            var RemoveBook = typeof(HomeSystem).GetMethod("RemoveBook", BindingFlags.NonPublic | BindingFlags.Instance);
            RemoveBook.Invoke(HomeSystem.instance, null);
            List<int> list = new List<int>();
            if (!Main.Enabled || Main.Setting.repo[0])
            {
                list.AddRange(ActorMenu.instance.GetActorItems(DateFile.instance.mianActorId, 0).Keys);
            }
            if (Main.Enabled && Main.Setting.repo[1])
            {
                list.AddRange(ActorMenu.instance.GetActorItems(-999, 0).Keys);
            }
            list = DateFile.instance.GetItemSort(list);
            for (int i = 0; i < list.Count; i++)
            {
                int num = list[i];

                if (int.Parse(DateFile.instance.GetItemDate(num, 31, true)) == HomeSystem.instance.studySkillTyp)
                {
                    if (!CheckBook(num))
                        continue;

                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(HomeSystem.instance.bookIcon, Vector3.zero, Quaternion.identity);
                    gameObject.name = "Item," + num;
                    gameObject.transform.SetParent(HomeSystem.instance.bookHolder, false);
                    gameObject.GetComponent<Toggle>().group = HomeSystem.instance.bookHolder.GetComponent<ToggleGroup>();
                    Image component = gameObject.transform.Find("ItemBack").GetComponent<Image>();
                    component.sprite = GetSprites.instance.itemBackSprites[int.Parse(DateFile.instance.GetItemDate(num, 4, true))];
                    component.color = ActorMenu.instance.LevelColor(int.Parse(DateFile.instance.GetItemDate(num, 8, true)));
                    GameObject gameObject2 = gameObject.transform.Find("ItemIcon").gameObject;
                    gameObject2.name = "ItemIcon," + num;
                    gameObject2.GetComponent<Image>().sprite = GetSprites.instance.itemSprites[int.Parse(DateFile.instance.GetItemDate(num, 98, true))];
                    int num2 = int.Parse(DateFile.instance.GetItemDate(num, 901, true));
                    int num3 = int.Parse(DateFile.instance.GetItemDate(num, 902, true));
                    gameObject.transform.Find("ItemHpText").GetComponent<Text>().text = string.Format("{0}{1}</color>/{2}", ActorMenu.instance.Color3(num2, num3), num2, num3);
                    int[] bookPage = DateFile.instance.GetBookPage(num);
                    Transform transform = gameObject.transform.Find("PageBack");
                    for (int j = 0; j < transform.childCount; j++)
                    {
                        if (bookPage[j] == 1)
                        {
                            transform.GetChild(j).GetComponent<Image>().color = new Color(0.392156869f, 0.784313738f, 0f, 1f);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 检查物品id是否满足条件
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        private static bool CheckBook(int itemId)
        {
            var df = DateFile.instance;
            int itemType = int.Parse(df.GetItemDate(itemId, 999));
            // Main.Logger.Log($"类型: {itemType}");
            // 品级
            int pinji = int.Parse(df.GetItemDate(itemId, 8, false));
            // Main.Logger.Log($"品级: {pinji}");
            if (!Main.Setting.pinji[pinji])
                return false;
            // 真传 or 手抄
            if (itemType < 700000 && !Main.Setting.tof[0])
                return false;
            if (itemType >= 700000 && !Main.Setting.tof[1])
                return false;
            if (itemType > 500000)
            {
                // 功法类型
                int gongfa = itemType / 10000;
                if (gongfa < 70)
                    gongfa -= 50;
                else
                    gongfa -= 70;
                // Main.Logger.Log($"功法: {gongfa}");
                if (!Main.Setting.gongfa[gongfa])
                    return false;
                // 帮派
                int gang = itemType / 100 % 100 - 1;
                // Main.Logger.Log($"帮派: {gang}");
                if (!Main.Setting.gang[gang])
                    return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ReadBook), "CloseReadBook")]
    public static class ReadBook_CloseReadBook_Patch
    {
        /// <summary>
        /// 仓库中的书耐久为0时将其移除
        /// </summary>
        static void Prefix()
        {
            var df = DateFile.instance;
            var bookId = HomeSystem.instance.readBookId;
            if (df.itemsDate.ContainsKey(bookId))
            {
                if (df.actorItemsDate[-999].ContainsKey(bookId))
                {
                    var hp = int.Parse(df.itemsDate[bookId][901]);
                    if (hp <= 1)
                    {
                        df.actorItemsDate[-999].Remove(bookId);
                        df.actorItemsDate[df.mianActorId].Add(bookId, 1);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 解决鼠标放在书本上不显示仓库中书上时，不显示仓库中书的阅读状态的BUG
    /// （暂时将书加入背包）
    /// </summary>
    [HarmonyPatch(typeof(WindowManage), "ShowBookMassage")]
    public static class WindowsManage_ShowBookMassage_Patch
    {
        /// <summary>
        /// 记录当前书的id，并将仓库中的书暂时加入背包
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="__state"></param>
        static void Prefix(ref int itemId, ref int __state)
        {
            if (DateFile.instance.actorItemsDate[-999].ContainsKey(itemId))
            {
                DateFile.instance.actorItemsDate[DateFile.instance.MianActorID()].Add(itemId, 1);
                __state = itemId;
            }
            else
            {
                __state = -1;
            }
        }

        /// <summary>
        /// 将书从背包中移除
        /// </summary>
        /// <param name="__state"></param>
        static void Postfix(ref int __state)
        {
            if (__state > 0)
            {
                DateFile.instance.actorItemsDate[DateFile.instance.MianActorID()].Remove(__state);
            }
        }
    }

    public class Settings : UnityModManager.ModSettings
    {
        public MyDict gang = new MyDict();
        public MyDict gongfa = new MyDict();
        public MyDict pinji = new MyDict();
        public MyDict tof = new MyDict();
        public MyDict repo = new MyDict();

        public void Init()
        {
            for (int i = 0; i < Main.gang.Length; i++)
            {
                if (!gang.ContainsKey(i))
                    gang[i] = true;
            }
            for (int i = 0; i < Main.gongfa.Length; i++)
            {
                if (!gongfa.ContainsKey(i))
                    gongfa[i] = true;
            }
            for (int i = 0; i < Main.pinji.Length; i++)
            {
                if (!pinji.ContainsKey(i))
                    pinji[i] = true;
            }
            for (int i = 0; i < Main.tof.Length; i++)
            {
                if (!tof.ContainsKey(i))
                    tof[i] = true;
            }
            for (int i = 0; i < Main.repo.Length; i++)
            {
                if (!repo.ContainsKey(i))
                    repo[i] = true;
            }
        }
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
    public class MyDict : Dictionary<int, bool>, IXmlSerializable
    {
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.IsEmptyElement)
                return;

            reader.Read();
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                Debug.Assert(reader.Name == "Pair");
                reader.MoveToAttribute("key");
                var key = int.Parse(reader.Value);
                reader.MoveToContent();
                var val = bool.Parse(reader.Value);
                this[key] = val;
                reader.Read();
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (int key in this.Keys)
            {
                writer.WriteStartElement("Pair");
                writer.WriteAttributeString("key", key.ToString());
                writer.WriteString(this[key].ToString());
                writer.WriteEndElement();
            }
        }
    }

    public class Main
    {
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Enabled { get; private set; }
        public static Settings Setting { get; private set; }

        public static string[] gang = { "少林", "峨眉", "百花", "武当", "元山", "狮相", "然山", "璇女", "铸剑", "空桑", "金刚", "五仙", "界青", "伏龙", "血吼", "其他" };

        public static string[] gongfa = { "内功", "轻功", "绝技", "拳掌", "指法", "腿法", "暗器", "剑法", "刀法", "长兵", "奇门", "软兵", "御射", "乐器" };

        public static string[] pinji = { "九品", "八品", "七品", "六品", "五品", "四品", "三品", "二品", "一品" };

        public static string[] tof = { "真传", "手抄" };

        public static string[] repo = { "背包", "仓库" };

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            Setting = Settings.Load<Settings>(modEntry);
            Setting.Init();

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            for (int i = 0; i < repo.Length; i++)
            {
                Setting.repo[i] = GUILayout.Toggle(Setting.repo[i], repo[i], GUILayout.Width(50));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            for (int i = 0; i < tof.Length; i++)
            {
                Setting.tof[i] = GUILayout.Toggle(Setting.tof[i], tof[i], GUILayout.Width(50));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            for (int i = 0; i < pinji.Length; i++)
            {
                Setting.pinji[i] = GUILayout.Toggle(Setting.pinji[i], pinji[i], GUILayout.Width(50));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            for (int i = 0; i < gongfa.Length; i++)
            {
                Setting.gongfa[i] = GUILayout.Toggle(Setting.gongfa[i], gongfa[i], GUILayout.Width(50));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            for (var i = 0; i < gang.Length; i++)
            {
                Setting.gang[i] = GUILayout.Toggle(Setting.gang[i], gang[i], GUILayout.Width(50));
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Setting.Save(modEntry);
        }
    }
}
