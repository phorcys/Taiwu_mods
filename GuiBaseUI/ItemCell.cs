using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCell : MonoBehaviour {

    RectTransform rectTransform;
    public virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
}
