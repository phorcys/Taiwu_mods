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
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace InventorySort
{

    public class Settings : UnityModManager.ModSettings
    {
        public int mainsort_category = 0;
        public int subsort_category = 1;
        [XmlIgnore]
        public UnityModManager.ModEntry modee;


        public int getmaincate()
        {
            if(mainsort_category <0 || mainsort_category >=ALLCATE.Length)
            {
                mainsort_category = 0;
            }
            return ALLCATE[mainsort_category].Key;
        }

        public int getsubcate()
        {
            if (subsort_category < 0 || subsort_category >= ALLCATE.Length)
            {
                subsort_category = 1;
            }
            return ALLCATE[subsort_category].Key;
        }

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public static KeyValuePair<int, string>[] ALLCATE = new KeyValuePair<int, string>[]
        {
            new KeyValuePair<int, string>( 4, "主分类" ),
            new KeyValuePair<int, string>(  98, "次分类" ),
            new KeyValuePair<int, string>(   8, "品质颜色"  ),
            new KeyValuePair<int, string>(  904, "物品价值" ),
        };



        //Output the new state of the Toggle into Text
        public void ToggleValueChanged(Toggle change)
        {
            subsort_category = int.Parse(change.name.Substring(change.name.Length-1));
            //Main.Logger.Log(" subcate changed :" + subsort_category);
            Save(modee);
        }

        //Output the new state of the Toggle into Text
        public void MainToggleValueChanged(Toggle change)
        {
            mainsort_category = int.Parse(change.name.Substring(change.name.Length - 1));
            //Main.Logger.Log(" maincate changed :" + mainsort_category);
            Save(modee);
        }

        internal void loadSystemsetting(UnityModManager.ModEntry mode)
        {
            modee = mode;
        }
    }


    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;



            settings = Settings.Load<Settings>(modEntry);
            settings.loadSystemsetting(modEntry);

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());



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
    ///  设置界面新增 Toggle
    /// </summary>
    [HarmonyPatch(typeof(SystemSetting), "Start")]
    public static class SystemSetting_Start_Patch
    {

        private static void Postfix(SystemSetting __instance, Toggle[] ___itemSortToggle)
        {
            if (!Main.enabled)
            {
                return;
            }

            var mg1 = GameObject.Instantiate(___itemSortToggle[2].gameObject) as GameObject;
            var cmg1 = mg1.GetComponent<Toggle>();
            cmg1.name = "sort_toggle_3";
            cmg1.isOn = Main.settings.mainsort_category == 3;
            cmg1.group = ___itemSortToggle[2].group;

            ___itemSortToggle[0].onValueChanged.RemoveAllListeners();
            ___itemSortToggle[1].onValueChanged.RemoveAllListeners();
            ___itemSortToggle[2].onValueChanged.RemoveAllListeners();
            cmg1.onValueChanged.RemoveAllListeners();
            ___itemSortToggle[0].onValueChanged.AddListener(delegate { Main.settings.MainToggleValueChanged(___itemSortToggle[0]); });
            ___itemSortToggle[1].onValueChanged.AddListener(delegate { Main.settings.MainToggleValueChanged(___itemSortToggle[1]); });
            ___itemSortToggle[2].onValueChanged.AddListener(delegate { Main.settings.MainToggleValueChanged(___itemSortToggle[2]); });
            cmg1.onValueChanged.AddListener(delegate { Main.settings.MainToggleValueChanged(cmg1); });

            mg1.transform.SetParent(___itemSortToggle[2].transform.parent);
            mg1.transform.position = ___itemSortToggle[2].transform.position + new Vector3(2.3f, 0.0f);

            var tg3 = GameObject.Instantiate(___itemSortToggle[0].gameObject) as GameObject;
            var tg4 = GameObject.Instantiate(___itemSortToggle[1].gameObject) as GameObject;
            var tg5 = GameObject.Instantiate(___itemSortToggle[2].gameObject) as GameObject;
            var tg6 = GameObject.Instantiate(mg1) as GameObject;

            var tgg = tg3.AddComponent<ToggleGroup>();

            var ctg3 = tg3.GetComponent<Toggle>();
            var ctg4 = tg4.GetComponent<Toggle>();
            var ctg5 = tg5.GetComponent<Toggle>();
            var ctg6 = tg6.GetComponent<Toggle>();

            ___itemSortToggle[0].name = "sort_toggle_0";
            ___itemSortToggle[1].name = "sort_toggle_1";
            ___itemSortToggle[2].name = "sort_toggle_2";
            

            ctg3.name = "subsort_toggle_0";
            ctg4.name = "subsort_toggle_1";
            ctg5.name = "subsort_toggle_2";
            ctg6.name = "subsort_toggle_3";

            ctg3.isOn = false;
            ctg4.isOn = false;
            ctg5.isOn = false;
            ctg6.isOn = false;
            switch (Main.settings.subsort_category)
            {
                case 0:
                    ctg3.isOn = true;
                    break;
                case 1:
                    ctg4.isOn = true;
                    break;
                case 2:
                    ctg5.isOn = true;
                    break;
                case 3:
                    ctg6.isOn = true;
                    break;
                default:
                    break;
            }

            ctg3.onValueChanged.RemoveAllListeners();
            ctg4.onValueChanged.RemoveAllListeners();
            ctg5.onValueChanged.RemoveAllListeners();
            ctg6.onValueChanged.RemoveAllListeners();

            ctg3.onValueChanged.AddListener(delegate { Main.settings.ToggleValueChanged(ctg3); });
            ctg4.onValueChanged.AddListener(delegate { Main.settings.ToggleValueChanged(ctg4); });
            ctg5.onValueChanged.AddListener(delegate { Main.settings.ToggleValueChanged(ctg5); });
            ctg6.onValueChanged.AddListener(delegate { Main.settings.ToggleValueChanged(ctg6); });


            ctg3.group = tgg;
            ctg4.group = tgg;
            ctg5.group = tgg;
            ctg6.group = tgg;

            //Main.Logger.Log(String.Format(" add toggle {0}  image {1}  text {2}", ___itemSortToggle[0].transform.position , ___itemSortToggle[0].image, ___itemSortToggle[0].GetComponentInChildren<Text>().text));
            //Main.Logger.Log(String.Format(" add toggle {0}  image {1}  text {2}", ___itemSortToggle[1].transform.position, ___itemSortToggle[1].image, ___itemSortToggle[1].GetComponentInChildren<Text>().text));
            //Main.Logger.Log(String.Format(" add toggle {0}  image {1}  text {2}", ___itemSortToggle[2].transform.position, ___itemSortToggle[2].image, ___itemSortToggle[2].GetComponentInChildren<Text>().text));
            //Main.Logger.Log(String.Format(" add toggle {0}  image {1}  text {2}", mg1.transform.position, cmg1.image, cmg1.GetComponentInChildren<Text>().text));

            tg3.transform.SetParent (___itemSortToggle[0].transform.parent);
            tg4.transform.SetParent(___itemSortToggle[1].transform.parent);
            tg5.transform.SetParent(___itemSortToggle[2].transform.parent);
            tg6.transform.SetParent(___itemSortToggle[2].transform.parent);

            tg3.transform.position = ___itemSortToggle[0].transform.position + new Vector3(0,-1.0f);
            tg4.transform.position = ___itemSortToggle[1].transform.position + new Vector3(0, -1.0f);
            tg5.transform.position = ___itemSortToggle[2].transform.position + new Vector3(0, -1.0f);
            tg6.transform.position = cmg1.transform.position + new Vector3(0, -1.0f);

            ___itemSortToggle[0].GetComponentInChildren<Text>().text = ctg3.GetComponentInChildren<Text>().text = Settings.ALLCATE[0].Value;
            ___itemSortToggle[1].GetComponentInChildren<Text>().text = ctg4.GetComponentInChildren<Text>().text = Settings.ALLCATE[1].Value;
            ___itemSortToggle[2].GetComponentInChildren<Text>().text = ctg5.GetComponentInChildren<Text>().text = Settings.ALLCATE[2].Value;
            cmg1.GetComponentInChildren<Text>().text = ctg6.GetComponentInChildren<Text>().text = Settings.ALLCATE[3].Value;



            GameObject par = mg1.transform.parent.gameObject;

            for (int i = 0; i < par.transform.childCount; i++)
            {
                var go = par.transform.GetChild(i).gameObject;
                var text = go.GetComponentInChildren<Text>();
                if (text != null && text.text == "道具排序")
                {

                    text.text = "主排序";
                    var go2 = GameObject.Instantiate(go) as GameObject;
                    var ct2 = go2.GetComponentInChildren<Text>();
                    ct2.text = "次排序";
                    go2.transform.SetParent(___itemSortToggle[0].transform.parent);
                    go2.transform.localScale = go.transform.localScale;
                    go2.transform.position = go.transform.position + new Vector3(0.0f, -1.0f);
                }

            }
            return;
        }

    }


    /// <summary>
    ///  打开设置时，根据主副排序设置更新toggle
    /// </summary>
    [HarmonyPatch(typeof(SystemSetting), "ShowSystemSettingWindow")]
    public static class SystemSetting_ShowSystemSettingWindow_Patch
    {

        private static void Postfix(SystemSetting __instance)
        {
            if (!Main.enabled)
            {
                return;
            }
            Transform obj = null;
            switch(Main.settings.mainsort_category)
            {
                case 0:
                    __instance.itemSortToggle[0].isOn = true;
                    break;
                case 1:
                    __instance.itemSortToggle[1].isOn = true;
                    break;
                case 2:
                    __instance.itemSortToggle[2].isOn = true;
                    break;
                case 3:
                    obj =__instance.transform.Find("sort_toggle_3");
                    if(obj != null)
                    {
                        obj.gameObject.GetComponentInChildren<Toggle>().isOn = true;
                    }
                    break;
                default:
                    break;
            }



            return;
        }

    }

    /// <summary>
    ///  注入排序方法 
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "GetItemSort")]
    public static class DateFile_GetItemSort_Patch
    {

        static public List<int> new_Sort(List<int>  itemlist)
        {

            return itemlist.OrderBy(o => (
                                           int.Parse(DateFile.instance.GetItemDate(o, Main.settings.getmaincate(), true)) * 10000
                                           + int.Parse(DateFile.instance.GetItemDate(o, Main.settings.getsubcate(), true))
                                      )).ToList();

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
                injectedCodes.Add(new CodeInstruction(OpCodes.Ldarg_1));
                injectedCodes.Add(new CodeInstruction(OpCodes.Call, typeof(DateFile_GetItemSort_Patch).GetMethod("new_Sort")));
                injectedCodes.Add(new CodeInstruction(OpCodes.Ret));

                codes.InsertRange(startIndex, injectedCodes);
            }
            else
            {
                Main.Logger.Log(" game changed ... this mod failed to find code to patch...");
            }

            //Main.Logger.Log(" dump the patch codes ");

            //for (int i = 0; i < codes.Count; i++)
            //{
            //    Main.Logger.Log(String.Format("{0} : {1}  {2}", i, codes[i].opcode, codes[i].operand));
            //}
            return codes.AsEnumerable();
        }
    }


    /// <summary>
    ///  商店 获取数据接口拦截，提前修改商店 this.sellItems 排序
    /// </summary>
    [HarmonyPatch(typeof(ShopSystem), "SetItems")]
    public static class ShopSystem_SetItems_Patch
    {

        private static void Prefix(ShopSystem __instance, bool actor, int typ, ref Dictionary<int, int> ___shopItems)
        {
            if (!Main.enabled)
            {
                return;
            }

            ___shopItems = ___shopItems.OrderBy(o =>
            (int.Parse(DateFile.instance.GetItemDate(o.Key, Main.settings.getmaincate(), true)) * 10000
                + int.Parse(DateFile.instance.GetItemDate(o.Key, Main.settings.getsubcate(), true)))
            ).ToDictionary((KeyValuePair<int, int> o) => o.Key, (KeyValuePair<int, int> p) => p.Value);

            return;
        }

    }
}