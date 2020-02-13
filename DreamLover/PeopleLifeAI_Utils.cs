using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DreamLover
{
	public static class PeopleLifeAI_Utils
	{
		public static void AiMoodChange(int actorId, int value)
		{
			PeopleLifeAI.instance.CallPrivateMethod<PeopleLifeAI>("AiMoodChange", new object[2]
			{
				actorId,
				value
			});
		}

		public static void AISetOtherLove(int mianActorId, int actorId, int loveId, int partId, int placeId)
		{
			PeopleLifeAI.instance.CallPrivateMethod<PeopleLifeAI>("AISetOtherLove", new object[5]
			{
				mianActorId,
				actorId,
				loveId,
				partId,
				placeId
			});
		}

		public static void AICantMove(int actorId)
		{
			PeopleLifeAI.instance.CallPrivateMethod<PeopleLifeAI>("AICantMove", new object[1]
			{
				actorId
			});
		}

		public static void AISetEvent(int typ, int[] aiEventDate)
		{
			PeopleLifeAI.instance.CallPrivateMethod<PeopleLifeAI>("AISetEvent", new object[2]
			{
				typ,
				aiEventDate
			});
		}

		public static void AISetMassage(int massageId, int actorId, int partId, int placeId, int[] paramValues = null, int otherActorId = -1)
		{
			PeopleLifeAI.instance.AISetMassage(massageId, actorId, partId, placeId, paramValues, otherActorId);
		}

		public static void AISetChildren(int fatherId, int motherId, int setFather, int setMother)
		{
			PeopleLifeAI.instance.AISetChildren(fatherId, motherId, setFather, setMother);
		}

		public static void GetTileCharacters(int mapId, int tileId, out int[] aliveChars)
		{
			HashSet<int> hashSet = new HashSet<int>();
			if (!DateFile.instance.doMapMoveing)
			{
				hashSet.UnionWith(PeopleLifeAI.instance.allFamily);
			}
			List<int> list = DateFile.instance.HaveActor(mapId, tileId, getNormal: true, getDieActor: true, getEnemy: true);
			foreach (int item in list)
			{
				if (int.Parse(DateFile.instance.GetActorDate(item, 8, applyBonus: false)) == 1 && int.Parse(DateFile.instance.GetActorDate(item, 26, applyBonus: false)) == 0)
				{
					hashSet.Add(item);
				}
			}
			aliveChars = hashSet.ToArray();
		}

		public static T CallPrivateMethod<T>(this object instance, string name, params object[] param)
		{
			Debug.Log((object)(((instance == null) ? "null" : instance.ToString()) + " -> " + ((name == null) ? "null" : name) + " -> " + ((param == null) ? "null" : param.ToString())));
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
			Type type = instance.GetType();
			Debug.Log((object)((type == null) ? "null type" : type.ToString()));
			MethodInfo method = type.GetMethod(name, bindingAttr);
			Debug.Log((object)((method == null) ? "null method" : method.ToString()));
			return (T)method.Invoke(instance, param);
		}
	}
}
