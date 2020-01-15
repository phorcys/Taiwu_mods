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
using GameData;

//using System.IO;

namespace LongDaoSSR
{
    public static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger logger;
        public static int lastNPCid = -1; //最后生成且还未判断的NPC的id，-1表示无
        public static bool oneFlag = false;
        public static bool isInGame = false;
        //public static string logPath; //调试输出路径

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            //logPath = System.IO.Path.Combine(modEntry.Path, "log/debuglog.txt");

            return true;
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value) return false;
            enabled = value;
            logger.Log("龙岛忠仆MOD正在运行");
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("  修改了一些龙岛忠仆的特性，让忠仆更适合太吾村的发展!");
            GUILayout.Label("<color=#8FBAE7FF>【志同道合】</color> 龙岛忠仆的处世立场与玩家获得忠仆时的立场相同.");
            GUILayout.Label("<color=#F28234FF>【龙神赐寿】</color> 每个龙岛忠仆都被龙神赋予更长的阳寿，终生侍奉主人.");
            GUILayout.Label("<color=#E4504DFF>【天资·艺】</color> 化全身资质于一道，大幅增强一项技艺的资质.");
            GUILayout.Label("<color=#E4504DFF>【天资·武】</color> 化全身资质于一道，大幅增强一项进攻武学的资质，中幅增强内功身法绝技的资质.");
            GUILayout.Label("  注: 天资特性只会是人物原本资质最好的一项");
            GUILayout.Label("  <color=#FF0000FF>如果要删除本MOD，请在对应存档内按下清除新特性的按钮并存档，避免坏档，清除新特性只影响显示效果，忠仆不会消失</color>");
            
            //检测存档
            DateFile tbl = DateFile.instance;
            //if (tbl == null || tbl.actorsDate == null || !tbl.actorsDate.ContainsKey(tbl.mianActorId))
            if (tbl == null || Game.Instance.GetCurrentGameStateName().ToString() != eGameState.InGame.ToString())
            {
                GUILayout.Label("  存档未载入!");
            }
            else
            {
                if (GUILayout.Button("清除新特性"))
                {
                    DeletNewFeature();
                }
            }
        }

        /// <summary>
        /// 遍历人物列表并清除新特性
        /// </summary>
        public static void DeletNewFeature()
        {
            List<int> idlist = new List<int>();
            //int num = 0;
            logger.Log("开始清除新特性");
            int[] allCount = Characters.GetAllCharIds();
            logger.Log("人物有" + allCount.Length + "个等待遍历");
            /*
            foreach (KeyValuePair<int, Dictionary<int, string>> e in DateFile.instance.actorsDate)
            {
                if (e.Value.ContainsKey(101))
                {
                    if(e.Value[101].Contains("4006"))
                    {
                        num++;
                        idlist.Add(e.Key);
                    }
                }
            }
            */
            foreach (int actorId in allCount)
            {
                if (Characters.HasChar(actorId) && Characters.GetCharProperty(actorId, 101).Contains("4006"))
                {
                    idlist.Add(actorId);
                }
            }
            logger.Log("检测到" + idlist.Count.ToString() + "个龙岛忠仆，开始清除新特性数据...");
            //logger.Log("检测到" + num + "个龙岛忠仆，开始清除新特性数据...");
            for(int i = 0; i < idlist.Count; i++)
            {
                DeletNPCNewFeature(idlist[i]);
            }
            logger.Log("清除完毕");
        }

        /// <summary>
        /// 清除NPC的新特性
        /// </summary>
        /// <param name="id">NPCid</param>
        public static void DeletNPCNewFeature(int id)
        {
            bool hasNewFeature = false;
            //Dictionary<int, string> npc;
            //npc = DateFile.instance.actorsDate[id];
            string featureStr = Characters.GetCharProperty(id, 101);
            
            List<int> feature = new List<int>();
            for (int i = 0; i < DateFile.instance.GetActorFeature(id).Count; i++)
            {
                feature.Add(DateFile.instance.GetActorFeature(id)[i]);
            }
            foreach (int f in feature)
            {
                if (f >= 4006 && f <= 4034)//新特性编号范围
                {
                    hasNewFeature = true;
                    featureStr = featureStr.Replace("|" + f.ToString(), "");
                }
            }
            
            if (hasNewFeature)
            {
                //DateFile.instance.ActorFeaturesCacheReset(id);
                Characters.SetCharProperty(id, 101, featureStr);
                DateFile.instance.ActorFeaturesCacheReset();
            }
            
        }


        /// <summary>
        /// 在开始游戏界面注入新特性
        /// </summary>
        [HarmonyPatch(typeof(MainMenu), "ShowStartGameWindow")]
        public static class MainMenu_ShowStartGameWindow_Patch
        {
            private static void Postfix()
            {
                if (!Main.enabled)
                {
                    return;
                }
                if (!oneFlag)
                {
                    AddAllFeature();
                    //debugLogIntIntString(DateFile.instance.actorFeaturesDate);//显示全部特性
                }
                return;
            }
        }

        /// <summary>
        /// 获取新生NPC的ID
        /// </summary>
        [HarmonyPatch(typeof(DateFile), "MakeNewActor")]
        public static class DateFile_MakeNewActor_Patch
        {
            private static void Postfix(DateFile __instance, int __result)
            {
                if (!Main.enabled)
                {
                    return;
                }
                //logger.Log("新的NPC生成了！id:" + __result);
                lastNPCid = __result;
                //DateFile.instance.ActorFeaturesCacheReset(__result); //刷新特性
                DateFile.instance.ActorFeaturesCacheReset(); //刷新特性缓存
                return;
            }
        }

        /// <summary>
        /// 创建NPC之后，显示新相知之前执行的函数，用来修改龙岛忠仆
        /// </summary>
        [HarmonyPatch(typeof(DateFile), "ChangeFavor")]
        public static class DateFile_ChangeFavor_Patch
        {
            private static void Postfix()
            {
                if (!Main.enabled)
                {
                    return;
                }
                if (lastNPCid != -1)
                {
                    //logger.Log("特性:" + DateFile.instance.actorsDate[lastNPCid][101]);
                    if (IsLongDaoZhongPu(lastNPCid))
                    {
                        NpcChange(lastNPCid);
                    }
                    lastNPCid = -1;
                }
            }
        }

        /// <summary>
        /// 判断指定idNPC是否为龙岛忠仆
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsLongDaoZhongPu(int id)
        {
            //bool flag = false;
            List<int> npcFeature = DateFile.instance.GetActorFeature(lastNPCid);

            return npcFeature.Contains(4005);
            /*
            for (int i = 0; i < npcFeature.Count; i++)
            {
                if (npcFeature[i] == 4005) //4005为龙岛忠仆特性
                {
                    flag = true;
                    logger.Log("检测到新加入的龙岛忠仆");
                }
            }
            return flag;
            */
        }

        /// <summary>
        /// 修改指定idNPC数据
        /// </summary>
        /// <param name="id"></param>
        public static void NpcChange(int id)
        {
            //Dictionary<int, string> npc;
            //npc = DateFile.instance.actorsDate[id];
            //Dictionary<int, string> player;
            //player = DateFile.instance.actorsDate[DateFile.instance.mianActorId];

            //1.【志同道合】龙岛忠仆的处世立场与玩家获得忠仆时的立场相同
            //logger.Log("npc[16]:"+npc[16] + ",player[16]:"+player[16]);
            Characters.SetCharProperty(id, 16, Characters.GetCharProperty(DateFile.instance.mianActorId, 16)); //修改立场
            //npc[16] = player[16]; //修改立场
            //npc[101] += "|4006"; //添加立场特性

            //2.【龙神赐寿】每个龙岛忠仆都被龙神赋予更多的阳寿，终生侍奉主人
            //logger.Log("npc[11] npc[12] npc[13]:"+npc[11] +" "+ npc[12] + " " + npc[13]);
            //npc[13] = "100";
            //npc[12] = "100";
            Characters.SetCharProperty(id, 13, "100");
            Characters.SetCharProperty(id, 12, "100");

            //logger.Log("npc[11] npc[12] npc[13]:" + npc[11] + " " + npc[12] + " " + npc[13]);
            //npc[101] += "|4007"; //添加寿命特性
            Characters.SetCharProperty(id, 101, Characters.GetCharProperty(id, 101) + "|4006|4007"); //添加立场与寿命特性

            //资质均衡
            //npc[551] = "2";
            //npc[651] = "2";
            Characters.SetCharProperty(id, 551, Characters.GetCharProperty(DateFile.instance.mianActorId, 551));
            Characters.SetCharProperty(id, 651, Characters.GetCharProperty(DateFile.instance.mianActorId, 651));

            //挑选最出众的资质
            int yiID = 501;
            for (int i = 0; i < 16; i++)
            {
                /*
                if (Int32.Parse(npc[yiID]) < Int32.Parse(npc[501 + i]))
                {
                    yiID = 501 + i;
                }
                */
                if (Int32.Parse(Characters.GetCharProperty(id, yiID)) < Int32.Parse(Characters.GetCharProperty(id, 501 + i)))
                {
                    yiID = 501 + i;
                }
            }
            int wuID = 604;
            for (int i = 0; i < 11; i++)
            {
                /*
                if (Int32.Parse(npc[wuID]) < Int32.Parse(npc[604 + i]))
                {
                    wuID = 604 + i;
                }
                */
                if (Int32.Parse(Characters.GetCharProperty(id, wuID)) < Int32.Parse(Characters.GetCharProperty(id, 604 + i)))
                {
                    wuID = 604 + i;
                }
            }

            //3.【精于一道·艺】化全身资质于一道，大幅增强一项技艺的资质
                //求其他技艺资质的平均值
                int jiyiAver = 0;
                for (int i = 0; i < 16; i++)
                {
                    if (yiID != 501 + i)
                    {
                        //jiyiAver += Int32.Parse(npc[501 + i]);
                        jiyiAver += Int32.Parse(Characters.GetCharProperty(id, 501 + i));
                    }
                }
                jiyiAver /= 15;
                //求武学资质平均值
                int wuxueAver = 0;
                for (int i = 0; i < 14; i++)
                {
                    //wuxueAver += Int32.Parse(npc[601 + i]);
                    wuxueAver += Int32.Parse(Characters.GetCharProperty(id, 601 + i));
                }
                wuxueAver /= 13;
                //增强的技艺资质 = 原技艺资质 + jiyiAver * 0.3 + wuxueAver * 0.2;
                Characters.SetCharProperty(id, yiID, ((int)(Int32.Parse(Characters.GetCharProperty(id, yiID)) + jiyiAver * 0.4 + wuxueAver * 0.3)).ToString());
                //npc[yiID] = ((int)(Int32.Parse(npc[yiID]) + jiyiAver * 0.4 + wuxueAver * 0.3)).ToString();
                //npc[101] += "|" + (3507 + yiID).ToString(); //添加资质
                Characters.SetCharProperty(id, 101, Characters.GetCharProperty(id, 101) + "|" + (3507 + yiID).ToString()); //添加资质
            

            //4.【精于一道·武】化全身资质于一道，大幅增强一项进攻武学的资质，中幅增强内功身法绝技的资质
                jiyiAver /= 15;
                //增强的武学资质 = 原武学资质 + wuxueAver * 0.3 + jiyiAver * 0.2;
                //npc[wuID] = ((int)(Int32.Parse(npc[wuID]) + wuxueAver * 0.4 + jiyiAver * 0.3)).ToString();
                Characters.SetCharProperty(id, wuID, ((int)(Int32.Parse(Characters.GetCharProperty(id, wuID)) + wuxueAver * 0.4 + jiyiAver * 0.3)).ToString());
                //npc[wuID] = ((int)(Int32.Parse(npc[wuID]) + wuxueAver * 0.4 + jiyiAver * 0.3)).ToString();
                //中幅增强内功身法绝技
                string shuxing = ((int) (Int32.Parse(Characters.GetCharProperty(id, 601)) + wuxueAver * 0.2 + jiyiAver * 0.2)).ToString();
                //npc[601] = ((int)(Int32.Parse(npc[601]) + wuxueAver * 0.2 + jiyiAver * 0.2)).ToString();
                //npc[602] = ((int)(Int32.Parse(npc[601]) + wuxueAver * 0.2 + jiyiAver * 0.2)).ToString();
                //npc[603] = ((int)(Int32.Parse(npc[601]) + wuxueAver * 0.2 + jiyiAver * 0.2)).ToString();
                Characters.SetCharProperty(id, 601, shuxing);
                Characters.SetCharProperty(id, 602, shuxing);
                Characters.SetCharProperty(id, 603, shuxing);
                Characters.SetCharProperty(id, 101, Characters.GetCharProperty(id, 101) + "|" + (3420 + wuID).ToString()); //添加资质
                                                                                                                           //npc[101] += "|" + (3420 + wuID).ToString(); //添加资质


            //DateFile.instance.ActorFeaturesCacheReset(id); //刷新特性
            DateFile.instance.ActorFeaturesCacheReset(); //刷新特性

            //工作服 73703 劲衣 工作车 83503 下泽车
            DateFile.instance.SetActorEquip(id, 305, DateFile.instance.MakeNewItem(73703));
            DateFile.instance.SetActorEquip(id, 305, DateFile.instance.MakeNewItem(83503));
            //npc[305] = DateFile.instance.MakeNewItem(73703).ToString();
            //npc[311] = DateFile.instance.MakeNewItem(83503).ToString();
        }

        /// <summary>
        /// 注入新特性，占用特性表4006-4034
        /// </summary>
        public static void AddAllFeature()
        {
            //志同道合
            AddNewFeature(4006, "<color=#8FBAE7FF>志同道合</color>", "<color=#EFE38EFF>因与太吾传人立场一致，愿意成为太吾的家仆</color>", "0", "1", "1|1", "4006");
            //龙神赐寿
            AddNewFeature(4007, "<color=#F28234FF>龙神赐寿</color>", "<color=#EFE38EFF>每个龙岛忠仆都被龙神赋予更长的阳寿，终生侍奉主人</color>", "0", "3|3|3", "0", "4007");
            //精于一道·艺
            String[] yiWrod = new string[] { "音律", "弈棋", "诗书", "绘画", "术数", "品鉴", "锻造", "制木", "医术", "毒术", "织锦", "巧匠", "道法", "佛学", "厨艺", "杂学" };
            for (int i = 4008; i < 4024; i++)
            {
                AddNewFeature(i, "<color=#E4504DFF>天资·" + yiWrod[i - 4008] + "</color>", "<color=#EFE38EFF>天生对</color><color=#E4504DFF>" + yiWrod[i - 4008] + "</color><color=#EFE38EFF>拥有异样的体悟，决定精于此道</color>", "0", "0", "1|1|1", "4008");
            }

            //精于一道·武
            String[] wuWrod = new string[] { "拳掌", "指法", "腿法", "暗器", "剑法", "刀法", "长兵", "奇门", "软兵", "御射", "乐器" };
            for (int i = 4024; i < 4035; i++)
            {
                AddNewFeature(i, "<color=#E4504DFF>天资·" + wuWrod[i - 4024] + "</color>", "<color=#EFE38EFF>天生对</color><color=#E4504DFF>" + wuWrod[i - 4024] + "</color><color=#EFE38EFF>拥有异样的体悟，决定精于此道</color>", "1|1|1", "0", "0", "4008");
            }
        }

        /// <summary>
        /// 向特性表中添加特性
        /// </summary>
        /// <param name="featureID">特性id</param>
        /// <param name="featureName">特性名称</param>
        /// <param name="featureDisc">特性描述</param>
        /// <param name="zhanDou">战斗点</param>
        /// <param name="fangYu">防御点</param>
        /// <param name="jiLue">机略点</param>
        /// <param name="zu">所属组</param>
        public static void AddNewFeature(int featureID, string featureName, string featureDisc, string zhanDou, string fangYu, string jiLue, string zu)
        {
            DateFile.instance.actorFeaturesDate[featureID] = new Dictionary<int, string>();
            foreach (KeyValuePair<int, string> kv in DateFile.instance.actorFeaturesDate[0])
            {
                DateFile.instance.actorFeaturesDate[featureID][kv.Key] = kv.Value;
            }
            DateFile.instance.actorFeaturesDate[featureID][0] = featureName;
            DateFile.instance.actorFeaturesDate[featureID][99] = featureDisc;
            DateFile.instance.actorFeaturesDate[featureID][1] = zhanDou;
            DateFile.instance.actorFeaturesDate[featureID][2] = fangYu;
            DateFile.instance.actorFeaturesDate[featureID][3] = jiLue;
            DateFile.instance.actorFeaturesDate[featureID][5] = zu;
        }

        /*
        //debug遍历输出Dictionary<int, Dictionary<int, string>>
        public static void debugLogIntIntString(Dictionary<int, Dictionary<int, string>> dic, bool savelog)
        {
            String logText = "";
            int tmpnum = 0;
            foreach (KeyValuePair<int, Dictionary<int, string>> e in dic)
            {
                logText += "\nkey:" + e.Key + " value: ";
                foreach (KeyValuePair<int, string> kv in e.Value)
                {
                    logText += kv.Value + ",";
                }
                tmpnum++;
                if (tmpnum > 10000)
                {
                    break;
                }
            }
            if (savelog) saveLog(logText);
            else logger.Log(logText);
        }

        //debug遍历输出List<int>
        public static void debugLogListInt(List<int> list)
        {
            String logText = "";
            for (int i = 0; i < list.Count; i++)
            {
                logText += list[i] + ",";
            }
            logger.Log(logText);
        }

        //保存日志到log目录
        public static void saveLog(string logtext)
        {
            FileStream fs = new FileStream(logPath, FileMode.Create);
            byte[] logdata = System.Text.Encoding.Default.GetBytes(logtext);
            fs.Write(logdata, 0, logdata.Length);
            fs.Flush();
            fs.Close();
        }
        */
    }
}
