using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Reflection.Emit;
using System.Runtime.Serialization;


namespace MoreMerchandise
{
    class MerchantMoney
    {
        // 搬运并改写了原版的 DateFile::UpdateShopMoney 方法
        // 更改商店金钱生成逻辑，使金钱增加
        public static void UpdateShopMoney(ref DateFile __instance)
        {
            // 商店金钱倍数
            int wealthyMultiple = Settings.GetWealthyMultiple();

            int num = 100 + __instance.GetWorldXXLevel() * 30;
            for (int i = 0; i < __instance.storyShopMoney.Length; i++)
            {
                var storyShopMoney = int.Parse(__instance.storyShopDate[i][1]) * num / 100 * (100 + UnityEngine.Random.Range(-20, 21)) / 100;
                __instance.storyShopMoney[i] = storyShopMoney * wealthyMultiple;
            }
        }
    }
}
