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


namespace Majordomo
{
    public class AutoHarvest
    {
        // 收获物类型
        public const int BOOTY_TYPE_RESOURCE = 0;
        public const int BOOTY_TYPE_ITEM = 1;
        public const int BOOTY_TYPE_ACTOR = 2;

        // 在事件文字中显示的最大物品/人物数量
        public const int MAX_DISPLAYED_ITEMS = 3;
        public const int MAX_DISPLAYED_ACTORS = 3;

        // resourceIndex -> quantity
        public static SortedList<int, int> harvestedResources;
        // quality -> {itemId -> quantity}
        public static SortedList<int, Dictionary<int, int>> harvestedItems;
        public static int numharvestedItems;
        // actorId
        public static List<int> harvestedActors;


        public static void InitializeBooties()
        {
            AutoHarvest.harvestedResources = new SortedList<int, int>();
            AutoHarvest.harvestedItems = new SortedList<int, Dictionary<int, int>>();
            AutoHarvest.numharvestedItems = 0;
            AutoHarvest.harvestedActors = new List<int>();
        }


        public static void RecordBooty(int[] booty)
        {
            int type = booty[0];
            int id = booty[1];
            int quantity = booty[2];
            int oriQuantity;

            switch (type)
            {
                case BOOTY_TYPE_RESOURCE:
                    AutoHarvest.harvestedResources.TryGetValue(id, out oriQuantity);
                    AutoHarvest.harvestedResources[id] = oriQuantity + quantity;
                    break;
                case BOOTY_TYPE_ITEM:
                    int quality = int.Parse(DateFile.instance.GetItemDate(id, 8));
                    if (!AutoHarvest.harvestedItems.ContainsKey(quality))
                    {
                        AutoHarvest.harvestedItems[quality] = new Dictionary<int, int>();
                    }
                    AutoHarvest.harvestedItems[quality].TryGetValue(id, out oriQuantity);
                    AutoHarvest.harvestedItems[quality][id] = oriQuantity + quantity;
                    AutoHarvest.numharvestedItems += quantity;
                    break;
                case BOOTY_TYPE_ACTOR:
                    AutoHarvest.harvestedActors.Add(id);
                    break;
                default:
                    Main.Logger.Warning($"Unknown booty type: {type}");
                    break;
            }
        }


        public static string GetBootiesSummary()
        {
            string summary = "";
            summary += GetHarvestedResourcesSummary();
            summary += GetHarvestedItemsSummary();
            summary += GetHarvestedActorsSummary();

            if (string.IsNullOrEmpty(summary)) summary = "本月尚无收获。\n";
            else summary = "本月收获" + summary;

            return summary;
        }


        private static string GetHarvestedResourcesSummary()
        {
            string summary = "";

            if (AutoHarvest.harvestedResources.Count == 0) return summary;

            foreach (var entry in AutoHarvest.harvestedResources)
            {
                int resourceIndex = entry.Key;
                int quantity = entry.Value;
                string name = DateFile.instance.resourceDate[resourceIndex][1];
                summary += TaiwuCommon.SetColor(TaiwuCommon.COLOR_YELLOW, name) + "\u00A0" +
                    TaiwuCommon.SetColor(TaiwuCommon.COLOR_WHITE, quantity.ToString()) + "、";
            }
            summary = summary.Substring(0, summary.Length - 1) + "。\n";

            return summary;
        }


        private static string GetHarvestedResourcesDetails(ref EarningStats stats)
        {
            string details = "";

            if (AutoHarvest.harvestedResources.Count == 0) return details;

            foreach (var entry in AutoHarvest.harvestedResources)
            {
                int resourceIndex = entry.Key;
                int quantity = entry.Value;
                string name = DateFile.instance.resourceDate[resourceIndex][1];
                details += TaiwuCommon.SetColor(TaiwuCommon.COLOR_YELLOW, name) + "\u00A0" + 
                    TaiwuCommon.SetColor(TaiwuCommon.COLOR_WHITE, quantity.ToString()) + "、";

                switch (resourceIndex)
                {
                    case ResourceMaintainer.RES_ID_MONEY:
                        stats.earnedMoney += quantity;
                        stats.gdp += quantity;
                        break;
                    case ResourceMaintainer.RES_ID_FAME:
                        stats.earnedFame += quantity;
                        stats.gdp += Mathf.RoundToInt(quantity * ResourceMaintainer.EXCHANGE_RATE_FAME);
                        break;
                    default:
                        stats.gdp += Mathf.RoundToInt(quantity * ResourceMaintainer.EXCHANGE_RATE_DEFAULT);
                        break;
                }
            }
            details = details.Substring(0, details.Length - 1) + "。";

            return details;
        }


