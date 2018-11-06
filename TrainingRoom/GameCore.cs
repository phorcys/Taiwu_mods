using System.Collections.Generic;

namespace TrainingRoom
{
    class GameCore
    {
        private static List<int> registerEvent = new List<int>();
        private static List<int> registerEnemyTeam = new List<int>();
        private static int storeGangId = 0;
        private static string data812Backup = "";

        internal static void SetGangGroupDate812(int gangId, string eventId)
        {
            storeGangId = gangId;
            data812Backup = DateFile.instance.presetGangGroupDateValue[gangId][812];
            DateFile.instance.presetGangGroupDateValue[gangId][812] = data812Backup == "0" ? eventId : data812Backup + "|" + eventId;
        }

        public static void AddNewEvent(
            int key,
            string message,
            string variables = "1",
            string requirement = "",
            string choices = "",
            string process = "",
            int nextEvent = -1,
            bool blackMask = false,
            int allBlackTime = 0,
            bool inputText = false,
            string remark = "",
            int spoker = -1,
            int backgroundID = 0)
        {
            DateFile.instance.eventDate.Add(key, new Dictionary<int, string>(){
                { 0, remark},
                { 1, backgroundID.ToString()},
                { 2, spoker.ToString()}, // 前境人物ID: -1=主角, 0=任務對象, -99=無
                { 3, message},
                { 4, variables}, //  請參考 MassageWindow.ChangeText,  
                { 5, choices},
                { 6, requirement}, // 請參考 MassageWindow.GetEventBooty
                { 7, nextEvent.ToString()},
                { 8, process},
                { 9, blackMask?"1":"0"},
                { 10, allBlackTime>0?allBlackTime.ToString():""},
                { 11, inputText ? "1" : "0"}
            });
            if (DateFile.instance.eventDate[key][5] == "") DateFile.instance.eventDate[key][5] = "0";
            registerEvent.Add(key);
            //Main.Logger.Log("registerEvent Added:" + key.ToString() );
            //Main.Logger.Log( DateFile.instance.eventDate[key].Select(x => String.Format("{0}:{1}", x.Key, x.Value)).ToArray().Join());
        }

        public static void AddEnemyTeam(
            int key,
            bool randomEnemyIds = true, // 0=自訂隊伍中的同道 ,1=隨機
            int defendRound = 0, // 試招 0, 5 接招5回合
            int difficulty = 10, // 難度 0~10 ; 0=按幫派地位算;10=最強,1=最弱
            bool isSwordSpirit = false, // 0, 1 屬於劍冢, 有精碎
            int xxPoint = 0, // 精粹 0~22
            int battleGetExp = 0, // 0, 50, 100, 150, 300, 500, 1500
            string enemyIds = "0", // 敵人 ID 可參考 PresetActor
            string eIdsMinMax = "0|0", // 敵人同道出場數量(最少/最大): 0|0, 1|1, 1|2, 1|3, 5|5
            string otherEIds = "0", // 敵人同道 IDs 可參考 PresetActor
            int eldsAppearRate = 100, // 敵人同道出場率 0, 35, 50, 100
            int dropRate = 40, // 掉寶率%: 0, 20, 40, 80, 200, 1000 
            int healRate = 0, // 失心人救治率% 0, 100
            int escapeAppraise = 3, // 逃走信息 0=不能逃走, 1=不分勝負, 3=落荒而逃。
            bool changeEnemyObbs = true, // 0, 1 額外EXP
            int hatingRate = 100, // 仇恨度 0, 50, 100, 150, 200
            bool enemyUseItem = true, // 敵人開戰時使用道具 0, 1
            bool Column_13 = true, // 這個不知道,程式碼內沒有使用  0, 1
            int changeMapTyp = -1, // 改變地圖類型 all -1=不改
            bool isDeadMode = true, // 死鬥模式 50=不是, 100=死鬥
            int initDistance = 0, // 初始距離  0=不改, 20, 50, 70
            int bgMusic = 3, // 1, 2, 3
            string nextEventWin = "0", //战斗结束事件for惡戰  
            string nextEventLoss = "0", //战斗结束事件for切磋
            string nextEventEscape = "0"  //战斗结束事件for接招
        )
        {
            DateFile.instance.enemyTeamDate.Add(key, new Dictionary<int, string>(){
                { 99, randomEnemyIds? "1" : "0"},
                { 6, defendRound.ToString()},
                { 7, difficulty.ToString()},
                { 15, isSwordSpirit? "1" : "0"},
                { 17, xxPoint.ToString()},
                { 16, battleGetExp.ToString()},
                { 1, enemyIds  },
                { 12, eIdsMinMax },
                { 2, otherEIds },
                { 10, eldsAppearRate.ToString()},
                { 3, dropRate.ToString()},
                { 4, healRate.ToString()},
                { 5, escapeAppraise.ToString()},
                { 8, changeEnemyObbs? "1" : "0"},
                { 9, hatingRate.ToString()},
                { 11, enemyUseItem? "1" : "0"},
                { 13, Column_13? "1" : "0"},
                { 14, changeMapTyp.ToString()},
                { 23, isDeadMode? "100" : "50"},
                { 97, initDistance.ToString()},
                { 98, bgMusic.ToString()},
                { 101, nextEventWin },
                { 102, nextEventLoss },
                { 103, nextEventEscape },
            });
            registerEnemyTeam.Add(key);
            //Main.Logger.Log("registerEnemyTeam Added:" + key.ToString());
        }

        public static void Reset()
        {
            //Main.Logger.Log("eventDate tobe removed:" + registerEvent.Join());
            for (int i = 0; i < registerEvent.Count; i++)
                DateFile.instance.eventDate.Remove(registerEvent[i]);
            registerEvent.Clear();
            //Main.Logger.Log("eventDate after removing:" + DateFile.instance.eventDate.Keys.ToList().Join());

            //Main.Logger.Log("registerEnemyTeam tobe removed:" + registerEvent.Join());
            for (int i = 0; i < registerEnemyTeam.Count; i++)
                DateFile.instance.enemyTeamDate.Remove(registerEnemyTeam[i]);
            registerEnemyTeam.Clear();
            //Main.Logger.Log("registerEnemyTeam after removing:" + DateFile.instance.eventDate.Keys.Join());

            DateFile.instance.presetGangGroupDateValue[storeGangId][812] = data812Backup;

        }

    }
}
