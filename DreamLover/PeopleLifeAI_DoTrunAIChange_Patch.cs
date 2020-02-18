using Harmony12;
using System.Collections.Generic;
using UnityEngine;

namespace DreamLover
{
	[HarmonyPatch(typeof(PeopleLifeAI), "DoTrunAIChange")]
	public static class PeopleLifeAI_DoTrunAIChange_Patch
	{
		private static void Debug(string str)
		{
			Main.Debug("<主动追求> " + str);
		}

		private static bool Prefix(int actorId, int mapId, int tileId, int mainActorId, bool isTaiwuAtThisTile, int worldId, Dictionary<int, List<int>> mainActorItems, int[] aliveChars, int[] deadChars)
		{
			if (!Main.enabled)
			{
				return true;
			}

			do
			{
				if (!Main.settings.belove.ForgetMe)
					break;
				// 如果没有任何一种爱慕之情则结束
				if (!DateFile.instance.GetActorSocial(actorId, 306).Contains(mainActorId) && !DateFile.instance.GetActorSocial(actorId, 312).Contains(mainActorId))
					break;
				Debug(actorId.ToString() + " " + DateFile.instance.GetActorName(actorId) + "开始进入雨恨云愁判定");
				// 如果已经结婚，则无法通过
				if (DateFile.instance.GetActorSocial(actorId, 309).Contains(mainActorId) || DateFile.instance.GetActorSocial(mainActorId, 309).Contains(actorId))
					break;
				// 如果太吾对对方两情相悦，则无法通过
				if (DateFile.instance.GetActorSocial(mainActorId, 306).Contains(actorId))
					break;
				// 如果太吾爱慕对方，则无法通过
				if (DateFile.instance.GetActorSocial(mainActorId, 312).Contains(actorId))
					break;
				// 对方对太吾有某种爱慕（倾心爱慕 或 两情相悦

				Debug(actorId.ToString() + " " + DateFile.instance.GetActorName(actorId) + "雨恨云愁判定通过");

				DateFile.instance.RemoveActorSocial(actorId, mainActorId, 306);
				DateFile.instance.RemoveActorSocial(actorId, mainActorId, 312);
				PeopleLifeAIHelper.AiMoodChange(actorId, -20);
				PeopleLifeAIHelper.AISetMassage(42, actorId, mapId, tileId, new int[1], mainActorId);
				PeopleLifeAI.instance.aiTrunEvents.Add(new int[4]
				{
					229,
					mapId,
					tileId,
					actorId
				});

			} while (false);

			if (!Main.settings.belove.Enabled)
			{
				return true;
			}
			if (!Main.settings.belove.IgnoreDistance && !isTaiwuAtThisTile)
			{
				return true;
			}

			

			int sex = int.Parse(DateFile.instance.GetActorDate(actorId, 14, applyBonus: false)) % 2;
			if (!Main.settings.belove.SexFilter[sex])
			{
				return true;
			}

			Debug(actorId.ToString() + " " + DateFile.instance.GetActorName(actorId) + "开始进入判定过程");

			int actorFavor = DateFile.instance.GetActorFavor(false, mainActorId, actorId, getLevel: true);
			if (TextConvertHelper.LikabilityKey.TryGetIndex(actorFavor, out int index) && !Main.settings.belove.AcceptedLikability[index])
			{
				Debug(string.Format("好感判定失败，好感度为：{0} {1}", actorFavor, TextConvertHelper.LikabilityText[index]));
				return true;
			}


			int AgeValue = int.Parse(DateFile.instance.GetActorDate(actorId, 11, applyBonus: false));
			if (AgeValue < Main.settings.belove.AcceptedAge.start || AgeValue > Main.settings.belove.AcceptedAge.end)
			{
				Debug(string.Format("年龄判定失败，年龄为：{0}", AgeValue));
				return true;
			}

			int charmLevel = DateFile.instance.GetActorCharm(actorId);
			if (TextConvertHelper.CharmKey.TryGetIndex(charmLevel, out index) && !Main.settings.belove.AcceptedCharm[index])
			{
				Debug(string.Format("魅力判定失败，魅力级别为：{0} {1}", charmLevel, TextConvertHelper.CharmText[index]));
				return true;
			}

			int rankLevel = DateFile.instance.GetActorRank(actorId);
			if (TextConvertHelper.RankKey.TryGetIndex(rankLevel, out index) && !Main.settings.belove.AcceptedRank[index])
			{
				Debug(string.Format("阶层判定失败，阶层为：{0} {1}", rankLevel, TextConvertHelper.RankText[index]));
				return true;
			}

			int gangId = int.Parse(DateFile.instance.GetActorDate(actorId, 19, applyBonus: false));
			int gangLevel = int.Parse(DateFile.instance.GetActorDate(actorId, 20, applyBonus: false));
			int gangValueId = DateFile.instance.GetGangValueId(gangId, gangLevel);
			if (!Main.settings.belove.IgnoreGang && int.Parse(DateFile.instance.presetGangGroupDateValue[gangValueId][803]) == 0)
			{
				Debug(string.Format("门派判定失败，门派数据：{0} {1} {2}", gangId, gangLevel, gangValueId));
				return true;
			}

			int actorGoodness = DateFile.instance.GetActorGoodness(actorId);
			if (TextConvertHelper.GoodnessKey.TryGetIndex(actorGoodness, out index))
			{
				if (!Main.settings.belove.AcceptedGoodness[index])
				{
					Debug(string.Format("立场判定失败，立场为 {0} {1}", actorGoodness, TextConvertHelper.GoodnessText[index]));
					return true;
				}
			}


			if (int.TryParse(DateFile.instance.GetActorDate(actorId, 6, applyBonus: false), out int xiangShuValue))
			{
				if (TextConvertHelper.XiangShuKey.TryGetIndex(xiangShuValue, out index) && !Main.settings.belove.AcceptedXiangShu[index])
				{
					Debug(string.Format("相枢化判定失败，值为 {0} {1}", xiangShuValue, TextConvertHelper.XiangShuText[index]));
					return true;
				}
			}

			for(int i=0; i<TextConvertHelper.RelationCount; ++i)
			{
				if(!Main.settings.belove.AcceptedRelation[i] && DateFile.instance.GetActorSocial(actorId, TextConvertHelper.RelationKey[i]).Contains(mainActorId))
				{
					Debug(string.Format("关系判定失败，太吾是对方的 {0} {1}", TextConvertHelper.RelationKey[i], TextConvertHelper.XiangShuText[index]));
					return true;
				}
			}

			Debug("通用筛选判定通过");

			do
			{
				if (!isTaiwuAtThisTile)
					break;
				if(!Main.settings.belove.marry.Enabled)
					break;
				// 如果没有双向 两情相悦，则无法通过
				if(!(DateFile.instance.GetActorSocial(actorId, 306).Contains(mainActorId) && DateFile.instance.GetActorSocial(mainActorId, 306).Contains(actorId)))
					break;
				// 如果已经结婚，则无法通过
				if(DateFile.instance.GetActorSocial(actorId, 309).Contains(mainActorId) || DateFile.instance.GetActorSocial(mainActorId, 309).Contains(actorId))
					break;
				// 如果不是已婚杀手，且对方已婚，则无法通过
				if(!Main.settings.belove.marry.MarriedKiller && DateFile.instance.GetActorSocial(actorId, 309).Count > 0)
					break;
				// 如果太吾不能多配偶制，且太吾已婚，则跳过
				if (!Main.settings.belove.marry.Polygynous && DateFile.instance.GetActorSocial(mainActorId, 309).Count > 0)
					break;
				// 如果太吾不是僧侣杀手，且对方出家，则跳过
				if (!Main.settings.belove.marry.MonkKiller && int.Parse(DateFile.instance.GetActorDate(actorId, 2, applyBonus: false)) != 0)
					break;
				// 如果太吾不是迷人和尚，且太吾出家，则跳过
				if (!Main.settings.belove.marry.CharmingBonze && int.Parse(DateFile.instance.GetActorDate(mainActorId, 2, applyBonus: false)) != 0)
					break;

				// 历经万难，开始求婚
				PeopleLifeAIHelper.AISetEvent(8, new int[4]
				{
					0,
					actorId,
					232,
					actorId
				});

				Debug("求婚事件判定成功");
			} while(false);

			do
			{
				if (!isTaiwuAtThisTile)
					break;
				if (!Main.settings.belove.pursued.Enabled)
					break;
				// 如果对方不爱慕太吾，则跳过
				if (!DateFile.instance.GetActorSocial(actorId, 312).Contains(mainActorId))
					break;
				// 如果需要互相爱慕，但是太吾不爱慕对方，则跳过
				if (Main.settings.belove.pursued.LoveEach && !DateFile.instance.GetActorSocial(mainActorId, 312).Contains(actorId))
					break;
				// 如果任何一方已有两情相悦，则跳过
				if (DateFile.instance.GetActorSocial(actorId, 306).Contains(mainActorId) || DateFile.instance.GetActorSocial(mainActorId, 306).Contains(actorId))
					break;
				// 如果任何一方与对方已婚，则跳过
				if (DateFile.instance.GetActorSocial(actorId, 309).Contains(mainActorId) ||DateFile.instance.GetActorSocial(mainActorId, 309).Contains(actorId))
					break;
				
				// 进入表白事件
				PeopleLifeAIHelper.AISetEvent(8, new int[4]
				{
					0,
					actorId,
					231,
					actorId
				});
				Debug("表白事件判定成功");
			} while(false);

			do
			{
				if(!Main.settings.belove.enamor.Enabled)
					break;
				// 如果对方已经爱慕，则跳过
				if(DateFile.instance.GetActorSocial(actorId, 312).Contains(mainActorId))
					break;

				PeopleLifeAIHelper.AISetOtherLove(mainActorId, actorId, mainActorId, mapId, tileId);

				Debug("爱慕事件判定成功");
			} while(false);
			
			return true;
		}
	}
}
