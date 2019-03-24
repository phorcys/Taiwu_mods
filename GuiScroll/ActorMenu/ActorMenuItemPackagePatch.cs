
using Harmony12;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GuiScroll
{
    public static class ActorMenuItemPackagePatch
    {
        public static int Key, Typ;
        private static NewItemPackage[] mm;
        public static NewItemPackage[] m_itemPackage
        {
            get
            {
                // Main.Logger.Log("获取 NewItem");
                if (mm == null || mm[0] == null)
                {
                    InitGuiUI();
                }
                // Main.Logger.Log("获取 NewItem = "+mm.ToString());
                return mm;
            }
            set
            {
                // Main.Logger.Log("设置 NewItem mm");
                mm = value;
            }
        }
        private static void InitGuiUI()
        {
            // Main.Logger.Log("初始化 NewItem mm begin");
            var t = ActorMenu.instance.itemsHolder;
            mm = new NewItemPackage[t.Length];
            for (int i = 0; i < t.Length; i++)
            {
                t[i].gameObject.SetActive(false);
                mm[i] = t[i].parent.parent.gameObject.AddComponent<NewItemPackage>();
                mm[i].Init();
            }
            // Main.Logger.Log("初始化 NewItem mm end");
        }
        public static int[] items_data;

        public static void UpdateItems()
        {
            m_itemPackage[Typ].data = items_data;
        }


        [HarmonyPatch(typeof(ActorMenu), "UpdateItems")]
        public static class ActorMenu_UpdateItems_Patch
        {
            public static bool Prefix(int key, int typ)
            {
                if (!Main.enabled)
                    return true;


                Key = key; Typ = typ;

                // Main.Logger.Log((new System.Diagnostics.StackTrace(true)).ToString());


                // Main.Logger.Log("UpdateItems 更新物品 key ：" + key + "      typ = " + typ);
                ActorMenu _this = ActorMenu.instance;

                _this.itemTyp = typ;
                _this.itemActorFace.SetActorFace(key);
                _this.itemActorAge.text = DateFile.instance.ShowActorAge(key);
                //int actorFavor = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), key, getLevel: false, realFavor: true);
                //Transform transform = _this.itemsHolder[typ].transform;
                int num = 0;
                //int childCount = transform.childCount;
                List<int> itemSort = DateFile.instance.GetItemSort(new List<int>(_this.GetActorItems(key).Keys));
                List<int> result = new List<int>();
                // Main.Logger.Log("开始循环");
                for (int i = 0; i < itemSort.Count; i++)
                {
                    int num2 = itemSort[i];
                    if (num2 != StorySystem.instance.useFoodId && (typ == 0 || typ == DateFile.instance.ParseInt(DateFile.instance.GetItemDate(num2, 4))))
                    {
                        num++;
                        result.Add(num2);
                    }
                }
                if (_this.isEnemy)
                {
                    _this.actorItemSize.text = DateFile.instance.SetColoer(20003, "- / -");
                }
                else
                {
                    int maxItemSize = _this.GetMaxItemSize(key);
                    int useItemSize = _this.GetUseItemSize(key);
                    _this.actorItemSize.text = string.Format("{0}{3}{1} / {2}</color>", _this.Color8(useItemSize, maxItemSize), ((float)useItemSize / 100f).ToString("f1"), ((float)maxItemSize / 100f).ToString("f1"), DateFile.instance.massageDate[807][2].Split('|')[0]);
                }

                // Main.Logger.Log("设置数据");
                items_data = result.ToArray();
                m_itemPackage[typ].data = items_data;
                return false;
            }
        }

        [HarmonyPatch(typeof(ActorMenu), "RemoveAllItems")]
        public static class ActorMenu_RemoveAllItems_Patch
        {
            public static bool Prefix()
            {
                if (!Main.enabled)
                    return true;

                // Main.Logger.Log("RemoveAllItems 清除所有物品");

                items_data = new int[0];
                foreach (var item in m_itemPackage)
                {
                    item.data = items_data;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(ActorMenu), "UpdateEquips")]
        public static class ActorMenu_UpdateEquips_Patch
        {
            public static void Postfix(int key, int typ)
            {
                if (!Main.enabled)
                    return;

                Transform parent = ActorMenu.instance.equipHolders[typ];

                for (int i = 0; i < parent.childCount; i++)
                {
                    Transform child = parent.GetChild(i);
                    Button btn = child.GetComponent<Button>();
                    if (!btn)
                    {
                        btn = child.gameObject.AddComponent<Button>();
                    }
                    var onclick = btn.onClick;
                    onclick.RemoveAllListeners();
                    onclick.AddListener(delegate { NewItemPackage.ClickItem(DateFile.instance.ParseInt(child.name.Split(',')[1]), child.GetComponent<SetItem>(), true); });
                }

                foreach (var child in ActorMenu.instance.equipIcons)
                {
                    Button btn = child.GetComponent<Button>();
                    if (!btn)
                    {
                        btn = child.gameObject.AddComponent<Button>();
                    }
                    var onclick = btn.onClick;
                    onclick.RemoveAllListeners();
                    onclick.AddListener(delegate { NewItemPackage.UnfixEquip(ActorMenu.instance.acotrId, DateFile.instance.ParseInt(child.name.Split(',')[1])); });
                }
            }
        }

    }

}