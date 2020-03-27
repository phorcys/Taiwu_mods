using GameData;
using Harmony12;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DreamLover
{
    /// <summary>
    /// 模块信息，淦啦，根本不懂程序该怎么写
    /// 只考虑了 Prefix，以后再考虑别的
    /// </summary>
    public class PatchModuleInfo
    {
        public string Name;

        private MethodInfo patchBase;
        private MethodInfo patch;
        private bool isPatched = false;
        public PatchModuleInfo(Type baseType, string baseName, Type patchType)
        {
            patchBase = AccessTools.Method(baseType, baseName);
            patch = AccessTools.Method(patchType, "Prefix");
            Name = patchType.Name;
        }
        /// <summary>
        /// 仅用于加载 Prefix，懒得考虑兼容
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool Patch(HarmonyInstance instance)
        {
            if(isPatched) return false;
            if(patchBase == null || patch == null) return false;
            instance.Patch(patchBase, prefix: new HarmonyMethod(patch));
            isPatched = true;
            return true;
        }
        public bool Unpatch(HarmonyInstance instance)
        {
            if(!isPatched) return false;
            instance.Unpatch(patchBase, patch);
            isPatched = false;
            return true;
        }
    }
}