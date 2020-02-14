using Harmony12;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace TrainingRoom
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry) { Save(this, modEntry); }
        public bool moreEvents = false; // 其他門派弟子是否開放任務
    }
    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            Main.settings.moreEvents = GUILayout.Toggle(Main.settings.moreEvents, "开启更多任务", new GUILayoutOption[0]);
        }

    }

    //[HarmonyPatch(typeof(Int32), "Parse", new Type[] { typeof(string) })]
    //public static class Int32_Parse
    //{
    //    public static void Prefix(string s)
    //    {
    //        Main.Logger.Log(s);
    //    }
    //}

    [HarmonyPatch(typeof(ui_MessageWindow), "SetMassageWindow")]
    public static class ui_MessageWindow_SetMassageWindow_Patch
    {
        public static void Prefix(int[] baseEventDate, int chooseId)
        {
            //這段只是用來查對話是什麼ID , 備用
            //Main.Logger.Log("baseEventDate:" + string.Join(", ", Array.ConvertAll(baseEventDate, i => i.ToString())));
            //Main.Logger.Log("chooseId:" + chooseId.ToString());
            int 任務參數2 = baseEventDate[2];
            int 任務內容2 = int.Parse(DateFile.instance.eventDate[任務參數2][2]);
            //Main.Logger.Log("任務內容2:" + 任務內容2.ToString());
            int 主角ID = DateFile.instance.MianActorID();
            //Main.Logger.Log("主角ID:" + 主角ID.ToString());
            int 任務對象ID = (任務內容2 != 0) ? ((任務內容2 != -1) ? 任務內容2 : 主角ID) : baseEventDate[1];
            //Main.Logger.Log("任務對象ID:" + 任務對象ID.ToString());
            int GangID = int.Parse(DateFile.instance.GetActorDate(任務對象ID, 19, false));
            //Main.Logger.Log("GangID:" + GangID.ToString());
            int GangGroupID = DateFile.instance.GetGangValueId(GangID, int.Parse(DateFile.instance.GetActorDate(任務對象ID, 20, false)));
            //Main.Logger.Log("GangGroupID:" + GangGroupID.ToString());

            Dictionary<int, string> GangGroupValue = DateFile.instance.presetGangGroupDateValue[GangGroupID];
            //Main.Logger.Log("GangGroupValue:" + GangGroupValue.Select(x => String.Format("{0}:{1}", x.Key, x.Value)).ToArray().Join());
            //Main.Logger.Log("\n");

            //if (DateFile.instance.eventDate.ContainsKey(90000001))
            //{
            //    Main.Logger.Log("eventDate:" + DateFile.instance.eventDate[90000001].Select(x => String.Format("{0}:{1}", x.Key, x.Value)).ToArray().Join());
            //    Main.Logger.Log("\n");
            //}
            //if (DateFile.instance.enemyTeamDate.ContainsKey(90000001))
            //{
            //    Main.Logger.Log("enemyTeamDate:" + DateFile.instance.enemyTeamDate[90002001].Select(x => String.Format("{0}:{1}", x.Key, x.Value)).ToArray().Join());
            //    Main.Logger.Log("\n");
            //}

            string eventId;
            if (chooseId == 1000000002)
            {
                if (GangGroupID == 0)  // "无"
                {
                    EventSeries.Series1(GangGroupID); // 添加 相枢幻身
                }
                else if (GangGroupID == 1) //"太吾村民"
                {
                    EventSeries.Series2(GangGroupID); // 添加 剑冢再临
                }
                else if (GangGroupValue.TryGetValue(812, out eventId) && eventId == "901300001")    //城主,村长,镇长,大当家
                {
                    EventSeries.Series3(GangGroupID);// 添加 清理宵小&剿灭邪道
                }
                else if (GangID == 15) // "血犼教"
                {
                    // EventSeries.Series4(GangGroupID);  // 添加 門派弟子互动
                }
                else if (Main.settings.moreEvents && GangID >= 1 && GangID < 15) //其他門派弟子互動
                {
                    // EventSeries.Series4(GangGroupID);  // 添加 門派弟子互动
                }
            }
        }



    }
}
