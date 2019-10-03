using UnityEngine;
using Harmony12;
using UnityModManagerNet;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SwitchTheBook
{
    
    static class GongFaDict
    {
        public static bool IsDictLoaded = false;
        private static Dictionary<int, int> gongfaDict = new Dictionary<int, int>();

        private static int _gongfa_index(int gongfaid, int ispirate)
        {
            return gongfaid * 2 + ispirate;
        }

        public static int Size()
        {
            return gongfaDict.Count();
        }

        public static void Add_new_GongFa(int gongfaid, int ispirate, int itemtrueid)
        {
            int indexKey = _gongfa_index(gongfaid, ispirate);
            if (gongfaDict.ContainsKey(indexKey))
                Main.Logger.Log("We already have this Gongfa in GongFaDict: " + indexKey.ToString());
            else
                gongfaDict.Add(_gongfa_index(gongfaid, ispirate), itemtrueid);
        }

        public static int Get_GongFa_ID(int gongfaid, int ispirate)
        {
            int itemtrueid = -1;
            int indexKey = _gongfa_index(gongfaid, ispirate);
            if (!gongfaDict.ContainsKey(indexKey))
                Main.Logger.Log("Fail to get Gongfa TrueID with index: " + indexKey.ToString());
            else
                itemtrueid = gongfaDict[indexKey];
            return itemtrueid;
        }
    }

    static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Logger = modEntry.Logger;

            modEntry.OnToggle = OnToggle;
            
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
                return false;

            enabled = value;

            return true;
        }
    }

    /// <summary>
    ///  鼠标事件处理
    /// </summary>
    public class ClickAction : MonoBehaviour, IPointerClickHandler
    {
        Text _text;
        int _itemid;
        int _actorid;
        public void OnPointerClick(PointerEventData eventData)
        {
            //将该书籍的真传和手抄属性进行切换
            int gongFaID = int.Parse(DateFile.instance.GetItemDate(_itemid, 32, false));
            int isPirate = int.Parse(DateFile.instance.GetItemDate(_itemid, 35, false));
            //int itemTrueID = int.Parse(DateFile.instance.GetItemDate(_itemid, 999, false));
            int newItemTrueID = GongFaDict.Get_GongFa_ID(gongFaID, 1 - isPirate);
            if (newItemTrueID == -1)
                Main.Logger.Error("We fail to get the GongFa ID:" + _text.text);
            else
                // DateFile.instance.itemsDate[_itemid][999] = newItemTrueID.ToString();
                GameData.Items.SetItemProperty(_itemid, 999, newItemTrueID.ToString());
            // DateFile.instance.ChangItemDate(_itemid, 999, newItemTrueID, true);

            DateFile.instance.GetItem(_actorid, _itemid, 1, false, 0, 0);
            // GameData.Items.GetItem(_itemid);

            Main.Logger.Log("We may complete the book switch.");

            // ActorMenu.instance.UpdateItemInformation(_itemid);
            //if (isPirate > 0)
            //    DateFile.instance.itemsDate[_itemid][999] = (itemTrueID - 200000).ToString();
            //else
            //    DateFile.instance.itemsDate[_itemid][999] = (itemTrueID + 200000).ToString();
        }

        public void SetParam(Text text, int itemid, int actorid)
        {
            _text = text;
            _itemid = itemid;
            _actorid = actorid;
        }
    }

    [HarmonyPatch(typeof(DateFile), "LoadDate")]
    public static class Loading_and_construct_GongFaDict
    {

        static void Prefix()
        {
            if (Main.enabled)
            {
                if (!GongFaDict.IsDictLoaded)
                {
                    if (DateFile.instance != null)
                    {
                        //建立功法ID的索引
                        int iterNum = 0;
                        foreach (KeyValuePair<int, Dictionary<int, string>> item in DateFile.instance.presetitemDate)
                        {
                            iterNum += 1;
                            //确认该物品为图纸或图书
                            if (int.Parse(item.Value[4]) == 5)
                                //确认该物品为图书
                                if (int.Parse(item.Value[5]) == 21)
                                    //确认该物品为功法书
                                    if (int.Parse(item.Value[506]) >= 20)
                                        //将其加入功法词典
                                        GongFaDict.Add_new_GongFa(int.Parse(item.Value[32]),
                                            int.Parse(item.Value[35]), int.Parse(item.Value[999]));
                        }
                        GongFaDict.IsDictLoaded = true;
                        Main.Logger.Log("GongFaDict Loaded with size: " + GongFaDict.Size().ToString());
                        Main.Logger.Log("Totally find itemsDate with size: " + iterNum);
                        Main.Logger.Log("Dict of itemsDate with size: " + DateFile.instance.presetitemDate.Count().ToString());
                    }
                    else
                        Main.Logger.Error("GongFaDict already Loaded?");
                }
                else
                    Main.Logger.Error("We fail to load the GongFaDict!");
            }
        }
    }
    public class SwitchBook
    {
        //进行具体的切换行为
        static void DoSwitch(SetItem __instance, int itemId)
        {
            bool actionFlag = false;

            //Main.Logger.Log("Lets start to switch for " + __instance.itemNumber.text);

            //确认该物品为图纸或图书
            if (int.Parse(DateFile.instance.GetItemDate(itemId, 4, false)) == 5)
                //确认该物品为图书
                if (int.Parse(DateFile.instance.GetItemDate(itemId, 5, false)) == 21)
                    //确认该物品为功法书
                    if (int.Parse(DateFile.instance.GetItemDate(itemId, 506, false)) >= 20)
                        //通过处理许可
                        actionFlag = true;

            if (!actionFlag)
                return;

            //添加相应处理Component,注入参数
            int actorId = DateFile.instance.mianActorId;
            var iconobj = __instance.gameObject;
            var clickActions = iconobj.GetComponents<ClickAction>();
            if (clickActions.Length >= 1)//避免重复添加
            {
                clickActions[0].SetParam(__instance.itemNumber, itemId, actorId);
            }
            else
            {
                var actionstub = iconobj.AddComponent<ClickAction>();
                actionstub.SetParam(__instance.itemNumber, itemId, actorId);
            }
            // var actionstub = iconobj.AddComponent<ClickAction>();
            // actionstub.setParam(__instance.itemNumber, itemId);
        }

        //将人物包裹中的功法书籍通过点击进行真传和手抄的切换
        [HarmonyPatch(typeof(SetItem), "SetActorMenuItemIcon")]
        public static class SetItem_SetActorMenuItemIcon_Patch
        {
            static void Postfix(SetItem __instance, int actorId, int itemId, int actorFavor, int injuryTyp)
            {
                if (!Main.enabled)
                    return;

                //Dictionary<int, int> gg = new Dictionary<int, int>();
                //int cc = gg[1];

                //Main.Logger.Log("We get in for " + __instance.itemNumber.text);

                if (!GongFaDict.IsDictLoaded)
                    return;

                if (actorId != DateFile.instance.mianActorId)
                    return;


                DoSwitch(__instance, itemId);
                
            }
        }


        //将人物装备中的功法书籍通过点击进行真传和手抄的切换
        [HarmonyPatch(typeof(SetItem), "SetActorEquipIcon")]
        public static class SetItem_SetActorEquipIcon_Patch
        {
            static void Postfix(SetItem __instance, int itemId)
            {
                if (!Main.enabled)
                    return;

                //Dictionary<int, int> gg = new Dictionary<int, int>();
                //int cc = gg[1];

                //Main.Logger.Log("We get in for " + __instance.itemNumber.text);

                if (!GongFaDict.IsDictLoaded)
                    return;

                if (ActorMenu.instance.actorId != DateFile.instance.mianActorId)
                    return;

                DoSwitch(__instance, itemId);

            }
        }
    }

    
}