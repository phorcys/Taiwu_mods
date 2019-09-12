using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityModManagerNet;


namespace Majordomo
{
    public class ResourceInfo
    {
        public int current;
        public int max;
        public int consumed;


        public ResourceInfo(int current, int max, int consumed)
        {
            this.current = current;
            this.max = max;
            this.consumed = consumed;
        }
    }


    public class TemporaryResourceShop
    {
        private class ItemInfo
        {
            public int id;
            public int price;
            public int amount;


            public ItemInfo(int id, int price, int amount)
            {
                this.id = id;
                this.price = price;
                this.amount = amount;
            }
        }


        private static readonly int[] RES_PACK_ITEM_IDS = { 11, 12, 13, 14, 15 };

        private readonly Dictionary<int, ItemInfo> resources;      // resourceId -> itemInfo


        // 直接调用 ShopSystem::SetShopItems 方法获得物品列表，商队类型和公共商品类型等随机数完全按照原逻辑进行
        // 如果有其他 mod 对 ShopSystem::SetShopItems 进行了修改，此 mod 会一并体现那些改动
        public TemporaryResourceShop()
        {
            this.resources = new Dictionary<int, ItemInfo>();

            int shopTyp = UnityEngine.Random.Range(0, 7);
            int moneyCost = 200;
            int levelAdd = 0;
            int isTaiWu = int.Parse(DateFile.instance.solarTermsDate[DateFile.instance.GetDayTrun()][4]);
            ShopSystem.instance.SetShopItems(shopTyp, moneyCost, levelAdd, isTaiWu);

            var field = typeof(ShopSystem).GetField("shopSystemCost", BindingFlags.NonPublic | BindingFlags.Instance);
            int shopSystemCost = (int) field.GetValue(ShopSystem.instance);

            foreach (var entry in ShopSystem.instance.shopItems)
            {
                int itemId = entry.Key;
                int resourceId = Array.IndexOf(RES_PACK_ITEM_IDS, itemId);
                if (resourceId < 0) continue;

                int itemPrice = int.Parse(DateFile.instance.GetItemDate(itemId, 905)) * shopSystemCost / 100;
                int nItemAmount = entry.Value;
                this.resources[resourceId] = new ItemInfo(itemId, itemPrice, nItemAmount);
            }

            ShopSystem.instance.shopItems.Clear();

            // TEST ------------------------------------------------------------
            Main.Logger.Log("临时商店商品：");
            foreach (var entry in resources)
            {
                int resourceId = entry.Key;
                var itemInfo = entry.Value;
                string name = DateFile.instance.resourceDate[resourceId][1];
                Main.Logger.Log($"  {name}: 价格 {itemInfo.price}, 数量 {itemInfo.amount}");
            }
            // -----------------------------------------------------------------
        }


        // 以随机顺序购买，以达到均匀化的效果
        // @return: spentMoney
        public int Buy(int initialMoney, Dictionary<int, int> resPacksNeedToBuy, SortedList<int, int> boughtResources)
        {
            // TEST ------------------------------------------------------------
            Main.Logger.Log($"可用银钱 {initialMoney}, 待购买商品：");
            foreach (var entry in resPacksNeedToBuy)
            {
                int resourceId = entry.Key;
                int amount = entry.Value;
                string name = DateFile.instance.resourceDate[resourceId][1];
                Main.Logger.Log($"  {name}: 待购买数量 {amount}");
            }
            // -----------------------------------------------------------------

            int remainedMoney = initialMoney;
            boughtResources.Clear();
            var rand = new System.Random();
            List<int> availableResIds = new List<int>();
            this.CalcAvailableResourceIds(resPacksNeedToBuy, remainedMoney, availableResIds);

            while (availableResIds.Count > 0)
            {
                int resourceId = availableResIds[rand.Next(availableResIds.Count)];
                var itemInfo = this.resources[resourceId];

                int nResources = int.Parse(DateFile.instance.GetItemDate(itemInfo.id, 55)) * UnityEngine.Random.Range(80, 121) / 100;
                int actorId = DateFile.instance.MianActorID();
                UIDate.instance.ChangeResource(actorId, resourceId, nResources, canShow: false);
                UIDate.instance.ChangeResource(actorId, ResourceMaintainer.RES_ID_MONEY, -itemInfo.price, canShow: false);

                remainedMoney -= itemInfo.price;

                int nOriResources;
                boughtResources.TryGetValue(resourceId, out nOriResources);
                boughtResources[resourceId] = nOriResources + nResources;

                --resPacksNeedToBuy[resourceId];
                if (resPacksNeedToBuy[resourceId] <= 0) resPacksNeedToBuy.Remove(resourceId);

                --this.resources[resourceId].amount;
                if (this.resources[resourceId].amount <= 0) this.resources.Remove(resourceId);

                this.CalcAvailableResourceIds(resPacksNeedToBuy, remainedMoney, availableResIds);
            }

            // TEST ------------------------------------------------------------
            Main.Logger.Log($"花费银钱 {initialMoney - remainedMoney}, 购得资源：");
            foreach (var entry in boughtResources)
            {
                string name = DateFile.instance.resourceDate[entry.Key][1];
                Main.Logger.Log($"  {name} x {entry.Value}");
            }
            // -----------------------------------------------------------------

            return initialMoney - remainedMoney;
        }


