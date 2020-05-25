using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Optional;
using Optional.Linq;
using Optional.Unsafe;

namespace Tauron
{
    [DebuggerStepThrough, PublicAPI]
    public static class IOExtensions
    {
        public static string PathShorten(this string path, int length)
        {
            var pathParts = path.Split('\\');
            var pathBuild = new StringBuilder(path.Length);
            var lastPart = pathParts[^1];
            var prevPath = "";

            //Erst prüfen ob der komplette String evtl. bereits kürzer als die Maximallänge ist
            if (path.Length >= length) return path;

            for (var i = 0; i < pathParts.Length - 1; i++)
            {
                pathBuild.Append(pathParts[i] + @"\");
                if ((pathBuild + @"...\" + lastPart).Length >= length) return prevPath;
                prevPath = pathBuild + @"...\" + lastPart;
            }

            return prevPath;
        }


        public static void Clear(this DirectoryInfo dic)
        {
            if (dic == null) throw new ArgumentNullException(nameof(dic));
            if (!dic.Exists) return;

            foreach (var entry in dic.GetFileSystemInfos())
            {
                if (entry is FileInfo file)
                    file.Delete();
                else
                {
                    if (!(entry is DirectoryInfo dici)) continue;

                    Clear(dici);
                    dici.Delete();
                }
            }
        }

        public static void ClearDirectory(this string dic)
        {
            if (dic == null) throw new ArgumentNullException(nameof(dic));
            Clear(new DirectoryInfo(dic));
        }


        public static void ClearParentDirectory(this string dic)
        {
            if (dic == null) throw new ArgumentNullException(nameof(dic));
            ClearDirectory(Path.GetDirectoryName(dic));
        }


        public static string CombinePath(this string path, params string[] paths)
        {
            paths = paths.Select(str => str.TrimStart('\\')).ToArray();
            if (Path.HasExtension(path))
                path = Path.GetDirectoryName(path);

            var tempPath = Path.Combine(paths);
            return Path.Combine(path, tempPath);
        }


        public static string CombinePath(this string path, string path1)
            => string.IsNullOrWhiteSpace(path) ? path1 : Path.Combine(path, path1);

        [JetBrains.Annotations.NotNull]
        public static string CombinePath(this FileSystemInfo path, string path1) 
            => CombinePath(path.FullName, path1);

        public static void CopyFileTo(this Option<string> source, Option<string> destination)
        {
            var result = from sourcePath in source
                         from destPath in destination
                         where source.ExisFile().ValueOr(false) || string.IsNullOrWhiteSpace(destPath)
                         select (sourcePath, destPath);
            result.MatchSome(t => File.Copy(t.sourcePath, t.destPath, true));
        }

        public static Option<bool, Exception> CreateDirectoryIfNotExis(this Option<string> path)
        {
            Option<bool, Exception> Mapping(string p)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(p)) return false.Some<bool, Exception>();

                    if (!Path.HasExtension(p)) return CreateDirectoryIfNotExis(new DirectoryInfo(p).Some()).WithException<Exception>(() => new InvalidOperationException($"Creation of {p} Failed"));

                    var name = Path.GetDirectoryName(p).Some().Where(s => !string.IsNullOrWhiteSpace(s));

                    return CreateDirectoryIfNotExis(name.Map(s => new DirectoryInfo(s))).WithException<Exception>(() => new InvalidOperationException($"Creation of {p} Failed"));
                }
                catch (Exception e)
                {
                    return Option.None<bool, Exception>(e);
                }
            }

            return Mapping(path.ValueOrDefault());
        }

        public static Option<bool> CreateDirectoryIfNotExis(this Option<DirectoryInfo> dico)
        {
            return dico.Map(dic =>
                            {
                                if (dic.Exists) return false;
                                dic.Create();

                                return true;
                            });
        }

        public static void SafeDelete(this FileSystemInfo info)
        {
            if (info.Exists) info.Delete();
        }

