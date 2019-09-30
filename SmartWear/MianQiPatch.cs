using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWear
{
    // AI.CharacterAge.UpdateInnerBreathDisorder
    [HarmonyPatch(typeof(AI.CharacterAge), "UpdateInnerBreathDisorder")]
    public class CharacterAge_UpdateInnerBreathDisorder_Patch
    {

        private static void Prefix(int charId)
        {
            var df = DateFile.instance;
            if (charId != df.mianActorId) return;
//#if (DEBUG)
//            Main.Logger.Log($"現在內息: {df.GetActorMianQi(charId)}");
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
                if (Main.settings.EnabledLog)
                {
                    foreach (var item in items.Take(3))
                    {
                        Main.Logger.Log($"裝備: {item} ({df.GetItemDate(item, 0, false)}), 內息: { df.GetItemDateValue(item, ItemDateKey.MianQi)}");
                    }
                }
                StateManager.EquipWeapons(items.Take(3));
            }

            if (Main.settings.RestGongFaIndex >= 0)
                StateManager.UseGongFa(Main.settings.RestGongFaIndex);


        }

        private static void Postfix(int charId, AI.AgeChangeModifiedData modifiedData)
        {
            if (charId != DateFile.instance.mianActorId) return;
#if (DEBUG)
            Main.Logger.Log($"內息變化量: {modifiedData?.ChangeMianQi.LastOrDefault().Value }");
#endif
            if (Main.Enabled)
                StateManager.RestoreAll();
        }
    }
}
