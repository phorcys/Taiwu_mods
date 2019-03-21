
using GuiBaseUI;
using UnityEngine;
using UnityEngine.UI;

namespace GuiScroll
{


    public class NewItemPackage : MonoBehaviour
    {
        public static int lineCount = 9;
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

            Vector2 size = new Vector2(730, 410);
            Vector2 pos = new Vector2(0, 0);
            Vector2 cellSize = new Vector2(80, 80);
            float cellWidth = cellSize.x;
            float cellHeight = cellSize.y;
            Main.Logger.Log("10");

            GameObject scrollView = CreateUI.NewScrollView(size, BarType.Vertical, ContentType.VerticalLayout); // 创建滑动UI
            scrollRect = scrollView.GetComponent<ScrollRect>(); // 拿到滑动组件
            //WorldMapSystem.instance.actorHolder = scrollRect.content; 
            rectContent = scrollRect.content; // 内容啊 content
            rectContent.GetComponent<ContentSizeFitter>().enabled = false; //关闭高度自动适应
            rectContent.GetComponent<VerticalLayoutGroup>().enabled = false; // 关闭自动排序
            Main.Logger.Log("完");

            scrollRect.verticalNormalizedPosition = 1; // 设置最上的位置
            Image imgScrollView = scrollView.GetComponentInChildren<Image>(); // 拿到背景图
            //imgScrollView.color = new Color(0f, 0f, 0f, 1f); // 背景图颜色
            imgScrollView.raycastTarget = false; // 设置背景不可点击
            RectTransform rScrollView = ((RectTransform)scrollView.transform); // 拿到滑动UI
            rScrollView.SetParent(gameObject.transform, false); // 设置父物体
            rScrollView.anchoredPosition = pos; // 设置位置

            //scrollView.GetComponentInChildren<Mask>().enabled = false;
            Main.Logger.Log("完0");

            GameObject gItemCell = new GameObject("line", new System.Type[] { typeof(RectTransform) }); // 创建一行
            RectTransform rItemCell = gItemCell.GetComponent<RectTransform>(); // 得到transform
            rItemCell.SetParent(transform, false); // 设置父物体
            rItemCell.anchoredPosition = new Vector2(10000, 10000); // 隐藏于遥远虚空
            rItemCell.sizeDelta = new Vector2(720, 85); // 设置大小

            // 测试图片
            //Image imgItemCell = gItemCell.AddComponent<Image>();
            //imgItemCell.color = new Color(1, 0, 0, 0.5f);
            Main.Logger.Log("完成");

            GameObject prefab = ActorMenu.instance.itemIconNoToggle; // 拿到子物体预制件  ！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
            for (int i = 0; i < lineCount; i++) // 一行几个
            {
                GameObject go = UnityEngine.Object.Instantiate(prefab); // 创建每行子物体
                go.transform.SetParent(rItemCell, false); // 设置父物体
                var tar = go.GetComponentInChildren<Image>();
                Button btn = go.AddComponent<Button>();
                btn.targetGraphic = tar;
            }
            Main.Logger.Log("完成0");


            GridLayoutGroup gridLayoutGroup = gItemCell.AddComponent<GridLayoutGroup>();  // 控制子物体
            gridLayoutGroup.cellSize = prefab.GetComponent<RectTransform>().sizeDelta; // 设置子物体大小
            gridLayoutGroup.spacing = new Vector2(7.5f, 0); // 子物体边距
            gridLayoutGroup.padding.left = (int)(5); // 左偏移
            gridLayoutGroup.padding.top = (int)(5); // 上偏移
            Main.Logger.Log("完成1");


            PackageItem itemCell = gItemCell.AddComponent<PackageItem>(); // 添加大数据子物体关联组件
            bigDataScroll = gameObject.AddComponent<BigDataScroll>();  // 添加大数据管理组件
            bigDataScroll.Init(scrollRect, itemCell, SetCell); // 初始化大数据组件
            bigDataScroll.cellHeight = 85; // 设置一行高度

            //GuiBaseUI.Main.LogAllChild(transform, true);



            // 以下是设置滑动条图片
            ScrollRect scroll = transform.GetComponent<ScrollRect>();
            Main.Logger.Log("完成v");
            RectTransform otherRect = scroll.verticalScrollbar.GetComponent<RectTransform>();
            Image other = otherRect.GetComponent<Image>();
            Main.Logger.Log("完成a");
            RectTransform myRect = scrollRect.verticalScrollbar.GetComponent<RectTransform>();
            //myRect.sizeDelta = new Vector2(10, 0);
            Main.Logger.Log("完成b");
            Image my = myRect.GetComponent<Image>();
            Main.Logger.Log("完成e");
            //my.color = new Color(0.9490196f, 0.509803951f, 0.503921571f);
            my.sprite = other.sprite;
            my.type = Image.Type.Sliced;
            Main.Logger.Log("完成p");