        // 计算自己想买、而且商店也有、而且还买得起的商品列表
        private void CalcAvailableResourceIds(Dictionary<int, int> resPacksNeedToBuy, int remainedMoney, List<int> availableResIds)
        {
            availableResIds.Clear();

            foreach (var entry in resPacksNeedToBuy)
            {
                int resouceId = entry.Key;

                if (!this.resources.ContainsKey(resouceId)) continue;

                int currPrice = this.resources[resouceId].price;
                if (remainedMoney < currPrice) continue;

                availableResIds.Add(resouceId);
            }
        }
    }


    public class ResourceMaintainer
    {
        // 资源 ID
        public const int RES_ID_FOOD = 0;
        public const int RES_ID_WOOD = 1;
        public const int RES_ID_STONE = 2;
        public const int RES_ID_SILK = 3;
        public const int RES_ID_HERBAL = 4;
        public const int RES_ID_MONEY = 5;
        public const int RES_ID_FAME = 6;
        public const int RES_ID_HUMAN = 7;
        public const int RES_ID_CULTURE = 8;
        public const int RES_ID_STABLE = 9;

        public const float EXCHANGE_RATE_FAME = 15;
        public const float EXCHANGE_RATE_DEFAULT = 1;

        public const int RESOURCE_PACK_SIZE = 480;


        // cache for reshow last turn event window
        public static string resourceWarning;
        public static string shoppingRecord;

        // cached data of the last turn
        public static int spentMoney;
        public static SortedList<int, int> boughtResources;     // resourceId -> nResources

        public static Dictionary<int, Text> resIdealHoldingText = new Dictionary<int, Text>();
        public static bool changingResIdealHolding;


        public static SortedList<int, ResourceInfo> GetResourcesInfo()
        {
            int[] currResources = DateFile.instance.ActorResource(DateFile.instance.MianActorID());
            int maxResource = 1000 + UIDate.instance.GetMaxResource();

            return new SortedList<int, ResourceInfo>
            {
                [RES_ID_FOOD] = new ResourceInfo(
                    currResources[RES_ID_FOOD],
                    maxResource,
                    UIDate.instance.ResourceUPValue(RES_ID_FOOD, DateFile.instance.foodUPList)),

                [RES_ID_WOOD] = new ResourceInfo(
                    currResources[RES_ID_WOOD],
                    maxResource,
                    UIDate.instance.ResourceUPValue(RES_ID_WOOD, DateFile.instance.woodUPList)),

                [RES_ID_STONE] = new ResourceInfo(
                    currResources[RES_ID_STONE],
                    maxResource,
                    UIDate.instance.ResourceUPValue(RES_ID_STONE, DateFile.instance.stoneUPList)),

                [RES_ID_SILK] = new ResourceInfo(
                    currResources[RES_ID_SILK],
                    maxResource,
                    UIDate.instance.ResourceUPValue(RES_ID_SILK, DateFile.instance.silkUPList)),

                [RES_ID_HERBAL] = new ResourceInfo(
                    currResources[RES_ID_HERBAL],
                    maxResource,
                    UIDate.instance.ResourceUPValue(RES_ID_HERBAL, DateFile.instance.herbalUPList)),

                [RES_ID_MONEY] = new ResourceInfo(
                    currResources[RES_ID_MONEY],
                    maxResource,
                    UIDate.instance.ResourceUPValue(RES_ID_MONEY, DateFile.instance.moneyUPList)),
            };
        }


