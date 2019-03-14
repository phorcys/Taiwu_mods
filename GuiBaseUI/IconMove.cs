using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace GuiBaseUI
{
    public class IconMove:MonoBehaviour
    {
        static Transform root;
        static GameObject prefab;
        static List<IconItem> pool = new List<IconItem>();
        static List<IconItem> use = new List<IconItem>();
        public static void Move(Vector3 start, Vector3 target, float speed = 20, Sprite sprite = null)
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
            item.target = target;
            item.transform.position = start;
            item.speed = speed;
            item.image.sprite = sprite;
            item.transform.localScale = Vector3.one;
            use.Add(item);
        }
        void Update()
        {
            for (int i = use.Count-1; i >= 0; i--)
            {
                IconItem item = use[i];
                float move = item.speed * Time.deltaTime;
                if (Vector3.Distance(item.target , item.transform.position) < move)
                {
                    pool.Add(item);
                    use.RemoveAt(i);
                    item.transform.localScale = Vector3.zero;
                    return;
                }
                item.transform.position += (item.target - item.transform.position).normalized * move;
            }
        }

        static IconItem GetNewIcon()
        {
            if (root == null)
            {
                GameObject go = new GameObject();
                go.AddComponent<IconMove>();
                go.name = "IconMovePool";
                root = go.transform;
                root.SetParent(FindObjectOfType<Canvas>().transform,false);
                root.position = Vector3.zero;
                prefab = CreateUI.NewImage();
            }
            IconItem item = new IconItem(GameObject.Instantiate<GameObject>(prefab));
            return item;
        }

        struct IconItem
        {
            public Transform transform;
            public Image image;
            public Vector3 target;
            public float speed;
            public IconItem(GameObject go)
            {
                transform = go.transform;
                image = go.GetComponent<Image>();
                target = Vector3.zero;
                transform.SetParent(root);
                transform.position = new Vector3(10000, 0, 0);
                speed = 20;
            }
        }
    }
}