        private static string GetHarvestedItemsSummary()
        {
            string summary = "";

            if (AutoHarvest.harvestedItems.Count == 0) return summary;

            int numDisplayedItems = 0;
            foreach (var items in AutoHarvest.harvestedItems.Reverse())
            {
                int quality = items.Key;

                foreach (var item in items.Value)
                {
                    int itemId = item.Key;
                    int quantity = item.Value;
                    string name = DateFile.instance.GetItemDate(itemId, 0, otherMassage: false);
                    string coloredName = TaiwuCommon.SetColor(TaiwuCommon.COLOR_LOWEST_LEVEL + quality - 1, name);
                    summary += coloredName + "、";
                    ++numDisplayedItems;

                    if (numDisplayedItems >= MAX_DISPLAYED_ITEMS) break;
                }

                if (numDisplayedItems >= MAX_DISPLAYED_ITEMS) break;
            }
            summary = summary.Substring(0, summary.Length - 1) + "等\u00A0" + AutoHarvest.numharvestedItems + "\u00A0件物品。\n";

            return summary;
        }


        private static string GetHarvestedItemsDetails(ref EarningStats stats)
        {
            string details = "";

            if (AutoHarvest.harvestedItems.Count == 0) return details;

            foreach (var items in AutoHarvest.harvestedItems.Reverse())
            {
                int quality = items.Key;

                foreach (var item in items.Value)
                {
                    int itemId = item.Key;
                    int quantity = item.Value;
                    string name = DateFile.instance.GetItemDate(itemId, 0, otherMassage: false);
                    string coloredName = TaiwuCommon.SetColor(TaiwuCommon.COLOR_LOWEST_LEVEL + quality - 1, name);
                    string coloredQuntity = TaiwuCommon.SetColor(TaiwuCommon.COLOR_WHITE, quantity.ToString());
                    details += $"{coloredName} × {coloredQuntity}、";

                    int itemValue = int.Parse(DateFile.instance.GetItemDate(itemId, 904));
                    stats.gdp += itemValue * quantity;
                }
            }
            details = details.Substring(0, details.Length - 1) + "。";

            return details;
        }


        private static string GetHarvestedActorsSummary()
        {
            string summary = "";

            if (AutoHarvest.harvestedActors.Count == 0) return summary;

            int numDisplayedActors = 0;
            foreach (var actorId in AutoHarvest.harvestedActors)
            {
                string name = DateFile.instance.GetActorName(actorId);
                string coloredName = TaiwuCommon.SetColor(TaiwuCommon.COLOR_DARK_BROWN, name);
                summary += coloredName + "、";
                ++numDisplayedActors;

                if (numDisplayedActors >= MAX_DISPLAYED_ACTORS) break;
            }
            summary = summary.Substring(0, summary.Length - 1) + "等\u00A0" + AutoHarvest.harvestedActors.Count + "\u00A0位村民。\n";

            return summary;
        }


        private static string GetHarvestedActorsDetails(ref EarningStats stats)
        {
            string details = "";

            if (AutoHarvest.harvestedActors.Count == 0) return details;

            foreach (var actorId in AutoHarvest.harvestedActors)
            {
                string name = DateFile.instance.GetActorName(actorId);
                string coloredName = TaiwuCommon.SetColor(TaiwuCommon.COLOR_DARK_BROWN, name);
                details += coloredName + "、";
            }
            details = details.Substring(0, details.Length - 1) + "。";

            return details;
        }


