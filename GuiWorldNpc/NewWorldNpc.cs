﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GuiBaseUI;
using System.Linq;

namespace GuiWorldNpc
{
    public class NewWorldNpc : MonoBehaviour
    {
        private int partId;
        private int placeId;
        BigDataScroll bigDataScroll;
        public ScrollRect scrollRect;
        private RectTransform rectContent;
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
            // Main.Logger.Log("初始化新世界NPC " + partId.ToString() + " " + placeId.ToString());
            this.partId = partId;
            this.placeId = placeId;
            InitUI();
        }

        private void InitUI()
        {
            isInit = true;

            Vector2 size = new Vector2(200, 600);
            Vector2 pos = new Vector2(0, 0);
            Vector2 cellSize = new Vector2(165.0f, 78.0f);
            float cellWidth = 165.0f;
            float cellHeight = 78.0f + 38.5f;
            // Main.Logger.Log("10");

            GameObject scrollView = CreateUI.NewScrollView(size, BarType.Vertical, ContentType.VerticalLayout);
            scrollRect = scrollView.GetComponent<ScrollRect>();
            WorldMapSystem.instance.actorHolder = scrollRect.content;
            rectContent = scrollRect.content;
            rectContent.GetComponent<ContentSizeFitter>().enabled = false;
            rectContent.GetComponent<VerticalLayoutGroup>().enabled = false;
            // Main.Logger.Log("完");

            scrollRect.verticalNormalizedPosition = 1;
            Image imgScrollView = scrollView.GetComponentInChildren<Image>();
            imgScrollView.color = new Color(0.5f, 0.5f, 0.5f, 0.005f);
            imgScrollView.raycastTarget = false;
            RectTransform rScrollView = ((RectTransform)scrollView.transform);
            rScrollView.SetParent(gameObject.transform, false);
            rScrollView.anchoredPosition = pos;

            //scrollView.GetComponentInChildren<Mask>().enabled = false;
            // Main.Logger.Log("完0");

            GameObject gItemCell = new GameObject("line", new System.Type[] { typeof(RectTransform) });
            RectTransform rItemCell = gItemCell.GetComponent<RectTransform>();
            rItemCell.SetParent(transform, false);
            rItemCell.anchoredPosition = new Vector2(10000, 10000);
            rItemCell.sizeDelta = new Vector2(cellWidth, cellHeight);
            //Image imgItemCell = gItemCell.AddComponent<Image>();
            //imgItemCell.color = new Color(1, 0, 0, 0.5f);
            // Main.Logger.Log("完成");

            GameObject prefab = WorldMapSystem.instance.actorIcon;
            for (int i = 0; i < Main.settings.numberOfColumns; i++)
            {
                GameObject go = UnityEngine.Object.Instantiate(prefab);
                go.transform.SetParent(rItemCell, false);
            }
            // Main.Logger.Log("完成0");


            GridLayoutGroup gridLayoutGroup = gItemCell.AddComponent<GridLayoutGroup>();
            gridLayoutGroup.cellSize = cellSize;
            gridLayoutGroup.spacing = new Vector2(0, 0);
            gridLayoutGroup.padding.left = (int)(0);
            gridLayoutGroup.padding.top = (int)(0);
            // Main.Logger.Log("完成1");


            NpcItem itemCell = gItemCell.AddComponent<NpcItem>();
            bigDataScroll = gameObject.AddComponent<BigDataScroll>();
            bigDataScroll.Init(scrollRect, itemCell, SetCell);
            bigDataScroll.cellHeight = cellHeight;

            //GuiBaseUI.Main.LogAllChild(transform, true);


            ScrollRect scroll = transform.GetComponent<ScrollRect>();
            // Main.Logger.Log("完成v");
            RectTransform otherRect = scroll.verticalScrollbar.GetComponent<RectTransform>();
            Image other = otherRect.GetComponent<Image>();
            // Main.Logger.Log("完成a");
            RectTransform myRect = scrollRect.verticalScrollbar.GetComponent<RectTransform>();
            //myRect.sizeDelta = new Vector2(10, 0);
            // Main.Logger.Log("完成b");
            Image my = myRect.GetComponent<Image>();
            // Main.Logger.Log("完成e");
            //my.color = new Color(0.9490196f, 0.509803951f, 0.503921571f);
            my.sprite = other.sprite;
            my.type = Image.Type.Sliced;
            // Main.Logger.Log("完成p");

            // Main.Logger.Log("完成V");
            RectTransform otherRect2 = scrollRect.verticalScrollbar.targetGraphic.GetComponent<RectTransform>();
            Image other2 = otherRect2.GetComponent<Image>();
            // Main.Logger.Log("完成A");
            RectTransform myRect2 = scrollRect.verticalScrollbar.targetGraphic.GetComponent<RectTransform>();
            // Main.Logger.Log("完成B");
            //myRect2.sizeDelta = new Vector2(10, 10);
            Image my2 = myRect2.GetComponent<Image>();
            // Main.Logger.Log("完成C");
            //my2.color = new Color(0.5882353f, 0.807843149f, 0.8156863f);
            my2.sprite = other2.sprite;
            my2.type = Image.Type.Sliced;
            // Main.Logger.Log("完成D");


            // Main.Logger.Log("完成3");
            SetData();


            ////test
            //Image imgContent = rectContent.gameObject.GetComponent<Image>();
            //if (imgContent == null)
            //{
            //    imgContent = rectContent.gameObject.AddComponent<Image>();
            //}
            //imgContent.color = new Color(0, 1, 0, 0.9f);
            //Image imgTop = bigDataScroll.top.gameObject.GetComponent<Image>();
            //if (imgTop == null)
            //{
            //    imgTop = bigDataScroll.top.gameObject.AddComponent<Image>();
            //}
            //imgTop.color = new Color(1, 1, 1, 0.9f);
            //Image imgBtm = bigDataScroll.btm.gameObject.GetComponent<Image>();
            //if (imgBtm == null)
            //{
            //    imgBtm = bigDataScroll.btm.gameObject.AddComponent<Image>();
            //}
            //imgContent.color = new Color(0, 0, 0, 0.9f);

            //bigDataScroll.top.name = "Actor,0";
            //bigDataScroll.btm.name = "Actor,0";

            //transform.localScale = transform.localScale * 0.33f;
        }

