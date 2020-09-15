using System.IO;
using System.IO.Compression;

namespace Servicemnager.Networking
{
    public sealed class HostConfiguration
    {
        public const string DefaultFileName = "HostConfig.json";

        public string Identifer { get; }

        public string TargetAdress { get; }

        public bool CreateShortcut { get; }

        private HostConfiguration(string identifer, string targetAdress, bool createShortcut)
        {
            Identifer = identifer;
            TargetAdress = targetAdress;
            CreateShortcut = createShortcut;
        }

        public static HostConfiguration Read()
        {
            using var stream = File.OpenText(DefaultFileName);

            return new HostConfiguration(stream.ReadLine() ?? string.Empty, stream.ReadLine() ?? string.Empty, bool.Parse(stream.ReadLine() ?? "false"));
        }

        public static void WriteInTo(string zipFile, string targetAdress, string identifer, bool createShortcut)
        {
            using var zip = ZipFile.Open(zipFile, ZipArchiveMode.Update);
            using var stream = new StreamWriter(zip.CreateEntry(DefaultFileName, CompressionLevel.Optimal).Open());

            stream.WriteLine(identifer);
            stream.WriteLine(targetAdress);
            stream.WriteLine(createShortcut);
            stream.Flush();
        }
    }
}