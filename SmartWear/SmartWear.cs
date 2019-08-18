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
using System.Linq.Expressions;

namespace SmartWear
{
    public static class Main
    {
        public static bool Enabled { get; private set; }
        public static ThreadSafeLogger Logger { get; private set; }
        public static Settings settings;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Logger = new ThreadSafeLogger(modEntry.Logger);
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
            // GUILayoutHelper.Title("練功 <color=#A0A0A0>(修習、突破、研讀)</color>");
            GUILayoutHelper.GongFaSelection("<color=#80FF80>練功</color>時使用功法", ref settings.HomeSystemGongFaIndex);
            settings.HomeSystemAutoAccessories = GUILayout.Toggle(settings.HomeSystemAutoAccessories, "<color=#80FF80>練功</color>時自動裝備適合的飾品 (資質優先，悟性其次)");
            settings.AdvancedReadBookMode = GUILayout.Toggle(settings.AdvancedReadBookMode, "進階研讀模式：難度超過 50% 則資質優先、悟性其次；否則悟性優先");
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            // GUILayoutHelper.Title("製造 <color=#A0A0A0>(锻造、制木、炼药、炼毒、织锦、制石、烹飪)</color>");
            settings.MakeSystemAutoAccessories = GUILayout.Toggle(settings.MakeSystemAutoAccessories, "<color=#80FF80>製造</color>時自動裝備適合的飾品");
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            settings.HealingAutoAccessories = GUILayout.Toggle(settings.HealingAutoAccessories, "<color=#80FF80>療傷</color>與<color=#80FF80>驅毒</color>自動裝備適合的飾品 <color=#FF8080>※如果不在城鎮/門派格，將不會使用倉庫裡的裝備※</color>");
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            GUILayoutHelper.GongFaSelection("跨月恢復<color=#80FF80>內息</color>時使用功法", ref settings.RestGongFaIndex);
            settings.RestAutoEquip = GUILayout.Toggle(settings.RestAutoEquip, "跨月恢復<color=#80FF80>內息</color>時自動裝備適合的武器 (內息優先) <color=#FF8080>※如果不在城鎮/門派格，將不會使用倉庫裡的裝備※</color>");
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            GUILayoutHelper.GongFaSelection("進入<color=#80FF80>戰鬥</color>準備畫面時，使用指定功法", ref settings.StartBattleGongFaIndex);
            GUILayoutHelper.EquipGroupSelection("進入<color=#80FF80>戰鬥</color>準備畫面時，使用指定裝備", ref settings.StartBattleEquipGroupIndex);
            GUILayout.Label("<color=#FF8080>※戰鬥後不會換回※</color>");
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            GUILayoutHelper.EquipGroupSelection("進入<color=#80FF80>較藝</color>準備畫面時，使用指定裝備", ref settings.StartSkillBattleEquipGroupIndex);
            GUILayout.Label("<color=#FF8080>※較藝後不會換回※</color>");
            GUILayout.EndVertical();
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
        public int RestGongFaIndex = -1;
        public bool RestAutoEquip = true;
        public bool AdvancedReadBookMode = true;
        public int StartBattleGongFaIndex = -1;
        public int StartBattleEquipGroupIndex = -1;
        public int StartSkillBattleEquipGroupIndex = -1;

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
            => GetAptitudeTypeByGongFaId(int.Parse(DateFile.instance.GetItemDate(bookId, (int)ItemDateKey.BookSkillId, true)));
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
            RestoreEquip();
            var items = ItemHelper.GetAptitudeUpOrComprehensionUpAccessories(aptitudeType);
            // 暫時寫死, 資質高的優先, 其次是悟性
            EquipAccessories(from item in items
                             orderby item.AptitudeUp descending, item.ComprehensionUp descending
                             select item);
        }

        static public void EquipAccessories(IEnumerable<ItemData> accessories)
            => EquipAccessories(accessories.Take(3).ToArray()); // 接下來要調整資料, 故先ToArray() 固定 IEnumerable

