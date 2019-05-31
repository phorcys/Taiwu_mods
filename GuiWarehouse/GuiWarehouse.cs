using Harmony12;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace GuiWarehouse
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        public int useWarehouse = 0; //0新版仓库 1鬼的仓库 2原本仓库
        //public int openTitle = 0;//默认打开仓库标签


        public int tackNum = 5;//自定义存取物品数量 配置
        public int numberOfColumns = 8;//几列物品 配置
        public int scrollSpeed = 30;//滚动速度 配置

        public bool remoteWarehouse = false;//远程仓库

        public int useClassify = 1;//使用分类

        public int levelClassify = Main.MaxLevelClassify();//等级筛选
        public int bookClassify = Main.MaxBookClassify();//书籍筛选
        //public int attrClassify = Main.MaxAttrClassify();//属性



    }
    public static class Main
    {
        //public static bool onOpenPackage = false;//使用茄子仓库时记录是打开仓库，物品强制显示为设置默认打开 背包
        //public static bool onOpenWarehouse = false;//使用茄子仓库时记录是打开仓库，物品强制显示为设置默认打开 仓库
        public static string keyWords = "";
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static bool OnChangeItem = false;

        public static WarehouseUI warehouse;
        ///// <summary>
        ///// 对象池根节点
        ///// </summary>
        //public static Transform poolRoot;
        ///// <summary>
        ///// 对象
        ///// </summary>
        //public static Stack<GameObject> itemObjs;

        public static int MaxLevelClassify()
        {
            int value = 0;
            for (int i = 0; i < warehouse.levelClassify.Length - 1; i++)
            {
                value |= 1 << i;
            }
            return value;
        }
        public static int MaxBookClassify()
        {
            int value = 0;
            for (int i = 0; i < warehouse.bookClassify.Length - 1; i++)
            {
                value |= 1 << i;
            }
            return value;
        }
        //public static int MaxAttrClassify()
        //{
        //    int value = 0;
        //    for (int i = 0; i < warehouse.attrClassify.Length - 1; i++)
        //    {
        //        value |= 1 << i;
        //    }
        //    return value;
        //}

        public static bool Load(UnityModManager.ModEntry modEntry)
        {

            warehouse = WarehouseUI.GetWarehouseUI();
            warehouse.open = false;
            //itemObjs = new Stack<GameObject>();
            settings = Settings.Load<Settings>(modEntry);
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            HarmonyInstance harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            GameObject pool = new GameObject();
            MonoBehaviour.DontDestroyOnLoad(pool);
            
            return true;
        }
        static string title = "鬼的仓库";

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(title, GUILayout.Width(300));
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(Main.settings.useWarehouse == 0, "新版仓库"))
            {
                Warehouse_UpdateActorItems_Patch.SetNewWarehouse(true);
                Main.settings.useWarehouse = 0;
            }
            if (GUILayout.Toggle(Main.settings.useWarehouse == 1, "旧版仓库"))
            {
                Main.settings.useWarehouse = 1;
            }
            if (GUILayout.Toggle(Main.settings.useWarehouse == 2, "原版仓库"))
            {
                Warehouse_UpdateActorItems_Patch.SetNewWarehouse(false);
                Main.settings.useWarehouse = 2;
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            string sNum = GUILayout.TextField(Main.settings.numberOfColumns.ToString());
            int num;
            if (int.TryParse(sNum, out num))
            {
                if (num > 0 || num < 1000)
                {
                    Main.settings.numberOfColumns = num;
                }
            }
            GUILayout.Label(string.Format("←←←←←← 设置背包一行显示{0}个物品：   <color=#F63333>修改行数和仓库版本设置建议重启游戏！</color>", Main.settings.numberOfColumns));
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.Label("<color=#F63333>取消拖拽存取物品功能    按住Ctrl+点击物品存取全部物品</color>");
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            string content = GUILayout.TextField(Main.settings.tackNum.ToString());
            int takeNum;
            if (int.TryParse(content, out takeNum))
            {
                if (takeNum > 0 || takeNum < 1000)
                {
                    Main.settings.tackNum = takeNum;
                }
            }
            GUILayout.Label(string.Format("<color=#F63333>←←←←←← 设置按住Shift+点击物品存储{0}个物品</color>", Main.settings.tackNum));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            string speed = GUILayout.TextField(Main.settings.scrollSpeed.ToString());
            int s;
            if (int.TryParse(speed, out s))
            {
                if (s > 0 || s < 1000)
                {
                    Main.settings.scrollSpeed = s;
                }
            }
            GUILayout.Label(string.Format("←←←←←← 设置背包滚轮滑动速度{0}", Main.settings.scrollSpeed));
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            Main.settings.useClassify = GUILayout.HorizontalSlider(Main.settings.useClassify, 0, 1) <= 0.5f ? 0 : 1;
            GUILayout.Label(string.Format("开启分类搜索：<color=#F63333>（{0}）</color>", Main.settings.useClassify == 0 ? "关" : "开"));
            if (Main.settings.useClassify == 1)
            {

                Main.settings.levelClassify = Main.MaxLevelClassify();//等级筛选
                Main.settings.bookClassify = Main.MaxBookClassify();//书籍筛选
                                                                    //Main.settings.attrClassify = Main.MaxAttrClassify();//属性
            }
            GUILayout.EndHorizontal();
            //GUILayout.BeginHorizontal();
            //GUILayout.Label("默认打开仓库标签：");
            //if (warehouse != null)
            //{
            //    for (int i = 0; i < warehouse.titleName.Length; i++)
            //    {
            //        if (GUILayout.Toggle(Main.settings.openTitle == i, warehouse.titleName[i]))
            //        {
            //            Main.settings.openTitle = i;
            //        }
            //    }
            //}
            //GUILayout.EndHorizontal();
        }

        [HarmonyPatch(typeof(Warehouse), "ChangeItem")]
        public static class Warehouse_ChangeItem_Patch
        {


            static bool Prefix(string changTyp, int itemId, int number = 1)
            {
                //Logger.Log("点击物品 " + changTyp + " " + itemId.ToString() + " " + number.ToString());
                if (Main.settings.useWarehouse == 2)
                {
                    return true;
                }
                int num = 1;
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    if (int.Parse(DateFile.instance.GetItemDate(itemId, 6, true)) > 0)
                    {
                        num = DateFile.instance.GetItemNumber(changTyp == "ActorItem" ? DateFile.instance.MianActorID() : -999, itemId);
                    }
                }else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    if (int.Parse(DateFile.instance.GetItemDate(itemId, 6, true)) > 0)
                    {
                        int maxNum = DateFile.instance.GetItemNumber(changTyp == "ActorItem" ? DateFile.instance.MianActorID() : -999, itemId);
                        if (Main.settings.tackNum > maxNum)
                        {
                            num = maxNum;
                        }
                        else
                        {
                            num = Main.settings.tackNum;
                        }
                    }
                }
                if (changTyp == "ActorItem")
                {
                    DateFile.instance.ChangeTwoActorItem(DateFile.instance.MianActorID(), -999, itemId, num, -1);
                }
                else
                {
                    DateFile.instance.ChangeTwoActorItem(-999, DateFile.instance.MianActorID(), itemId, num, -1);
                }
                Main.OnChangeItem = true;
                Warehouse_UpdateActorItems_Patch.Prefix(true, Warehouse.instance.actorItemTyp);
                Warehouse_UpdateActorItems_Patch.Prefix(false, Warehouse.instance.warehouseItemTyp);
                Main.OnChangeItem = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(Warehouse), "OpenWarehouse")]
        public static class Warehouse_OpenWarehouse_Patch
        {
            static bool Prefix()
            {
                //Main.Logger.Log("打开仓库 ");
                if (Main.settings.useWarehouse == 1)
                {
                    Main.warehouse.open = true;
                    return false;
                }
                else
                {
                    //Main.onOpenPackage = true;
                    //Main.onOpenWarehouse = true;
                    return true;
                }
            }
        }


        [HarmonyPatch(typeof(Warehouse), "UpdateActorItems")]
        public static class Warehouse_UpdateActorItems_Patch
        {
            public static bool isInit = false;
            public static NewWarehouse[] actorItemHolder;
            public static NewWarehouse[] warehouseItemHolder;

            public static void UpdateData()
            {
                Prefix(true, Warehouse.instance.actorItemTyp);
                Prefix(false, Warehouse.instance.warehouseItemTyp);
            }

            public static void SetNewWarehouse(bool active)
            {
                if (actorItemHolder!=null)
                {
                    for (int i = 0; i < actorItemHolder.Length; i++)
                    {
                        if (actorItemHolder[i]!=null&&actorItemHolder[i].scrollRect != null)
                        {
                            actorItemHolder[i].scrollRect.gameObject.SetActive(active);
                        }
                    }
                    for (int i = 0; i < warehouseItemHolder.Length; i++)
                    {
                        if (warehouseItemHolder[i]!=null&&warehouseItemHolder[i].scrollRect != null)
                        {
                            warehouseItemHolder[i].scrollRect.gameObject.SetActive(active);
                        }
                    }
                }
            }

            
            public static bool Prefix(bool actor, int typ)
            {
                //Main.Logger.Log("更新物品 " + actor.ToString() + " " + typ.ToString());

                //if (actor)
                //{
                //    if (Main.onOpenPackage && typ != Main.settings.openTitle)
                //    {
                //        typ = Main.settings.openTitle;
                //    }
                //    Main.onOpenPackage = false;
                //}
                //else
                //{
                //    if (Main.onOpenWarehouse && typ != Main.settings.openTitle)
                //    {
                //        typ = Main.settings.openTitle;
                //    }
                //    Main.onOpenWarehouse = false;
                //}

                if (Main.settings.useWarehouse == 2)
                {
                    //QieziUpdateItem(actor, typ);
                    return true;
                }

                if (!isInit)
                {
                    isInit = true;
                    actorItemHolder = new NewWarehouse[Warehouse.instance.actorItemHolder.Length];
                    for (int i = 0; i < Warehouse.instance.actorItemHolder.Length; i++)
                    {
                        actorItemHolder[i] = Warehouse.instance.actorItemHolder[i].parent.gameObject.AddComponent<NewWarehouse>();
                    }
                    warehouseItemHolder = new NewWarehouse[Warehouse.instance.warehouseItemHolder.Length];
                    for (int i = 0; i < Warehouse.instance.warehouseItemHolder.Length; i++)
                    {
                        warehouseItemHolder[i] = Warehouse.instance.warehouseItemHolder[i].parent.gameObject.AddComponent<NewWarehouse>();
                    }
                }

                if (actor)
                {
                    for (int i = 0; i < actorItemHolder.Length; i++)
                    {
                        if (actorItemHolder[i] == null)
                        {
                            actorItemHolder[i] = Warehouse.instance.actorItemHolder[i].parent.gameObject.AddComponent<NewWarehouse>();
                        }
                    }


                    if (Warehouse.instance.actorItemTyp != 0 && Warehouse.instance.actorItemTyp != typ)
                    {
                        Warehouse.instance.actorItemToggle[typ].isOn = true;
                    }
                    Warehouse.instance.actorItemTyp = typ;

                    int num2 = DateFile.instance.MianActorID();
                    int maxItemSize = ActorMenu.instance.GetMaxItemSize(num2);
                    int useItemSize = ActorMenu.instance.GetUseItemSize(num2);
                    Warehouse.instance.actorItemSize.text = string.Format("{0}{3}{1} / {2}</color>", new object[]
                    {
                        ActorMenu.instance.Color8(useItemSize, maxItemSize),
                        ((float)useItemSize / 100f).ToString("f1"),
                        ((float)maxItemSize / 100f).ToString("f1"),
                        DateFile.instance.massageDate[807][2].Split(new char[]
                        {
                            '|'
                        })[0]
                    });

                    //设置数据
                    if (!actorItemHolder[typ].isInit)
                    {
                        actorItemHolder[typ].Init(actor,typ);
                    }
                    //List<int> list = DateFile.instance.GetItemSort(new List<int>(ActorMenu.instance.GetActorItems(num2, 0, false).Keys));
                    List<int> list = new List<int>(DateFile.instance.GetItemSort(new List<int>(ActorMenu.instance.GetActorItems(num2).Keys)));
                    int[] data = Select(list, typ, actor);
                    actorItemHolder[typ].data = data;
                }
                else
                {
                    for (int i = 0; i < warehouseItemHolder.Length; i++)
                    {
                        if (warehouseItemHolder[i] == null)
                        {
                            warehouseItemHolder[i] = Warehouse.instance.warehouseItemHolder[i].parent.gameObject.AddComponent<NewWarehouse>();
                        }
                    }

                    if (Warehouse.instance.warehouseItemTyp != 0 && Warehouse.instance.warehouseItemTyp != typ)
                    {
                        Warehouse.instance.warehouseItemToggle[typ].isOn = true;
                    }
                    Warehouse.instance.warehouseItemTyp = typ;

                    int num4 = -999;
                    int warehouseMaxSize = Warehouse.instance.GetWarehouseMaxSize();
                    int useItemSize2 = ActorMenu.instance.GetUseItemSize(num4);
                    Warehouse.instance.warehouseItemSize.text = string.Format("{0}{3}{1} / {2}</color>", new object[]
                    {
                        ActorMenu.instance.Color8(useItemSize2, warehouseMaxSize),
                        ((float)useItemSize2 / 100f).ToString("f1"),
                        ((float)warehouseMaxSize / 100f).ToString("f1"),
                        DateFile.instance.massageDate[807][2].Split(new char[]
                        {
                            '|'
                        })[1]
                    });
                    HomeSystem.instance.UpdateButtonText();

                    //设置数据
                    if (!warehouseItemHolder[typ].isInit)
                    {
                        warehouseItemHolder[typ].Init(actor, typ);
                    }
                    //List<int> list = DateFile.instance.GetItemSort(new List<int>(ActorMenu.instance.GetActorItems(num4, 0, false).Keys));
                    List<int> list = new List<int>(DateFile.instance.GetItemSort(new List<int>(ActorMenu.instance.GetActorItems(num4).Keys)));
                    int[] data = Select(list, typ, actor);
                    warehouseItemHolder[typ].data = data;


                    HomeSystem.instance.UpdateButtonText();
                }
                return false;
            }

            // 筛选物品
            static int[] Select(List<int> list,int typ,bool actor)
            {
                if(Main.settings.levelClassify==0
                    || ((Main.settings.bookClassify == 0) && (warehouse.showBookClassify[typ] == 1))
                    //||((Main.settings.attrClassify == 0) && ((actor && warehouse.showAttrClassify[Warehouse.instance.actorItemTyp] == 1) || (!actor && warehouse.showAttrClassify[Warehouse.instance.warehouseItemTyp] == 1)))
                    )
                {
                    return new int[0];
                }

                bool needLevel = Main.MaxLevelClassify() != Main.settings.levelClassify;
                bool needBook = Main.MaxBookClassify() != Main.settings.bookClassify && (warehouse.showBookClassify[typ] == 1);
                //bool needAttr = Main.MaxAttrClassify() != Main.settings.attrClassify && ((actor && warehouse.showAttrClassify[Warehouse.instance.actorItemTyp] == 1) || (!actor && warehouse.showAttrClassify[Warehouse.instance.warehouseItemTyp] == 1));
                bool needSearch = (!string.IsNullOrEmpty(Main.keyWords));
                if (typ == 0
                    && !needLevel
                    && !needBook
                    //&& !needAttr
                    && !needSearch
                    )
                {
                    return list.ToArray();
                }
                List<int> result = new List<int>();
                string des = string.Empty;

                for (int i = 0; i < list.Count; i++)
                {
                    int itemId = list[i];
                    int itemTyp = int.Parse(DateFile.instance.GetItemDate(itemId, 4, true));
                    if (typ == 0 || typ == itemTyp)
                    {
                        if (needLevel)
                        {
                            if (Main.settings.levelClassify != 0)
                            {
                                int level = 1 << (9 - int.Parse(DateFile.instance.GetItemDate(itemId, 8, false)));
                                if ((Main.settings.levelClassify & level) != level)
                                {
                                    continue;
                                }
                            }
                        }
                        if(needBook
                            //|| needAttr
                            || needSearch
                            )
                        {
                            int k;
                            string s1 = DateFile.instance.GetItemDate(itemId, 0, true);
                            string s2 = warehouse.GetDes(true, itemId, out k);
                            des = s1 + s2;
                        }
                        if (needBook)
                        {
                            bool isAdd = false;
                            if (Main.settings.bookClassify != 0)
                            {
                                for (int j = 1; j < warehouse.bookClassify.Length; j++)
                                {
                                    if ((Main.settings.bookClassify | (1 << (j - 1))) == Main.settings.bookClassify)
                                    {
                                        if (i != 1&&(itemTyp == 3|| itemTyp == 4))
                                        {
                                            if (des.Contains((warehouse.bookClassify[j] + "：")))
                                            {
                                                isAdd = true;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (des.Contains((warehouse.bookClassify[j])))
                                            {
                                                isAdd = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            if (!isAdd)
                            {
                                continue;
                            }
                        }
                        //if (needAttr)
                        //{
                        //    bool isAdd = false;
                        //    if (Main.settings.attrClassify != 0)
                        //    {
                        //        for (int j = 1; j < warehouse.attrClassify.Length; j++)
                        //        {
                        //            if ((Main.settings.attrClassify | (1 << (j - 1))) == Main.settings.attrClassify)
                        //            {
                        //                if (i == 1)
                        //                {
                        //                    if (des.Contains((warehouse.attrClassify[j])))
                        //                    {
                        //                        isAdd = true;
                        //                        break;
                        //                    }
                        //                }
                        //                else
                        //                {
                        //                    if (des.Contains((warehouse.attrClassify[j] + "：")))
                        //                    {
                        //                        isAdd = true;
                        //                        break;
                        //                    }
                        //                }
                        //            }
                        //        }
                        //    }
                        //    if (!isAdd)
                        //    {
                        //        continue;
                        //    }
                        //}
                        if (needSearch)
                        {
                            bool v1 = false;
                            string[] sl = Main.keyWords.Split('|');
                            for (int j = 0; j < sl.Length; j++)
                            {
                                if(sl.Length > 0)
                                {
                                    bool v2 = true;
                                    string[] sl2 = sl[j].Split('&');
                                    if(sl2.Length > 0)
                                    {
                                        for (int k = 0; k < sl2.Length; k++)
                                        {
                                            if (!des.Contains((sl2[k])))
                                            {
                                                v2 = false;
                                                break;
                                            }
                                        }
                                    }
                                    if (!v1 && v2)
                                    {
                                        v1 = true;
                                        break;
                                    }
                                }
                            }
                            if (!v1)
                            {
                                continue;
                            }
                        }
                        result.Add(list[i]);
                    }
                }
                return result.ToArray();
            }


            static bool QieziUpdateItem(bool actor, int typ)
            {
                int num = 0;
                var _this = Warehouse.instance;
                if (actor)
                {
                    int num2 = DateFile.instance.MianActorID();
                    if (_this.actorItemTyp != 0 && _this.actorItemTyp != typ)
                    {
                        _this.actorItemToggle[typ].isOn = true;
                    }
                    _this.actorItemTyp = typ;
                    bool flag = HomeSystem.instance.homeMapPartId != DateFile.instance.mianPartId || HomeSystem.instance.homeMapPlaceId != DateFile.instance.mianPlaceId;
                    int childCount = _this.actorItemHolder[typ].childCount;
                    List<int> list = new List<int>(DateFile.instance.GetItemSort(new List<int>(ActorMenu.instance.GetActorItems(num2, 0, false).Keys)));
                    for (int i = 0; i < list.Count; i++)
                    {
                        int num3 = list[i];
                        if (typ == 0 || typ == int.Parse(DateFile.instance.GetItemDate(num3, 4, true)))
                        {
                            num++;
                            GameObject gameObject;
                            if (childCount >= num)
                            {
                                gameObject = _this.actorItemHolder[typ].GetChild(num - 1).gameObject;
                                gameObject.SetActive(true);
                            }
                            else
                            {
                                gameObject = UnityEngine.Object.Instantiate<GameObject>(_this.warehouseItemIcon, Vector3.zero, Quaternion.identity);
                                gameObject.transform.SetParent(_this.actorItemHolder[typ], false);
                            }
                            gameObject.name = "ActorItem," + num3;
                            gameObject.GetComponent<SetItem>().SetWarehouseItemIcon(num2, num3, int.Parse(DateFile.instance.GetItemDate(num3, 3, true)) != 1 || flag, _this.warehouseItemDes, 201);
                        }
                    }
                    if (childCount > num)
                    {
                        for (int j = num; j < childCount; j++)
                        {
                            _this.actorItemHolder[typ].GetChild(j).gameObject.SetActive(false);
                        }
                    }
                    int maxItemSize = ActorMenu.instance.GetMaxItemSize(num2);
                    int useItemSize = ActorMenu.instance.GetUseItemSize(num2);
                    _this.actorItemSize.text = string.Format("{0}{3}{1} / {2}</color>", new object[]
                    {
                    ActorMenu.instance.Color8(useItemSize, maxItemSize),
                    ((float)useItemSize / 100f).ToString("f1"),
                    ((float)maxItemSize / 100f).ToString("f1"),
                    DateFile.instance.massageDate[807][2].Split(new char[]
                    {
                        '|'
                    })[0]
                    });
                }
                else
                {
                    int num4 = -999;
                    if (_this.warehouseItemTyp != 0 && _this.warehouseItemTyp != typ)
                    {
                        _this.warehouseItemToggle[typ].isOn = true;
                    }
                    _this.warehouseItemTyp = typ;
                    bool cantTake = HomeSystem.instance.homeMapPartId != DateFile.instance.mianPartId || HomeSystem.instance.homeMapPlaceId != DateFile.instance.mianPlaceId;
                    int childCount2 = _this.warehouseItemHolder[typ].childCount;
                    List<int> list2 = new List<int>(DateFile.instance.GetItemSort(new List<int>(ActorMenu.instance.GetActorItems(num4, 0, false).Keys)));
                    for (int k = 0; k < list2.Count; k++)
                    {
                        int num5 = list2[k];
                        if (typ == 0 || typ == int.Parse(DateFile.instance.GetItemDate(num5, 4, true)))
                        {
                            num++;
                            GameObject gameObject2;
                            if (childCount2 >= num)
                            {
                                gameObject2 = _this.warehouseItemHolder[typ].GetChild(num - 1).gameObject;
                                gameObject2.SetActive(true);
                            }
                            else
                            {
                                gameObject2 = UnityEngine.Object.Instantiate<GameObject>(_this.warehouseItemIcon, Vector3.zero, Quaternion.identity);
                                gameObject2.transform.SetParent(_this.warehouseItemHolder[typ], false);
                            }
                            gameObject2.name = "WarehouseItem," + num5;
                            gameObject2.GetComponent<SetItem>().SetWarehouseItemIcon(num4, num5, cantTake, _this.actorItemDes, 202);
                        }
                    }
                    if (childCount2 > num)
                    {
                        for (int l = num; l < childCount2; l++)
                        {
                            _this.warehouseItemHolder[typ].GetChild(l).gameObject.SetActive(false);
                        }
                    }
                    int warehouseMaxSize = _this.GetWarehouseMaxSize();
                    int useItemSize2 = ActorMenu.instance.GetUseItemSize(-999);
                    _this.warehouseItemSize.text = string.Format("{0}{3}{1} / {2}</color>", new object[]
                    {
                    ActorMenu.instance.Color8(useItemSize2, warehouseMaxSize),
                    ((float)useItemSize2 / 100f).ToString("f1"),
                    ((float)warehouseMaxSize / 100f).ToString("f1"),
                    DateFile.instance.massageDate[807][2].Split(new char[]
                    {
                        '|'
                    })[1]
                    });
                    HomeSystem.instance.UpdateButtonText();
                }
                return false;
            }
        }
    }
}