        private void SetData()
        {
            if (bigDataScroll != null && m_data != null && isInit)
            {
                int count = m_data.Length / Main.settings.numberOfColumns + 1;
                //Main.Logger.Log("=======！！！！！=======数据数量"+count);

                bigDataScroll.cellCount = count;
                //if (!Main.OnChangeList)
                //{
                //    scrollRect.verticalNormalizedPosition = 1;
                //}
            }
        }

        private void SetCell(ItemCell itemCell, int index)
        {
            //Main.Logger.Log(index.ToString() + "设置 itemCell。。。" + itemCell.ToString() + " pos=" + scrollRect.verticalNormalizedPosition.ToString());
            NpcItem item = itemCell as NpcItem;
            if (item == null)
            {
                //Main.Logger.Log("WarehouseItem出错。。。");
                return;
            }
            ChildData[] childDatas = item.childDatas;
            for (int i = 0; i < Main.settings.numberOfColumns; i++)
            {
                int idx = (index - 1) * Main.settings.numberOfColumns + i;
                //Main.Logger.Log("循环"+i+"获取第几个元素的数据" + idx.ToString());
                if (i < childDatas.Length)
                {
                    ChildData childData = childDatas[i];
                    GameObject go = childData.gameObject;
                    if (idx < m_data.Length)
                    {
                        int num4 = m_data[idx];
                        if (!go.activeSelf)
                        {
                            go.SetActive(true);
                        }
                        itemCell.name = "Actor," + num4;
                        if(itemCell.transform.childCount > 0)
                        {
                            itemCell.transform.GetChild(0).name = "Actor," + num4;
                        }
                        childData.setPlaceActor.SetActor(num4, Main.showNpcInfo);

                        //int key = num4;
                        //int num3 = int.Parse(DateFile.instance.GetActorDate(key, 19, false));
                        //int num2 = int.Parse(DateFile.instance.GetActorDate(key, 20, false));
                        //int key2 = (num2 < 0) ? (1001 + int.Parse(DateFile.instance.GetActorDate(key, 14, false))) : 1001;
                        //int gangValueId = DateFile.instance.GetGangValueId(num3, num2);
                        //int actorFavor = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), key, false, false);
                        //string des = "======"+((actorFavor != -1) ? ActorMenu.instance.Color5(actorFavor, true, -1) : DateFile.instance.SetColoer(20002, DateFile.instance.massageDate[303][2], false));
                        //des += "\n======" + ((int.Parse(DateFile.instance.GetActorDate(key, 8, false)) != 1) ? DateFile.instance.SetColoer((int.Parse(DateFile.instance.GetActorDate(key, 19, false)) == 18) ? 20005 : 20010, DateFile.instance.GetActorName(key, false, false), false) : DateFile.instance.GetActorName(key, false, false));
                        //des += "\n======" + DateFile.instance.SetColoer(10003, DateFile.instance.GetGangDate(num3, 0), false) + ((num3 == 0) ? "" : DateFile.instance.SetColoer(20011 - Mathf.Abs(num2), DateFile.instance.presetGangGroupDateValue[gangValueId][key2], false));
                        //Main.Logger.Log(des);
                    }
                    else
                    {
                        if (go.activeSelf)
                        {
                            go.SetActive(false);
                        }
                    }
                    if (i == 0 && !go.transform.parent.gameObject.activeSelf)
                        go.transform.parent.gameObject.SetActive(true);


                }
                else
                {
                    Main.Logger.Log("数据出错。。。");
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
            var mouseOnPackage = mousePosition.x < Screen.width / 10 && mousePosition.y > Screen.width / 10 && mousePosition.y < Screen.width / 10 * 9;

            var v = Input.GetAxis("Mouse ScrollWheel");
            if (v != 0)
            {
                if (mouseOnPackage)
                {
                    float count = m_data.Length / Main.settings.numberOfColumns + 1;
                    scrollRect.verticalNormalizedPosition += v / count * Main.settings.scrollSpeed;
                }
            }
        }

        //private bool late_update;
        //private void LateUpdate()
        //{
        //    if (!late_update)
        //        return;

        //    late_update = false;
        //    if (bigDataScroll != null && m_data != null && isInit)
        //    {
        //        int count = m_data.Length / Main.settings.numberOfColumns + 1;
        //        Main.Logger.Log("数量"+count);

        //        bigDataScroll.cellCount = count;
        //        if (!Main.OnChangeList)
        //        {
        //            scrollRect.verticalNormalizedPosition = 1;
        //        }
        //    }
        //}

        //void OnGUI()
        //{
        //    if (GUILayout.Button("Test"))
        //    {
        //        GuiBaseUI.Main.LogAllChild(transform);
        //    }
        //}
    }

}








