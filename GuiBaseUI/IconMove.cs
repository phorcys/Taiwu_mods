using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace GuiBaseUI
{
    public class IconMove
    {
        static Transform root;
        static GameObject prefab;
        static List<IconItem> pool = new List<IconItem>();

        public static void Move(Vector3 start, Vector3 target, float duration = .5f, Sprite sprite = null)
        {
            IconItem item;
            if (pool.Count > 0)
            {
                item = pool[0];
                pool.RemoveAt(0);
            }
            else
            {
                item = GetNewIcon();
            }
            item.image.sprite = sprite;
            item.transform.localScale = Vector3.one;
            item.transform.position = start;
            var tweener = item.transform.DOMove(target, duration);
            tweener.SetEase(Ease.Linear);
            tweener.onComplete = delegate () {
                if (item.transform)
                {
                    item.transform.localScale = Vector3.zero;
                    pool.Add(item);
                }
            };
        }

        static IconItem GetNewIcon()
        {
            if (root == null)
            {
                GameObject go = new GameObject();
                go.name = "IconMove";
                root = go.transform;
                root.SetParent(Object.FindObjectOfType<Canvas>().transform,false);
                root.position = Vector3.zero;
                prefab = CreateUI.NewImage();
                prefab.transform.SetParent(root);
                prefab.transform.localScale = Vector3.zero;
            }
            IconItem item = new IconItem(Object.Instantiate(prefab));
            return item;
        }

        struct IconItem
        {
            public Transform transform;
            public Image image;
            public IconItem(GameObject go)
            {
                transform = go.transform;
                image = go.GetComponent<Image>();
                transform.SetParent(root);
                transform.localScale = Vector3.zero;
            }
        }
    }
}