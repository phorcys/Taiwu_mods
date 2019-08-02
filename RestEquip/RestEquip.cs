using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityModManagerNet;

namespace RestEquip
{
    public static class Main
    {
        const int GongFaIndexOffsetInSetting = -1;


        public static bool Enabled { get; private set; }
        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }
        public static Settings settings;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Logger = modEntry.Logger;
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            
            return true;
        }


        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            // 功法
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("功法");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            settings.RecoveryMianQiGongFaSelectedIndex = GUILayout.SelectionGrid(
                settings.RecoveryMianQiGongFaSelectedIndex,
                new string[] { "<color=#808080>不切換</color>", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" }, 10,
                new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            // 裝備
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("裝備");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            settings.RecoveryMianQiEquipSelectedIndex = GUILayout.SelectionGrid(
                settings.RecoveryMianQiEquipSelectedIndex,
                new string[] { "<color=#808080>不切換</color>", "壹", "贰", "叁" }, 4,
                new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }

    [HarmonyPatch(typeof(ActorMenu), "MianQiAutoChange")]
    public class ActorMenu_MianQiAutoChange_Patch
    {
        static int _origin_mianActorEquipGongFaIndex = -1;
        private static int _origin_EquipConfigIndex = -1;

        private static bool Prefix(ActorMenu __instance, ref int key)
        {
            if (!Main.Enabled) return true;
            if (DateFile.instance == null)
                Main.Logger.Error("DateFile.instance is null");
            if (key != DateFile.instance.MianActorID())
                return true;

            if (ActorMenu.instance == null)
            {
                Main.Logger.Error("ActorMenu.instance is null");
                return true;
            }

            changeGongFa();
            changeEquip();
 
            return true;
        }

        private static void changeGongFa()
        {
            if (Main.settings.RecoveryMianQiGongFaIndex < 0) return;
            int current_GongFaIndex = DateFile.instance.mianActorEquipGongFaIndex;
            if (current_GongFaIndex == Main.settings.RecoveryMianQiGongFaIndex) return;
            _origin_mianActorEquipGongFaIndex = current_GongFaIndex;
            //Main.Logger.Log($"Change 功法 to {Main.settings.RecoveryMianQiGongFaIndex}");
            ActorMenu.instance.ChangeEquipGongFa(Main.settings.RecoveryMianQiGongFaIndex);
        }


        private static void changeEquip()
        {
            int newEquipConfigIndex = Main.settings.RecoveryMianQiEquipConfigIndex;
            if (newEquipConfigIndex < 0) return;
            int nowEquipConfigIndex = DateFile.instance.nowEquipConfigIndex;
            if (nowEquipConfigIndex == newEquipConfigIndex) return;

            //Main.Logger.Log($"Change 裝備 from {nowEquipConfigIndex} to {newEquipConfigIndex}");
            _origin_EquipConfigIndex = nowEquipConfigIndex;
            changeEquip(newEquipConfigIndex);
        }


        private static void changeEquip(int index)
        {
            // Main.Logger.Log($"changeEquip: {index}");
            int num = DateFile.instance.MianActorID();
            int nowEquipConfigIndex = DateFile.instance.nowEquipConfigIndex;
            Dictionary<int, int> dictionary = DateFile.instance.mainActorequipConfig[nowEquipConfigIndex];
            foreach (KeyValuePair<int, int> keyValuePair in dictionary)
            {
                bool flag = keyValuePair.Value != 0 && DateFile.instance.actorsDate[num][keyValuePair.Key] == keyValuePair.Value.ToString();
                if (flag)
                {
                    DateFile.instance.GetItem(num, keyValuePair.Value, 1, false, -1, 0);
                    DateFile.instance.actorsDate[num][keyValuePair.Key] = "0";
                }
            }
            DateFile.instance.nowEquipConfigIndex = index;
            Dictionary<int, int> nowEquipGroup = DateFile.instance.mainActorequipConfig[index];
            List<int> list = new List<int>();
            foreach (KeyValuePair<int, int> keyValuePair2 in nowEquipGroup)
            {
                bool flag2 = keyValuePair2.Value != 0;
                if (flag2)
                {
                    bool flag3 = DateFile.instance.HasItem(num, keyValuePair2.Value);
                    if (flag3)
                    {
                        DateFile.instance.LoseItem(num, keyValuePair2.Value, 1, false, false);
                        DateFile.instance.actorsDate[num][keyValuePair2.Key] = keyValuePair2.Value.ToString();
                    }
                    else
                    {
                        list.Add(keyValuePair2.Key);
                    }
                }
            }
            list.ForEach(delegate (int e)
            {
                nowEquipGroup[e] = 0;
            });
        }

        public static void Postfix()
        {
            // change Gougfa back
            if (_origin_mianActorEquipGongFaIndex >= 0)
            {
                //Main.Logger.Log($"Change 功法 back to {_origin_mianActorEquipGongFaIndex}");
                ActorMenu.instance.ChangeEquipGongFa(_origin_mianActorEquipGongFaIndex);
                _origin_mianActorEquipGongFaIndex = -1;
            }

            // change equ back
            if(_origin_EquipConfigIndex >= 0)
            {
                // Main.Logger.Log($"Change 裝備 back to {_origin_EquipConfigIndex}, 內息 = {DateFile.instance.GetActorMianQi(DateFile.instance.MianActorID())}");
                changeEquip(_origin_EquipConfigIndex);
                _origin_EquipConfigIndex = -1;
            }
        }

 
    }
}
