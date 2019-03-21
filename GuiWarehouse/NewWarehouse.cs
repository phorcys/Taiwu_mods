using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GuiBaseUI;
using System.Linq;

namespace GuiWarehouse
{
    public class NewWarehouse:MonoBehaviour
    {
        private bool actor;
        private int typ;
        BigDataScroll bigDataScroll;
        public ScrollRect scrollRect;
        private int[] m_data;
        public int[] data
        {
            set
            {
                m_data = value;
                SetData();
            }
            get
            {
                return m_data;
            }
        }
        public bool setClick = false;
        public bool isInit = false;


        public void Init(bool actor,int typ)
        {
            Main.Logger.Log("NewWarehouse Init " + actor.ToString());
            this.actor = actor;
            this.typ = typ;
            InitUI();
        }

        private void InitUI()
        {
            isInit = true;;

            Vector2 size = new Vector2(650, 650);
            Vector2 leftPos = actor ? new Vector2(-size.x * 0f, 0) : new Vector2(size.x * 0f, 0);

            GameObject scrollView = CreateUI.NewScrollView(size, BarType.Vertical, ContentType.VerticalLayout);
            scrollRect = scrollView.GetComponent<ScrollRect>();
            scrollRect.verticalNormalizedPosition = 1;
            Image imgScrollView = scrollView.GetComponentInChildren<Image>();
            imgScrollView.color = new Color(0.5f, 0.5f, 0.5f, 0.005f);
            imgScrollView.raycastTarget = false;
            RectTransform rScrollView = ((RectTransform)scrollView.transform);
            rScrollView.SetParent(gameObject.transform, false);
            rScrollView.anchoredPosition = leftPos;

            //scrollView.GetComponentInChildren<Mask>().enabled = false;

            GameObject image = new GameObject("line", new System.Type[] { typeof(RectTransform) });
            RectTransform rItemCell = image.GetComponent<RectTransform>();
            rItemCell.SetParent(transform, false);
            rItemCell.anchoredPosition = new Vector2(10000, 10000);


            GameObject prefab = Warehouse.instance.warehouseItemIcon;


            for (int i = 0; i < Main.settings.numberOfColumns; i++)
            {
                GameObject go = UnityEngine.Object.Instantiate(prefab);
                go.transform.SetParent(rItemCell, false);
            }


            rItemCell.sizeDelta = new Vector2(size.x, size.x / Main.settings.numberOfColumns);
            GridLayoutGroup gridLayoutGroup = image.AddComponent<GridLayoutGroup>();
            gridLayoutGroup.cellSize = size / Main.settings.numberOfColumns * 0.9f;
            gridLayoutGroup.spacing = 0.05f * size / Main.settings.numberOfColumns;
            gridLayoutGroup.padding.left = (int)(size.x / Main.settings.numberOfColumns * 0.05f);
            gridLayoutGroup.padding.top = (int)(size.x / Main.settings.numberOfColumns * 0.05f);


            WarehouseItem itemCell = image.AddComponent<WarehouseItem>();
            bigDataScroll = gameObject.AddComponent<BigDataScroll>();
            bigDataScroll.Init(scrollRect, itemCell, SetCell);
            bigDataScroll.cellHeight = size.x / Main.settings.numberOfColumns;

            Transform parent = transform.parent;
            ScrollRect scroll = parent.GetComponent<ScrollRect>();
            if (scroll != null)
            {
                if (scroll.verticalScrollbar != null)
                {
                    Image other = scroll.verticalScrollbar.GetComponent<Image>();
                    if (other != null)
                    {
                        RectTransform rect = (RectTransform)rScrollView.Find("ScrollbarVertical");
                        //rect.sizeDelta = new Vector2(10, 0);
                        Image my = rect.GetComponent<Image>();
                        //my.color = new Color(10.9490196f, 0.509803951f, 0.203921571f);
                        my.sprite = other.sprite;
                        my.type = Image.Type.Sliced;
                    }
                    else
                    {
                        Main.Logger.Log("没找到 ScrollbarVertical Image");
                    }

                    if (scroll.verticalScrollbar.targetGraphic != null)
                    {
                        Image other2 = scroll.verticalScrollbar.targetGraphic.GetComponent<Image>();
                        if (other != null)
                        {
                            RectTransform rect = (RectTransform)rScrollView.Find("ScrollbarVertical/SlidingArea/Handle");
                            //rect.sizeDelta = new Vector2(10, 10);
                            Image my = rect.GetComponent<Image>();
                            //my.color = new Color(0.3882353f, 0.807843149f, 0.8156863f);
                            my.sprite = other2.sprite;
                            my.type = Image.Type.Sliced;
                        }
                        else
                        {
                            Main.Logger.Log("没找到 Handle Image");
                        }
                    }
                    else
                    {
                        Main.Logger.Log("没找到 Handle");
                    }
                }
            }
            else
            {
                Main.Logger.Log("没找到 ScrollbarVertical");
            }

            SetData();
        }

