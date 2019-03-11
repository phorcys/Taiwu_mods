
using Harmony12;
using System.Collections.Generic;
using UnityEngine;

namespace GuiScroll
{
    public static class ActorMenuItemPackagePatch
    {
        public static int Key, Typ;
        public static NewItemPackage[] m_itemPackage;
        private static void InitGuiUI()
        {

            var t = ActorMenu.instance.itemsHolder;
            m_itemPackage = new NewItemPackage[t.Length];
            for (int i = 0; i < t.Length; i++)
            {
                t[i].gameObject.SetActive(false);
                m_itemPackage[i] = t[i].parent.parent.gameObject.AddComponent<NewItemPackage>();
                m_itemPackage[i].Init();
            }


        }

        [HarmonyPatch(typeof(ActorMenu), "UpdateItems")]
        public static class ActorMenu_UpdateItems_Patch
        {
            public static bool Prefix(int key, int typ)
            {
                if (!Main.enabled)
                    return true;


                Key = key; Typ = typ;

                if (m_itemPackage == null)
                {
                    InitGuiUI();
                }



                Main.Logger.Log((new System.Diagnostics.StackTrace(true)).ToString());


                Main.Logger.Log("UpdateItems 更新物品 key ：" + key + "      typ = " + typ);
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
                Main.Logger.Log("开始循环");
                for (int i = 0; i < itemSort.Count; i++)
                {
                    int num2 = itemSort[i];
                    if (num2 != StorySystem.instance.useFoodId && (typ == 0 || typ == DateFile.instance.ParseInt(DateFile.instance.GetItemDate(num2, 4))))
                    {
                        num++;
                        result.Add(num2);
                        //GameObject gameObject = null;
                        //if (childCount >= num)
                        //{
                        //    gameObject = transform.GetChild(num - 1).gameObject;
                        //    gameObject.SetActive(value: true);
                        //}
                        //else
                        //{
                        //    gameObject = UnityEngine.Object.Instantiate(_this.itemIconNoToggle, Vector3.zero, Quaternion.identity);
                        //    gameObject.transform.SetParent(transform, worldPositionStays: false);
                        //}
                        //gameObject.name = "Item," + num2;
                        //gameObject.GetComponent<SetItem>().SetActorMenuItemIcon(key, num2, actorFavor, _this.injuryTyp);
                    }
                }
                //for (int j = 0; j < transform.childCount; j++)
                //{
                //    GameObject gameObject2 = transform.GetChild(j).gameObject;
                //    if (gameObject2 != null && gameObject2.activeSelf)
                //    {
                //        gameObject2.GetComponent<SetItem>().SetItemAdd(key, DateFile.instance.ParseInt(gameObject2.name.Split(',')[1]), transform);
                //    }
                //}
                //if (childCount > num)
                //{
                //    for (int k = num; k < childCount; k++)
                //    {
                //        transform.GetChild(k).gameObject.SetActive(value: false);
                //    }
                //}
                Main.Logger.Log("完成循环");
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

                Main.Logger.Log("设置数据");
                m_itemPackage[typ].data = result.ToArray();
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

                Main.Logger.Log("RemoveAllItems 清除所有物品");

                foreach (var item in m_itemPackage)
                {
                    item.data = new int[0];
                }
                return false;
            }
        }

    }

}