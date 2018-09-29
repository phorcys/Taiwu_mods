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


    /// <summary>
    ///  制造界面列表显示仓库物品，包括工具和引子材料，包括制造/修理/强化/淬毒界面
    ///  因为是直接修改函数，所以不能禁用
    /// </summary>

    [HarmonyPatch(typeof(MakeSystem), "SetItem")]
    public static class MakeSystem_SetItem_Patch
    {

        static IEnumerable<CodeInstruction> Transpiler(MethodBase methodBase, IEnumerable<CodeInstruction> instructions, ILGenerator  generator )
        {
            Main.Logger.Log(" Transpiler init codes ");
            var codes = new List<CodeInstruction>(instructions);

            var foundtheforend = false;
            int startIndex = -1;

            //寻找注入点
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Newobj && codes[i-1].opcode == OpCodes.Callvirt && codes[i + 1].opcode == OpCodes.Stloc_1)
                {
                    startIndex = i+2;
                    foundtheforend = true;
                    Main.Logger.Log(" found the end of the new List<int> , at index: " + i);
                }

            }

            if (foundtheforend)
            {
                var injectedCodes = new List<CodeInstruction>();

                // 注入 IL code  等效代码  list.AddRange(ActorMenu.instance.GetActorItems(-999, 0).Keys);
                // 
                injectedCodes.Add(new CodeInstruction(OpCodes.Ldloc_1 ));
                injectedCodes.Add(new CodeInstruction(OpCodes.Ldsfld, typeof(ActorMenu).GetField("instance")));
                injectedCodes.Add(new CodeInstruction(OpCodes.Ldc_I4, -999));
                injectedCodes.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
                injectedCodes.Add(new CodeInstruction(OpCodes.Callvirt, typeof(ActorMenu).GetMethod("GetActorItems")));
                injectedCodes.Add(new CodeInstruction(OpCodes.Callvirt, typeof(Dictionary<int, int>).GetMethod("get_Keys")));
                injectedCodes.Add(new CodeInstruction(OpCodes.Callvirt, typeof(List<int>).GetMethod("AddRange")));

                codes.InsertRange(startIndex, injectedCodes);
            }
            else
            {
                Main.Logger.Log(" game changed ... this mod failed to find code to patch...");
            }

            //Main.Logger.Log(" dump the patch codes ");

            //for (int i = 0; i < codes.Count ; i++)
            //{
            //    Main.Logger.Log(String.Format("{0} : {1}  {2}",i ,codes[i].opcode, codes[i].operand ));
            //}
            return codes.AsEnumerable();
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

            if ( (__state & 1) == 1)
            {
                DateFile.instance.ChangeItemHp(-999, ___mianItemId, -1, 0, true);
            }
            if ((__state & 2) == 2)
            {
                DateFile.instance.LoseItem(-999, ___secondItemId, 1, true);
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
        private static void Prefix(MakeSystem __instance, int ___mianItemId, int[] ___thirdItemId)
        {
            //主工具
            int actorId = DateFile.instance.MianActorID();
            if (!DateFile.instance.actorItemsDate[actorId].ContainsKey(___mianItemId))
            {
                DateFile.instance.ChangeItemHp(-999, ___mianItemId, -1, 0, true);
                //Main.Logger.Log(" poison use warehouse main item : ");
            }


            //毒液
            for (int j = 0; j < ___thirdItemId.Length; j++)
            {
                if (___thirdItemId[j] > 0 && !DateFile.instance.actorItemsDate[actorId].ContainsKey(___thirdItemId[j]))
                {
                    if (int.Parse(DateFile.instance.GetItemDate(___thirdItemId[j], 6, true)) > 0)
                    {
                        DateFile.instance.LoseItem(-999, ___thirdItemId[j], 1, true);
                        //Main.Logger.Log(" poison use warehouse third item ,remove it");
                    }
                    else
                    {
                        DateFile.instance.ChangeItemHp(-999, ___thirdItemId[j], -1, 0, true);
                       // Main.Logger.Log(" poison use warehouse third item , dec hp ");
                    }
                    
                }
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
        private static void Prefix(MakeSystem __instance, int ___secondItemId, int[] ___thirdItemId)
        {

            int actorId = DateFile.instance.MianActorID();

            //主工具
            if (!DateFile.instance.actorItemsDate[actorId].ContainsKey(___secondItemId))
            {
                DateFile.instance.ChangeItemHp(-999, ___secondItemId, -1, 0, true);
               // Main.Logger.Log(" poison use warehouse main item : ");
            }


            //材料
            for (int j = 0; j < ___thirdItemId.Length; j++)
            {
                if (___thirdItemId[j] > 0 && !DateFile.instance.actorItemsDate[actorId].ContainsKey(___thirdItemId[j]))
                {
                    DateFile.instance.LoseItem(-999, ___thirdItemId[j], 1, true);
                    //Main.Logger.Log(" poison use warehouse third item ,remove it");
                }
            }

        }

    }

}