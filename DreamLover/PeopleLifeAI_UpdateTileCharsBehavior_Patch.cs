using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DreamLover
{
	[HarmonyPatch(typeof(PeopleLifeAI), "UpdateTileCharsBehavior")]
	public static class PeopleLifeAI_UpdateTileCharsBehavior_Patch
	{
		public static bool Prefix(int mapId, int tileId, bool isTaiwuAtThisTile, Dictionary<int, int> righteousInfo, object disasterInfo, int worldId, int mainActorId, Dictionary<int, List<int>> mainActorItems, System.Random random)
		{
			if (!Main.enabled || !Main.settings.启用主动欺辱功能)
			{
				return true;
			}
			if (!isTaiwuAtThisTile)
			{
				return true;
			}
			int 角色立场 = DateFile.instance.GetActorGoodness(mainActorId);
			int 欺辱概率 = int.Parse(DateFile.instance.goodnessDate[角色立场][25]);
			int 战力评价 = int.Parse(DateFile.instance.GetActorDate(mainActorId, 993, applyBonus: false));
			int 性别 = int.Parse(DateFile.instance.GetActorDate(mainActorId, 14, applyBonus: false));
			PeopleLifeAI_Utils.GetTileCharacters(mapId, tileId, out int[] aliveChars);
			List<int> list = aliveChars.ToList();

			if (Main.settings.主动欺辱爱慕筛选)
			{
				list = list.Where((int id) => DateFile.instance.GetActorSocial(mainActorId, 312).Contains(id)).ToList();
			}
			if (Main.settings.主动欺辱人名筛选)
			{
				list = list.Where((int id) => DateFile.instance.GetActorName(id).IndexOf(Main.settings.人名字符串片段) != -1).ToList();
			}
			if (Main.settings.主动欺辱异性筛选)
			{
				list = list.Where((int id) => int.Parse(DateFile.instance.GetActorDate(id, 14, applyBonus: false)) != 性别).ToList();
			}

			Debug.Log(("目标名单：" + ((list.Count == 0) ? "None" : (DateFile.instance.GetActorName(list[0]) + " etc."))));
			if (Main.settings.指定主动欺辱概率)
			{
				欺辱概率 = Main.settings.主动欺辱概率;
			}
			if (list.Count > 0 && UnityEngine.Random.Range(0, 100) < 欺辱概率)
			{
				int num3 = list[UnityEngine.Random.Range(0, list.Count)];
				if (!Main.settings.跳过战力检定 && 战力评价 < int.Parse(DateFile.instance.GetActorDate(num3, 993, applyBonus: false)) + 10000)
				{
					Debug.Log("欺辱打不过的人");
					if (Main.settings.主动欺辱结仇)
					{
						DateFile.instance.AddSocial(num3, mainActorId, 401);
					}
					if (Main.settings.主动欺辱影响双方情绪)
					{
						PeopleLifeAI_Utils.AiMoodChange(mainActorId, int.Parse(DateFile.instance.goodnessDate[DateFile.instance.GetActorGoodness(mainActorId)][102]));
					}
					PeopleLifeAI_Utils.AISetMassage(99, mainActorId, mapId, tileId, new int[1], num3);
				}
				else
				{
					if (Main.settings.主动欺辱影响双方情绪)
					{
						PeopleLifeAI_Utils.AiMoodChange(mainActorId, int.Parse(DateFile.instance.goodnessDate[DateFile.instance.GetActorGoodness(mainActorId)][102]) * 10);
					}
					if (DateFile.instance.GetActorSocial(num3, 312).Contains(mainActorId))
					{
						Debug.Log("欺辱成功，但是并无过多怨恨");
						if (Main.settings.主动欺辱影响双方情绪)
						{
							PeopleLifeAI_Utils.AiMoodChange(num3, UnityEngine.Random.Range(-10, 11));
						}
						if (UnityEngine.Random.Range(0, 100) < 50 && Main.settings.主动欺辱结仇)
						{
							DateFile.instance.AddSocial(num3, mainActorId, 402);
						}
						PeopleLifeAI_Utils.AISetMassage(97, num3, mapId, tileId, new int[1], mainActorId);
					}
					else
					{
						Debug.Log("欺辱成功，对方伤心欲绝");
						if (Main.settings.主动欺辱影响双方情绪)
						{
							PeopleLifeAI_Utils.AiMoodChange(num3, -50);
						}
						if (Main.settings.主动欺辱结仇)
						{
							DateFile.instance.AddSocial(num3, mainActorId, 402);
						}
						PeopleLifeAI_Utils.AISetMassage(96, num3, mapId, tileId, new int[1], mainActorId);
					}
					if (性别 != int.Parse(DateFile.instance.GetActorDate(num3, 14, applyBonus: false)))
					{
						Debug.Log("试图怀孕");
						PeopleLifeAI_Utils.AISetChildren((性别 == 1) ? mainActorId : num3, (性别 == 1) ? num3 : mainActorId, Main.settings.主动欺辱的孩子有双亲 ? 1 : ((性别 != 1) ? 1 : 0), Main.settings.主动欺辱的孩子有双亲 ? 1 : ((性别 == 1) ? 1 : 0));
					}
					else
					{
						Debug.Log("同性不能怀孕");
						DateFile.instance.ChangeActorFeature(mainActorId, 4001, 4002);
						DateFile.instance.ChangeActorFeature(num3, 4001, 4002);
						GEvent.OnEvent(eEvents.Copulate, mainActorId, num3);
					}
				}
			}
			return true;
		}
	}
}
