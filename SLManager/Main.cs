using Harmony12;
using System.IO;
using System.Linq;
using System.Reflection;
using System;
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
        public bool enableTurboQuickLoadAfterLoad = false;
        public bool enableTurboQuickLoadAfterSave = false;
        public bool regenerateRandomSeedAfterLoad = false;
    }


    public class ThreadSafeLogger 
    {
        UnityModManager.ModEntry.ModLogger _baseLogger;
        private object _writeLock = new object();

        public ThreadSafeLogger(UnityModManager.ModEntry.ModLogger baseLogger)
        {
            _baseLogger = baseLogger;
        }


        public virtual void Critical(string str)
        {
            lock (_writeLock) _baseLogger.Critical(str);
        }
        public virtual void Error(string str)
        {
            lock (_writeLock) _baseLogger.Error(str);
        }
        public virtual void Log(string str)
        {
            lock (_writeLock) _baseLogger.Log(str);
        }
        public virtual void Warning(string str)
        {
            lock (_writeLock) _baseLogger.Warning(str);
        }
    }


    public static class Main
    {
        public static bool Enabled { get; private set; }
        public static bool ForceSave = false;
        public static bool isBackuping = false;
        private static string logPath;
        public static readonly string[] Off_And_On = {"关闭", "启用"};

        public static Settings settings;

        public static ThreadSafeLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = new ThreadSafeLogger(modEntry.Logger);
            try
            {
                var userProfile = System.Environment.GetEnvironmentVariable("USERPROFILE");
                logPath = Path.Combine(userProfile,
                    @"AppData\LocalLow\Conch Ship Game\The Scroll Of Taiwu Alpha V1.0\output_log.txt"
                );
                settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
                HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());

            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                throw;
            }



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
            GUILayout.Label("禁用游戏换季存档", GUILayout.Width(250));
            settings.blockAutoSave = GUILayout.SelectionGrid(settings.blockAutoSave ? 1 : 0,
                                         Off_And_On, 2, GUILayout.Width(150)) == 1;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("讀檔後重置亂數種子", GUILayout.Width(250));
            settings.regenerateRandomSeedAfterLoad = GUILayout.SelectionGrid(settings.regenerateRandomSeedAfterLoad ? 1 : 0,
                                                    Off_And_On, 2, GUILayout.Width(150)) == 1;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            HyperQuickLoad.InitClass.OnGUI(modEntry);


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
        /// 反射执行方法 (静态型別)
        /// </summary>
        /// <param name="instance">类实例(静态方法则为null)</param>
        /// <param name="method">方法名</param>
        /// <param name="args">方法的参数类型列表</param>
        /// <typeparam name="T1">类</typeparam>
        /// <typeparam name="T2">返回值类型</typeparam>
        /// <returns></returns>
        public static T2 Invoke<T2>(Type type, string method, params object[] args)
        {
            return (T2)type.GetMethod(method, Flags)?.Invoke(null, args);
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
        public static void SetProperty<T>(T instance, string property, object value)
        {
            typeof(T).GetProperty(property, Flags)?.SetValue(instance, value);
        }
        public static T2 GetProperty<T1, T2>(T1 instance, string property)
        {
            return (T2) typeof(T1).GetProperty(property, Flags)?.GetValue(instance);
        }
    }

    /// <summary>
    /// 修正Ionic.Zip.OffsetStream的错误
    /// </summary>
    internal class OffsetStream : Stream, IDisposable
    {
        public OffsetStream(Stream s)
        {
            this._originalPosition = s.Position;
            this._innerStream = s;
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            return this._innerStream.Read(buffer, offset, count);
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
        public override bool CanRead
        {
            get
            {
                return this._innerStream.CanRead;
            }
        }
        public override bool CanSeek
        {
            get
            {
                return this._innerStream.CanSeek;
            }
        }
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }
        public override void Flush()
        {
            this._innerStream.Flush();
        }
        /// <summary>
        /// error 1
        /// </summary>
        /// <value></value>
        public override long Length
        {
            get
            {
                return this._innerStream.Length - this._originalPosition;
            }
        }
        public override long Position
        {
            get
            {
                return this._innerStream.Position - this._originalPosition;
            }
            set
            {
                this._innerStream.Position = this._originalPosition + value;
            }
        }
        /// <summary>
        /// error 2
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return this._innerStream.Seek(offset + (origin == SeekOrigin.Begin ? this._originalPosition : 0), origin) - this._originalPosition;
        }
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
            this.Close();
        }
        public override void Close()
        {
            base.Close();
        }

        private readonly long _originalPosition;
        private readonly Stream _innerStream;
    }
}
