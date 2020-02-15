using UnityModManagerNet;

namespace DreamLover
{
	public class Setting : UnityModManager.ModSettings
	{
		public int 菜单打开情况 = 0;

		public bool 主动爱慕 = true;

		public bool 主动表白 = true;

		public bool 主动求婚 = true;

		public bool 太吾不爱的人也表白太吾 = false;

		public bool 不顾门派爱太吾 = false;

		public bool 已婚人士想和太吾结婚 = false;

		public bool 即使太吾已婚别人也想求婚 = false;

		public bool 即使出家也要求婚太吾 = false;

		public bool 即使太吾出家也要求婚 = false;

		public bool 男的可以来 = false;

		public bool 女的可以来 = true;

		public bool 全世界都喜欢太吾 = false;

		public SerializableDictionary<int, bool> 可接受的好感度等级 = new SerializableDictionary<int, bool>
		{
			{ 0, false}, { 1, false}, { 2, false}, { 3, false},
            { 4, true}, { 5, true}, { 6, true}, { -1, false}
		};

		public SerializableDictionary<int, bool> 可接受的立场等级 = new SerializableDictionary<int, bool>
        {
            { 0, true}, { 1, true}, { 2, false}, { 3, false}, { 4, false}
		};

		public SerializableDictionary<int, bool> 可接受的魅力等级 = new SerializableDictionary<int, bool>
		{
			{ 0, false}, { 1, false}, { 2, false}, { 3, true}, { 4, true},
            { 5, true}, { 6, true}, { 7, true}, { 8, true}, { 9, true}
		};

		public SerializableDictionary<int, bool> 可接受的阶层等级 = new SerializableDictionary<int, bool>
		{
            { 1, false}, { 2, false}, { 3, true}, { 4, true},
            { 5, true}, { 6, true}, { 7, true}, { 8, true}, { 9, true}
		};

		public int 入魔程度 = 0;

		public bool 兄弟姐妹 = false;

		public bool 亲生父母 = false;

		public bool 义父义母 = false;

		public bool 授业恩师 = false;

		public bool 义结金兰 = false;

		public bool 儿女子嗣 = false;

		public bool 嫡系传人 = false;

		public bool 势不两立 = false;

		public bool 血海深仇 = false;

		public bool 父系血统 = false;

		public bool 母系血统 = false;

		public int 年龄下限 = 14;

		public int 年龄上限 = 60;

		public bool 启用主动欺辱功能 = false;

		public bool 指定主动欺辱概率 = false;

		public int 主动欺辱概率 = 10;

		public bool 跳过战力检定 = false;

		public bool 主动欺辱影响双方情绪 = true;

		public bool 主动欺辱结仇 = true;

		public bool 主动欺辱的孩子有双亲 = false;

		public bool 主动欺辱筛选项 = false;

		public bool 主动欺辱人名筛选 = false;

		public string 人名字符串片段 = "";

		public bool 主动欺辱爱慕筛选 = true;

		public bool 主动欺辱异性筛选 = false;

		public bool 启用指定怀孕功能 = false;

        public bool 指定生育可能性 = false;

		public int 生育可能性 = 7500;

		public bool 指定怀孕概率 = false;

		public int 怀孕概率 = 50;

		public bool 指定蛐蛐概率 = false;

		public int 蛐蛐概率 = 10;

        public bool 启用防绿功能 = true;

        public bool 太吾爱慕的人不能与他人发生关系 = true;

        public bool 爱慕太吾的人不能与他人发生关系 = false;

        public bool 和太吾之外的人都不能发生关系 = false;

		public bool debugmode = false;

        public SerializableDictionary<int, bool> 爱慕关系 = new SerializableDictionary<int, bool>
        {
            { 312, true}, { 306, true}, { 309, true}
        };

        public override void Save(UnityModManager.ModEntry modEntry)
		{
			UnityModManager.ModSettings.Save(this, modEntry);
		}
	}
}
