
using GuiBaseUI;
using UnityEngine;
using UnityEngine.UI;

namespace GuiScroll
{


    public class NewActorListScroll : MonoBehaviour
    {
        public static int lineCount = 1;
        BigDataScroll bigDataScroll;
        public ScrollRect scrollRect;
        public RectTransform rectContent;
        private int[] m_data;
        public int[] data
        {
            set
            {
                m_data = value;
                SetData();
            }
            get
            {
                return m_data;
            }
        }
        public bool isInit = false;


        //脚本附带ActorHolder上面
        public void Init()
        {
            InitUI();
        }

        private void InitUI()
        {
            isInit = true;

            Vector2 size = new Vector2(230, 830);
            Vector2 pos = new Vector2(0, 0);
            Vector2 cellSize = new Vector2(230, 97);
            float cellWidth = cellSize.x;
            float cellHeight = cellSize.y;
            // Main.Logger.Log("10");

            GameObject scrollView = CreateUI.NewScrollView(size, BarType.Vertical, ContentType.VerticalLayout); // 创建滑动UI
            scrollRect = scrollView.GetComponent<ScrollRect>(); // 拿到滑动组件
            //WorldMapSystem.instance.actorHolder = scrollRect.content; 
            rectContent = scrollRect.content; // 内容啊 content
            rectContent.GetComponent<ContentSizeFitter>().enabled = false; //关闭高度自动适应
            rectContent.GetComponent<VerticalLayoutGroup>().enabled = false; // 关闭自动排序
            // Main.Logger.Log("完");

            scrollRect.verticalNormalizedPosition = 1; // 设置最上的位置
            Image imgScrollView = scrollView.GetComponentInChildren<Image>(); // 拿到背景图
            imgScrollView.color = new Color(0.5f, 0.5f, 0.5f, 1f); // 背景图颜色
            imgScrollView.raycastTarget = false; // 设置背景不可点击
            RectTransform rScrollView = ((RectTransform)scrollView.transform); // 拿到滑动UI
            rScrollView.SetParent(gameObject.transform, false); // 设置父物体
            rScrollView.anchoredPosition = pos; // 设置位置

            //scrollView.GetComponentInChildren<Mask>().enabled = false;
            // Main.Logger.Log("完0");

            GameObject gItemCell = new GameObject("line", new System.Type[] { typeof(RectTransform) }); // 创建一行
            RectTransform rItemCell = gItemCell.GetComponent<RectTransform>(); // 得到transform
            rItemCell.SetParent(transform, false); // 设置父物体
            rItemCell.anchoredPosition = new Vector2(10000, 10000); // 隐藏于遥远虚空
            rItemCell.sizeDelta = new Vector2(cellWidth, cellHeight); // 设置大小
                                                                      //Image imgItemCell = gItemCell.AddComponent<Image>();
                                                                      //imgItemCell.color = new Color(1, 0, 0, 0.5f);
            // Main.Logger.Log("完成");

            GameObject prefab = ActorMenu.instance.listActor; // 拿到子物体预制件  ！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
            for (int i = 0; i < lineCount; i++) // 一行几个
            {
                GameObject go = UnityEngine.Object.Instantiate(prefab); // 创建每行子物体
                go.transform.SetParent(rItemCell, false); // 设置父物体
                Toggle tog = go.GetComponentInChildren<Toggle>();
                var tar = tog.targetGraphic;
                // Main.Logger.Log(tog + "隐藏" + tog.graphic + " " + tog.graphic.transform.parent);
                DestroyImmediate(tog);
                Button btn = go.AddComponent<Button>();
                btn.targetGraphic = tar;
            }
            // Main.Logger.Log("完成0");


            GridLayoutGroup gridLayoutGroup = gItemCell.AddComponent<GridLayoutGroup>();  // 控制子物体
            gridLayoutGroup.cellSize = prefab.GetComponent<RectTransform>().sizeDelta; // 设置子物体大小
            gridLayoutGroup.spacing = new Vector2(0, 0); // 子物体边距
            gridLayoutGroup.padding.left = (int)(12); // 左偏移
            gridLayoutGroup.padding.top = (int)(0); // 上偏移
            // Main.Logger.Log("完成1");


            ActorItem itemCell = gItemCell.AddComponent<ActorItem>(); // 添加大数据子物体关联组件
            bigDataScroll = gameObject.AddComponent<BigDataScroll>();  // 添加大数据管理组件
            bigDataScroll.Init(scrollRect, itemCell, SetCell); // 初始化大数据组件
            bigDataScroll.cellHeight = cellHeight; // 设置一行高度

            //GuiBaseUI.Main.LogAllChild(transform, true);



            // 以下是设置滑动条图片
            ScrollRect scroll = transform.GetComponent<ScrollRect>();
            // Main.Logger.Log("完成v");
            RectTransform otherRect = scroll.verticalScrollbar.GetComponent<RectTransform>();
            Image other = otherRect.GetComponent<Image>();
            // Main.Logger.Log("完成a");
            RectTransform myRect = scrollRect.verticalScrollbar.GetComponent<RectTransform>();
            //myRect.sizeDelta = new Vector2(10, 0);
            // Main.Logger.Log("完成b");
            Image my = myRect.GetComponent<Image>();
            // Main.Logger.Log("完成e");
            //my.color = new Color(0.9490196f, 0.509803951f, 0.503921571f);
            my.sprite = other.sprite;
            my.type = Image.Type.Sliced;
            // Main.Logger.Log("完成p");

            // Main.Logger.Log("完成V");
            RectTransform otherRect2 = scrollRect.verticalScrollbar.targetGraphic.GetComponent<RectTransform>();
            Image other2 = otherRect2.GetComponent<Image>();
            // Main.Logger.Log("完成A");
            RectTransform myRect2 = scrollRect.verticalScrollbar.targetGraphic.GetComponent<RectTransform>();
            // Main.Logger.Log("完成B");
            //myRect2.sizeDelta = new Vector2(10, 10);
            Image my2 = myRect2.GetComponent<Image>();
            // Main.Logger.Log("完成C");
            //my2.color = new Color(0.5882353f, 0.807843149f, 0.8156863f);
            my2.sprite = other2.sprite;
            my2.type = Image.Type.Sliced;
            // Main.Logger.Log("完成D");


            // Main.Logger.Log("完成3");
            SetData();

        }

