using System;
using System.Collections.Generic;
using UnityEngine;

namespace GuiWarehouse
{
    public class WarehouseUI : UnityEngine.MonoBehaviour
    {
        private static WarehouseUI instance;
        public static WarehouseUI GetWarehouseUI()
        {
            if (instance == null)
            {
                GameObject go = new GameObject();
                MonoBehaviour.DontDestroyOnLoad(go);
                instance = go.AddComponent<WarehouseUI>();
                go.name = "WarehouseUI";
            }
            return instance;
        }

        public int[] data = new int[1];
        public int[] data2 = new int[1];

        public string[] titleName = new string[] { "全部", "制作", "丹药", "食物", "装备", "图书", "其他" };

        public string[] levelClassify = new string[] { "全部", "一品", "二品", "三品", "四品", "五品", "六品", "七品", "八品", "九品", };

        public string[] bookClassify = new string[] {"全部", "未读", "内功", "身法", "绝技","拳掌","指法","腿法","暗器","剑法","刀法","长兵","奇门","软兵","御射","乐器",
            "音律", "弈棋", "诗书", "绘画", "术数", "品鉴", "制木", "锻造", "织锦", "巧匠", "医术", "毒术","道法","佛学","厨艺","杂学", };

        //public string[] attrClassify =new string[]  {"全部", "宝物","内功", "身法", "绝技","拳掌","指法","腿法","暗器","剑法","刀法","长兵","奇门","软兵","御射","乐器",
        //    "音律", "弈棋", "诗书", "绘画", "术数", "品鉴", "制木", "锻造", "锦织", "巧匠", "医术", "毒术","道法","佛法","厨艺","杂学", };



        public int[] showBookClassify = new int[] { 1, 0, 0, 1, 1, 1, 0 };
        //public int[] showAttrClassify = new int[] { 0, 0, 0, 1, 1, 0, 0 };

        private string[] color = new string[] { "#E9D443","#FFFFFF","#7E7E7E","#60e038","#2FA4FF","#B350FF","#F5691E","#F63333","#F8DC1E","#E9D443" };

        Vector2 mousePosition;
        private bool _open;
        public bool open
        {
            get { return _open; }
            set { _open = value;
                if (_open)
                {
                    if (DateFile.instance != null)
                    {
                        actorId = DateFile.instance.MianActorID();
                        //SelectTitle(true, Main.settings.openTitle);
                        //SelectTitle(false, Main.settings.openTitle);
                        SelectTitle(true, 0);
                        SelectTitle(false, 0);
                    }
                }
            }
        }
        public bool mouseOnPackage;

        int edge = 50;//边缘宽度 配置
        int interval = 0;//间隔 配置
        int numberOfColumns = 6;//几列物品 配置
        int handle = 20;//滚动条宽度 写死
        int cellSize = 0;//格子大小 计算
        int numberOfLines = 4;//几行物品 计算

        int title = 0;
        int title2 = 0;
        int pack = 1;//当前显示页 默认1
        int maxPack = 1;//最大显示页 计算
        int pack2 = 1;//当前显示页 默认1
        int maxPack2 = 1;//最大显示页 计算

        int actorId = 0;
        int warehouseId = -999;

        public void Awake()
        {
            open = false;


            float[] a = new float[] {
            0.894117653f, 0.3137255f, 0.3019608f,
            0.5568628f, 0.5568628f, 0.5568628f,
            0.9843137f, 0.9843137f, 0.9843137f,
            0.427450985f, 0.7176471f, 0.372549027f,
            0.56078434f, 0.7294118f, 0.905882359f,
            0.3882353f, 0.807843149f, 0.8156863f,
            0.68235296f, 0.3529412f, 0.784313738f,
            0.8901961f, 0.7764706f, 0.427450985f,
            0.9490196f, 0.509803951f, 0.203921571f,
            };
            color = new string[10];

            string[] c = new string[3];
            for (int i = 0; i < a.Length; i++)
            {
                int b = (int)(a[i] * 255);
                string s = Convert.ToString(b, 16);
                int idx = i % 3;
                c[idx] = s;
                if (idx == 2)
                {
                    color[i / 3] = string.Format("#{0}{1}{2}", c[0], c[1], c[2]);
                }
            }
            color[9] = color[0];
        }

        public void Update()
        {
            if (!open)
            {
                return;
            }
            mousePosition = Input.mousePosition;
            mouseOnPackage = mousePosition.x > Screen.width / 2;

            var v = Input.GetAxis("Mouse ScrollWheel");
            if (v != 0)
            {
                if (mouseOnPackage)
                {
                    pack2 = pack2 + (v < 0 ? 1 : -1);
                    pack2 = pack2 < 1 ? 1 : pack2;
                    pack2 = pack2 > maxPack2 ? maxPack2 : pack2;
                }
                else
                {
                    pack = pack + (v < 0 ? 1 : -1);
                    pack = pack < 1 ? 1 : pack;
                    pack = pack > maxPack ? maxPack : pack;
                }
            }
            if (Input.GetKey(KeyCode.Escape))
            {
                open = false;
            }

        }