        /// <summary>
        /// 获取所有据点的所有收获物
        /// </summary>
        public static void GetAllBooties()
        {
            AutoHarvest.InitializeBooties();

            foreach (var parts in DateFile.instance.homeShopBootysDate)
            {
                int partId = parts.Key;
                var places = parts.Value;
                foreach (var place in places)
                {
                    int placeId = place.Key;
                    var buildings = place.Value;
                    foreach (var building in buildings)
                    {
                        int buildingIndex = building.Key;
                        var booties = building.Value;
                        foreach (var booty in booties.ToArray())
                        {
                            bool gotBooty = AutoHarvest.GetBooty(partId, placeId, buildingIndex, booty);
                            if (gotBooty)
                            {
                                booties.Remove(booty);
                                AutoHarvest.RecordBooty(booty);
                            }
                        }
                    }
                }
            }

            AutoHarvest.RecordHarvestDetails();

            if (Main.settings.showNewActorWindow && AutoHarvest.harvestedActors.Count > 0)
                GetActorWindow.instance.ShowGetActorWindow(AutoHarvest.harvestedActors, 0);
        }


        /// <summary>
        /// 记录收获详情文本（以及当月收获银钱）
        /// </summary>
        private static void RecordHarvestDetails()
        {
            var currDate = new TaiwuDate();
            var stats = new EarningStats();

            string details = GetHarvestedResourcesDetails(ref stats);
            if (!string.IsNullOrEmpty(details))
                MajordomoWindow.instance.AppendMessage(currDate, Message.IMPORTANCE_HIGH,
                    TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_GRAY, "本月收获资源") + "：" + details);

            details = GetHarvestedItemsDetails(ref stats);
            if (!string.IsNullOrEmpty(details))
                MajordomoWindow.instance.AppendMessage(currDate, Message.IMPORTANCE_HIGH,
                    TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_GRAY, "本月收获物品") + "：" + details);

