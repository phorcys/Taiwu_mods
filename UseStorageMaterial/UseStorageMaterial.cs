using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Reflection.Emit;

namespace UseStorageMaterial
{

    public static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

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


    public static class UseStorangeMaterialHelper
    {
        public static bool isActorEquip(int actor_id, int itemid)
        {
            for (int i = 0; i < 12; i++)
            {
                int num3 = int.Parse(DateFile.instance.GetActorDate(actor_id, 301 + i, false));
                if (itemid != 0 && num3 == itemid)
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    ///  制造界面列表显示仓库物品，包括工具和引子材料，包括制造/修理/强化/淬毒界面
    ///  因为是直接修改函数，所以不能禁用
    /// </summary>

    [HarmonyPatch(typeof(MakeSystem), "SetItem")]
    public static class MakeSystem_SetItem_Patch
    {
        static bool Prefix()
        {
            if (!Main.enabled)
                return true;

            NewSetItem();
            return false;
        }

        static T2 GetValue<T1, T2>(T1 instance, string field)
        {
            var flags = BindingFlags.Instance
                | BindingFlags.Static
                | BindingFlags.NonPublic
                | BindingFlags.Public;
            return (T2)typeof(T1).GetField(field, flags).GetValue(instance);
        }

        static T2 Invoke<T1, T2>(T1 instance, string method, object[] args = null)
        {
            var flags = BindingFlags.Instance
                | BindingFlags.Static
                | BindingFlags.NonPublic
                | BindingFlags.Public;
            return (T2)typeof(T1).GetMethod(method, flags).Invoke(instance, args);
        }
        static void Invoke<T1>(T1 instance, string method, object[] args = null)
        {
            var flags = BindingFlags.Instance
                | BindingFlags.Static
                | BindingFlags.NonPublic
                | BindingFlags.Public;
            typeof(T1).GetMethod(method, flags).Invoke(instance, args);
        }

        static void NewSetItem()
        {
            var ins = MakeSystem.instance;

            Invoke(ins, "RemoveMianItem");

            int num = DateFile.instance.MianActorID();
            List<int> list = new List<int>(DateFile.instance.GetActorItems(num, 0, false).Keys);
            for (int i = 0; i < 12; i++)
            {
                int num2 = int.Parse(DateFile.instance.GetActorDate(num, 301 + i, applyBonus: false));
                if (num2 > 0)
                {
                    list.Add(num2);
                }
            }
            list.AddRange(DateFile.instance.GetActorItems(-999, 0, false).Keys);

            list = DateFile.instance.GetItemSort(list);

            var baseMakeTyp = ins.baseMakeTyp;
            var mianItem = ins.mianItem;
            var secondItem = ins.secondItem;
            var secondItemId = ins.secondItemId;
            var thirdItemId = ins.thirdItemId;
            var mianItemHolder = ins.mianItemHolder;
            var secondItemHolder = ins.secondItemHolder;
            var thirdItemHolder = ins.thirdItemHolder;
            var mianItemDragDes = ins.mianItemDragDes;
            var secondItemDragDes = ins.secondItemDragDes;
            var thirdItemDragDes = ins.thirdItemDragDes;
            var fixMianItemDragDes = ins.fixMianItemDragDes;
            var fixSecondItemDragDes = ins.fixSecondItemDragDes;
            var changeMianItemDragDes = ins.changeMianItemDragDes;
            var changeSecondItemDragDes = ins.changeSecondItemDragDes;
            var poisonMianItemDragDes = ins.poisonMianItemDragDes;
            var poisonSecondItemDragDes = ins.poisonSecondItemDragDes;
            var poisonThirdItemDragDes = ins.poisonThirdItemDragDes;

            switch (GetValue<MakeSystem, int>(ins, "makeTyp"))
            {
                case 0:
                    for (int num7 = 0; num7 < list.Count; num7++)
                    {
                        int id2 = list[num7];
                        if ((int.Parse(DateFile.instance.GetItemDate(id2, 41)) == baseMakeTyp
                            || (baseMakeTyp == 10 && int.Parse(DateFile.instance.GetItemDate(id2, 41)) == 9))
                            && int.Parse(DateFile.instance.GetItemDate(id2, 42)) > 0)
                        {
                            Invoke<MakeSystem>(ins, "SetMianToolItem", new object[] { id2, mianItem, mianItemHolder, mianItemDragDes, false });
                        }
                        if (int.Parse(DateFile.instance.GetItemDate(id2, 41)) == baseMakeTyp && int.Parse(DateFile.instance.GetItemDate(id2, 48)) > 0)
                        {
                            Invoke<MakeSystem>(ins, "SetMianToolItem", new object[] { id2, secondItem, secondItemHolder, secondItemDragDes, true});
                        }
                    }
                    if (DateFile.instance.teachingOpening == 203)
                    {
                        ins.StartCoroutine(Invoke<MakeSystem, System.Collections.IEnumerator>(ins, "Teaching2"));
                    }
                    break;
                case 1:
                    for (int l = 0; l < list.Count; l++)
                    {
                        int id = list[l];
                        if (int.Parse(DateFile.instance.GetItemDate(id, 41)) == baseMakeTyp && int.Parse(DateFile.instance.GetItemDate(id, 42)) > 0)
                        {
                            Invoke<MakeSystem>(ins, "SetMianToolItem", new object[] { id, mianItem, mianItemHolder, fixMianItemDragDes, false });
                        }
                        if (int.Parse(DateFile.instance.GetItemDate(id, 41)) == baseMakeTyp && int.Parse(DateFile.instance.GetItemDate(id, 49)) > 0 && int.Parse(DateFile.instance.GetItemDate(id, 901)) < int.Parse(DateFile.instance.GetItemDate(id, 902)))
                        {
                            Invoke<MakeSystem>(ins, "SetMianToolItem", new object[] { id, secondItem, secondItemHolder, fixSecondItemDragDes, false });
                        }
                    }
                    break;
                case 2:
                    for (int m = 0; m < list.Count; m++)
                    {
                        int num5 = list[m];
                        if (int.Parse(DateFile.instance.GetItemDate(num5, 41)) == baseMakeTyp && int.Parse(DateFile.instance.GetItemDate(num5, 42)) > 0)
                        {
                            Invoke<MakeSystem>(ins, "SetMianToolItem", new object[] { num5, mianItem, mianItemHolder, changeMianItemDragDes, false });
                        }
                        if (int.Parse(DateFile.instance.GetItemDate(num5, 41)) == baseMakeTyp && int.Parse(DateFile.instance.GetItemDate(num5, 50)) > 0)
                        {
                            Invoke<MakeSystem>(ins, "SetMianToolItem", new object[] { num5, secondItem, secondItemHolder, changeSecondItemDragDes, false });
                        }
                        if (int.Parse(DateFile.instance.GetItemDate(num5, 41)) == 10 || int.Parse(DateFile.instance.GetItemDate(num5, 51)) <= 0)
                        {
                            continue;
                        }
                        int num6 = DateFile.instance.GetItemNumber(num, num5);
                        for (int n = 0; n < thirdItemId.Length; n++)
                        {
                            if (thirdItemId[n] == num5)
                            {
                                num6--;
                            }
                        }
                        if (num6 > 0)
                        {
                            Invoke<MakeSystem>(ins, "SetThirdItem", new object[] { num5, thirdItemHolder[0], thirdItemDragDes, num6 });
                        }
                    }
                    break;
                case 3:
                    for (int j = 0; j < list.Count; j++)
                    {
                        int num3 = list[j];
                        if (int.Parse(DateFile.instance.GetItemDate(num3, 41)) == 9 && int.Parse(DateFile.instance.GetItemDate(num3, 42)) > 0)
                        {
                            Invoke<MakeSystem>(ins, "SetMianToolItem", new object[] { num3, mianItem, mianItemHolder, poisonMianItemDragDes, false });
                        }
                        if (int.Parse(DateFile.instance.GetItemDate(num3, 41)) == baseMakeTyp)
                        {
                            int num4 = DateFile.instance.GetItemNumber(num, num3);
                            for (int k = 0; k < thirdItemId.Length; k++)
                            {
                                if (thirdItemId[k] == num3)
                                {
                                    num4--;
                                }
                            }
                            if (num4 > 0 && num3 != secondItemId)
                            {
                                Invoke<MakeSystem>(ins, "SetThirdItem", new object[] { num3, thirdItemHolder[1], poisonThirdItemDragDes, num4 });
                            }
                        }
                        if (int.Parse(DateFile.instance.GetItemDate(num3, 53)) > 0 && num3 != thirdItemId[0] && num3 != thirdItemId[1] && num3 != thirdItemId[2])
                        {
                            Invoke<MakeSystem>(ins, "SetMianToolItem", new object[] { num3, secondItem, secondItemHolder, poisonSecondItemDragDes, false });
                        }
                    }
                    break;
            }
        }
    }



    /// <summary>
    ///  如果需要，销毁使用的制造引子，减少制造工具hp
    /// </summary>
    [HarmonyPatch(typeof(MakeSystem), "SetMakeItem")]
    public static class MakeSystem_SetMakeItem_Patch
    {
        //检测是否有道具
        private static void Prefix(MakeSystem __instance, int ___mianItemId, int ___secondItemId, ref int __state)
        {
            //flag
            __state = 0;
            int actorId = DateFile.instance.MianActorID();
            if (!DateFile.instance.actorItemsDate[actorId].ContainsKey(___mianItemId))
            {
                __state = __state | 1;
            }
            if (!DateFile.instance.actorItemsDate[actorId].ContainsKey(___secondItemId))
            {
                __state = __state | 2;
            }
        }

        //实际扣除函数
        private static void Postfix(MakeSystem __instance, int ___mianItemId, int ___secondItemId, int __state)
        {
            //主函数不能禁用，所以禁用没意义了
            //if (!Main.enabled)
            //{
            //    return;
            //}

            //Main.Logger.Log(" __state get state: " + __state);

            int actorId = DateFile.instance.MianActorID();

            if ((__state & 1) == 1)
            {
                if (0 == int.Parse(DateFile.instance.GetItemDate(___mianItemId, 901)))
                {
                    DateFile.instance.LoseItem(-999, ___mianItemId, -1, true, true);
                }

            }
            if ((__state & 2) == 2)
            {
                DateFile.instance.LoseItem(-999, ___secondItemId, 1, true, true);
            }
            return;
        }

    }


    /// <summary>
    ///  如果需要，销毁使用的淬毒材料hp，减少制造工具hp
    /// </summary>
    [HarmonyPatch(typeof(MakeSystem), "StartPoisonItem")]
    public static class MakeSystem_StartPoisonItem_Patch
    {
        //检测是否有道具
        private static bool Prefix(MakeSystem __instance, int ___mianItemId, int[] ___thirdItemId, int ___secondItemId, ref int __state)
        {
            __state = -1;
            //主工具
            int actorId = DateFile.instance.MianActorID();

            //主工具
            if (!DateFile.instance.actorItemsDate[actorId].ContainsKey(___mianItemId))
            {
                //仅当hp为1时，移除mianitem, 其余情况不扣hp，否则会扣两次
                if (int.Parse(DateFile.instance.GetItemDate(___mianItemId, 901)) == 1)
                {
                    DateFile.instance.ChangeItemHp(-999, ___mianItemId, -1, 0, true);
                }

                // Main.Logger.Log(" StartChangeItem use warehouse main item : ");
            }


            //毒液
            for (int j = 0; j < ___thirdItemId.Length; j++)
            {
                if (___thirdItemId[j] > 0 && !DateFile.instance.actorItemsDate[actorId].ContainsKey(___thirdItemId[j]))
                {
                    if (int.Parse(DateFile.instance.GetItemDate(___thirdItemId[j], 6, true)) > 0)
                    {
                        DateFile.instance.LoseItem(-999, ___thirdItemId[j], 1, true, true);
                        //Main.Logger.Log(" poison use warehouse third item ,remove it");
                    }
                    else
                    {
                        DateFile.instance.ChangeItemHp(-999, ___thirdItemId[j], -1, 0, true);
                        //Main.Logger.Log(" poison use warehouse third item , dec hp ");
                    }

                }
            }

            //被强化物品
            //被强化物品装备了
            if (UseStorangeMaterialHelper.isActorEquip(actorId, ___secondItemId) == true)
            {
                //啥也不干
            }
            else if (!DateFile.instance.actorItemsDate[actorId].ContainsKey(___secondItemId))
            {
                //被强化物品在仓库，先挪 到背包里
                __state = ___secondItemId;
                DateFile.instance.ChangeTwoActorItem(-999, DateFile.instance.MianActorID(), ___secondItemId, 1, -1, 0, 0);
                //Main.Logger.Log(" poison use warehouse second item : " + ___secondItemId + " move to bag");
            }
            return true;
        }

        //把移动到背包的物品，如果有需要，移动回去
        private static void Postfix(int __state)
        {
            //被强化物品
            int actorId = DateFile.instance.MianActorID();
            Main.Logger.Log(" poison use warehouse second item : " + __state + " state:" + __state + " move back to warehouse");
            if (__state > 0 && DateFile.instance.actorItemsDate[actorId].ContainsKey(__state))
            {
                //Main.Logger.Log(" poison use warehouse second item : " + __state + " move back to warehouse");
                //被强化物品原来在仓库，需要挪回去仓库里
                DateFile.instance.ChangeTwoActorItem(DateFile.instance.MianActorID(), -999, __state, 1, -1, 0, 0);
            }
        }

    }
    /// <summary>
    ///  如果需要，销毁使用的强化材料，减少制造工具hp
    /// </summary>
    [HarmonyPatch(typeof(MakeSystem), "StartChangeItem")]
    public static class MakeSystem_StartChangeItem_Patch
    {
        //检测是否有道具
        private static bool Prefix(MakeSystem __instance, int ___mianItemId, int ___secondItemId, int[] ___thirdItemId, ref int __state)
        {

            __state = -1;
            int actorId = DateFile.instance.MianActorID();

            //主工具
            if (!DateFile.instance.actorItemsDate[actorId].ContainsKey(___mianItemId))
            {
                //仅当hp为1时，移除mianitem, 其余情况不扣hp，否则会扣两次
                if (int.Parse(DateFile.instance.GetItemDate(___mianItemId, 901)) == 1)
                {
                    DateFile.instance.ChangeItemHp(-999, ___mianItemId, -1, 0, true);
                }

                // Main.Logger.Log(" StartChangeItem use warehouse main item : ");
            }


            //材料
            for (int j = 0; j < ___thirdItemId.Length; j++)
            {
                if (___thirdItemId[j] > 0 && !DateFile.instance.actorItemsDate[actorId].ContainsKey(___thirdItemId[j]))
                {
                    DateFile.instance.LoseItem(-999, ___thirdItemId[j], 1, true, true);
                    //Main.Logger.Log(" poison use warehouse third item ,remove it");
                }
            }

            //被强化物品
            //被强化物品装备了
            if (UseStorangeMaterialHelper.isActorEquip(actorId, ___secondItemId) == true)
            {
                //啥也不干
            }
            else if (!DateFile.instance.actorItemsDate[actorId].ContainsKey(___secondItemId))
            {
                //被强化物品在仓库，先挪 到背包里
                __state = ___secondItemId;
                DateFile.instance.ChangeTwoActorItem(-999, DateFile.instance.MianActorID(), ___secondItemId, 1, -1, 0, 0);
                //Main.Logger.Log(" 强化 use warehouse second item : " + ___secondItemId + " move to bag");
            }
            return true;
        }

        //把移动到背包的物品，如果有需要，移动回去
        private static void Postfix(int __state)
        {
            //被强化物品
            int actorId = DateFile.instance.MianActorID();
            if (__state > 0 && DateFile.instance.actorItemsDate[actorId].ContainsKey(__state))
            {
                //Main.Logger.Log(" 强化 use warehouse second item : " + __state + " move back to warehouse");
                //被强化物品原来在仓库，需要挪回去仓库里
                DateFile.instance.ChangeTwoActorItem(DateFile.instance.MianActorID(), -999, __state, 1, -1, 0, 0);
            }
        }
    }

}