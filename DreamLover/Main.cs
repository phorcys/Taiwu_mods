using Harmony12;
using System;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using System.Collections.Generic;

namespace DreamLover
{
	public static class Main
	{
		public static bool enabled;

		public static UnityModManager.ModEntry.ModLogger Logger;

		public static Setting settings;

        public static List<int> NoNtrSocialTypList = new List<int>();

		public static HarmonyInstance GetInstance(UnityModManager.ModEntry modEntry) => HarmonyInstance.Create(modEntry.Info.Id);

		public static bool Load(UnityModManager.ModEntry modEntry)
		{
			var instance = GetInstance(modEntry);
			//val.PatchAll(Assembly.GetExecutingAssembly());

			settings = UnityModManager.ModSettings.Load<Setting>(modEntry);
			Logger = modEntry.Logger;
			modEntry.OnToggle = OnToggle;
			modEntry.OnGUI = OnGUI;
			modEntry.OnSaveGUI = OnSaveGUI;


			AdjustPatch(modEntry);

			return true;
		}

		public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
		{
			enabled = value;
			return true;
		}

		public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
		{
			settings.Save(modEntry);
		}

		public static void Debug(string str)
		{
			if(settings.DebugMode)
				Logger.Log("[梦中情人] " + str);
		}

		public static void OnGUI(UnityModManager.ModEntry modEntry)
		{
			GUILayout.BeginVertical("Box");
			GUILayout.BeginHorizontal("Box");
			GUILayout.Label("显示：");
			if (GUILayout.Toggle(settings.MenuTabIndex == 0, "被追求设置页")) settings.MenuTabIndex = 0;
			if (GUILayout.Toggle(settings.MenuTabIndex == 1, "情难自已设置页")) settings.MenuTabIndex = 1;
			if (GUILayout.Toggle(settings.MenuTabIndex == 2, "怀孕设置页")) settings.MenuTabIndex = 2;
			if (GUILayout.Toggle(settings.MenuTabIndex == 3, "防绿设置页")) settings.MenuTabIndex = 3;
			settings.DebugMode = GUILayout.Toggle(settings.DebugMode, "Debug Mode");
            GUILayout.EndHorizontal();
			switch(settings.MenuTabIndex)
			{
				case 0:
					{
						GUILayout.BeginHorizontal("Box");
						settings.belove.enamor.Enabled = GUILayout.Toggle(settings.belove.enamor.Enabled, "主动爱慕");
						settings.belove.pursued.Enabled = GUILayout.Toggle(settings.belove.pursued.Enabled, "主动表白");
						if(settings.belove.enamor.Enabled || settings.belove.pursued.Enabled)
							settings.belove.ForgetMe = false;
						settings.belove.marry.Enabled = GUILayout.Toggle(settings.belove.marry.Enabled, "主动求婚");
						GUILayout.EndHorizontal();
						if (settings.belove.Enabled)
						{
							GUILayout.Label("通用配置");

							GUILayout.BeginHorizontal("Box");
							settings.belove.SexFilter[0] = GUILayout.Toggle(settings.belove.SexFilter[0], "接受女性");
							settings.belove.SexFilter[1] = GUILayout.Toggle(settings.belove.SexFilter[1], "接受男性");

							settings.belove.IgnoreDistance = GUILayout.Toggle(settings.belove.IgnoreDistance, "爱慕不受地理限制");
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal("Box");
							GUILayout.Label("接受的关系");
							for (int i=0; i<TextConvertHelper.RelationCount/2; ++i)
							{
								settings.belove.AcceptedRelation[i] = GUILayout.Toggle(settings.belove.AcceptedRelation[i], TextConvertHelper.RelationText[i]);
							}
							GUILayout.EndHorizontal();
							GUILayout.BeginHorizontal("Box");
							for (int i = TextConvertHelper.RelationCount / 2; i < TextConvertHelper.RelationCount; ++i)
							{
								settings.belove.AcceptedRelation[i] = GUILayout.Toggle(settings.belove.AcceptedRelation[i], TextConvertHelper.RelationText[i]);
							}
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal("Box");
							GUILayout.Label("年龄范围：");
							string minageStr = GUILayout.TextField(settings.belove.AcceptedAge.start.ToString(), 3);
							if (GUI.changed && int.TryParse(minageStr, out int minage))
							{
								settings.belove.AcceptedAge.start = minage;
							}
							GUILayout.Label("<= 可接受年龄 <=");
							string maxageStr = GUILayout.TextField(settings.belove.AcceptedAge.end.ToString(), 3);
							if (GUI.changed && int.TryParse(maxageStr, out int maxage))
							{
								settings.belove.AcceptedAge.length = maxage - settings.belove.AcceptedAge.start;
							}
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal("Box");
							GUILayout.Label("接受的入魔范围：");
							for (int i = 0; i < TextConvertHelper.XiangShuCount; ++i)
							{
								settings.belove.AcceptedXiangShu[i] = GUILayout.Toggle(settings.belove.AcceptedXiangShu[i], TextConvertHelper.XiangShuText[i]);
							}
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal("Box");
							GUILayout.Label("立场等级范围：");
							for(int i=0; i<TextConvertHelper.GoodnessCount; ++i)
							{
								settings.belove.AcceptedGoodness[i] = GUILayout.Toggle(settings.belove.AcceptedGoodness[i], TextConvertHelper.GoodnessText[i]);
							}
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal("Box");
							GUILayout.Label("好感度等级范围：");
							for(int i=0; i<TextConvertHelper.LikabilityCount; ++i)
							{
								settings.belove.AcceptedLikability[i] = GUILayout.Toggle(settings.belove.AcceptedLikability[i], TextConvertHelper.LikabilityText[i]);
							}
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal("Box");
							GUILayout.Label("魅力等级范围：");
							for(int i=0; i<TextConvertHelper.CharmCount; ++i)
							{
								settings.belove.AcceptedCharm[i] = GUILayout.Toggle(settings.belove.AcceptedCharm[i], TextConvertHelper.CharmText[i]);
							}
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal("Box");
							GUILayout.Label("阶层等级范围：");
							for(int i=0; i<TextConvertHelper.RankCount; ++i)
							{
								settings.belove.AcceptedRank[i] = GUILayout.Toggle(settings.belove.AcceptedRank[i], TextConvertHelper.MakeColor(TextConvertHelper.RankText[i], i));
							}
							GUILayout.EndHorizontal();
						}
						if (settings.belove.pursued.Enabled)
						{
							GUILayout.Label("主动表白相关配置");
							GUILayout.BeginHorizontal("Box");
							settings.belove.pursued.LoveEach = GUILayout.Toggle(settings.belove.pursued.LoveEach, "互相爱慕");
							GUILayout.Label("如果选中互相爱慕，太吾不爱慕的人将不会试图表白太吾");
							GUILayout.EndHorizontal();
						}
						if (settings.belove.marry.Enabled)
						{
							GUILayout.Label("主动求婚相关配置");
							GUILayout.BeginHorizontal("Box");
							settings.belove.marry.MarriedKiller = GUILayout.Toggle(settings.belove.marry.MarriedKiller, "已婚人士");
							settings.belove.marry.Polygynous = GUILayout.Toggle(settings.belove.marry.Polygynous, "太吾多配偶制");
							settings.belove.IgnoreGang = GUILayout.Toggle(settings.belove.IgnoreGang, "无视门派规章");
							settings.belove.marry.MonkKiller = GUILayout.Toggle(settings.belove.marry.MonkKiller, "出家人");
							settings.belove.marry.CharmingBonze = GUILayout.Toggle(settings.belove.marry.CharmingBonze, "太吾出家也要求婚");
							GUILayout.EndHorizontal();
						}

						if (!settings.belove.enamor.Enabled && !settings.belove.pursued.Enabled)
						{
							GUILayout.BeginHorizontal("Box");
							settings.belove.ForgetMe = GUILayout.Toggle(settings.belove.ForgetMe, TextConvertHelper.MakeColor("雨恨云愁", 0));
							GUILayout.Label("单恋太吾的人会忘却太吾");
							GUILayout.EndHorizontal();
						}
						else
						{
							settings.belove.ForgetMe = false;
						}
						break;
					}
				case 1:
					{
						GUILayout.Label("通用配置");
						GUILayout.BeginHorizontal("Box");
						settings.rape.skipBattle = GUILayout.Toggle(settings.rape.skipBattle, "跳过战力判定");
						settings.rape.moodChange = GUILayout.Toggle(settings.rape.moodChange, "影响情绪");
						settings.rape.beHated = GUILayout.Toggle(settings.rape.beHated, "导致仇恨");
						settings.rape.oneParent = GUILayout.Toggle(settings.rape.oneParent, "单亲孩子");
						GUILayout.EndHorizontal();

						GUILayout.Label("发生关系配置");
						GUILayout.BeginHorizontal("Box");
						settings.rape.overwriteArg = GUILayout.Toggle(settings.rape.overwriteArg, "是否覆盖 EndEvent 的传入参数");
						GUILayout.EndHorizontal();

						GUILayout.Label("过月时欺辱配置");
						GUILayout.BeginHorizontal("Box");
						settings.rape.autorape.Enabled = GUILayout.Toggle(settings.rape.autorape.Enabled, "启用过月自动欺辱");

						settings.rape.autorape.SpecifiedPossibility = GUILayout.Toggle(settings.rape.autorape.SpecifiedPossibility, "指定概率");
						if (settings.rape.autorape.SpecifiedPossibility)
						{
							string s3 = GUILayout.TextField(settings.rape.autorape.Possibility.ToString(), 3);
							if (GUI.changed && !int.TryParse(s3, out settings.rape.autorape.Possibility))
							{
								settings.rape.autorape.Possibility = 10;
							}
							GUILayout.Label("%");
						}

						settings.rape.autorape.JustLover = GUILayout.Toggle(settings.rape.autorape.JustLover, "仅太吾爱慕的人");
						settings.rape.autorape.DifferentSex = GUILayout.Toggle(settings.rape.autorape.DifferentSex, "仅异性");
						settings.rape.autorape.FilterName = GUILayout.Toggle(settings.rape.autorape.FilterName, "指定名称（支持部分匹配）");
						settings.rape.autorape.Name = GUILayout.TextField(settings.rape.autorape.Name.ToString(), 6, GUILayout.Width(120f));
						GUILayout.EndHorizontal();
						break;
					}
				case 2:
					{
						GUILayout.Label("怀孕设置页");
						GUILayout.BeginHorizontal("Box");
						settings.pregnant.Enabled = GUILayout.Toggle(settings.pregnant.Enabled, "启用指定怀孕功能");
						settings.pregnant.SpecifiedFecundity = GUILayout.Toggle(settings.pregnant.SpecifiedFecundity, "指定生育能力");
						if (settings.pregnant.SpecifiedFecundity)
						{
							string s4 = GUILayout.TextField(settings.pregnant.Fecundity.ToString(), 5);
							if (GUI.changed && !int.TryParse(s4, out settings.pregnant.Fecundity))
							{
								settings.pregnant.Fecundity = 7500;
							}
							GUILayout.Label("/15000");
						}
						settings.pregnant.SpecifiedPossibility = GUILayout.Toggle(settings.pregnant.SpecifiedPossibility, "指定怀孕概率");
						if (settings.pregnant.SpecifiedPossibility)
						{
							string s5 = GUILayout.TextField(settings.pregnant.Possibility.ToString(), 3);
							if (GUI.changed && !int.TryParse(s5, out settings.pregnant.Possibility))
							{
								settings.pregnant.Possibility = 50;
							}
							GUILayout.Label("%");
						}
						settings.pregnant.SpecifiedQuQu = GUILayout.Toggle(settings.pregnant.SpecifiedQuQu, "指定蛐蛐概率");
						if (settings.pregnant.SpecifiedQuQu)
						{
							string s6 = GUILayout.TextField(settings.pregnant.QuQu.ToString(), 3);
							if (GUI.changed && !int.TryParse(s6, out settings.pregnant.QuQu))
							{
								settings.pregnant.QuQu = 10;
							}
							GUILayout.Label("%");
						}
						GUILayout.EndHorizontal();
						break;
					}
				case 3:
					{
						GUILayout.Label("防绿设置页");
						GUILayout.BeginHorizontal("Box");
						settings.nontr.Enabled = GUILayout.Toggle(settings.nontr.Enabled, "启用防绿功能");
						settings.nontr.AllowCouple = GUILayout.Toggle(settings.nontr.AllowCouple, "能与配偶发生关系");
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal("Box");
						GUILayout.Label("想要控制的关系");
						NoNtrSocialTypList.Clear();
						for (int i = 0; i < TextConvertHelper.RelationCount / 2; ++i)
						{
							settings.nontr.PreventRelation[i] = GUILayout.Toggle(settings.nontr.PreventRelation[i], TextConvertHelper.RelationText[i]);
							if(settings.nontr.PreventRelation[i])
							{
								NoNtrSocialTypList.Add(TextConvertHelper.RelationKey[i]);
							}
						}
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal("Box");
						for (int i = TextConvertHelper.RelationCount / 2; i < TextConvertHelper.RelationCount; ++i)
						{
							settings.nontr.PreventRelation[i] = GUILayout.Toggle(settings.nontr.PreventRelation[i], TextConvertHelper.RelationText[i]);
							if (settings.nontr.PreventRelation[i])
							{
								NoNtrSocialTypList.Add(TextConvertHelper.RelationKey[i]);
							}
						}
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal("Box");
						for (int i = 0; i < TextConvertHelper.ExpandRelationCount; ++i)
						{
							settings.nontr.PreventRelationEx[i] = GUILayout.Toggle(settings.nontr.PreventRelationEx[i], TextConvertHelper.ExpandRelationText[i]);
							if (settings.nontr.PreventRelationEx[i])
							{
								NoNtrSocialTypList.Add(TextConvertHelper.ExpandRelationKey[i]);
							}
						}
						settings.nontr.PreventAll = GUILayout.Toggle(settings.nontr.PreventAll, "任何人");
						GUILayout.EndHorizontal();
						break;
					}
				default:
					{
						settings.MenuTabIndex = 0;
						GUILayout.Label("请重新展开设置页面");
						break;
					}
			}
            GUILayout.EndVertical();
			AdjustPatch(modEntry);
		}

