using UnityModManagerNet;
using System.Collections.Generic;
using UnityEngine;

namespace DreamLover
{
	public class Setting : UnityModManager.ModSettings
	{
		public int MenuTabIndex = 0;

		/// <summary>
		/// 梦中情人的配置（即被爱慕、追求及求婚）
		/// </summary>
		public class BELOVE
		{
			/// <summary>
			/// 关于爱慕的配置
			/// </summary>
			public class Enamor
			{
				public bool Enabled = true;
			}
			/// <summary>
			/// 主动追求的配置
			/// </summary>
			public class Pursued
			{
				public bool Enabled = true;
				/// <summary>
				/// 互相爱慕才会表白
				/// </summary>
				public bool LoveEach = true;
			}
			/// <summary>
			/// 主动求婚的配置
			/// </summary>
			public class Marry
			{
				public bool Enabled = true;
				/// <summary>
				/// 已婚人士杀手
				/// </summary>
				public bool MarriedKiller = false;
				/// <summary>
				/// 一夫多妻制
				/// </summary>
				public bool Polygynous = false;
				/// <summary>
				/// 僧侣杀手
				/// </summary>
				public bool MonkKiller = false;
				/// <summary>
				/// 迷人和尚
				/// </summary>
				public bool CharmingBonze = false;
			}
			public bool Enabled
			{
				get { return enamor.Enabled || pursued.Enabled;}
			}

			public bool IgnoreGang = false;

			public bool IgnoreDistance = false;

			/// <summary>
			/// 0 女 1 男
			/// 草，茄子这个定义真是象形
			/// </summary>
			public bool[] SexFilter = new bool[2]{ true, false};

			/// <summary>
			/// 可接受的好感度等级，引用下方的 TextConvertHelper
			/// </summary>
			public bool[] AcceptedLikability = new bool[TextConvertHelper.LikabilityCount]
			{ false, false, false, false, true, true, true, true};

			public bool[] AcceptedGoodness = new bool[TextConvertHelper.GoodnessCount]
			{ false, true, true, false, false};

			public bool[] AcceptedCharm = new bool[TextConvertHelper.CharmCount]{ false, false, false, false, true, true, true, true, true};

			public bool [] AcceptedRank = new bool[TextConvertHelper.RankCount]{ true, true, true, true, true, true, true, false, false};

			public bool[] AcceptedRelation = new bool[TextConvertHelper.RelationCount] {
			true,
			false, false, false,
			true, true, false,
			false, false,
			true, true,
			false, false,
			true};

			public bool [] AcceptedXiangShu = new bool[TextConvertHelper.XiangShuCount]{ true, false, false};

			public bool ForgetMe = false;

			public RangeInt AcceptedAge = new RangeInt(16, 44);

			public Enamor enamor = new Enamor();
			public Pursued pursued = new Pursued();
			public Marry marry = new Marry();
		}

		public class RAPE
		{
			public class AutoRape
			{
				public bool Enabled = false;
				
				public bool SpecifiedPossibility = false;

				public int Possibility = 10;

				public bool FilterName = false;

				public string Name = "";

				public bool JustLover = true;

				public bool DifferentSex = true;
			}

			public AutoRape autorape = new AutoRape();

			public bool skipBattle = false;

			public bool moodChange = true;

			public bool beHated = true;

			public bool oneParent = true;

			public bool overwriteArg = false;
		}

		public class PREGNANT
		{
			public bool Enabled = false;

			/// <summary>
			/// 指定生育能力（与年龄、性别相关）
			/// </summary>
			public bool SpecifiedFecundity = false;

			public int Fecundity = 7500;

			public bool SpecifiedPossibility = false;

			public int Possibility = 50;

			public bool SpecifiedQuQu = false;

			public int QuQu = 10;
		}

		public class NONTR
		{
			public bool Enabled = true;

			public bool[] PreventRelation = new bool[TextConvertHelper.RelationCount] {
			false, false, false, false, false, false, false,
			false, false, false, false, false, false, false};

			public bool[] PreventRelationEx = new bool[TextConvertHelper.ExpandRelationCount] {
				true, true, true
			};

			public bool PreventAll = false;

			/// <summary>
			/// 可以和配偶做（默认和任何人不能做）
			/// </summary>
			public bool AllowCouple = true;
		}


		public BELOVE belove = new BELOVE();

		public RAPE rape = new RAPE();

		public PREGNANT pregnant = new PREGNANT();

		public NONTR nontr = new NONTR();

		public bool DebugMode = false;

