
using Harmony12;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace QuQuXRay
{
    public static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger logger;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            
            logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            new GameObject(typeof(UI).FullName, typeof(UI));
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {

            enabled = value;
            logger.Log(enabled ? "已开启" : "已关闭");
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (!Main.enabled)
            {
                GUILayout.Label("Mod已关闭!");
                return;
            }
            GUILayout.Label("捕捉促织时按f2打开/关闭透视窗口");
        }
    }

    [HarmonyPatch(typeof(GetQuquWindow), "SetGetQuquWindow")]
    public static class NewGame_SetNewGameDate_Patch
    {

        private static void Postfix()
        {
            Main.logger.Log("attached");
            foreach(int i in GetQuquWindow.instance.cricketDate.Keys)
            {
                string ququ_data = i + " : ";
                for (int j = 0; j < GetQuquWindow.instance.cricketDate[i].Length; j++)
                {
                    ququ_data += "(" + j + "," + GetQuquWindow.instance.cricketDate[i][j] + ")";
                }
                Main.logger.Log(DateFile.instance.cricketDate[GetQuquWindow.instance.cricketDate[i][1]][0] + "," + DateFile.instance.cricketDate[GetQuquWindow.instance.cricketDate[i][2]][0]);
                
                Main.logger.Log(ququ_data);
            }

            return;
        }

    }


}