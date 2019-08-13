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
    }

    static class LogHelper
    {
        public static void DumpStack(this Logger logger, int deep = 20)
        {
            var st = new System.Diagnostics.StackTrace(true);
            for (int i = 2; i < Math.Min(st.FrameCount, deep); i++)
            {
                // Note that high up the call stack, there is only
                // one stack frame.
                var sf = st.GetFrame(i);
                var method = sf.GetMethod();
                logger.Log($"Stack: {method.DeclaringType.Name}.{method.Name}");
            }
        }
    }
}
