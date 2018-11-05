using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GuiBaseUI;
using System.Linq;

namespace GuiWorldNpc
{
    public class NewWorldNpc:MonoBehaviour
    {
        private int partId;
        private int placeId;
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
        public bool isInit = false;


        //脚本附带ActorHolder上面
        public void Init(int partId, int placeId)
        {
            Main.Logger.Log("初始化新世界NPC " + partId.ToString()+" "+ placeId.ToString());
            this.partId = partId;
            this.placeId = placeId;
            InitUI();
        }
        private void InitUI()
        {
            isInit = true;

            Vector2 size = new Vector2(200, 640);
            Vector2 pos = new Vector2(0, 0);
            Vector2 cellSize = new Vector2(165.0f, 78.0f);

            
            GameObject scrollView = transform.parent.gameObject;//GameObject scrollView = CreateUI.NewScrollView(size, BarType.Vertical, ContentType.VerticalLayout);
            scrollRect = scrollView.GetComponent<ScrollRect>();
            scrollRect.verticalNormalizedPosition = 1;
            Image imgScrollView = scrollView.GetComponentInChildren<Image>();
            imgScrollView.color = new Color(0.5f, 0.5f, 0.5f, 0.005f);
            imgScrollView.raycastTarget = false;
            RectTransform rScrollView = ((RectTransform)scrollView.transform);
            //rScrollView.SetParent(gameObject.transform, false);
            //rScrollView.anchoredPosition = pos;

            scrollView.GetComponentInChildren<Mask>().enabled = false;

            
            GameObject gItemCell = WorldMapSystem.instance.actorIcon;//GameObject gItemCell = new GameObject("line", new System.Type[] { typeof(RectTransform) });
            RectTransform rItemCell = gItemCell.GetComponent<RectTransform>();
            //rItemCell.SetParent(transform, false);
            //rItemCell.anchoredPosition = new Vector2(10000, 10000);

           
            GameObject prefab = gItemCell; //GameObject prefab = WorldMapSystem.instance.actorIcon;



            //for (int i = 0; i < Main.settings.numberOfColumns; i++)
            //{
            //    GameObject go = UnityEngine.Object.Instantiate(prefab);
            //    go.transform.SetParent(rItemCell, false);
            //}


            //GridLayoutGroup gridLayoutGroup = gItemCell.AddComponent<GridLayoutGroup>();
            //float cell_x = cellSize.x / Main.settings.numberOfColumns * 0.9f;
            //float cell_y = cell_x * cellSize.y / cellSize.x;
            //float spacing = 0.1f * cell_x;
            //gridLayoutGroup.cellSize = new Vector2(cell_x, cell_y);
            //gridLayoutGroup.spacing = new Vector2(spacing, spacing);
            //gridLayoutGroup.padding.left = (int)(spacing);
            //gridLayoutGroup.padding.top = (int)(spacing);

            //rItemCell.sizeDelta = new Vector2(size.x, cell_y);

            Transform parent = transform.parent;
            ScrollRect scroll = parent.GetComponent<ScrollRect>();

            #region 修订
            RectTransform rtfContent = scroll.content;
            GameObject goContent = rtfContent.gameObject;
            GridLayoutGroup gridLayoutGroup = rtfContent.GetComponent<GridLayoutGroup>();
            UnityEngine.Object.Destroy(gridLayoutGroup);
            VerticalLayoutGroup verticalLayoutGroup = goContent.AddComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.spacing = 0;
            verticalLayoutGroup.childControlHeight = false;
            verticalLayoutGroup.childControlWidth = false;
            verticalLayoutGroup.childForceExpandHeight = false;
            verticalLayoutGroup.childForceExpandWidth = false;
            verticalLayoutGroup.childAlignment = TextAnchor.UpperCenter;

            #endregion
            //if (scroll != null)
            //{
            //    if (scroll.verticalScrollbar != null)
            //    {
            //        Image other = scroll.verticalScrollbar.GetComponent<Image>();
            //        if (other != null)
            //        {
            //            RectTransform rect = (RectTransform)rScrollView.Find("ScrollbarVertical");
            //            Image my = rect.GetComponent<Image>();
            //            my.sprite = other.sprite;
            //            my.type = Image.Type.Sliced;
            //        }
            //        else
            //        {
            //            Main.Logger.Log("没找到 ScrollbarVertical Image");
            //        }

            //        if (scroll.verticalScrollbar.targetGraphic != null)
            //        {
            //            Image other2 = scroll.verticalScrollbar.targetGraphic.GetComponent<Image>();
            //            if (other != null)
            //            {
            //                RectTransform rect = (RectTransform)rScrollView.Find("ScrollbarVertical/SlidingArea/Handle");
            //                Image my = rect.GetComponent<Image>();
            //                my.sprite = other2.sprite;
            //                my.type = Image.Type.Sliced;
            //            }
            //            else
            //            {
            //                Main.Logger.Log("没找到 Handle Image");
            //            }
            //        }
            //        else
            //        {
            //            Main.Logger.Log("没找到 Handle");
            //        }
            //    }
            //}
            //else
            //{
            //    Main.Logger.Log("没找到 ScrollbarVertical");
            //}




            NpcItem itemCell = gItemCell.AddComponent<NpcItem>();
            bigDataScroll = gameObject.AddComponent<BigDataScroll>();
            bigDataScroll.Init(scrollRect, itemCell, SetCell);
            bigDataScroll.cellHeight = rItemCell.sizeDelta.y;

            SetData();
        }

