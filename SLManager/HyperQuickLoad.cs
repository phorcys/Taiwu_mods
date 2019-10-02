//#define ENABLE_HYPER_QUICK_LOAD
using Harmony12;
using Ionic.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ArchiveSystem;
using ArchiveSystem.GameData;
using UnityModManagerNet;

namespace Sth4nothing.SLManager.HyperQuickLoad
{
    class InitClass
    {
#if (ENABLE_HYPER_QUICK_LOAD)

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("讀檔後使用高速快速讀取 (魔改危險)", GUILayout.Width(250));
            Main.settings.enableTurboQuickLoadAfterLoad = GUILayout.SelectionGrid(Main.settings.enableTurboQuickLoadAfterLoad ? 1 : 0,
                                         Main.Off_And_On, 2, GUILayout.Width(150)) == 1;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.Label("存檔後使用高速快速讀取 (魔改危險)", GUILayout.Width(250));
            Main.settings.enableTurboQuickLoadAfterSave = GUILayout.SelectionGrid(Main.settings.enableTurboQuickLoadAfterSave ? 1 : 0,
                                         Main.Off_And_On, 2, GUILayout.Width(150)) == 1;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
#else
        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            Main.settings.enableTurboQuickLoadAfterLoad = false;
            Main.settings.enableTurboQuickLoadAfterSave = false;
        }
#endif
    }
