using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GuiBaseUI
{
    public class ItemCell : MonoBehaviour
    {

        RectTransform rectTransform;
        public virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void SetActive(bool value)
        {
            if (gameObject.activeSelf != value)
            {
                gameObject.SetActive(value);
            }
        }
    }
}