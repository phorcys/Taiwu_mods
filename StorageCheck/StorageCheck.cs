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
using System.Linq;
using System.Reflection.Emit;

namespace StorageCheck
{

    public class Settings : UnityModManager.ModSettings
    {
        public bool displaybag = true;
        public bool displaywarehouse = true;
        public bool displaybookinfo = true;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

    }


    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            Logger = modEntry.Logger;
            
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

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

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginVertical("Box");
            settings.displaybag = GUILayout.Toggle(settings.displaybag, "是否显示 背包内是否有此物品 ");
            settings.displaywarehouse = GUILayout.Toggle(settings.displaywarehouse, "是否显示 仓库内是否有此物品 ");
            settings.displaybookinfo = GUILayout.Toggle(settings.displaybookinfo, "是否显示 功法书籍比对信息 ");

            GUILayout.Label("说明： 点击是否启用对应功能。");
            GUILayout.EndVertical();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

    }


    /// <summary>
    ///  物品tips 显示，查询是否需要插入 背包/仓库是否有此物品显示
    /// </summary>
    [HarmonyPatch(typeof(WindowManage), "ShowItemMassage")]
    public static class WindowManage_ShowItemMassage_Patch
    {

        public static  int localGetItemNumber(int actorId, int itemId)
        {
            int num = 0;
            int result;
            if (DateFile.instance.actorItemsDate.ContainsKey(actorId) && DateFile.instance.actorItemsDate[actorId].ContainsKey(itemId))
            {
                result = ((int.Parse(DateFile.instance.GetItemDate(itemId, 6, true)) != 0) ? DateFile.instance.actorItemsDate[actorId][itemId] : 1);
            }
            else
            {
                result = num;
            }
            return result;
        }

        private static bool getItemNumber(int actorid, int item_id, ref int num, ref int usenum, ref int totalcount)
        {
            num = 0;
            usenum = 0;

            num = localGetItemNumber(actorid, item_id);

            int itemtypeid = int.Parse(DateFile.instance.GetItemDate(item_id, 999, true));
            if (num > 0)
            {
                usenum = int.Parse(DateFile.instance.itemsDate.ContainsKey(item_id) ? DateFile.instance.itemsDate[item_id][901] : DateFile.instance.GetItemDate(item_id, 902, true));
                totalcount = int.Parse(DateFile.instance.itemsDate.ContainsKey(item_id) ? DateFile.instance.itemsDate[item_id][902] : DateFile.instance.GetItemDate(item_id, 902, true));
            }


            if (itemtypeid >0)
            {
                foreach (int key in DateFile.instance.actorItemsDate[actorid].Keys)
                {
                    if(DateFile.instance.GetItemDate(key, 999, true) == itemtypeid.ToString())
                    {
                        num = num + 1;
                        usenum = usenum + int.Parse(DateFile.instance.itemsDate.ContainsKey(key)? DateFile.instance.itemsDate[key][901]: DateFile.instance.GetItemDate(key, 902, true));
                        totalcount = totalcount + int.Parse(DateFile.instance.itemsDate.ContainsKey(key) ? DateFile.instance.itemsDate[key][902] : DateFile.instance.GetItemDate(key, 902, true));
                    }
                }
                if(num >1)
                {
                    num = num - 1;
                }
            }
            return usenum>0;
         }


        private static void Postfix(WindowManage __instance, int itemId, ref string ___baseWeaponMassage, ref Text ___informationMassage)
        {
            if (!Main.enabled)
            {
                return;
            }
            string text = ___baseWeaponMassage;
            if (Main.settings.displaybag)
            {
                int num = 0;int usecount = 0;int totalcount = 0;
                bool canstack = getItemNumber(DateFile.instance.MianActorID(), itemId,ref num , ref usecount, ref totalcount);
                if(!canstack)
                {
                    text += DateFile.instance.SetColoer(20008, string.Format("\n 背包数量: {0} ", num));
                }
                else
                {
                    text += DateFile.instance.SetColoer(20008, string.Format("\n 背包数量: {0}  总耐久: {1}/{2}", num, usecount, totalcount));
                }
                
            }
            if (Main.settings.displaywarehouse )
            {
                int num = 0; int usecount = 0;int totalcount = 0;
                bool canstack = getItemNumber(-999, itemId, ref num, ref usecount, ref totalcount);
                if (!canstack)
                {
                    text += DateFile.instance.SetColoer(20008, string.Format("\n 仓库数量: {0} ", num));
                }
                else
                {
                    text += DateFile.instance.SetColoer(20008, string.Format("\n 仓库数量: {0}  总耐久: {1}/{2}", num, usecount, totalcount));
                }
            }

            ___baseWeaponMassage = text;
            ___informationMassage.text = text;

            return;
        }
    }


    /// <summary>
    ///  物品tips 显示，查询是否需要插入 背包/仓库是否有此物品显示
    /// </summary>
    [HarmonyPatch(typeof(WindowManage), "ShowBookMassage")]
    public static class WindowManage_ShowBookMassage_Patch
    {

        public static string getBookInfo(WindowManage __instance, int itemId)
        {
            bool flag = false;
            if (ShopSystem.instance.shopWindow.activeInHierarchy
                || BookShopSystem.instance.shopWindow.activeInHierarchy
                || Warehouse.instance.warehouseWindow.activeInHierarchy
                || (ActorMenu.instance.actorMenu.activeInHierarchy && !ActorMenu.instance.isEnemy)
                || DateFile.instance.actorItemsDate[DateFile.instance.MianActorID()].ContainsKey(itemId))
            {
                flag = true;
            }
            string text = "";
            if (!flag)
            {
                text = string.Format("{0}{1}{2}\n\n", __instance.SetMassageTitle(8007, 0, 12, 10002)
                    , DateFile.instance.massageDate[10][2]
                    , DateFile.instance.massageDate[8006][4]);
            }
            else
            {


                //页信息 索引
                int pagedataindex = int.Parse(DateFile.instance.GetItemDate(itemId, 32, true));
                //技能类别
                int skillcate = int.Parse(DateFile.instance.GetItemDate(itemId, 31, true));
                int[] playerlearned = (skillcate != 17) ?
                    ((!DateFile.instance.skillBookPages.ContainsKey(pagedataindex)) ? new int[10] : DateFile.instance.skillBookPages[pagedataindex])
                    : ((!DateFile.instance.gongFaBookPages.ContainsKey(pagedataindex)) ? new int[10] : DateFile.instance.gongFaBookPages[pagedataindex]);

                int[] curbookPage = DateFile.instance.GetBookPage(itemId);
                int itemtypeid = int.Parse(DateFile.instance.GetItemDate(itemId, 999, true));
                //每页可读共计有几页
                int[] bagpages = new int[10];
                foreach (int actorid in new int[] { DateFile.instance.MianActorID(), -999 })
                {
                    foreach (int key in DateFile.instance.actorItemsDate[actorid].Keys)
                    {
                        if (DateFile.instance.GetItemDate(key, 999, true) == itemtypeid.ToString() && key != itemId)
                        {
                            var pages = DateFile.instance.GetBookPage(key);
                            for (int z = 0; z < 10; z++)
                            {
                                if (pages[z] != 0)
                                {
                                    bagpages[z]++;
                                }
                            }
                        }
                    }
                }
                if (bagpages.Sum() > 0)
                {
                    text += "背包功法页已读统计:\n";
                    for (int i = 0; i < playerlearned.Length; i++)
                    {
                        text += string.Format("{0}{1}{2}{3}{4}", new object[]
                        {
                        DateFile.instance.massageDate[10][2],
                        DateFile.instance.massageDate[8][2].Split('|')[i],
                        (curbookPage[i] != 1) ? DateFile.instance.SetColoer(20010, DateFile.instance.massageDate[7010][4].Split('|')[0], false)
                                                  : DateFile.instance.SetColoer(20004, DateFile.instance.massageDate[7010][4].Split('|')[1], false),
                        (playerlearned[i] != 1) ? DateFile.instance.SetColoer(20002, string.Format("  ({0})",
                                                          DateFile.instance.massageDate[7009][4].Split('|')[2]), false)
                                                   : DateFile.instance.SetColoer(20005, string.Format("  ({0})",
                                                          DateFile.instance.massageDate[7009][4].Split('|')[3]), false),
                        (bagpages[i] >= 1) ? DateFile.instance.SetColoer(20004, string.Format("  ○已有{0}页\n",bagpages[i]))
                                                   : DateFile.instance.SetColoer(20010, string.Format("  ×无此页)\n"))
                        });
                    }
                    text += "\n";

                }
                if (skillcate == 17)
                {
                    int num2 = int.Parse(DateFile.instance.gongFaDate[pagedataindex][103 + int.Parse(DateFile.instance.GetItemDate(itemId, 35, true))]);
                    if (num2 > 0)
                    {
                        text += string.Format("{0}{1}{2}\n\n", __instance.SetMassageTitle(8007, 0, 14, 10002), DateFile.instance.massageDate[10][2], DateFile.instance.SetColoer(20002, string.Format("{0}{1}{2}{3}{4}", new object[]
                        {
                    DateFile.instance.massageDate[8006][5].Split(new char[]
                    {
                        '|'
                    })[0],
                    DateFile.instance.SetColoer(20001 + int.Parse(DateFile.instance.gongFaDate[pagedataindex][2]), DateFile.instance.gongFaDate[pagedataindex][0], false),
                    DateFile.instance.massageDate[8006][5].Split(new char[]
                    {
                        '|'
                    })[1],
                    DateFile.instance.gongFaFPowerDate[num2][99],
                    DateFile.instance.massageDate[5001][5]
                        }), false));
                    }
                }
            }
            return text;
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Main.Logger.Log(" Transpiler init codes ");
            var codes = new List<CodeInstruction>(instructions);

            var foundtheforend = true;
            int startIndex = 0;


            if (foundtheforend)
            {
                var injectedCodes = new List<CodeInstruction>();

                // 注入 IL code 
                //
                injectedCodes.Add(new CodeInstruction(OpCodes.Ldsfld, typeof(Main).GetField("enabled")));
                injectedCodes.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
                injectedCodes.Add(new CodeInstruction(OpCodes.Beq_S,7));
                injectedCodes.Add(new CodeInstruction(OpCodes.Ldarg_0));
                injectedCodes.Add(new CodeInstruction(OpCodes.Ldarg_1));
                injectedCodes.Add(new CodeInstruction(OpCodes.Call, typeof(WindowManage_ShowBookMassage_Patch).GetMethod("getBookInfo")));
                injectedCodes.Add(new CodeInstruction(OpCodes.Ret));

                codes.InsertRange(startIndex, injectedCodes);
            }
            else
            {
                Main.Logger.Log(" game changed ... this mod failed to find code to patch...");
            }

            Main.Logger.Log(" dump the patch codes ");

            for (int i = 0; i < codes.Count; i++)
            {
                Main.Logger.Log(String.Format("{0} : {1}  {2}", i, codes[i].opcode, codes[i].operand));
            }
            return codes.AsEnumerable();
        }
    }
}