        string m_keyWords = "";
        float keyWordsT;
        public void OnGUI()
        {

            int sw = Screen.width;//屏幕宽度
            int sh = Screen.height;//屏幕高度
            int int1 = (1280/512);//间隔
            int int2 = (1280/24);//宽度
            int int3 = (1280/ 48);//高度

            if (!open && Main.settings != null && Warehouse.instance != null && Warehouse.instance.warehouseWindow)
            {
                if (Main.settings.useClassify!=0 && Main.settings.useWarehouse == 0 && Warehouse.instance.warehouseWindow.activeSelf)
                {
                    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    {
                        return;
                    }

                    int idx = 0;
                    SetSelectButton(Main.MaxLevelClassify(), levelClassify, ref Main.settings.levelClassify, ref idx, int1, int2, int3);
                    if ((showBookClassify[Warehouse.instance.actorItemTyp] | showBookClassify[Warehouse.instance.warehouseItemTyp]) == 1)
                    {
                        SetSelectButton(Main.MaxBookClassify(), bookClassify, ref Main.settings.bookClassify, ref idx, int1, int2, int3);
                    }
                    //if ((showAttrClassify[Warehouse.instance.actorItemTyp] | showAttrClassify[Warehouse.instance.warehouseItemTyp]) == 1)
                    //{
                    //    SetSelectButton(Main.MaxAttrClassify(), attrClassify, ref Main.settings.attrClassify, ref idx, int1, int2, int3);
                    //}

                    GUI.Label(new Rect(int1 * 5 + levelClassify.Length * (int1 + int2), int1, int2, int3), "搜索");
                    m_keyWords = GUI.TextField(new Rect(int1 + (levelClassify.Length + 1) * (int1 + int2), int1, int2 * 5, int3), m_keyWords);

                    if (m_keyWords != Main.keyWords && (Time.time - keyWordsT) > 0.5f)
                    {
                        keyWordsT = Time.time;
                        Main.keyWords = m_keyWords;
                        Main.Warehouse_UpdateActorItems_Patch.UpdateData();
                    }
                }
            }


            if (!open)
            {
                return;
            }
            int on = -1;
            GUI.Box(new Rect(0, 0, sw, sh), string.Empty);
            GUI.Box(new Rect(0, 0, sw, sh), string.Empty);
            GUI.Box(new Rect(0, 0, sw, sh), string.Empty);
            GUI.Box(new Rect(0, 0, sw, sh), string.Empty);
            GUI.Box(new Rect(0, 0, sw, sh), string.Empty);
            string ssss = "";
            for (int i = 0; i < color.Length; i++)
            {
                ssss += string.Format("<color={0}>{1}</color>", color[i], i.ToString());
            }
            GUILayout.Label(ssss);
            int mx = (int)mousePosition.x;//鼠标位置
            int my = sh - (int)mousePosition.y;//鼠标位置

            cellSize = (sw / 2 - handle - edge * 2 - interval) / numberOfColumns - interval;
            numberOfLines = (sh - edge * 2 - interval) / (cellSize + interval);

            int maxCount = data.Length;
            int overCount = maxCount - numberOfColumns * numberOfLines;//超出页显示的数量
            maxPack = overCount / numberOfColumns + 2;
            if (maxPack < 1)
            {
                maxPack = 1;
            }

            int mx1 = mx - edge;
            int my1 = my - edge;

            if (GUI.Button(new Rect(sw - edge + 5, 5, edge - 10, edge - 10), "<color=#F63333>×</color>"))
            {
                open = false;
            }

            int w = sw / 2 - edge * 2;
            for (int i = 0; i < titleName.Length; i++)
            {
                string name;
                name = titleName[i];
                if(i == title)
                {
                    name = "<color=#F5691E>" + name + "</color>";
                }
                if (GUI.Button(new Rect(edge + i * w / titleName.Length + 5, 10, w / titleName.Length - 10, edge - 20), name))
                {
                    SelectTitle(false, i);
                }
                name = titleName[i];
                if (i == title2)
                {
                    name = "<color=#F5691E>" + name + "</color>";
                }
                if (GUI.Button(new Rect(sw / 2 + edge + i * w / titleName.Length + 5, 10, w / titleName.Length - 10, edge - 20), name))
                {
                    SelectTitle(true, i);
                }
            }
            GUI.Box(new Rect(edge, edge, w, sh - edge * 2), string.Empty);
            GUI.BeginGroup(new Rect(edge, edge, w, sh - edge * 2), new GUIStyle());
            pack = (int)GUI.VerticalSlider(new Rect(numberOfColumns * (cellSize + interval) + interval, 0, handle, sh - edge * 2), pack, 1, maxPack);
            int starIndex = (pack - 1) * numberOfColumns;
            int endIndex = maxPack - pack > 0 ? (pack + numberOfLines - 1) * numberOfColumns : data.Length;

            for (int i = starIndex; i < endIndex; i++)
            {
                if (i >= data.Length)
                {
                    break;
                }
                int itemId = data[i];
                int r = (i - starIndex) % numberOfColumns;
                int l = (i - starIndex) / numberOfColumns;

                int x = r * (cellSize + interval) + interval;
                int y = l * (cellSize + interval) + interval;
                string name = GetItemName(true, itemId);
                if (GUI.Button(new Rect(x, y, cellSize, cellSize), name))
                {
                    SelectItem(false, itemId);
                }

                if (!mouseOnPackage && mx1 > x && mx1 < x + cellSize && my1 > y && my1 < y + cellSize)
                {
                    on = itemId;
                }
            }
            GUI.EndGroup();


            int maxCount2 = data2.Length;
            int overCount2 = maxCount2 - numberOfColumns * numberOfLines;//超出页显示的数量
            maxPack2 = overCount2 / numberOfColumns + 2;
            if (maxPack2 < 1)
            {
                maxPack2 = 1;
            }
            GUI.Box(new Rect(sw / 2 + edge, edge, w, sh - edge * 2), string.Empty);
            GUI.BeginGroup(new Rect(sw / 2 + edge, edge, w, sh - edge * 2), new GUIStyle());
            pack2 = (int)GUI.VerticalSlider(new Rect(numberOfColumns * (cellSize + interval) + interval, 0, handle, sh - edge * 2), pack2, 1, maxPack2);
            int starIndex2 = (pack2 - 1) * numberOfColumns;
            int endIndex2 = maxPack2 - pack2 > 0 ? (pack2 + numberOfLines - 1) * numberOfColumns : data2.Length;

            int mx2 = mx - sw / 2 - edge;//鼠标位置
            int my2 = my - edge;//鼠标位置
            for (int i = starIndex2; i < endIndex2; i++)
            {
                if (i >= data2.Length)
                {
                    break;
                }
                int itemId = data2[i];
                int r = (i - starIndex2) % numberOfColumns;
                int l = (i - starIndex2) / numberOfColumns;

                int x = r * (cellSize + interval) + interval;
                int y = l * (cellSize + interval) + interval;
                string name = GetItemName(true, itemId);
                if (GUI.Button(new Rect(x, y, cellSize, cellSize), name))
                {
                    SelectItem(true, itemId);
                }

                if (mouseOnPackage && mx2 > x && mx2 < x + cellSize && my2 > y && my2 < y + cellSize)
                {
                    on = itemId;
                }
            }
            GUI.EndGroup();

            int i_warehouseMaxSize = Warehouse.instance.GetWarehouseMaxSize();
            int i_useItemSize2 = ActorMenu.instance.GetUseItemSize(-999);
            string warehouseMaxSize = (((float)i_warehouseMaxSize) /100f).ToString("f1");
            string useItemSize2 = (((float)i_useItemSize2) / 100f).ToString("f1");
            string s = string.Empty;
            if (i_useItemSize2 >= i_warehouseMaxSize)
            {
                s = string.Format("重量：<color=#F63333>{0}/{1}</color>\n滑动：{2}/{3}", useItemSize2, warehouseMaxSize, pack, maxPack);
            }
            else
            {
                s = string.Format("重量：{0}/{1}\n滑动：{2}/{3}", useItemSize2, warehouseMaxSize, pack, maxPack);
            }
            GUI.Label(new Rect(sw * 0.4f - edge * 2, sh - edge, 300, edge), s);


            int i_maxItemSize = ActorMenu.instance.GetMaxItemSize(actorId);
            int i_useItemSize = ActorMenu.instance.GetUseItemSize(actorId);
            string maxItemSize = (((float)i_maxItemSize) / 100f).ToString("f1");
            string useItemSize = (((float)i_useItemSize) / 100f).ToString("f1");
            string s2 = string.Empty;
            if (i_useItemSize2 >= i_warehouseMaxSize)
            {
                s2 = string.Format("重量：<color=#F63333>{0}/{1}</color>\n滑动：{2}/{3}", useItemSize, maxItemSize, pack2, maxPack2);
            }
            else
            {
                s2 = string.Format("重量：{0}/{1}\n滑动：{2}/{3}", useItemSize, maxItemSize, pack2, maxPack2);
            }
            GUI.Label(new Rect(sw * 0.9f - edge * 2, sh - edge, 300, edge), s2);


            if (on > -1)
            {
                MouseOnItem(mouseOnPackage, on, mx, my);
            }
        }

