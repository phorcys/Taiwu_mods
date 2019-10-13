using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Litfal
{

    static public class ControlHelper
    {
        static CToggleGroup GetEquipToggleGroup()
        {
            var actorEquipGroups = Resources.FindObjectsOfTypeAll<ActorEquipGroup>();
            var actorEquipGroup = actorEquipGroups?.FirstOrDefault();
            if (actorEquipGroup == null) return null;
            return (CToggleGroup)typeof(ActorEquipGroup).GetField("toggleGroup", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                .GetValue(actorEquipGroup);
        }

        /// <summary>
        /// 改變使用的裝備集合
        /// </summary>
        /// <param name="index">0~2</param>
        public static void ChangeEquipGroup(int index)
        {
            if (index < 0 || index >= 3) throw new ArgumentOutOfRangeException("index");
            DateFile df = DateFile.instance;
            if (df.nowEquipConfigIndex == index) return;

            var equipToggleGroup = GetEquipToggleGroup();
            if (equipToggleGroup != null)
            {
                // Main.Logger.Debug("ChangeEquipGroup By UI");
                // 使用 現有的UI做切換, 避免UI顯示不正常
                equipToggleGroup.Set(index);
            }
            else
            {
                // UI尚未建立, 使用程式碼做切換
                if (df.mainActorequipConfig == null) return;
                int mainActorId = df.MianActorID();
                Dictionary<int, int> currentEquipConfig = df.mainActorequipConfig[df.nowEquipConfigIndex];
                var playerId = df.mianActorId;
                // Dictionary<int, string> playerActorsData = df.actorsDate[mainActorId];

                // 脫下現在的裝備, 移動到身上(行李)
                foreach (KeyValuePair<int, int> slotItemPair in currentEquipConfig)
                {
                    int itemId = slotItemPair.Value;
                    int slot = slotItemPair.Key;
                    if (itemId != 0 &&
                        itemId.ToString() == GameData.Characters.GetCharProperty(playerId, slotItemPair.Key))
                    {
                        df.GetItem(mainActorId, itemId, 1, false, -1, 0);
                        GameData.Characters.SetCharProperty(playerId, slot, "0");
                    }
                }

                // 裝備 + 從行李內移除物件
                Dictionary<int, int> newEquipGroup = df.mainActorequipConfig[index];
                foreach (KeyValuePair<int, int> slotItemPair in newEquipGroup)
                {
                    int itemId = slotItemPair.Value;
                    int slot = slotItemPair.Key;
                    if (itemId != 0 &&
                        df.HasItem(mainActorId, itemId))
                    {
                        df.LoseItem(mainActorId, itemId, 1, false, false);
                        GameData.Characters.SetCharProperty(playerId, slot, itemId.ToString());

                    }
                }
                df.nowEquipConfigIndex = index;
            }
        }

        /// <summary>
        /// 改變使用的功法
        /// </summary>
        /// <param name="index">0~8</param>
        public static void ChangeGongFa(int index)
        {
            if (index < 0 || index >= 9) throw new ArgumentOutOfRangeException("index");

            var currnetGongFa = DateFile.instance.mianActorEquipGongFaIndex;
            if (currnetGongFa == index) return;
            // 先關, 再開, UI顯示才會正常
            ActorMenu.instance.equipGongFaTypToggle[currnetGongFa].isOn = false;
            ActorMenu.instance.equipGongFaTypToggle[index].isOn = true; 
            // 直接使用 ActorMenu.instance.ChangeEquipGongFa(index) 會造成功法介面顯示不正常
        }

        /// <summary>
        /// 暫時改變使用的功法
        /// 不處理UI與事件, 避免跨執行續控制UI
        /// 但記得要換回來, 否則UI會顯示dirty data (顯示和實際資料不同步)
        /// </summary>
        /// <param name="index"></param>
        public static void ChangeGongFaTemporarily(int index)
        {
            if (index < 0 || index >= 9) throw new ArgumentOutOfRangeException("index");

            var currnetGongFa = DateFile.instance.mianActorEquipGongFaIndex;
            if (currnetGongFa == index) return;
            DateFile.instance.mianActorEquipGongFaIndex = index;
            DateFile.instance.GetMianActorEquipGongFa(index);
        }


        /// <summary>
        /// toActorId = mianActorId 代表脫下裝備並放到包裹裡
        /// toActorId = -999 代表脫下裝備並放到倉庫裡
        /// 當然也可以指定其他的actorId, 不過可能會有不合理的情況
        /// </summary>
        /// <param name="slotKey"></param>
        /// <param name="toActorId"></param>
        private static void TakeoffEquipTo(int slotKey, int toActorId)
        {
            if (slotKey < 301 || slotKey > 312)
                throw new ArgumentOutOfRangeException($"slotKey must be between 301~312, value={slotKey}");
            var df = DateFile.instance;
            var itemId = int.Parse(GameData.Characters.GetCharProperty(df.mianActorId, slotKey));
            GetItemFromTakeoffEquip(toActorId, itemId);
            GameData.Characters.SetCharProperty(df.mianActorId, slotKey, "0");
        }

        /// <summary>
        /// 脫下裝備, 並將物品放到包裹裡
        /// </summary>
        /// <param name="slotKey"></param>
        public static void TakeoffEquipToBag(int slotKey)
            => TakeoffEquipTo(slotKey, DateFile.instance.mianActorId);

        /// <summary>
        /// 脫下裝備, 並將物品放到倉庫裡
        /// </summary>
        /// <param name="slotKey"></param>
        public static void TakeoffEquipToWarehouse(int slotKey)
            => TakeoffEquipTo(slotKey, -999);

        /// <summary>
        /// 脫下裝備後, 將裝備放入包裹的函數
        /// 由於直接用 GetItem 會觸發 GEvent.OnEvent
        /// 會導致非主執行續控制UI錯誤
        /// 這個函數擷取自 DateFile.GetItem() 
        /// 直接針對裝備類, 控制 actorItemsDate
        /// 避免後續呼叫 GEvent.OnEvent
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="itemId"></param>
        public static void GetItemFromTakeoffEquip(int actorId, int itemId)
        {
            /*
            // 擷取自
            bool flag3 = int.Parse(this.GetItemDate(itemId, 6, true)) == 0;  // 是否可堆疊
			if (flag3)
			{
				bool flag4 = !this.actorItemsDate[actorId].ContainsKey(itemId); // 檢查相同 裝備Id 是否已在身上, 不在才加入
				if (flag4)
				{
					this.actorItemsDate[actorId].Add(itemId, 1);                
				}
			}
             */
            // 因為裝備必定不可堆疊, 不需要檢查可否堆疊, 
            // 也不需要檢查是否已在身上
            // 反正在也是數量1, 不在也是變成數量1 
            DateFile.instance.actorItemsDate[actorId][itemId] = 1;
        }

        /// <summary>
        /// fromActorId = mianActorId 代表從包裹拿出裝備並穿上
        /// fromActorId = -999 代表從倉庫拿出裝備並穿上
        /// 當然也可以指定其他的actorId, 不過可能會有不合理的情況
        /// </summary>
        /// <param name="slotKey"></param>
        /// <param name="itemId"></param>
        /// <param name="fromActorId"></param>
        /// <returns></returns>
        public static bool WearEquipFrom(int slotKey, int itemId, int fromActorId)
        {
            if (slotKey < 301 || slotKey > 312)
                throw new ArgumentOutOfRangeException($"slotKey must be between 301~312, value={slotKey}");
            var df = DateFile.instance;
            if (!df.HasItem(fromActorId, itemId))
                return false;
            LostItemToWearEquip(fromActorId, itemId);
            GameData.Characters.SetCharProperty(df.mianActorId, slotKey, itemId.ToString());
            return true;
        }

        /// <summary>
        /// 從包裹拿出裝備並穿上
        /// </summary>
        /// <param name="slotKey"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public static bool WearEquipFromBag(int slotKey, int itemId)
            => WearEquipFrom(slotKey, itemId, DateFile.instance.mianActorId);

        /// <summary>
        /// 從倉庫拿出裝備並穿上
        /// </summary>
        /// <param name="slotKey"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public static bool WearEquipFromWarehouse(int slotKey, int itemId)
            => WearEquipFrom(slotKey, itemId, -999);

        /// <summary>
        /// 穿上裝備, 由包裹移除裝備的函數
        /// 由於直接用 LoseItem 會觸發 GEvent.OnEvent
        /// 會導致非主執行續控制UI錯誤
        /// 這個函數擷取自 DateFile.LoseItem() 
        /// 直接針對裝備類, 控制 actorItemsDate
        /// 避免後續呼叫 GEvent.OnEvent
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="itemId"></param>
        public static void LostItemToWearEquip(int actorId, int itemId)
        {
            // 因為裝備必定不可堆疊, 不需要檢查可否堆疊, 
            // 也不需要檢查是否已在身上
            DateFile.instance.actorItemsDate[actorId].Remove(itemId);
            // 似乎也能避免一些奇書的問題
        }
    }
}
