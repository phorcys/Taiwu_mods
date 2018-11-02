using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GuiBaseUI;

namespace GuiWarehouse
{
    public class NewWarehouse
    {
        static NewWarehouse instance;
        public static NewWarehouse Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NewWarehouse();
                }
                else if(instance.gameObject==null)
                {
                    instance.Init();
                }
                return instance;
            }
        }
        private NewWarehouse()
        {
            Main.Logger.Log("NewWarehouse 29");
            Init();
        }

        private void Init()
        {
            GameObject Canvas = CreateUI.NewCanvas();

            Main.Logger.Log("Init 35");
            RectTransform actorItemHolder = ((RectTransform)Warehouse.instance.actorItemHolder[1]);
            //Vector2 size = actorItemHolder.sizeDelta;
            Vector2 size = new Vector2(600, 600);
            gameObject = actorItemHolder.parent.gameObject;

            GameObject left = CreateUI.NewScrollView(size, BarType.Vertical, ContentType.VerticalLayout);
            left.transform.SetParent(Canvas.transform, false);
            Canvas canvas = left.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = GuiBaseUI.Main.MainCanvas.sortingOrder + 100;


            GameObject prefab = Warehouse.instance.warehouseItemIcon;
            RectTransform rPrefab = (RectTransform)prefab.transform;
            GameObject itemCell = new GameObject("itemCell", new Type[] { typeof(RectTransform),typeof(Image) });
            itemCell.layer = GuiBaseUI.Main.Layer;
            RectTransform rItemCell = (RectTransform)itemCell.transform;
            rItemCell.SetParent(actorItemHolder.parent,false);
            rItemCell.anchorMin = new Vector2(0.5f, 0.5f);
            rItemCell.anchorMax = new Vector2(0.5f, 0.5f);
            rItemCell.anchoredPosition = new Vector2(0, 0);
            rItemCell.pivot = new Vector2(0.5f, 0.5f);
            //rItemCell.sizeDelta = new Vector2(rPrefab.sizeDelta.x * 8 + 20, rPrefab.sizeDelta.y);
            rItemCell.sizeDelta = new Vector2(600, 600 / 8);
            HorizontalLayoutGroup hor = itemCell.AddComponent<HorizontalLayoutGroup>();
            hor.childControlHeight = false;
            hor.childControlWidth = false;
            hor.childForceExpandHeight = false;
            hor.childForceExpandWidth = false;
            hor.childAlignment = TextAnchor.UpperLeft;
            for (int i = 0; i < 8; i++)
            {
                GameObject go = UnityEngine.Object.Instantiate(prefab);
                go.transform.SetParent(rPrefab, false);
                RectTransform rect = (RectTransform)go.transform;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0, 0);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(600 / 8, 600 / 8);
                Main.Logger.Log("Init i " + i.ToString());
            }
            itemCell.SetActive(false);
            WarehouseItem warehouseItem = itemCell.AddComponent<WarehouseItem>();
            bigDataScroll = new BigDataScroll(left.GetComponent<ScrollRect>(), warehouseItem, SetData);
            //bigDataScroll.cellHeight = rPrefab.sizeDelta.y;
            bigDataScroll.cellHeight = 600 / 8;
            Main.Logger.Log("Init 69");

            //test(GuiBaseUI.Main.MainCanvas.transform,0);
        }

        //¥Ú”°UIΩ·ππ
        //public void test(Transform tf, int idx)
        //{
        //    string s = "";
        //    for (int i = 0; i < idx; i++)
        //    {
        //        s += "--";
        //    }
        //    s += tf.ToString();
        //    Main.Logger.Log(s);
        //    idx++;
        //    for (int i = 0; i < tf.childCount; i++)
        //    {
        //        Transform child = tf.GetChild(i);
        //        test(child, idx);
        //    }
        //}

        GameObject gameObject;
        BigDataScroll bigDataScroll;
        private List<int> m_data;
        public List<int> data { set
            {
                Main.Logger.Log("Init 2222");
                m_data = value;
                if (bigDataScroll != null)
                {
                    int count = m_data.Count / 8 + 1;
                    Main.Logger.Log(m_data.Count.ToString()+" Init count" +count.ToString());
                    bigDataScroll.cellCount = count;
                }
            }
            get
            {
                return m_data;
            }
        }

        private void SetData(ItemCell itemCell,int index)
        {
            Main.Logger.Log("SetData ItemCell " + index.ToString());
            bool actor = false;
            int num4 = -999;
            bool cantTake = HomeSystem.instance.homeMapPartId != DateFile.instance.mianPartId || HomeSystem.instance.homeMapPlaceId != DateFile.instance.mianPlaceId;
            WarehouseItem item = (WarehouseItem)itemCell;
            ChildData[] childDatas = item.childDatas;
            for (int i = 0; i < childDatas.Length; i++)
            {
                int idx = (index - 1) * 8 + index - 1;
                ChildData childData = childDatas[i];
                if (idx < m_data.Count)
                {
                    Main.Logger.Log("true"+ idx.ToString());
                    int num5 = m_data[idx];
                    GameObject go = childData.gameObject;
                    if (!go.activeSelf)
                    {
                        go.SetActive(true);
                    }
                    go.name = "WarehouseItem," + num5;
                    childData.setItem.SetWarehouseItemIcon(num4, num5, cantTake, Warehouse.instance.actorItemDes, 202);
                }
                else
                {
                    Main.Logger.Log("false" + idx.ToString());
                    GameObject go = childData.gameObject;
                    if (go.activeSelf)
                    {
                        go.SetActive(false);
                    }
                }
            }
        }

    }

}