        private void SetData()
        {
            if (bigDataScroll != null && m_data != null && isInit)
            {
                int count = m_data.Length / lineCount + 1;
                // Main.Logger.Log("=======！！！！！=======数据数量"+count);

                bigDataScroll.cellCount = count;
                //if (!Main.OnChangeList)
                //{
                //    scrollRect.verticalNormalizedPosition = 1;
                //}
            }
        }

        private void SetCell(ItemCell itemCell, int index)
        {
            // Main.Logger.Log(index.ToString() + "设置 itemCell。。。" + itemCell.ToString() + " pos=" + scrollRect.verticalNormalizedPosition.ToString());
            ActorItem item = itemCell as ActorItem;
            if (item == null)
            {
                // Main.Logger.Log("WarehouseItem出错。。。");
                return;
            }
            // Main.Logger.Log("数据长度：" + m_data.Length);
            ChildData[] childDatas = item.childDatas;
            for (int i = 0; i < lineCount; i++)
            {
                int idx = (index - 1) * lineCount + i;
                // Main.Logger.Log("循环" + i + "获取第【" + idx + "】个元素的数据");
                if (i < childDatas.Length)
                {
                    ChildData childData = childDatas[i];
                    GameObject go = childData.gameObject;
                    if (idx < m_data.Length)
                    {
                        go.transform.parent.gameObject.SetActive(true);
                        if (!go.activeSelf)
                        {
                            go.SetActive(true);
                        }
                        int num2 = m_data[idx];
                        itemCell.name = "Actor," + num2;
                        // Main.Logger.Log("改名A：" + itemCell.name);
                        if (itemCell.transform.childCount > i)
                        {
                            var child = itemCell.transform.GetChild(i);
                            child.name = "Actor," + num2;
                            // Main.Logger.Log("改名B：" + itemCell.name);
                            //Toggle tog = child.GetComponent<Toggle>();
                            //tog.group = ActorMenu.instance.listActorsHolder.GetComponent<ToggleGroup>();
                            // Main.Logger.Log(ActorMenu.instance.acotrId +" "+ num2 + "判断是否选中：" + (ActorMenu.instance.acotrId == num2));
                            //tog.isOn = ActorMenu.instance.acotrId == num2;
                            if (ActorMenu.instance.acotrId == num2 && ActorMenuActorListPatch.giveActorId == num2)
                            {
                                childData.Select(SelectState.All);
                            }
                            else if (ActorMenu.instance.acotrId == num2)
                            {
                                childData.Select(SelectState.Select);
                            }
                            else if (ActorMenuActorListPatch.giveActorId == num2)
                            {
                                childData.Select(SelectState.Give);
                            }
                            else
                            {
                                childData.Select(SelectState.Node);
                            }
                            Button btn = child.GetComponentInChildren<Button>();
                            btn.onClick.RemoveAllListeners();
                            btn.onClick.AddListener(delegate ()
                            {
                                ActorMenu.instance.SetActorAttr(int.Parse(go.name.Split(',')[1]));
                            });
                        }


                        SetListActor component = childData.setListActor;
                        component.SetActor(num2);

                        // Main.Logger.Log("此处是一些处理 不同身份的人处理方式不同");
                        if (!ActorMenu.instance.isEnemy)
                        {
                            if (num2 == DateFile.instance.MianActorID())
                            {
                                // Main.Logger.Log("主角");
                                component.SetInTeamIcon(true);
                                component.SetInBuildingIcon(false);
                                component.RestMoodFace();
                                component.listActorFavorText.text = "";
                                component.listActorSamsaraText.text = "";
                            }
                            else if (DateFile.instance.acotrTeamDate.Contains(num2))
                            {
                                // Main.Logger.Log("是出战队员");
                                component.SetInTeamIcon(true);
                                component.SetInBuildingIcon(false);
                                component.RestMoodFace();
                            }
                            else if (DateFile.instance.ParseInt(DateFile.instance.GetActorDate(num2, 27, addValue: false)) == 1)
                            {
                                // Main.Logger.Log("不知道是啥");
                                component.SetInTeamIcon(false);
                                component.SetInBuildingIcon(false);
                                component.RestMoodFace();
                            }
                            else if (DateFile.instance.ActorIsWorking(num2) != null)
                            {
                                // Main.Logger.Log("正在工作的?");
                                component.SetInTeamIcon(false);
                                component.SetInBuildingIcon(true);
                                component.RestMoodFace();
                            }
                            else
                            {
                                // Main.Logger.Log("其他");
                                component.SetInTeamIcon(false);
                                component.SetInBuildingIcon(false);
                                component.RestMoodFace();
                            }
                        }
                        else
                        {
                            // Main.Logger.Log("敌人");
                            component.SetInTeamIcon(false);
                            component.SetInBuildingIcon(false);
                            component.RestMoodFace();
                        }
                    }
                    else
                    {
                        if (go.activeSelf)
                        {
                            go.SetActive(false);
                        }
                    }
                    if (i == 0 && !go.transform.parent.gameObject.activeSelf)
                        go.transform.parent.gameObject.SetActive(true);

                }
                else
                {
                    // Main.Logger.Log("数据出错。。。");
                }
                // Main.Logger.Log("人物设置完毕");
            }
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy | m_data == null | scrollRect == null)
            {
                return;
            }
            var mousePosition = Input.mousePosition;
            var mouseOnPackage = mousePosition.x < Screen.width / 16 && mousePosition.y > Screen.width / 10 && mousePosition.y < Screen.width / 10 * 9;

