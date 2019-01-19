using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace Sth4nothing.SLManager
{
    public class SevenItem
    {
        [Flags]
        public enum ItemAttr
        {
            N = 0x00,
            D = 0x01,
            R = 0x02,
            C = 0x04,
            B = 0x08,
            A = 0x10,
        }
        public string path;
        public DateTime time;
        public ItemAttr type;
        public int realSize;
        public int size;
        private static readonly Regex reg =
            new Regex(@"(\d{4,4}-\d\d-\d\d \d\d:\d\d:\d\d)\s+([A-Z\.]+)\s+(\d+)\s+(\d+)\s+(.+)\s*",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static bool IsItem(string line)
        {
            return reg.IsMatch(line);
        }
        public static SevenItem Parse(string line)
        {
            var match = reg.Match(line);
            if (!match.Success)
            {
                throw new ArgumentException("does not match");
            }
            var item = new SevenItem
            {
                time = DateTime.Parse(match.Groups[1].Value),
                type = ParseAttr(match.Groups[2].Value),
                realSize = int.Parse(match.Groups[3].Value),
                size = int.Parse(match.Groups[4].Value),
                path = match.Groups[5].Value,
            };
            return item;
        }
        private const string AttrStr = "DRCBA";
        public static string Attr2String(ItemAttr attr)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < AttrStr.Length; i++)
            {
                if ((attr | (ItemAttr)(1 << i)) != ItemAttr.N)
                {
                    sb.Append(AttrStr[i]);
                }
                else
                {
                    sb.Append('.');
                }
            }
            return sb.ToString();
        }
        public static ItemAttr ParseAttr(string attrStr)
        {
            ItemAttr attr = ItemAttr.N;
            for (int i = 0; i < AttrStr.Length; i++)
            {
                if (attrStr[i] == AttrStr[i])
                    attr |= (ItemAttr)(1 << i);
            }
            return attr;
        }
        public override string ToString()
        {
            return $"{time:yyyy-MM-dd HH:mm:ss} {type.ToString()} {realSize} {size} {path}";
        }
    }
    public static class SevenZipHelper
    {
        private static string sevenZipPath = "7z.exe";
        public static string SevenZipPath
        {
            get
            {
                if (!File.Exists(sevenZipPath))
                {
                    throw new FileNotFoundException("cannot found 7z.exe");
                }
                return sevenZipPath;
            }
            set
            {
                if (File.Exists(value))
                {
                    sevenZipPath = value;
                }
            }
        }
        private static string workDir = ".\\";
        public static string WorkDir
        {
            get
            {
                if (!Directory.Exists(workDir))
                {
                    throw new DirectoryNotFoundException("working dir does not exist");
                }
                return workDir;
            }
            set
            {
                if (!Directory.Exists(value))
                {
                    throw new DirectoryNotFoundException("working dir does not exist");
                }
                workDir = value;
            }
        }
        public static IEnumerable<SevenItem> List(string archPath, string filter = null)
        {
            if (!File.Exists(archPath))
            {
                throw new FileNotFoundException("cannot found archieve");
            }
            var startInfo = new ProcessStartInfo()
            {
                FileName = SevenZipPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = WorkDir,
                Arguments = $"l \"{archPath}\" {filter ?? ""}",
                RedirectStandardOutput = true,
            };
            var p = Process.Start(startInfo);
            p.WaitForExit();
            while (!p.StandardOutput.EndOfStream)
            {
                var line = p.StandardOutput.ReadLine();
                if (SevenItem.IsItem(line))
                {
                    yield return SevenItem.Parse(line);
                }
            }
        }
        public static void Extract(string archPath, string dstPath, bool fullPath = false, string include = null, bool overwrite = true)
        {
            if (!File.Exists(archPath))
            {
                throw new FileNotFoundException("cannot found archieve");
            }
            var startInfo = new ProcessStartInfo()
            {
                FileName = SevenZipPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = WorkDir,
                Arguments = $"{(fullPath ? "x" : "e")} \"{archPath}\" -o\"{dstPath}\" {include ?? ""} {(overwrite ? "-aoa" : "-aos")}",
            };
            var p = Process.Start(startInfo);
            p.WaitForExit();
        }
        public static void Create(string archPath, string files, bool recurse = false)
        {
            if (File.Exists(archPath))
            {
                File.Delete(archPath);
            }
            var startInfo = new ProcessStartInfo()
            {
                FileName = SevenZipPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = WorkDir,
                Arguments = $"a \"{archPath}\" {files} {(recurse ? "-r" : "")}",
            };
            var p = Process.Start(startInfo);
            p.WaitForExit();
        }
        public static void Update(string archPath, string files, bool recurse = false)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = SevenZipPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = WorkDir,
                Arguments = $"u \"{archPath}\" {files} {(recurse ? "-r" : "")}",
            };
            var p = Process.Start(startInfo);
            p.WaitForExit();
        }
        public static void Delete(string archPath, string files, bool recurse = false)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = SevenZipPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = WorkDir,
                Arguments = $"d \"{archPath}\" {files} {(recurse ? "-r" : "")}",
            };
            var p = Process.Start(startInfo);
            p.WaitForExit();
        }
    }
    class TempDir : IDisposable
    {
        private static readonly string TEMP =
            System.Environment.GetEnvironmentVariable("TEMP");
        private readonly DirectoryInfo dir;
        public string Dir { get => dir.FullName; }

        public TempDir() : this(Path.Combine(TEMP, System.Guid.NewGuid().ToString()))
        {
        }
        protected TempDir(string tmpDir)
        {
            dir = Directory.CreateDirectory(tmpDir);
        }

        void IDisposable.Dispose()
        {
            if (dir.Exists)
            {
                dir.Delete(true);
            }
        }
        public override string ToString()
        {
            return dir.FullName;
        }
    }
}