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
        public bool open = true; //使用鬼的仓库
        public int openTitle = 0;//默认打开仓库标签

    }
    public static class Main
    {
        public static bool onOpen = false;//使用茄子仓库时记录是打开仓库，物品强制显示为设置默认打开
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static WarehouseUI warehouse;
        ///// <summary>
        ///// 对象池根节点
        ///// </summary>
        //public static Transform poolRoot;
        ///// <summary>
        ///// 对象
        ///// </summary>
        //public static Stack<GameObject> itemObjs;

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
            Main.settings.open = GUILayout.Toggle(Main.settings.open, "使用快捷仓库  点击物品存取1个  按住Ctrl+点击物品存取全部");
            GUILayout.BeginHorizontal();
            GUILayout.Label("默认打开仓库标签：");
            if (warehouse != null)
            {
                for (int i = 0; i < warehouse.titleName.Length; i++)
                {
                    if (GUILayout.Toggle(Main.settings.openTitle == i, warehouse.titleName[i]))
                    {
                        Main.settings.openTitle = i;
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        [HarmonyPatch(typeof(Warehouse), "OpenWarehouse")]
        public static class Warehouse_OpenWarehouse_Patch
        {
            static bool Prefix()
            {
                if (Main.settings.open)
                {
                    Main.warehouse.open = true;
                    return false;
                }
                else
                {
                    Main.onOpen = true;
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(Warehouse), "UpdateActorItems")]
        public static class Warehouse_UpdateActorItems_Patch
        {
            static bool Prefix(bool actor, int typ)
            {
                if (Main.onOpen)
                {
                    typ = Main.settings.openTitle;
                    if (!actor)
                    {
                        Main.onOpen = false;
                    }
                }

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
                    List<int> list = new List<int>(DateFile.instance.GetItemSort(new List<int>(ActorMenu.instance.GetActorItems(num2, 0).Keys)));
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
                    List<int> list2 = new List<int>(DateFile.instance.GetItemSort(new List<int>(ActorMenu.instance.GetActorItems(num4, 0).Keys)));
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

        //static GameObject InstanceObj()
        //{
        //    if(itemObjs.Count > 1)
        //    {
        //       return itemObjs.Pop();
        //    }
        //    else
        //    {
        //        return UnityEngine.Object.Instantiate<GameObject>(Warehouse.instance.warehouseItemIcon, Vector3.zero, Quaternion.identity);
        //    }
        //}

        //static void DestroyObj(GameObject obj)
        //{
        //    if (obj == null)
        //    {
        //        return;
        //    }
        //    Transform tf = obj.transform;
        //    tf.SetParent(poolRoot, false);
        //    itemObjs.Push(obj);
        //}

        //[HarmonyPatch(typeof(Warehouse), "RemoveAllActorItems")]
        //public static class Warehouse_RemoveAllActorItems_Patch
        //{
        //    static bool Prefix()
        //    {
        //        var AllActorItems = Warehouse_UpdateActorItems_Patch.AllActorItems;
        //        var keys = AllActorItems.Keys;
        //        foreach (var i in keys)
        //        {
        //            var list = AllActorItems[i];
        //            foreach (var item in list)
        //            {
        //                Main.DestroyObj(item);
        //            }
        //            list.Clear();
        //        }
        //        return false;
        //    }
        //}

        //[HarmonyPatch(typeof(Warehouse), "RemoveAllWarehouseItems")]
        //public static class Warehouse_RemoveAllWarehouseItems_Patch
        //{
        //    static bool Prefix()
        //    {
        //        var AllActorItems = Warehouse_UpdateActorItems_Patch.AllWarehouseItems;
        //        var keys = AllActorItems.Keys;
        //        foreach (var i in keys)
        //        {
        //            var list = AllActorItems[i];
        //            foreach (var item in list)
        //            {
        //                Main.DestroyObj(item);
        //            }
        //            list.Clear();
        //        }
        //        return false;
        //    }
        //}


        //[HarmonyPatch(typeof(Warehouse), "UpdateActorItems")]
        //public static class Warehouse_UpdateActorItems_Patch
        //{
        //    public static Dictionary<int, List<GameObject>> AllWarehouseItems = new Dictionary<int, List<GameObject>>();
        //    public static Dictionary<int, List<GameObject>> AllActorItems = new Dictionary<int, List<GameObject>>();

        //    static List<GameObject> getWarehouseItems(int typ)
        //    {
        //        if (!AllWarehouseItems.ContainsKey(typ))
        //        {
        //            AllWarehouseItems.Add(typ, new List<GameObject>());
        //        }
        //        return AllWarehouseItems[typ];
        //    }
        //    static List<GameObject> getActorItems(int typ)
        //    {
        //        if (!AllActorItems.ContainsKey(typ))
        //        {
        //            AllActorItems.Add(typ, new List<GameObject>());
        //        }
        //        return AllActorItems[typ];
        //    }

        //    static bool Prefix(bool actor, int typ)
        //    {
        //        Main.Logger.Log("UpdateActorItems " + actor.ToString() + " " + typ.ToString());
        //        Warehouse _this = Warehouse.instance;
        //        int num = 0;
        //        if (actor)
        //        {
        //            int num2 = DateFile.instance.MianActorID();
        //            if (_this.actorItemTyp != 0 && _this.actorItemTyp != typ)
        //            {
        //                _this.actorItemToggle[typ].isOn = true;
        //            }
        //            _this.actorItemTyp = typ;
        //            bool flag = HomeSystem.instance.homeMapPartId != DateFile.instance.mianPartId || HomeSystem.instance.homeMapPlaceId != DateFile.instance.mianPlaceId;
        //            int childCount = _this.actorItemHolder[typ].childCount;
        //            List<int> list = new List<int>(DateFile.instance.GetItemSort(new List<int>(ActorMenu.instance.GetActorItems(num2, 0).Keys)));
        //            for (int i = 0; i < list.Count; i++)
        //            {
        //                int num3 = list[i];
        //                if (typ == 0 || typ == int.Parse(DateFile.instance.GetItemDate(num3, 4, true)))
        //                {
        //                    num++;
        //                    GameObject gameObject;
        //                    if (childCount >= num)
        //                    {
        //                        //gameObject = _this.actorItemHolder[typ].GetChild(num - 1).gameObject;
        //                        //Main.Logger.Log("显示已有子物体" + childCount.ToString() + " " + getActorItems(typ).Count.ToString() + " " + i.ToString());
        //                        gameObject = getActorItems(typ)[num - 1];
        //                        if (!gameObject.activeSelf)
        //                            gameObject.SetActive(true);
        //                    }
        //                    else
        //                    {
        //                        gameObject = Main.InstanceObj();
        //                        gameObject.transform.SetParent(_this.actorItemHolder[typ], false);
        //                        //Main.Logger.Log("添加子物体" + i.ToString());
        //                        getActorItems(typ).Add(gameObject);
        //                    }
        //                    gameObject.name = "ActorItem," + num3;
        //                    gameObject.GetComponent<SetItem>().SetWarehouseItemIcon(num2, num3, int.Parse(DateFile.instance.GetItemDate(num3, 3, true)) != 1 || flag, _this.warehouseItemDes, 201);
        //                }
        //            }
        //            if (childCount > num)
        //            {
        //                for (int j = num; j < childCount; j++)
        //                {
        //                    //_this.actorItemHolder[typ].GetChild(j).gameObject.SetActive(false);
        //                    //Main.Logger.Log("隐藏多余子物体" + childCount.ToString() + " " + getActorItems(typ).Count.ToString() + " " + j.ToString());
        //                     var go = getActorItems(typ)[j];
        //                    if (go.activeSelf)
        //                        go.SetActive(false);
        //                }
        //            }
        //            int maxItemSize = ActorMenu.instance.GetMaxItemSize(num2);
        //            int useItemSize = ActorMenu.instance.GetUseItemSize(num2);
        //            _this.actorItemSize.text = string.Format("{0}{3}{1} / {2}</color>", new object[]
        //            {
        //        ActorMenu.instance.Color8(useItemSize, maxItemSize),
        //        ((float)useItemSize / 100f).ToString("f1"),
        //        ((float)maxItemSize / 100f).ToString("f1"),
        //        DateFile.instance.massageDate[807][2].Split(new char[]
        //        {
        //            '|'
        //        })[0]
        //            });
        //        }
        //        else
        //        {
        //            int num4 = -999;
        //            if (_this.warehouseItemTyp != 0 && _this.warehouseItemTyp != typ)
        //            {
        //                _this.warehouseItemToggle[typ].isOn = true;
        //            }
        //            _this.warehouseItemTyp = typ;
        //            bool cantTake = HomeSystem.instance.homeMapPartId != DateFile.instance.mianPartId || HomeSystem.instance.homeMapPlaceId != DateFile.instance.mianPlaceId;
        //            int childCount2 = _this.warehouseItemHolder[typ].childCount;
        //            List<int> list2 = new List<int>(DateFile.instance.GetItemSort(new List<int>(ActorMenu.instance.GetActorItems(num4, 0).Keys)));
        //            for (int k = 0; k < list2.Count; k++)
        //            {
        //                int num5 = list2[k];
        //                if (typ == 0 || typ == int.Parse(DateFile.instance.GetItemDate(num5, 4, true)))
        //                {
        //                    num++;
        //                    GameObject gameObject2;
        //                    if (childCount2 >= num)
        //                    {
        //                        //Main.Logger.Log("显示已有子物体" + childCount2.ToString() + " " + getWarehouseItems(typ).Count.ToString() + " " + k.ToString());
        //                        //gameObject2 = _this.warehouseItemHolder[typ].GetChild(num - 1).gameObject;
        //                        gameObject2 = getWarehouseItems(typ)[num - 1];
        //                        if (!gameObject2.activeSelf)
        //                        {
        //                            gameObject2.SetActive(true);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        //Main.Logger.Log("添加子物体" + k.ToString());
        //                        gameObject2 = Main.InstanceObj();
        //                        gameObject2.transform.SetParent(_this.warehouseItemHolder[typ], false);
        //                        getWarehouseItems(typ).Add(gameObject2);
        //                    }
        //                    gameObject2.name = "WarehouseItem," + num5;
        //                    gameObject2.GetComponent<SetItem>().SetWarehouseItemIcon(num4, num5, cantTake, _this.actorItemDes, 202);
        //                }
        //            }
        //            if (childCount2 > num)
        //            {
        //                for (int l = num; l < childCount2; l++)
        //                {
        //                    //_this.warehouseItemHolder[typ].GetChild(l).gameObject.SetActive(false);
        //                    //Main.Logger.Log("隐藏多余子物体" + childCount2.ToString() + " " + getWarehouseItems(typ).Count.ToString() + " " + l.ToString());
        //                    var go = getWarehouseItems(typ)[l];
        //                    if (go.activeSelf)
        //                        go.SetActive(false);
        //                }
        //            }
        //            int warehouseMaxSize = _this.GetWarehouseMaxSize();
        //            int useItemSize2 = ActorMenu.instance.GetUseItemSize(-999);
        //            _this.warehouseItemSize.text = string.Format("{0}{3}{1} / {2}</color>", new object[]
        //            {
        //        ActorMenu.instance.Color8(useItemSize2, warehouseMaxSize),
        //        ((float)useItemSize2 / 100f).ToString("f1"),
        //        ((float)warehouseMaxSize / 100f).ToString("f1"),
        //        DateFile.instance.massageDate[807][2].Split(new char[]
        //        {
        //            '|'
        //        })[1]
        //            });
        //            HomeSystem.instance.UpdateButtonText();
        //        }
        //        return false;
        //    }
        //}
    }
}