using Harmony12;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;
using UnityModManagerNet;

namespace Sth4nothing.SLManager
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public bool blockAutoSave = false;
        public int maxBackupToLoad = 8;
        public int maxBackupsToKeep = 1000;
    }

    public static class Main
    {
        public static bool Enabled { get; private set; }
        public static bool ForceSave = false;
        public static bool onLoad = false;

        private static string logPath;
        private static readonly string[] AutoSaveState = {"关闭", "启用"};

        public static Settings settings;

        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var userProfile = System.Environment.GetEnvironmentVariable("USERPROFILE");
            logPath = Path.Combine(userProfile,
                @"AppData\LocalLow\Conch Ship Game\The Scroll Of Taiwu Alpha V1.0\output_log.txt"
            );

            Logger = modEntry.Logger;
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            return true;
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("每个存档槽最大保留备份数量(0-1000)：");

            if (int.TryParse(GUILayout.TextField(settings.maxBackupsToKeep.ToString()),
                out int maxBackupsToKeep))
            {
                if (maxBackupsToKeep <= 1000 && maxBackupsToKeep >= 0)
                    settings.maxBackupsToKeep = maxBackupsToKeep;
            }

            GUILayout.Label("读档列表的最大存档数(0表示不受限制)");
            if (int.TryParse(GUILayout.TextField(settings.maxBackupToLoad.ToString()),
                out int maxBackupToLoad))
            {
                if (maxBackupToLoad >= 0)
                    settings.maxBackupToLoad = maxBackupToLoad;
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("禁用游戏换季存档", GUILayout.Width(200));
            settings.blockAutoSave = GUILayout.SelectionGrid(settings.blockAutoSave ? 1 : 0,
                                         AutoSaveState, 2, GUILayout.Width(150)) == 1;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("打印log", GUILayout.Width(100)))
            {
                Log();

            }

            if (GUILayout.Button("显示log路径", GUILayout.Width(100)))
            {
                if (logPath != null && File.Exists(logPath))
                {
                    var p = new System.Diagnostics.Process();
                    p.StartInfo.FileName = "explorer.exe";
                    p.StartInfo.Arguments = "/e,/select,\"" + logPath + "\"";
                    p.Start();
                }
            }

            GUILayout.EndHorizontal();
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }


        /// <summary>
        /// 用于调试
        /// </summary>
        public static void Log()
        {
            Debug.Log("version: " + MainMenu.instance.gameVersionText.text);
            Debug.Log("dateId: " + SaveManager.DateId);
            Debug.Log("dirpath: " + SaveManager.SavePath);
            Debug.Log("backpath: " + SaveManager.BackPath);

            Debug.Log("savedFiles: ");
            if (LoadFile.SavedFiles != null)
            {
                foreach (var file in LoadFile.SavedFiles)
                {
                    Debug.Log("\t" + file);
                }
            }

            Debug.Log("savedInfos: ");
            if (LoadFile.SavedInfos != null)
            {
                foreach (var pair in LoadFile.SavedInfos)
                {
                    Debug.Log("\t" + pair.Key + ": " +
                              Newtonsoft.Json.JsonConvert.SerializeObject(pair.Value));
                }
            }

            using (var stream = new MemoryStream())
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(Settings));
                serializer.Serialize(stream, Main.settings);
                stream.Seek(0, System.IO.SeekOrigin.Begin);
                Debug.Log(System.Text.Encoding.UTF8.GetString(stream.ToArray()));
            }

            Debug.Log(settings);
        }
    }

    public class ReflectionMethod
    {
        private const BindingFlags Flags = BindingFlags.Instance
                                           | BindingFlags.Static
                                           | BindingFlags.NonPublic
                                           | BindingFlags.Public;
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
            return (T2) typeof(T1).GetMethod(method, Flags)?.Invoke(instance, args);
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
            return (T2) typeof(T1).GetField(field, Flags)?.GetValue(instance);
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
}