        public static void TryBuyingResources()
        {
            ResourceMaintainer.shoppingRecord = "";

            var resourcesInfo = ResourceMaintainer.GetResourcesInfo();

            var resPacksNeedToBuy = ResourceMaintainer.GetResPacksNeedToBuy(resourcesInfo);
            if (resPacksNeedToBuy.Count == 0) return;

            var moneyInfo = resourcesInfo[RES_ID_MONEY];
            int usableMoney = Math.Min(
                moneyInfo.current - Main.settings.moneyMinHolding,
                moneyInfo.current - Math.Max(-moneyInfo.consumed * Main.settings.resMinHolding, 0));
            if (usableMoney <= 0) return;

            ResourceMaintainer.BuyResources(usableMoney, resPacksNeedToBuy);
        }


        private static Dictionary<int, int> GetResPacksNeedToBuy(SortedList<int, ResourceInfo> resourcesInfo)
        {
            // resouceId -> nPacks
            Dictionary<int, int> resPacksNeedToBuy = new Dictionary<int, int>();

            for (int i = 0; i < Main.settings.resIdealHolding.Length; ++i)
            {
                int ideal = Main.settings.resIdealHolding[i];
                int current = resourcesInfo[i].current;
                int nLackedPacks = (ideal - current) / RESOURCE_PACK_SIZE;
                if (nLackedPacks > 0) resPacksNeedToBuy[i] = nLackedPacks;
            }

            return resPacksNeedToBuy;
        }


        private static void BuyResources(int usableMoney, Dictionary<int, int> resPacksNeedToBuy)
        {
            TemporaryResourceShop shop = new TemporaryResourceShop();
            ResourceMaintainer.boughtResources = new SortedList<int, int>();
            ResourceMaintainer.spentMoney = shop.Buy(usableMoney, resPacksNeedToBuy, ResourceMaintainer.boughtResources);
            ResourceMaintainer.UpdateShoppingRecord();
        }


        private static void UpdateShoppingRecord()
        {
            string text = "";

            if (ResourceMaintainer.spentMoney > 0)
            {
                text += "花费" + TaiwuCommon.SetColor(TaiwuCommon.COLOR_YELLOW, "银钱") + "\u00A0" +
                    TaiwuCommon.SetColor(TaiwuCommon.COLOR_WHITE, ResourceMaintainer.spentMoney.ToString()) + "\u00A0，购入了";

                foreach (var entry in ResourceMaintainer.boughtResources)
                {
                    int resourceId = entry.Key;
                    int nResources = entry.Value;
                    string name = DateFile.instance.resourceDate[resourceId][1];
                    text += TaiwuCommon.SetColor(TaiwuCommon.COLOR_YELLOW, name) + "\u00A0" +
                        TaiwuCommon.SetColor(TaiwuCommon.COLOR_WHITE, nResources.ToString()) + "、";
                }
                text = text.Substring(0, text.Length - 1) + "。\n";
            }

            ResourceMaintainer.shoppingRecord = text;

            if (!string.IsNullOrEmpty(text))
            {
                string recordingMessage = "管家" + text.Replace("\n", "");
                MajordomoWindow.instance.AppendMessage(new TaiwuDate(), Message.IMPORTANCE_HIGH, recordingMessage);
            }
        }