		public static void Patch(this HarmonyInstance instance, PatchModuleInfo patchMod)
		{
			if(patchMod.Patch(instance))
			{
				Debug(string.Format("Succeeded in Patching Module {0}.", patchMod.Name));
			}
			else
			{
			}
		}
		public static void Unpatch(this HarmonyInstance instance, PatchModuleInfo patchMod)
		{
			if (patchMod.Unpatch(instance))
			{
				Debug(string.Format("Succeeded in Unpatching Module {0}.", patchMod.Name));
			}
			else
			{
			}
		}

		/// <summary>
		/// 假设 EndEvent_Rape_Patch 不需要考虑
		/// 遍历所有项来调整装载情况
		/// </summary>
		/// <param name="modEntry"></param>
		public static void AdjustPatch(UnityModManager.ModEntry modEntry)
		{
			var instance = GetInstance(modEntry);

			if (enabled && settings.belove.Enabled)
				instance.Patch(BeLove_Patch.patchModuleInfo);
			else
				instance.Unpatch(BeLove_Patch.patchModuleInfo);

			if (enabled && settings.rape.autorape.Enabled)
				instance.Patch(AutoRape_Patch.patchModuleInfo);
			else
				instance.Unpatch(AutoRape_Patch.patchModuleInfo);

			if (enabled && settings.pregnant.Enabled || settings.nontr.Enabled)
				instance.Patch(SetChildren_Patch.patchModuleInfo);
			else
				instance.Unpatch(SetChildren_Patch.patchModuleInfo);

			if(enabled)
				instance.Patch(EndEvent_Rape_Patch.patchModuleInfo);
			else
				instance.Unpatch(EndEvent_Rape_Patch.patchModuleInfo);
		}
	}
}
