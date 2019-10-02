using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GuiBaseUI;
using System.Linq;

namespace Sth4nothing.UseStorageBook
{
    public class NewBookView : MonoBehaviour
    {
        public static readonly int columns = 9;
        BigDataScroll bigDataScroll;
        public ScrollRect scrollRect;

        public BookCell bookCell;
        private int[] data;
        public int[] Data
        {
            set
            {
                data = value;
                SetData();
            }
            get
            {
                return data;
            }
        }
        public bool hasInit = false;


        public void Init()
        {
            Main.Logger.Log("NewBookHolder Init");
            InitUI();
        }

        private void InitUI()
        {
            hasInit = true;

            Vector2 size = new Vector2(740, 740);

            GameObject scrollView = CreateUI.NewScrollView(size, BarType.Vertical, ContentType.VerticalLayout);
            scrollRect = scrollView.GetComponent<ScrollRect>();
            scrollRect.verticalNormalizedPosition = 1;
            Image imgScrollView = scrollView.GetComponentInChildren<Image>();
            imgScrollView.color = new Color(0.5f, 0.5f, 0.5f, 0.005f);
            imgScrollView.raycastTarget = false;
            RectTransform rScrollView = scrollView.GetComponent<RectTransform>();
            rScrollView.SetParent(gameObject.transform, false);
            rScrollView.anchoredPosition = Vector2.zero;

            GameObject image = new GameObject("line", new System.Type[] { typeof(RectTransform) });
            RectTransform rItemCell = image.GetComponent<RectTransform>();
            rItemCell.SetParent(transform, false);
            rItemCell.anchoredPosition = new Vector2(10000, 10000);

            GameObject prefab = BuildingWindow.instance.bookIcon;

            for (int i = 0; i < columns; i++)
            {
                GameObject go = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
                go.transform.SetParent(rItemCell, false);
            }

            rItemCell.sizeDelta = new Vector2(size.x, size.x / columns + 35f);
            GridLayoutGroup gridLayoutGroup = image.AddComponent<GridLayoutGroup>();
            gridLayoutGroup.cellSize = size / columns * 0.9f;
            gridLayoutGroup.spacing = 0.05f * size / columns;
            gridLayoutGroup.padding.left = (int)(size.x / columns * 0.05f);
            gridLayoutGroup.padding.top = (int)(size.x / columns * 0.05f);

            bookCell = image.AddComponent<BookCell>();
            bigDataScroll = gameObject.AddComponent<BigDataScroll>();
            bigDataScroll.Init(scrollRect, bookCell, SetCell);
            bigDataScroll.cellHeight = size.x / columns + 35f;

            ScrollRect scroll = BuildingWindow.instance.bookHolder.parent.parent.GetComponent<ScrollRect>();
            if (scroll != null)
            {
                if (scroll.verticalScrollbar != null)
                {
                    Image other = scroll.verticalScrollbar.GetComponent<Image>();
                    if (other != null)
                    {
                        RectTransform rect = (RectTransform)rScrollView.Find("ScrollbarVertical");
                        Image my = rect.GetComponent<Image>();
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
                            Image my = rect.GetComponent<Image>();
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
            if (bigDataScroll != null && data != null && hasInit)
            {
                int count = data.Length / columns + 1;
                bigDataScroll.cellCount = count;
                scrollRect.verticalNormalizedPosition = 1;
            }
        }

        private void SetCell(ItemCell itemCell, int index)
        {
            BookCell item = itemCell as BookCell;
            if (item == null)
            {
                Main.Logger.Log("WarehouseItem出错。。。");
                return;
            }
            ChildData[] childDatas = item.childDatas;
            for (int i = 0; i < columns; i++)
            {
                int idx = (index - 1) * columns + i;
                if (i < childDatas.Length)
                {
                    ChildData childData = childDatas[i];
                    if (idx < data.Length)
                    {
                        int bookId = data[idx];
                        GameObject go = childData.gameObject;
                        if (!go.activeSelf)
                        {
                            go.SetActive(true);
                        }
                        childData.setBook.SetBookId(bookId);
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

        private void Update()
        {
            if (!gameObject.activeInHierarchy | data == null | scrollRect == null)
            {
                return;
            }
            var mousePosition = Input.mousePosition;

            var v = Input.GetAxis("Mouse ScrollWheel");
            if (v != 0)
            {
                float count = data.Length / columns + 1;
                scrollRect.verticalNormalizedPosition += v / count * Main.Setting.scrollSpeed;
            }
        }
    }

}
