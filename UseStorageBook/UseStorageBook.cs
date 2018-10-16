using Harmony12;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace UseStorageBook
{
    [HarmonyPatch(typeof(HomeSystem), "SetBook")]
    public static class HomeSystem_SetBook_Patch
    {
        /// <summary>
        /// 将HomeSystem.SetBook 修改为 NewSetBook
        /// </summary>
        /// <param name="instructions"></param>
        /// <returns></returns>
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            if (!Main.enabled)
            {
                return instructions;
            }
            Main.logger.Log("start to patch SetBook");
            List<CodeInstruction> list = new List<CodeInstruction>(instructions);
            list.InsertRange(0, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Call, typeof(HomeSystem_SetBook_Patch).GetMethod("NewSetBook", BindingFlags.Public | BindingFlags.Static)),
                new CodeInstruction(OpCodes.Ret),
            });
            Main.logger.Log("success!");
            return list;
        }

        /// <summary>
        /// 在加载主角背包中的书同时加载仓库中的书
        /// </summary>
        public static void NewSetBook()
        {
            var RemoveBook = typeof(HomeSystem).GetMethod("RemoveBook", BindingFlags.NonPublic | BindingFlags.Instance);
            RemoveBook.Invoke(HomeSystem.instance, null);
            int key = DateFile.instance.MianActorID();
            List<int> list = new List<int>(ActorMenu.instance.GetActorItems(key, 0).Keys);
            list.AddRange(ActorMenu.instance.GetActorItems(-999, 0).Keys);
            list = DateFile.instance.GetItemSort(list);
            for (int i = 0; i < list.Count; i++)
            {
                int num = list[i];
                if (int.Parse(DateFile.instance.GetItemDate(num, 31, true)) == HomeSystem.instance.studySkillTyp)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(HomeSystem.instance.bookIcon, Vector3.zero, Quaternion.identity);
                    gameObject.name = "Item," + num;
                    gameObject.transform.SetParent(HomeSystem.instance.bookHolder, false);
                    gameObject.GetComponent<Toggle>().group = HomeSystem.instance.bookHolder.GetComponent<ToggleGroup>();
                    Image component = gameObject.transform.Find("ItemBack").GetComponent<Image>();
                    component.sprite = GetSprites.instance.itemBackSprites[int.Parse(DateFile.instance.GetItemDate(num, 4, true))];
                    component.color = ActorMenu.instance.LevelColor(int.Parse(DateFile.instance.GetItemDate(num, 8, true)));
                    GameObject gameObject2 = gameObject.transform.Find("ItemIcon").gameObject;
                    gameObject2.name = "ItemIcon," + num;
                    gameObject2.GetComponent<Image>().sprite = GetSprites.instance.itemSprites[int.Parse(DateFile.instance.GetItemDate(num, 98, true))];
                    int num2 = int.Parse(DateFile.instance.GetItemDate(num, 901, true));
                    int num3 = int.Parse(DateFile.instance.GetItemDate(num, 902, true));
                    gameObject.transform.Find("ItemHpText").GetComponent<Text>().text = string.Format("{0}{1}</color>/{2}", ActorMenu.instance.Color3(num2, num3), num2, num3);
                    int[] bookPage = DateFile.instance.GetBookPage(num);
                    Transform transform = gameObject.transform.Find("PageBack");
                    for (int j = 0; j < transform.childCount; j++)
                    {
                        if (bookPage[j] == 1)
                        {
                            transform.GetChild(j).GetComponent<Image>().color = new Color(0.392156869f, 0.784313738f, 0f, 1f);
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ReadBook), "CloseReadBook")]
    public static class ReadBook_CloseReadBook_Patch
    {
        /// <summary>
        /// 仓库中的书耐久为0时将其移除
        /// </summary>
        static void Prefix()
        {
            var df = DateFile.instance;
            var bookId = HomeSystem.instance.readBookId;
            if (df.itemsDate.ContainsKey(bookId))
            {
                if (df.actorItemsDate[-999].ContainsKey(bookId))
                {
                    var hp = int.Parse(df.itemsDate[bookId][901]);
                    if (hp <= 1)
                    {
                        df.actorItemsDate[-999].Remove(bookId);
                        df.actorItemsDate[df.mianActorId].Add(bookId, 1);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 解决鼠标放在书本上不显示仓库中书上时，不显示仓库中书的阅读状态的BUG
    /// （暂时将书加入背包）
    /// </summary>
    [HarmonyPatch(typeof(WindowManage), "ShowBookMassage")]
    public static class WindowsManage_ShowBookMassage_Patch
    {
        /// <summary>
        /// 记录当前书的id，并将仓库中的书暂时加入背包
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="__state"></param>
        static void Prefix(ref int itemId, ref int __state)
        {
            if (DateFile.instance.actorItemsDate[-999].ContainsKey(itemId))
            {
                DateFile.instance.actorItemsDate[DateFile.instance.MianActorID()].Add(itemId, 1);
                __state = itemId;
            }
            else
            {
                __state = -1;
            }
        }

        /// <summary>
        /// 将书从背包中移除
        /// </summary>
        /// <param name="__state"></param>
        static void Postfix(ref int __state)
        {
            if (__state > 0)
            {
                DateFile.instance.actorItemsDate[DateFile.instance.MianActorID()].Remove(__state);
            }
        }
    }

    public class Main
    {
        public static UnityModManager.ModEntry.ModLogger logger;

        public static bool enabled;

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return value;
        }

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            modEntry.OnToggle = new Func<UnityModManager.ModEntry, bool, bool>(OnToggle);
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }
    }
}
