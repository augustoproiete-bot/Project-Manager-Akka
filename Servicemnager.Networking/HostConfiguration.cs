using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;

namespace Servicemnager.Networking
{
    public sealed class HostConfiguration
    {
        public const string DefaultFileName = "HostConfig.json";

        public string Identifer { get; }

        public string TargetAdress { get; }

        public HostConfiguration(string identifer, string targetAdress)
        {
            Identifer = identifer;
            TargetAdress = targetAdress;
        }

        public static HostConfiguration Read() => JsonConvert.DeserializeObject<HostConfiguration>(DefaultFileName);

        public static void WriteInTo(string zipFile, string targetAdress, string identifer)
        {
            using var zip = ZipFile.Open(zipFile, ZipArchiveMode.Update);
            using var stream = new StreamWriter(zip.CreateEntry(DefaultFileName, CompressionLevel.Optimal).Open());

            stream.Write(JsonConvert.SerializeObject(new HostConfiguration(identifer, targetAdress)));
            stream.Flush();
        }
    }
}