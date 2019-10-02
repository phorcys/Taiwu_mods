using System;
using UnityEngine;
using UnityEngine.UI;

namespace Majordomo
{
    /// <summary>
    /// 游戏中一些稳定的通用方法，基本会为了便捷性做一些轻微改动
    /// </summary>
    public class TaiwuCommon
    {
        public const int COLOR_BLACK = 10000;
        public const int COLOR_DARK_GRAY = 10001;
        public const int COLOR_DARK_BROWN = 10002;
        public const int COLOR_LIGHT_BROWN = 10003;
        public const int COLOR_DARK_RED = 10004;
        public const int COLOR_LIGHT_YELLOW = 10005;
        public const int COLOR_PINK = 10006;
        public const int COLOR_RICE_WHITE = 20001;
        public const int COLOR_LIGHT_GRAY = 20002;
        public const int COLOR_WHITE = 20003;
        public const int COLOR_LIGHT_GREEN = 20004;
        public const int COLOR_LIGHT_BLUE = 20005;
        public const int COLOR_CYAN = 20006;
        public const int COLOR_DARK_PURPLE = 20007;
        public const int COLOR_YELLOW = 20008;
        public const int COLOR_ORANGE = 20009;
        public const int COLOR_RED = 20010;
        public const int COLOR_GOLDEN = 20011;

        public const int COLOR_LOWEST_LEVEL = 20002;


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


        /// <summary>
        /// 获取受伤程度（内伤与外伤中的最大值）
        /// 0 表示未受伤，1 表示受了 100% 的伤
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        public static float GetInjuryRate(int actorId)
        {
            float hp = DateFile.instance.Hp(actorId);
            float maxHp = DateFile.instance.MaxHp(actorId);
            float hpInjuryRate = hp / maxHp;

            float sp = DateFile.instance.Sp(actorId);
            float maxSp = DateFile.instance.MaxSp(actorId);
            float spInjuryRate = sp / maxSp;

            return Mathf.Max(hpInjuryRate, spInjuryRate);
        }


        /// <summary>
        /// 获取内息受阻程度
        /// 0 表示未受阻，1 表示阻碍程度 100%
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        public static float GetCirculatingBlockingRate(int actorId)
        {
            const int MAX_CIRCULATING_BLOCKING = 8000;

            float circulating = DateFile.instance.GetActorMianQi(actorId);
            return circulating / MAX_CIRCULATING_BLOCKING;
        }


        /// <summary>
        /// 获取中毒程度（取所有毒素中毒程度的最大值）
        /// 0 表示未中毒，1 表示中毒程度 100%
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        public static float GetPoisoningRate(int actorId)
        {
            const int N_POISON_TYPES = 6;
            float maxPoisoningRate = 0;

            for (int i = 0; i < N_POISON_TYPES; ++i)
            {
                float poisoning = DateFile.instance.Poison(actorId, i);
                float maxPoisoning = DateFile.instance.MaxPoison(actorId, i);
                maxPoisoningRate = Mathf.Max(poisoning / maxPoisoning, maxPoisoningRate);
            }

            return maxPoisoningRate;
        }


        /// <summary>
        /// 获取寿命受损程度
        /// 0 表示未受损，1 表示受损程度程度 100%
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        public static float GetLifespanDamageRate(int actorId)
        {
            float health = DateFile.instance.Health(actorId);
            float maxHealth = DateFile.instance.MaxHealth(actorId);

            return (maxHealth - health) / maxHealth;
        }
    }
}
