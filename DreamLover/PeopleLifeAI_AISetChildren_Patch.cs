using GameData;
using Harmony12;
using System.Collections.Generic;
using UnityEngine;

namespace DreamLover
{
	[HarmonyPatch(typeof(PeopleLifeAI), "AISetChildren")]
	public static class PeopleLifeAI_AISetChildren_Patch
	{
		public static bool Prefix(int fatherId, int motherId, int setFather, int setMother, ref bool __result)
		{
			if (!Main.enabled)
			{
				return true;
			}
			if (!Main.settings.启用指定怀孕功能)
			{
				return true;
			}
			int num = DateFile.instance.MianActorID();
			if (fatherId != num && motherId != num)
			{
				return true;
			}
			Debug.Log((object)"PrecisionShooting...");
			int num2 = int.Parse(DateFile.instance.GetActorDate(fatherId, 24));
			int num3 = int.Parse(DateFile.instance.GetActorDate(motherId, 24));
			DateFile.instance.ChangeActorFeature(fatherId, 4001, 4002);
			DateFile.instance.ChangeActorFeature(motherId, 4001, 4002);
			GEvent.OnEvent(eEvents.Copulate, fatherId, motherId);
			if (Main.settings.指定生育可能性 ? (Main.settings.生育可能性 <= 0) : (num2 <= 0 || num3 <= 0))
			{
				__result = false;
				return false;
			}
			if (int.Parse(DateFile.instance.GetActorDate(motherId, 14, applyBonus: false)) != 2)
			{
				__result = false;
				return false;
			}
			if (!DateFile.instance.HaveLifeDate(motherId, 901) && Random.Range(0, 15000) < (Main.settings.指定生育可能性 ? Main.settings.生育可能性 : (num2 * num3)))
			{
				Debug.Log((object)"生育可能性判定成功");
				bool flag = fatherId == num || motherId == num;
				int num4 = 100;
				int num5 = flag ? 20 : 50;
				num4 -= DateFile.instance.GetActorSocial(fatherId, 310).Count * num5;
				num4 -= DateFile.instance.GetActorSocial(motherId, 310).Count * num5;
				if (Random.Range(0, 100) < (Main.settings.指定怀孕概率 ? Main.settings.怀孕概率 : num4))
				{
					Debug.Log((object)"怀孕判定成功");
					DateFile.instance.ChangeActorFeature(motherId, 4002, 4003);
					if (Random.Range(0, 100) < (Main.settings.指定蛐蛐概率 ? Main.settings.蛐蛐概率 : ((DateFile.instance.getQuquTrun - 100) / 10)))
					{
						Debug.Log((object)"异胎判定成功");
						DateFile.instance.getQuquTrun = 0;
						DateFile.instance.actorLife[motherId].Add(901, new List<int>
						{
							1042,
							fatherId,
							motherId,
							setFather,
							setMother
						});
					}
					else
					{
						Debug.Log((object)"常胎判定成功");
						DateFile.instance.actorLife[motherId].Add(901, new List<int>
						{
							Random.Range(7, 10),
							fatherId,
							motherId,
							setFather,
							setMother
						});
						DateFile.instance.pregnantFeature.Add(motherId, new string[2]
						{
							Characters.GetCharProperty(fatherId, 101),
							Characters.GetCharProperty(motherId, 101)
						});
					}
					__result = true;
				}
			}
			return false;
		}
	}
}