        static public void EquipAccessories(ItemData[] accessories)
        {
            Equip(accessories.Select(a => a.Id).ToArray(), EquipSlot.Accessory1);
#if (DEBUG)
            foreach (var item in accessories.Take(3))
            {
                Main.Logger.Log($"Equip: {item.Id}, {((ItemDateKey)item.AptitudeType).GetDescription()}: {item.AptitudeUp}, 悟性: {item.ComprehensionUp}");
            }
#endif
        }

        static public void UseGongFa(int gongFaIndex)
        {
            if (gongFaIndex < 0) return;
            var currentGongFaIndex = DateFile.instance.mianActorEquipGongFaIndex;
            if (currentGongFaIndex == gongFaIndex) return;
//#if (DEBUG)
//            Main.Logger.Log($"UseGongFa:{gongFaIndex}");
//#endif
            _originGongFaIndex = currentGongFaIndex;
            ActorMenu.instance.ChangeEquipGongFa(gongFaIndex);
        }

        static public void RestoreAll()
        {
            RestoreEquip();
            RestoreGongFa();
        }

        public static void RestoreEquip()
        {
            var df = DateFile.instance;
            var actorDate = df.actorsDate[df.mianActorId];
            foreach (var kvp in _originEquitments)
            {
                actorDate[kvp.Key] = kvp.Value;
//#if (DEBUG)
//                Main.Logger.Log($"Restore Equip: Id:{kvp.Value}@{kvp.Key}");
//#endif
            }
            _originEquitments.Clear();
        }

        public static void RestoreGongFa()
        {
            if (_originGongFaIndex != -1)
            {
                int current_GongFaIndex = DateFile.instance.mianActorEquipGongFaIndex;
                if (current_GongFaIndex != _originGongFaIndex)
                {
                    ActorMenu.instance.ChangeEquipGongFa(_originGongFaIndex);
                }
                _originGongFaIndex = -1;
            }
        }

        public static void EquipWeapons(IEnumerable<int> items)
            => EquipWeapons(items.Take(3).ToArray());

        public static void EquipWeapons(int[] items)
        {
            Equip(items, EquipSlot.Weapon1);
        }


