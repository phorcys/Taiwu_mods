using System;
using System.Collections.Generic;
using UnityEngine;
using GuiBaseUI;

namespace GuiWarehouse
{
    public class WarehouseItem:ItemCell
    {

        public ChildData[] childDatas;
        public override void Awake()
        {
            base.Awake();
            childDatas = new ChildData[8];
            for (int i = 0; i < 8; i++)
            {
                Transform child = transform.GetChild(i);
                childDatas[i] = new ChildData(child);
            }
        }
    }
    public struct ChildData
    {
        public GameObject gameObject;
        public SetItem setItem;
        public ChildData(Transform child)
        {
            gameObject = child.gameObject;
            setItem = gameObject.GetComponent<SetItem>();
        }
    }
}