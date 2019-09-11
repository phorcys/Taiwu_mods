using System;
using System.Text.RegularExpressions;

namespace NpcScan
{
    /// <summary>
    /// 角色物品信息类
    /// </summary>
    /// <remarks>
    /// 该类建立速度较慢，大量计算而又不需要输出名称和品级时推荐使用
    /// <see cref="ValueTuple(int BaseId, int QuquId, int QuquPartId)"/>作为对象
    /// 三种ID共同唯一确定物品的名称和品阶
    /// <seealso cref="ValueTuple"/>
    /// 而只在需要输出排序时创建本类的实例
    /// </remarks>
    internal class Item : IComparable<Item>, IEquatable<Item>
    {
        public static readonly Regex itemNameRegex = new Regex(@"^(?>[^\(\r\n]+)");

        /// <summary>物品的基础ID, <see cref="DateFile.presetitemDate"/></summary>
        public int BaseId { get; private set; }
        /// <summary>促织ID</summary>
        public int QuquId { get; private set; }
        /// <summary>促织部位ID</summary>
        public int QuquPartId { get; private set; }
        /// <summary>品阶</summary>
        public int Grade { get; private set; }
        /// <summary>名称</summary>
        public string Name { get; private set; }

        /// <summary>
        /// 角色物品信息类
        /// </summary>
        /// <param name="itemId">物品的ID</param>
        /// <remarks>基础ID、促织ID和促织部位ID共同唯一确定任何物品的品阶和名称</remarks>
        public Item(int itemId)
        {
            BaseId = int.Parse(DateFile.instance.GetItemDate(itemId, 999, false));
            QuquId = int.Parse(DateFile.instance.GetItemDate(itemId, 2002, false));
            QuquPartId = int.Parse(DateFile.instance.GetItemDate(itemId, 2003, false));
            Grade = int.Parse(DateFile.instance.GetItemDate(itemId, 8, false));
            Name = DateFile.instance.GetItemDate(itemId, 0, false)
                   + (int.Parse(DateFile.instance.GetItemDate(itemId, 31, false)) == 17
                      && int.Parse(DateFile.instance.GetItemDate(itemId, 35, false)) == 1
                      ? "(抄)" : ""); // 标记手抄本
        }

        /// <summary>
        /// 依次按照品级、基础ID、促织ID和促织部位ID排序
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int CompareTo(Item item)
        {
            if (item == null)
                return 1;
            int result = Grade.CompareTo(item.Grade);
            if (result != 0)
                return result;
            result = BaseId.CompareTo(item.BaseId);
            if (result != 0)
                return result;
            result = QuquId.CompareTo(item.QuquId);
            if (result != 0)
                return result;
            return QuquPartId.CompareTo(item.QuquPartId);
        }

        /// <summary>
        /// 获取Item的HashCode
        /// </summary>
        /// <returns></returns>
        /// <remarks>用于依赖HashCode的Collection，如HashSet、Dictionary</remarks>
        public override int GetHashCode() => CombineHashCode(CombineHashCode(BaseId.GetHashCode(), QuquId.GetHashCode()), QuquPartId.GetHashCode());

        /// <summary>
        /// 判断是否相等
        /// </summary>
        /// <param name="other"></param>
        /// <remarks>用于依赖判断大小的Collection，如HashSet、Dictionary、SortedDictionary、SortedSet等</remarks>
        public bool Equals(Item other)
        {
            if (other == null) return false;
            return BaseId == other.BaseId && QuquId == other.QuquId && QuquPartId == other.QuquPartId;
        }

        public override bool Equals(object obj)
        {
            if (obj is Item item)
            {
                return Equals(item);
            }
            return false;
        }

        /// <summary>
        /// 去掉颜色标签并只保留物品名称第一行括号前面的内容
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string PurifyItemName(string name) => UI.colorTagRegex.Replace(itemNameRegex.Match(name).Value, "");

        /// <summary>
        /// 组合多个HashCode
        /// </summary>
        /// <param name="h1"></param>
        /// <param name="h2"></param>
        /// <returns></returns>
        private static int CombineHashCode(int h1, int h2)
        {
            uint num = (uint)((h1 << 5) | (int)((uint)h1 >> 27));
            return ((int)num + h1) ^ h2;
        }
    }
}
