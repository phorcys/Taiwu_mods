using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace GuiBaseUI
{

    public delegate void handleSetData(ItemCell go, int index);
    public class BigDataScroll:MonoBehaviour
    {

        enum InsertPos
        {
            top,
            btm,
        }

        public float cellHeight { get; set; }
        private handleSetData m_funcSetData;
        public handleSetData funcSetData { get { return m_funcSetData; } }
        private int m_cellCount;
        public int cellCount
        {
            set
            {
                m_cellCount = value;
                InitData();
            }
            get
            {
                return m_cellCount;
            }
        }

        public ScrollRect scrollRect;
        public GameObject go;
        public RectTransform retTransform;
        public RectTransform content;
        public RectTransform view;
        public RectTransform top;
        public RectTransform btm;
        public ContentSizeFitter contentSizeFitter;
        public VerticalLayoutGroup verticalLayoutGroup;

        /// <summary>
        /// 初始化滚动
        /// </summary>
        /// <param name="_scrollRect">ScrollRect组件</param>
        /// <param name="_itemcell">格子预制件的ItemCell组件</param>
        /// <param name="_handleSetData">委托回调方法 设置格子数据</param>
        public void Init(ScrollRect _scrollRect, ItemCell _itemcell, handleSetData _handleSetData, float wHeight = 0)
        {
            scrollRect = _scrollRect;
            m_funcSetData = _handleSetData;
            contentSizeFitter = scrollRect.content.GetComponent<ContentSizeFitter>();
            verticalLayoutGroup = scrollRect.content.GetComponent<VerticalLayoutGroup>();
            ItemPool.instance.SetItem(funcSetData, _itemcell.gameObject);
            go = scrollRect.gameObject;
            retTransform = scrollRect.GetComponent<RectTransform>();
            content = scrollRect.content;
            view = scrollRect.viewport;
            scrollRect.onValueChanged.AddListener(OnValueChanged);
            top = new GameObject("top", new Type[] { typeof(RectTransform) }).GetComponent<RectTransform>();
            btm = new GameObject("button", new Type[] { typeof(RectTransform) }).GetComponent<RectTransform>();
            top.SetParent(content, false);
            btm.SetParent(content, false);
            top.sizeDelta = default(Vector2);
            btm.sizeDelta = default(Vector2);
        }

        private void OnValueChanged(Vector2 pos)
        {
            maxHeight = cellHeight * m_cellCount;

            if(contentSizeFitter == null || !contentSizeFitter.enabled)
            {
                content.sizeDelta = new Vector2(0, maxHeight);
            }

            // Main.Logger.Log("总高度" + maxHeight.ToString());
            windowsHeight = retTransform.sizeDelta.y;
            // Main.Logger.Log("窗口高度" + windowsHeight.ToString() + " 格子高度" + cellHeight.ToString());
            showCount = (int)(windowsHeight / cellHeight);
            // Main.Logger.Log("显示数量" + showCount.ToString());


            // Main.Logger.Log("Star对象池数量" + ItemPool.instance.pool[funcSetData].Count.ToString() + " 使用的数量" + (content.childCount - 2).ToString());
            float value = 1 - pos.y;
            // Main.Logger.Log("value " + value.ToString());
            int _startIndex = (int)(value * (cellCount - showCount));
            int _endIndex = (int)(value * (cellCount - showCount)) + showCount + 2;
            if (_startIndex < 1)
            {
                // Main.Logger.Log(_startIndex.ToString() + "异常修正1");
                _startIndex = 1;
            }
            if (_endIndex > cellCount)
            {
                // Main.Logger.Log(_endIndex.ToString() + "异常修正" + (cellCount).ToString());
                _endIndex = cellCount;
            }
            // Main.Logger.Log("新的index" + _startIndex.ToString() + " -- " + _endIndex.ToString());
            // Main.Logger.Log("旧的index" + startIndex.ToString() + " -- " + endIndex.ToString());


            // Main.Logger.Log(string.Format("for {0} -- {1}", startIndex.ToString(), endIndex.ToString()));
            for (int i = startIndex; i <= endIndex; i++)//delete
            {
                if (i > -1)
                {
                    if (i < _startIndex)
                    {
                        // Main.Logger.Log("delete top" + i.ToString());
                        ItemPool.instance.PutItem(funcSetData, content.GetChild(1).gameObject);
                    }
                    else if (i > _endIndex)
                    {
                        // Main.Logger.Log("delete btm" + i.ToString());
                        ItemPool.instance.PutItem(funcSetData, content.GetChild(content.childCount - 2).gameObject);
                    }
                    else
                    {
                        // Main.Logger.Log("not delete" + i.ToString());
                    }
                }
            }

            // Main.Logger.Log(string.Format("for {0} -- {1}", _startIndex.ToString(), _endIndex.ToString()));
            for (int i = _startIndex; i <= _endIndex; i++)// add
            {
                if (i < startIndex)
                {
                    // Main.Logger.Log("add top" + i.ToString());
                    GameObject item = ItemPool.instance.GetItem(funcSetData, content);
                    item.transform.SetSiblingIndex((i - _startIndex + 1));
                    Flush(item, i);
                }
                else if (i > endIndex)
                {
                    // Main.Logger.Log("add btm" + i.ToString());
                    GameObject item = ItemPool.instance.GetItem(funcSetData, content);
                    item.transform.SetSiblingIndex(content.childCount - 2);
                    Flush(item, i);
                }
                else
                {
                    // Main.Logger.Log("not add" + i.ToString());
                    if (!isInit)
                    {
                        int idx = i - _startIndex + 1;
                        if (idx > 0 && idx <( content.childCount - 1))
                        {
                            GameObject item = content.GetChild(idx).gameObject;
                            Flush(item, i);
                        }
                        else
                        {
                            // Main.Logger.Log("why flush index " + i.ToString());
                        }
                    }
                }
            }

            startIndex = _startIndex;
            endIndex = _endIndex;

            if (verticalLayoutGroup == null || !verticalLayoutGroup.enabled)
            {
                Vector2 star = new Vector2(0, (startIndex - 1) * cellHeight + cellHeight / 2);
                for (int i = 1; i < content.childCount - 1; i++)
                {
                    RectTransform rtf = (RectTransform)content.GetChild(i);
                    Vector2 p = (star + new Vector2(0, (i - 1) * cellHeight - maxHeight / 2)) * -1;
                    rtf.anchoredPosition = p;
                }
            }
            else
            {
                top.sizeDelta = new Vector2(0, (startIndex - 1) * cellHeight);
                btm.sizeDelta = new Vector2(0, (cellCount - endIndex) * cellHeight);
            }

            // Main.Logger.Log("end对象池数量" + ItemPool.instance.pool[funcSetData].Count.ToString() + " 使用的数量" + (content.childCount - 2).ToString());
        }

        float maxHeight;
        float windowsHeight;
        int showCount;
        int startIndex = -1;
        int endIndex = -1;
        bool isInit = false;
        private void InitData()
        {
            //if (startIndex != -1)
            //{
            //    for (int i = startIndex; i < endIndex; i++)
            //    {
            //        ItemPool.instance.PutItem(funcSetData, content.GetChild(1).gameObject);
            //    }
            //    startIndex = -1;
            //    endIndex = -1;
            //}


            //if (scrollRect.verticalNormalizedPosition != 1)
            //{
            //    scrollRect.verticalNormalizedPosition = 1;
            //}
            //else
            //{

            isInit = false;
                OnValueChanged(new Vector2(0, scrollRect.verticalNormalizedPosition));
            isInit = true;


            //}
        }



        private void Flush(GameObject go, int index)
        {
            funcSetData(go.GetComponent<ItemCell>(), (int)index);
        }





        class ItemPool
        {
            static ItemPool itemPool;
            static public ItemPool instance
            {
                get
                {
                    if (itemPool == null)
                    {
                        itemPool = new ItemPool();
                    }
                    return itemPool;
                }
            }
            private ItemPool()
            {
                GameObject go = new GameObject("ItemPool");
                GameObject.DontDestroyOnLoad(go);
                poolRoot = go.transform;
            }

            Transform poolRoot;
            public Dictionary<handleSetData, List<GameObject>> pool = new Dictionary<handleSetData, List<GameObject>>();

            public GameObject GetItem(handleSetData func, Transform parent)
            {
                if (pool.ContainsKey(func))
                {
                    var v = pool[func];
                    if (v != null && v.Count > 0)
                    {
                        GameObject go = null;
                        if (v.Count > 1)
                        {
                            go = v[1];
                            v.RemoveAt(1);
                        }
                        else
                        {
                            go = GameObject.Instantiate<GameObject>(v[0]);
                        }
                        go.SetActive(true);
                        Transform tf = go.transform;
                        tf.SetParent(parent, false);
                        return go;
                    }
                }
                // Main.Logger.Log("not set item");
                return null;
            }
            public void PutItem(handleSetData func, GameObject item)
            {
                if (pool.ContainsKey(func))
                {
                    var v = pool[func];
                    if (v != null && v.Count > 0)
                    {
                        item.transform.SetParent(poolRoot, false);
                        item.SetActive(false);
                        v.Add(item);
                        return;
                    }
                }
                GameObject.Destroy(item);
            }
            public void SetItem(handleSetData func, GameObject prefab)
            {
                if (prefab.GetComponent<ItemCell>() == null)
                {
                    // Main.Logger.Log(prefab.ToString() + " don't find ItemCell");
                    return;
                }
                prefab.SetActive(false);
                if (!pool.ContainsKey(func))
                {
                    var v = new List<GameObject>();
                    v.Add(prefab);
                    pool.Add(func, v);
                }
                else
                {
                    var v = pool[func];
                    for (int i = 1; i < v.Count; i++)
                    {
                        GameObject.Destroy(v[(int)i]);
                    }
                    v.Clear();
                    v.Add(prefab);
                }
            }
        }
    }
}