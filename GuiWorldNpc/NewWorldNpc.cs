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
                Main.Logger.Log("设置数据 长度：" + value.Length);
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
        //    //[GuiWorldNpc] -- -- ActorView sizeDelta=(0.0, -40.0)
        //    //[GuiWorldNpc] -- -- -- ActorViewport sizeDelta=(0.0, 0.0)
        //    //[GuiWorldNpc] -- -- -- -- ActorHolder sizeDelta=(-10.0, 0.0)
        //    //[GuiWorldNpc] ActorHolder (UnityEngine.UI.GridLayoutGroup)
        //    //[GuiWorldNpc] (165.0, 78.0) cellSize
        //    //[GuiWorldNpc] (15.0, 38.5) spacing
        //    //[GuiWorldNpc] -- -- -- ActorScrollbar sizeDelta=(10.0, -40.0)
        //    //[GuiWorldNpc] -- -- -- -- Sliding Area sizeDelta=(-10.0, -10.0)
        //    //[GuiWorldNpc] -- -- -- -- -- Handle sizeDelta=(12.0, 12.0)

        private void InitUI()
        {

            Vector2 size = new Vector2(200, 560);
            Vector2 pos = new Vector2(0, 0);
            Vector2 cellSize = new Vector2(165.0f, 78.0f);


            GameObject scrollView = gameObject;
            scrollRect = scrollView.GetComponent<ScrollRect>();
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.verticalNormalizedPosition = 1;
            this.pos = 1;


            RectTransform rScrollView = ((RectTransform)scrollView.transform);
            RectTransform rView = scrollRect.viewport;
            RectTransform rtfContent = scrollRect.content;

            GameObject gItemCell = WorldMapSystem.instance.actorIcon;//GameObject gItemCell = new GameObject("line", new System.Type[] { typeof(RectTransform) });
            RectTransform rItemCell = gItemCell.GetComponent<RectTransform>();


            #region 修改滚动条
            Image bg = scrollRect.verticalScrollbar.GetComponent<Image>();
            bg.color = new Color(GuiBaseUI.Main.settings.bgR, GuiBaseUI.Main.settings.bgG, GuiBaseUI.Main.settings.bgB);
            Image hand = scrollRect.verticalScrollbar.targetGraphic as Image;
            hand.color = new Color(GuiBaseUI.Main.settings.handR, GuiBaseUI.Main.settings.handG, GuiBaseUI.Main.settings.handB);
            RectTransform rHandBg = bg.GetComponent<RectTransform>();
            RectTransform rHandBg2 = rHandBg.GetChild(0) as RectTransform;
            RectTransform rHand = hand.GetComponent<RectTransform>();
            #endregion

            //Main.Logger.Log("scrollRect大小");
            #region 修改scrollRect大小
            //Main.Logger.Log(rScrollView.ToString());
            //rScrollView.anchorMin = new Vector2(0.5f, 0.5f);
            //rScrollView.anchorMax = new Vector2(0.5f, 0.5f);
            //rScrollView.pivot = new Vector2(0.5f, 0.5f);
            //rScrollView.anchoredPosition = new Vector2(0, 0);
            //rScrollView.sizeDelta = size;

            //Main.Logger.Log(rView.ToString());
            //rView.anchorMin = new Vector2(0, 0);
            //rView.anchorMax = new Vector2(1, 1);
            //rView.anchoredPosition = new Vector2(0, 0);
            //rView.sizeDelta = new Vector2(-17, -17);
            //rView.pivot = new Vector2(0, 1);
            rView.GetComponent<Mask>().enabled = true;

            //Main.Logger.Log(rtfContent.ToString());
            //rtfContent.anchorMin = new Vector2(0, 1);
            //rtfContent.anchorMax = new Vector2(1, 1);
            //rtfContent.anchoredPosition = new Vector2(0, 0);
            //rtfContent.sizeDelta = new Vector2(0, 0);
            //rtfContent.pivot = new Vector2(0.5f, 1);
            ContentSizeFitter contentSizeFitter = rtfContent.GetComponent<ContentSizeFitter>();
            //if (contentSizeFitter == null)
            //{
            //    contentSizeFitter = rtfContent.gameObject.AddComponent<ContentSizeFitter>();
            //}
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            GameObject goContent = rtfContent.gameObject;
            GridLayoutGroup gridLayoutGroup = rtfContent.GetComponent<GridLayoutGroup>();
            UnityEngine.Object.DestroyImmediate(gridLayoutGroup);
            VerticalLayoutGroup verticalLayoutGroup = goContent.AddComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.spacing = 0;
            verticalLayoutGroup.childControlHeight = false;
            verticalLayoutGroup.childControlWidth = false;
            verticalLayoutGroup.childForceExpandHeight = true;
            verticalLayoutGroup.childForceExpandWidth = true;
            verticalLayoutGroup.childAlignment = TextAnchor.UpperCenter;
            
            //Main.Logger.Log(rHandBg.ToString());
            //rHandBg.anchorMin = new Vector2(1, 0);
            //rHandBg.anchorMax = new Vector2(1, 1);
            //rHandBg.anchoredPosition = new Vector2(0, 0);
            //rHandBg.sizeDelta = new Vector2(20, 0);
            //rHandBg.pivot = new Vector2(1, 1);

            //Main.Logger.Log(rHandBg2.ToString());
            //rHandBg2.anchorMin = new Vector2(0, 0);
            //rHandBg2.anchorMax = new Vector2(1, 1);
            //rHandBg2.anchoredPosition = new Vector2(0, 0);
            //rHandBg2.sizeDelta = new Vector2(-20, -20);
            //rHandBg2.pivot = new Vector2(0.5f, 0.5f);

            //Main.Logger.Log(rHand.ToString());
            //rHand.anchorMin = new Vector2(0, 0.5f);
            //rHand.anchorMax = new Vector2(1, 1);
            //rHand.anchoredPosition = new Vector2(0, 0);
            //rHand.sizeDelta = new Vector2(20, 20);
            //rHand.pivot = new Vector2(0.5f, 0.5f);
            //rHand.anchorMin = new Vector2(0, 0.5f);
            #endregion

            #region 修改NPC Item
            rItemCell.anchorMin = new Vector2(0.5f, 0.5f);
            rItemCell.anchorMax = new Vector2(0.5f, 0.5f);
            rItemCell.sizeDelta = new Vector2(165.0f, 78.0f + 38.5f);
            rItemCell.pivot = new Vector2(0.5f, 0.5f);
            Image imgItemBg = rItemCell.GetComponent<Image>();
            if (imgItemBg != null)
            {
                imgItemBg.enabled = false;
                GameObject newGo = CreateUI.NewImage(imgItemBg.sprite);
                Image newImg = newGo.GetComponent<Image>();
                newImg.sprite = imgItemBg.sprite;
                newImg.color = imgItemBg.color;
                newImg.type = imgItemBg.type;
                RectTransform newRtf = newGo.GetComponent<RectTransform>();
                newRtf.SetParent(rItemCell, false);
                newRtf.SetAsFirstSibling();
                newRtf.anchoredPosition = new Vector2(0, 0 - 5); // 0 7
                newRtf.sizeDelta = new Vector2(165.0f, 78.0f);
            }
            #endregion

            NpcItem itemCell = gItemCell.AddComponent<NpcItem>();
            bigDataScroll = gameObject.AddComponent<BigDataScroll>();
            bigDataScroll.Init(scrollRect, itemCell, SetCell,580);
            bigDataScroll.cellHeight = 78.0f + 38.5f;


            #region 初始化上下结构
            //bigDataScroll.top.name = "Actor,10000";
            //bigDataScroll.btm.name = "Actor,10000";
            //Image m_t = bigDataScroll.top.gameObject.AddComponent<Image>();
            //Image m_b = bigDataScroll.btm.gameObject.AddComponent<Image>();
            //m_t.color = new Color(1, 0, 0,1);
            //m_b.color = new Color(1, 0, 1,1);

            DestroyImmediate(bigDataScroll.top.gameObject);
            DestroyImmediate(bigDataScroll.btm.gameObject);
            GameObject top = Instantiate<GameObject>(gItemCell);
            GameObject btm = Instantiate<GameObject>(gItemCell);
            top.transform.SetParent(rtfContent, false);
            btm.transform.SetParent(rtfContent, false);
            LayoutElement ltop = top.AddComponent<LayoutElement>();
            LayoutElement lbtm = btm.AddComponent<LayoutElement>();
            ltop.preferredWidth = 1;
            lbtm.preferredWidth = 1;
            ltop.preferredHeight = 1;
            lbtm.preferredHeight = 1;
            bigDataScroll.top = ltop;
            bigDataScroll.btm = lbtm;

            LayoutElement lItem =  gItemCell.AddComponent<LayoutElement>();
            lItem.preferredWidth = 165.0f;
            lItem.preferredHeight = 165.0f;
            lItem.minHeight = 78.0f + 38.5f;
            lItem.minHeight = 78.0f + 38.5f;

            #endregion


            isInit = true;
            Main.Logger.Log("初始化成功 " + rScrollView.anchoredPosition.ToString() + " " + rScrollView.sizeDelta.ToString());

            SetData();
        }

        private void SetData()
        {
            Main.Logger.Log((bigDataScroll != null).ToString() + " " + (m_data != null).ToString() + " " + isInit.ToString());
            if (bigDataScroll != null && m_data != null && isInit)
            {
                int count = m_data.Length + 1;//int count = m_data.Length / Main.settings.numberOfColumns + 1;
                Main.Logger.Log((count).ToString() + " " + (!Main.OnChangeList).ToString() + " ");
                bigDataScroll.cellCount = count;
                if (!Main.OnChangeList)
                {
                    scrollRect.verticalNormalizedPosition = 1;
                    pos = 1;
                }
            }
        }

        private void SetCell(ItemCell itemCell, int index)
        {
            //test = itemCell.transform.GetChild(0).GetComponent<RectTransform>();
            NpcItem item = itemCell as NpcItem;
            if (item == null)
            {
                Main.Logger.Log("NpcItem。。。");
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
                        itemCell.name = "Actor," + num4;
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

        float pos = 1;
        private void Update()
        {
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = pos;
            }

            //滑动增强
            //if(Main.settings.scrollSpeed <= 0)
            //{
            //    return;
            //}
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
                    float count = m_data.Length + 1;
                    pos += v / count * Main.settings.scrollSpeed;
                    scrollRect.verticalNormalizedPosition = pos;
                }
            }
        }

        private void OnGUI()
        {
            if (GUILayout.Button("测试测试"))
            {
                GuiBaseUI.Main.LogAllChild(transform, true);
            }
        }
    }

}