            var v = Input.GetAxis("Mouse ScrollWheel");
            if (v != 0)
            {
                if (mouseOnPackage)
                {
                    float count = m_data.Length / lineCount + 1;
                    scrollRect.verticalNormalizedPosition += v / count * Main.settings.scrollSpeed;
                }
            }
        }
        public class ActorItem : ItemCell
        {

            public ChildData[] childDatas;
            public override void Awake()
            {
                base.Awake();
                childDatas = new ChildData[lineCount];
                for (int i = 0; i < lineCount; i++)
                {
                    Transform child = transform.GetChild(i);
                    childDatas[i] = new ChildData(child);
                }
                // Main.Logger.Log("WarehouseItem Awake " + childDatas.Length);
            }
        }
        public struct ChildData
        {
            public GameObject gameObject;
            public SetListActor setListActor;
            public Text des;
            public Image select;


            public ChildData(Transform child)
            {
                // Main.Logger.Log("创建一个");
                gameObject = child.gameObject;
                // Main.Logger.Log("aaa");
                setListActor = gameObject.GetComponent<SetListActor>();

                // Main.Logger.Log("获取" + setListActor);
                GameObject obj = GameObject.Instantiate<GameObject>(setListActor.listActorNameText.gameObject);
                // Main.Logger.Log("aaa");
                des = obj.GetComponent<Text>();
                // Main.Logger.Log("aaa");
                des.color = Color.red;
                // Main.Logger.Log("aaa");
                RectTransform tf = obj.GetComponent<RectTransform>();
                // Main.Logger.Log("aaa");
                tf.SetParent(gameObject.transform, false);
                // Main.Logger.Log("aaa");
                tf.sizeDelta = new Vector2(tf.sizeDelta.y, tf.sizeDelta.y);
                // Main.Logger.Log("aaa");
                tf.anchoredPosition = new Vector2(-30, 15);
                // Main.Logger.Log("aaa");

                foreach (var item in child)
                {
                    // Main.Logger.Log("... " + item);
                }

                select = child.Find("ListActorLabel").GetComponent<Image>();
                des.text = "赠";
            }

            public void Select(SelectState value)
            {
                select.enabled = (SelectState.Select & value) == SelectState.Select;
                des.enabled = (SelectState.Give & value) == SelectState.Give;
            }
        }

        public enum SelectState
        {
            Node = 0,
            Select = 1,
            Give = 2,
            All = 3,
        }
    }
}