        static public void Equip(int[] items, EquipSlot startSlot)
        {
            var df = DateFile.instance;
            var actorDate = df.actorsDate[df.mianActorId];
            var actorsDateKey = (int)startSlot.ToActorsDateKey();
            // 紀錄原本的裝備
            if (_originEquitments.Count == 0)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    int key = (int)(actorsDateKey + i);
                    _originEquitments.Add(key, actorDate[key]);
                }
            }
            // 因為只是暫時的, 裝起來就對了
            // 不用特別取出
            foreach (var item in items)
            {
                actorDate[actorsDateKey++] = item.ToString();
//#if (DEBUG)
//                Main.Logger.Log($"Equip:{item}");
//#endif
            }
        }
    }




    public enum ItemSource
    {
        Bag,
        Equipment,
        Warehouse,
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
        

        //static public IQueryable<int> ApplyFilter(IQueryable<int> items)
        //{

        //}

        static public IEnumerable<ItemData> GetAptitudeUpOrComprehensionUpAccessories(int aptitudeType)
        {
            var items =
                GetEquipAptitudeUpAccessories(aptitudeType).Concat(
                GetBagAptitudeUpAccessories(aptitudeType)).Concat(
                GetWarehouseAptitudeUpAccessories(aptitudeType));
            return items;
        }


        /// <summary>
        /// 依裝備類型篩選
        /// </summary>
        /// <param name="items">接受篩選的物品ID</param>
        /// <param name="equipType">裝備類型</param>
        /// <returns></returns>
        static private IQueryable<int> FilterByEquipType(IQueryable<int> items, EquipType equipType)
        {
            return items.Where(GetEquipTypeFilter(equipType));
        }

        static public Expression<Func<int, bool>> GetHasDataFilter(ItemDateKey itemDateKey)
        {
            return (item) => DateFile.instance.GetItemDateValue(item, itemDateKey) > 0;
        }

        static public Expression<Func<int, int>> GetOrderByData(ItemDateKey itemDateKey)
        {
            return (item) => DateFile.instance.GetItemDateValue(item, itemDateKey);
        }

        /// <summary>
        /// 篩選所有裝備
        /// </summary>
        /// <param name="items">接受篩選的物品ID</param>
        /// <returns></returns>
        static private IQueryable<int> FilterEquip(IQueryable<int> items)
        {
            return items.Where(GetIsEquipFilter());
        }

        static public Expression<Func<int, bool>> GetIsEquipFilter()
        {
            return (item) => DateFile.instance.GetItemDateValue(item, ItemDateKey.EquipType) > 0;
        }

        static public Expression<Func<int, bool>> GetEquipTypeFilter(EquipType equipType)
        {
            return (item) => DateFile.instance.GetItemDateValue(item, ItemDateKey.EquipType) == (int)equipType;
        }


        /// <summary>
        /// 篩選可提升指定資質的物品
        /// </summary>
        /// <param name="items">接受篩選的物品ID</param>
        /// <param name="aptitudeType">資質種類, 505XX & 506XX </param>
        /// <returns></returns>
        static private IQueryable<ItemData> Filter(IQueryable<int> items, int aptitudeType, ItemSource itemSource)
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

        private static IQueryable<ItemData> FilterInEquip(IEnumerable<KeyValuePair<int, int>> slotItemPairs, int aptitudeType, ItemSource itemSource)
        {
            return from kvp in slotItemPairs.AsQueryable()
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
            return DateFile.instance.GetItemDateValue(itemId, ItemDateKey.EquipType) == (int)EquipType.Accessory;
        }

        /// <summary>
        /// 取得悟性上升
        /// </summary>
        /// <param name="itemId">物品唯一碼</param>
        /// <returns></returns>
        static private int GetComprehensionUp(int itemId)
        {
            return DateFile.instance.GetItemDateValue(itemId, ItemDateKey.Comprehension);
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
        static public IEnumerable<int> GetBagItems()
            => DateFile.instance.actorItemsDate[DateFile.instance.MianActorID()].Keys;

        /// <summary>
        /// 從玩家身上篩選可提升指定資質的物品
        /// </summary>
        /// <param name="aptitudeType">資質種類, 505XX & 506XX </param>
        /// <returns></returns>
        static public IQueryable<ItemData> GetBagAptitudeUpAccessories(int aptitudeType)
            => Filter(GetBagItems().AsQueryable(), aptitudeType, ItemSource.Bag);

        /// <summary>
        /// 玩家倉庫的物品 (不可訪問倉庫時則返回空)
        /// </summary>
        /// <returns></returns>
        static public IEnumerable<int> GetWarehouseItems()
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
        static public IQueryable<ItemData> GetWarehouseAptitudeUpAccessories(int aptitudeType)
            => Filter(GetWarehouseItems().AsQueryable(), aptitudeType, ItemSource.Warehouse);

        ///// <summary>
        ///// 玩家裝備的物品
        ///// </summary>
        ///// <returns>slot/itemId pair</returns>

        static public IEnumerable<KeyValuePair<int, int>> GetEquipItems()
        {
            var df = DateFile.instance;
            var playerActorData = df.actorsDate[df.mianActorId];
            for (int i = (int)ActorsDateKey.Weapon1; i <= (int)ActorsDateKey.Accessory3; i++)
            {
                string itemIdStr = playerActorData[i];
                if (itemIdStr != "0")
                {
                    int itemId = int.Parse(itemIdStr);
                    yield return new KeyValuePair<int, int>(i, itemId);
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
