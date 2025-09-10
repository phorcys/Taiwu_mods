using Harmony12;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityModManagerNet;
using Characters = GameData.Characters;
using Text = UnityEngine.UI.Text;


namespace QuickSwap
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool IsCtrlBatchMoving = true;
        internal bool IsDoubleClick = true;
        internal bool IsAddEquipConfig = true;
        internal bool IsSimilarItemsMoving = true;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

    }
    public static class Main
    {

        public static bool enabled;
        public static Settings settings;

        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool binding_key = false;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }


        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            using (new GUILayout.VerticalScope("box"))
                settings.IsDoubleClick = GUILayout.Toggle(settings.IsDoubleClick, "双击装备、使用物品");

            using (new GUILayout.VerticalScope("box"))
                settings.IsCtrlBatchMoving = GUILayout.Toggle(settings.IsCtrlBatchMoving, "Ctrl+左键：在商店和仓库中批量移动物品，在物品栏中可以快速拆解物品");

            using (new GUILayout.VerticalScope("box"))
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                settings.IsSimilarItemsMoving = GUILayout.Toggle(settings.IsSimilarItemsMoving, "批量移动相同的物品");
            }

            using (new GUILayout.VerticalScope("box"))
                settings.IsAddEquipConfig = GUILayout.Toggle(settings.IsAddEquipConfig, "增加装备切换套餐");


        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }

    #region 参照泰拉瑞亚快捷键快速移动物品
    [HarmonyPatch(typeof(OnToggle), "ChangeShopItem")]
    public static class OnToggle_ChangeShopItem_Patch
    {
        static FieldInfo ShopSystem_wSellItems;
        static FieldInfo ShopSystem_sellItems;
        static FieldInfo ShopSystem_buyItems;
        static FieldInfo ShopSystem_itemChangeIng;
        private static bool Prefix(OnToggle __instance)
        {
            int itemId,
                typ,
                changeTyp,
                number,
                actorID
                ;
            Dictionary<int, int> itemDictionary;

            if (!Main.enabled || !Main.settings.IsCtrlBatchMoving)
                return true;
            if (!(Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl)))
                return true;

            var dfIns = DateFile.instance;

            itemId = int.Parse(__instance.name.Split(',')[1]);
            typ = int.Parse(__instance.name.Split(',')[2]);
            changeTyp = int.Parse(__instance.name.Split(',')[3]);
            number = -99;
            actorID = dfIns.MianActorID();

            //不批量移动同类物品
            if (!Main.settings.IsSimilarItemsMoving)
            {
                ShopSystem.instance.ChangeItem(itemId, typ, changeTyp, number);
                ShopSystem.instance.UpdateMoneyText();
                return false;
            }

            var basicId = (DateFile.instance.GetItemDate(itemId, 999));
            switch (typ)
            {
                case 0 when changeTyp != 0:
                    if (ShopSystem_wSellItems == null)
                        ShopSystem_wSellItems = AccessTools.Field(typeof(ShopSystem), "wSellItems");
                    itemDictionary = (Dictionary<int, int>)ShopSystem_wSellItems.GetValue(ShopSystem.instance);
                    break;
                case 0:
                    if (ShopSystem_sellItems == null)
                        ShopSystem_sellItems = AccessTools.Field(typeof(ShopSystem), "sellItems");
                    itemDictionary = (Dictionary<int, int>)ShopSystem_sellItems.GetValue(ShopSystem.instance);
                    break;
                case 1:
                    itemDictionary = changeTyp == 0 ? DateFile.instance.GetActorItems(actorID) : DateFile.instance.GetActorItems(-999);
                    break;
                case 2:
                    if (ShopSystem_buyItems == null)
                        ShopSystem_buyItems = AccessTools.Field(typeof(ShopSystem), "buyItems");
                    itemDictionary = (Dictionary<int, int>)ShopSystem_buyItems.GetValue(ShopSystem.instance);
                    break;
                case 3:
                    itemDictionary = ShopSystem.instance.shopItems;
                    break;
                default:
                    return true;
            }
            var items = itemDictionary.AsParallel()
                .Select(x => x.Key)
                .Where(x => dfIns.GetItemDate(x, 999) == basicId)
                .ToArray();

            if (ShopSystem_itemChangeIng == null)
                ShopSystem_itemChangeIng = AccessTools.Field(typeof(ShopSystem), "itemChangeIng");

            foreach (var x in items)
            {
                ShopSystem_itemChangeIng.SetValue(ShopSystem.instance, false);
                ShopSystem.instance.ChangeItem(x, typ, changeTyp, number);
            }
            ShopSystem.instance.UpdateMoneyText();
            return false;
        }
    }



    /// <summary>
    /// Ctrl+左键：仓库页面时为批量移动物品
    /// </summary>
    [HarmonyPatch(typeof(Warehouse), "ChangeItem")]
    public static class Warehouse_ChangeItem_Patch
    {
        static MethodInfo InitAllItem;
        private static bool Prefix(Warehouse __instance, ref string changTyp, ref int itemId, ref int number, bool ___itemChangeing, Toggle[] ___warehouseItemToggle, Toggle[] ___actorItemToggle)
        {
            if (!Main.enabled || !Main.settings.IsCtrlBatchMoving ||
                !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftControl)
                || ___itemChangeing || !HomeSystem.instance.PlaceHasWareHouse(DateFile.instance.mianPartId, DateFile.instance.mianPlaceId)
                )
                return true;

            int actorID;
            int getActorID;
            Toggle[] toggles;

            var dfIns = DateFile.instance;

            //不批量移动同类物品
            if (!Main.settings.IsSimilarItemsMoving)
            {
                actorID = changTyp == "ActorItem" ? dfIns.MianActorID() : -999;
                number = dfIns.GetItemNumber(actorID, itemId);
                return true;
            }

            ___itemChangeing = true;


            if (changTyp == "ActorItem")
            {
                actorID = dfIns.MianActorID();
                getActorID = -999;
                toggles = ___warehouseItemToggle;
            }
            else
            {
                actorID = -999;
                getActorID = dfIns.MianActorID();
                toggles = ___actorItemToggle;
            }

            if (!toggles[0].isOn)
                toggles[int.Parse(DateFile.instance.GetItemDate(itemId, 4))].isOn = true;

            //key:物品ID  value:数量
            var basicId = (DateFile.instance.GetItemDate(itemId, 999));
            var items = dfIns.actorItemsDate[actorID]
                .AsParallel()
                //.Select(x => x.Value)
                .Select(x => x.Key)
                .Where(x => dfIns.GetItemDate(x, 999) == basicId)
                .ToArray();
            foreach (var x in items)
            {
                var num = dfIns.GetItemNumber(actorID, x);
                //ChangeTwoActorItem 必须主线程运行
                DateFile.instance.ChangeTwoActorItem(actorID, getActorID, x, num);
            }

            //__instance.InitAllItem();
            if (InitAllItem == null)
                InitAllItem = typeof(Warehouse).GetMethod("InitAllItem", BindingFlags.NonPublic | BindingFlags.Instance);
            InitAllItem.Invoke(__instance, null);

            ___itemChangeing = false;
            return false;
        }
    }

    //物品栏
    [HarmonyPatch(typeof(DropObject), "OnEnable")]
    public static class AddStartComponentPatch
    {
        static void Postfix(DropObject __instance)
        {
            if (!Main.enabled || !Main.settings.IsDoubleClick)
                return;

            string pattern = @"^(ActorFaceDrop|RemoveAItemDrop|FixItemDrop)";
            //跳过人物头像,拆解,修理
            if (Regex.IsMatch(__instance.name, pattern))
                return;

            var doubleClickHandler = __instance.GetComponent<ItemDoubleClickHandler>();
            if (doubleClickHandler != null)
                return;

            doubleClickHandler = __instance.gameObject.AddComponent<ItemDoubleClickHandler>();
            doubleClickHandler.dropObject = __instance;
        }
    }

    //装备
    [HarmonyPatch(typeof(SetItem), "SetActorEquipIcon")]
    public static class SetItem_SetActorEquipIcon_Patch
    {
        static void Postfix(SetItem __instance, int itemId, int actorFavor)
        {
            if (!Main.enabled || !Main.settings.IsDoubleClick)
                return;

            var doubleClickHandler = __instance.GetComponent<EquipClickHandler>();
            if (doubleClickHandler != null)
                return;

            __instance.gameObject.AddComponent<EquipClickHandler>();
        }
    }


    public class ItemDoubleClickHandler : ClickHandler
    {
        public DropObject dropObject;

        public class ItemUsage
        {
            private int dropObjectTyp = -1;

            public ItemUsage(int dropObjectTyp, string dragDesType = null, bool isPoisoning = false)
            {
                IsPoisoning = isPoisoning;
                DropObjectTyp = dropObjectTyp;
                DragDesType = dragDesType;
            }

            /// <summary>
            /// FlowControl为false时，执行break
            /// </summary>
            public bool FlowControl { get; set; }
            public bool IsPoisoning { get; set; }
            public int DropObjectTyp { get => !FlowControl ? -1 : dropObjectTyp; set => dropObjectTyp = value; }
            public string DragDesType { get; set; }
        }

        protected override void OnDoubleClick(PointerEventData eventData) =>
            UseItem(eventData.pointerDrag.GetComponent<DragObject>(), eventData);
        protected override void OnCtrlClick(PointerEventData eventData) =>
            CtrlUseItem(eventData.pointerDrag.GetComponent<DragObject>(), eventData);


        private void CtrlUseItem(DragObject dragObject, PointerEventData eventData)
        {
            string dragDesType = "RemoveAItemDropBack,";
            //调用OnDrop实现
            OnDrop(dragObject, eventData, dragDesType, 10);
        }

        private void OnDrop(DragObject dragObject, PointerEventData eventData, string dragDesType, int dropObjectTyp)
        {
            for (int i = dragObject.dragDes.Count - 1; i > -1; i--)
            {
                if (!dragObject.dragDes[i].name.Contains(dragDesType))
                    continue;
                dropObject.dropObjectTyp = dropObjectTyp;
                dropObject.dropDesImage = dragObject.dragDes[i];
                dropObject.OnDrop(eventData);
                return;
            }
        }

        private void UseItem(DragObject dragObject, PointerEventData eventData, bool isLeftControl = false)
        {
            int itemId = int.Parse(eventData.pointerDrag.gameObject.name.Split(',')[1]);

            // 检查物品是否可以使用
            int itemType = int.Parse(DateFile.instance.GetItemDate(itemId, 4));
            int itemSubType = int.Parse(DateFile.instance.GetItemDate(itemId, 5));
            var audioId = 8;

            //判断方式：
            //      itemType+itemSubType        无法区分什么药
            //      dragObjectTyp               毒、解毒、制造、拆解混杂
            //      dragObject.dragDes[].name   可行，就是需要循环范围，消耗资源较高

            ItemUsage itemUsage = null;

            switch (itemType)
            {
                case 1 when itemSubType == 22://工具401

                    audioId = 6;
                    itemUsage = ShowFixWindow(itemId);
                    break;
                //内息药7    外伤药9     内伤药9     健康药3     解毒药
                case 2 when itemSubType == 31 && dragObject.dragObjectTyp != 2:
                    switch (dragObject.dragObjectTyp)
                    {
                        //外伤内伤药      需要先获取伤口列表，再判断可以用在哪个最重的伤口
                        case 8:
                            itemUsage = HpSpMedicine(dragObject, itemId);
                            break;
                        //内息药   
                        case 6:
                            itemUsage = ChangeMianQi(itemId);
                            break;
                        //健康药    源代码太长
                        case 5:
                            itemUsage = Longevity();
                            break;
                    }
                    break;

                //毒药,解毒,毒器
                case 2 when itemSubType == 30 && dragObject.dragObjectTyp == 2:
                case 2 when itemSubType == 31:// && dragObject.dragObjectTyp == 2:
                case 4 when itemSubType == 39:

                    itemUsage = ChangePoisons(itemId);
                    break;

                //卸下装备  
                //宝物    鞋子    促织    代步
                case 0 when itemSubType == 0:
                //武器    冠饰    护甲    衣着
                case 5 when itemSubType == 20:
                    itemUsage = Unwield(eventData, dragObject.name, itemId);
                    break;
                default:
                    return;
            }
            if (itemUsage?.FlowControl == false)
                return;
            if (!string.IsNullOrEmpty(itemUsage?.DragDesType))
            {
                OnDrop(dragObject, eventData, itemUsage.DragDesType, itemUsage.DropObjectTyp);
                return;
            }
            //参毒检查
            if (itemUsage?.IsPoisoning == true)
            {
                DateFile.instance.SetItemPoison(ActorMenu.instance.actorId, itemId);
                DateFile.instance.ChangeItemHp(ActorMenu.instance.actorId, itemId, -1);
            }
            UseItemEnd(audioId, itemUsage.DropObjectTyp);

        }

        //健康药
        private ItemUsage Longevity()
        {
            ItemUsage itemUsage = new ItemUsage(3, "ActorFaceDropBack");
            int he1 = DateFile.instance.Health(ActorMenu.instance.actorId);
            int he2 = DateFile.instance.MaxHealth(ActorMenu.instance.actorId);
            itemUsage.FlowControl = he1 != he2;
            if (!itemUsage.FlowControl)
                return itemUsage;

            return itemUsage;
        }

        //内息药
        private static ItemUsage ChangeMianQi(int itemId)
        {
            //额外判断内息是否为0
            ItemUsage itemUsage = new ItemUsage(7, isPoisoning: true);
            itemUsage.FlowControl = DateFile.instance.GetActorMianQi(ActorMenu.instance.actorId) > 0;
            if (!itemUsage.FlowControl)
                return itemUsage;
            DateFile.instance.ChangeMianQi(ActorMenu.instance.actorId, int.Parse(DateFile.instance.GetItemDate(itemId, 39)) * 10);
            return itemUsage;
        }

        //卸下装备
        private static ItemUsage Unwield(PointerEventData eventData, string name, int itemId)
        {
            ItemUsage itemUsage = new ItemUsage(1);

            if (ActorMenu.instance.isEnemy || DateFile.instance.teachingOpening != 0)
                return itemUsage;

            string pattern = @"^(Weapon[1-3]Drop|ShoesDrop|HeadDrop|ArmorDrop|CricketDrop|ClothesDrop|CarDrop|Treasure[1-3]Drop)";
            var typeRegex = Regex.Match(name, pattern);
            if (!typeRegex.Success)
                return itemUsage;
            //忘了干嘛用
            var actorDate = int.Parse(DateFile.instance.GetActorDate(ActorMenu.instance.actorId, itemId));
            itemUsage.FlowControl = actorDate != 0;
            if (!itemUsage.FlowControl)
                return itemUsage;

            int id1 = int.Parse(DateFile.instance.GetActorDate(ActorMenu.instance.actorId, int.Parse(eventData.pointerDrag.gameObject.name.Split(',')[1]), false));
            if (!ActorMenu.instance.equipToggles[0].isOn)
                ActorMenu.instance.equipToggles[int.Parse(DateFile.instance.GetItemDate(id1, 1))].isOn = true;
            DateFile.instance.SetActorEquip(ActorMenu.instance.actorId, int.Parse(eventData.pointerDrag.gameObject.name.Split(',')[1]), 0);
            ActorMenu.instance.UpdateActorListFace();
            WorldMapSystem.instance.UpdateMovePath(WorldMapSystem.instance.targetPlaceId);
            if (DateFile.instance.battleStart && Game.Instance.GetCurrentGameStateName() == eGameState.Battle)
                StartBattle.instance.UpdateBattlerFace();
            GEvent.OnEvent(eEvents.UpdateActorFace, true);
            return itemUsage;
        }

        //外伤内伤药
        private static ItemUsage HpSpMedicine(DragObject dragObject, int itemId)
        {
            ItemUsage itemUsage = new ItemUsage(9, isPoisoning: true);
            var injurys = dragObject.dragDes
                   .Where(item => item.name.StartsWith("injury,"))
                   .Select(x => int.Parse(x.name.Split(',')[1]))
                   .ToList();

            //筛选外伤内伤，伤口大小排序
            var sortedByValue = DateFile.instance.actorInjuryDate[ActorMenu.instance.actorId]
                //1为外伤，2为内伤
                //.Where(x => int.Parse(DateFile.instance.injuryDate[x.Key][1]) > 0)
                .Where(x => injurys.Contains(x.Key))
                .OrderByDescending((i) => i.Value)
                .ToList();

            itemUsage.FlowControl = injurys.Count > 0 && DateFile.instance.actorInjuryDate[ActorMenu.instance.actorId].Count != 0 && sortedByValue.Count() > 0;
            if (!itemUsage.FlowControl)
                return itemUsage;

            //获取药效
            int injury = Mathf.Abs(int.Parse(DateFile.instance.GetItemDate(itemId, 11)));
            if (injury < 1)
                injury = Mathf.Abs(int.Parse(DateFile.instance.GetItemDate(itemId, 12)));

            var injury_3 = injury * 3;
            var injuryId = 0;
            var isInjuryMax = true;

            foreach (var item in sortedByValue)
            {
                //最大可治愈伤口
                injuryId = item.Key;
                if (item.Value <= injury_3)
                {
                    isInjuryMax = false;
                    break;
                }
                //迭代到最小可治愈伤口
            }
            if (isInjuryMax)
                injury /= 5;

            DateFile.instance.RemoveInjury(ActorMenu.instance.actorId, injuryId, -injury);

            return itemUsage;
        }

        //施毒/解毒
        private ItemUsage ChangePoisons(int itemId)
        {
            ItemUsage itemUsage = new ItemUsage(4)
            {
                FlowControl = true
            };
            for (int typ = 0; typ < 6; ++typ)
            {
                ChangePoison(itemId, 61 + typ, typ, 5);

                var multiple = int.Parse(DateFile.instance.GetItemDate(itemId, 4)) == 4 ? 1 : 10;
                ChangePoison(itemId, 71 + typ, typ, multiple);
            }
            //可能是除蛊的
            int featureId = int.Parse(DateFile.instance.GetItemDate(itemId, 67));
            if (featureId > 0)
            {
                List<int> actorFeature = DateFile.instance.GetActorFeature(ActorMenu.instance.actorId);
                int num17 = int.Parse(DateFile.instance.GetItemDate(itemId, 8)) * 5;
                if (UnityEngine.Random.Range(0, 100) < num17)
                {
                    if (actorFeature.Contains(featureId))
                        DateFile.instance.RemoveActorFeature(ActorMenu.instance.actorId, featureId);
                    if (actorFeature.Contains(featureId + 1))
                        DateFile.instance.RemoveActorFeature(ActorMenu.instance.actorId, featureId + 1);
                }
            }
            if (int.Parse(DateFile.instance.GetItemDate(itemId, 6)) == 1)
            {
                DateFile.instance.LoseItem(ActorMenu.instance.actorId, itemId, 1, true, loseType: 1);
                return itemUsage;
            }
            DateFile.instance.ChangeItemHp(ActorMenu.instance.actorId, itemId, -1);
            return itemUsage;
        }

        private void ChangePoison(int itemId, int index, int typ, int multiple)
        {
            int volume = int.Parse(DateFile.instance.GetItemDate(itemId, index));
            if (volume != 0)
                DateFile.instance.ChangePoison(ActorMenu.instance.actorId, typ, volume * multiple);
        }

        //修理物品
        private ItemUsage ShowFixWindow(int itemId)
        {
            ItemUsage itemUsage = new ItemUsage(401);

            itemUsage.FlowControl = ActorMenu.instance.actorId == DateFile.instance.MianActorID();
            if (!itemUsage.FlowControl)
                return itemUsage;

            MakeSystem.instance.ShowFixWindow(int.Parse(DateFile.instance.GetItemDate(itemId, 41)));
            return itemUsage;

        }
    }

    public class EquipClickHandler : ClickHandler
    {
        protected override void OnDoubleClick(PointerEventData eventData)
        {
            var patternItems = new List<string>{
                "EquipWeapon1", "EquipWeapon2", "EquipWeapon3",
                "EquipHead","EquipClothes", "EquipArmor","EquipShoes",
                "EquipTreasure1","EquipTreasure2", "EquipTreasure3",
                "EquipCar","EquipCricket",
            };
            //string pattern = @"^(EquipWeapon[1-3]|EquipHead|EquipClothes|EquipArmor|EquipShoes|EquipTreasure[1-3]|EquipCar|EquipCricket)";

            var dragObject = eventData.pointerDrag.GetComponent<DragObject>();
            var equipments = new List<int>();
            string dragDesType = string.Empty;
            var equipIndex = -1;

            foreach (var item in dragObject.dragDes)
            {
                var index = patternItems.IndexOf(item.name);
                if (index == -1)
                    continue;
                equipments.Add(index);
            }
            //不是装备跳过
            if (equipments.Count == 0)
                return;

            foreach (var index in equipments)
            {
                if ((index < 3 || (index > 6 && index < 10)) && Characters.GetCharProperty(ActorMenu.instance.actorId, 301 + index) != "0")
                    continue;
                //3 4 5 6   10 11不做判断
                equipIndex = index;
                dragDesType = patternItems[index];
                break;
            }
            //没有空位跳过
            if (string.IsNullOrEmpty(dragDesType))
                return;

            Equip(eventData, 301 + equipIndex);

            UseItemEnd(8, 0);
        }

        private static void Equip(PointerEventData eventData, int equipIndex)
        {
            int itemId = int.Parse(eventData.pointerDrag.gameObject.name.Split(',')[1]);

            if (eventData.pointerDrag.gameObject.name.Split(',').Length > 2)
            {
                int num3 = int.Parse(DateFile.instance.GetActorDate(ActorMenu.instance.actorId, itemId, false));
                int num4 = int.Parse(DateFile.instance.GetActorDate(ActorMenu.instance.actorId, itemId, false));
                //未验证
                Characters.SetCharProperty(ActorMenu.instance.actorId, int.Parse(eventData.pointerDrag.gameObject.name.Split(',')[2]), num3.ToString());
                Characters.SetCharProperty(ActorMenu.instance.actorId, equipIndex, num4.ToString());
            }
            else
                DateFile.instance.SetActorEquip(ActorMenu.instance.actorId, equipIndex, itemId);
            if (DateFile.instance.teachingOpening == 101)
            {
                DateFile.instance.teachingOpening = 102;
                Teaching.instance.RemoveTeachingWindow(2);
                Teaching.instance.RemoveTeachingWindow(4);
                Teaching.instance.SetTeachingWindow(5);
            }
            ActorMenu.instance.UpdateActorListFace();
            WorldMapSystem.instance.UpdateMovePath(WorldMapSystem.instance.targetPlaceId);
            GEvent.OnEvent(eEvents.UpdateActorFace, true);
        }

        protected override void OnCtrlClick(PointerEventData eventData) { }
    }

    public abstract class ClickHandler : MonoBehaviour, IPointerClickHandler
    {
        public float doubleClickThreshold = 0.3f;
        private float lastClickTime = 0;
        private int _clickCount = 0;


        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Main.enabled || !Main.settings.IsDoubleClick)
                return;


            //Ctrl+左键
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (!Main.settings.IsCtrlBatchMoving)
                    return;
                OnCtrlClick(eventData);
            }
            float time = Time.time;
            // 如果距离上次点击时间超过阈值，重置计数器
            if (Time.time - lastClickTime > doubleClickThreshold)
                _clickCount = 0;

            lastClickTime = Time.time;

            if (++_clickCount < 2)
                return;

            _clickCount = 0;

            OnDoubleClick(eventData);
        }

        /// <summary>
        /// 双击
        /// </summary>
        /// <param name="eventData"></param>
        protected abstract void OnDoubleClick(PointerEventData eventData);

        /// <summary>
        /// Crtl+左键
        /// </summary>
        /// <param name="eventData"></param>
        protected abstract void OnCtrlClick(PointerEventData eventData);

        public static void UseItemEnd(int audioId, int dropObjectTyp)
        {
            AudioManager.instance.PlayButtonSE(audioId);
            WindowManage.instance.WindowSwitch(false);
            DropUpdate.instance.updateId = dropObjectTyp;
        }
    }

    #endregion

    #region 增加装备切换

    [HarmonyPatch(typeof(DateFile), "CheckMainActorEquipConfig")]
    public static class GongFaEditorPatch
    {
        // 后置补丁函数 
        static void Postfix(DateFile __instance)
        {
            if (!Main.enabled)//|| !Main.settings.IsAddEquipConfig)
                return;

            // 首次运行时将装备切换配置加到10个
            while (__instance.mainActorequipConfig.Count < 10)
            {
                Dictionary<int, int> newConfig = new Dictionary<int, int>();
                for (int index = 0; index < 12; ++index)
                {
                    newConfig.Add(301 + index, 0);
                }
                __instance.mainActorequipConfig.Add(newConfig);
            }
        }
    }

    // Harmony补丁 - 在ActorEquipGroup.Start方法中添加额外的切换按钮
    [HarmonyPatch(typeof(ActorEquipGroup), "Start")]
    public static class ActorEquipGroupStartPatch
    {
        // 后置补丁函数
        static void Postfix(ActorEquipGroup __instance, CToggleGroup ___toggleGroup)
        {
            if (!Main.enabled || !Main.settings.IsAddEquipConfig)
                return;
            AddExtraToggleButtons(__instance);
            if (DateFile.instance.nowEquipConfigIndex > 2)
            {
                ___toggleGroup.Get(0).isOn = false;
                ___toggleGroup.Set(DateFile.instance.nowEquipConfigIndex);
            }

        }

        // 添加额外的切换按钮
        static void AddExtraToggleButtons(ActorEquipGroup actorEquipGroup)
        {
            // 获取父级容器用于放置新按钮 - 使用ActorEquipGroup的transform作为父容器
            Transform parent = actorEquipGroup.transform;

            if (parent.childCount > 8)
                return;

            // 获取CToggleGroup组件
            CToggleGroup toggleGroup = actorEquipGroup.GetComponent<CToggleGroup>();
            var toggle = parent.GetChild(0).GetComponent<CToggle>();

            // 定义额外按钮的文本（肆到玖）
            string[] extraToggleNames = { "肆", "伍", "陆", "柒", "捌", "玖" };

            // 为每个额外配置创建按钮
            for (int i = 0; i < extraToggleNames.Length; i++)
            {
                int configIndex = i + 3;

                // 检查是否已经存在该配置的按钮
                if (toggleGroup.Get(configIndex) != null)
                    continue;

                CToggle tog = Object.Instantiate(toggle, Vector3.zero, Quaternion.identity);

                tog.key = configIndex;
                tog.transform.rotation = Quaternion.Euler(0, 0, 45f);
                tog.transform.SetParent(parent, false);

                // 注册到切换组
                tog.Register(toggleGroup);

                // 添加到切换组
                toggleGroup.Add(tog);

                actorEquipGroup.StartCoroutine(SetTextAfterCToggleInit(tog, extraToggleNames[i]));
            }

            EquipToggleRearrange(toggleGroup);
        }
        static IEnumerator SetTextAfterCToggleInit(Toggle toggle, string text)
        {
            // 等待一帧，确保CToggle已经完成初始化
            yield return null;

            Text textComponent = toggle.GetComponentInChildren<Text>();
            textComponent.text = text;
        }

        private static void EquipToggleRearrange(CToggleGroup toggleGroup)
        {
            // 获取现有的按钮数量
            int existingButtonCount = 0;

            //根据计算应该是29，但实际观察到的偏移是29 - 4
            const float startX = 29 - 4;

            // 根据原图观察，按钮之间间隔约58单位
            const float spacing = 58f;

            // 遍历现有的CToggle组件，重新排列位置
            foreach (CToggle toggle in toggleGroup.GetComponentsInChildren<CToggle>())
            {
                RectTransform rectTransform = toggle.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(startX + (existingButtonCount++ * spacing), -34.55f);
            }
        }
    }
    #endregion
}