        public static void DeleteDirectory([JetBrains.Annotations.NotNull] this string path)
        {
            if (Path.HasExtension(path))
                path = Path.GetDirectoryName(path);

            try
            {
                if (Directory.Exists(path)) Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        public static void DeleteDirectory(this string path, object sub)
        {
            var tempsub = sub.ToString();
            var compl = CombinePath(path, tempsub);
            if (Directory.Exists(compl)) Directory.Delete(compl);
        }

        public static void DeleteDirectory(this string path, bool recursive)
        {
            if (Directory.Exists(path)) 
                Directory.Delete(path, recursive);
        }

        public static void DeleteDirectoryIfEmpty(this string path)
        {
            if(!Directory.Exists(path)) return;
            if (!Directory.EnumerateFileSystemEntries(path).Any()) Directory.Delete(path);
        }

        public static void DeleteFile(this Option<string> path) 
            => path.SomeWhen(o => o.ExisFile().ValueOr(false)).Flatten().MatchSome(File.Delete);

        public static bool DirectoryConainsInvalidChars(this string path)
        {
            var invalid = Path.GetInvalidPathChars();
            return path?.Any(invalid.Contains!) ?? true;
        }

        public static IEnumerable<string> EnumrateFileSystemEntries(this string dic) 
            => Directory.EnumerateFileSystemEntries(dic);

        public static IEnumerable<string> EnumerateAllFiles(this string dic) 
            => Directory.EnumerateFiles(dic, "*.*", SearchOption.AllDirectories);

        public static IEnumerable<string> EnumerateAllFiles(this string dic, string filter) 
            => Directory.EnumerateFiles(dic, filter, SearchOption.AllDirectories);

        public static IEnumerable<string> EnumerateDirectorys(this string path) 
            => !Directory.Exists(path) ? Enumerable.Empty<string>() : Directory.EnumerateDirectories(path);

        public static IEnumerable<FileSystemInfo> EnumerateFileSystemEntrys(this string path) 
            => new DirectoryInfo(path).EnumerateFileSystemInfos();

        public static IEnumerable<string> EnumerateFiles(this string dic) 
            => Directory.EnumerateFiles(dic, "*.*", SearchOption.TopDirectoryOnly);

        public static IEnumerable<string> EnumerateFiles(this string dic, string filter) 
            => Directory.EnumerateFiles(dic, filter, SearchOption.TopDirectoryOnly);

        public static IEnumerable<string> EnumerateTextLinesIfExis(this string path)
        {
            if (!File.Exists(path)) yield break;

            using var reader = File.OpenText(path);

            while (true)
            {
                var line = reader.ReadLine();
                if (line == null) yield break;

                yield return line;
            }
        }

        public static IEnumerable<string> EnumerateTextLines(this TextReader reader)
        {
            while (true)
            {
                var line = reader.ReadLine();
                if (line == null) yield break;

                yield return line;
            }
        }

        public static Option<bool> ExisDirectory(this Option<string> path) 
            => path.Map(Directory.Exists);

        public static bool ExisFile(this string workingDirectory, string file)
        {
            try
            {
                return File.Exists(Path.Combine(workingDirectory, file));
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public static Option<bool> ExisFile(this Option<string> file)
            => file.Map(s => !string.IsNullOrWhiteSpace(s) && File.Exists(s));

        public static DateTime GetDirectoryCreationTime(this string path) 
            => Directory.GetCreationTime(path);

        public static string GetDirectoryName(this string path) 
            => Path.GetDirectoryName(path);

        public static string GetDirectoryName(this StringBuilder path) 
            => GetDirectoryName(path.ToString());


        public static string[] GetDirectorys(this string path) 
            => Directory.GetDirectories(path);

        public static string GetExtension(this string path) 
            => Path.GetExtension(path);

        public static string GetFileName(this string path) 
            => Path.GetFileName(path);

        public static string GetFileNameWithoutExtension(this string path) 
            => Path.GetFileNameWithoutExtension(path);

        public static int GetFileSystemCount(this string strDir) 
            => GetFileSystemCount(new DirectoryInfo(strDir));

        public static int GetFileSystemCount(this DirectoryInfo di)
        {
            var count = di.GetFiles().Length;
            
            // 2. Für alle Unterverzeichnisse im aktuellen Verzeichnis
            foreach (var diSub in di.GetDirectories())
            {
                // 2a. Statt Console.WriteLine hier die gewünschte Aktion
                count++;

                // 2b. Rekursiver Abstieg
                count += GetFileSystemCount(diSub);
            }

            return count;
        }

        public static string[] GetFiles(this string dic) 
            => Directory.GetFiles(dic);

        public static string[] GetFiles(this string path, string pattern, SearchOption option) 
            => Directory.GetFiles(path, pattern, option);

        public static Option<string> GetFullPath(this Option<string> path) 
            => path.Map(Path.GetFullPath);

        public static bool HasExtension(this string path) 
            => Path.HasExtension(path);

        public static bool IsPathRooted(this string path) 
            => Path.IsPathRooted(path);

        public static void MoveTo(this string source, string dest) 
            => File.Move(source, dest);

        public static void MoveTo(this string source, string workingDirectory, string dest)
        {
            var realDest = dest;

            if (!dest.HasExtension())
            {
                var fileName = Path.GetFileName(source);
                realDest = Path.Combine(dest, fileName);
            }

            var realSource = Path.Combine(workingDirectory, source);

            File.Move(realSource, realDest);
        }

        public static Option<Stream> OpenRead(this Option<string> path, Option<FileShare> share)
            => path.SimpleCheckIsValidCreation()
               .Map<Stream>(p => new FileStream(p, FileMode.OpenOrCreate, FileAccess.Read, share.ValueOr(FileShare.Read)));

        public static Option<Stream> OpenRead(this Option<string> path) 
            => OpenRead(path, FileShare.None.Some());

        public static Option<StreamWriter> OpenTextAppend(this Option<string> path) 
            => path.OpenRead()
               .Map(s => new StreamWriter(s));

        public static StreamReader OpenTextRead(this string path) 
            => File.OpenText(path);

        private static Option<string> SimpleCheckIsValidCreation(this Option<string> path) 
            => path.GetFullPath()
               .NoneWhen(p => p.CreateDirectoryIfNotExis().HasValue)
               .Flatten();

        public static Option<StreamWriter> OpenTextWrite(this Option<string> path) 
            => path.OpenWrite().Map(s => new StreamWriter(s));

        public static Option<Stream> OpenWrite(this Option<string> path, Option<bool> delete = default) 
            => OpenWrite(path, FileShare.None.Some(), delete);

        public static Option<Stream> OpenWrite(this Option<string> path, Option<FileShare> share, Option<bool> delete = default)
        {
            if (delete.ValueOr(true))
                path.DeleteFile();

            return path.SimpleCheckIsValidCreation().Map<Stream>(p => new FileStream(p, FileMode.OpenOrCreate, FileAccess.Write, share.ValueOr(FileShare.Read)));
        }

        public static byte[] ReadAllBytesIfExis(this string path) 
            => !File.Exists(path) ? new byte[0] : File.ReadAllBytes(path);

        public static byte[] ReadAllBytes(this string path) 
            => File.ReadAllBytes(path);

        public static string ReadTextIfExis(this string path)
        {
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        public static string ReadTextIfExis(this string workingDirectory, string subPath) 
            => ReadTextIfExis(CombinePath(workingDirectory, subPath));

        public static IEnumerable<string> ReadTextLinesIfExis(this string path)
        {
            if (!File.Exists(path)) yield break;

            using var reader = File.OpenText(path);
            while (true)
            {
                var line = reader.ReadLine();
                if (line == null) break;

                yield return line;
            }
        }

        public static bool TryCreateUriWithoutScheme(this string str, [MaybeNullWhen(false)]out Uri? uri, params string[] scheme)
        {
            var flag = Uri.TryCreate(str, UriKind.RelativeOrAbsolute, out var target);

            // ReSharper disable once AccessToModifiedClosure
            if (flag)
            {
                foreach (var s in scheme.Where(s => flag))
                    flag = target.Scheme != s;
            }

            uri = flag ? target : null;

            return flag;
        }

        public static void WriteTextContentTo(this string content, string path) 
            => File.WriteAllText(path, content);


        public static void WriteTextContentTo(this string content, string workingDirectory, string path) 
            => WriteTextContentTo(content, CombinePath(workingDirectory, path));
    }
}