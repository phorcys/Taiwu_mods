using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        static bool Prefix()
        {
            if (!Main.Enabled)
                return true;
            if (!BookSetting.Instance.Open)
                BookSetting.Instance.ToggleWindow();
            SetBookData();
            return false;
        }

        /// <summary>
        /// 加载书籍数据
        /// </summary>
        public static void SetBookData()
        {
            var list = GetBooks();
            HomeSystem_SetChooseBookWindow_Patch.bookView.Data = list.Where(CheckBook).ToArray();
        }

        /// <summary>
        /// 获取所有可用书籍
        /// </summary>
        private static List<int> GetBooks()
        {
            var actorItems =
                from item in ActorMenu.instance.GetActorItems(DateFile.instance.mianActorId, 0).Keys
                where int.Parse(DateFile.instance.GetItemDate(item, 31, true)) == HomeSystem.instance.studySkillTyp
                select item;
            var warehouseItems =
                from item in ActorMenu.instance.GetActorItems(-999, 0).Keys
                where int.Parse(DateFile.instance.GetItemDate(item, 31, true)) == HomeSystem.instance.studySkillTyp
                select item;
            var items = actorItems.Concat(warehouseItems);
            return DateFile.instance.GetItemSort(items.ToList());
        }

        /// <summary>
        /// 检查物品id是否满足筛选条件
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        private static bool CheckBook(int itemId)
        {
            var df = DateFile.instance;
            // 背包
            if (!Main.Setting.repo[0] && df.actorItemsDate[df.mianActorId].ContainsKey(itemId))
            {
                return false;
            }
            // 仓库
            if (!Main.Setting.repo[1] && df.actorItemsDate[-999].ContainsKey(itemId))
            {
                return false;
            }
            int itemType = int.Parse(df.GetItemDate(itemId, 999));
            // Main.Logger.Log($"类型: {itemType}");
            // 技艺书籍
            if (itemType < 500000)
                return true;
            int bookId = int.Parse(df.GetItemDate(itemId, 32));
            // 品级
            int pinji = int.Parse(df.gongFaDate[bookId][2]) - 1;
            // Main.Logger.Log($"品级: {pinji}");
            if (!Main.Setting.pinji[pinji])
                return false;
            // 真传 or 手抄
            if (itemType < 700000 && !Main.Setting.tof[0])
                return false;
            if (itemType >= 700000 && !Main.Setting.tof[1])
                return false;
            // 功法类型
            int gongfa = int.Parse(df.gongFaDate[bookId][1]);
            // Main.Logger.Log($"功法: {gongfa}");
            if (!Main.Setting.gongfa[gongfa])
                return false;
            // 帮派
            int gang = int.Parse(df.gongFaDate[bookId][3]);
            // Main.Logger.Log($"帮派: {gang}");
            if (!Main.Setting.gang[gang])
                return false;
            // 阅读
            int pages = df.gongFaBookPages.ContainsKey(bookId) ? df.gongFaBookPages[bookId].Sum() : 0; // 阅读总页数
            int read = pages <= 0? 0 : (pages < 10 ? 1: 2);
            if (!Main.Setting.read[read])
                return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(WorldMapSystem), "Start")]
    public class WorldMapSystem_Start_Patch
    {
        static void Postfix()
        {
            HomeSystem_SetChooseBookWindow_Patch.hasInit = false;

            if (BookSetting.Instance == null)
            {
                BookSetting.Load();
            }
        }
    }

    [HarmonyPatch(typeof(HomeSystem), "SetChooseBookWindow")]
    public class HomeSystem_SetChooseBookWindow_Patch
    {
        public static bool hasInit;
        public static NewBookView bookView;

        static void Prefix(HomeSystem __instance)
        {
            var bookViewBack = HomeSystem.instance.bookHolder.parent.gameObject;
            if (bookViewBack.GetComponent<NewBookView>() == null)
            {
                bookView = bookViewBack.AddComponent<NewBookView>();
            }
            if (!hasInit)
            {
                hasInit = true;
                bookView.Init();
            }
            // HomeSystem.instance.bookHolder.parent.parent.gameObject.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(HomeSystem), "CloseBookWindow")]
    public class HomeSystem_CloseBookWindow_Patch
    {
        static void Prefix()
        {
            if (BookSetting.Instance.Open)
                BookSetting.Instance.ToggleWindow();
        }
    }

    /// <summary>
    /// 仓库中的书耐久为0时将其移除
    /// </summary>
    [HarmonyPatch(typeof(ReadBook), "CloseReadBook")]
    public static class ReadBook_CloseReadBook_Patch
    {
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
        public MyDict read = new MyDict();
        internal float scrollSpeed = 30;

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
            for (int i = 0; i < Main.read.Length; i++)
            {
                if (!read.ContainsKey(i))
                    read[i] = true;
            }
        }
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    /// <summary>
    /// 可序列化的Dictionary[int, bool]
    /// </summary>
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
                var key = int.Parse(reader.GetAttribute("key"));
                reader.ReadStartElement("Pair");
                var val = reader.ReadContentAsBoolean();
                reader.ReadEndElement();
                this[key] = val;
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (var pair in this)
            {
                writer.WriteStartElement("Pair");
                writer.WriteAttributeString("key", pair.Key.ToString());
                writer.WriteValue(pair.Value);
                writer.WriteEndElement();
            }
        }
    }

    public class Main
    {
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Enabled { get; private set; }
        public static Settings Setting { get; private set; }

        public static string[] gang = { "其他", "少林", "峨眉", "百花", "武当", "元山", "狮相", "然山", "璇女", "铸剑", "空桑", "金刚", "五仙", "界青", "伏龙", "血吼" };

        public static string[] gongfa = { "内功", "轻功", "绝技", "拳掌", "指法", "腿法", "暗器", "剑法", "刀法", "长兵", "奇门", "软兵", "御射", "乐器" };

        public static string[] pinji = { "九品", "八品", "七品", "六品", "五品", "四品", "三品", "二品", "一品" };

        public static string[] tof = { "真传", "手抄" };

        public static string[] repo = { "背包", "仓库" };

        public static string[] read = { "未读", "已读", "读完" };

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
            ShowSetting("背包/仓库", Setting.repo, repo);
            ShowSetting("真传/手抄", Setting.tof, tof);
            ShowSetting("品级", Setting.pinji, pinji);
            ShowSetting("功法", Setting.gongfa, gongfa);
            ShowSetting("帮派", Setting.gang, gang);
            ShowSetting("阅读进度", Setting.read, read);
            GUILayout.EndVertical();
        }

        public static void ShowSetting(string label, Dictionary<int, bool> dict, string[] setting)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(100));
            if (GUILayout.Button("全部", GUILayout.Width(50)))
            {
                var all = dict.All((pair) => pair.Value);
                for (int i = 0; i < setting.Length; i++)
                {
                    dict[i] = !all;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            for (var i = 0; i < setting.Length; i++)
            {
                dict[i] = GUILayout.Toggle(dict[i], setting[i], GUILayout.Width(50));
            }
            GUILayout.EndHorizontal();
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Setting.Save(modEntry);
        }
    }
}