        void SetSelectButton(int max,string[] nameArray,ref int record,ref int idx,int int1,int int2,int int3)
        {
            int cur = record;
            int left = 0;
            for (int i = 0; i < nameArray.Length; i++)
            {
                bool value = false;
                if (i == 0)
                {
                    value = cur == max;
                }
                else
                {
                    value = ((1 << (i - 1)) | cur) == cur;
                }
                string btnName = nameArray[i];
                if (value)
                {
                    btnName = "<color=#2FA4FF>" + btnName + "</color>";
                }
                if (i== 16)
                {
                    idx++;
                    left = -16 * (int1 + int2);
                }
                if (GUI.Button(new Rect(int1 + i * (int1 + int2)+ left, int1 + idx * (int1 + int3), int2, int3), btnName))
                {
                    if (i == 0)
                    {
                        if (value)
                        {
                            cur = 0;
                        }
                        else
                        {
                            cur = max;
                        }
                    }
                    else
                    {
                        if (value)
                        {
                            cur -= cur & (1 << (i - 1));
                        }
                        else
                        {
                            cur |= 1 << (i - 1);
                        }
                    }
                }
            }
            if (cur != record)
            {
                //Main.Logger.Log(record + " ==> " + cur);
                record = cur;
                Main.Warehouse_UpdateActorItems_Patch.UpdateData();
            }
            idx++;

        }

        string GetItemName(bool actor, int itemId)
        {
            string des;
            des = GetName(itemId);

            if (int.Parse(DateFile.instance.GetItemDate(itemId, 6, true)) > 0)
            {
                des += "\n×" + DateFile.instance.GetItemNumber(actor ? actorId : warehouseId, itemId);
            }
            else
            {
                int num = int.Parse(DateFile.instance.GetItemDate(itemId, 901, true));
                int num2 = int.Parse(DateFile.instance.GetItemDate(itemId, 902, true));
                des += string.Format("\n{0}/{1}", num, num2);
            }

            List<string> list = new List<string>();
            List<string> list2 = new List<string>();
            List<int> list3 = new List<int>(DateFile.instance.buffAttrDate.Keys);
            for (int i = 0; i < list3.Count; i++)
            {
                int num7 = list3[i];

                if (DateFile.instance.presetitemDate[int.Parse(DateFile.instance.GetItemDate(itemId, 999, true))].ContainsKey(num7) && int.Parse(DateFile.instance.buffAttrDate[num7][8]) != 0)
                {
                    int num8 = int.Parse(DateFile.instance.GetItemDate(itemId, num7, true));
                    if (num8 != 0)
                    {
                        string str = "+";
                        int coloer;
                        if (num8 > 0)
                        {
                            coloer = int.Parse(DateFile.instance.buffAttrDate[num7][3]);
                        }
                        else
                        {
                            str = "-";
                            coloer = int.Parse(DateFile.instance.buffAttrDate[num7][4]);
                        }
                        num8 = Mathf.Abs(num8);
                        if (num7 == 61 || num7 == 62 || num7 == 63 || num7 == 64 || num7 == 65 || num7 == 66)
                        {
                            num8 *= 5;
                        }
                        float num9 = (float)num8 / float.Parse(DateFile.instance.buffAttrDate[num7][1]);
                        string format = (int.Parse(DateFile.instance.buffAttrDate[num7][1]) != 100) ? "f1" : "f2";
                        if (int.Parse(DateFile.instance.buffAttrDate[num7][5]) == 1)
                        {
                            string text4 = string.Format("{0}{1}{2}", DateFile.instance.buffAttrDate[num7][0], WindowManage.instance.Mut(), DateFile.instance.SetColoer(coloer, str + ((int.Parse(DateFile.instance.buffAttrDate[num7][1]) != 1) ? num9.ToString(format) : num8.ToString()) + DateFile.instance.buffAttrDate[num7][2], false));
                            if (num7 == 11 || num7 == 12)
                            {
                                string text5 = text4;
                                text4 = string.Concat(new string[]
                                {
                                text5,
                                "\n",
                                WindowManage.instance.Dit(),
                                DateFile.instance.massageDate[9][1].Split(new char[]
                                {
                                    '|'
                                })[0],
                                (num9 * 3f).ToString(format),
                                DateFile.instance.massageDate[9][1].Split(new char[]
                                {
                                    '|'
                                })[1]
                                });
                            }
                            if (num7 == 13)
                            {
                                string text5 = text4;
                                text4 = string.Concat(new string[]
                                {
                                text5,
                                "\n",
                                WindowManage.instance.Dit(),
                                DateFile.instance.massageDate[9][4].Split(new char[]
                                {
                                    '|'
                                })[0],
                                num9.ToString(),
                                DateFile.instance.massageDate[9][4].Split(new char[]
                                {
                                    '|'
                                })[1]
                                });
                            }
                            if (num7 == 39)
                            {
                                string text5 = text4;
                                text4 = string.Concat(new string[]
                                {
                                text5,
                                "\n",
                                WindowManage.instance.Dit(),
                                DateFile.instance.massageDate[9][2].Split(new char[]
                                {
                                    '|'
                                })[0],
                                num9.ToString(),
                                DateFile.instance.massageDate[9][2].Split(new char[]
                                {
                                    '|'
                                })[1]
                                });
                            }
                            if (num7 == 61 || num7 == 62 || num7 == 63 || num7 == 64 || num7 == 65 || num7 == 66)
                            {
                                string text5 = text4;
                                text4 = string.Concat(new string[]
                                {
                                text5,
                                "\n",
                                WindowManage.instance.Dit(),
                                DateFile.instance.massageDate[9][3].Split(new char[]
                                {
                                    '|'
                                })[0],
                                (num9 * 3f).ToString(),
                                DateFile.instance.massageDate[9][3].Split(new char[]
                                {
                                    '|'
                                })[1]
                                });
                            }
                            list.Add(text4);
                        }
                        else
                        {
                            list2.Add(DateFile.instance.buffAttrDate[num7][0] + WindowManage.instance.Mut() + DateFile.instance.SetColoer(coloer, str + ((int.Parse(DateFile.instance.buffAttrDate[num7][1]) != 1) ? num9.ToString(format) : num8.ToString()) + DateFile.instance.buffAttrDate[num7][2], false));
                        }
                    }
                }
            }
            if (list.Count > 0)
            {
                //des += WindowManage.instance.SetMassageTitle(8007, 0, 3, 10002);
                for (int j = 0; j < list.Count; j++)
                {
                    des += string.Format("{0}{1}\n", WindowManage.instance.Dit(), list[j]);
                }
            }

            if (int.Parse(DateFile.instance.GetItemDate(itemId, 908, true)) != 0 || list2.Count > 0)
            {
                //int num10 = int.Parse(DateFile.instance.GetItemDate(itemId, 5, true));
                //bool flag = int.Parse(DateFile.instance.GetItemDate(itemId, 401, true)) != 0;
                //int num11 = int.Parse(DateFile.instance.GetItemDate(itemId, 402, true));
                //int index = 4;
                //if (num10 == 34 || num10 == 35)
                //{
                //    index = 3;
                //}
                //if (flag)
                //{
                //    index = 16;
                //}
                //des += WindowManage.instance.SetMassageTitle(8007, 0, index, 10002);
                for (int k = 0; k < list2.Count; k++)
                {
                    des += string.Format("\n{0}{1}", WindowManage.instance.Dit(), list2[k]);
                }
                //if (flag)
                //{
                //    des += string.Format("\n{0}{1}{2} {3}", new object[]
                //    {
                //    WindowManage.instance.Dit(),
                //    DateFile.instance.massageDate[8007][1].Split(new char[]
                //    {
                //        '|'
                //    })[11],
                //    DateFile.instance.SetColoer(20005, num11.ToString(), false),
                //    DateFile.instance.massageDate[8007][1].Split(new char[]
                //    {
                //        '|'
                //    })[12]
                //    });
                //    des += string.Format("\n{0}{1}", WindowManage.instance.Dit(), DateFile.instance.SetColoer(20002, DateFile.instance.massageDate[8007][2].Split(new char[]
                //    {
                //    '|'
                //    })[2], false));
                //}
                //if (num10 == 34 || num10 == 35)
                //{
                //    des += string.Format("\n{0}{1}", WindowManage.instance.Dit(), DateFile.instance.SetColoer(20002, DateFile.instance.massageDate[8007][2].Split(new char[]
                //    {
                //    '|'
                //    })[1], false));
                //}
                //des += "\n";
            }

            //int num12 = int.Parse(DateFile.instance.GetItemDate(itemId, 504, true));
            //if (num12 > 0)
            //{
                //string text6 = DateFile.instance.itemPowerDate[num12][99];
                //if (int.Parse(DateFile.instance.GetItemDate(itemId, 5, true)) == 15)
                //{
                //    text6 = text6.Replace("。", "") + DateFile.instance.SetColoer(20002, string.Format("{0}{1}{2}", DateFile.instance.massageDate[301][5].Split(new char[]
                //    {
                //    '|'
                //    })[0], DateFile.instance.SetColoer(20003, DateFile.instance.GetFootDamage(itemId) + "%", false), DateFile.instance.massageDate[301][5].Split(new char[]
                //    {
                //    '|'
                //    })[1]), false);
                //}


                //des += DateFile.instance.SetColoer(20002, string.Format("{0}{1}{2}{3}\n\n", new object[]
                //{
                //WindowManage.instance.Dit(),
                //DateFile.instance.SetColoer(20005, DateFile.instance.itemPowerDate[num12][0], false),
                //WindowManage.instance.Mut(),
                //text6
                //}), false);
            //}

            return des;
        }

