using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWear
{

    static class DateFileHelper
    {
        public static bool CanInToPlaceHome(this DateFile df)
        {
            return df.baseHomeDate[df.mianPartId].ContainsKey(df.mianPlaceId);
        }

        public static int GetItemDateValue(this DateFile df, int itemId, ItemDateKey itemDateKey)
            => GetItemDateValue(df, itemId, (int)itemDateKey);

        public static int GetItemDateValue(this DateFile df, int itemId, int itemDateKey)
        {
            if (int.TryParse(DateFile.instance.GetItemDate(itemId, itemDateKey), out int value))
                return value;
            return 0;
        }
    }

    public enum ItemDateKey : int
    {
        /// <summary>
        /// 裝備種類
        /// </summary>
        EquipType = 1,
        /// <summary>
        /// 悟性
        /// </summary>
        Comprehension = 50065,
        /// <summary>
        /// 內息
        /// </summary>
        MianQi = 50083,
        /// <summary>
        /// 書本習得的技能Id
        /// </summary>
        BookSkillId = 32,

        /// <summary>音律</summary>
        [Description("音律")]
        Aptitude001 = 50501,
        /// <summary>弈棋</summary>
        [Description("弈棋")]
        Aptitude002 = 50502,
        /// <summary>诗书</summary>
        [Description("诗书")]
        Aptitude003 = 50503,
        /// <summary>绘画</summary>
        [Description("绘画")]
        Aptitude004 = 50504,
        /// <summary>术数</summary>
        [Description("术数")]
        Aptitude005 = 50505,
        /// <summary>品鉴</summary>
        [Description("品鉴")]
        Aptitude006 = 50506,
        /// <summary>锻造</summary>
        [Description("锻造")]
        Aptitude007 = 50507,
        /// <summary>制木</summary>
        [Description("制木")]
        Aptitude008 = 50508,
        /// <summary>医术</summary>
        [Description("医术")]
        Aptitude009 = 50509,
        /// <summary>毒术</summary>
        [Description("毒术")]
        Aptitude010 = 50510,
        /// <summary>织锦</summary>
        [Description("织锦")]
        Aptitude011 = 50511,
        /// <summary>巧匠</summary>
        [Description("巧匠")]
        Aptitude012 = 50512,
        /// <summary>道法</summary>
        [Description("道法")]
        Aptitude013 = 50513,
        /// <summary>佛法</summary>
        [Description("佛法")]
        Aptitude014 = 50514,
        /// <summary>厨艺</summary>
        [Description("厨艺")]
        Aptitude015 = 50515,
        /// <summary>杂学</summary>
        [Description("杂学")]
        Aptitude016 = 50516,
        /// <summary>内功</summary>
        [Description("内功")]
        Aptitude101 = 50601,
        /// <summary>身法</summary>
        [Description("身法")]
        Aptitude102 = 50602,
        /// <summary>绝技</summary>
        [Description("绝技")]
        Aptitude103 = 50603,
        /// <summary>拳掌</summary>
        [Description("拳掌")]
        Aptitude104 = 50604,
        /// <summary>指法</summary>
        [Description("指法")]
        Aptitude105 = 50605,
        /// <summary>腿法</summary>
        [Description("腿法")]
        Aptitude106 = 50606,
        /// <summary>暗器</summary>
        [Description("暗器")]
        Aptitude107 = 50607,
        /// <summary>剑法</summary>
        [Description("剑法")]
        Aptitude108 = 50608,
        /// <summary>刀法</summary>
        [Description("刀法")]
        Aptitude109 = 50609,
        /// <summary>长兵</summary>
        [Description("长兵")]
        Aptitude110 = 50610,
        /// <summary>奇门</summary>
        [Description("奇门")]
        Aptitude111 = 50611,
        /// <summary>软兵</summary>
        [Description("软兵")]
        Aptitude112 = 50612,
        /// <summary>御射</summary>
        [Description("御射")]
        Aptitude113 = 50613,
        /// <summary>乐器</summary>
        [Description("乐器")]
        Aptitude114 = 50614,

    }


    public enum EquipType : int
    {
        Weapon = 1,
        Armor = 2,
        Accessory = 3,
        Clothing = 4,
    }


    public enum EquipSlot
    {
        Weapon1 = 0,
        Weapon2 = 1,
        Weapon3 = 2,
        Headwear = 3,
        Clothing = 4,
        BodyArmor = 5,
        Shoes = 6,
        Accessory1 = 7,
        Accessory2 = 8,
        Accessory3 = 9,
        Travel = 10,
        Ququ = 11,
    }

    static public class EquipSlotHelper
    {
        public static ActorsDateKey ToActorsDateKey(this EquipSlot slot)
        {
            return (ActorsDateKey)(slot + (int)ActorsDateKey.Weapon1);
        }
    }

    public enum ActorsDateKey
    {
        Weapon1 = 301,
        Weapon2 = 302,
        Weapon3 = 303,
        Headwear = 304,
        Clothing = 305,
        BodyArmor = 306,
        Shoes = 307,
        Accessory1 = 308,
        Accessory2 = 309,
        Accessory3 = 310,
        Travel = 311,
        Ququ = 312,
    }

    static public class EnumHelper
    {

        public static string GetDescription<Enum>(this Enum source)
        {
            System.Reflection.FieldInfo fi = source.GetType().GetField(source.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
             typeof(DescriptionAttribute), false);
            if (attributes.Length > 0) return attributes[0].Description;
            else return source.ToString();
        }
    }
}