#if (ENABLE_HYPER_QUICK_LOAD)
    class SaveCacheFactory
    {
        static Dictionary<Type, ISaveCache> _dict_instances = new Dictionary<Type, ISaveCache>();

        static public SaveCache<T> GetInstance<T>()
        {
            lock (_dict_instances)
            {
                if (!_dict_instances.TryGetValue(typeof(T), out ISaveCache instance))
                {
                    instance = new SaveCache<T>();
                    _dict_instances[typeof(T)] = instance;
                }
                return (SaveCache<T>)instance;
            }
        }

        public static void WaitAll()
        {
            foreach (var saveCache in _dict_instances.Values)
            {
                saveCache.EndSetCloneCache();
            } 
        }
    }

    interface ISaveCache
    {
        void EndSetCloneCache();
        void ExpireCache();
    }

    class SaveCache<T> : ISaveCache
    {
        Action<T, T> _deepCopyAction;
        Action<T, T> _shallowCopyAction;
        private T _cache;
        object _lock = new object();
        Task<T> _cloneTask;
        // Task<>

        public SaveCache()
        {
            _deepCopyAction = (new DeepCopier.DeepCopier<T>()).CompileAllDeepCopyFieldAction();
            _shallowCopyAction = (new DeepCopier.DeepCopier<T>()).CompileAllShallowCopyFieldAction();
        }

        public T GetCache()
        {
            if (_cloneTask?.IsCompleted == false)
                _cloneTask.Wait();
            lock (_lock)
            {
                return _cache;
            }
            return default(T);
        }

        public void SetCloneCache(T data)
        {
            Clone(data);
        }

        public Task StartSetCloneCache( T data)
        {
            var sd = (T)Activator.CreateInstance(typeof(T));
            _shallowCopyAction(data, sd);
            _cloneTask = StartClone(sd);
            return _cloneTask;
        }

        public void EndSetCloneCache()
            => _cloneTask?.Wait();


        private Task<T> StartClone(T data)
            => Task.Run(() => Clone(data));

        private T Clone(T data)
        {

#if DEBUG
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
#endif
            try
            {
                var newCache = (T)Activator.CreateInstance(typeof(T));
                _deepCopyAction(data, newCache);
                lock (_lock)
                {
                    _cache = newCache;
                }
#if DEBUG
                Main.Logger.Log($"Cache<{typeof(T).Name}> clone finished: {stopwatch.ElapsedMilliseconds} ms");
#endif
                return newCache;
            }
            catch (Exception ex)
            {
                Main.Logger.Log($"Cache<{typeof(T).Name}> clone failed, error: \r\n{ex}");
                return default(T);
            }
        }

        public void ExpireCache()
        {
            lock (_lock)
            {
                _cache = default(T);
            }
        }
    }

    [HarmonyAfter(new string[] { "FastLoad.DefaultData_Load_Patch" })]
    // 攔截讀檔時(後)
    [HarmonyPatch(typeof(DefaultData), "Load")]
    public class DefaultData_Load_Patch
    {
        static System.Diagnostics.Stopwatch _stopwatch;

        static SaveCache<DateFile.SaveDate> _saveCache = SaveCacheFactory.GetInstance<DateFile.SaveDate>();

        static object _lastCalledLoadingState = null;

        private static bool Prefix(ref object __result)
        {
            if (!Main.Enabled) return true;
            _stopwatch = System.Diagnostics.Stopwatch.StartNew();
            if (!StateHelper.IsQuickLoad ||
                !StateHelper.IsIntoGame())
                return true;

            var data = _saveCache.GetCache();
            if (data != null)
            {
                Main.Logger.Log($"DefaultData.Load use cache.");
                __result = data;
                return false;
            }
            return true;
        }
        private static void Postfix(object __result)
        {
            if (!Main.Enabled) return;
            if (Main.settings.enableTurboQuickLoadAfterLoad &&
                StateHelper.IntoGameIndex > 0)
            {
                if (_lastCalledLoadingState == StateHelper.LoadingState)
                {
#if DEBUG
                    Main.Logger.Log($"In the same LoadingState, skip to cache");
#endif
                    return;
                }
                _lastCalledLoadingState = StateHelper.LoadingState;

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                _saveCache.StartSetCloneCache((DateFile.SaveDate)__result);
            }
#if DEBUG
            Main.Logger.Log($"DefaultData.Load: {_stopwatch?.ElapsedMilliseconds} ms");
#endif
        }
    }


    // 攔截存檔時(後)
    [HarmonyPatch(typeof(DateFile.SaveDate), "FillDate")]
    public class SaveDate_FillDate_Patch
    {
        static SaveCache<DateFile.SaveDate> _saveCache = SaveCacheFactory.GetInstance<DateFile.SaveDate>();
        private static void Postfix(DateFile.SaveDate __instance)
        {
            _saveCache.ExpireCache();
            if (!Main.Enabled)
                return;
            if (Main.settings.enableTurboQuickLoadAfterSave)
            {
                _saveCache.StartSetCloneCache(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(WorldData), "Load")]
    public class WorldData_Load_Patch
    {
        static System.Diagnostics.Stopwatch _stopwatch;
        static SaveCache<DateFile.WorldDate> _saveCache = SaveCacheFactory.GetInstance<DateFile.WorldDate>();

        private static bool Prefix(ref object __result)
        {
            if (!Main.Enabled) return true;
            _stopwatch = System.Diagnostics.Stopwatch.StartNew();
            if (!StateHelper.IsQuickLoad ||
                !StateHelper.IsIntoGame())
                return true;
            var data = _saveCache.GetCache();
            if (data != null)
            {
                Main.Logger.Log($"WorldData.Load use cache.");
                __result = data;
                return false;
            }
            return true;
        }
        private static void Postfix(object __result)
        {
            if (!Main.Enabled) return;
            if (Main.settings.enableTurboQuickLoadAfterLoad &&
                StateHelper.IntoGameIndex > 0)
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                _saveCache.StartSetCloneCache((DateFile.WorldDate)__result);
            }
#if DEBUG
            Main.Logger.Log($"WorldData.Load: {_stopwatch?.ElapsedMilliseconds} ms");
#endif
        }
    }

    // 攔截存檔時(後)
    [HarmonyPatch(typeof(DateFile.WorldDate), "FillDate")]
    public class WorldDate_FillDate_Patch
    {
        static SaveCache<DateFile.WorldDate> _saveCache = SaveCacheFactory.GetInstance<DateFile.WorldDate>();
        private static void Postfix(DateFile.WorldDate __instance)
        {
            _saveCache.ExpireCache();
            if (!Main.Enabled)
                return;
            if (Main.settings.enableTurboQuickLoadAfterSave)
            {
                _saveCache.StartSetCloneCache(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(DateFile), "NewDate")]
    public class DateFile_NewDate_Patch
    {
        static public System.Diagnostics.Stopwatch Stopwatch = new System.Diagnostics.Stopwatch();
        private static void Prefix()
        {
            Stopwatch.Restart();
        }
    }


    // GEvent.OnEvent(eEvents.LoadingProgress, 100)
    [HarmonyPatch(typeof(GEvent), "OnEvent")]
    public class GEvent_OnEvent_Patch
    {
        private static void Prefix(Enum _em, object[] args)
        {
            if(eEvents.LoadingProgress.Equals(_em) &&
               100 == (int)args[0])
            {
#if DEBUG
                Main.Logger.Log($"Loading spends {DateFile_NewDate_Patch.Stopwatch.ElapsedMilliseconds} ms");
#endif
                SaveCacheFactory.WaitAll();
                StateHelper.IsQuickLoad = false;
            }
        }
    }



    // 在存檔作業完成前, 確保Cache已建立完成
    [HarmonyPatch(typeof(SaveGame), "DoSavingAgent")]
    public class SaveGame_DoSavingAgent_Patch
    {
        private static void Postfix()
        {
#if DEBUG
            Main.Logger.Log($"Wait cache finish after DoSavingAgent");
#endif
            SaveCacheFactory.WaitAll();
        }
    }


    // 攔截讀取並進入遊戲的行為, 用以控制狀態
    [HarmonyPatch(typeof(DateFile), "Initialize", new Type[] { typeof(int), typeof(bool) })]
    class DateFile_Initialize_Patch
    {
        private static void Prefix(int dateId)
        {
            StateHelper.IntoGameIndex = dateId;
            StateHelper.LoadingState = new object();
        }
    }

    // 攔截讀取並進入遊戲的行為, 用以控制狀態
    [HarmonyPatch(typeof(DateFile), "Initialize", new Type[] { typeof(BackupItem) })]
    class DateFile_Initialize2_Patch
    {
        private static void Prefix(BackupItem item)
        {
            StateHelper.IntoGameIndex = item.DataId;
            StateHelper.LoadingState = new object();
        }
    }


    [HarmonyPatch(typeof(DateFile), "BackToStartMenu")]
    public class MainMenu_BackToMainWindow_Patch
    {
        private static void Prefix()
        {
            StateHelper.IntoGameIndex = 0;
            StateHelper.LoadingState = null;
        }
    }
#endif

}

