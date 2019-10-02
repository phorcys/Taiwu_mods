using Harmony12;
using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UMM = UnityModManagerNet.UnityModManager;

namespace Sth4nothing.DynamicExecutor
{
    public class Settings : UMM.ModSettings
    {
        /// <summary>
        /// MsBuild路径
        /// </summary>
        public string msbuildPath =
            @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MsBuild.exe";
        public string dllsPath = @"D:\Desktop\Work\Taiwu_mods\build\dlls";
        public override void Save(UMM.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    public class Main
    {
        public static UMM.ModEntry.ModLogger Logger { get; private set; }
        public static Settings Setting { get; private set; }
        public static bool Enabled { get; private set; }
        /// <summary>
        /// 执行状态
        /// </summary>
        private static bool running = false;
        /// <summary>
        /// 执行次数，防止动态加载时冲突
        /// </summary>
        private static int count = 0;
        /// <summary>
        /// 对所有mod的引用
        /// </summary>
        private static string modsReference;

        private static readonly string[] files =
        {
            "Execute.cs.template",
            "Execute.csproj.template",
            "AssemblyInfo.cs.template",
            // "Execute.csproj.user.template",
        };

        /// <summary>
        /// 本mod根目录
        /// </summary>
        private static string rootPath;

        public static bool OnToggle(UMM.ModEntry modEntry, bool value)
        {
            if (!value)
            {
                return false;
            }
            Enabled = true;
            return true;
        }

        public static bool Load(UMM.ModEntry modEntry)
        {
            rootPath = modEntry.Path;
            Logger = modEntry.Logger;
            Func<UMM.ModEntry, string> convert = (mod) =>
                $"\t\t<Reference Include=\"{mod.Info.AssemblyName.Replace(".dll", "")}\">\n" +
                "\t\t\t<ReferenceOutputAssembly>true</ReferenceOutputAssembly>\n" +
                "\t\t\t<Private>false</Private>\n" +
                $"\t\t\t<HintPath>{Path.Combine(mod.Path, mod.Info.AssemblyName)}</HintPath>\n" +
                "\t\t</Reference>";
            modsReference = string.Join("\n",
                UMM.modEntries.Where((mod) => mod.Enabled).Select(convert).ToArray());
            Setting = Settings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }

        public static void OnGUI(UMM.ModEntry modEntry)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label("msbuild路径：");
            Setting.msbuildPath = GUILayout.TextField(Setting.msbuildPath);
            GUILayout.Label("dlls路径: ");
            Setting.dllsPath = GUILayout.TextField(Setting.dllsPath);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("打开代码路径", GUILayout.Width(100)))
            {
                var p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "explorer.exe";
                p.StartInfo.UseShellExecute = true;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WorkingDirectory = rootPath;
                p.StartInfo.Arguments = "/e,/select,\""
                    + Path.Combine(rootPath, "Execute.cs.template") + "\"";
                p.Start();
            }
            if (!running)
            {
                if (GUILayout.Button("运行代码", GUILayout.Width(100)))
                {
                    Execute();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 执行代码
        /// </summary>
        public static void Execute()
        {
            // 检测文件
            foreach (var file in files)
            {
                if (!File.Exists(Path.Combine(rootPath, file)))
                {
                    Logger.Log("不存在文件： " + file);
                    return;
                }
            }
            if (File.Exists(Path.Combine(rootPath, "Execute.csproj.user.template")))
            {
                File.Delete(Path.Combine(rootPath, "Execute.csproj.user.template"));
            }
            if (File.Exists(Path.Combine(rootPath, "Execute.csproj.user")))
            {
                File.Delete(Path.Combine(rootPath, "Execute.csproj.user"));
            }
            if (File.Exists(Path.Combine(rootPath, $"Execute{count}.dll")))
            {
                File.Delete(Path.Combine(rootPath, $"Execute{count}.dll"));
            }

            running = true;

            try
            {
                // Execute.cs
                var reg = new Regex(@"\bExecute\b");
                var cs = File.ReadAllText(Path.Combine(rootPath, "Execute.cs.template"));
                File.WriteAllText(Path.Combine(rootPath, "Execute.cs"),
                    reg.Replace(cs, "Execute" + count));
                // Execute.csproj
                var csproj = File.ReadAllText(Path.Combine(rootPath, "Execute.csproj.template"));
                File.WriteAllText(Path.Combine(rootPath, "Execute.csproj"),
                    csproj.Replace("<AssemblyName>Execute</AssemblyName>",
                        $"<AssemblyName>Execute{count}</AssemblyName>")
                        .Replace("%DLLS%", Setting.dllsPath)
                        .Replace("<!-- Mods -->", modsReference)); // 引用mods
                // AssemblyInfo.cs
                File.Copy(Path.Combine(rootPath, "AssemblyInfo.cs.template"),
                    Path.Combine(rootPath, "AssemblyInfo.cs"), true);

                // 编译
                var p = new System.Diagnostics.Process();
                p.StartInfo.FileName = Setting.msbuildPath;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WorkingDirectory = rootPath;
                p.StartInfo.Arguments = "/p:Configuration=Release /noconlog /fl "
                    + "/flp:LogFile=compile.log;Encoding=UTF-8;Verbosity=normal "
                    + "Execute.csproj ";
                p.Start();
                p.WaitForExit();

                if (!File.Exists(Path.Combine(rootPath, $"Execute{count}.dll")))
                {
                    Logger.Error("编译失败");
                }
                else
                {
                    // 使用反射调用代码入口Sth4nothing.Execute.Main
                    var ass = Assembly.LoadFile(Path.Combine(rootPath, $"Execute{count}.dll"));
                    var execute = ass.CreateInstance("Sth4nothing.Execute" + count);
                    var method = execute.GetType().GetMethod("Main",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    var ans = method.Invoke(execute, null);
                    if (ans != null)
                    {
                        Logger.Log($"返回类型: {ans.GetType()}\n返回结果: {ans}");
                    }
                    else
                    {
                        Logger.Log("null");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log($"{e.GetType().Name}: {e.Message}\n{e.StackTrace}\n{e.TargetSite}");
            }
            finally
            {
                running = false;
                count++;
            }
        }

        public static void OnSaveGUI(UMM.ModEntry modEntry)
        {
            Setting.Save(modEntry);
        }
    }
}