        private void SetData()
        {
            if (bigDataScroll != null && m_data != null && isInit)
            {
                int count = m_data.Length  + 1;//int count = m_data.Length / Main.settings.numberOfColumns + 1;
                bigDataScroll.cellCount = count;
                if (!Main.OnChangeList)
                {
                    scrollRect.verticalNormalizedPosition = 1;
                }
            }
        }

        private void SetCell(ItemCell itemCell, int index)
        {
            NpcItem item = itemCell as NpcItem;
            if (item == null)
            {
                Main.Logger.Log("WarehouseItem出错。。。");
                return;
            }
            //ChildData[] childDatas = item.childDatas;
            //for (int i = 0; i < Main.settings.numberOfColumns; i++)
            //{
            int idx = index - 1; //int idx = (index - 1) * Main.settings.numberOfColumns + index - 1 + i;
                                 //if (i < childDatas.Length)
                                 //{
            ChildData childData = item.childData;  //ChildData childData = childDatas[i];
                    if (idx < m_data.Length)
                    {
                        int num4 = m_data[idx];
                        GameObject go = childData.gameObject;
                        if (!go.activeSelf)
                        {
                            go.SetActive(true);
                        }
                        gameObject.name = "Actor," + num4;
                        childData.setPlaceActor.SetActor(num4, Main.showNpcInfo);
                    }
                    else
                    {
                        GameObject go = childData.gameObject;
                        if (go.activeSelf)
                        {
                            go.SetActive(false);
                        }
                    }
                //}
                //else
                //{
                //    Main.Logger.Log("数据出错。。。");
                //}
            //}
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy | m_data == null | scrollRect == null)
            {
                return;
            }
            var mousePosition = Input.mousePosition;
            var mouseOnPackage = mousePosition.x < Screen.width / 10;

            var v = Input.GetAxis("Mouse ScrollWheel");
            if (v != 0)
            {
                if (mouseOnPackage)
                {
                    float count = m_data.Length  + 1; //float count = m_data.Length / Main.settings.numberOfColumns + 1;
                    scrollRect.verticalNormalizedPosition += v / count ; //scrollRect.verticalNormalizedPosition += v / count * Main.settings.scrollSpeed;
                }
            }
        }
    }

}









