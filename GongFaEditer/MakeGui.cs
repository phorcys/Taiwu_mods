using Ju.GongFaEditer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
public class MakeGui : MonoBehaviour
{
    public static MakeGui Instance;
    private Dictionary<string, Object> allComponets;
    public MainMenu parentInstance;
    public bool isShow = false;

    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

    }

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
        allComponets = new Dictionary<string, Object>();
        CreateBaseUI();
    }

    public void ShowOrHide(bool state)
    {
        if (state)
        {
            GameObject.Find("MainMenu").SetActive(false);
            uiWindow.transform.SetParent(GameObject.Find("MianMenuBack").transform);
            uiWindow.transform.SetAsLastSibling();
            uiWindow.rectTransform.localPosition = new Vector3(0, 0, 0);
            uiWindow.rectTransform.localScale = new Vector3(1, 1, 1);
            AddGongFa();
        }
        else
        {
            this.transform.Find("MainMenu").gameObject.SetActive(true);
        }
        uiWindow.gameObject.SetActive(state);
        isShow = state;
    }

    private void ClearAllChildren(Transform transform)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            Destroy(child.gameObject);
        }
    }

    private void AddGongFa(string str = "")
    {
        var contentRect = GetObjectComponent<RectTransform>("gongFaScrollViewportContent");
        ClearAllChildren(contentRect);
        ClearAllInput();
        var itemHeight = 10f;
        var allGongfa = DateFile.instance.allGongFaKey;
        if (!string.IsNullOrEmpty(str))
        {
            if (int.TryParse(str,out var gfId))
            {
                allGongfa = allGongfa.Where(gongfaId => DateFile.instance.gongFaDate.ContainsKey(gfId)).ToList();
            }
            else
            {
                allGongfa = allGongfa.Where(gongfaId => DateFile.instance.gongFaDate[gongfaId][0].Contains(str)).ToList();
            }
        }
        foreach (var gongfaId in allGongfa)
        {
            var buttonGameobject = new GameObject($"gfButton{gongfaId}", typeof(Button), typeof(Image));
            var buttonImage = buttonGameobject.GetComponent<Image>();
            buttonImage.sprite = borderSprite;
            buttonImage.color = new Color(0, 0, 0, 20);
            var button = buttonGameobject.GetComponent<Button>();
            var textGameobject = new GameObject($"gfTBtn{gongfaId}", typeof(RectTransform), typeof(Text));
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(() => { ChangeGongFa(gongfaId); });
            var text = textGameobject.GetComponent<Text>();
            text.rectTransform.anchorMax = new Vector2(1, 1);
            text.rectTransform.anchorMin = new Vector2(0, 0);
            text.text = DateFile.instance.gongFaDate[gongfaId][0];
            text.color = new Color(255, 255, 255);
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontStyle = FontStyle.Normal;
            text.fontSize = 20;
            text.alignment = TextAnchor.MiddleCenter;
            text.transform.SetParent(buttonGameobject.transform);
            buttonGameobject.transform.SetParent(contentRect.transform);
            buttonImage.rectTransform.localScale = new Vector3(1, 1, 1);
            itemHeight += buttonImage.rectTransform.rect.height + 5;
            buttonImage.rectTransform.anchoredPosition = Vector3.zero;
            buttonImage.rectTransform.anchoredPosition3D = Vector3.zero;
        }
        scrollRect.content.transform.localPosition = new Vector3(scrollRect.content.transform.localPosition.x, -itemHeight / 2);
        //AddAllItem(GetObjectComponent<Image>("editArea").transform);
    }

    private Image uiWindow;

    #region Window

    private void CreateBaseUI()
    {
        CreateMask();
        DrawWindowRect();
        uiWindow.rectTransform.SetParent(GameObject.Find("MianMenuBack").transform);
        uiWindow.gameObject.layer = GameObject.Find("MianMenuBack").layer;
        uiWindow.transform.SetAsLastSibling();
        uiWindow.rectTransform.localScale = new Vector3(1, 1, 1);
        uiWindow.gameObject.SetActive(false);
    }

    private int windowWidth;
    private int windowHeight;

    private void CreateMask()
    {
        this.transform.position = new Vector3(0, 0, 0);
        var gameobject = new GameObject("bgMask", typeof(Image));
        var ImageMask = gameobject.GetComponent<Image>();
        uiWindow = ImageMask;
        ImageMask.color = new Color(0, 0, 0, 100);
        ImageMask.rectTransform.localPosition = Vector3.zero;
        ImageMask.rectTransform.position = Vector3.zero;
        ImageMask.rectTransform.sizeDelta = this.GetComponent<RectTransform>().rect.size;
        //gameobject.transform.SetParent(this.transform);
        allComponets.Add("bgMask", ImageMask);
        windowWidth = (int)ImageMask.rectTransform.rect.width - (int)(ImageMask.rectTransform.rect.width / 6);
        windowHeight = (int)ImageMask.rectTransform.rect.height - (int)(ImageMask.rectTransform.rect.height / 6);
    }

    private Vector4 windowBorder;

    private Sprite borderSprite;

    private void DrawWindowRect()
    {
        var gameobject = new GameObject("uiWindow", typeof(Image));
        var image = gameobject.GetComponent<Image>();
        image.color = new Color(49, 43, 40, 255);
        image.type = Image.Type.Sliced;
        borderSprite = Resources.Load<Sprite>("Graphics/BaseUI/GUI_Window_Small_Black_NoColor");
        Debug.Log(borderSprite.name);
        image.sprite = borderSprite;
        windowBorder = new Vector4(borderSprite.border.x * 2, borderSprite.border.y * 2);
        image.rectTransform.sizeDelta = new Vector2(windowWidth, windowHeight);
        image.rectTransform.localPosition = Vector3.zero;
        gameobject.transform.SetParent(GetObjectComponent<Image>("bgMask").transform);
        allComponets.Add("uiWindow", image);
        DrawScrollView(gameobject.transform);
        DrawSearchArea(gameobject.transform);
        DrawEditArea(gameobject.transform);
        DrawCloseButton(gameobject.transform);
        DrawOkButton(gameobject.transform);
    }

    private void DrawCloseButton(Transform transform)
    {
        var gameobject = new GameObject("uiWindowCloseButton", typeof(Button), typeof(Image));
        var image = gameobject.GetComponent<Image>();
        var button = gameobject.GetComponent<Button>();
        image.sprite = Resources.Load<Sprite>("Graphics/BaseUI/ClosseButton");
        image.type = Image.Type.Simple;
        image.rectTransform.localPosition = new Vector3(windowWidth / 2, windowHeight / 2, 0);
        image.rectTransform.sizeDelta = new Vector2(80, 80);
        button.interactable = true;
        button.transition = Selectable.Transition.ColorTint;
        button.targetGraphic = image;
        button.onClick.AddListener(() => { ShowOrHide(false); });
        gameobject.transform.SetParent(transform);
    }

    private void DrawOkButton(Transform transform)
    {
        var gameobject = new GameObject("uiWindowOkButton", typeof(Button), typeof(Image));
        var image = gameobject.GetComponent<Image>();
        var button = gameobject.GetComponent<Button>();
        image.sprite = Resources.Load<Sprite>("Graphics/BaseUI/OkButton");
        image.type = Image.Type.Simple;
        image.rectTransform.localPosition = new Vector3(windowWidth / 2, -windowHeight / 2, 0);
        image.rectTransform.sizeDelta = new Vector2(80, 80);
        button.interactable = true;
        button.transition = Selectable.Transition.ColorTint;
        button.targetGraphic = image;
        button.onClick.AddListener(() => {
            Main.SaveChangedGongFa();
            ShowOrHide(false);
        });
        gameobject.transform.SetParent(transform);
    }

    #region ScrollView

    private ScrollRect scrollRect;


    private void DrawSearchArea(Transform transform)
    {
        var gameobject = new GameObject("searchArea", typeof(RectTransform));
        var image = gameobject.GetComponent<RectTransform>();
        var rectWidth = 300;
        var rectHeight = 50;
        image.anchorMin = new Vector2(0, 1);
        image.anchorMax = new Vector2(0, 1);
        image.pivot = new Vector2(0, 1);
        image.sizeDelta = new Vector2(rectWidth, rectHeight);
        image.localPosition = Vector3.zero;
        var sprite = borderSprite;
        allComponets.Add("searchArea", image);
        DrawSearchInputField(image.transform);
        gameobject.transform.SetParent(transform);
        image.anchoredPosition = new Vector2(windowBorder.x, -30);
        image.anchoredPosition3D = new Vector2(windowBorder.x, -30);
    }

    private void DrawSearchInputField(Transform transform)
    {
        var gameobject = new GameObject($"tSearchInput", typeof(Image), typeof(InputField));
        var inputField = gameobject.GetComponent<InputField>();
        var inputImage = gameobject.GetComponent<Image>();
        var sprite = Resources.Load<Sprite>("Graphics/BaseUI/GUI_ValuBack_NoBack");
        inputImage.sprite = sprite;
        inputImage.type = Image.Type.Sliced;
        inputImage.color = new Color(255, 255, 255);
        inputImage.rectTransform.sizeDelta = new Vector2(300, 50);

        var inputText = CreateText("text", "", 300 - 20 * 2);
        inputText.rectTransform.anchorMin = new Vector2(0, 0);
        inputText.rectTransform.anchorMax = new Vector2(1f, 1f);
        inputText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        inputText.transform.SetParent(inputField.transform);
        inputText.rectTransform.offsetMin = new Vector2(-1, -1);
        inputText.rectTransform.offsetMax = new Vector2(-1, -1);

        inputField.targetGraphic = inputImage;
        inputField.textComponent = inputText;
        inputField.onEndEdit = new InputField.SubmitEvent();
        inputField.onEndEdit.AddListener(value =>
        {
            AddGongFa(value);
        });
        inputField.transform.SetParent(transform);
        inputImage.rectTransform.anchoredPosition = Vector3.zero;
        inputImage.rectTransform.anchoredPosition3D = Vector3.zero;
    }

    private void DrawScrollView(Transform transform)
    {
        var gameobject = new GameObject("gongFaScrollView", typeof(ScrollRect), typeof(Image));
        var scrollview = scrollRect = gameobject.GetComponent<ScrollRect>();
        var scrollviewRect = gameobject.GetComponent<RectTransform>();
        gameobject.transform.SetParent(transform);
        allComponets.Add("gongFaScrollView", scrollview);
        allComponets.Add("gongFaScrollViewRect", scrollviewRect);

        var image = gameobject.GetComponent<Image>();
        image.color = new Color(0, 0, 0, 0);

        scrollview.horizontal = false;
        //scrollviewRect.localPosition = new Vector3(windowBorder.x, 0, 0);
        scrollviewRect.anchorMin = new Vector2(0, 0);
        scrollviewRect.anchorMax = new Vector2(0, 1);
        scrollviewRect.pivot = new Vector2(0, 0);
        scrollviewRect.offsetMin = new Vector2(windowBorder.x, windowBorder.y);
        scrollviewRect.offsetMax = new Vector2(0f, windowBorder.y);
        scrollviewRect.sizeDelta = new Vector2(300, -windowBorder.y * 2 - 80);
        scrollview.movementType = ScrollRect.MovementType.Elastic;
        scrollview.elasticity = 0.1f;
        scrollview.inertia = true;
        scrollview.decelerationRate = 0.135f;

        scrollview.viewport = CreateViewport(gameobject.transform, out var conetnt);
        scrollview.viewport.anchorMin = new Vector2(0, 0);
        scrollview.viewport.anchorMax = new Vector2(1, 1);
        scrollview.viewport.pivot = new Vector2(0, 0);
        scrollview.viewport.offsetMin = new Vector2(0, 0);
        scrollview.viewport.offsetMax = new Vector2(-scrollbarWidth, 0);
        //scrollview.verticalScrollbar = CreateVerticalBar(gameobject.transform);
        //scrollview.verticalScrollbar.direction = Scrollbar.Direction.TopToBottom;
        //scrollview.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

        scrollview.content = conetnt;
    }

    private Scrollbar CreateVerticalBar(Transform transform)
    {
        var gameobject = new GameObject("gongFaScrollBar", typeof(Image), typeof(Scrollbar));
        var image = gameobject.GetComponent<Image>();
        gameobject.transform.SetParent(transform);
        allComponets.Add("gongFaScrollBar", image);
        image.color = new Color(255, 255, 255, 50);
        var scrollbar = gameobject.GetComponent<Scrollbar>();
        scrollbar.interactable = true;
        scrollbar.transition = Selectable.Transition.ColorTint;
        image.rectTransform.anchorMin = new Vector2(1, 0);
        image.rectTransform.anchorMax = new Vector2(1, 1);
        image.rectTransform.pivot = new Vector2(1, 1);
        image.rectTransform.offsetMin = new Vector2(-scrollbarWidth, 0);
        image.rectTransform.offsetMax = new Vector2(0, 0);
        scrollbar.handleRect = CreateSlidingArea(gameobject.transform);
        return scrollbar;
    }

    private int scrollbarWidth = 10;

    private RectTransform CreateSlidingArea(Transform transform)
    {
        var gameobject = new GameObject("SlidingArea", typeof(RectTransform));
        var rectTransform = gameobject.GetComponent<RectTransform>();
        gameobject.transform.SetParent(transform);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.offsetMin = new Vector2(scrollbarWidth / 2, scrollbarWidth / 2);
        rectTransform.offsetMax = new Vector2(-scrollbarWidth / 2, -scrollbarWidth / 2);
        gameobject.transform.SetParent(transform);
        return CreateHandle(gameobject.transform);
    }

    private RectTransform CreateHandle(Transform transform)
    {
        var gameobject = new GameObject("SliderHandle", typeof(Image));
        var image = gameobject.GetComponent<Image>();
        gameobject.transform.SetParent(transform);
        image.rectTransform.anchorMin = new Vector2(0f, 0f);
        image.rectTransform.anchorMax = new Vector2(1f, 1f);
        image.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        image.rectTransform.offsetMin = new Vector2(-scrollbarWidth / 2, -scrollbarWidth / 2);
        image.rectTransform.offsetMax = new Vector2(scrollbarWidth / 2, scrollbarWidth / 2);
        return image.rectTransform;
    }

    private RectTransform CreateViewport(Transform transform, out RectTransform contentTransform)
    {
        var gameobject = new GameObject("gongFaScrollViewport", typeof(Image), typeof(Mask));
        var image = gameobject.GetComponent<Image>();
        image.raycastTarget = image.fillCenter = true;
        image.type = Image.Type.Sliced;
        image.sprite = borderSprite;
        image.color = new Color(35, 35, 35, 200);
        gameobject.transform.SetParent(transform);
        allComponets.Add("gongFaScrollViewport", image);
        contentTransform = CreateViewportContent(gameobject.transform);
        return image.rectTransform;
    }

    private RectTransform CreateViewportContent(Transform transform)
    {
        var gameobject = new GameObject("gongFaScrollViewportContent", typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        var glg = gameobject.GetComponent<GridLayoutGroup>();
        var csf = gameobject.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        glg.padding = new RectOffset(5, 5, 10, 5);
        glg.cellSize = new Vector2(200, 50);
        glg.spacing = new Vector2(0, 5);
        glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
        glg.startAxis = GridLayoutGroup.Axis.Vertical;
        glg.childAlignment = TextAnchor.UpperCenter;
        glg.constraint = GridLayoutGroup.Constraint.Flexible;
        var rectTransform = gameobject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        gameobject.transform.SetParent(transform);
        allComponets.Add("gongFaScrollViewportContent", rectTransform);
        return gameobject.GetComponent<RectTransform>();
    }

    #endregion

    private void DrawEditArea(Transform parent)
    {
        var gameobject = new GameObject("editArea", typeof(Image));
        gameobject.transform.SetParent(parent);
        var image = gameobject.GetComponent<Image>();
        image.rectTransform.anchorMin = new Vector2(0, 1);
        image.rectTransform.anchorMax = new Vector2(0, 1);
        var rectWidth = windowWidth - scrollRect.GetComponent<RectTransform>().rect.width - windowBorder.x * 2.5f;
        var rectHeight = (int)(windowHeight - windowBorder.y * 2);
        var xMove = windowWidth / 2 - scrollRect.GetComponent<RectTransform>().rect.width - windowBorder.x * 1.5f - rectWidth / 2;
        image.rectTransform.sizeDelta = new Vector2(rectWidth, rectHeight);
        image.rectTransform.localPosition = new Vector3(-xMove, 0, 0);
        image.type = Image.Type.Sliced;
        var sprite = borderSprite;
        image.sprite = sprite;
        allComponets.Add("editArea", image);
        padding = new Vector2(5, 5);
        DrawEditText(image.rectTransform);
    }

    private Dictionary<string, InputField> AllInput = new Dictionary<string, InputField>();

    private void DrawEditText(RectTransform parent)
    {
        var currentItemId = 1;
        var currentRow = 1;
        var currentX = 20f;
        var currentY = 20f;
        properties = typeof(GongFa).GetProperties().ToList();
        foreach (var prop in properties)
        {
            var gfAttr = prop.GetCustomAttribute<GongFaAttribute>();
            if (gfAttr == null) continue;
            if (!gfAttr.Enable) continue;
            var area = new GameObject($"edit{currentRow}.{currentItemId}", typeof(Image));
            var areaImage = area.GetComponent<Image>();
            areaImage.sprite = borderSprite;
            areaImage.type = Image.Type.Sliced;
            areaImage.color = new Color(0, 0, 0, 0);
            areaImage.rectTransform.anchorMin = new Vector2(0, 1);
            areaImage.rectTransform.anchorMax = new Vector2(0, 1);
            areaImage.transform.SetParent(parent.transform);
            var text = CreateText(prop.Name, gfAttr.DisplayName);
            text.rectTransform.anchorMin = new Vector2(0, 1f);
            text.rectTransform.anchorMax = new Vector2(0, 1f);
            text.rectTransform.pivot = new Vector2(0, 1);
            var inputField = prop.Name == "Description" ? CreateInputField(prop.Name, 1000) : prop.PropertyType == typeof(string) ? CreateInputField(prop.Name, 20 * 5) : CreateInputField(prop.Name, 100);
            var inputRect = inputField.GetComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0, 1f);
            inputRect.anchorMax = new Vector2(0, 1f);
            inputRect.pivot = new Vector2(0, 1);
            text.transform.SetParent(area.transform);
            inputField.transform.SetParent(area.transform);
            areaImage.rectTransform.sizeDelta = new Vector2(text.rectTransform.rect.width + inputField.GetComponent<RectTransform>().rect.width, editHeight);
            if (currentX + areaImage.rectTransform.rect.width > parent.rect.width - 40)
            {
                currentItemId = 1;
                currentX = 20f;
                currentY += editHeight + 10;
                currentRow++;
            }
            areaImage.rectTransform.pivot = new Vector2(0, 1);
            text.transform.localPosition = new Vector3(0, 0, 0);
            inputRect.transform.localPosition = new Vector3(text.rectTransform.rect.width, 0, 0);
            area.transform.position = new Vector3(currentX - areaImage.rectTransform.anchoredPosition.x, -currentY - areaImage.rectTransform.anchoredPosition.y, 0);
            currentX += areaImage.rectTransform.rect.width;
            currentItemId++;
            AllInput.Add(prop.Name, inputField);
        }
        //AllArea.ForEach(t =>
        //{
        //    t.transform.localPosition = new Vector3(t.transform.localPosition.x - 514,t.transform.localPosition.y + 457.5f,0);
        //    t.transform.localScale = new Vector3(1,1,1);
        //    t.transform.localRotation = new Quaternion(0,0,0,0);
        //});
    }

    private int editHeight = 30;

    private Text CreateText(string displayName, string textStr, int Width = 0)
    {
        var gameobject = new GameObject($"t{displayName}", typeof(Text));
        var text = gameobject.GetComponent<Text>();
        text.text = textStr;
        text.color = new Color(255, 255, 255);
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontStyle = FontStyle.Normal;
        text.fontSize = 20;
        text.alignment = TextAnchor.MiddleCenter;
        text.supportRichText = false;
        text.rectTransform.sizeDelta = new Vector2(Width > 0 ? Width : 22 * textStr.Length, editHeight);
        return text;
    }

    private InputField CreateInputField(string displayName, int Width)
    {
        var gameobject = new GameObject($"t{displayName}", typeof(Image), typeof(InputField));
        var inputField = gameobject.GetComponent<InputField>();
        var inputImage = gameobject.GetComponent<Image>();
        var sprite = Resources.Load<Sprite>("Graphics/BaseUI/GUI_ValuBack_NoBack");
        inputImage.sprite = sprite;
        inputImage.type = Image.Type.Sliced;
        inputImage.color = new Color(255, 255, 255);
        inputImage.rectTransform.sizeDelta = new Vector2(Width, editHeight);

        var inputText = CreateText("text", "", Width);
        inputText.rectTransform.anchorMin = new Vector2(0, 0);
        inputText.rectTransform.anchorMax = new Vector2(1f, 1f);
        inputText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        inputText.transform.SetParent(inputField.transform);
        inputText.rectTransform.offsetMin = new Vector2(-1, -1);
        inputText.rectTransform.offsetMax = new Vector2(-1, -1);

        inputField.targetGraphic = inputImage;
        inputField.textComponent = inputText;
        return inputField;
    }

    private void CreateSearchArea()
    {

    }

    private Vector2 padding;

    private List<PropertyInfo> properties;
    private GongFa c_gongfa;
    private int gongFaId = 0;

    public void ClearAllInput()
    {
        foreach (InputField input in AllInput.Values)
        {
            input.text = "";
            input.onEndEdit = new InputField.SubmitEvent();
        }
    }

    public void ChangeGongFa(int id)
    {
        c_gongfa = null;
        c_gongfa = GetGongFa(id);
        foreach (var item in properties)
        {
            if (AllInput.TryGetValue(item.Name, out var input))
            {
                var value = item.GetValue(c_gongfa).ToString();
                input.text = item.GetValue(c_gongfa).ToString();
                var tempColor = input.GetComponent<Image>().color;
                input.onEndEdit = new InputField.SubmitEvent();
                input.onEndEdit.AddListener((v) =>
                {
                    if (v == value) return;
                    if (string.IsNullOrEmpty(v))
                    {
                        input.text = value;
                        input.GetComponent<Image>().color = Color.red;
                        return;
                    }
                    var gfAttr = item.GetCustomAttribute<GongFaAttribute>();
                    if ((item.PropertyType.Equals(typeof(int)) || item.PropertyType.Equals(typeof(float))) && (gfAttr.Max != int.MaxValue || gfAttr.Min != int.MinValue))
                    {
                        if (item.PropertyType.Equals(typeof(float)) && int.TryParse(v, out var i))
                        {
                            if (gfAttr.Max!= int.MaxValue && i > gfAttr.Max)
                            {
                                input.text = value;
                                input.GetComponent<Image>().color = Color.red;
                                return;
                            }
                            if (gfAttr.Min != int.MinValue && i < gfAttr.Min)
                            {
                                input.text = value;
                                input.GetComponent<Image>().color = Color.red;
                                return;
                            }
                        }
                        if (item.PropertyType.Equals(typeof(float)) && float.TryParse(v, out var f))
                        {
                            if (gfAttr.Max != int.MaxValue && f > gfAttr.Max)
                            {
                                input.text = value;
                                input.GetComponent<Image>().color = Color.red;
                                return;
                            }
                            if (gfAttr.Min != int.MinValue && f < gfAttr.Min)
                            {
                                input.text = value;
                                input.GetComponent<Image>().color = Color.red;
                                return;
                            }
                        }
                        if (item.PropertyType.Equals(typeof(decimal)) && decimal.TryParse(v, out var d))
                        {
                            if (gfAttr.Max != int.MaxValue && d > gfAttr.Max)
                            {
                                input.text = value;
                                input.GetComponent<Image>().color = Color.red;
                                return;
                            }
                            if (gfAttr.Min != int.MinValue && d < gfAttr.Min)
                            {
                                input.text = value;
                                input.GetComponent<Image>().color = Color.red;
                                return;
                            }
                        }
                    }
                    input.GetComponent<Image>().color = tempColor;
                    if (!Main.EditHostory.ContainsKey(id))
                    {
                        Main.Logger.Log($"功法改动{id}不存在,添加{id},{gfAttr.Index},{DateFile.instance.gongFaDate[id][gfAttr.Index]},{v}");
                        var editValue = new Dictionary<int, string[]>();
                        editValue.Add(gfAttr.Index, new string[] { DateFile.instance.gongFaDate[id][gfAttr.Index], v });
                        Main.EditHostory.Add(id, editValue);
                    }
                    else
                    {
                        if (!Main.EditHostory[id].ContainsKey(gfAttr.Index))
                        {
                            Main.Logger.Log($"功法改动{id}存在,但不存在{gfAttr.Index}的改动,添加{gfAttr.Index},{DateFile.instance.gongFaDate[id][gfAttr.Index]},{v}");
                            Main.EditHostory[id].Add(gfAttr.Index, new string[] { DateFile.instance.gongFaDate[id][gfAttr.Index], v });
                        }
                        else
                        {
                            Main.Logger.Log($"功法改动{id}和{gfAttr.Index}存在,更改({Main.EditHostory[id][gfAttr.Index][1]})->({v})");
                            Main.EditHostory[id][gfAttr.Index][1] = v;
                        }
                    }
                    DateFile.instance.gongFaDate[id][gfAttr.Index] = v;
                    item.SetValue(c_gongfa, DataConvert(v, item.PropertyType), null);

                        //if (!ChangedGongFa.ContainsKey(id))
                        //{
                        //    ChangedGongFa.Add(id, c_gongfa);
                        //}
                        //else
                        //{
                        //    ChangedGongFa[id] = c_gongfa;
                        //}
                    });
            }
        }
    }

    public GongFa GetGongFa(int gongfaId)
    {
        var newGf = new GongFa();
        newGf.GongFaId = gongfaId;
        var gongfa = DateFile.instance.gongFaDate[gongfaId];
        foreach (var prop in newGf.GetType().GetProperties())
        {
            var gfAttr = prop.GetCustomAttribute<GongFaAttribute>();
            if (gfAttr == null) continue;
            try
            {
                prop.SetValue(newGf, DataConvert(gongfa[gfAttr.Index], prop.PropertyType), null);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"数据转换错误,字段索引:{gfAttr.Index},字段名称;{gfAttr.DisplayName}");
                throw;
            }
        }
        return newGf;
    }

    public object DataConvert(string data, System.Type type)
    {
        if (typeof(int) == type)
        {
            return int.Parse(data);
        }
        if (typeof(decimal) == type)
        {
            return decimal.Parse(data);
        }
        if (typeof(float) == type)
        {
            return float.Parse(data);
        }
        return data;
    }

    public void OnInputValueChange()
    {

    }

    //private Button CreateButton()
    //{
    //    var gameobject = new GameObject("");
    //}

    private Object GetObject(string name)
    {
        if (allComponets.TryGetValue(name, out var gb))
        {
            return gb;
        }
        return null;
    }

    private T GetObjectComponent<T>(string name) where T : Object
    {
        return GetObject(name) as T;
    }
    #endregion
}

