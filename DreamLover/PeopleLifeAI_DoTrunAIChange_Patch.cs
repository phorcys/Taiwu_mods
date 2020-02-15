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
			if (!Main.settings.主动爱慕 && !Main.settings.主动表白 && !Main.settings.主动求婚)
			{
				return true;
			}
			if (!Main.settings.全世界都喜欢太吾 && !isTaiwuAtThisTile)
			{
				return true;
			}

			string name = DateFile.instance.GetActorName(actorId);
			Debug(name + "通过位置判定");

			int actorFavor = DateFile.instance.GetActorFavor(false, mainActorId, actorId, getLevel: true);
			if (Main.settings.可接受的好感度等级.TryGetValue(actorFavor, out bool value))
			{
				if (!value)
				{
					return true;
				}
			}

			Debug(name + "通过好感判定 ：" + DateFile.instance.GetActorName(actorId) + "好感度为: " + actorFavor);

			int num = int.Parse(DateFile.instance.GetActorDate(actorId, 11, applyBonus: false));
			if (num < Main.settings.年龄下限 || num > Main.settings.年龄上限)
			{
				return true;
			}

			Debug(name + "通过年龄判定");

			int num2 = int.Parse(DateFile.instance.GetActorDate(actorId, 15));
			int key = num2 / 100;
			if (Main.settings.可接受的魅力等级.TryGetValue(key, out bool value2))
			{
				if (!value2)
				{
					return true;
				}
			}

			Debug(name + "通过魅力判定：" + DateFile.instance.GetActorName(actorId) + "魅力为: " + num2);

			if (int.TryParse(DateFile.instance.GetActorDate(actorId, 20, applyBonus: false), out int result))
			{
				int num3 = Mathf.Abs(result);
				if (Main.settings.可接受的阶层等级.TryGetValue(num3, out bool value3))
				{
					if (!value3)
					{
						return true;
					}
				}

				Debug(name + "通过阶层判定：" + DateFile.instance.GetActorName(actorId) + "阶层为: " + result + ": " + num3);
			}

			int num4 = int.Parse(DateFile.instance.GetActorDate(actorId, 997, applyBonus: false));
			if (!Main.settings.男的可以来 && num4 % 2 == 1)
			{
				return true;
			}
			if (!Main.settings.女的可以来 && num4 % 2 == 0)
			{
				return true;
			}

			Debug(name + "通过性别判定");

			int gangId = int.Parse(DateFile.instance.GetActorDate(actorId, 19, applyBonus: false));
			int gangLevel = int.Parse(DateFile.instance.GetActorDate(actorId, 20, applyBonus: false));
			int gangValueId = DateFile.instance.GetGangValueId(gangId, gangLevel);
			if (!Main.settings.不顾门派爱太吾 && int.Parse(DateFile.instance.presetGangGroupDateValue[gangValueId][803]) == 0)
			{
				return true;
			}

			Debug(name + "通过门派判定");

			int actorGoodness = DateFile.instance.GetActorGoodness(actorId);
			if (Main.settings.可接受的立场等级.TryGetValue(actorGoodness, out bool value4))
			{
				if (!value4)
				{
					return true;
				}
			}

			Debug(name + "通过立场判定：" + DateFile.instance.GetActorName(actorId) + "立场为: " + actorGoodness);

			if (int.TryParse(DateFile.instance.GetActorDate(actorId, 6, applyBonus: false), out int result2))
			{
				if (result2 > Main.settings.入魔程度)
				{
					return true;
				}
			}

			Debug(name + "通过入魔判定");

			if (!Main.settings.兄弟姐妹 && DateFile.instance.GetActorSocial(actorId, 302).Contains(mainActorId))
			{
				return true;
			}
			if (!Main.settings.亲生父母 && DateFile.instance.GetActorSocial(actorId, 303).Contains(mainActorId))
			{
				return true;
			}
			if (!Main.settings.义父义母 && DateFile.instance.GetActorSocial(actorId, 304).Contains(mainActorId))
			{
				return true;
			}
			if (!Main.settings.授业恩师 && DateFile.instance.GetActorSocial(actorId, 305).Contains(mainActorId))
			{
				return true;
			}
			if (!Main.settings.义结金兰 && DateFile.instance.GetActorSocial(actorId, 308).Contains(mainActorId))
			{
				return true;
			}
			if (!Main.settings.儿女子嗣 && DateFile.instance.GetActorSocial(actorId, 310).Contains(mainActorId))
			{
				return true;
			}
			if (!Main.settings.嫡系传人 && DateFile.instance.GetActorSocial(actorId, 311).Contains(mainActorId))
			{
				return true;
			}
			if (!Main.settings.势不两立 && DateFile.instance.GetActorSocial(actorId, 401).Contains(mainActorId))
			{
				return true;
			}
			if (!Main.settings.血海深仇 && DateFile.instance.GetActorSocial(actorId, 402).Contains(mainActorId))
			{
				return true;
			}
			if (!Main.settings.父系血统 && DateFile.instance.GetActorSocial(actorId, 601).Contains(mainActorId))
			{
				return true;
			}
			if (!Main.settings.母系血统 && DateFile.instance.GetActorSocial(actorId, 602).Contains(mainActorId))
			{
				return true;
			}

			Debug(name + "通过关系判定");

			if (isTaiwuAtThisTile && Main.settings.主动求婚 && DateFile.instance.GetActorSocial(actorId, 306).Contains(mainActorId) && DateFile.instance.GetActorSocial(mainActorId, 306).Contains(actorId) && !DateFile.instance.GetActorSocial(actorId, 309).Contains(mainActorId) && !DateFile.instance.GetActorSocial(mainActorId, 309).Contains(actorId) && (Main.settings.已婚人士想和太吾结婚 || DateFile.instance.GetActorSocial(actorId, 309).Count <= 0) && (Main.settings.即使太吾已婚别人也想求婚 || DateFile.instance.GetActorSocial(mainActorId, 309).Count <= 0) && (Main.settings.即使出家也要求婚太吾 || int.Parse(DateFile.instance.GetActorDate(actorId, 2, applyBonus: false)) == 0) && (Main.settings.即使太吾出家也要求婚 || int.Parse(DateFile.instance.GetActorDate(mainActorId, 2, applyBonus: false)) == 0))
			{
				PeopleLifeAI_Utils.AISetEvent(8, new int[4]
				{
					0,
					actorId,
					232,
					actorId
				});

				Debug("进入求婚事件：" + name + " 试图求婚 太吾传人");
			}

			if (isTaiwuAtThisTile && Main.settings.主动表白 && DateFile.instance != null && DateFile.instance.GetActorSocial(actorId, 312).Contains(mainActorId) && (Main.settings.太吾不爱的人也表白太吾 || DateFile.instance.GetActorSocial(mainActorId, 312).Contains(actorId)) && !DateFile.instance.GetActorSocial(actorId, 306).Contains(mainActorId) && !DateFile.instance.GetActorSocial(mainActorId, 306).Contains(actorId) && !DateFile.instance.GetActorSocial(actorId, 309).Contains(mainActorId) && !DateFile.instance.GetActorSocial(mainActorId, 309).Contains(actorId))
			{
				PeopleLifeAI_Utils.AISetEvent(8, new int[4]
				{
					0,
					actorId,
					231,
					actorId
				});
				Debug("进入表白事件：" + name + " 试图表白 太吾传人");
			}
			if (Main.settings.主动爱慕 && !DateFile.instance.GetActorSocial(actorId, 312).Contains(mainActorId))
			{
				PeopleLifeAI_Utils.AISetOtherLove(mainActorId, actorId, mainActorId, mapId, tileId);

				Debug("进入爱慕事件：" + name + " 喜欢上了 太吾传人");
			}
			return true;
		}
	}
}
