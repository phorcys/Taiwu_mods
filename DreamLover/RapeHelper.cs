using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamLover
{
    public static class RapeHelper
    {
        public static bool Rape(int raperId, int victimId, int mapId, int tileId, bool skipBattle = false, bool moodChange = true, bool beHated = true, bool oneParent = true)
        {
			int BattleAbility = int.Parse(DateFile.instance.GetActorDate(raperId, 993, applyBonus: false));
			if (!skipBattle && BattleAbility < int.Parse(DateFile.instance.GetActorDate(victimId, 993, applyBonus: false)) + 10000)
			{
				if(beHated) DateFile.instance.AddSocial(victimId, raperId, 401);
				if(moodChange) PeopleLifeAIHelper.AiMoodChange(raperId, int.Parse(DateFile.instance.goodnessDate[DateFile.instance.GetActorGoodness(raperId)][102]));
				PeopleLifeAIHelper.AISetMassage(99, raperId, mapId, tileId, new int[1], victimId);
				return false;
			}

			if (moodChange) PeopleLifeAIHelper.AiMoodChange(raperId, int.Parse(DateFile.instance.goodnessDate[DateFile.instance.GetActorGoodness(raperId)][102]) * 10);

			if (DateFile.instance.GetActorSocial(victimId, 312).Contains(raperId))
			{
				if (moodChange) PeopleLifeAIHelper.AiMoodChange(victimId, UnityEngine.Random.Range(-10, 11));
				if (UnityEngine.Random.Range(0, 100) < 50)
				{
					if (beHated) DateFile.instance.AddSocial(victimId, raperId, 402);
				}
				PeopleLifeAIHelper.AISetMassage(97, victimId, mapId, tileId, new int[1], raperId);
			}
			else
			{
				if (moodChange) PeopleLifeAIHelper.AiMoodChange(victimId, -50);
				if (beHated) DateFile.instance.AddSocial(victimId, raperId, 402);
				PeopleLifeAIHelper.AISetMassage(96, victimId, mapId, tileId, new int[1], raperId);
			}

			int RaperSex = int.Parse(DateFile.instance.GetActorDate(raperId, 14, applyBonus: false));

			if (RaperSex != int.Parse(DateFile.instance.GetActorDate(victimId, 14, applyBonus: false)))
			{
				int setFather = (RaperSex != 1) ? 1 : 0;
				int setMother = (RaperSex == 1) ? 1 : 0;
				if(!oneParent)
					setFather = setMother = 1;
				
				PeopleLifeAIHelper.AISetChildren((RaperSex == 1) ? raperId : victimId, (RaperSex == 1) ? victimId : raperId, setFather, setMother);
			}
			else
			{
				DateFile.instance.ChangeActorFeature(raperId, 4001, 4002);
				DateFile.instance.ChangeActorFeature(victimId, 4001, 4002);
				GEvent.OnEvent(eEvents.Copulate, raperId, victimId);
			}

			return true;
		}
    }
}
