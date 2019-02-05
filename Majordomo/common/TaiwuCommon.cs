using System;
using UnityEngine.UI;

namespace Majordomo
{
    public class TaiwuCommon
    {
        public static readonly int COLOR_BLACK = 10000;
        public static readonly int COLOR_DARK_GRAY = 10001;
        public static readonly int COLOR_DARK_BROWN = 10002;
        public static readonly int COLOR_LIGHT_BROWN = 10003;
        public static readonly int COLOR_DARK_RED = 10004;
        public static readonly int COLOR_LIGHT_YELLOW = 10005;
        public static readonly int COLOR_PINK = 10006;
        public static readonly int COLOR_RICE_WHITE = 20001;
        public static readonly int COLOR_LIGHT_GRAY = 20002;
        public static readonly int COLOR_WHITE = 20003;
        public static readonly int COLOR_LIGHT_GREEN = 20004;
        public static readonly int COLOR_LIGHT_BLUE = 20005;
        public static readonly int COLOR_CYAN = 20006;
        public static readonly int COLOR_DARK_PURPLE = 20007;
        public static readonly int COLOR_YELLOW = 20008;
        public static readonly int COLOR_ORANGE = 20009;
        public static readonly int COLOR_RED = 20010;
        public static readonly int COLOR_GOLDEN = 20011;

        public static readonly int COLOR_LOWEST_LEVEL = 20002;


        public static string SetColor(int colorId, string text)
        {
            return DateFile.instance.massageDate[colorId][0] + text + "</color>";
        }


        public static void SetFont(Text text, bool isTitle = false)
        {
            if (isTitle)
            {
                text.font = DateFile.instance.boldFont;
            }
            else
            {
                text.font = DateFile.instance.font;
                text.fontSize -= 2;
            }
        }
    }
}
