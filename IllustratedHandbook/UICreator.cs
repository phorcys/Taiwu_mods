using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Util from https://github.com/tomori-hikage/ugui-script-creator
namespace HC.UI
{
    public static class UICreator
    {
        #region enum

        public enum LayoutGroupType
        {
            Grid,
            Horizontal,
            Vertical,
        }

        #endregion

        #region struct

        public struct Resources
        {
            public Sprite standard;
            public Sprite background;
            public Sprite inputField;
            public Sprite knob;
            public Sprite checkmark;
            public Sprite dropdown;
            public Sprite mask;
            public Font font;
        }

        #endregion

        #region variable

        private const string kUILayerName = "UI";

        private const string kStandardSpritePath = "UI/Skin/UISprite.psd";
        private const string kBackgroundSpritePath = "UI/Skin/Background.psd";
        private const string kInputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";
        private const string kKnobPath = "UI/Skin/Knob.psd";
        private const string kCheckmarkPath = "UI/Skin/Checkmark.psd";
        private const string kDropdownArrowPath = "UI/Skin/DropdownArrow.psd";
        private const string kMaskPath = "UI/Skin/UIMask.psd";
        private const string kFontPath = "Arial.ttf";

        static private Resources s_StandardResources;


        private const float kWidth = 160f;
        private const float kThickHeight = 30f;
        private const float kThinHeight = 20f;
        private static Vector2 s_ThickElementSize = new Vector2(kWidth, kThickHeight);
        private static Vector2 s_ThinElementSize = new Vector2(kWidth, kThinHeight);
        private static Vector2 s_ImageElementSize = new Vector2(100f, 100f);
        private static Color s_DefaultSelectableColor = new Color(1f, 1f, 1f, 1f);
        private static Color s_PanelColor = new Color(1f, 1f, 1f, 0.392f);
        private static Color s_TextColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

        private static Font font = UnityEngine.Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;

        #endregion

        #region method

        #region Helper

        static private Resources GetStandardResources()
        {
            if (s_StandardResources.standard == null)
            {
#if UNITY_EDITOR
                s_StandardResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>(kStandardSpritePath);
                s_StandardResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpritePath);
                s_StandardResources.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>(kInputFieldBackgroundPath);
                s_StandardResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>(kKnobPath);
                s_StandardResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>(kCheckmarkPath);
                s_StandardResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>(kDropdownArrowPath);
                s_StandardResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>(kMaskPath);
#else
                s_StandardResources.standard = UnityEngine.Resources.Load<GameObject>("UISprite").GetComponent<SpriteRenderer>().sprite;
                s_StandardResources.background = UnityEngine.Resources.Load<GameObject>("Background").GetComponent<SpriteRenderer>().sprite;
                s_StandardResources.inputField = UnityEngine.Resources.Load<GameObject>("InputFieldBackground").GetComponent<SpriteRenderer>().sprite;
                s_StandardResources.knob = UnityEngine.Resources.Load<GameObject>("Knob").GetComponent<SpriteRenderer>().sprite;
                s_StandardResources.checkmark = UnityEngine.Resources.Load<GameObject>("Checkmark").GetComponent<SpriteRenderer>().sprite;
                s_StandardResources.dropdown = UnityEngine.Resources.Load<GameObject>("DropdownArrow").GetComponent<SpriteRenderer>().sprite;
                s_StandardResources.mask = UnityEngine.Resources.Load<GameObject>("UIMask").GetComponent<SpriteRenderer>().sprite;
#endif
                s_StandardResources.font = UnityEngine.Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            }
            return s_StandardResources;
        }

        private static GameObject CreateUIElementRoot(string name, Vector2 size)
        {
            GameObject child = new GameObject(name);
            RectTransform rectTransform = child.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            return child;
        }