        private bool lockPack = false;
        /// <summary>
        /// 点击物品
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="itemId"></param>
        void SelectItem(bool actor, int itemId)
        {
            string level = DateFile.instance.GetItemDate(itemId, 8, false);
            Main.Logger.Log("level " + level);
            int num = 1;
            if(Input.GetKey(KeyCode.LeftControl)|| Input.GetKey(KeyCode.RightControl))
            {
                if (int.Parse(DateFile.instance.GetItemDate(itemId, 6, true)) > 0)
                {
                    num = DateFile.instance.GetItemNumber(actor ? actorId : warehouseId, itemId);
                }
            }
            if (actor)
            {
                DateFile.instance.ChangeTwoActorItem(DateFile.instance.MianActorID(), -999, itemId, num, -1);
            }
            else
            {
                DateFile.instance.ChangeTwoActorItem(-999, DateFile.instance.MianActorID(), itemId, num, -1);
            }
            lockPack = true;
            SelectTitle(true, title2 );
            SelectTitle(false, title);
            lockPack = false;
        }

        /// <summary>
        /// 点击标题
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="itemId"></param>
        void SelectTitle(bool actor, int index)
        {
            if (actor)
            {
                if (!lockPack)
                {
                    pack2 = 1;
                }
                title2 = index;
                List<int> list = new List<int>(DateFile.instance.GetItemSort(new List<int>(ActorMenu.instance.GetActorItems(actorId, 0, false).Keys)));
                data2 = GetData(list, index);
            }
            else
            {
                if (!lockPack)
                {
                    pack = 1;
                }
                title = index;
                List<int> list = new List<int>(DateFile.instance.GetItemSort(new List<int>(ActorMenu.instance.GetActorItems(warehouseId, 0, false).Keys)));
                data = GetData(list, index);
            }
        }

