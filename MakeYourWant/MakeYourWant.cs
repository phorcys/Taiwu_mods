using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace MakeYourWant
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        public bool EffectAll = false;
        //武器
        public bool SetWeapon = false;
        public bool WeaponMoreDuration = false;
        public bool AttackTypExtend = false;
        public int[] AttackTyp = new int[6] {15,15,15,15,15,15};//式
        public int WeaponPower0 = 3;//词条
        public int WeaponPower1 = 0;
        public int WeaponPower2 = 0;
        public int WeaponPower3 = 5;
        public bool WeaponPowerMustBe = false;//必出
        public int WeaponEngraving = 0;//功法发挥
	    public bool WeaponEngravingMustBe = false;
        //防具
        public bool SetArmor = false;
        public int ArmorPower = 0;
        public bool ArmorPowerMustBe = false;
        public int ArmorAttr = 0;
        public bool ArmorAttrMustBe = false;
        //饰品
        public bool SetJewelry = false;
        public bool JewelryPowerMustBe = false;
        public int JewelryAttr0 = 0;
        public int JewelryAttr1 = 0;
        public int JewelryAttr2 = 0;
        public bool JewelryAttrMustBe = false;
    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static readonly string[] AttackTypName = { "掷", "弹", "御", "劈", "刺", "撩",
            "崩", "点", "拿", "音", "缠", "咒", "机", "药", "毒", "？" };
        public static readonly string[] WeaponPowerName0 = { "破", "辟", "杀", "其他" };
        public static readonly string[] WeaponPowerName1 = { "掌", "剑", "刀", "毒", "长鞭", "软兵", "暗器",
            "奇门", "魔音", "金", "木", "玉" };
        public static readonly string[] WeaponPowerName2 = { "武器杀", "材质杀" };
        public static readonly string[] WeaponPowerName3 = { "厚重", "轻盈", "锋锐", "钝拙", "贵重", "随机" };
        public static readonly string[] WeaponEngravingName = { "随机", "力道", "精妙", "迅疾" };
        public static readonly string[] ArmorPowerName = { "随机", "不变", "厚重", "轻盈", "藏锋", "百折", "贵重" };
        public static readonly string[] ArmorAttrName = { "随机", "膂力", "体质", "灵敏", "根骨", "悟性", "定力" };
        public static readonly string[] JewelryAttrName0 = { "随机", "技艺", "武学" };
        public static readonly string[] JewelryAttrName1 = { "音律", "弈棋", "诗书", "绘画",
            "术数", "品鉴", "锻造", "制木", "医术", "毒术", "织锦", "巧匠", "道法", "佛学", "厨艺", "杂学" };
        public static readonly string[] JewelryAttrName2 = { "内功", "身法", "绝技", "拳掌",
            "指法", "腿法", "暗器", "剑法", "刀法", "长兵", "奇门", "软兵", "御射", "乐器" };


        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("<color=#F28234FF>跨月制作的装备在投入材料时决定</color>", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            settings.EffectAll = GUILayout.Toggle(settings.EffectAll, "影响所有新产生的装备，包括商店出售的与NPC获得的（不建议）", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal("Box");
            settings.SetWeapon = GUILayout.Toggle(settings.SetWeapon, "定制武器", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            if (settings.SetWeapon)
            {
                GUILayout.BeginHorizontal();
	            GUILayout.Label("式排列", GUILayout.Width(100));
                string[] s = new string[6];
                for (int i = 0; i < 6; i = i + 1)
                {
                    s[i] = AttackTypName[settings.AttackTyp[i]];
                }
                GUILayout.Label(string.Concat("<size=15>",string.Join("|",s),"</size>"), GUILayout.Width(150));
                bool flag = GUILayout.Button((settings.AttackTypExtend ? "收起" : "展开"), GUILayout.Width(100));
                GUILayout.Label("<color=#E4504DFF>不会产生武器没有的式</color>", GUILayout.Width(210));
                //GUILayout.FlexibleSpace();
                settings.WeaponMoreDuration = GUILayout.Toggle(settings.WeaponMoreDuration, "较高耐久", new GUILayoutOption[0]);
                if (flag)
                {
                    settings.AttackTypExtend = !settings.AttackTypExtend;
                }
                GUILayout.EndHorizontal();
                if (settings.AttackTypExtend)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        GUILayout.BeginHorizontal();
                        settings.AttackTyp[i] = GUILayout.SelectionGrid(settings.AttackTyp[i], AttackTypName, 16);
                        GUILayout.EndHorizontal();
                    }
                }
	            GUILayout.BeginHorizontal();
	            GUILayout.Label("词条", GUILayout.Width(100));
                settings.WeaponPower0 = GUILayout.SelectionGrid(settings.WeaponPower0, WeaponPowerName0, 4);
                //GUILayout.FlexibleSpace();
                settings.WeaponPowerMustBe = GUILayout.Toggle(settings.WeaponPowerMustBe, "必出", new GUILayoutOption[0]);
		        GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (settings.WeaponPower0 == 2)
                {
                    settings.WeaponPower2 = GUILayout.SelectionGrid(settings.WeaponPower2, WeaponPowerName2, 2);
                }
                else
                {
                    if (settings.WeaponPower0 == 3)
                    {
                        settings.WeaponPower3 = GUILayout.SelectionGrid(settings.WeaponPower3, WeaponPowerName3, 6);
                    }
                    else
                    {
                        settings.WeaponPower1 = GUILayout.SelectionGrid(settings.WeaponPower1, WeaponPowerName1, 12);
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
	            GUILayout.Label("功法发挥", GUILayout.Width(100));
                settings.WeaponEngraving = GUILayout.SelectionGrid(settings.WeaponEngraving, WeaponEngravingName, 4);
                //GUILayout.FlexibleSpace();
                settings.WeaponEngravingMustBe = GUILayout.Toggle(settings.WeaponEngravingMustBe, "必出", new GUILayoutOption[0]);
	            GUILayout.EndHorizontal();
		    }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal("Box");
            settings.SetArmor = GUILayout.Toggle(settings.SetArmor, "定制防具", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            if (settings.SetArmor)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("词条", GUILayout.Width(100));
                settings.ArmorPower = GUILayout.SelectionGrid(settings.ArmorPower, ArmorPowerName, 7);
                //GUILayout.FlexibleSpace();
                settings.ArmorPowerMustBe = GUILayout.Toggle(settings.ArmorPowerMustBe, "必出", new GUILayoutOption[0]);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("额外属性", GUILayout.Width(100));
                settings.ArmorAttr = GUILayout.SelectionGrid(settings.ArmorAttr, ArmorAttrName, 7);
                //GUILayout.FlexibleSpace();
                settings.ArmorAttrMustBe = GUILayout.Toggle(settings.ArmorAttrMustBe, "必出", new GUILayoutOption[0]);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal("Box");
            settings.SetJewelry = GUILayout.Toggle(settings.SetJewelry, "定制饰品", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            if (settings.SetJewelry)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("词条", GUILayout.Width(100));
                //GUILayout.FlexibleSpace();
                settings.JewelryPowerMustBe = GUILayout.Toggle(settings.JewelryPowerMustBe, "必出贵重", new GUILayoutOption[0]);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("额外属性", GUILayout.Width(100));
                settings.JewelryAttr0 = GUILayout.SelectionGrid(settings.JewelryAttr0, JewelryAttrName0, 3);
                //GUILayout.FlexibleSpace();
                settings.JewelryAttrMustBe = GUILayout.Toggle(settings.JewelryAttrMustBe, "必出", new GUILayoutOption[0]);
                GUILayout.EndHorizontal();
                if (settings.JewelryAttr0 != 0)
                {
                    GUILayout.BeginHorizontal();
                    if(settings.JewelryAttr0==1)
                        settings.JewelryAttr1 = GUILayout.SelectionGrid(settings.JewelryAttr1, JewelryAttrName1, 8);
                    else
                        settings.JewelryAttr2 = GUILayout.SelectionGrid(settings.JewelryAttr2, JewelryAttrName2, 7);
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }


    [HarmonyPatch(typeof(DateFile), "MakeNewItem")]
    public static class MakeYourWant_MakeNewItem_Patch
    {
        static bool Prefix(DateFile __instance, ref int __result,ref int id, ref int temporaryId, 
            ref int bookObbs, ref int buffObbs, ref int sBuffObbs)
        {
            int equipTyp = int.Parse(__instance.presetitemDate[id][1]);//1武器，2防具，3饰品
            if (!Main.enabled) return true;//mod未开启
            if (__instance.presetitemDate[id][4] != "4") return true;//排除非装备
            int itemTyp = int.Parse(__instance.presetitemDate[id][5]);
            if (itemTyp == 42) return true;//排除神书
            if (itemTyp == 36) return true;//排除神兵
            if (itemTyp == 21 || int.Parse(__instance.presetitemDate[id][31]) > 0) ;//排除书籍
            if (equipTyp <= 0 || equipTyp >= 4) return true;//装备类型不符
            if ((equipTyp == 1 && !Main.settings.SetWeapon) || (equipTyp == 2 && !Main.settings.SetArmor) || 
                (equipTyp == 3 && !Main.settings.SetJewelry)) return true;//装备类型对应选项未开启
            if (!Main.settings.EffectAll)//仅对玩家制造的装备生效
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(false);
                if (!st.GetFrames().Select(f => f.GetMethod().Name).Any(n => n.Contains("SetMakeItem")))
                    return true;
            }
            Dictionary<int, string> value = new Dictionary<int, string>();
            int itemInstanceId;
            if (temporaryId < 0)
            {
                itemInstanceId = temporaryId;
                __instance.itemsDate.Remove(temporaryId);
                __instance.itemsChangeDate.Remove(temporaryId);
            }
            else
            {
                __instance.newItemId++;
                itemInstanceId = __instance.newItemId;
            }
            __instance.itemsDate.Add(itemInstanceId, value);
            __instance.itemsDate[itemInstanceId][999] = id.ToString();//物品类型id
            bool isFixed = int.Parse(__instance.presetitemDate[id][902]) <= 0;
            int duration = Mathf.Abs(int.Parse(__instance.presetitemDate[id][902]));
            //武器
            if (equipTyp == 1)
            {
                //耐久
                int minimum = Main.settings.WeaponMoreDuration ? 80 : 50;
                duration = isFixed ? duration : 1 + duration * UnityEngine.Random.Range(minimum, 101) / 100;
                __instance.itemsDate[itemInstanceId][902] = duration.ToString();//最大耐久
                __instance.itemsDate[itemInstanceId][901] = duration.ToString();//当前耐久
                //式
                if (int.Parse(__instance.presetitemDate[id][606]) > 0)
                {
                    List<string> attackTypList0 = new List<string>(
                        __instance.presetitemDate[id][7].Split(new char[] { '|' }));
                    string attackTypList1 = "";
                    List<string> attackTypList2 = new List<string>();
                    for (int i = 0; i < 6; i++) attackTypList2.Add(Main.settings.AttackTyp[i].ToString());
                    int index;
                    string s;
                    for (int i = 0; i < 6; i++)
                    {
                        s = attackTypList2[i];
                        if (attackTypList0.Contains(s)) attackTypList0.Remove(s);
                        else attackTypList2[i] = "";
                    }
                    Main.Logger.Log(String.Join("|", attackTypList2.ToArray()));
                    int count = attackTypList0.Count;
                    for (int i = 0; i < count; i++)
                    {
                        index = attackTypList2.IndexOf("");
                        s = attackTypList0[UnityEngine.Random.Range(0, attackTypList0.Count)];
                        if (index != -1) attackTypList2[index] = s;
                        else attackTypList2.Add(s);
                        attackTypList0.Remove(s);
                    }
                    attackTypList1 = String.Join("|", attackTypList2.ToArray());
                    __instance.itemsDate[itemInstanceId][7] = attackTypList1;
                }
                //功法发挥
                if (Main.settings.WeaponEngravingMustBe || UnityEngine.Random.Range(0, 100) < buffObbs)
                {
                    int carving = Main.settings.WeaponEngraving == 0 ? (UnityEngine.Random.Range(0, 3) + 1) : Main.settings.WeaponEngraving;
                    if (buffObbs == 999)
                    {
                        carving *= 100;
                    }
                    __instance.itemsDate[itemInstanceId][505] = carving.ToString();
                }
                //词条
                int power = int.Parse(__instance.presetitemDate[id][504]);
                if (Main.settings.WeaponPowerMustBe || (power != 0 && UnityEngine.Random.Range(0, 100) < sBuffObbs))
                {
                    if (Main.settings.WeaponPower0 == 3)//其他
                    {
                        if (Main.settings.WeaponPower3 == 5)//随机
                        {
                            List<int> powerList = new List<int>
                        {
                            2,
                            3,
                            4,
                            5,
                            6,
                            7,
                            8,
                            9,
                            10,
                            11,
                            12,
                            13,
                            101,
                            102,
                            103,
                            104,
                            105,
                            106,
                            107,
                            108,
                            109,
                            110,
                            111,
                            112,
                            401,
                            402,
                            403,
                            404,
                            407
                        };
                            if (power < 0)
                            {
                                powerList.Add(Mathf.Abs(power));//武器杀
                            }
                            int material = int.Parse(__instance.presetitemDate[id][506]);
                            if (material != 0)
                            {
                                powerList.Add(209 + material);//材质杀
                            }
                            power = powerList[UnityEngine.Random.Range(0, powerList.Count)];
                        }
                        else power = Main.settings.WeaponPower3 == 4 ? 407 : 401 + Main.settings.WeaponPower3;
                    }
                    else
                    {
                        if (Main.settings.WeaponPower0 == 2)//杀
                        {
                            if (Main.settings.WeaponPower2 == 1)//材质杀
                            {
                                int material = int.Parse(__instance.presetitemDate[id][506]);
                                if (material == 0)//没有布杀
                                {
                                    __result = itemInstanceId;
                                    return false;
                                }
                                else power = 209 + material;
                            }
                            else power = Mathf.Abs(power);//武器杀
                        }
                        else power = (Main.settings.WeaponPower0 == 0 ? 2 : 101) + Main.settings.WeaponPower1;
                    }
                    //__instance.ChangeSPower(id, num, list4[UnityEngine.Random.Range(0, list4.Count)]);
                    var method = typeof(DateFile).GetMethod("ChangeNewItemSPower",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    method.Invoke(__instance, new object[] { id, itemInstanceId, power });
                }
                __result = itemInstanceId;
                return false;
            }
            //防具
            if (equipTyp == 2)
            {
                //耐久
                int minimum = 50;
                duration = isFixed ? duration : 1 + duration * UnityEngine.Random.Range(minimum, 101) / 100;
                __instance.itemsDate[itemInstanceId][902] = duration.ToString();//最大耐久
                __instance.itemsDate[itemInstanceId][901] = duration.ToString();//当前耐久
                if (Main.settings.ArmorAttrMustBe || UnityEngine.Random.Range(0, 100) < buffObbs)
                {
                    int attrTyp = (Main.settings.ArmorAttr == 0) ? UnityEngine.Random.Range(0, 6) : (Main.settings.ArmorAttr - 1);
                    __instance.itemsDate[itemInstanceId][51361 + attrTyp] = 
                        (Mathf.Max(int.Parse(__instance.presetitemDate[id][8]) / 2, 1) * ((buffObbs != 999) ? 5 : 10)).ToString();
                }
                if (Main.settings.ArmorPower!=1&&
                    (Main.settings.ArmorPowerMustBe || UnityEngine.Random.Range(0, 100) < sBuffObbs))
                {
                    int power;
                    if (Main.settings.ArmorPower == 0)
                    {
                        List<int> list2 = new List<int>
                        {
                            401,
                            402,
                            405,
                            406,
                            407
                        };
                        power = list2[UnityEngine.Random.Range(0, list2.Count)];
                    }
                    else
                    {
                        power = (Main.settings.ArmorPower <= 3) ?
                            (Main.settings.ArmorPower + 399) : (Main.settings.ArmorPower + 401);
                    }
                    var method = typeof(DateFile).GetMethod("ChangeNewItemSPower",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    method.Invoke(__instance, new object[] { id, itemInstanceId, power });
                    //__instance.ChangeNewItemSPower(id, num, list2[UnityEngine.Random.Range(0, list2.Count)]);
                }
                __result = itemInstanceId;
                return false;
            }
            //饰品
            if (equipTyp == 3)
            {
                //耐久
                int minimum = 50;
                duration = isFixed ? duration : 1 + duration * UnityEngine.Random.Range(minimum, 101) / 100;
                __instance.itemsDate[itemInstanceId][902] = duration.ToString();//最大耐久
                __instance.itemsDate[itemInstanceId][901] = duration.ToString();//当前耐久
                if (Main.settings.JewelryAttrMustBe || UnityEngine.Random.Range(0, 100) < buffObbs)
                {
                    int attrTyp;
                    if (Main.settings.JewelryAttr0 == 0) attrTyp = (UnityEngine.Random.Range(0, 100) < 50) ?
                            UnityEngine.Random.Range(0, 16) + 50501 : UnityEngine.Random.Range(0, 14) + 50601;
                    else
                    {
                        if (Main.settings.JewelryAttr0 == 1) attrTyp = Main.settings.JewelryAttr1 + 50501;
                        else attrTyp = Main.settings.JewelryAttr2 + 50601;
                    }
                    __instance.itemsDate[itemInstanceId][attrTyp] =
                        (Mathf.Max(int.Parse(__instance.presetitemDate[id][8]) / 2, 1) * ((buffObbs != 999) ? 5 : 10)).ToString();
                }
                if (Main.settings.JewelryPowerMustBe || UnityEngine.Random.Range(0, 100) < sBuffObbs)
                {
                    int power = 407;
                    var method = typeof(DateFile).GetMethod("ChangeNewItemSPower",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    method.Invoke(__instance, new object[] { id, itemInstanceId, power });
                    //__instance.ChangeNewItemSPower(id, num, list2[UnityEngine.Random.Range(0, list2.Count)]);
                }
                __result = itemInstanceId;
                return false;
            }
            //虽然这理论上是不可能的情况
            Main.Logger.Log("wrong type:"+equipTyp.ToString()+"; id:"+id.ToString());
            __result = itemInstanceId;
            return false;
        }
    }
}
