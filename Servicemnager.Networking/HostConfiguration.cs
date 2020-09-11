using System.IO;
using System.IO.Compression;

namespace Servicemnager.Networking
{
    public sealed class HostConfiguration
    {
        public const string DefaultFileName = "HostConfig.json";

        public string Identifer { get; }

        public string TargetAdress { get; }

        private HostConfiguration(string identifer, string targetAdress)
        {
            Identifer = identifer;
            TargetAdress = targetAdress;
        }

        public static HostConfiguration Read()
        {
            using var stream = File.OpenText(DefaultFileName);

            return new HostConfiguration(stream.ReadLine(), stream.ReadLine());
        }

        public static void WriteInTo(string zipFile, string targetAdress, string identifer)
        {
            using var zip = ZipFile.Open(zipFile, ZipArchiveMode.Update);
            using var stream = new StreamWriter(zip.CreateEntry(DefaultFileName, CompressionLevel.Optimal).Open());

            stream.WriteLine(identifer);
            stream.WriteLine(targetAdress);
            stream.Flush();
        }
    }
}