        static GameObject CreateUIObject(string name, GameObject parent)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<RectTransform>();
            SetParentAndAlign(go, parent);
            return go;
        }

        private static void SetDefaultTextValues(Text lbl)
        {
            // Set text values we want across UI elements in default controls.
            // Don't set values which are the same as the default values for the Text component,
            // since there's no point in that, and it's good to keep them as consistent as possible.
            lbl.color = s_TextColor;
        }

        private static void SetDefaultColorTransitionValues(Selectable slider)
        {
            ColorBlock colors = slider.colors;
            colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
            colors.pressedColor = new Color(0.698f, 0.698f, 0.698f);
            colors.disabledColor = new Color(0.521f, 0.521f, 0.521f);
        }

        private static void SetParentAndAlign(GameObject child, GameObject parent)
        {
            if (parent == null)
                return;

            child.transform.SetParent(parent.transform, false);
            SetLayerRecursively(child, parent.layer);
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }

        #endregion

        #region CreateUI

        public static Canvas CreateCanvas(GameObject parent = null, string name = "Canvas")
        {
            // Root for the UI
            var root = new GameObject(name);
            SetParentAndAlign(root, parent);
            root.layer = LayerMask.NameToLayer(kUILayerName);
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();

            // if there is no event system add one...
            CreateEventSystem();

            if (root.transform.parent as RectTransform)
            {
                RectTransform rect = root.transform as RectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.zero;
            }

            return canvas;
        }

        public static EventSystem CreateEventSystem(GameObject parent = null, string name = "EventSystem")
        {
            var esys = Object.FindObjectOfType<EventSystem>();
            if (esys == null)
            {
                var eventSystem = new GameObject(name);
                SetParentAndAlign(eventSystem, parent);
                esys = eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }

            return esys;
        }

        public static Image CreatePanel(GameObject parent = null, string name = "Panel")
        {

            GameObject panelRoot = CreateUIElementRoot(name, s_ThickElementSize);
            SetParentAndAlign(panelRoot, parent);
            panelRoot.layer = LayerMask.NameToLayer(kUILayerName);

            // Set RectTransform to stretch
            RectTransform rectTransform = panelRoot.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;

            Image image = panelRoot.AddComponent<Image>();
            image.sprite = null;
            image.type = Image.Type.Sliced;
            image.color = new Color(0, 0, 0, 0);

            return image;
        }

        public static Button CreateButton(GameObject parent = null, string name = "Button", string defaultLabel = "Button")
        {
            GameObject buttonRoot = CreateUIElementRoot(name, s_ThickElementSize);
            SetParentAndAlign(buttonRoot, parent);
            buttonRoot.layer = LayerMask.NameToLayer(kUILayerName);

            GameObject childText = new GameObject("Text");
            SetParentAndAlign(childText, buttonRoot);

            Image image = buttonRoot.AddComponent<Image>();
            image.sprite = null;
            image.type = Image.Type.Sliced;
            image.color = s_DefaultSelectableColor;

            Button bt = buttonRoot.AddComponent<Button>();
            SetDefaultColorTransitionValues(bt);

            Text text = childText.AddComponent<Text>();
            text.text = defaultLabel;
            text.font = font;
            text.alignment = TextAnchor.MiddleCenter;
            SetDefaultTextValues(text);

            RectTransform textRectTransform = childText.GetComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.sizeDelta = Vector2.zero;

            return bt;
        }

        public static Text CreateText(GameObject parent = null, string name = "Text", string defaultLabel = "Text")
        {
            GameObject go = CreateUIElementRoot(name, s_ThickElementSize);
            SetParentAndAlign(go, parent);
            go.layer = LayerMask.NameToLayer(kUILayerName);

            Text lbl = go.AddComponent<Text>();
            lbl.text = defaultLabel;
            lbl.font = font;
            SetDefaultTextValues(lbl);

            return lbl;
        }

        public static Image CreateImage(GameObject parent = null, string name = "Image")
        {
            GameObject go = CreateUIElementRoot(name, s_ImageElementSize);
            SetParentAndAlign(go, parent);
            go.layer = LayerMask.NameToLayer(kUILayerName);

            Image image = go.AddComponent<Image>();

            return image;
        }

        public static RawImage CreateRawImage(GameObject parent = null, string name = "RawImage")
        {
            GameObject go = CreateUIElementRoot(name, s_ImageElementSize);
            SetParentAndAlign(go, parent);
            go.layer = LayerMask.NameToLayer(kUILayerName);

            RawImage rawImage = go.AddComponent<RawImage>();

            return rawImage;
        }

        public static Slider CreateSlider(GameObject parent = null, string name = "Slider")
        {
            Resources resources = GetStandardResources();


            // Create GOs Hierarchy
            GameObject root = CreateUIElementRoot(name, s_ThinElementSize);
            SetParentAndAlign(root, parent);
            root.layer = LayerMask.NameToLayer(kUILayerName);

            GameObject background = CreateUIObject("Background", root);
            GameObject fillArea = CreateUIObject("Fill Area", root);
            GameObject fill = CreateUIObject("Fill", fillArea);
            GameObject handleArea = CreateUIObject("Handle Slide Area", root);
            GameObject handle = CreateUIObject("Handle", handleArea);

            // Background
            Image backgroundImage = background.AddComponent<Image>();
            backgroundImage.sprite = resources.background;
            backgroundImage.type = Image.Type.Sliced;
            backgroundImage.color = s_DefaultSelectableColor;
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0, 0.25f);
            backgroundRect.anchorMax = new Vector2(1, 0.75f);
            backgroundRect.sizeDelta = new Vector2(0, 0);

            // Fill Area
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1, 0.75f);
            fillAreaRect.anchoredPosition = new Vector2(-5, 0);
            fillAreaRect.sizeDelta = new Vector2(-20, 0);

            // Fill
            Image fillImage = fill.AddComponent<Image>();
            fillImage.sprite = resources.standard;
            fillImage.type = Image.Type.Sliced;
            fillImage.color = s_DefaultSelectableColor;

            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.sizeDelta = new Vector2(10, 0);

            // Handle Area
            RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.sizeDelta = new Vector2(-20, 0);
            handleAreaRect.anchorMin = new Vector2(0, 0);
            handleAreaRect.anchorMax = new Vector2(1, 1);

            // Handle
            Image handleImage = handle.AddComponent<Image>();
            handleImage.sprite = resources.knob;
            handleImage.color = s_DefaultSelectableColor;

            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 0);

            // Setup slider component
            Slider slider = root.AddComponent<Slider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            SetDefaultColorTransitionValues(slider);

            return slider;
        }

        public static Scrollbar CreateScrollbar(GameObject parent = null, string name = "Scrollbar")
        {
            Resources resources = GetStandardResources();


            // Create GOs Hierarchy
            GameObject scrollbarRoot = CreateUIElementRoot(name, s_ThinElementSize);
            SetParentAndAlign(scrollbarRoot, parent);
            scrollbarRoot.layer = LayerMask.NameToLayer(kUILayerName);

            GameObject sliderArea = CreateUIObject("Sliding Area", scrollbarRoot);
            GameObject handle = CreateUIObject("Handle", sliderArea);

            Image bgImage = scrollbarRoot.AddComponent<Image>();
            bgImage.sprite = resources.background;
            bgImage.type = Image.Type.Sliced;
            bgImage.color = s_DefaultSelectableColor;

            Image handleImage = handle.AddComponent<Image>();
            handleImage.sprite = resources.standard;
            handleImage.type = Image.Type.Sliced;
            handleImage.color = s_DefaultSelectableColor;

            RectTransform sliderAreaRect = sliderArea.GetComponent<RectTransform>();
            sliderAreaRect.sizeDelta = new Vector2(-20, -20);
            sliderAreaRect.anchorMin = Vector2.zero;
            sliderAreaRect.anchorMax = Vector2.one;

            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 20);

            Scrollbar scrollbar = scrollbarRoot.AddComponent<Scrollbar>();
            scrollbar.handleRect = handleRect;
            scrollbar.targetGraphic = handleImage;
            SetDefaultColorTransitionValues(scrollbar);

            return scrollbar;
        }

        public static Toggle CreateToggle(GameObject parent = null, string name = "Toggle", string defaultLabel = "Toggle", bool isOn = true)
        {
            Resources resources = GetStandardResources();


            // Set up hierarchy
            GameObject toggleRoot = CreateUIElementRoot(name, s_ThinElementSize);
            SetParentAndAlign(toggleRoot, parent);
            toggleRoot.layer = LayerMask.NameToLayer(kUILayerName);

            GameObject background = CreateUIObject("Background", toggleRoot);
            GameObject checkmark = CreateUIObject("Checkmark", background);
            GameObject childLabel = CreateUIObject("Label", toggleRoot);

            // Set up components
            Toggle toggle = toggleRoot.AddComponent<Toggle>();
            toggle.isOn = isOn;

            Image bgImage = background.AddComponent<Image>();
            bgImage.sprite = resources.standard;
            bgImage.type = Image.Type.Sliced;
            bgImage.color = s_DefaultSelectableColor;

            Image checkmarkImage = checkmark.AddComponent<Image>();
            checkmarkImage.sprite = resources.checkmark;

            Text label = childLabel.AddComponent<Text>();
            label.text = defaultLabel;
            label.font = resources.font;
            SetDefaultTextValues(label);

            toggle.graphic = checkmarkImage;
            toggle.targetGraphic = bgImage;
            SetDefaultColorTransitionValues(toggle);

            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 1f);
            bgRect.anchorMax = new Vector2(0f, 1f);
            bgRect.anchoredPosition = new Vector2(10f, -10f);
            bgRect.sizeDelta = new Vector2(kThinHeight, kThinHeight);

            RectTransform checkmarkRect = checkmark.GetComponent<RectTransform>();
            checkmarkRect.anchorMin = new Vector2(0.5f, 0.5f);
            checkmarkRect.anchorMax = new Vector2(0.5f, 0.5f);
            checkmarkRect.anchoredPosition = Vector2.zero;
            checkmarkRect.sizeDelta = new Vector2(20f, 20f);

            RectTransform labelRect = childLabel.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(23f, 1f);
            labelRect.offsetMax = new Vector2(-5f, -2f);

            return toggle;
        }

        public static InputField CreateInputField(GameObject parent = null, string name = "InputField", string defaultPlaceholder = "Enter text...")
        {
            Resources resources = GetStandardResources();


            GameObject root = CreateUIElementRoot(name, s_ThickElementSize);
            SetParentAndAlign(root, parent);
            root.layer = LayerMask.NameToLayer(kUILayerName);

            GameObject childPlaceholder = CreateUIObject("Placeholder", root);
            GameObject childText = CreateUIObject("Text", root);

            Image image = root.AddComponent<Image>();
            image.sprite = resources.inputField;
            image.type = Image.Type.Sliced;
            image.color = s_DefaultSelectableColor;

            InputField inputField = root.AddComponent<InputField>();
            SetDefaultColorTransitionValues(inputField);

            Text text = childText.AddComponent<Text>();
            text.text = "";
            text.font = resources.font;
            text.supportRichText = false;
            SetDefaultTextValues(text);

            Text placeholder = childPlaceholder.AddComponent<Text>();
            placeholder.text = defaultPlaceholder;
            placeholder.font = resources.font;
            placeholder.fontStyle = FontStyle.Italic;
            // Make placeholder color half as opaque as normal text color.
            Color placeholderColor = text.color;
            placeholderColor.a *= 0.5f;
            placeholder.color = placeholderColor;

            RectTransform textRectTransform = childText.GetComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.sizeDelta = Vector2.zero;
            textRectTransform.offsetMin = new Vector2(10, 6);
            textRectTransform.offsetMax = new Vector2(-10, -7);

            RectTransform placeholderRectTransform = childPlaceholder.GetComponent<RectTransform>();
            placeholderRectTransform.anchorMin = Vector2.zero;
            placeholderRectTransform.anchorMax = Vector2.one;
            placeholderRectTransform.sizeDelta = Vector2.zero;
            placeholderRectTransform.offsetMin = new Vector2(10, 6);
            placeholderRectTransform.offsetMax = new Vector2(-10, -7);

            inputField.textComponent = text;
            inputField.placeholder = placeholder;

            return inputField;
        }

        public static Dropdown CreateDropdown(GameObject parent = null, string name = "Dropdown", string defaultLabel = "Option A")
        {
            Resources resources = GetStandardResources();


            GameObject root = CreateUIElementRoot(name, s_ThickElementSize);
            SetParentAndAlign(root, parent);
            root.layer = LayerMask.NameToLayer(kUILayerName);

            GameObject label = CreateUIObject("Label", root);
            GameObject arrow = CreateUIObject("Arrow", root);
            GameObject template = CreateUIObject("Template", root);
            GameObject viewport = CreateUIObject("Viewport", template);
            GameObject content = CreateUIObject("Content", viewport);
            GameObject item = CreateUIObject("Item", content);
            GameObject itemBackground = CreateUIObject("Item Background", item);
            GameObject itemCheckmark = CreateUIObject("Item Checkmark", item);
            GameObject itemLabel = CreateUIObject("Item Label", item);

            // Sub controls.

            GameObject scrollbar = CreateScrollbar().gameObject;
            scrollbar.name = "Scrollbar";
            SetParentAndAlign(scrollbar, template);

            Scrollbar scrollbarScrollbar = scrollbar.GetComponent<Scrollbar>();
            scrollbarScrollbar.SetDirection(Scrollbar.Direction.BottomToTop, true);

            RectTransform vScrollbarRT = scrollbar.GetComponent<RectTransform>();
            vScrollbarRT.anchorMin = Vector2.right;
            vScrollbarRT.anchorMax = Vector2.one;
            vScrollbarRT.pivot = Vector2.one;
            vScrollbarRT.sizeDelta = new Vector2(vScrollbarRT.sizeDelta.x, 0);

            // Setup item UI components.

            Text itemLabelText = itemLabel.AddComponent<Text>();
            SetDefaultTextValues(itemLabelText);
            itemLabelText.font = resources.font;
            itemLabelText.alignment = TextAnchor.MiddleLeft;

            Image itemBackgroundImage = itemBackground.AddComponent<Image>();
            itemBackgroundImage.color = new Color32(245, 245, 245, 255);

            Image itemCheckmarkImage = itemCheckmark.AddComponent<Image>();
            itemCheckmarkImage.sprite = resources.checkmark;

            Toggle itemToggle = item.AddComponent<Toggle>();
            itemToggle.targetGraphic = itemBackgroundImage;
            itemToggle.graphic = itemCheckmarkImage;
            itemToggle.isOn = true;

            // Setup template UI components.

            Image templateImage = template.AddComponent<Image>();
            templateImage.sprite = resources.standard;
            templateImage.type = Image.Type.Sliced;

            ScrollRect templateScrollRect = template.AddComponent<ScrollRect>();
            templateScrollRect.content = (RectTransform)content.transform;
            templateScrollRect.viewport = (RectTransform)viewport.transform;
            templateScrollRect.horizontal = false;
            templateScrollRect.movementType = ScrollRect.MovementType.Clamped;
            templateScrollRect.verticalScrollbar = scrollbarScrollbar;
            templateScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            templateScrollRect.verticalScrollbarSpacing = -3;

            Mask scrollRectMask = viewport.AddComponent<Mask>();
            scrollRectMask.showMaskGraphic = false;

            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.sprite = resources.mask;
            viewportImage.type = Image.Type.Sliced;

            // Setup dropdown UI components.

            Text labelText = label.AddComponent<Text>();
            SetDefaultTextValues(labelText);
            labelText.font = resources.font;
            labelText.alignment = TextAnchor.MiddleLeft;

            Image arrowImage = arrow.AddComponent<Image>();
            arrowImage.sprite = resources.dropdown;

            Image backgroundImage = root.AddComponent<Image>();
            backgroundImage.sprite = resources.standard;
            backgroundImage.color = s_DefaultSelectableColor;
            backgroundImage.type = Image.Type.Sliced;

            Dropdown dropdown = root.AddComponent<Dropdown>();
            dropdown.targetGraphic = backgroundImage;
            SetDefaultColorTransitionValues(dropdown);
            dropdown.template = template.GetComponent<RectTransform>();
            dropdown.captionText = labelText;
            dropdown.itemText = itemLabelText;

            // Setting default Item list.
            labelText.text = defaultLabel;
            dropdown.options.Add(new Dropdown.OptionData { text = "Option A" });
            dropdown.options.Add(new Dropdown.OptionData { text = "Option B" });
            dropdown.options.Add(new Dropdown.OptionData { text = "Option C" });

            // Set up RectTransforms.

            RectTransform labelRT = label.GetComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(10, 6);
            labelRT.offsetMax = new Vector2(-25, -7);

            RectTransform arrowRT = arrow.GetComponent<RectTransform>();
            arrowRT.anchorMin = new Vector2(1, 0.5f);
            arrowRT.anchorMax = new Vector2(1, 0.5f);
            arrowRT.sizeDelta = new Vector2(20, 20);
            arrowRT.anchoredPosition = new Vector2(-15, 0);

            RectTransform templateRT = template.GetComponent<RectTransform>();
            templateRT.anchorMin = new Vector2(0, 0);
            templateRT.anchorMax = new Vector2(1, 0);
            templateRT.pivot = new Vector2(0.5f, 1);
            templateRT.anchoredPosition = new Vector2(0, 2);
            templateRT.sizeDelta = new Vector2(0, 150);

            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            viewportRT.anchorMin = new Vector2(0, 0);
            viewportRT.anchorMax = new Vector2(1, 1);
            viewportRT.sizeDelta = new Vector2(-18, 0);
            viewportRT.pivot = new Vector2(0, 1);

            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0f, 1);
            contentRT.anchorMax = new Vector2(1f, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.anchoredPosition = new Vector2(0, 0);
            contentRT.sizeDelta = new Vector2(0, 28);

            RectTransform itemRT = item.GetComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(0, 0.5f);
            itemRT.anchorMax = new Vector2(1, 0.5f);
            itemRT.sizeDelta = new Vector2(0, 20);

            RectTransform itemBackgroundRT = itemBackground.GetComponent<RectTransform>();
            itemBackgroundRT.anchorMin = Vector2.zero;
            itemBackgroundRT.anchorMax = Vector2.one;
            itemBackgroundRT.sizeDelta = Vector2.zero;

            RectTransform itemCheckmarkRT = itemCheckmark.GetComponent<RectTransform>();
            itemCheckmarkRT.anchorMin = new Vector2(0, 0.5f);
            itemCheckmarkRT.anchorMax = new Vector2(0, 0.5f);
            itemCheckmarkRT.sizeDelta = new Vector2(20, 20);
            itemCheckmarkRT.anchoredPosition = new Vector2(10, 0);

            RectTransform itemLabelRT = itemLabel.GetComponent<RectTransform>();
            itemLabelRT.anchorMin = Vector2.zero;
            itemLabelRT.anchorMax = Vector2.one;
            itemLabelRT.offsetMin = new Vector2(20, 1);
            itemLabelRT.offsetMax = new Vector2(-10, -2);

            template.SetActive(false);

            return dropdown;
        }

        public static ScrollRect CreateScrollView(GameObject parent = null, string name = "Scroll View", LayoutGroupType layoutGroupType = LayoutGroupType.Vertical)
        {
            Resources resources = GetStandardResources();


            GameObject root = CreateUIElementRoot(name, new Vector2(200, 200));
            SetParentAndAlign(root, parent);
            root.layer = LayerMask.NameToLayer(kUILayerName);

            GameObject viewport = CreateUIObject("Viewport", root);
            GameObject content = CreateUIObject("Content", viewport);

            // Sub controls.

            GameObject hScrollbar = CreateScrollbar().gameObject;
            hScrollbar.name = "Scrollbar Horizontal";
            SetParentAndAlign(hScrollbar, root);
            RectTransform hScrollbarRT = hScrollbar.GetComponent<RectTransform>();
            hScrollbarRT.anchorMin = Vector2.zero;
            hScrollbarRT.anchorMax = Vector2.right;
            hScrollbarRT.pivot = Vector2.zero;
            hScrollbarRT.sizeDelta = new Vector2(0, hScrollbarRT.sizeDelta.y);

            GameObject vScrollbar = CreateScrollbar().gameObject;
            vScrollbar.name = "Scrollbar Vertical";
            SetParentAndAlign(vScrollbar, root);
            vScrollbar.GetComponent<Scrollbar>().SetDirection(Scrollbar.Direction.BottomToTop, true);
            RectTransform vScrollbarRT = vScrollbar.GetComponent<RectTransform>();
            vScrollbarRT.anchorMin = Vector2.right;
            vScrollbarRT.anchorMax = Vector2.one;
            vScrollbarRT.pivot = Vector2.one;
            vScrollbarRT.sizeDelta = new Vector2(vScrollbarRT.sizeDelta.x, 0);

            // Setup RectTransforms.

            // Make viewport fill entire scroll view.
            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.sizeDelta = Vector2.zero;
            viewportRT.pivot = Vector2.up;

            // Make context match viewpoprt width and be somewhat taller.
            // This will show the vertical scrollbar and not the horizontal one.
            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = Vector2.up;
            contentRT.anchorMax = Vector2.one;
            contentRT.sizeDelta = new Vector2(0, 300);
            contentRT.pivot = Vector2.up;

            // Setup UI components.

            ScrollRect scrollRect = root.AddComponent<ScrollRect>();
            scrollRect.content = contentRT;
            scrollRect.viewport = viewportRT;
            scrollRect.horizontalScrollbar = hScrollbar.GetComponent<Scrollbar>();
            scrollRect.verticalScrollbar = vScrollbar.GetComponent<Scrollbar>();
            scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.horizontalScrollbarSpacing = -3;
            scrollRect.verticalScrollbarSpacing = -3;

            Image rootImage = root.AddComponent<Image>();
            rootImage.sprite = resources.background;
            rootImage.type = Image.Type.Sliced;
            rootImage.color = s_PanelColor;

            Mask viewportMask = viewport.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.sprite = resources.mask;
            viewportImage.type = Image.Type.Sliced;

            switch (layoutGroupType)
            {
                case LayoutGroupType.Grid:
                    content.AddComponent<GridLayoutGroup>();
                    break;

                case LayoutGroupType.Horizontal:
                    content.AddComponent<HorizontalLayoutGroup>();
                    break;

                case LayoutGroupType.Vertical:
                    content.AddComponent<VerticalLayoutGroup>();
                    break;
            }

            return scrollRect;
        }

        #endregion

        #endregion
    }
}