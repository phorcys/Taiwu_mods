using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace GuiBaseUI
{

    public delegate void handleSetData(ItemCell go, int index);
    public class BigDataScroll
    {

        enum InsertPos
        {
            top,
            btm,
        }

        public float cellHeight { get; set; }
        private handleSetData m_funcSetData;
        public handleSetData funcSetData { get { return m_funcSetData; } }
        private long m_cellCount;
        public long cellCount
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

        ScrollRect scrollRect;
        GameObject gameObject;
        RectTransform transform;
        RectTransform content;
        RectTransform view;
        RectTransform top;
        RectTransform btm;

        public BigDataScroll(ScrollRect _scrollRect, ItemCell _itemcell, handleSetData _handleSetData)
        {
            scrollRect = _scrollRect;
            m_funcSetData = _handleSetData;
            ItemPool.instance.SetItem(funcSetData, _itemcell.gameObject);
            gameObject = scrollRect.gameObject;
            transform = scrollRect.GetComponent<RectTransform>();
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
            float value =  1 - pos.y;
             Main.Logger.Log("value"+ value.ToString());
            long _startIndex = (long)(value * (cellCount - showCount));
             Main.Logger.Log("开始index" + _startIndex.ToString());
            long _endIndex = (long)(value * (cellCount - showCount)) + showCount + 2;
             Main.Logger.Log("结束index" + _endIndex.ToString());
            if (_startIndex < 1)
            {
                Main.Logger.Log(_startIndex.ToString() + "异常修正1");
                _startIndex = 1;
            }
            if(_endIndex > cellCount)
            {
                Main.Logger.Log(_endIndex.ToString() + "异常修正"+(cellCount).ToString());
                _endIndex = cellCount;
            }


            for (long i = startIndex; i <= endIndex; i++)//delete
            {
                if (i > -1)
                {
                    if(i< _startIndex)
                    {
                         Main.Logger.Log("delete top" + i.ToString());
                        ItemPool.instance.PutItem(funcSetData, content.GetChild(1).gameObject);
                    }
                    else if(i> _endIndex)
                    {
                         Main.Logger.Log("delete btm" + i.ToString());
                        ItemPool.instance.PutItem(funcSetData, content.GetChild(content.childCount - 2).gameObject);
                    }
                }
            }
            for (long i = _startIndex; i <= _endIndex; i++)// add
            {
                if(i< startIndex)
                {
                     Main.Logger.Log("add top" + i.ToString());
                    GameObject item = ItemPool.instance.GetItem(funcSetData, content);
                    item.transform.SetSiblingIndex((int)(i - _startIndex + 1));
                    Flush(item, i);
                }else if (i > endIndex)
                {
                     Main.Logger.Log("add btm" + i.ToString());
                    GameObject item = ItemPool.instance.GetItem(funcSetData, content);
                    item.transform.SetSiblingIndex(content.childCount - 2);
                    Flush(item, i);
                }
            }

            startIndex = _startIndex;
            endIndex = _endIndex;

            top.sizeDelta = new Vector2(0, (startIndex - 1) * cellHeight);
            btm.sizeDelta = new Vector2(0, (cellCount - endIndex) * cellHeight);

             Main.Logger.Log("对象池数量" + ItemPool.instance.pool[funcSetData].Count.ToString() + " 使用的数量" + (content.childCount - 2).ToString());
        }

        float maxHeight;
        float windowsHeight;
        long showCount;
        long startIndex = -1;
        long endIndex = -1;
        private void InitData()
        {
            maxHeight = cellHeight * m_cellCount;
            Main.Logger.Log("总高度" + maxHeight.ToString());
            windowsHeight = transform.sizeDelta.y;
            Main.Logger.Log("窗口高度" + windowsHeight.ToString());
            showCount = (long)(windowsHeight / cellHeight);
             Main.Logger.Log("显示数量" + showCount.ToString());
            if (scrollRect.verticalNormalizedPosition != 1)
            {
                scrollRect.verticalNormalizedPosition = 1;
            }
            else
            {
                OnValueChanged(new Vector2(0, scrollRect.verticalNormalizedPosition));
            }
        }



        private void Flush(GameObject go, long index)
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
                Main.Logger.Log("not set item");
                return null;
            }
            public void PutItem(handleSetData func, GameObject item)
            {
                if (pool.ContainsKey(func))
                {
                    var v = pool[func];
                    if (v != null && v.Count > 0)
                    {
                        item.transform.SetParent(poolRoot,false);
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
                    Main.Logger.Log(prefab.ToString() + " don't find ItemCell");
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
                    for (long i = 1; i < v.Count; i++)
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