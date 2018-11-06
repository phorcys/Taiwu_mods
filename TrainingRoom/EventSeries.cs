using System;

namespace TrainingRoom
{
    public static class EventSeries
    {

        public static void Series1(int gangId)
        {
            GameCore.Reset();

            int offset = 90000000;

            GameCore.SetGangGroupDate812(gangId, offset.ToString());

            GameCore.AddNewEvent(offset, "假扮相枢……", variables: "0", requirement: "FA&6|TIME&5|ATTMIN&406&1000|ATTMIN&407&1000", nextEvent: offset + 1);

            string eventChoices = "", comma = "";
            for (int enemyId = 2001; enemyId <= 2009; enemyId++)
            {
                eventChoices+=string.Format("{0}{1}",comma,offset + enemyId);
                comma = "|";
                GameCore.AddNewEvent(offset + enemyId, "请助我一臂之力！我想跟·" + DateFile.instance.GetActorName(enemyId, false, false) + " 比试……\n<color=#4B4B4BFF>（消耗银钱：1000；消耗威望：1000；消耗时间：5）</color>",  process: "TIME&5|RES&5&-1000|RES&6&-1000");
                GameCore.AddNewEvent(offset + enemyId + 100, "好吧!·我就奉陪到底......\n(说罢, D0妆扮成" + DateFile.instance.GetActorName(enemyId, false, false) + "的模样)", choices: ( offset + enemyId + 200 ).ToString() );
                GameCore.AddNewEvent(offset + enemyId + 200, "即管放马过来~", process: string.Format("BAT&{0}&0", offset + enemyId));
                GameCore.AddEnemyTeam(offset + enemyId, battleGetExp: 300, enemyIds: enemyId.ToString(), eIdsMinMax: "2|4", randomEnemyIds: false, otherEIds: "6001|6002|6003|6004|6005", nextEventWin: string.Format("{0}&1", offset + 2), nextEventLoss: string.Format("{0}&1", offset + 3), nextEventEscape: string.Format("{0}&1", offset + 4), difficulty: Math.Min(DateFile.instance.GetWorldXXLevel(false), 3)); 
            }
            eventChoices += string.Format("{0}{1}", comma, 900700001);

            GameCore.AddNewEvent(offset + 1, "要用我的功力来模拟相枢化身？不会有危险吧……", choices: eventChoices);

            GameCore.AddNewEvent(offset + 2, "MN拼死将D0幻化的相枢击败！\nD0 随之回复原貎，幸好未有受伤。嘻嘻对着MN笑说：“不错，你果然成长不少！”", nextEvent: 912800001);
            GameCore.AddNewEvent(offset + 3, "MN被D0幻化的相枢击中要害，倒在地上！\nD0 随後回复原貎，欣勤地为 MN 治疗。却叹道：“你如此不济，应当勤加苦练！”", nextEvent: 912800001);
            GameCore.AddNewEvent(offset + 4, "MN成功从D0幻化的相枢试练逃脱！D0见MN落荒而逃，却并不追赶，只立在原地，似乎在等MN自己回来再斗……", nextEvent: 912800001);

        }


        public static void Series2(int gangId)
        {

            GameCore.Reset();

            int offset = 90000000;

            GameCore.SetGangGroupDate812(gangId, offset.ToString());

            GameCore.AddNewEvent(offset, "  剑冢再临……", requirement: "FA&6|TIME&20|ATTMAX&406&4000|ATTMAX&407&4000", nextEvent: offset + 1);

            string eventChoices = "", comma = "";
            for (int enemyId = 2001; enemyId <= 2009; enemyId++)
            {
                eventChoices += string.Format("{0}{1}", comma, offset + enemyId);
                comma = "|";
                GameCore.AddNewEvent(offset + enemyId, "我自有分寸！（挑战·" + DateFile.instance.GetActorName(enemyId, false, false) + "……）\n<color=#4B4B4BFF>（副本消耗银钱：4000；消耗威望：4000；消耗时间：20）</color>", variables:"1", process: "TIME&20|RES&5&-4000|RES&6&-4000", nextEvent: offset + enemyId + 100);
                GameCore.AddNewEvent(offset + enemyId + 100, DateFile.instance.eventDate[enemyId - 2001 + 147][3], choices: (offset + enemyId + 200).ToString(), spoker: enemyId);
                GameCore.AddNewEvent(offset + enemyId + 200, "（拼死一战！）", process: string.Format("BAT&{0}&0", offset + enemyId));
                GameCore.AddEnemyTeam(offset + enemyId, battleGetExp: 300, enemyIds: enemyId.ToString(), eIdsMinMax: "3|5", randomEnemyIds: false, otherEIds: "6005|6006|6007|6008|6009", nextEventWin: "176&1", nextEventLoss: "185&1", nextEventEscape: "186&1", difficulty: 1+Math.Min(DateFile.instance.GetWorldXXLevel(false), 6));
            }
            eventChoices += string.Format("{0}{1}", comma, 900700001);
            GameCore.AddNewEvent(offset + 1, "什么，你还想去那么可怕的地方？", choices: eventChoices, spoker: 0);
            //Main.Logger.Log(String.Join("|", eventChoices.Select(i => i.ToString()).ToArray()));
        }


