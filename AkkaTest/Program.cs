using System;
using System.IO;
using System.Linq;

namespace AkkaTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var test = Path.GetTempFileName();

            try
            {
                using var testStream = new FileStream(test, FileMode.Open, FileAccess.ReadWrite, FileShare.Delete, 4096, FileOptions.DeleteOnClose);
                testStream.WriteByte(43);
                testStream.Flush();
            }
            finally
            {
                if(File.Exists(test))
                    File.Delete(test);
            }
            //var test = Environment.GetEnvironmentVariable("Path")?.Split(new [] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            //var test2 = test.Where(Directory.Exists).SelectMany(Directory.EnumerateFiles).Where(f => f.EndsWith("dotnet.exe")).ToArray();

            //var test3 = test2.FirstOrDefault(p =>
            //{
            //    var sdkPath = Path.Combine(Path.GetDirectoryName(p) ?? string.Empty, "sdk");
            //    return Directory.Exists(sdkPath) && Directory.EnumerateDirectories(sdkPath).Any(e => new DirectoryInfo(e).Name.StartsWith("3.1"));
            //});
        }
    }
}