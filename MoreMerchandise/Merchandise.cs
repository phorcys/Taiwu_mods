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
    class Merchandise
    {
        // 搬运并改写了原版的 ShopSystem::SetShopItems 方法
        // 更改商店商品生成逻辑，使物品数量增加
        public static void SetShopItems(ref ShopSystem __instance, int shopTyp, int moneyCost, int levelAdd, int isTaiWu, int shopActorId,
            ref int ___actorShopId, ref int ___shopSystemTyp, ref int ___showLevelAdd,
            ref int ___newShopLevel, ref int ___shopSellCost, ref int ___shopSystemCost)
        {
            // 角色 ID（猜测行商没有角色 ID）
            ___actorShopId = shopActorId;
            // 如果有角色 ID，那么在角色数据表里取商队 ID，再根据商队 ID 获得商店类型
            if (___actorShopId != 0)
            {
                shopTyp = int.Parse(DateFile.instance.GetGangDate(int.Parse(DateFile.instance.GetActorDate(___actorShopId, 9, false)), 16));
            }
            __instance.shopItems.Clear();
            ___shopSystemTyp = shopTyp;
            ___showLevelAdd = levelAdd;
            ___newShopLevel = DateFile.instance.storyShopLevel[shopTyp] + levelAdd;

            Main.Logger.Log($"商店基础等级: {DateFile.instance.storyShopLevel[shopTyp]}, 附加等级: {levelAdd}");

            if (___actorShopId != 0)
            {
                // 获得角色对主角的好感，根据好感计算买卖价格
                int actorFavor = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), ___actorShopId, getLevel: true);
                ___shopSellCost = 30 + actorFavor * 5;
                ___shopSystemCost = moneyCost - actorFavor * 15;
            }
            else
            {
                ___shopSellCost = 25 + ___newShopLevel / 200;
                ___shopSystemCost = moneyCost - Mathf.Clamp(100 * ___newShopLevel / 5000, -100, 100);
            }

            // 如果该角色当季已经刷出了商品，那么直接从该角色的商品列表中获取商品
            if (___actorShopId != 0 && DateFile.instance.actorShopDate.ContainsKey(___actorShopId))
            {
                List<int> list = new List<int>(DateFile.instance.actorShopDate[___actorShopId].Keys);
                for (int i = 0; i < list.Count; i++)
                {
                    int num = list[i];
                    int num2 = DateFile.instance.actorShopDate[___actorShopId][num];
                    if (num > 0 && num2 > 0)
                    {
                        __instance.shopItems.Add(num, num2);
                    }
                }
            }
            // 新生成商品列表
            else
            {
                // Modified ********************************************************************************************
                __instance.shopItems = GenerateMerchandiseWrapper(shopTyp, isTaiWu, ref ___newShopLevel);
                // *****************************************************************************************************

                // 商品数据生成完毕，保存到该人物对应的存储空间中
                if (___actorShopId != 0)
                {
                    DateFile.instance.actorShopDate.Add(___actorShopId, new Dictionary<int, int>());
                    List<int> list8 = new List<int>(__instance.shopItems.Keys);
                    for (int num30 = 0; num30 < list8.Count; num30++)
                    {
                        int key = list8[num30];
                        DateFile.instance.actorShopDate[___actorShopId].Add(key, __instance.shopItems[key]);
                    }
                }
            }
        }


        public static Dictionary<int, int> FilterLowQualityItems(Dictionary<int, int> shopItems)
        {
            Dictionary<int, int> filtered = new Dictionary<int, int>();

            foreach (var pair in shopItems)
            {
                int itemQuality = 10 - int.Parse(DateFile.instance.GetItemDate(pair.Key, 8));
                // 类型组 82 为资源包
                int itemTypeGroup = int.Parse(DateFile.instance.GetItemDate(pair.Key, 98));
                // 小类 23 为食材
                int itemSubType = int.Parse(DateFile.instance.GetItemDate(pair.Key, 5));
                // 物品品级小于等于阈值，或物品类型为资源包、食材，则保留
                if (itemQuality <= Main.settings.lowQualityThreshold || itemTypeGroup == 82 || itemSubType == 23)
                {
                    filtered[pair.Key] = pair.Value;
                }
            }

            return filtered;
        }


        // 照抄原版的 ShopSystem::SetShopItems 方法，把生成物品列表的部分代码运行了多次
        private static Dictionary<int, int> GenerateMerchandiseWrapper(int shopTyp, int isTaiWu,
            ref int ___newShopLevel)
        {
            // 首先往待生成商品信息列表中，加入商队的默认商品信息，然后根据当前商店等级加入更高等级的商品信息
            // 每个商品信息的数据形式为 "4201&6"，即由两个数字组成，第一个为商品 ID，第二个为理想品级
            List<string> list2 = new List<string>(DateFile.instance.storyShopDate[shopTyp][2].Split('|'));
            // num3 和 num4 都和商店等级正相关，num3 取值范围 [0, 5000]，num4 取值范围 [0, 5] 且为整数
            int num3 = Mathf.Clamp(___newShopLevel, 0, 5000);
            int num4 = num3 * 5 / 5000;
            if (num4 > 0)
            {
                for (int j = 0; j < num4; j++)
                {
                    // 表格列查询范围 [3, 7]
                    if (DateFile.instance.storyShopDate[shopTyp][3 + j] != "0&0")
                    {
                        list2.AddRange(DateFile.instance.storyShopDate[shopTyp][3 + j].Split('|'));
                    }
                }
            }

            // Modified ************************************************************************************************
            Dictionary<int, int> shopItems = new Dictionary<int, int>();

            // 商品数量倍数
            int wealthyMultiple = Settings.GetWealthyMultiple();
            for (int i = 0; i < wealthyMultiple; ++i)
            {
                Dictionary<int, int> currShopItems = GenerateMerchandise(shopTyp, isTaiWu, list2, num3);
                foreach (var pair in currShopItems)
                {
                    int oriValue;
                    shopItems.TryGetValue(pair.Key, out oriValue);
                    shopItems[pair.Key] = oriValue + pair.Value;
                }
            }
            // *********************************************************************************************************

            return shopItems;
        }


        // 照抄原版的 ShopSystem::SetShopItems 方法，只是提取了部分片段写成了单独方法而已
        private static Dictionary<int, int> GenerateMerchandise(int shopTyp, int isTaiWu,
            List<string> list2, int num3)
        {
            Dictionary<int, int> shopItems = new Dictionary<int, int>();

            // 物品 ID 列表
            List<int> list3 = new List<int>();
            for (int k = 0; k < list2.Count; k++)
            {
                string[] array = list2[k].Split('&');
                // 物品原始 ID（属性可变的物品会在之后生成新 ID）
                int num5 = int.Parse(array[0]);
                if (num5 > 0)
                {
                    // 商品理想品级，在商店等级最高时，商品实际品级和该数值一致
                    int num6 = Mathf.Max(int.Parse(array[1]) * num3 / 5000, 0);
                    // 是否为可叠加物品，取值 0/1。为 1 的有：打包资源、促织罐、杂虫、图纸、引子。
                    int num7 = int.Parse(DateFile.instance.GetItemDate(num5, 6));
                    // 物品的数量。对于不可叠加物品，此值为 1；可叠加的，此值为 [1.5, 4.53] 之间的随机值。
                    int num8 = (num7 == 0) ? 1 : Mathf.Max(3 * UnityEngine.Random.Range(50, 151) / 100, 1);
                    // 物品类型图纸的，数量也为 1
                    if (int.Parse(DateFile.instance.GetItemDate(num5, 5)) == 20)
                    {
                        num8 = 1;
                    }
                    // 物品品级，九品为 1，一品为 9
                    int num9 = int.Parse(DateFile.instance.GetItemDate(num5, 8));
                    // 低品级物品必然被放入列表，高品级物品有一定概率放入，品级越高概率越小
                    if (num9 <= 3 || UnityEngine.Random.Range(0, 100) < 100 - num9 * 10)
                    {
                        list3.Add(num5);
                        // 对于不可叠加物品，生成新的实例
                        shopItems.Add((num7 != 0) ? num5 : DateFile.instance.MakeNewItem(num5), num8);
                        if (num6 > 0)
                        {
                            // 尝试放入高品级物品，品级越高被放入的概率越小
                            for (int l = 0; l < num6; l++)
                            {
                                // 当前待放入的物品原始 ID（属性可变的物品会在之后生成新 ID）
                                int num10 = num5 + l + 1;
                                if (UnityEngine.Random.Range(0, 100) < 100 - int.Parse(DateFile.instance.GetItemDate(num10, 8)) * 10)
                                {
                                    // 是否为可叠加物品
                                    int num11 = int.Parse(DateFile.instance.GetItemDate(num10, 6));
                                    // 物品的数量
                                    int value = (num11 == 0) ? 1 : Mathf.Max(num8 - num8 * (l + 1) / num6, 1);
                                    shopItems.Add((num11 != 0) ? num10 : DateFile.instance.MakeNewItem(num10), value);
                                }
                            }
                        }
                    }
                }
            }

            // 如果商店类型是公输坊，会根据主角所学技艺增加相应图纸
            int num12 = 0;
            if (shopTyp == 5)
            {
                List<int> list4 = new List<int>(DateFile.instance.actorSkills.Keys);
                for (int m = 0; m < list4.Count; m++)
                {
                    int num13 = list4[m];
                    if (DateFile.instance.GetSkillLevel(num13) >= 75)
                    {
                        string[] array2 = DateFile.instance.skillDate[num13][1].Split('|');
                        for (int n = 0; n < array2.Length; n++)
                        {
                            int num14 = int.Parse(array2[n]);
                            if (num14 != 0)
                            {
                                shopItems.Add(num14, 1);
                                num12++;
                            }
                        }
                    }
                }
            }

            // 当商品数量超过限制时，随机删除部分商品（判断超限有个随机范围）
            // num3 和商店等级正相关，取值范围 [0, 5000]
            // num15 为商品超限个数
            int num15 = shopItems.Count - ((15 + Mathf.Max(num3 / 200, 0)) * UnityEngine.Random.Range(50, 101) / 100 + num12 / 2);
            if (num15 > 0)
            {
                for (int num16 = 0; num16 < num15; num16++)
                {
                    List<int> list5 = new List<int>(shopItems.Keys);
                    int num17 = list5[UnityEngine.Random.Range(0, list5.Count)];
                    shopItems.Remove(num17);
                    DateFile.instance.DeleteItemDate(num17);
                }
            }

            // list6 为商店特殊商品（不会被随机删除的商品）列表
            List<string> list6 = new List<string>(DateFile.instance.storyShopDate[shopTyp][99].Split('|'));
            for (int num18 = 0; num18 < list6.Count; num18++)
            {
                string[] array3 = list6[num18].Split('&');
                // num19 为物品 ID
                int num19 = int.Parse(array3[0]);
                if (num19 > 0)
                {
                    // num20: 商品理想品级，在商店等级最高时，商品实际品级和该数值一致
                    int num20 = Mathf.Max(int.Parse(array3[1]) * num3 / 5000, 0);
                    // num21: 是否为可叠加物品，取值 0/1。为 1 的有：打包资源、促织罐、杂虫、图纸、引子。
                    int num21 = int.Parse(DateFile.instance.GetItemDate(num19, 6));
                    // num22: 物品的数量。对于不可叠加物品，此值为 1；可叠加的，此值为 [1, 4] 之间的随机值。
                    int num22 = (num21 == 0) ? 1 : Mathf.Max(3 * UnityEngine.Random.Range(50, 151) / 100, 1);
                    // num23: 物品品级，九品为 1，一品为 9
                    int num23 = int.Parse(DateFile.instance.GetItemDate(num19, 8));
                    // 低品级物品必然被放入列表，高品级物品有一定概率放入，品级越高概率越小
                    if (num23 <= 3 || UnityEngine.Random.Range(0, 100) < 100 - num23 * 10)
                    {
                        list3.Add(num19);
                        // 对于不可叠加物品，生成新的实例
                        shopItems.Add((num21 != 0) ? num19 : DateFile.instance.MakeNewItem(num19), num22);
                        if (num20 > 0)
                        {
                            // 尝试放入高品级物品，品级越高被放入的概率越小
                            for (int num24 = 0; num24 < num20; num24++)
                            {
                                // 当前待放入的物品原始 ID（属性可变的物品会在之后生成新 ID）
                                int num25 = num19 + num24 + 1;
                                if (UnityEngine.Random.Range(0, 100) < 100 - int.Parse(DateFile.instance.GetItemDate(num25, 8)) * 10)
                                {
                                    // 是否为可叠加物品
                                    int num26 = int.Parse(DateFile.instance.GetItemDate(num25, 6));
                                    // 物品的数量
                                    int value2 = (num26 == 0) ? 1 : Mathf.Max(num22 - num22 * (num24 + 1) / num20, 1);
                                    shopItems.Add((num26 != 0) ? num25 : DateFile.instance.MakeNewItem(num25), value2);
                                }
                            }
                        }
                    }
                }
            }

            // 根据 isTaiWu 参数额外放入所有商会公共的物品
            if (isTaiWu > 0)
            {
                // 商店类型为 99，列表根据输入参数 isTaiWu 选择
                List<string> list7 = new List<string>(DateFile.instance.storyShopDate[99][isTaiWu].Split('|'));
                for (int num27 = 0; num27 < list7.Count; num27++)
                {
                    // 固定概率添加物品
                    if (UnityEngine.Random.Range(0, 100) >= 25)
                    {
                        string[] array4 = list7[num27].Split('&');
                        // num28: 物品 ID
                        int num28 = int.Parse(array4[0]);
                        if (num28 > 0)
                        {
                            // 是否为可叠加物品，0 为不可叠加
                            int num29 = int.Parse(DateFile.instance.GetItemDate(num28, 6));
                            // 如果物品不可叠加，或者可叠加但是基础商品列表里没有，则添加该商品
                            if (num29 == 0 || !list3.Contains(num28))
                            {
                                // 物品的数量
                                int value3 = (num29 == 0) ? 1 : Mathf.Max(int.Parse(array4[1]) * UnityEngine.Random.Range(50, 151) / 100, 1);
                                shopItems.Add((num29 != 0) ? num28 : DateFile.instance.MakeNewItem(num28), value3);
                            }
                        }
                    }
                }
            }

            return shopItems;
        }
    }
}
