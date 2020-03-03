using GameData;
using UnityEngine;
using System.Collections.Generic;

namespace DreamLover
{
	public static class DateFileHelper
	{
		public static bool HasSocial(int actorId, int socialTyp, int targetId, bool getDieActor = false, bool getNpc = false) => DateFile.instance.GetActorSocial(actorId, socialTyp, getDieActor, getNpc).Contains(targetId);

		public static bool HasAnySocial(int actorId, IEnumerable<int> socialTypList, int targetId, bool getDieActor = false, bool getNpc = false)
		{
			foreach(int t in socialTypList)
				if(HasSocial(actorId, t, targetId, getDieActor, getNpc))
					return true;
			return false;
		}
	}
}
