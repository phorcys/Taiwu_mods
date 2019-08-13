using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using Litfal;

namespace SmartWear
{
    public static class Main
    {
        public static bool Enabled { get; private set; }
        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }
        public static Settings settings;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Logger = modEntry.Logger;
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            
            return true;
        }


        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginVertical("Box");
            GUILayoutHelper.Title("練功 <color=#A0A0A0>(修習、突破、研讀)</color>");
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("使用功法");
            settings.HomeSystemGongFaIndex =
                GUILayout.SelectionGrid(
                    settings.HomeSystemGongFaIndex + 1,
                    new string[] { "<color=#808080>不切換</color>", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" }, 10,
                    new GUILayoutOption[0]) - 1;
            GUILayout.EndHorizontal();
            settings.HomeSystemAutoAccessories = GUILayout.Toggle(settings.HomeSystemAutoAccessories, "自動裝備適合的飾品 (資質優先，悟性其次)");
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            GUILayoutHelper.Title("製造 <color=#A0A0A0>(锻造、制木、炼药、炼毒、织锦、制石、烹飪)</color>");
            settings.MakeSystemAutoAccessories = GUILayout.Toggle(settings.MakeSystemAutoAccessories, "自動裝備適合的飾品");
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            GUILayoutHelper.Title("醫療與解毒");
            settings.HealingAutoAccessories = GUILayout.Toggle(settings.HealingAutoAccessories, "自動裝備適合的飾品");
            GUILayout.EndVertical();
            GUILayout.Label("<color=#FF8080>※如果不在城鎮/門派格，將不會使用倉庫裡的裝備※</color>");
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }



    public class Settings : UnityModManager.ModSettings
    {
        public int HomeSystemGongFaIndex = -1;
        public bool HomeSystemAutoAccessories = true;
        public bool MakeSystemAutoAccessories = true;
        public bool HealingAutoAccessories = true;

        //public int GongFaIndexAtHomeSystem = -1;

        public override void Save(UnityModManager.ModEntry modEntry)
            => Save(this, modEntry);
    }


    public class AptitudeTypeHelper
    {
        static public int GetAptitudeTypeByBaseMakeType(int baseMakeType)
            => baseMakeType + 50500;

        static public int GetAptitudeTypeBySkillType(int skillType)
            => skillType + 50500;

        static public int GetAptitudeTypeByGongFaId(int gongFaId)
            => GetAptitudeTypeBySkillType(int.Parse(DateFile.instance.gongFaDate[gongFaId][61]));

        static public int GetAptitudeTypeByBookId(int bookId)
            => GetAptitudeTypeByGongFaId(int.Parse(DateFile.instance.GetItemDate(bookId, (int)ItemAttributeKey.BookSkillId, true)));
    }

    /// <summary>
    /// 管理 Item 從倉庫取出, 裝備 與 還原狀態的類別
    /// </summary>
    public class StateManager
    {
        // static Dictionary<int, int> _originEquitments = new Dictionary<int, int>();
        // static List<string> _originEquitments = new List<string>(3);
        static Dictionary<int, string> _originEquitments = new Dictionary<int, string>();
        static int _originGongFaIndex = -1;

        static public void EquipAccessories(int aptitudeType)
        {
            EquipAccessories(ItemHelper.GetAptitudeUpOrComprehensionUpAccessories(aptitudeType));
        }

        static public void EquipAccessories(IEnumerable<ItemData> accessories)
            => EquipAccessories(accessories.Take(3).ToArray()); // 接下來要調整資料, 故先ToArray() 固定 IEnumerable

        static public void EquipAccessories(ItemData[] accessories)
        {

            var df = DateFile.instance;
            var actorDate = df.actorsDate[df.mianActorId];
            // 紀錄原本的裝備
            if (_originEquitments.Count == 0)
            {
                _originEquitments.Add((int)ActorsDate.Accessory1, actorDate[(int)ActorsDate.Accessory1]);
                _originEquitments.Add((int)ActorsDate.Accessory2, actorDate[(int)ActorsDate.Accessory2]);
                _originEquitments.Add((int)ActorsDate.Accessory3, actorDate[(int)ActorsDate.Accessory3]);
            }
            // 因為只是暫時的, 裝起來就對了
            // 不用特別取出
            int actorsDateIndex = (int)ActorsDate.Accessory1;
            foreach (var item in accessories.Take(3))
            {
                actorDate[actorsDateIndex++] = item.Id.ToString();
#if (DEBUG)
                Main.Logger.Log($"Equip: Id:{item.Id} 資質:{item.AptitudeUp} 悟性:{item.ComprehensionUp}");
#endif
            }
        }

        static public void UseGongFa(int gongFaIndex)
        {
            if (gongFaIndex < 0) return;
            var currentGongFaIndex = DateFile.instance.mianActorEquipGongFaIndex;
            if (currentGongFaIndex == gongFaIndex) return;
#if (DEBUG)
            Main.Logger.Log($"UseGongFa:{gongFaIndex}");
#endif
            _originGongFaIndex = currentGongFaIndex;
            ActorMenu.instance.ChangeEquipGongFa(gongFaIndex);
        }

        static public void Restore()
        {
            var df = DateFile.instance;
            var actorDate = df.actorsDate[df.mianActorId];
            foreach (var kvp in _originEquitments)
            {
                actorDate[kvp.Key] = kvp.Value;
#if (DEBUG)
                Main.Logger.Log($"Restore Equip: Id:{kvp.Value}@{kvp.Key}");
#endif
            }
            _originEquitments.Clear();

            if (_originGongFaIndex != -1)
            {
                int current_GongFaIndex = df.mianActorEquipGongFaIndex;
                if (current_GongFaIndex != _originGongFaIndex)
                {
#if (DEBUG)
                    Main.Logger.Log($"Restore GongFa: Index:{_originGongFaIndex}");
#endif
                    ActorMenu.instance.ChangeEquipGongFa(_originGongFaIndex);
                }
                _originGongFaIndex = -1;
            }
        }
    }

    enum ItemAttributeKey : int
    {
        EquipType = 1,
        Comprehension = 50065,
        BookSkillId = 32,
    }

    enum EquipType : int
    {
        Accessory = 3,
    }

    public enum ItemSource
    {
        Bag,
        Equipment,
        Warehouse,
    }

    enum EquipSlot
    {
        Accessory1 = 7,
        Accessory2 = 8,
        Accessory3 = 9,
    }

    enum ActorsDate
    {
        Accessory1 = 307,
        Accessory2 = 308,
        Accessory3 = 309,
    }

    enum AptitudeType
    {
        // 醫術
        Treatment = 50509,
        // 毒術
        Poison = 50510,
    }

    public class ItemData
    {
        public int Id { get; private set; }
        public ItemSource ItemSource { get; set; }
        public int ComprehensionUp { get; internal set; }
        public int AptitudeUp { get; internal set; }
        public int AptitudeType { get; internal set; }
        public int EquipSlot { get; internal set; } = 0;

        public ItemData(int id, ItemSource itemSource)
        {
            Id = id;
            ItemSource = itemSource;
        }

    }


    public class ItemHelper
    {
        static public IEnumerable<ItemData> GetAptitudeUpOrComprehensionUpAccessories(int aptitudeType)
        {
            var items =
                GetEquipAptitudeUpAccessories(aptitudeType).Concat(
                GetBagAptitudeUpAccessories(aptitudeType)).Concat(
                GetWarehouseAptitudeUpAccessories(aptitudeType));

            // 排序
            // 暫時寫死, 資質高的優先, 其次是悟性
            return (from item in items
                    orderby item.AptitudeUp descending, item.ComprehensionUp descending
                    select item).Take(3);
        }


        /// <summary>
        /// 篩選可提升指定資質的物品
        /// </summary>
        /// <param name="items">接受篩選的物品ID</param>
        /// <param name="aptitudeType">資質種類, 505XX & 506XX </param>
        /// <returns></returns>
        static private IEnumerable<ItemData> Filter(IEnumerable<int> items, int aptitudeType, ItemSource itemSource)
        {
            return from item in items
                   where IsAccessory(item)
                   let comprehensionUp = GetComprehensionUp(item)
                   let aptitudeUp = GetAptitudeUp(item, aptitudeType)
                   where (comprehensionUp > 0 || aptitudeUp > 0)
                   select new ItemData(item, itemSource)
                   {
                       ComprehensionUp = comprehensionUp,
                       AptitudeUp = aptitudeUp,
                       AptitudeType = aptitudeType,
                   };
        }

        private static IEnumerable<ItemData> FilterInEquip(IEnumerable<KeyValuePair<int, int>> slotItemPairs, int aptitudeType, ItemSource itemSource)
        {
            return from kvp in slotItemPairs
                   let item = kvp.Value
                   where IsAccessory(item)
                   let comprehensionUp = GetComprehensionUp(item)
                   let aptitudeUp = GetAptitudeUp(item, aptitudeType)
                   where (comprehensionUp > 0 || aptitudeUp > 0)
                   select new ItemData(item, itemSource)
                   {
                       ComprehensionUp = comprehensionUp,
                       AptitudeUp = aptitudeUp,
                       AptitudeType = aptitudeType,
                       EquipSlot = kvp.Key,
                   };
        }

        /// <summary>
        /// 檢查是否為裝飾品
        /// </summary>
        /// <param name="itemId">物品唯一碼</param>
        /// <returns></returns>
        static private bool IsAccessory(int itemId)
        {
            return DateFile.instance.GetItemDate(itemId, (int)ItemAttributeKey.EquipType) == ((int)EquipType.Accessory).ToString();
        }

        /// <summary>
        /// 取得悟性上升
        /// </summary>
        /// <param name="itemId">物品唯一碼</param>
        /// <returns></returns>
        static private int GetComprehensionUp(int itemId)
        {
            if (int.TryParse(DateFile.instance.GetItemDate(itemId, (int)ItemAttributeKey.Comprehension), out int value))
                return value;
            return 0;
        }

        /// <summary>
        /// 取得提升指定資質的物品
        /// </summary>
        /// <param name="itemId">物品唯一碼</param>
        /// <param name="aptitudeType">資質碼</param>
        /// <returns></returns>
        static private int GetAptitudeUp(int itemId, int aptitudeType)
        {
            if (int.TryParse(DateFile.instance.GetItemDate(itemId, aptitudeType), out int value))
                return value;
            return 0;
        }


        /// <summary>
        /// 玩家身上的物品
        /// </summary>
        /// <returns></returns>
        static private IEnumerable<int> GetBagItems()
            => DateFile.instance.actorItemsDate[DateFile.instance.MianActorID()].Keys;

        /// <summary>
        /// 從玩家身上篩選可提升指定資質的物品
        /// </summary>
        /// <param name="aptitudeType">資質種類, 505XX & 506XX </param>
        /// <returns></returns>
        static public IEnumerable<ItemData> GetBagAptitudeUpAccessories(int aptitudeType)
            => Filter(GetBagItems(), aptitudeType, ItemSource.Bag);

        /// <summary>
        /// 玩家倉庫的物品 (不可訪問倉庫時則返回空)
        /// </summary>
        /// <returns></returns>
        static private IEnumerable<int> GetWarehouseItems()
        {
            var df = DateFile.instance;
            if (df.CanInToPlaceHome())
                return df.actorItemsDate[-999].Keys;
            else
                return Enumerable.Empty<int>(); // 不可訪問倉庫時則返回空
        }

        /// <summary>
        /// 從玩家倉庫篩選可提升指定資質的物品
        /// </summary>
        /// <param name="aptitudeType">資質種類, 505XX & 506XX </param>
        /// <returns></returns>
        static public IEnumerable<ItemData> GetWarehouseAptitudeUpAccessories(int aptitudeType)
            => Filter(GetWarehouseItems(), aptitudeType, ItemSource.Warehouse);

        ///// <summary>
        ///// 玩家裝備的物品
        ///// </summary>
        ///// <returns>slot/itemId pair</returns>

        static private IEnumerable<KeyValuePair<int, int>> GetEquipItems()
        {
            DateFile df = DateFile.instance;
            for (int i = (int)EquipSlot.Accessory1; i <= (int)EquipSlot.Accessory3; i++)
            {
                string itemIdStr = df.actorsDate[df.mianActorId][300 + i];
                if (itemIdStr != "0")
                {
                    int itemId = int.Parse(itemIdStr);
                    yield return new KeyValuePair<int, int>(300 + i, itemId);
                }
            }
        }

        /// <summary>
        /// 從玩家裝備篩選可提升指定資質的物品
        /// </summary>
        /// <param name="aptitudeType">資質種類, 505XX & 506XX </param>
        /// <returns></returns>
        static public IEnumerable<ItemData> GetEquipAptitudeUpAccessories(int aptitudeType)
        { 
            return FilterInEquip(GetEquipItems(), aptitudeType, ItemSource.Equipment);
        }


    }
}
