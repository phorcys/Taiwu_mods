using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GuiBaseUI
{
    public enum BarType
    {
        None,
        Vertical,
    }
    public enum ContentType
    {
        Grid,
        VerticalLayout,
    }
    public static class CreateUI
    {
        static int sort = 10000;
        public static GameObject NewCamera(int sortLayer = int.MinValue)
        {
            //如果没有UI事件系统则创建
            EventSystem eventSystem = GameObject.FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject go1 = new GameObject("EventSystem", new System.Type[]
                { typeof(EventSystem), typeof(StandaloneInputModule) });
            }

            //创建画布及设置参数
            GameObject go2 = new GameObject("Canvas", new System.Type[]
            { typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster), });
            Canvas canvas = go2.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortLayer == int.MinValue ? ++sort : sortLayer;
            CanvasScaler canvasScaler = go2.GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1280, 720);

            return go2;
        }

        public static GameObject NewScrollView(Vector2 size = default(Vector2), BarType barType = BarType.None, ContentType contentType = ContentType.VerticalLayout, Vector2 spacing = default(Vector2), Vector2 gridSize = default(Vector2))
        {
            //创建ScrollRect
            GameObject go1 = new GameObject("ScrollView", new System.Type[]
           { typeof(RectTransform), typeof(ScrollRect) });
            RectTransform tf1 = go1.GetComponent<RectTransform>();
            tf1.anchoredPosition = new Vector2(0, 0);
            tf1.sizeDelta = size == default(Vector2) ? new Vector2(100, 100) : size;
            ScrollRect scrollRect = go1.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;

            //创建Viewport
            GameObject go2 = new GameObject("Viewport", new System.Type[]
           { typeof(RectTransform), typeof(Mask),
                typeof(Image) });
            RectTransform tf2 = go2.GetComponent<RectTransform>();
            tf2.SetParent(tf1, false);
            tf2.anchorMin = new Vector2(0, 0);
            tf2.anchorMax = new Vector2(1, 1);
            tf2.anchoredPosition = new Vector2(0, 0);
            tf2.sizeDelta = new Vector2(-17, -17);
            tf2.pivot = new Vector2(0, 1);
            Image image2 = go2.GetComponent<Image>();
            image2.color = new Color(0.5568628f, 0.5568628f, 0.5568628f);

            //创建Content
            GameObject go3 = new GameObject("Content", new System.Type[]
           { typeof(RectTransform), typeof(ContentSizeFitter)});
            RectTransform tf3 = go3.GetComponent<RectTransform>();
            tf3.SetParent(tf2, false);
            tf3.anchorMin = new Vector2(0, 1);
            tf3.anchorMax = new Vector2(1, 1);
            tf3.anchoredPosition = new Vector2(0, 0);
            tf3.sizeDelta = new Vector2(0, 0);
            tf3.pivot = new Vector2(0.5f, 1);
            ContentSizeFitter contentSizeFitter = tf3.GetComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            switch (contentType)
            {
                case ContentType.Grid:
                    GridLayoutGroup gridLayoutGroup = go3.AddComponent<GridLayoutGroup>();
                    gridLayoutGroup.cellSize = gridSize == default(Vector2) ? new Vector2(100, 100) : gridSize;
                    gridLayoutGroup.spacing = spacing;
                    break;
                case ContentType.VerticalLayout:
                    VerticalLayoutGroup verticalLayoutGroup = go3.AddComponent<VerticalLayoutGroup>();
                    verticalLayoutGroup.spacing = spacing.y;
                    verticalLayoutGroup.childControlHeight = false;
                    verticalLayoutGroup.childControlWidth = false;
                    verticalLayoutGroup.childForceExpandHeight = false;
                    verticalLayoutGroup.childForceExpandWidth = false;
                    verticalLayoutGroup.childAlignment = TextAnchor.UpperCenter;
                    break;
                default:
                    break;
            }

            //关联控件
            scrollRect.viewport = tf2;
            scrollRect.content = tf3;
            switch (barType)
            {
                case BarType.Vertical:
                    GameObject go4 = new GameObject("ScrollbarVertical", new System.Type[]
                        { typeof(RectTransform), typeof(Image), typeof(Scrollbar)});
                    RectTransform tf4 = go4.GetComponent<RectTransform>();
                    tf4.SetParent(tf1, false);
                    tf4.anchorMin = new Vector2(1, 0);
                    tf4.anchorMax = new Vector2(1, 1);
                    tf4.anchoredPosition = new Vector2(0, 0);
                    tf4.sizeDelta = new Vector2(20, 0);
                    tf4.pivot = new Vector2(1, 1);
                    Image image4 = go4.GetComponent<Image>();
                    image4.sprite = default(Sprite);
                    image4.color = new Color(0, 0, 0, 1 / 255f);
                    Scrollbar scrollbar = go4.GetComponent<Scrollbar>();
                    scrollbar.direction = Scrollbar.Direction.BottomToTop;

                    GameObject go5 = new GameObject("SlidingArea", new System.Type[]
                        { typeof(RectTransform)});
                    RectTransform tf5 = go5.GetComponent<RectTransform>();
                    tf5.SetParent(tf4, false);
                    tf5.anchorMin = new Vector2(0, 0);
                    tf5.anchorMax = new Vector2(1, 1);
                    tf5.anchoredPosition = new Vector2(0, 0);
                    tf5.sizeDelta = new Vector2(-20, -20);
                    tf5.pivot = new Vector2(0.5f, 0.5f);

                    GameObject go6 = new GameObject("Handle", new System.Type[]
                        { typeof(RectTransform), typeof(Image)});
                    RectTransform tf6 = go6.GetComponent<RectTransform>();
                    tf6.SetParent(tf5, false);
                    tf6.anchorMin = new Vector2(0, 0.5f);
                    tf6.anchorMax = new Vector2(1, 1);
                    tf6.anchoredPosition = new Vector2(0, 0);
                    tf6.sizeDelta = new Vector2(20, 20);
                    tf6.pivot = new Vector2(0.5f, 0.5f);
                    Image image6 = go6.GetComponent<Image>();
                    image6.sprite = default(Sprite);
                    image6.color = new Color(0.894117653f, 0.3137255f, 0.3019608f);

                    scrollbar.targetGraphic = image6;
                    scrollbar.handleRect = tf6;
                    scrollRect.verticalScrollbar = scrollbar;
                    scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
                    scrollRect.verticalScrollbarSpacing = -3;
                    break;
                default:
                    break;
            }
            return go1;
        }

        public static GameObject NewImage(Sprite sprite = null)
        {
            GameObject go1 = new GameObject("Image", new System.Type[]
           { typeof(RectTransform), typeof(Image)});
            RectTransform tf1 = go1.GetComponent<RectTransform>();
            Image image = go1.GetComponent<Image>();
            if(sprite == null)
            {
                tf1.sizeDelta = new Vector2(100, 100);
            }
            else
            {
                image.sprite = sprite;
                image.SetNativeSize();
            }

            return go1;
        }


        public static GameObject NewText(string s = null,Vector2 size=default(Vector2))
        {
            GameObject go1 = new GameObject("Text", new System.Type[]
           { typeof(RectTransform), typeof(Text)});
            RectTransform tf1 = go1.GetComponent<RectTransform>();
            Text text = go1.GetComponent<Text>();
            text.text = s;
            text.fontSize = 20;
            
            if (size == default(Vector2))
            {
                tf1.sizeDelta = new Vector2(100, 100);
            }
            else
            {
                tf1.sizeDelta = size;
            }
            

            return go1;
        }
    }
}