using GameData;
using Harmony12;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NpcScan
{
    /// <summary>
    /// Hook游戏中的方法
    /// </summary>
    internal static class Patch
    {
        /// <summary>是否需要启用<see cref="DateFile.GetActorFeature(int key)"/>补丁</summary>
        private static bool isGetFeaturePatch;
        /// <summary>角色特性缓存(线程安全)</summary>
        private static ConcurrentDictionary<int, List<int>> actorsFeatureCache;

        /// <summary>
        /// 启用<see cref="DateFile.GetActorFeature"/>的线程安全补丁
        /// </summary>
        public static void StartGetFeaturePatch()
        {
            isGetFeaturePatch = true;
            actorsFeatureCache = new ConcurrentDictionary<int, List<int>>();
        }

        /// <summary>
        /// 停止<see cref="DateFile.GetActorFeature"/>的线程安全补丁
        /// </summary>
        public static void StopGetFeaturePatch()
        {
            isGetFeaturePatch = false;
            actorsFeatureCache = null;
        }

        /// <summary>
        /// 使<see cref="DateFile.GetActorFeature(int key)"/>线程安全，并显示儿童隐藏特性
        /// </summary>
        [HarmonyPatch(typeof(DateFile), "GetActorFeature", new Type[] { typeof(int), typeof(bool) })]
        internal static class DateFile_GetActorFeature_Patch
        {
            public static bool Prefix(int key, ref List<int> __result)
            {
                if (!Main.enabled || !isGetFeaturePatch)
                    return true;

                if (actorsFeatureCache.TryGetValue(key, out __result))
                    return false;

                var features = new List<int>();
                string charProperty = Characters.GetCharProperty(key, 101);
                string[] actorFeatures = charProperty.IsNullOrEmpty() ? Array.Empty<string>() : charProperty.Split('|');

                for (int j = 0; j < actorFeatures.Length; j++)
                {
                    int fetureKey = int.Parse(actorFeatures[j]);
                    if (fetureKey != 0 && DateFile.instance.actorFeaturesDate.ContainsKey(fetureKey))
                        features.Add(fetureKey);
                }
                actorsFeatureCache.TryAdd(key, features);
                __result = features;
                return false;
            }
        }
    }
}