        private void SetData()
        {
            if (bigDataScroll != null&& m_data!=null&& isInit)
            {
                int count = m_data.Length / Main.settings.numberOfColumns + 1;
                bigDataScroll.cellCount = count;
                if (!Main.OnChangeItem)
                {
                    scrollRect.verticalNormalizedPosition = 1;
                }
            }
        }

        private void SetCell(ItemCell itemCell, int index)
        {
            if (actor)
            {
                int num2 = DateFile.instance.MianActorID();
                bool flag = !Main.settings.remoteWarehouse;
                if (flag)
                {
                    flag = HomeSystem.instance.homeMapPartId != DateFile.instance.mianPartId || HomeSystem.instance.homeMapPlaceId != DateFile.instance.mianPlaceId;
                }
                WarehouseItem item = itemCell as WarehouseItem;
                if (item == null)
                {
                    Main.Logger.Log("WarehouseItem出错。。。");
                    return;
                }
                ChildData[] childDatas = item.childDatas;
                for (int i = 0; i < Main.settings.numberOfColumns; i++)
                {
                    int idx = (index - 1) * Main.settings.numberOfColumns + i;
                    if (i < childDatas.Length)
                    {
                        ChildData childData = childDatas[i];
                        if (idx < m_data.Length)
                        {
                            int num3 = m_data[idx];
                            GameObject go = childData.gameObject;
                            if (!go.activeSelf)
                            {
                                go.SetActive(true);
                            }
                            go.name = "ActorItem," + num3;
                            //childData.setItem.SetWarehouseItemIcon(num2, num3, int.Parse(DateFile.instance.GetItemDate(num3, 3, true)) != 1 || flag);
                            childData.setItem.SetWarehouseItemIcon(num2, num3, (DateFile.instance.ParseInt(
                                DateFile.instance.GetItemDate(num3, 3)) != 1 
                                || DateFile.instance.ParseInt(DateFile.instance.GetItemDate(num3, 5)) == 42) | flag, Warehouse.instance.warehouseItemDes, 201);
                        }
                        else
                        {
                            GameObject go = childData.gameObject;
                            if (go.activeSelf)
                            {
                                go.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        Main.Logger.Log("数据出错。。。");
                    }
                }
            }
            else
            {
                int num4 = -999;
                bool cantTake = !Main.settings.remoteWarehouse;
                if (cantTake)
                {
                    cantTake = HomeSystem.instance.homeMapPartId != DateFile.instance.mianPartId || HomeSystem.instance.homeMapPlaceId != DateFile.instance.mianPlaceId;
                }
                WarehouseItem item = itemCell as WarehouseItem;
                if (item == null)
                {
                    Main.Logger.Log("WarehouseItem出错。。。");
                    return;
                }
                ChildData[] childDatas = item.childDatas;
                for (int i = 0; i < Main.settings.numberOfColumns; i++)
                {
                    int idx = (index - 1) * Main.settings.numberOfColumns  + i;
                    if (i < childDatas.Length)
                    {
                        ChildData childData = childDatas[i];
                        if (idx < m_data.Length)
                        {
                            int num5 = m_data[idx];
                            GameObject go = childData.gameObject;
                            if (!go.activeSelf)
                            {
                                go.SetActive(true);
                            }
                            go.name = "WarehouseItem," + num5;
                            childData.setItem.SetWarehouseItemIcon(num4, num5, cantTake);
                        }
                        else
                        {
                            GameObject go = childData.gameObject;
                            if (go.activeSelf)
                            {
                                go.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        Main.Logger.Log("数据出错。。。");
                    }
                }
            }
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy | m_data == null | scrollRect == null)
            {
                return;
            }
            var mousePosition = Input.mousePosition;
            var mouseOnPackage = mousePosition.x > Screen.width / 2;

            var v = Input.GetAxis("Mouse ScrollWheel");
            if (v != 0)
            {
                if (mouseOnPackage == actor)
                {
                    float count = m_data.Length / Main.settings.numberOfColumns + 1;
                    scrollRect.verticalNormalizedPosition += v / count * Main.settings.scrollSpeed;
                }
            }
        }
    }

}
