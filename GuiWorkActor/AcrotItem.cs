using System;
using System.Collections.Generic;
using UnityEngine;
using GuiBaseUI;

namespace GuiWorkActor
{
    public class ActorItem : ItemCell
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
        }
    }
    public struct ChildData
    {
        public GameObject gameObject;
        public SetWorkActorIcon setItem;
        public UnityEngine.UI.Toggle toggle;

        public ChildData(Transform child)
        {
            gameObject = child.gameObject;
            setItem = gameObject.GetComponent<SetWorkActorIcon>();
            toggle = gameObject.GetComponent<UnityEngine.UI.Toggle>();
        }
    }
}