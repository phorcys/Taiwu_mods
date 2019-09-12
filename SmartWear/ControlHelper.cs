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
                Dictionary<int, string> playerActorsData = df.actorsDate[mainActorId];

                // 脫下現在的裝備, 移動到身上(行李)
                foreach (KeyValuePair<int, int> slotItemPair in currentEquipConfig)
                {
                    int itemId = slotItemPair.Value;
                    int slot = slotItemPair.Key;
                    if (itemId != 0 &&
                        itemId.ToString() == playerActorsData[slotItemPair.Key])
                    {
                        df.GetItem(mainActorId, itemId, 1, false, -1, 0);
                        playerActorsData[slot] = "0";
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
                        playerActorsData[slot] = itemId.ToString();

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

        public static void TakeoffEquip(int slotKey)
        {
            if (slotKey < 301 || slotKey > 312)
                throw new ArgumentOutOfRangeException($"slotKey must be between 301~312, value={slotKey}");
            var df = DateFile.instance;
            var itemId = int.Parse(df.actorsDate[df.mianActorId][slotKey]);
            df.GetItem(df.mianActorId, itemId, 1, false, -1, 0);
            df.actorsDate[df.mianActorId][slotKey] = "0";
        }

        public static bool WearEquip(int slotKey, int itemId)
        {
            if (slotKey < 301 || slotKey > 312)
                throw new ArgumentOutOfRangeException($"slotKey must be between 301~312, value={slotKey}");
            var df = DateFile.instance;
            var playerId = df.mianActorId;
            if (!df.HasItem(playerId, itemId))
                return false;
            df.LoseItem(playerId, itemId, 1, false, false, -1);
            df.actorsDate[playerId][slotKey] = itemId.ToString();
            return true;
        }
    }
}