            Main.Logger.Log("完成V");
            RectTransform otherRect2 = scrollRect.verticalScrollbar.targetGraphic.GetComponent<RectTransform>();
            Image other2 = otherRect2.GetComponent<Image>();
            Main.Logger.Log("完成A");
            RectTransform myRect2 = scrollRect.verticalScrollbar.targetGraphic.GetComponent<RectTransform>();
            Main.Logger.Log("完成B");
            //myRect2.sizeDelta = new Vector2(10, 10);
            Image my2 = myRect2.GetComponent<Image>();
            Main.Logger.Log("完成C");
            //my2.color = new Color(0.5882353f, 0.807843149f, 0.8156863f);
            my2.sprite = other2.sprite;
            my2.type = Image.Type.Sliced;
            Main.Logger.Log("完成D");


            Main.Logger.Log("完成3");
            SetData();

        }

        private void SetData()
        {
            if (bigDataScroll != null && m_data != null && isInit)
            {
                int count = m_data.Length / lineCount + 1;
                Main.Logger.Log("=======！！！！！=======数据数量" + count);

                bigDataScroll.cellCount = count;
                //if (!Main.OnChangeList)
                //{
                //    scrollRect.verticalNormalizedPosition = 1;
                //}
            }
        }

        private void SetCell(ItemCell itemCell, int index)
        {
            int mainActorId = DateFile.instance.MianActorID();
            int key = ActorMenuItemPackagePatch.Key;
            int typ = ActorMenuItemPackagePatch.Typ;
            int actorFavor = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), key, false, true);
            ActorMenu _this = ActorMenu.instance;
            Main.Logger.Log(index.ToString() + "设置 itemCell。。。" + itemCell.ToString() + " pos=" + scrollRect.verticalNormalizedPosition.ToString());
            PackageItem item = itemCell as PackageItem;
            if (item == null)
            {
                Main.Logger.Log("WarehouseItem出错。。。");
                return;
            }
            Main.Logger.Log("数据长度：" + m_data.Length);
            ChildData[] childDatas = item.childDatas;
            for (int i = 0; i < lineCount; i++)
            {
                int idx = (index - 1) * lineCount + i;
                Main.Logger.Log("循环" + i + "获取第【" + idx + "】个元素的数据");
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
                        go.name = "Item," + num2;
                        Main.Logger.Log("改物品名A：" + go.name);
                        SetItem setItem = childData.setItem;
                        setItem.SetActorMenuItemIcon(key, num2, actorFavor, _this.injuryTyp);
                        //setItem.SetItemAdd(key, num2, transform);
                        Button btn = go.GetComponentInChildren<Button>();
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(delegate ()
                        {
                            ClickItem(num2, setItem);
                        });
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
                    Main.Logger.Log("数据出错。。。");
                }
                Main.Logger.Log("物品设置完毕");
            }
        }

        private void ClickItem(int itemId, SetItem setItem)
        {
            if(setItem.notakeIcon.activeSelf&&setItem.notakeIcon.GetComponent<Image>().color == Color.red)
            {
                YesOrNoWindow.instance.SetYesOrNoWindow(-1, "触动禁制", "这个东西被锁住了！", false, true);
                return;
            }

            int actorId = ActorMenuActorListPatch.acotrId;
            if (DateFile.instance.ActorIsInBattle(actorId) != 0)
            {
                YesOrNoWindow.instance.SetYesOrNoWindow(-1, "战斗中", "战斗中不能使用这个功能！", false, true);
                return;
            }
            int giveId = ActorMenuActorListPatch.giveActorId;
            if(actorId!= giveId)
            {
                if(Input.GetKey(KeyCode.LeftControl)|| Input.GetKey(KeyCode.RightControl))
                {
                    Main.Logger.Log(actorId + "赠送" + giveId + "物品" + itemId);
                }
                else
                {
                    Main.Logger.Log(actorId + "暂存" + giveId + "物品" + itemId);

                    Vector3 start, target = new Vector3(-1000, 0, 0);
                    start = setItem.transform.position;
                    bool get_target = false;
                    Sprite sprite = setItem.itemIcon.GetComponent<Image>().sprite;
                    int star_id = 0, end_id = 0;
                    Vector3 start_pos = Vector3.zero, end_pos = Vector3.zero;
                    Vector3 dis = Vector3.zero;
                    int for_end = ActorMenuActorListPatch.m_listActorsHolder.rectContent.childCount - 1;
                    int idx = 0;
                    for (int i = 0; i <= for_end; i++)
                    {
                        Transform child = ActorMenuActorListPatch.m_listActorsHolder.rectContent.GetChild(i);
                        string[] ss = child.name.Split(',');
                        if(ss.Length <= 1)
                        {
                            continue;
                        }
                        idx++;
                        int actor_id = DateFile.instance.ParseInt(ss[1]);
                        if (actor_id == giveId)
                        {
                            target = child.position;
                            get_target = true;
                            break;
                        }
                        if (idx == 0)
                        {
                            star_id = actor_id;
                            start_pos = child.position;
                        }
                        end_id = actor_id;
                        end_pos = child.position;
                        if (idx == 1)
                        {
                            dis = start_pos - child.position;
                        }
                    }
                    if (!get_target)
                    {
                        bool top = true;
                        for (int i = 0; i < data.Length; i++)
                        {
                            int actor_id = data[i];
                            if (top)
                            {
                                if (actor_id == star_id || actor_id == end_id)
                                {
                                    top = false;
                                }
                            }
                            if (actor_id == giveId)
                            {
                                if (top)
                                {
                                    target = start_pos + dis;
                                }
                                else
                                {
                                    target = start_pos - dis;
                                }
                            }
                        }
                    }
                    IconMove.Move(start, target, 0.5f, sprite);


                    SaveItem(giveId, actorId, itemId);
                }
            }
            else
            {
                Main.Logger.Log(actorId + "使用物品" + itemId);
                bool use = UseCure(actorId, itemId);

            }
        }

        /// <summary>
        /// 尝试使用物品疗伤
        /// </summary>
        /// <param name="actorId">人物id</param>
        /// <param name="itemId">物品id</param>
        /// <returns>是否为疗伤药</returns>
        private bool UseCure(int actorId,int itemId)
        {
            bool result = false; // 是否判定为疗伤药并且使用成功
            int cureValue = 0; // 疗伤值
            int damageTyp = 1; // 伤口类型 1是外伤 2是内伤
            cureValue = Mathf.Abs(DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 11))); // 判断是否外伤药 如果是拿到疗伤效果 11是外伤疗伤效果
            if (cureValue > 0)
            {
                damageTyp = 1;
                Main.Logger.Log("外伤药");
                result = true;
                //if (DateFile.instance.actorInjuryDate[actorId][num19] > num21 * 3)
                //{
                //    num21 /= 5;
                //}
                //DateFile.instance.RemoveInjury(actorId, num19, -num21);
            }
            cureValue = Mathf.Abs(DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 12))); // 判断是否内伤药 如果是拿到疗伤效果 12是内伤疗伤效果
            if (cureValue > 0)
            {
                damageTyp = 2;
                Main.Logger.Log("内伤药");
                result = true;
                //if (DateFile.instance.actorInjuryDate[actorId][num19] > num22 * 3)
                //{
                //    num22 /= 5;
                //}
                //DateFile.instance.RemoveInjury(actorId, num19, -num22);
            }

            Main.Logger.Log("打印伤口 伤口类型："+ damageTyp);
            if (!result)
                return false;

            int injuryId = -1; // 记录可治疗药效不削弱的最深的伤口id
            int ijIdValue = 0; // 当前记录的伤口id的伤害值
            int maxIjId = -1; // 最深的伤口id
            int maxIjIdValue = 0; // 当前记录的最深的伤口伤害值
            foreach (var item in DateFile.instance.actorInjuryDate[actorId])
            {
                Main.Logger.Log(item.Key + " 伤口 " + item.Value);
                int ijId = item.Key;
                foreach (var vvv in DateFile.instance.injuryDate[ijId])
                {
                    Main.Logger.Log(vvv.Key + " 伤口类型 " + vvv.Value);
                }
                if (DateFile.instance.injuryDate[ijId].ContainsKey(damageTyp))
                {
                    int injury = DateFile.instance.ParseInt(DateFile.instance.injuryDate[ijId][damageTyp]);
                    if (injury > 0) // 是对应疗伤药的伤口
                    {
                        if (injury > maxIjIdValue) // 更新记录的最深的伤口
                        {
                            maxIjId = ijId;
                            maxIjIdValue = injury;
                        }
                        if (injury > ijIdValue && !(DateFile.instance.actorInjuryDate[actorId][ijId] > cureValue * 3)) // 更新记录可治疗药效不削弱的最深的伤口id
                        {
                            injuryId = ijId;
                            ijIdValue = injury;
                        }
                    }
                }
            }
            if (injuryId == -1)
            {
                if (maxIjId == -1)
                {
                    YesOrNoWindow.instance.SetYesOrNoWindow(-1, "你逗我呢？", "你都没有受伤干嘛要吃药？", false, true);
                    return true;
                }
                else
                {
                    injuryId = maxIjId;
                    ijIdValue = maxIjIdValue / 5;
                }
            }
            DateFile.instance.RemoveInjury(actorId, injuryId, -ijIdValue);



            // 如果药物有淬毒那么会中毒
            for (int m = 0; m < 6; m++)
            {
                int num23 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 71 + m));
                if (num23 != 0)
                {
                    ActorMenu.instance.ChangePoison(actorId, m, num23 * 10);
                }
            }
            DateFile.instance.ChangeItemHp(actorId, itemId, -1); // 消耗物品耐久度
            DateFile.instance.RemoveInjury(actorId, injuryId, -cureValue); // 删除角色伤口


            DateFile.instance.PlayeSE(8); // 音效
            WindowManage.instance.WindowSwitch(on: false);
            ActorMenu.instance.GetActorInjury(actorId, damageTyp);
            if (DateFile.instance.battleStart)
            {
                StartBattle.instance.UpdateActorHSQP();
            }
            return result;
        }

        /// <summary>
        /// 存放物品到同道身上
        /// </summary>
        /// <param name="giveId">受赠者人物ID</param>
        /// <param name="actorId">赠送者人物ID</param>
        /// <param name="itemId">物品ID</param>
        private void SaveItem(int giveId,int actorId,int itemId)
        {
            if (ActorMenu.instance.isEnemy)
            {
                Main.Logger.Log("是敌人");
                return;
            }
            int typ = 8;
            int mainActorId = DateFile.instance.MianActorID();
            //int giveId = DateFile.instance.ParseInt(containerImage.transform.parent.gameObject.name.Split(',')[1]);
            Main.Logger.Log("不能更换团队"+ ActorMenu.instance.cantChanageTeam);
            if (ActorMenu.instance.cantChanageTeam)
            {
                if (!DateFile.instance.acotrTeamDate.Contains(actorId))
                {
                    // <color=#E3C66DFF>一</color>未与我们同行！< color =#E3C66DFF>（无法在奇遇过程中向其拿取物品。）</color>
                    float x = -770f;
                    float y = 365f;
                    TipsWindow.instance.SetTips(0, new string[1]
                    {
                                DateFile.instance.SetColoer(20008, DateFile.instance.GetActorDate(actorId, 0, addValue: false)) + DateFile.instance.massageDate[304][0]
                    }, 180, x, y);
                    return;
                }
                if (!DateFile.instance.acotrTeamDate.Contains(giveId))
                {
                    //< color =#E3C66DFF>0</color>未与我们同行！< color =#E3C66DFF>（无法在奇遇过程中向不在出战队伍的同道转交物品。）</color>
                    float x2 = -400f;
                    float y2 = 315f;
                    TipsWindow.instance.SetTips(0, new string[1]
                    {
                                DateFile.instance.SetColoer(20008, DateFile.instance.GetActorDate(giveId, 0, addValue: false)) + DateFile.instance.massageDate[304][1]
                    }, 180, x2, y2);
                    return;
                }
            }
            Main.Logger.Log("actorId != mainActorId " + (actorId != mainActorId));
            Main.Logger.Log("!DateFile.instance.giveItemsDate.ContainsKey(actorId) "+(!DateFile.instance.giveItemsDate.ContainsKey(actorId)));
            Main.Logger.Log("actorId != mainActorId && (!DateFile.instance.giveItemsDate.ContainsKey(actorId) || !DateFile.instance.giveItemsDate[actorId].ContainsKey(giveId)) "+(actorId != mainActorId && (!DateFile.instance.giveItemsDate.ContainsKey(actorId) || !DateFile.instance.giveItemsDate[actorId].ContainsKey(itemId))));
            if (actorId != mainActorId && (!DateFile.instance.giveItemsDate.ContainsKey(actorId) || !DateFile.instance.giveItemsDate[actorId].ContainsKey(itemId)))
            {
                int num13 = 0;
                int num14 = 100;
                int num15 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 5));
                Main.Logger.Log("num15 "+ num15+" "+(DateFile.instance.ParseInt(DateFile.instance.GetActorDate(actorId, 202, addValue: false)))+" "+(DateFile.instance.ParseInt(DateFile.instance.GetActorDate(actorId, 203, addValue: false))));
                if (num15 == DateFile.instance.ParseInt(DateFile.instance.GetActorDate(actorId, 202, addValue: false)))
                {
                    num13 = 2;
                    num14 += 100;
                    DateFile.instance.actorsDate[actorId][207] = "1";
                }
                else if (num15 == DateFile.instance.ParseInt(DateFile.instance.GetActorDate(actorId, 203, addValue: false)))
                {
                    num13 = 1;
                    num14 -= 50;
                    DateFile.instance.actorsDate[actorId][208] = "1";
                }
                Main.Logger.Log("xx == 1 "+(DateFile.instance.ParseInt(DateFile.instance.GetActorDate(actorId, 27, addValue: false))));
                if (DateFile.instance.ParseInt(DateFile.instance.GetActorDate(actorId, 27, addValue: false)) == 1)
                {
                    num13 = 2;
                    num14 += 100;
                    DateFile.instance.SetActorMood(actorId, -DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 103)));
                }
                //for (int j = 0; j < ActorMenu.instance.listActorsHolder.childCount; j++)
                //{
                //    Transform child = ActorMenu.instance.listActorsHolder.GetChild(j);
                //    if (DateFile.instance.ParseInt(child.name.Split(',')[1]) == actorId)
                //    {
                //        GameObject[] moodFace = child.GetComponent<SetListActor>().moodFace;
                //        for (int k = 0; k < moodFace.Length; k++)
                //        {
                //            bool flag = k == num13;
                //            moodFace[k].SetActive(flag);
                //            if (flag)
                //            {
                //                Component[] componentsInChildren = moodFace[k].GetComponentsInChildren<Component>();
                //                Component[] array = componentsInChildren;
                //                foreach (Component component in array)
                //                {
                //                    if (component is Graphic)
                //                    {
                //                        (component as Graphic).CrossFadeAlpha(1f, 0f, ignoreTimeScale: true);
                //                        (component as Graphic).CrossFadeAlpha(0f, 5f, ignoreTimeScale: true);
                //                    }
                //                }
                //            }
                //        }
                //        break;
                //    }
                //}
                int num16 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 102)) * num14 / 100;
                Main.Logger.Log("num16 = "+ num16);
                DateFile.instance.actorsDate[actorId][210] = (DateFile.instance.ParseInt(DateFile.instance.GetActorDate(actorId, 210, addValue: false)) + num16).ToString();
                DateFile.instance.ChangeFavor(actorId, -num16, updateActor: false, showMassage: false);
            }
            Main.Logger.Log("OK1 ");
            DateFile.instance.AddGiveItems(giveId, itemId);
            Main.Logger.Log("OK2 ");
            DateFile.instance.ChangeTwoActorItem(actorId, giveId, itemId);
            Main.Logger.Log("OK3 ");
            ActorMenu.instance.UpdateActorListFavor();
            Main.Logger.Log("OK4 ");
            DateFile.instance.PlayeSE(typ);
            WindowManage.instance.WindowSwitch(on: false);
            ActorMenu.instance.UpdateItems(actorId, ActorMenu.instance.itemTyp);
            ActorMenu.instance.UpdateEquips(actorId, ActorMenu.instance.equipTyp);
            Main.Logger.Log("OK5 ");
        }

        private void Update()
        {
            //if (!gameObject.activeInHierarchy | m_data == null | scrollRect == null)
            //{
            //    return;
            //}
            //var mousePosition = Input.mousePosition;
            //var mouseOnPackage = mousePosition.x < Screen.width / 16 && mousePosition.y > Screen.width / 10 && mousePosition.y < Screen.width / 10 * 9;

            //var v = Input.GetAxis("Mouse ScrollWheel");
            //if (v != 0)
            //{
            //    if (mouseOnPackage)
            //    {
            //        float count = m_data.Length / lineCount + 1;
            //        scrollRect.verticalNormalizedPosition += v / count * Main.settings.scrollSpeed;
            //    }
            //}
        }
        public class PackageItem : ItemCell
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
                Main.Logger.Log("WarehouseItem Awake " + childDatas.Length);
            }
        }
        public struct ChildData
        {
            public GameObject gameObject;
            public SetItem setItem;

            public ChildData(Transform child)
            {
                gameObject = child.gameObject;
                setItem = gameObject.GetComponent<SetItem>();
            }
        }
    }
}