            details = GetHarvestedActorsDetails(ref stats);
            if (!string.IsNullOrEmpty(details))
                MajordomoWindow.instance.AppendMessage(currDate, Message.IMPORTANCE_HIGH,
                    TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_GRAY, "本月接纳新村民") + "：" + details);

            MajordomoWindow.instance.SetEarningStats(currDate, stats);
        }


        /// <summary>
        /// 拿取收获物（但没有从收获物列表中删除该收获物）
        /// 同时保存了接纳的新村民列表
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="buildingIndex"></param>
        /// <param name="booty"></param>
        /// <returns>是否拿取了收获物</returns>
        private static bool GetBooty(int partId, int placeId, int buildingIndex, int[] booty)
        {
            var building = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];

            int bootyType = booty[0];
            int bootyId = booty[1];
            int bootyQuantity = booty[2];

            string text = "";
            switch (bootyType)
            {
                case BOOTY_TYPE_RESOURCE: // bootyId: resourceIndex
                {
                    text = $"{DateFile.instance.massageDate[7018][1].Split('|')[0]}{DateFile.instance.basehomePlaceDate[building[0]][0]}{DateFile.instance.massageDate[7018][2].Split('|')[(bootyId == 6) ? 1 : 0]}{DateFile.instance.resourceDate[bootyId][1]}{bootyQuantity}</color>";
                    UIDate.instance.ChangeResource(DateFile.instance.MianActorID(), bootyId, bootyQuantity, canShow: false);
                    break;
                }
                case BOOTY_TYPE_ITEM: // bootyId: itemId
                {
                    if (!Main.settings.autoHarvestItems) return false;

                    int itemQuality = int.Parse(DateFile.instance.GetItemDate(bootyId, 8));
                    text = $"{DateFile.instance.massageDate[7018][1].Split('|')[0]}{DateFile.instance.basehomePlaceDate[building[0]][0]}{DateFile.instance.massageDate[7018][2].Split('|')[0]}{DateFile.instance.SetColoer(20001 + itemQuality, $"{DateFile.instance.GetItemDate(bootyId, 0, otherMassage: false)}×{bootyQuantity}")}</color>";
                    DateFile.instance.GetItem(-999, bootyId, bootyQuantity, newItem: false);
                    break;
                }
                case BOOTY_TYPE_ACTOR: // bootyId: actorId
                {
                    if (!Main.settings.autoHarvestActors) return false;
                    if (Main.settings.filterNewActorGender && TryFilterNewActorGender(bootyId)) return false;
                    if (Main.settings.filterNewActorCharm && TryFilterNewActorCharm(bootyId)) return false;
                    if (Main.settings.filterNewActorGoodness && TryFilterNewActorGoodness(bootyId)) return false;
                    if (Main.settings.filterNewActorAttr && TryFilterNewActorAttribute(bootyId)) return false;

                    text = $"{DateFile.instance.massageDate[7018][1].Split('|')[1]}{DateFile.instance.basehomePlaceDate[building[0]][0]}{DateFile.instance.massageDate[7018][2].Split('|')[2]}{DateFile.instance.GetActorName(bootyId)}</color>";
                    DateFile.instance.GetActor(new List<int> { bootyId });
                    DateFile.instance.FamilyActorLeave(bootyId, 16);
                    DateFile.instance.MoveToPlace(int.Parse(DateFile.instance.GetGangDate(16, 3)), int.Parse(DateFile.instance.GetGangDate(16, 4)), bootyId, fromPart: false);
                    UIDate.instance.UpdateManpower();

                    AutoHarvest.LogNewVillagerMerchantType(bootyId);
                    break;
                }
                default:
                {
                    Main.Logger.Warning($"Unknown booty type: {bootyType}");
                    break;
                }
            }

            string[] massageDate = new string[3]{
                DateFile.instance.year.ToString(),
                DateFile.instance.solarTermsDate[DateFile.instance.GetDayTrun()][99],
                text};

            DateFile.instance.AddHomeShopMassage(partId, placeId, buildingIndex, massageDate);

            // 减少建筑的新消息数量
            if (building[12] > 0) --building[12];

            return true;
        }


        /// <summary>
        /// 若新村民性别不符合条件，则返回 true
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        private static bool TryFilterNewActorGender(int actorId)
        {
            int gender = int.Parse(DateFile.instance.GetActorDate(actorId, 14));
            return gender != Main.settings.newActorGenderFilterAccept;
        }


        /// <summary>
        /// 若新村民原始魅力小于阈值，则返回 true
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        private static bool TryFilterNewActorCharm(int actorId)
        {
            int charm = int.Parse(GameData.Characters.GetCharProperty(actorId, 15));
            return charm < Main.settings.newActorCharmFilterThreshold;
        }


        /// <summary>
        /// 若新村民立场在排除列表中，则返回 true
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        private static bool TryFilterNewActorGoodness(int actorId)
        {
            int goodness = DateFile.instance.GetActorGoodness(actorId);
            return !Main.settings.newActorGoodnessFilters[goodness];
        }


        /// <summary>
        /// 若新村民所有原始资质都小于阈值，则返回 true
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        private static bool TryFilterNewActorAttribute(int actorId)
        {
            int maxAttr = -1;

            // 技艺资质
            for (int i = 501; i <= 516; ++i)
                maxAttr = Mathf.Max(maxAttr, int.Parse(GameData.Characters.GetCharProperty(actorId, i)));
            // 武学资质
            for (int i = 601; i <= 614; ++i)
                maxAttr = Mathf.Max(maxAttr, int.Parse(GameData.Characters.GetCharProperty(actorId, i)));

            return maxAttr < Main.settings.newActorAttrFilterThreshold;
        }


        /// <summary>
        /// 在 log 中输出新村民的潜在商队
        /// </summary>
        /// <param name="actorId"></param>
        private static void LogNewVillagerMerchantType(int actorId)
        {
            int gangType = int.Parse(DateFile.instance.GetActorDate(actorId, 9, false));
            int merchantType = int.Parse(DateFile.instance.GetGangDate(gangType, 16));
            string merchantTypeName = DateFile.instance.storyShopDate[merchantType][0];
            Main.Logger.Log($"新村民：{DateFile.instance.GetActorName(actorId)}，潜在商队：{merchantTypeName}");
        }
    }
}