        // 因为每时每刻的资源数量都可能变化，因此要显示月初的资源警示，就必须缓存住
        public static void UpdateResourceWarning()
        {
            string text = "";

            foreach (var entry in ResourceMaintainer.GetResourcesInfo())
            {
                var resourceId = entry.Key;
                var resourceInfo = entry.Value;
                if (resourceInfo.current < -resourceInfo.consumed * Main.settings.resMinHolding)
                {
                    string name = DateFile.instance.resourceDate[(int)resourceId][1];
                    text += TaiwuCommon.SetColor(TaiwuCommon.COLOR_YELLOW, name) + "、";
                }
            }

            if (text.Length > 0)
            {
                text = "以下资源库存不足：" + text.Substring(0, text.Length - 1) + "。\n需要尽快补充，否则将导致建筑损坏。\n";
                text = TaiwuCommon.SetColor(TaiwuCommon.COLOR_RED, text);
            }

            ResourceMaintainer.resourceWarning = text;

            if (!string.IsNullOrEmpty(text))
            {
                string recordingMessage = text.Replace("\n", "");
                MajordomoWindow.instance.AppendMessage(new TaiwuDate(), Message.IMPORTANCE_HIGHEST, recordingMessage);
            }
        }


        public static void InitialzeResourcesIdealHolding()
        {
            // 初始化资源保有目标
            if (Main.settings.resIdealHolding == null)
            {
                Main.settings.resIdealHolding = new int[5];

                var resourcesInfo = ResourceMaintainer.GetResourcesInfo();

                for (int i = 0; i < Main.settings.resIdealHolding.Length; ++i)
                {
                    int ideal = (int)(resourcesInfo[i].max * Main.settings.resInitIdealHoldingRatio);
                    ideal = (ideal / 100) * 100;
                    Main.settings.resIdealHolding[i] = ideal;
                }
            }

            // 向资源图标注册鼠标事件
            GameObject[] resourceIcons = GameObject.FindGameObjectsWithTag("ResourceIcon");
            foreach (GameObject resourceIcon in resourceIcons)
            {
                int resourceId = int.Parse(resourceIcon.name.Split(',')[1]);
                switch (resourceId)
                {
                    case RES_ID_FOOD:
                    case RES_ID_WOOD:
                    case RES_ID_STONE:
                    case RES_ID_SILK:
                    case RES_ID_HERBAL:
                        var handler = resourceIcon.AddComponent<ResourceIconPointerHandler>();
                        handler.resourceId = resourceId;
                        break;
                }
            }

            // 增加资源保有目标文本控件
            foreach (var text in GameObject.FindObjectsOfType(typeof(Text)) as Text[])
            {
                switch (text.name)
                {
                    case "FoodUPText":
                        ResourceMaintainer.RegisterResourceIdealHoldingText(RES_ID_FOOD, text.transform.parent);
                        break;
                    case "WoodUPText":
                        ResourceMaintainer.RegisterResourceIdealHoldingText(RES_ID_WOOD, text.transform.parent);
                        break;
                    case "StoneUPText":
                        ResourceMaintainer.RegisterResourceIdealHoldingText(RES_ID_STONE, text.transform.parent);
                        break;
                    case "SilkUPText":
                        ResourceMaintainer.RegisterResourceIdealHoldingText(RES_ID_SILK, text.transform.parent);
                        break;
                    case "HerbalUPText":
                        ResourceMaintainer.RegisterResourceIdealHoldingText(RES_ID_HERBAL, text.transform.parent);
                        break;
                }
            }
        }


        public static bool ChangeResourceIdealHolding(int resourceId, bool add, int nMultiple)
        {
            int amount = (add ? 100 : -100) * nMultiple;

            int ideal = Main.settings.resIdealHolding[resourceId] + amount;

            var resourcesInfo = ResourceMaintainer.GetResourcesInfo();
            int max = resourcesInfo[resourceId].max;

            if (ideal > max) ideal = max;
            if (ideal < 0) ideal = 0;

            if (Main.settings.resIdealHolding[resourceId] == ideal) return false;

            Main.settings.resIdealHolding[resourceId] = ideal;
            ResourceMaintainer.resIdealHoldingText[resourceId].text = ideal.ToString();
            return true;
        }


