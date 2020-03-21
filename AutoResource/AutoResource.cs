using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using GameData;
using Random = UnityEngine.Random;
using ArchiveSystem;

namespace AutoResource
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool wide;
        public bool per;
        public bool city;
        public string[] lessThan = new string[6];
        public string[] moreThan = new string[6];
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

    }

    public class ReflectionMethod
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static
            | BindingFlags.NonPublic | BindingFlags.Public;
        /// <summary>
        /// 反射执行方法
        /// </summary>
        /// <param name="instance">类实例(静态方法则为null)</param>
        /// <param name="method">方法名</param>
        /// <param name="args">方法的参数类型列表</param>
        /// <typeparam name="T1">类</typeparam>
        /// <typeparam name="T2">返回值类型</typeparam>
        /// <returns></returns>
        public static T2 Invoke<T1, T2>(T1 instance, string method, params object[] args)
        {
            return (T2)typeof(T1).GetMethod(method, Flags)?.Invoke(instance, args);
        }
        /// <summary>
        /// 反射执行方法
        /// </summary>
        /// <param name="instance">类实例(静态方法则为null)</param>
        /// <param name="method">方法名</param>
        /// <param name="args">方法的参数类型列表</param>
        /// <typeparam name="T1">类</typeparam>
        public static void Invoke<T1>(T1 instance, string method, params object[] args)
        {
            typeof(T1).GetMethod(method, Flags)?.Invoke(instance, args);
        }
        /// <summary>
        /// 反射执行方法
        /// </summary>
        /// <param name="instance">类实例(静态方法则为null)</param>
        /// <param name="method">方法名</param>
        /// <param name="argTypes">方法的参数类型列表</param>
        /// <param name="args">参数</param>
        /// <typeparam name="T">类</typeparam>
        /// <returns>函数的返回值(void则返回null)</returns>
        public static object Invoke<T>(T instance, string method, System.Type[] argTypes, params object[] args)
        {
            argTypes = argTypes ?? new System.Type[0];
            var methods = typeof(T).GetMethods(Flags).Where(m =>
            {
                if (m.Name != method)
                    return false;
                return m.GetParameters()
                    .Select(p => p.ParameterType)
                    .SequenceEqual(argTypes);
            });

            if (methods.Count() != 1)
            {
                throw new AmbiguousMatchException("cannot find method to invoke");
            }

            return methods.First()?.Invoke(instance, args);
        }
        /// <summary>
        /// 反射获取类字段的值
        /// </summary>
        /// <param name="instance">类实例(静态字段则为null)</param>
        /// <param name="field">字段名</param>
        /// <typeparam name="T1">类</typeparam>
        /// <typeparam name="T2">返回值类型</typeparam>
        /// <returns>字段的值</returns>
        public static T2 GetValue<T1, T2>(T1 instance, string field)
        {
            return (T2)typeof(T1).GetField(field, Flags)?.GetValue(instance);
        }
        /// <summary>
        /// 反射获取类字段的值
        /// </summary>
        /// <param name="instance">类实例(静态字段则为null)</param>
        /// <param name="field">字段名</param>
        /// <typeparam name="T">类</typeparam>
        /// <returns>字段的值</returns>
        public static object GetValue<T>(T instance, string field)
        {
            return typeof(T).GetField(field, Flags)?.GetValue(instance);
        }
        /// <summary>
        /// 反射设置类字段的值
        /// </summary>
        /// <param name="instance">类实例(静态字段则为null)</param>
        /// <param name="field">字段名</param>
        /// <param name="value">设置的字段的值</param>
        /// <typeparam name="T">类</typeparam>
        public static void SetValue<T>(T instance, string field, object value)
        {
            typeof(T).GetField(field, Flags)?.SetValue(instance, value);
        }
    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static Dictionary<int, Dictionary<int, List<int>>> free, occupied;

        //public static Dictionary<int, List<int>> 
        //    max = new Dictionary<int, List<int>>(),
        //    min = new Dictionary<int, List<int>>();

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());

            settings = Settings.Load<Settings>(modEntry);

            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        static void SetResourceBlock(int resourceId)
        {
            string[] resourceName = new string[] { "食材", "木材", "金石", "织物", "药材", "银钱" };
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal(new GUILayoutOption[] { GUILayout.Width(160f) });
                {
                    GUILayout.FlexibleSpace();
                    string moreThan = GUILayout.TextArea(settings.moreThan[resourceId], new GUILayoutOption[] { GUILayout.Width(40f) });
                    if (GUI.changed)
                        settings.moreThan[resourceId] = moreThan;
                    GUILayout.Label(" ≤ " + resourceName[resourceId] + " ≤ ");
                    string lessThan = GUILayout.TextArea(settings.lessThan[resourceId], new GUILayoutOption[] { GUILayout.Width(40f) });
                    if (GUI.changed)
                        settings.lessThan[resourceId] = lessThan;
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal(new GUILayoutOption[] { GUILayout.Width(160f) });
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("+", new GUILayoutOption[] { GUILayout.Width(40f) }))
                        if (free.ContainsKey(resourceId))
                            ArrangeWork(resourceId, settings.wide);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("");
                    GUILayout.FlexibleSpace();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("-", new GUILayoutOption[] { GUILayout.Width(40f) }))
                        if (occupied.ContainsKey(resourceId))
                            CancelWork(resourceId);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            var instance = DateFile.instance;
            var SaveDataLoad = instance != null && GameData.Characters.HasChar(instance.mianActorId);

            GUILayout.BeginHorizontal();
            settings.wide = GUILayout.Toggle(settings.wide, settings.wide ? "在全地图范围分配" : "仅在当前地图分配",
                new GUILayoutOption[] { GUILayout.Width(160f) });
            settings.city = GUILayout.Toggle(settings.city, "包含城镇", new GUILayoutOption[] { GUILayout.Width(80f) });
            settings.per = settings.city ? GUILayout.Toggle(settings.per, "按每人口单位收益折算", new GUILayoutOption[] { GUILayout.Width(160f) }) : false;
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical("Box");
            GUILayout.Label("♦ 人力资源分配 ♦",
                new GUIStyle { normal = { textColor = new Color(0.999999f, 0.5647058f, 0.3411764f) } });
            if (GUILayout.Button("点击重置mod状态", new GUILayoutOption[] { GUILayout.Width(160f) }))
                PrepareForWork();
            if (SaveDataLoad)
            {
                for (int row = 0; row < 2; ++row)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    for (int resourceId = 0 + row * 3; resourceId < 3 + row * 3; ++resourceId)
                    {
                        SetResourceBlock(resourceId);
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.Label("存档未载入");
            }
            GUILayout.EndVertical();
        }
        private static void UpdateUiManpower()
        {
            var mpg = GameObject.FindObjectOfType<ui_ManPowerManage>();
            if (mpg == null) return;
            ReflectionMethod.Invoke(mpg, "UpdateTotalManPower");
            ReflectionMethod.Invoke(mpg, "UpdateMarkedPlace");
        }

        [HarmonyPatch(typeof(EnterGame), "EnterWorld")]
        public static class EnterGame_EnterWorld_Patch
        {
            private static void Postfix() //初始化列表
            {
                if (!Main.enabled)
                    return;

                Logger.Log("开始初始化列表");
                PrepareForWork();
                Logger.Log("列表初始化完成");
            }
        }

        [HarmonyPatch(typeof(DateFile), "SetMapPlaceShow")]
        public static class DateFile_SetMapPlaceShow_Patch
        {
            static int previous;
            private static void Postfix(int partId, int placeId, bool show, int value = -1, bool update = true)
            {
                if (!Main.enabled)
                    return;
                if (DateFile.instance.mianPartId > 2000)
                    return;
                if (previous != DateFile.instance.HaveShow(partId, placeId))
                {
                    int[] limit = new int[] { 100, 100, 100, 100, 100, 30 };
                    for (int i = 0; i < 6; ++i)
                        // 地块资源符合要求 且 并未损坏
                        if (GetPlaceMaxResource(partId, placeId, i) >= limit[i] && !DateFile.instance.PlaceIsBad(partId, placeId))
                            AddInto(free, i, partId, placeId);
                }
            }
            private static bool Prefix(int partId, int placeId, bool show, int value = -1, bool update = true)
            {
                if (Main.enabled && DateFile.instance.mianPartId < 2000)
                    previous = DateFile.instance.HaveShow(partId, placeId);
                return true;
            }
        }

        private static void AddInto(Dictionary<int, Dictionary<int, List<int>>> d, int resourceId, int partId, int placeId)
        {
            if (!d.ContainsKey(resourceId))
                d.Add(resourceId, new Dictionary<int, List<int>>());
            if (!d[resourceId].ContainsKey(partId))
                d[resourceId].Add(partId, new List<int>());
            if (!d[resourceId][partId].Contains(placeId))
                d[resourceId][partId].Add(placeId);
        }
        private static void RemoveFrom(Dictionary<int, Dictionary<int, List<int>>> d, int resourceId, int partId, int placeId)
        {
            if (!d.ContainsKey(resourceId))
                return;
            if (!d[resourceId].ContainsKey(partId))
                return;
            if (!d[resourceId][partId].Contains(placeId))
                return;
            d[resourceId][partId].Remove(placeId);
            if (d[resourceId][partId].Count == 0)
                d[resourceId].Remove(partId);
            if (d[resourceId].Count == 0)
                d.Remove(resourceId);
        }
        public static void PrepareForWork()
        {
            if (DateFile.instance.mianPartId > 2000)
                return;
            // Dictionary<int resourceId, Dictionary<int partId, List<int placeId>>>
            free = new Dictionary<int, Dictionary<int, List<int>>>();   // 每种资源类型，在每个地区，能够使用的地点
            occupied = new Dictionary<int, Dictionary<int, List<int>>>();   // 每种资源类型，在每个地区，已经被使用的地点

            // 1.找出已经探索过的所有地块
            // 2.筛选出，至少一种资源>=100 / 银钱 >= 30的地块
            // 3.按类别添加到对应字典中
            int[] limit = new int[] { 100, 100, 100, 100, 100, 30 };
            var mapShowDate = DateFile.instance.mapPlaceShowDate;
            foreach (var partId in mapShowDate.Keys)
            {
                List<int> placeIds = mapShowDate[partId].Keys.Where(placeId => mapShowDate[partId][placeId] == 1).ToList();
                // 该地图 已经探索过的地块
                // 符合条件的按类别加入到字典中
                foreach (var placeId in placeIds)
                {
                    for (int i = 0; i < 6; ++i)
                        // 地块资源符合要求 且 并未损坏
                        if (GetPlaceMaxResource(partId, placeId, i) >= limit[i] && !DateFile.instance.PlaceIsBad(partId, placeId))
                            if (!DateFile.instance.HaveWork(partId, placeId))
                                AddInto(free, i, partId, placeId);
                }
            }
            Logger.Log("开始检查占用块");
            var work = DateFile.instance.worldMapWorkState;
            foreach (var partId in work.Keys)
                foreach (var placeId in work[partId].Keys)
                {
                    var resourceId = work[partId][placeId] - 1;
                    //if (free.ContainsKey(resourceId) && free[resourceId].ContainsKey(partId) && free[resourceId][partId].Contains(placeId))
                    //    RemoveFrom(free, resourceId, partId, placeId);
                    if (!occupied.ContainsKey(resourceId) || !occupied[resourceId].ContainsKey(partId) || !occupied[resourceId][partId].Contains(placeId))
                        AddInto(occupied, resourceId, partId, placeId);
                }
            Logger.Log($"{string.Join(", ", occupied.Keys.ToArray())}");
        }
        public static int GetPlaceMaxResource(int partId, int placeId, int resourceId) =>
            settings.per ? DateFile.instance.GetNewMapDate(partId, placeId, resourceId + 1).ParseInt() / DateFile.instance.GetMarkNeedManPower(partId, placeId)
            : DateFile.instance.GetNewMapDate(partId, placeId, resourceId + 1).ParseInt();
        public static void ArrangeWork(int resourceId, bool wide = false)
        {
            if (!free.ContainsKey(resourceId))
                return;
            int rest = UIDate.instance.GetUseManPower();
            if (rest < 1)
                return;
            int partId = -1, placeId = -1;
            var d = free[resourceId];
            int max = 0;
            bool check(int a, int b)
            {
                var t = GetPlaceMaxResource(a, b, resourceId);
                if (t > settings.lessThan[resourceId].ParseInt())
                    return false;
                if (t < settings.moreThan[resourceId].ParseInt())
                    return false;
                var p = DateFile.instance.GetMarkNeedManPower(a, b);
                if (settings.city ? p == 2 : p > rest)
                    return false;
                return t > max && !DateFile.instance.HaveWork(a, b);
            }
            if (wide)
            {
                foreach (var a in d.Keys)
                {
                    var available = d[a].Where((t) => check(a, t)).ToList();
                    if (available.Count > 0)
                    {
                        partId = a;
                        placeId = available.OrderByDescending(t => GetPlaceMaxResource(a, t, resourceId)).First();
                        max = GetPlaceMaxResource(partId, placeId, resourceId);
                    }
                }
            }
            else
            {
                partId = DateFile.instance.mianPartId;
                var available = d[partId].Where(t => check(partId, t)).ToList();
                if (available.Count > 0)
                {
                    placeId = available.OrderByDescending(t => GetPlaceMaxResource(partId, t, resourceId)).First();
                    max = GetPlaceMaxResource(partId, placeId, resourceId);
                }
            }
            if (partId == -1 && placeId == -1)
            {
                Logger.Log("未有符合要求的地点，分配失败");
                return;
            }
            ChoosePlaceWindow.Instance.SetPlaceWork(partId, placeId, resourceId);
            // 维护 occupied 和 free
            RemoveFrom(free, resourceId, partId, placeId);
            AddInto(occupied, resourceId, partId, placeId);
            UpdateUiManpower();
        }
        public static void CancelWork(int resourceId)
        {
            if (!occupied.ContainsKey(resourceId))
                return;
            int partId = -1, placeId = -1;
            var d = occupied[resourceId];
            int min = 999;
            foreach (var a in d.Keys)
            {
                var available = d[a].Where(t => GetPlaceMaxResource(a, t, resourceId) < min).ToList();
                if (available.Count > 0)
                {
                    partId = a;
                    placeId = available.OrderBy(t => GetPlaceMaxResource(a, t, resourceId)).First();
                    min = GetPlaceMaxResource(partId, placeId, resourceId);
                }
            }
            if (partId == -1 || placeId == -1)
            {
                Logger.Log("回收失败");
                return;
            }
            ChoosePlaceWindow.Instance.RemovePlaceWork(partId, placeId);
            DateFile.instance.MarkPlace(partId, placeId, false);
            ChoosePlaceWindow.Instance.UpdateMarkPlaceButton();
            ChoosePlaceWindow.Instance.UpdateLookToChoosePlaceButton();
            // 维护 free 和 occupied
            RemoveFrom(occupied, resourceId, partId, placeId);
            AddInto(free, resourceId, partId, placeId);
            UpdateUiManpower();
        }
    } // End of Main
}

