using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;
using UnityEngine.UI;

namespace ToTraditionalChinese
{
    public class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            modEntry.OnToggle = OnToggle;
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
                return false;

            enabled = value;

            return true;
        }
    }

    /// <summary>
    ///  直接Patch UnityEngine.UI中的Text类, 替换返回值为繁体
    /// </summary>
    [HarmonyPatch(typeof(Text), "text", MethodType.Getter)]
    public static class UnityEngine_UI_Text_text_Patch
    {
        private static void Postfix(ref string __result)
        {
            if (!Main.enabled)
            {
                return;
            }

            __result = Util.ToTraditionalChinese(__result);
            return;
        }
    }

    /// <summary>
    ///  转换工具类 烂大街的代码.... 来源已不可考
    /// </summary>
    public static class Util
    {
        private const int LOCALE_SYSTEM_DEFAULT = 0x0800;
        private const int LCMAP_SIMPLIFIED_CHINESE = 0x02000000;
        private const int LCMAP_TRADITIONAL_CHINESE = 0x04000000;

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int LCMapString(int Locale, int dwMapFlags,
        string lpSrcStr, int cchSrc, [Out] string lpDestStr, int cchDest);

        public static string ToTraditionalChinese(string simplified)
        {
            // 文本中不包含中文就直接返回
            if (ContainsChinese(simplified))
            {
                string traditional = new String(' ', simplified.Length);
                int ret = LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_TRADITIONAL_CHINESE, simplified, simplified.Length, traditional, simplified.Length);
                return traditional;
            }
            else
                return simplified;
        }

        private static bool ContainsChinese(this string str)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
        }
    }
}