        public static void RegisterResourceIdealHoldingText(int resourceId, Transform parent)
        {
            var textGO = new GameObject($"ResourceIdealHolding,{resourceId + 101}", typeof(Text));
            textGO.transform.SetParent(parent, false);

            var text = textGO.GetComponent<Text>();
            text.font = DateFile.instance.font;
            text.text = "<color=#8E8E8EFF>" + Main.settings.resIdealHolding[resourceId].ToString() + "</color>";
            text.fontSize = 16;
            text.color = Color.gray;
            text.alignment = TextAnchor.MiddleRight;
            text.gameObject.SetActive(false);

            var outline = textGO.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1, -1);
            outline.useGraphicAlpha = true;

            var textTransform = textGO.GetComponent<RectTransform>();
            textTransform.localPosition = new Vector3(35.0f, -18.0f, 0.0f);
            textTransform.sizeDelta = new Vector2(110.0f, 30.0f);

            ResourceMaintainer.resIdealHoldingText[resourceId] = text;
        }


        public static void InterfereFloatWindow(WindowManage __instance)
        {
            if (ResourceMaintainer.changingResIdealHolding) __instance.anTips = false;
        }


        // 根据鼠标位置，显示或隐藏资源保有目标
        public static void ShowResourceIdealHoldingText()
        {
            var posY = Input.mousePosition.y;
            bool active = (posY >= Screen.height - 60) && (posY < Screen.height);
            foreach (var text in ResourceMaintainer.resIdealHoldingText.Values)
            {
                text.gameObject.SetActive(active);
            }
        }
    }


    public class ResourceIconPointerHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public int resourceId;
        private bool isPressed;
        private PointerEventData.InputButton pressedButton;
        private int nPressedFrames;


        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Main.enabled) return;

            this.isPressed = true;
            this.nPressedFrames = 0;
            ResourceMaintainer.changingResIdealHolding = true;

            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                case PointerEventData.InputButton.Right:
                    this.pressedButton = eventData.button;
                    this.OnClick();
                    break;
            }
        }


        public void OnPointerUp(PointerEventData eventData)
        {
            if (!Main.enabled) return;

            this.isPressed = false;
            this.nPressedFrames = 0;
            ResourceMaintainer.changingResIdealHolding = false;
        }


        public void Update()
        {
            if (!Main.enabled) return;

            if (!this.isPressed) return;

            ResourceMaintainer.changingResIdealHolding = true;
            ++this.nPressedFrames;

            if (this.nPressedFrames >= 25)
            {
                if (this.nPressedFrames % 5 == 0)
                {
                    int nMultiple = this.nPressedFrames / 25;
                    if (nMultiple < 1) nMultiple = 1;
                    if (nMultiple > 10) nMultiple = 10;
                    this.OnClick(nMultiple);
                }
            }
        }


        private void OnClick(int nMultiple = 1)
        {
            bool add = this.pressedButton == PointerEventData.InputButton.Left;
            bool changed = ResourceMaintainer.ChangeResourceIdealHolding(this.resourceId, add, nMultiple);
            if (changed) AudioManager.instance.PlaySE("SE_2");
        }
    }


    /// <summary>
    /// Patch: 创建 UI
    /// </summary>
    [HarmonyPatch(typeof(ui_TopBar), "Awake")]
    public static class ui_TopBar_Awake_InitialzeResources
    {
        static void Postfix()
        {
            if (!Main.enabled) return;

            ResourceMaintainer.InitialzeResourcesIdealHolding();
        }
    }


    /// <summary>
    /// Patch: 更新 UI
    /// 因为 ui_TopBar 没有 Update 方法, 所以只好加在生命周期一致的另一个 UI 上
    /// </summary>
    [HarmonyPatch(typeof(ui_BottomLeft), "Update")]
    public static class ui_BottomLeft_Update_ShowOrHideText
    {
        static void Postfix()
        {
            if (!Main.enabled) return;

            ResourceMaintainer.ShowResourceIdealHoldingText();
        }
    }


    /// <summary>
    /// Patch: 禁止显示浮窗
    /// </summary>
    [HarmonyPatch(typeof(WindowManage), "LateUpdate")]
    public static class WindowManage_LateUpdate_ShowOrHideFloatWindow
    {
        static bool Prefix(WindowManage __instance)
        {
            if (!Main.enabled) return true;

            ResourceMaintainer.InterfereFloatWindow(__instance);

            return true;
        }
    }
}
