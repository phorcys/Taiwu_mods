using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Harmony12;
using UnityModManagerNet;
using System.Reflection;

namespace DoNotDisTurb
{

    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        public  bool disableEvent = true;
        public  bool skillbattleTips = true;
        public  bool disableBeg = true;
    }
    public static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger logger;
        public static Settings settings;


        public static string AskTips;
        public static string ReAskTips;
        public static string AnswerTips;

        public static bool PowerChange = false;
        public static bool isFirstTime = true;

        public static string AskTips_base;
        public static string ReAskTips_base;
        public static string AnswerTips_base;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            settings = Settings.Load<Settings>(modEntry);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value) return false;
            enabled = value;
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.disableEvent = GUILayout.Toggle(settings.disableEvent, "屏蔽入魔事件", new GUILayoutOption[0]);
            settings.disableBeg = GUILayout.Toggle(settings.disableBeg, "屏蔽乞讨事件", new GUILayoutOption[0]);
            settings.skillbattleTips = GUILayout.Toggle(settings.skillbattleTips, "开启较艺提示", new GUILayoutOption[0]);

        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        [HarmonyPatch(typeof(UIDate), "SetTrunChangeWindow")]
        public static class UIData_SetTrunChangeWindow_Patch
        {
            static void Prefix()
            {
                if (!settings.disableEvent)
                    return;
                for (int index = UIDate.instance.changTrunEvents.Count - 1; index >= 0; index--)
                {
                    //入魔事件屏蔽
                    if (UIDate.instance.changTrunEvents[index][0] == 248)
                    {
                        UIDate.instance.changTrunEvents.Remove(UIDate.instance.changTrunEvents[index]);
                        //logger.Log("delete an event\n");
                    }
                }

            }
        }

        [HarmonyPatch(typeof(PeopleLifeAI), "AISetEvent")]
        public static class PeopleLifeAI_AISetEvent_Patch
        {
            static void Postfix(PeopleLifeAI __instance,ref int typ , ref int[] aiEventDate, ref Dictionary<int, Dictionary<int, List<int[]>>> ___eventDate)
            {
                if (!settings.disableBeg)
                    return;
                
               //乞讨事件屏蔽
                if(___eventDate == null)
                {
                    logger.Log("___eventDate is null");
                    return;
                }
                if(typ>0&&typ<10&&typ !=8)
                {
                    if(aiEventDate[2] == 207|| aiEventDate[2] == 208|| aiEventDate[2] == 209
                        || aiEventDate[2] == 218 || aiEventDate[2] == 219 || aiEventDate[2] == 220
                        || aiEventDate[2] == 221 || aiEventDate[2] == 222 || aiEventDate[2] == 223
                        || aiEventDate[2] == 224)
                    {
                        if(___eventDate.ContainsKey(typ))
                        {
                            for (int i = ___eventDate[typ][aiEventDate[1]].Count - 1; i >= 0; i--)
                            {
                                
                                if (___eventDate[typ][aiEventDate[1]][i][2] == aiEventDate[2])
                                {
                                    ___eventDate[typ][aiEventDate[1]].Remove(___eventDate[typ][aiEventDate[1]][i]);
                                    ___eventDate.Remove(typ);
                                    //logger.Log(string.Format("remove actor id{0},event id {1},evnet type{2}", aiEventDate[1], aiEventDate[2], typ));
                                }
                            }
                           
                        }
                        
                    }
                }
               
               

            }
        }

        //较艺助手
        [HarmonyPatch(typeof(SkillBattleSystem), "QuestionPowerChange")]
        public static class SkillBattleSystem_QuestionPowerChange_Patch
        {
            static void Postfix(SkillBattleSystem __instance, ref int ___enemyVp1,ref int ___enemyVp2,
                ref int ___actorVp1,ref int ___actorVp2,ref int[] ___actorSkillBattleValue, ref int[] ___enemySkillBattleValue,
                ref int ___nowSkillIndex,ref int ___questionPower)
            {
                int re_obbs;
                int anwser_obbs;
                if (___enemySkillBattleValue[___nowSkillIndex] >= ___questionPower)
                {
                    re_obbs = 0;
                    anwser_obbs = 100;
                    Main.AskTips = string.Format("对方解答的概率为{0}%", anwser_obbs);
                    Main.ReAskTips = "此次反问会失败";
                    Main.AnswerTips = string.Format("对方放弃的概率为{0}%", ___enemyVp2);
                }
                else
                {
                    re_obbs = Mathf.Max(80 - ___actorSkillBattleValue[___nowSkillIndex] * 80 / ___questionPower, 0);
                    re_obbs += ___enemyVp1;
                    re_obbs = Mathf.Min(100, re_obbs);
                    Main.AskTips = string.Format("对方反问的概率为{0}%，放弃概率为{1}%",re_obbs,100-re_obbs);
                    Main.ReAskTips = "此次反问将成功";
                    Main.AnswerTips = "解答后对方将必然放弃";
                }
                Main.PowerChange = true;
            }
        }

       [HarmonyPatch(typeof(WindowManage), "ShowTips")]
        public static class WindowManage_ShowTips_Patch
        {
            static void Postfix(WorldMapSystem __instance, ref Text ___itemMoneyText, ref Text ___itemLevelText, ref Text ___informationMassage, ref Text ___informationName, ref bool ___anTips)
            {
                if (!Main.enabled && !settings.skillbattleTips)
                {
                    return;
                }
                if(!Main.PowerChange)
                {
                    return;
                }
                if(___anTips)
                {
                    if (___informationName.text == "解答" )
                    {
                        //logger.Log(string.Format("name = {0} text={1}", ___informationName.text, ___informationMassage.text));
                        ___informationMassage.text = string.Format("{0}\n\n{1}",DateFile.instance.massageDate[906][1],DateFile.instance.SetColoer(20004, Main.AnswerTips));
                    }
                    else if(___informationName.text == "反问")
                    {
                        ___informationMassage.text = string.Format("{0}\n\n{1}", DateFile.instance.massageDate[905][1], DateFile.instance.SetColoer(20004, Main.ReAskTips));
                    }
                    else if(___informationName.text == "提问")
                    {
                        ___informationMassage.text = string.Format("{0}\n\n{1}", DateFile.instance.massageDate[903][1], DateFile.instance.SetColoer(20004, Main.AskTips));
                    }
                    //Main.PowerChange = false;
                }
            }
        }
    }
}
