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

        public static List<int> TaiwuLovers;

		private static bool 接受入邪 = false;

		private static bool 接受入魔 = false;

		public static bool Load(UnityModManager.ModEntry modEntry)
		{
			HarmonyInstance val = HarmonyInstance.Create(modEntry.Info.Id);
			val.PatchAll(Assembly.GetExecutingAssembly());
			settings = UnityModManager.ModSettings.Load<Setting>(modEntry);
			Logger = modEntry.Logger;
			modEntry.OnToggle = OnToggle;
			modEntry.OnGUI = OnGUI;
			modEntry.OnSaveGUI = OnSaveGUI;
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

		public static void Debug(String str)
		{
			if(Main.settings.debugmode)
				Logger.Log("[梦中情人] " + str);
		}

		public static void OnGUI(UnityModManager.ModEntry modEntry)
		{
			GUILayout.BeginVertical("Box");
			GUILayout.BeginHorizontal("Box");
			GUILayout.Label("显示：");
			if (GUILayout.Toggle(settings.菜单打开情况 == 0, "被追求设置页")) settings.菜单打开情况 = 0;
			if (GUILayout.Toggle(settings.菜单打开情况 == 1, "情难自已设置页")) settings.菜单打开情况 = 1;
			if (GUILayout.Toggle(settings.菜单打开情况 == 2, "怀孕设置页")) settings.菜单打开情况 = 2;
			if (GUILayout.Toggle(settings.菜单打开情况 == 3, "防绿设置页")) settings.菜单打开情况 = 3;
            GUILayout.EndHorizontal();
			switch(settings.菜单打开情况)
			{
				case 0:
					{
						GUILayout.Label("被追求设置页");
						GUILayout.BeginHorizontal("Box");
						settings.主动爱慕 = GUILayout.Toggle(settings.主动爱慕, "主动爱慕");
						settings.主动表白 = GUILayout.Toggle(settings.主动表白, "主动表白");
						settings.主动求婚 = GUILayout.Toggle(settings.主动求婚, "主动求婚");
						GUILayout.EndHorizontal();
						if (settings.主动爱慕 || settings.主动表白 || settings.主动求婚)
						{
							GUILayout.BeginHorizontal("Box");
							settings.男的可以来 = GUILayout.Toggle(settings.男的可以来, "接受男性");
							settings.女的可以来 = GUILayout.Toggle(settings.女的可以来, "接受女性");
							settings.全世界都喜欢太吾 = GUILayout.Toggle(settings.全世界都喜欢太吾, "爱慕不受地理限制");
							GUILayout.EndHorizontal();
						}
						if (settings.主动爱慕 || settings.主动表白 || settings.主动求婚)
						{
							GUILayout.Label("通用配置");
							GUILayout.BeginHorizontal("Box");
							GUILayout.Label("接受的关系（指NPC面板中的关系）");
							GUILayout.EndHorizontal();
							GUILayout.BeginHorizontal("Box");
							settings.兄弟姐妹 = GUILayout.Toggle(settings.兄弟姐妹, "兄弟姐妹");
							settings.亲生父母 = GUILayout.Toggle(settings.亲生父母, "亲生父母");
							settings.义父义母 = GUILayout.Toggle(settings.义父义母, "义父义母");
							settings.授业恩师 = GUILayout.Toggle(settings.授业恩师, "授业恩师");
							settings.义结金兰 = GUILayout.Toggle(settings.义结金兰, "义结金兰");
							settings.儿女子嗣 = GUILayout.Toggle(settings.儿女子嗣, "儿女子嗣");
							settings.嫡系传人 = GUILayout.Toggle(settings.嫡系传人, "嫡系传人");
							settings.势不两立 = GUILayout.Toggle(settings.势不两立, "势不两立");
							settings.血海深仇 = GUILayout.Toggle(settings.血海深仇, "血海深仇");
							settings.父系血统 = GUILayout.Toggle(settings.父系血统, "父系血统");
							settings.母系血统 = GUILayout.Toggle(settings.母系血统, "母系血统");
							GUILayout.EndHorizontal();
							GUILayout.BeginHorizontal("Box");
							GUILayout.Label("年龄范围：");
							string s = GUILayout.TextField(settings.年龄下限.ToString(), 3);
							if (GUI.changed && !int.TryParse(s, out settings.年龄下限))
							{
								settings.年龄下限 = 14;
							}
							GUILayout.Label("<= 可接受年龄 <=");
							string s2 = GUILayout.TextField(settings.年龄上限.ToString(), 3);
							if (GUI.changed && !int.TryParse(s2, out settings.年龄上限))
							{
								settings.年龄上限 = 60;
							}
							GUILayout.EndHorizontal();
							GUILayout.BeginHorizontal("Box");
							接受入邪 = GUILayout.Toggle(接受入邪, "是否接受入邪者");
							接受入魔 = (接受入邪 && GUILayout.Toggle(接受入魔, "是否接受入魔者"));
							settings.入魔程度 = (接受入邪 ? ((!接受入魔) ? 1 : 2) : 0);
							GUILayout.EndHorizontal();
							GUILayout.BeginHorizontal("Box");
							GUILayout.Label("立场等级范围：");
							settings.可接受的立场等级[2] = GUILayout.Toggle(settings.可接受的立场等级[2], "刚正");
							settings.可接受的立场等级[1] = GUILayout.Toggle(settings.可接受的立场等级[1], "仁善");
							settings.可接受的立场等级[0] = GUILayout.Toggle(settings.可接受的立场等级[0], "中庸");
							settings.可接受的立场等级[3] = GUILayout.Toggle(settings.可接受的立场等级[3], "叛逆");
							settings.可接受的立场等级[4] = GUILayout.Toggle(settings.可接受的立场等级[4], "唯我");
							GUILayout.EndHorizontal();
							GUILayout.BeginHorizontal("Box");
							GUILayout.Label("好感度等级范围：");
							settings.可接受的好感度等级[-1] = GUILayout.Toggle(settings.可接受的好感度等级[-1], "素未谋面");
							settings.可接受的好感度等级[0] = GUILayout.Toggle(settings.可接受的好感度等级[0], "陌路");
							settings.可接受的好感度等级[1] = GUILayout.Toggle(settings.可接受的好感度等级[1], "冷淡");
							settings.可接受的好感度等级[2] = GUILayout.Toggle(settings.可接受的好感度等级[2], "融洽");
							settings.可接受的好感度等级[3] = GUILayout.Toggle(settings.可接受的好感度等级[3], "热忱");
							settings.可接受的好感度等级[4] = GUILayout.Toggle(settings.可接受的好感度等级[4], "喜欢");
							settings.可接受的好感度等级[5] = GUILayout.Toggle(settings.可接受的好感度等级[5], "亲密");
							settings.可接受的好感度等级[6] = GUILayout.Toggle(settings.可接受的好感度等级[6], "不渝");
							GUILayout.EndHorizontal();
							GUILayout.BeginHorizontal("Box");
							GUILayout.Label("魅力等级范围：");
							settings.可接受的魅力等级[0] = GUILayout.Toggle(settings.可接受的魅力等级[0], "非人");
							settings.可接受的魅力等级[1] = GUILayout.Toggle(settings.可接受的魅力等级[1], "可憎");
							settings.可接受的魅力等级[2] = GUILayout.Toggle(settings.可接受的魅力等级[2], "不扬");
							SerializableDictionary<int, bool> 可接受的魅力等级 = settings.可接受的魅力等级;
							bool value = settings.可接受的魅力等级[3] = GUILayout.Toggle(settings.可接受的魅力等级[3], "寻常");
							可接受的魅力等级[4] = value;
							settings.可接受的魅力等级[5] = GUILayout.Toggle(settings.可接受的魅力等级[5], "出众");
							settings.可接受的魅力等级[6] = GUILayout.Toggle(settings.可接受的魅力等级[6], "瑾瑜/瑶碧");
							settings.可接受的魅力等级[7] = GUILayout.Toggle(settings.可接受的魅力等级[7], "龙资/凤仪");
							settings.可接受的魅力等级[8] = GUILayout.Toggle(settings.可接受的魅力等级[8], "绝世/出尘");
							settings.可接受的魅力等级[9] = GUILayout.Toggle(settings.可接受的魅力等级[9], "天人");
							GUILayout.EndHorizontal();
							GUILayout.BeginHorizontal("Box");
							GUILayout.Label("阶层等级范围：");
							settings.可接受的阶层等级[1] = GUILayout.Toggle(settings.可接受的阶层等级[1], "<color=#E4504DFF>一品</color>");
							settings.可接受的阶层等级[2] = GUILayout.Toggle(settings.可接受的阶层等级[2], "<color=#F28234FF>二品</color>");
							settings.可接受的阶层等级[3] = GUILayout.Toggle(settings.可接受的阶层等级[3], "<color=#E3C66DFF>三品</color>");
							settings.可接受的阶层等级[4] = GUILayout.Toggle(settings.可接受的阶层等级[4], "<color=#AE5AC8FF>四品</color>");
							settings.可接受的阶层等级[5] = GUILayout.Toggle(settings.可接受的阶层等级[5], "<color=#63CED0FF>五品</color>");
							settings.可接受的阶层等级[6] = GUILayout.Toggle(settings.可接受的阶层等级[6], "<color=#8FBAE7FF>六品</color>");
							settings.可接受的阶层等级[7] = GUILayout.Toggle(settings.可接受的阶层等级[7], "<color=#6DB75FFF>七品</color>");
							settings.可接受的阶层等级[8] = GUILayout.Toggle(settings.可接受的阶层等级[8], "<color=#FBFBFBFF>八品</color>");
							settings.可接受的阶层等级[9] = GUILayout.Toggle(settings.可接受的阶层等级[9], "<color=#8E8E8EFF>九品</color>");
							GUILayout.EndHorizontal();
						}
						if (settings.主动表白)
						{
							GUILayout.Label("主动表白相关配置");
							GUILayout.BeginHorizontal("Box");
							settings.太吾不爱的人也表白太吾 = GUILayout.Toggle(settings.太吾不爱的人也表白太吾, "主动追求");
							GUILayout.Label("如果选中主动追求，太吾不爱慕的人也会试图表白太吾");
							GUILayout.EndHorizontal();
						}
						if (settings.主动求婚)
						{
							GUILayout.Label("主动求婚相关配置");
							GUILayout.BeginHorizontal("Box");
							settings.已婚人士想和太吾结婚 = GUILayout.Toggle(settings.已婚人士想和太吾结婚, "宁可出轨也要太吾");
							settings.即使太吾已婚别人也想求婚 = GUILayout.Toggle(settings.即使太吾已婚别人也想求婚, "为了太吾甘愿做小");
							GUILayout.EndHorizontal();
							GUILayout.BeginHorizontal("Box");
							settings.不顾门派爱太吾 = GUILayout.Toggle(settings.不顾门派爱太吾, "无视门派规章");
							settings.即使出家也要求婚太吾 = GUILayout.Toggle(settings.即使出家也要求婚太吾, "出家难忘太吾");
							settings.即使太吾出家也要求婚 = GUILayout.Toggle(settings.即使太吾出家也要求婚, "太吾出家也要求婚");
							GUILayout.EndHorizontal();
						}
						if (!settings.主动爱慕)
						{
							GUILayout.BeginHorizontal("Box");
							GUILayout.Label("雨恨云愁");
							if (GUILayout.Button("单向爱慕太吾的人忘记这份感情"))
							{
								ExpandUtils.RemoveAllLove();
							}
							GUILayout.EndHorizontal();
						}
						break;
					}
				case 1:
					{
						GUILayout.Label("情难自已设置页");
						GUILayout.BeginHorizontal("Box");
						settings.启用主动欺辱功能 = GUILayout.Toggle(settings.启用主动欺辱功能, "启用主动欺辱功能");
						settings.指定主动欺辱概率 = GUILayout.Toggle(settings.指定主动欺辱概率, "指定概率");
						if (settings.指定主动欺辱概率)
						{
							string s3 = GUILayout.TextField(settings.主动欺辱概率.ToString(), 3);
							if (GUI.changed && !int.TryParse(s3, out settings.主动欺辱概率))
							{
								settings.主动欺辱概率 = 10;
							}
							GUILayout.Label("%");
						}
						settings.跳过战力检定 = GUILayout.Toggle(settings.跳过战力检定, "无视战力直接成功");
						settings.主动欺辱影响双方情绪 = GUILayout.Toggle(settings.主动欺辱影响双方情绪, "影响双方情绪");
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal("Box");
						settings.主动欺辱结仇 = GUILayout.Toggle(settings.主动欺辱结仇, "导致对方仇恨");
						settings.主动欺辱的孩子有双亲 = !GUILayout.Toggle(!settings.主动欺辱的孩子有双亲, "单亲孩子");
						settings.主动欺辱筛选项 = GUILayout.Toggle(settings.主动欺辱筛选项, "显示定向欺辱选项");
						GUILayout.EndHorizontal();
						if (settings.主动欺辱筛选项)
						{
							GUILayout.BeginHorizontal("Box");
							settings.主动欺辱爱慕筛选 = GUILayout.Toggle(settings.主动欺辱爱慕筛选, "只对自己爱慕的人下手");
							settings.主动欺辱异性筛选 = GUILayout.Toggle(settings.主动欺辱异性筛选, "只对异性下手");
							settings.主动欺辱人名筛选 = GUILayout.Toggle(settings.主动欺辱人名筛选, "只对指定名称的人下手（支持部分匹配）");
							GUILayout.Label("名称（部分）：", GUILayout.Width(120f));
							settings.人名字符串片段 = GUILayout.TextField(settings.人名字符串片段.ToString(), 6, GUILayout.Width(120f));
							GUILayout.EndHorizontal();
						}
						break;
					}
				case 2:
					{
						GUILayout.Label("怀孕设置页");
						GUILayout.BeginHorizontal("Box");
						settings.启用指定怀孕功能 = GUILayout.Toggle(settings.启用指定怀孕功能, "启用指定怀孕功能");
						settings.指定生育可能性 = GUILayout.Toggle(settings.指定生育可能性, "指定生育可能性");
						if (settings.指定生育可能性)
						{
							string s4 = GUILayout.TextField(settings.生育可能性.ToString(), 5);
							if (GUI.changed && !int.TryParse(s4, out settings.生育可能性))
							{
								settings.生育可能性 = 7500;
							}
							GUILayout.Label("/15000");
						}
						settings.指定怀孕概率 = GUILayout.Toggle(settings.指定怀孕概率, "指定怀孕概率");
						if (settings.指定怀孕概率)
						{
							string s5 = GUILayout.TextField(settings.怀孕概率.ToString(), 3);
							if (GUI.changed && !int.TryParse(s5, out settings.怀孕概率))
							{
								settings.怀孕概率 = 50;
							}
							GUILayout.Label("%");
						}
						settings.指定蛐蛐概率 = GUILayout.Toggle(settings.指定蛐蛐概率, "指定蛐蛐概率");
						if (settings.指定蛐蛐概率)
						{
							string s6 = GUILayout.TextField(settings.蛐蛐概率.ToString(), 3);
							if (GUI.changed && !int.TryParse(s6, out settings.蛐蛐概率))
							{
								settings.蛐蛐概率 = 10;
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
						settings.启用防绿功能 = GUILayout.Toggle(settings.启用防绿功能, "启用防绿功能");
						settings.太吾爱慕的人不能与他人发生关系 = GUILayout.Toggle(settings.太吾爱慕的人不能与他人发生关系, "太吾爱慕的人不能与他人发生关系");
						settings.爱慕太吾的人不能与他人发生关系 = GUILayout.Toggle(settings.爱慕太吾的人不能与他人发生关系, "爱慕太吾的人不能与他人发生关系");
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal("Box");
						settings.和太吾之外的人都不能发生关系 = GUILayout.Toggle(settings.和太吾之外的人都不能发生关系, "太吾之外的任何人都不能发生关系（慎用）");
						GUILayout.Space(200);
						GUILayout.Label("爱慕关系包括：");
						settings.爱慕关系[312] = GUILayout.Toggle(settings.爱慕关系[312], "倾心爱慕");
						settings.爱慕关系[306] = GUILayout.Toggle(settings.爱慕关系[306], "两情相悦");
						settings.爱慕关系[309] = GUILayout.Toggle(settings.爱慕关系[309], "结发夫妻");
						GUILayout.EndHorizontal();
						break;
					}
				default:
					{
						settings.菜单打开情况 = 0;
						GUILayout.Label("请重新展开设置页面");
						break;
					}
			}
            GUILayout.EndVertical();
		}
	}
}