        public static void Series3(int gangId)
        {
            GameCore.Reset();

            int offset = 90000000;
            string newEvents = string.Format("{0}|{1}", offset, offset + 100);// 添加 清理宵小&剿灭邪道
            GameCore.SetGangGroupDate812(gangId, newEvents);

            GameCore.AddNewEvent(offset, "清理宵小……", requirement: "FA&4|TIME&15|ATTMAX&406&100|ATTMAX&407&100", nextEvent: offset +1);
            GameCore.AddNewEvent(offset+1, "最近乡里又有宵小横行霸道，还请大侠代为清扫。", choices: string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", offset + 2 , offset + 3 , offset + 4 , offset + 5 , offset + 6 , offset + 7, 900700001));
            GameCore.AddNewEvent(offset+2, "还请为我指点道路！（清扫·恶丐窝点……）\n<color=#4B4B4BFF>（副本消耗银钱：100；消耗威望：100；消耗时间：15）</color>", process: "TIME&15|RES&5&-100|RES&6&-100", nextEvent:2001);
            GameCore.AddNewEvent(offset+3, "还请为我指点道路！（清扫·贼人营寨……）\n<color=#4B4B4BFF>（副本消耗银钱：200；消耗威望：200；消耗时间：15）</color>", process: "TIME&15|RES&5&-200|RES&6&-200", nextEvent:2002);
            GameCore.AddNewEvent(offset+4, "还请为我指点道路！（清扫·悍匪山砦……）\n<color=#4B4B4BFF>（副本消耗银钱：300；消耗威望：300；消耗时间：15）</color>", process: "TIME&15|RES&5&-300|RES&6&-300", nextEvent:2003);
            GameCore.AddNewEvent(offset+5, "还请为我指点道路！（清扫·叛徒结伙……）\n<color=#4B4B4BFF>（副本消耗银钱：400；消耗威望：400；消耗时间：15）</color>", process: "TIME&15|RES&5&-400|RES&6&-400", nextEvent:2004);
            GameCore.AddNewEvent(offset+6, "还请为我指点道路！（清扫·恶人深谷……）\n<color=#4B4B4BFF>（副本消耗银钱：600；消耗威望：600；消耗时间：15）</color>", process: "TIME&15|RES&5&-600|RES&6&-600", nextEvent:2005);
            GameCore.AddNewEvent(offset+7, "还请为我指点道路！（清扫·迷香幻阵……）\n<color=#4B4B4BFF>（副本消耗银钱：800；消耗威望：800；消耗时间：15）</color>", process: "TIME&15|RES&5&-800|RES&6&-800", nextEvent:2006);

            offset = 90000100;
            GameCore.AddNewEvent(offset, "剿灭邪道……", requirement: "FA&4|TIME&15|ATTMAX&406&1000|ATTMAX&407&1000", nextEvent: offset+1);
            GameCore.AddNewEvent(offset + 1, "最近总有邪魔外道蠢蠢欲动，还请大侠代为清扫。", choices: string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", offset + 2, offset + 3, offset + 4, offset + 5, offset + 6, offset + 7, 900700001));
            GameCore.AddNewEvent(offset + 2, "还请为我指点道路！（清扫·乱葬坟岗……）\n<color=#4B4B4BFF>（副本消耗银钱：1000；消耗威望：1000；消耗时间：15）</color>", process: "TIME&15|RES&5&-1000|RES&6&-1000", nextEvent: 2007);
            GameCore.AddNewEvent(offset + 3, "还请为我指点道路！（清扫·异士居所……）\n<color=#4B4B4BFF>（副本消耗银钱：1200；消耗威望：1200；消耗时间：15）</color>", process: "TIME&15|RES&5&-1200|RES&6&-1200", nextEvent: 2008);
            GameCore.AddNewEvent(offset + 4, "还请为我指点道路！（清扫·邪人死地……）\n<color=#4B4B4BFF>（副本消耗银钱：1400；消耗威望：1400；消耗时间：15）</color>", process: "TIME&15|RES&5&-1400|RES&6&-1400", nextEvent: 2009);
            GameCore.AddNewEvent(offset + 5, "还请为我指点道路！（清扫·修罗道场……）\n<color=#4B4B4BFF>（副本消耗银钱：1600；消耗威望：1600；消耗时间：15）</color>", process: "TIME&15|RES&5&-1600|RES&6&-1600", nextEvent: 2010);
            GameCore.AddNewEvent(offset + 6, "还请为我指点道路！（清扫·群魔乱舞……）\n<color=#4B4B4BFF>（副本消耗银钱：1800；消耗威望：1800；消耗时间：15）</color>", process: "TIME&15|RES&5&-1800|RES&6&-1800", nextEvent: 2011);
            GameCore.AddNewEvent(offset + 7, "还请为我指点道路！（清扫·弃世绝境……）\n<color=#4B4B4BFF>（副本消耗银钱：2000；消耗威望：2000；消耗时间：15）</color>", process: "TIME&15|RES&5&-2000|RES&6&-2000", nextEvent: 2012);

        }

    }
}
