
namespace NpcScan
{
    internal class Features
    {
        /// <summary>角色ID</summary>
        public int Key { get; private set; } = 0;
        /// <summary>特性等级</summary>
        public int Level { get; private set; } = 0;
        /// <summary>特性组别</summary>
        public int Group { get; private set; } = 0;
        /// <summary>特性名称</summary>
        public string Name { get; private set; } = "";
        /// <summary>特性类型</summary>
        public int Plus { get; private set; } = 0;
        /// <summary>显示颜色</summary>
        public string Color { get; private set; } = "";
        /// <summary>搜索命中的特性颜色</summary>
        public string TarColor { get; private set; } = "";

        public Features(int i)
        {
            Key = i;
            // 特性等级
            Level = int.Parse(DateFile.instance.actorFeaturesDate[i][4]);
            Group = int.Parse(DateFile.instance.actorFeaturesDate[i][5]);
            Name = DateFile.instance.actorFeaturesDate[i][0];
            Plus = int.Parse(DateFile.instance.actorFeaturesDate[i][8]);
            switch (Plus)
            {
                default:
                    Color = Main.textColor[20003];
                    break;
                case 3:
                    Color = Main.textColor[20006];
                    break;
                case 4:
                    Color = Main.textColor[10004];
                    break;
            }
            TarColor = Main.textColor[20004];
        }
    }
}