        int[] GetData(List<int> list, int index)
        {
            List<int> result;
            if (index == 0)
            {
                return list.ToArray();
            }
            else
            {
                result = new List<int>();
                for (int i = 0; i < list.Count; i++)
                {
                    int itemId = list[i];
                    if (index == int.Parse(DateFile.instance.GetItemDate(itemId, 4, true)))
                    {
                        result.Add(itemId);
                    }
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// 鼠标悬浮在物品上
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="itemId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void MouseOnItem(bool actor, int itemId, int x, int y)
        {
            string name;
            string des;
            int line = 2;
            name = DateFile.instance.GetItemDate(itemId, 0, true);
            des = GetDes(actor, itemId, out line);
            string s = name + "\n" + des;

            int width = 600, height = line * 30;
            int _x;
            if (x > Screen.width / 2)
            {
                _x = x - width;
            }
            else
            {
                _x = x;
            }

            //if(height > Screen.height)
            //{
            //    height = Screen.height;
            //}
            //if (y < Screen.height / 2)//屏幕中心上方
            //{
            //    _y = y;
            //    if (height > Screen.height - _y)//高度大于一半
            //    {
            //        _y -= Screen.height - _y;
            //    }
            //}
            //else//屏幕中心下方
            //{
            //    _y = y - height;
            //    if(height > Screen.height - _y)
            //    {
            //        y += Screen.height - _y;
            //    }
            //}
            //GUI.Box(new Rect(_x, _y, width, height), s);
            //GUI.Box(new Rect(_x, _y, width, height), s);
            //GUI.Box(new Rect(_x, _y, width, height), s);
            //GUI.Box(new Rect(_x, _y, width, height), s);
            //GUI.Box(new Rect(_x, _y, width, height), s);
            width = Screen.width/2 + 100;
            height = Screen.height - 300;
            GUI.Box(new Rect(_x, 150, width, height), s);
            GUI.Box(new Rect(_x, 150, width, height), s);
            GUI.Box(new Rect(_x, 150, width, height), s);
            GUI.Box(new Rect(_x, 150, width, height), s);
            GUI.Box(new Rect(_x, 150, width, height), s);
        }

        string GetName(int itemId)
        {
            string des = DateFile.instance.GetItemDate(itemId, 0, false);
            string level = DateFile.instance.GetItemDate(itemId, 8, false);
            int l = 0;
            int.TryParse(level, out l);
            des = "<color=" + color[int.Parse(level)] + ">" + des + "</color>";
            return des;
        }

        /*《琴操》
         * 【神·一品】
         * 
         * 999:400009           data:0
         * 
         * 902:2                data:0
         * 
         * 901:2                data:0
         * 
         * 33:1|0|1|1|0|1|0|0|0|0
         * 
         * */

        public string GetDes(bool actor, int itemId, out int line)
        {
            line = 2;
            string des = DateFile.instance.GetItemDate(itemId, 99, true) + "\n\n";
            line += 2;
            int actorId = actor ? this.actorId : warehouseId;

            // 武器
            if (int.Parse(DateFile.instance.GetItemDate(itemId, 1, true)) == 1)
            {
                des +=  DateFile.instance.SetColoer(20003, string.Format("\n{0}{1}%", DateFile.instance.massageDate[8007][1].Split(new char[]
                 {
                        '|'
                 })[19], (BattleVaule.instance.GetWeaponDamage(actor, actorId, false, itemId) / 10).ToString()), false);
                des += DateFile.instance.SetColoer(20002, string.Format("\n{0}{1}~{2}", DateFile.instance.massageDate[8007][1].Split(new char[]
                {
                    '|'
                })[21], ((float)int.Parse(DateFile.instance.GetItemDate(itemId, 502, true)) / 100f).ToString("f1"), ((float)int.Parse(DateFile.instance.GetItemDate(itemId, 503, true)) / 100f).ToString("f1")), false);
                line += 2;
            }


            if (BattleSystem.instance.battleWindow.activeInHierarchy && ActorMenu.instance.actorMenu.activeInHierarchy)
            {
                des += DateFile.instance.massageDate[3011][0] + "\n";
                des += DateFile.instance.massageDate[3011][1] + "\n";
                line += 2;
            }

            // 建筑
            int num3 = int.Parse(DateFile.instance.GetItemDate(itemId, 301, true));
            if (num3 != 0)
            {
                des += DateFile.instance.SetColoer(20002, DateFile.instance.basehomePlaceDate[num3][100], false) + "\n";
                des += DateFile.instance.SetColoer(20002, DateFile.instance.basehomePlaceDate[num3][99], false) + "\n";
                line += 2;
            }
            else
            {
                des += DateFile.instance.SetColoer(20002, DateFile.instance.GetItemDate(itemId, 99, true), false) + "\n\n";
                line += 2;
            }

            des += this.SetItemTypText(itemId, actorId, ref line);

            //标题？
            if (int.Parse(DateFile.instance.GetItemDate(itemId, 2001, true)) == 1)
            {
                des += WindowManage.instance.SetMassageTitle(8007, 0, 5, 10002);
                des += WindowManage.instance.QuquMassage(itemId) + "\n";
                line += 1;
            }

            //外装
            int num4 = int.Parse(DateFile.instance.GetItemDate(itemId, 15, true));
            if (num4 != 0)
            {
                des += WindowManage.instance.SetMassageTitle(8007, 0, 7, 10002);
                des += string.Format("{0}{1}{2}\n\n", WindowManage.instance.Dit(), DateFile.instance.massageDate[8007][1].Split(new char[]
                {
                '|'
                })[13], DateFile.instance.SetColoer(20006, DateFile.instance.identityDate[num4][0], false));
                line += 2;
            }

            //又是武器？
            if (int.Parse(DateFile.instance.GetItemDate(itemId, 1, true)) == 1)
            {
                des += WindowManage.instance.SetMassageTitle(8007, 0, 8, 10002);
                des +=this.EquipMassage(itemId, ref line) + "\n";
                line += 1;
            }

            // 书籍？
            int num5 = int.Parse(DateFile.instance.GetItemDate(itemId, 32, true));
            if (num5 > 0)
            {
                des += this.ShowBookMassage(itemId, ref line);
            }

            // 增加造诣
            int num6 = int.Parse(DateFile.instance.GetItemDate(itemId, 42, true));
            if (num6 > 0)
            {
                des += WindowManage.instance.SetMassageTitle(8007, 0, 13, 10002);
                int key = int.Parse(DateFile.instance.GetItemDate(itemId, 41, true)) - 1;
                des += string.Format("{0}{1}{2}{3}\n\n", new object[]
                {
                WindowManage.instance.Dit(),
                DateFile.instance.baseSkillDate[key][0],
                DateFile.instance.massageDate[8007][1].Split(new char[]
                {
                    '|'
                })[39],
                DateFile.instance.SetColoer(20004, "+" + num6, false)
                });
                line += 2;
            }

            des += this.ShowItemPoison(itemId, ref line);

            List<string> list = new List<string>();
            List<string> list2 = new List<string>();
            List<int> list3 = new List<int>(DateFile.instance.buffAttrDate.Keys);
            for (int i = 0; i < list3.Count; i++)
            {
                int num7 = list3[i];
                if (DateFile.instance.presetitemDate[int.Parse(DateFile.instance.GetItemDate(itemId, 999, true))].ContainsKey(num7) && int.Parse(DateFile.instance.buffAttrDate[num7][8]) != 0)
                {
                    int num8 = int.Parse(DateFile.instance.GetItemDate(itemId, num7, true));
                    if (num8 != 0)
                    {
                        string str = "+";
                        int coloer;
                        if (num8 > 0)
                        {
                            coloer = int.Parse(DateFile.instance.buffAttrDate[num7][3]);
                        }
                        else
                        {
                            str = "-";
                            coloer = int.Parse(DateFile.instance.buffAttrDate[num7][4]);
                        }
                        num8 = Mathf.Abs(num8);
                        if (num7 == 61 || num7 == 62 || num7 == 63 || num7 == 64 || num7 == 65 || num7 == 66)
                        {
                            num8 *= 5;
                        }
                        float num9 = (float)num8 / float.Parse(DateFile.instance.buffAttrDate[num7][1]);
                        string format = (int.Parse(DateFile.instance.buffAttrDate[num7][1]) != 100) ? "f1" : "f2";
                        if (int.Parse(DateFile.instance.buffAttrDate[num7][5]) == 1)
                        {
                            string text4 = string.Format("{0}{1}{2}", DateFile.instance.buffAttrDate[num7][0], WindowManage.instance.Mut(), DateFile.instance.SetColoer(coloer, str + ((int.Parse(DateFile.instance.buffAttrDate[num7][1]) != 1) ? num9.ToString(format) : num8.ToString()) + DateFile.instance.buffAttrDate[num7][2], false));
                            if (num7 == 11 || num7 == 12)
                            {
                                string text5 = text4;
                                text4 = string.Concat(new string[]
                                {
                                text5,
                                "\n",
                                WindowManage.instance.Dit(),
                                DateFile.instance.massageDate[9][1].Split(new char[]
                                {
                                    '|'
                                })[0],
                                (num9 * 3f).ToString(format),
                                DateFile.instance.massageDate[9][1].Split(new char[]
                                {
                                    '|'
                                })[1]
                                });
                            }
                            if (num7 == 13)
                            {
                                string text5 = text4;
                                text4 = string.Concat(new string[]
                                {
                                text5,
                                "\n",
                                WindowManage.instance.Dit(),
                                DateFile.instance.massageDate[9][4].Split(new char[]
                                {
                                    '|'
                                })[0],
                                num9.ToString(),
                                DateFile.instance.massageDate[9][4].Split(new char[]
                                {
                                    '|'
                                })[1]
                                });
                            }
                            if (num7 == 39)
                            {
                                string text5 = text4;
                                text4 = string.Concat(new string[]
                                {
                                text5,
                                "\n",
                                WindowManage.instance.Dit(),
                                DateFile.instance.massageDate[9][2].Split(new char[]
                                {
                                    '|'
                                })[0],
                                num9.ToString(),
                                DateFile.instance.massageDate[9][2].Split(new char[]
                                {
                                    '|'
                                })[1]
                                });
                            }
                            if (num7 == 61 || num7 == 62 || num7 == 63 || num7 == 64 || num7 == 65 || num7 == 66)
                            {
                                string text5 = text4;
                                text4 = string.Concat(new string[]
                                {
                                text5,
                                "\n",
                                WindowManage.instance.Dit(),
                                DateFile.instance.massageDate[9][3].Split(new char[]
                                {
                                    '|'
                                })[0],
                                (num9 * 3f).ToString(),
                                DateFile.instance.massageDate[9][3].Split(new char[]
                                {
                                    '|'
                                })[1]
                                });
                            }
                            list.Add(text4);
                        }
                        else
                        {
                            list2.Add(DateFile.instance.buffAttrDate[num7][0] + WindowManage.instance.Mut() + DateFile.instance.SetColoer(coloer, str + ((int.Parse(DateFile.instance.buffAttrDate[num7][1]) != 1) ? num9.ToString(format) : num8.ToString()) + DateFile.instance.buffAttrDate[num7][2], false));
                        }
                    }
                }
            }
            if (list.Count > 0)
            {
                des += WindowManage.instance.SetMassageTitle(8007, 0, 3, 10002);
                for (int j = 0; j < list.Count; j++)
                {
                    des += string.Format("{0}{1}\n", WindowManage.instance.Dit(), list[j]);
                    line += 1;
                }
                des += "\n";
                line += 1;
            }

            if (int.Parse(DateFile.instance.GetItemDate(itemId, 908, true)) != 0 || list2.Count > 0)
            {
                int num10 = int.Parse(DateFile.instance.GetItemDate(itemId, 5, true));
                bool flag = int.Parse(DateFile.instance.GetItemDate(itemId, 401, true)) != 0;
                int num11 = int.Parse(DateFile.instance.GetItemDate(itemId, 402, true));
                int index = 4;
                if (num10 == 34 || num10 == 35)
                {
                    index = 3;
                }
                if (flag)
                {
                    index = 16;
                }
                des += WindowManage.instance.SetMassageTitle(8007, 0, index, 10002);
                for (int k = 0; k < list2.Count; k++)
                {
                    des += string.Format("{0}{1}\n", WindowManage.instance.Dit(), list2[k]);
                    line += 1;
                }
                if (flag)
                {
                    des += string.Format("{0}{1}{2} {3}\n", new object[]
                    {
                    WindowManage.instance.Dit(),
                    DateFile.instance.massageDate[8007][1].Split(new char[]
                    {
                        '|'
                    })[11],
                    DateFile.instance.SetColoer(20005, num11.ToString(), false),
                    DateFile.instance.massageDate[8007][1].Split(new char[]
                    {
                        '|'
                    })[12]
                    });
                    des += string.Format("\n{0}{1}\n", WindowManage.instance.Dit(), DateFile.instance.SetColoer(20002, DateFile.instance.massageDate[8007][2].Split(new char[]
                    {
                    '|'
                    })[2], false));
                    line += 2;
                }
                if (num10 == 34 || num10 == 35)
                {
                    des += string.Format("\n{0}{1}\n", WindowManage.instance.Dit(), DateFile.instance.SetColoer(20002, DateFile.instance.massageDate[8007][2].Split(new char[]
                    {
                    '|'
                    })[1], false));
                    line += 1;
                }
                des += "\n";
                line += 1;
            }

            int num12 = int.Parse(DateFile.instance.GetItemDate(itemId, 504, true));
            if (num12 > 0)
            {
                string text6 = DateFile.instance.itemPowerDate[num12][99];
                if (int.Parse(DateFile.instance.GetItemDate(itemId, 5, true)) == 15)
                {
                    text6 = text6.Replace("。", "") + DateFile.instance.SetColoer(20002, string.Format("{0}{1}{2}", DateFile.instance.massageDate[301][5].Split(new char[]
                    {
                    '|'
                    })[0], DateFile.instance.SetColoer(20003, DateFile.instance.GetFootDamage(itemId) + "%", false), DateFile.instance.massageDate[301][5].Split(new char[]
                    {
                    '|'
                    })[1]), false);
                }
                des += DateFile.instance.SetColoer(20002, string.Format("{0}{1}{2}{3}\n\n", new object[]
                {
                WindowManage.instance.Dit(),
                DateFile.instance.SetColoer(20005, DateFile.instance.itemPowerDate[num12][0], false),
                WindowManage.instance.Mut(),
                text6
                }), false);
                line += 1;
            }

            return des;
        }


        bool showWeapon;
        int showWeaponId;
        int showEquipActorId;

        string SetItemTypText(int itemId, int actorId, ref int line)
        {
            string str = "";
            int num = int.Parse(DateFile.instance.GetItemDate(itemId, 901, true));
            int num2 = int.Parse(DateFile.instance.GetItemDate(itemId, 902, true));
            int num3 = int.Parse(DateFile.instance.GetItemDate(itemId, 4, true));
            int num4 = int.Parse(DateFile.instance.GetItemDate(itemId, 1, true));
            int num5 = int.Parse(DateFile.instance.GetItemDate(itemId, 996, true));
            if (int.Parse(DateFile.instance.GetItemDate(itemId, 1, true)) == 1)
            {
                line += 8;
                this.showEquipActorId = ((actorId == -1) ? ((!ActorMenu.instance.actorMenu.activeInHierarchy) ? DateFile.instance.MianActorID() : ActorMenu.instance.acotrId) : actorId);
                int weaponUsePower = DateFile.instance.GetWeaponUsePower(this.showEquipActorId, itemId);
                int weaponMaxUsePower = DateFile.instance.GetWeaponMaxUsePower(this.showEquipActorId, itemId);
                str += string.Format("{0}{1}{2}", WindowManage.instance.Dit(), DateFile.instance.massageDate[8007][2].Split(new char[]
                {
                '|'
                })[3], DateFile.instance.SetColoer(this.Color6(weaponUsePower, weaponMaxUsePower, 100), weaponUsePower + "%", false));
            }
            str += string.Format("{0}{1}{2}", WindowManage.instance.Dit(), DateFile.instance.massageDate[8007][1].Split(new char[]
            {
            '|'
            })[0], DateFile.instance.SetColoer(20008, DateFile.instance.GetItemDate(itemId, 904, true), false));
            return str + string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}\n", new object[]
            {
            WindowManage.instance.SetMassageTitle(8007, 0, 0, 10002),
            (num3 == 0) ? "" : string.Format("{0}{1} / {2}{3}\n", new object[]
            {
                WindowManage.instance.Dit(),
                DateFile.instance.massageDate[301][3].Split(new char[]
                {
                    '|'
                })[num3],
                DateFile.instance.massageDate[301][4].Split(new char[]
                {
                    '|'
                })[int.Parse(DateFile.instance.GetItemDate(itemId, 506, true))],
                DateFile.instance.massageDate[301][0].Split(new char[]
                {
                    '|'
                })[int.Parse(DateFile.instance.GetItemDate(itemId, 5, true))]
            }),
            (num5 <= 0) ? "" : string.Format("{0}{1}{2}\n", WindowManage.instance.Dit(), DateFile.instance.massageDate[8007][1].Split(new char[]
            {
                '|'
            })[15], DateFile.instance.SetColoer(20003, num5.ToString() + " / 10", false)),
            (num2 <= 0) ? "" : string.Format("{0}{1}{2}{3}</color> / {4}{5}\n", new object[]
            {
                WindowManage.instance.Dit(),
                DateFile.instance.massageDate[8007][1].Split(new char[]
                {
                    '|'
                })[4],
                ActorMenu.instance.Color3(num, num2),
                num,
                num2,
                (int.Parse(DateFile.instance.GetItemDate(itemId, 49, true)) != 0 || int.Parse(DateFile.instance.GetItemDate(itemId, 2001, true)) != 0) ? "" : DateFile.instance.massageDate[8007][1].Split(new char[]
                {
                    '|'
                })[5]
            }),
            WindowManage.instance.Dit(),
            DateFile.instance.massageDate[8007][1].Split(new char[]
            {
                '|'
            })[31],
            DateFile.instance.SetColoer(20003, DateFile.instance.massageDate[8007][1].Split(new char[]
            {
                '|'
            })[16], false),
            DateFile.instance.SetColoer(20008, ((float)int.Parse(DateFile.instance.GetItemDate(itemId, 501, true)) / 100f).ToString("f1"), false),
            (num4 != 1) ? ((num4 != 2) ? "\n" : string.Format("{0}{1}{2}{3}{4}{5}\n", new object[]
            {
                WindowManage.instance.Cut(20002),
                DateFile.instance.SetColoer(20003, DateFile.instance.massageDate[8007][1].Split(new char[]
                {
                    '|'
                })[37], false),
                DateFile.instance.SetColoer(20008, ((float)int.Parse(DateFile.instance.GetItemDate(itemId, 603, true)) / 100f).ToString("f1"), false),
                WindowManage.instance.Cut(20002),
                DateFile.instance.SetColoer(20003, DateFile.instance.massageDate[8007][1].Split(new char[]
                {
                    '|'
                })[38], false),
                DateFile.instance.SetColoer(20008, ((float)int.Parse(DateFile.instance.GetItemDate(itemId, 601, true)) / 100f).ToString("f1"), false)
            })) : string.Format("{0}{1}{2}{3}{4}{5}\n", new object[]
            {
                WindowManage.instance.Cut(20002),
                DateFile.instance.SetColoer(20003, DateFile.instance.massageDate[8007][1].Split(new char[]
                {
                    '|'
                })[17], false),
                DateFile.instance.SetColoer(20008, ((float)int.Parse(DateFile.instance.GetItemDate(itemId, 601, true)) / 100f).ToString("f1"), false),
                WindowManage.instance.Cut(20002),
                DateFile.instance.SetColoer(20003, DateFile.instance.massageDate[8007][1].Split(new char[]
                {
                    '|'
                })[18], false),
                DateFile.instance.SetColoer(20008, ((float)int.Parse(DateFile.instance.GetItemDate(itemId, 603, true)) / 100f).ToString("f1"), false)
            }),
            (int.Parse(DateFile.instance.GetItemDate(itemId, 50, true)) != 0) ? "" : string.Format("{0}{1}\n", WindowManage.instance.Dit(), DateFile.instance.massageDate[8007][1].Split(new char[]
            {
                '|'
            })[2]),
            (int.Parse(DateFile.instance.GetItemDate(itemId, 53, true)) != 0) ? "" : string.Format("{0}{1}\n", WindowManage.instance.Dit(), DateFile.instance.massageDate[8007][1].Split(new char[]
            {
                '|'
            })[3]),
            (int.Parse(DateFile.instance.GetItemDate(itemId, 3, true)) != 0) ? "" : string.Format("{0}{1}\n", WindowManage.instance.Dit(), DateFile.instance.massageDate[8007][1].Split(new char[]
            {
                '|'
            })[1])
            });
        }

        string EquipMassage(int equipId, ref int line)
        {
            line += 5;
            this.showWeapon = true;
            this.showWeaponId = equipId;
            string str = "";
            int num = DateFile.instance.ActorIsInBattle(this.showEquipActorId);
            bool isActor = num == 0 || num == 1;
            if (ActorMenu.instance.actorMenu.activeInHierarchy)
            {
                isActor = !ActorMenu.instance.isEnemy;
            }
            string text = "";
            string[] array = DateFile.instance.GetItemDate(equipId, 7, true).Split(new char[]
            {
            '|'
            });
            for (int i = 0; i < array.Length; i++)
            {
                text += DateFile.instance.SetColoer(int.Parse(DateFile.instance.attackTypDate[int.Parse(array[i])][99]), DateFile.instance.attackTypDate[int.Parse(array[i])][0], false);
                if (i < array.Length - 1)
                {
                    text += WindowManage.instance.Cut(20002);
                }
            }
            str += string.Format("{0}{1}{2}\n", WindowManage.instance.Dit(), DateFile.instance.massageDate[8007][1].Split(new char[]
            {
            '|'
            })[14], text);
            str += string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}\n", new object[]
            {
            WindowManage.instance.Dit(),
            DateFile.instance.massageDate[8007][1].Split(new char[]
            {
                '|'
            })[33],
            DateFile.instance.SetColoer(20003, DateFile.instance.massageDate[8007][1].Split(new char[]
            {
                '|'
            })[24], false),
            DateFile.instance.SetColoer(20005, BattleVaule.instance.GetWeaponHit(isActor, this.showEquipActorId, num != 0, equipId, 1).ToString(), false),
            WindowManage.instance.Cut(20002),
            DateFile.instance.SetColoer(20003, DateFile.instance.massageDate[8007][1].Split(new char[]
            {
                '|'
            })[25], false),
            DateFile.instance.SetColoer(20005, BattleVaule.instance.GetWeaponHit(isActor, this.showEquipActorId, num != 0, equipId, 2).ToString(), false),
            WindowManage.instance.Cut(20002),
            DateFile.instance.SetColoer(20003, DateFile.instance.massageDate[8007][1].Split(new char[]
            {
                '|'
            })[26], false),
            DateFile.instance.SetColoer(20005, BattleVaule.instance.GetWeaponHit(isActor, this.showEquipActorId, num != 0, equipId, 3).ToString(), false)
            });
            str += string.Format("{0}{1}{2}{3}{4}{5}{6}\n", new object[]
            {
            WindowManage.instance.Dit(),
            DateFile.instance.massageDate[8007][1].Split(new char[]
            {
                '|'
            })[32],
            DateFile.instance.SetColoer(20003, DateFile.instance.massageDate[8007][1].Split(new char[]
            {
                '|'
            })[22], false),
            DateFile.instance.SetColoer(20006, BattleVaule.instance.GetWeaponDestroy(isActor, this.showEquipActorId, equipId, 0).ToString(), false),
            WindowManage.instance.Cut(20002),
            DateFile.instance.SetColoer(20003, DateFile.instance.massageDate[8007][1].Split(new char[]
            {
                '|'
            })[23], false),
            DateFile.instance.SetColoer(20006, BattleVaule.instance.GetWeaponDestroy(isActor, this.showEquipActorId, equipId, 1).ToString(), false)
            });
            return str + string.Format("{0}{1}{2}{3}\n", new object[]
            {
            WindowManage.instance.Dit(),
            DateFile.instance.massageDate[8007][1].Split(new char[]
            {
                '|'
            })[34],
            DateFile.instance.SetColoer(20007, int.Parse(DateFile.instance.GetItemDate(equipId, 10, true)) + "%", false),
            DateFile.instance.SetColoer(20004, string.Format(" +{0}%{1}", Mathf.Max(int.Parse(DateFile.instance.GetActorDate(this.showEquipActorId, 1105, true)) * int.Parse(DateFile.instance.GetItemDate(equipId, 17, true)) / 100, 0), DateFile.instance.massageDate[8007][1].Split(new char[]
            {
                '|'
            })[35]), false)
            });
        }
        string ShowBookMassage(int itemId, ref int line)
        {
            //bool flag = false;
            //if (ShopSystem.instance.shopWindow.activeInHierarchy || BookShopSystem.instance.shopWindow.activeInHierarchy || Warehouse.instance.warehouseWindow.activeInHierarchy || (ActorMenu.instance.actorMenu.activeInHierarchy && !ActorMenu.instance.isEnemy) || DateFile.instance.actorItemsDate[DateFile.instance.MianActorID()].ContainsKey(itemId))
            //{
            //    flag = true;
            //}
            string result;
            //if (!flag)
            //{
            //    line += 2;
            //    result = string.Format("{0}{1}{2}\n\n", WindowManage.instance.SetMassageTitle(8007, 0, 12, 10002), WindowManage.instance.Dit(), DateFile.instance.massageDate[8006][4]);
            //}
            //else
            //{
                int key = int.Parse(DateFile.instance.GetItemDate(itemId, 32, true));
                int num = int.Parse(DateFile.instance.GetItemDate(itemId, 31, true));
                int[] array = (num != 17) ? ((!DateFile.instance.skillBookPages.ContainsKey(key)) ? new int[10] : DateFile.instance.skillBookPages[key]) : ((!DateFile.instance.gongFaBookPages.ContainsKey(key)) ? new int[10] : DateFile.instance.gongFaBookPages[key]);
                int[] bookPage = DateFile.instance.GetBookPage(itemId);
                string text = WindowManage.instance.SetMassageTitle(8007, 0, 12, 10002);
                for (int i = 0; i < array.Length; i++)
                {
                    text += string.Format("{0}{1}{2}{3}", new object[]
                    {
                    WindowManage.instance.Dit(),
                    DateFile.instance.massageDate[8][2].Split(new char[]
                    {
                        '|'
                    })[i],
                    (bookPage[i] != 1) ? DateFile.instance.SetColoer(20010, DateFile.instance.massageDate[7010][4].Split(new char[]
                    {
                        '|'
                    })[0], false) : DateFile.instance.SetColoer(20004, DateFile.instance.massageDate[7010][4].Split(new char[]
                    {
                        '|'
                    })[1], false),
                    (array[i] != 1) ? DateFile.instance.SetColoer(20002, string.Format("  ({0})\n", DateFile.instance.massageDate[7009][4].Split(new char[]
                    {
                        '|'
                    })[2]), false) : DateFile.instance.SetColoer(20005, string.Format("  ({0})\n", DateFile.instance.massageDate[7009][4].Split(new char[]
                    {
                        '|'
                    })[3]), false)
                    });

                    line += 1;
                }
                text += "\n";
                if (num == 17)
                {
                    int num2 = int.Parse(DateFile.instance.gongFaDate[key][103 + int.Parse(DateFile.instance.GetItemDate(itemId, 35, true))]);
                    if (num2 > 0)
                    {
                        text += string.Format("{0}{1}{2}\n\n", WindowManage.instance.SetMassageTitle(8007, 0, 14, 10002), WindowManage.instance.Dit(), DateFile.instance.SetColoer(20002, string.Format("{0}{1}{2}{3}{4}", new object[]
                        {
                        DateFile.instance.massageDate[8006][5].Split(new char[]
                        {
                            '|'
                        })[0],
                        DateFile.instance.SetColoer(20001 + int.Parse(DateFile.instance.gongFaDate[key][2]), DateFile.instance.gongFaDate[key][0], false),
                        DateFile.instance.massageDate[8006][5].Split(new char[]
                        {
                            '|'
                        })[1],
                        DateFile.instance.gongFaFPowerDate[num2][99],
                        DateFile.instance.massageDate[5001][5]
                        }), false));
                        line += 2;
                    }
                }
                result = text;
            //}
            return result;
        }
        string ShowItemPoison(int itemId, ref int line)
        {
            string text = "";
            List<string> list = new List<string>();
            for (int i = 0; i < 6; i++)
            {
                int num = int.Parse(DateFile.instance.GetItemDate(itemId, 71 + i, true));
                if (num > 0)
                {
                    list.Add(string.Format("{0}{1}{2}{3}\n", new object[]
                    {
                    WindowManage.instance.Dit(),
                    DateFile.instance.buffAttrDate[61 + i][0],
                    WindowManage.instance.Mut(),
                    DateFile.instance.SetColoer(int.Parse(DateFile.instance.buffAttrDate[61 + i][3]), num.ToString(), false)
                    }));
                    line += 1;
                }
            }
            if (list.Count > 0)
            {
                text += WindowManage.instance.SetMassageTitle(8007, 0, 11, 10002);
                for (int j = 0; j < list.Count; j++)
                {
                    text += list[j];
                }
                text += "\n";
                line += 1;
            }
            return text;
        }
        int Color6(int a, int max, int b = 100)
        {
            int result = 20008;
            if (a >= max)
            {
                result = 20005;
            }
            else if (a >= b)
            {
                result = 20004;
            }
            else if (a < b * 50 / 100)
            {
                result = 20010;
            }
            return result;
        }

    }

}
