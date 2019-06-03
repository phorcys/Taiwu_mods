using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GuiBaseUI;
using System.Linq;

namespace GuiWorkActor
{
    public class NewWorkActor : MonoBehaviour
    {
        public bool favorChange;
        public int skillTyp;
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



        //将这个脚本挂在原本的ScrollRect的gameObject上，然后Init()
        public void Init(int _skillTyp, bool _favorChange)
        {
            // Main.Logger.Log("NewWarehouse 初始化 " + _skillTyp.ToString());

            // 设置参数
            this.skillTyp = _skillTyp;
            this.favorChange = _favorChange;

            InitUI();
        }

        private void InitUI()
        {
            Vector2 size = new Vector2(995.0f, 780.0f);
            Vector2 cellSize = new Vector2(210, 78);
            float spacing = 30f;
            float lineHeight = size.y / 6;

            GameObject scrollView = CreateUI.NewScrollView(size, BarType.Vertical, ContentType.Node);

            ContentSizeFitter contentSizeFitter = scrollView.GetComponentInChildren<ContentSizeFitter>();
            if (contentSizeFitter != null)
            {
                contentSizeFitter.enabled = false;
            }

            scrollRect = scrollView.GetComponent<ScrollRect>();
            scrollRect.verticalNormalizedPosition = 1;
            Image imgScrollView = scrollView.GetComponentInChildren<Image>();
            imgScrollView.color = new Color(0.5f, 0.5f, 0.5f, 0.005f);// 测试时可以把透明度调成0.5可以看见底板大小是否正确
            imgScrollView.raycastTarget = false;
            RectTransform rScrollView = ((RectTransform)scrollView.transform);
            rScrollView.SetParent(transform, false);
            rScrollView.anchoredPosition = new Vector2(0, 0);

            //scrollView.GetComponentInChildren<Mask>().enabled = false;//测试的时候设置不要遮罩方便看

            GameObject itemCell = new GameObject("line", new System.Type[] { typeof(RectTransform) });// 创建一行的预制件
            //itemCell.AddComponent<Image>().color = new Color(1, 0, 0, 0.5f);//测试的时候加个图片方便看到是否有创建

            RectTransform rItemCell = itemCell.GetComponent<RectTransform>();
            rItemCell.SetParent(transform, false);
            rItemCell.anchoredPosition = new Vector2(10000, 10000);// 把预制件设置到看不见的地方
            rItemCell.sizeDelta = new Vector2(size.x, lineHeight);
            GameObject prefab = HomeSystem.instance.listActor; // 这里设置Item预制件

            for (int i = 0; i < Main.settings.numberOfColumns; i++)// 初始化预制件
            {
                GameObject go = UnityEngine.Object.Instantiate(prefab);
                go.transform.SetParent(rItemCell, false);
            }

            GridLayoutGroup gridLayoutGroup = itemCell.AddComponent<GridLayoutGroup>();
            gridLayoutGroup.cellSize = cellSize;
            gridLayoutGroup.spacing = new Vector2(spacing, spacing);
            gridLayoutGroup.padding.left = (int)spacing;
            gridLayoutGroup.padding.top = (int)spacing;


            
            ActorItem actorItem = itemCell.AddComponent<ActorItem>();
            bigDataScroll = gameObject.AddComponent<BigDataScroll>();// 添加大数据滚动组件
            bigDataScroll.Init(scrollRect, actorItem, SetCell);//初始化大数据滚动组件
            bigDataScroll.cellHeight = lineHeight;//设置每行高度

            // 设置滚动条图片
            Transform parent = transform.parent;
            ScrollRect scroll = GetComponent<ScrollRect>();//获取原本的组件
            Image otherBar = scroll.verticalScrollbar.GetComponent<Image>();
            Image myBar = scrollRect.verticalScrollbar.GetComponent<Image>();
            myBar.sprite = otherBar.sprite;
            myBar.type = Image.Type.Sliced;

            Image otherHand = scroll.verticalScrollbar.targetGraphic.GetComponent<Image>();
            Image myHand = scrollRect.verticalScrollbar.targetGraphic.GetComponent<Image>();
            myHand.sprite = otherHand.sprite;
            myHand.type = Image.Type.Sliced;

            // Main.Logger.Log("UI改造完毕");
            //GuiBaseUI.Main.LogAllChild(transform.parent, true);

            isInit = true;
            SetData();
        }

        private void SetData()
        {
            if (bigDataScroll != null && m_data != null && isInit)
            {
                int count = m_data.Length / Main.settings.numberOfColumns + 1;
                // Main.Logger.Log("数据长度" + m_data.Length + " 行数" + count.ToString());

                for (int i = 0; i < m_data.Length; i++)
                {
                    // Main.Logger.Log("数据 " + i + ":" + m_data[i]);
                }

                bigDataScroll.cellCount = count;
                scrollRect.verticalNormalizedPosition = 1;


                //GuiBaseUI.Main.LogAllChild(transform.parent, true);
            }
        }

        private void SetCell(ItemCell itemCell, int index)
        {
            ActorItem item = itemCell as ActorItem;
            if (item == null)
            {
                // Main.Logger.Log("ItemCell 出错。。。");
                return;
            }
            item.name = "Actor,10000";

            //item.GetComponent<Image>().color = new Color(index%2 == 0 ? 1:0, 0, 0);

            string[] names = new string[item.childDatas.Length];

            ChildData[] childDatas = item.childDatas;
            for (int i = 0; i < Main.settings.numberOfColumns; i++)
            {
                if (i < childDatas.Length)
                {
                    int idx = (index - 1) * Main.settings.numberOfColumns + i;
                    ChildData childData = childDatas[i];
                    if (idx < m_data.Length)
                    {
                        int num2 = m_data[idx];
                        GameObject go = childData.gameObject;
                        go.name = "Actor," + num2;
                        childData.toggle.group = HomeSystem.instance.listActorsHolder.GetComponent<ToggleGroup>();
                        childData.setItem.SetActor(num2, skillTyp, favorChange);

                        names[i] =  "\nActor," + num2+ " 多余的隐藏 第" + index + "行 第" + i + "个  索引=" + idx;
                        if (!go.activeSelf)
                        {
                            go.SetActive(true);
                        }
                    }
                    else
                    {
                        GameObject go = childData.gameObject;
                        if (go.activeSelf)
                        {
                            go.SetActive(false);
                        }
                        // Main.Logger.Log("多余的隐藏 第" + index + "行 第" + i + "个  索引=" + idx);
                    }
                }
                else
                {
                    // Main.Logger.Log("行数量出错。。。");
                }
            }

            // Main.Logger.Log(string.Format("创建第{0}行NPC {1} {2} {3} {4}", index, names[0], names[1], names[2], names[3]));
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy | m_data == null | scrollRect == null)
            {
                return;
            }
            var mousePosition = Input.mousePosition;
            var mouseOnPackage = mousePosition.x > Screen.width * 0.9f && mousePosition.x > Screen.width * 0.1f && mousePosition.y > Screen.height * 0.9f && mousePosition.y > Screen.height * 0.1f;

            var v = Input.GetAxis("Mouse ScrollWheel");
            if (v != 0)
            {
                    float count = m_data.Length / Main.settings.numberOfColumns + 1;
                    scrollRect.verticalNormalizedPosition += v / count * Main.settings.scrollSpeed;
            }
        }

        // void OnGUI()
        // {
        //     if (GUILayout.Button("xxxxxxx"))
        //     {
        //         GuiBaseUI.Main.LogAllChild(transform.parent.parent, true);
        //     }
        // }
    }

}
