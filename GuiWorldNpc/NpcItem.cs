using System;
using System.Collections.Generic;
using UnityEngine;
using GuiBaseUI;

namespace GuiWorldNpc
{
    public class NpcItem : ItemCell
    {

        public ChildData[] childDatas;
        public override void Awake()
        {
            base.Awake();
            childDatas = new ChildData[Main.settings.numberOfColumns];
            for (int i = 0; i < Main.settings.numberOfColumns; i++)
            {
                Transform child = transform.GetChild(i);
                childDatas[i] = new ChildData(child);
            }
            //Main.Logger.Log("WarehouseItem Awake " + childDatas.Length);
        }
    }
    public struct ChildData
    {
        public GameObject gameObject;
        public SetPlaceActor setPlaceActor;

        public ChildData(Transform child)
        {
            gameObject = child.gameObject;
            setPlaceActor = gameObject.GetComponent<SetPlaceActor>();
        }
    }
}