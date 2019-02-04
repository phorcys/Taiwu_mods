using System;

namespace Majordomo
{
    public class TaiwuDate : IComparable<TaiwuDate>
    {
        public static readonly int N_SOLAR_TERMS = 24;

        public int year;
        public int solarTerm;   // 二十四节气 index，一年中第一个节气 index 为 0


        /// <summary>
        /// 以游戏当前日期初始化
        /// </summary>
        public TaiwuDate()
        {
            this.year = DateFile.instance.year;
            this.solarTerm = N_SOLAR_TERMS - DateFile.instance.dayTrun;
        }


        public TaiwuDate(int year, int solarTerm)
        {
            this.year = year;
            this.solarTerm = solarTerm;
        }


        public int GetMonthIndex()
        {
            return this.solarTerm / 2;
        }


        public override int GetHashCode()
        {
            return this.year * N_SOLAR_TERMS + this.solarTerm;
        }


        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is TaiwuDate))
                return false;
            else
            {
                TaiwuDate other = obj as TaiwuDate;
                return this.year == other.year && this.solarTerm == other.solarTerm;
            }
        }


        public int CompareTo(TaiwuDate other)
        {
            if (other == null) return 1;

            if (this.year < other.year)
                return -1;
            else if (this.year > other.year)
                return 1;

            return this.solarTerm.CompareTo(other.solarTerm);
        }


        public override string ToString()
        {
            return TaiwuCommon.SetColor(TaiwuCommon.COLOR_DARK_BROWN,
                string.Format("第 {0} 年 {1} 月",
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_BROWN, this.year.ToString()),
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_BROWN, (this.GetMonthIndex() + 1).ToString())));
        }
    }
}
