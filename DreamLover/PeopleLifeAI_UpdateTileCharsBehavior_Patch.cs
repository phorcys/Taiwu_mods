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
		private static void Debug(string str)
		{
			Main.Debug("<主动欺辱> " + str);
		}
		public static bool Prefix(int mapId, int tileId, bool isTaiwuAtThisTile, Dictionary<int, int> righteousInfo, object disasterInfo, int worldId, int mainActorId, Dictionary<int, List<int>> mainActorItems, System.Random random)
		{
			if (!Main.enabled || !Main.settings.rape.autorape.Enabled)
			{
				return true;
			}

			if (!isTaiwuAtThisTile)
			{
				return true;
			}

			Debug("开始寻找目标");

			int 角色立场 = DateFile.instance.GetActorGoodness(mainActorId);
			int 欺辱概率 = int.Parse(DateFile.instance.goodnessDate[角色立场][25]);
			int 战力评价 = int.Parse(DateFile.instance.GetActorDate(mainActorId, 993, applyBonus: false));
			int 性别 = int.Parse(DateFile.instance.GetActorDate(mainActorId, 14, applyBonus: false));

			PeopleLifeAIHelper.GetTileCharacters(mapId, tileId, out int[] aliveChars);
			List<int> list = aliveChars.ToList();

			if (Main.settings.rape.autorape.JustLover)
			{
				list = list.Where((int id) => DateFile.instance.GetActorSocial(mainActorId, 312).Contains(id)).ToList();
			}
			if (Main.settings.rape.autorape.FilterName)
			{
				list = list.Where((int id) => DateFile.instance.GetActorName(id).IndexOf(Main.settings.rape.autorape.Name) != -1).ToList();
			}
			if (Main.settings.rape.autorape.DifferentSex)
			{
				list = list.Where((int id) => int.Parse(DateFile.instance.GetActorDate(id, 14, applyBonus: false)) != 性别).ToList();
			}

			string names = "";
			foreach(int kid in list) 
				names += DateFile.instance.GetActorName(kid) + " ";

			Debug("欺辱目标名单" + ((list.Count == 0) ? "为空" : (": " + names)));

			if (Main.settings.rape.autorape.SpecifiedPossibility)
			{
				欺辱概率 = Main.settings.rape.autorape.Possibility;
			}
			int var1 = UnityEngine.Random.Range(0, 100);
			if (list.Count > 0 && var1 < 欺辱概率)
			{
				Debug("欺辱概率判定通过，需求 " + var1 + "，结果 " + 欺辱概率);
				int targetID = list[UnityEngine.Random.Range(0, list.Count)];
				bool r = RapeHelper.Rape(mainActorId, targetID, mapId, tileId, Main.settings.rape.skipBattle, Main.settings.rape.moodChange, Main.settings.rape.beHated, Main.settings.rape.oneParent);
			}
			else
			{
				Debug("欺辱概率判定未通过，需求 " + var1 + "，结果 " + 欺辱概率);
			}
			return true;
		}
	}
}
