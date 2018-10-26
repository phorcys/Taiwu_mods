using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace NoMoreTrash
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        public int levelLimit = 2;//0-8,0最低，高于指定等级的物品不会被丢弃
        public bool dropEquip = true;//丢弃装备
        public bool dropMaterial = true;//丢弃制造系(素材+工具)(Qu式英语)
        public bool dropDrag = true;//丢弃成药

    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static String[] toolBarText = { "九品", "八品", "七品", "六品", "五品", "四品", "三品", "二品" ,"一品"};

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
            GUILayout.Label("丢弃品级(小于等于该品级的才会被丢弃)");
            Main.settings.levelLimit = GUILayout.Toolbar(Main.settings.levelLimit, toolBarText);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Main.settings.dropDrag = GUILayout.Toggle(Main.settings.dropDrag, "丢弃丹药", new GUILayoutOption[0]);
            Main.settings.dropEquip = GUILayout.Toggle(Main.settings.dropEquip, "丢弃装备(不包括蛐蛐)", new GUILayoutOption[0]);
            Main.settings.dropMaterial = GUILayout.Toggle(Main.settings.dropMaterial, "丢弃制造系(工具、引子、强化素材)", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }


    [HarmonyPatch(typeof(BattleSystem), "ShowBattleRated", new Type[] { typeof(float), typeof(int) })]
    public static class BattleSystem_ShowBattleRated_Patch
    {
        public static int index = 0;
        public const int MinorTypeQuQu = 19;
        public const int MajorTypeEquip = 4;
        public const int MajorTypeMaterial = 1;
        public const int MajorTypeDrag = 2;
        static void Prefix(BattleSystem __instance)
        {
            if (!Main.enabled)
                return;
            BindingFlags bind_flag = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = __instance.GetType();
            FieldInfo field = type.GetField("battleBooty", bind_flag);
            int actorId = DateFile.instance.MianActorID();

            var booty=(List<int[]>)field.GetValue(__instance);
            List<int[]> new_booty =new List<int[]>();
            foreach(var item_pair in booty)
                if(item_pair.Count()==2&& IsTrash(item_pair[0]))
                {
                    int itemId = item_pair[0];
                    int itemNumber = item_pair[1];
                    if (DateFile.instance.actorItemsDate.ContainsKey(actorId))
                    {
                        bool flag = int.Parse(DateFile.instance.GetItemDate(itemId, 6, true)) == 0;//来自LoseItem代码，推测是可叠加
                        if (DateFile.instance.actorItemsDate[actorId].ContainsKey(itemId))
                        {
                            Main.Logger.Log("Drop:" + GetItemDesc(itemId));
                            DateFile.instance.actorItemsDate[actorId][itemId]-= itemNumber;//减少数量
                            if (DateFile.instance.actorItemsDate[actorId][itemId] <= 0 || flag)//移除
                            {
                                DateFile.instance.actorItemsDate[actorId].Remove(itemId);
                                if (flag)
                                    DateFile.instance.lateDeleteItems.Add(itemId);
                            }
                        }

                    }
                }
                else
                    new_booty.Add(item_pair);
            field.SetValue(__instance, new_booty);
        }
        private static string GetItemDesc(int id)
        {
            int typeMinor = int.Parse(DateFile.instance.GetItemDate(id, 5));//小类
            string text = string.Format("{0}({1})",
                                    DateFile.instance.GetItemDate(id, 0)//名字
                                    ,DateFile.instance.massageDate[301][0].Split(new char[] { '|' })[typeMinor]);//种类
            return text;
        }
        private static bool IsTrash(int id)
        {
            int level = int.Parse(DateFile.instance.GetItemDate(id, 8));//品级，1为最低
            int typeMajor = int.Parse(DateFile.instance.GetItemDate(id, 4));//大种类
            int typeMinor = int.Parse(DateFile.instance.GetItemDate(id, 5));//小种类
            if (level> 1 + Main.settings.levelLimit)//品级高过指定值不处理，levelLimit是[0,8]，level是[1,9]需要转化后对比
                return false;
            if (typeMinor == MinorTypeQuQu)//促织不处理
                return false;
            if (typeMajor == MajorTypeEquip && Main.settings.dropEquip)//装备
                return true;
            if (typeMajor == MajorTypeMaterial && Main.settings.dropMaterial)//制造系
                return true;
            if (typeMajor == MajorTypeDrag && Main.settings.dropDrag)//药
                return true;
            return false;
        }
    }
    //返回主界面从新计数
}
