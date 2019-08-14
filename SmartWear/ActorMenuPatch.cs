﻿using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWear
{
    [HarmonyPatch(typeof(ActorMenu), "MianQiAutoChange")]
    public class ActorMenu_MianQiAutoChange_Patch
    {

        private static void Prefix(int key)
        {
            var df = DateFile.instance;
            if (key != df.mianActorId) return;
//#if (DEBUG)
//            Main.Logger.Log($"現在內息: {df.GetActorMianQi(key)}");
//#endif 
            if (!Main.Enabled) return;

            if (Main.settings.RestAutoEquip)
            {
                var items = ItemHelper.GetEquipItems()
                    .Select(kvp => kvp.Value).Concat(ItemHelper.GetBagItems().Concat(ItemHelper.GetWarehouseItems()))
                    .AsQueryable();

                // 取得內息最高的3樣武器
                items = items.Where(ItemHelper.GetEquipTypeFilter(EquipType.Weapon));
                items = items.Where(ItemHelper.GetHasDataFilter(ItemDateKey.MianQi));
                items = items.OrderByDescending(ItemHelper.GetOrderByData(ItemDateKey.MianQi));
#if (DEBUG)
                foreach (var item in items.Take(3))
                {
                    Main.Logger.Log($"Equip: {item}, 內息: { df.GetItemDateValue(item, ItemDateKey.MianQi)}");
                }
#endif
                StateManager.EquipWeapons(items.Take(3));
            }

            if (Main.settings.RestGongFaIndex >= 0)
                StateManager.UseGongFa(Main.settings.RestGongFaIndex);


        }

        private static void Postfix(int key)
        {
            if (key != DateFile.instance.mianActorId) return;
//#if (DEBUG)
//            Main.Logger.Log($"恢復後內息: {DateFile.instance.GetActorMianQi(key)}");
//#endif
            if (Main.Enabled)
                StateManager.RestoreAll();
        }
    }
}