        public override void Save(UnityModManager.ModEntry modEntry)
		{
			UnityModManager.ModSettings.Save(this, modEntry);
		}
	}

	/// <summary>
	/// 对于要处理的属性 XX，先将属性值在 XXKey.TryGetIndex 中查找索引，
	/// 然后用索引来获取 XXText 或 Setting 中的对应值
	/// 如果没有 XXKey，则使用 TryGetXXIndex 方法
	/// </summary>
	public static class TextConvertHelper
	{
		public const int LikabilityCount = 8;
		public readonly static int [] LikabilityKey = new int[LikabilityCount] { -1, 0, 1, 2, 3, 4, 5, 6};
		public readonly static string[] LikabilityText = new string[LikabilityCount] { "素未谋面", "陌路", "冷淡", "融洽", "热忱", "喜欢", "亲密", "不渝" };

		public const int GoodnessCount = 5;
		public readonly static int [] GoodnessKey = new int[GoodnessCount] { 2, 1, 0, 3, 4};
		public readonly static string [] GoodnessText = new string[GoodnessCount] { "刚正", "仁善", "中庸", "叛逆", "唯我"};

		public const int CharmCount = 9;
		public readonly static int [] CharmKey = new int[CharmCount] { 0, 1, 2, 3, 4, 5, 6, 7, 8};
		public readonly static string [] CharmText = new string[CharmCount] { "非人", "可憎", "不扬", "寻常", "出众", "瑾瑜/瑶碧", "龙资/凤仪", "绝世/出尘", "天人" };
		public static int GetActorCharm(this DateFile instance, int actorId)
		{
			int CharmValue = int.Parse(instance.GetActorDate(actorId, 15));
			int key = CharmValue / 100;
			if(key < 4) return key;
			return key-1;
		}

		public const int RankCount = 9;
		public readonly static int [] RankKey = new int[RankCount] { 1, 2, 3, 4, 5, 6, 7, 8, 9};
		public readonly static string [] RankText = new string[RankCount] { "一品", "二品", "三品", "四品", "五品", "六品", "七品", "八品", "九品"};
		public static int GetActorRank(this DateFile instance, int actorId)
		{
			if (int.TryParse(instance.GetActorDate(actorId, 20, applyBonus: false), out int result))
			{
				return Mathf.Abs(result);
			}
			return 0;
		}

		public const int RelationCount = 14;
		public readonly static int [] RelationKey = new int[RelationCount]{
			301,
			302, 303, 304,
			305, 307, 308,
			310, 311,
			401, 402,
			601, 602,
			801 };
		/// <summary>
		/// 参考来自 https://www.bilibili.com/read/cv2847101
		/// </summary>
		public readonly static string [] RelationText = new string[RelationCount] {
			"知心之交",
			"兄弟姐妹", "亲父亲母", "义父义母",
			"授业恩师", "恩深义重", "义结金兰",
			"儿女子嗣", "嫡系传人",
			"势不两立", "血海深仇",
			"父系血统", "母系血统",
			"轮回" };

		public const int ExpandRelationCount = 3;
		public readonly static int[] ExpandRelationKey = new int[ExpandRelationCount]{
			306, 309, 312};
		public readonly static string[] ExpandRelationText = new string[ExpandRelationCount] {
			"两情相悦", "结发夫妻", "倾心爱慕"};

		public const int XiangShuCount = 3;
		public readonly static int [] XiangShuKey = new int[XiangShuCount] { 0, 1, 2 };
		public readonly static string [] XiangShuText = new string[XiangShuCount] { "未相枢化", "相枢化魔", "相枢化身"};


		public static bool TryGetIndex<T>(this T[] arr, T key, out int id)
		{
			for(int i=0; i<arr.Length; ++i)
				if(arr[i].Equals(key))
				{
					id = i;
					return true;
				}
			id = -1;
			return false;
		}

		public readonly static string[] colorModifier = new string[9] {
			"#E4504DFF", // 一品赤
			"#F28234FF", // 二品橙
			"#E3C66DFF", // 三品金
			"#AE5AC8FF", // 四品紫
			"#63CED0FF", // 五品蓝
			"#8FBAE7FF", // 六品青
			"#6DB75FFF", // 七品绿
			"#FBFBFBFF", // 八品白
			"#8E8E8EFF"  // 九品灰
			};
		public static string MakeColor(string text, int colorLevel)
		{
			if (colorLevel >= 0 && colorLevel < colorModifier.Length)
			{
				return string.Format("<color={0}>{1}</color>", colorModifier[colorLevel], text);
			}
			return text;
		}
	}
}
