using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Litfal
{
    static public class GUILayoutHelper
    {
        /// <summary>
        /// A title label with center alignment
        /// </summary>
        /// <param name="text"></param>
        static public void Title(string text)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(text);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        static readonly string[] _gongFaBtnStrings = new string[] { "<color=#808080>不切換</color>", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };
        static readonly string[] _equipGroupBtnStrings = new string[] { "<color=#808080>不切換</color>", "壹", "贰", "叁" };

        static public void GongFaSelection(string text, ref int settingField)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(text);
            settingField = GUILayout.SelectionGrid(settingField + 1,
                    _gongFaBtnStrings, 10, GUILayout.Width(660)) - 1;
            GUILayout.EndHorizontal();
        }

        static public void EquipGroupSelection(string text, ref int settingField)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(text);
            settingField = GUILayout.SelectionGrid(settingField + 1,
                    _equipGroupBtnStrings, 4, GUILayout.Width(660)) - 1;
            GUILayout.EndHorizontal();
        }
    }

    static class LogHelper
    {
        public static void DumpStack(this Logger logger, int deep = 20)
        {
            var st = new System.Diagnostics.StackTrace(true);
            for (int i = 2; i < Math.Min(st.FrameCount, deep); i++)
            {
                var sf = st.GetFrame(i);
                var method = sf.GetMethod();
                logger.Log($"Stack: {method.DeclaringType.Name}.{method.Name}");
            }
        }

        public static void DumpStack(this ThreadSafeLogger logger, int deep = 20)
        {
            var st = new System.Diagnostics.StackTrace(true);
            for (int i = 2; i < Math.Min(st.FrameCount, deep); i++)
            {
                var sf = st.GetFrame(i);
                var method = sf.GetMethod();
                logger.Debug($"Stack: {method.DeclaringType.Name}.{method.Name}");
            }
        }
    }
}
