using System.IO.Compression;
using Serilog;

namespace SimpleHostSetup.Impl
{
    public sealed class AppPacker
    {
        private static ILogger _log = Log.ForContext<AppPacker>();

        public void MakeZip(string root, string targetFile, string appName)
        {
            _log.Information("Creating Zip File: {AppName}", appName);
            ZipFile.CreateFromDirectory(root, targetFile);
        }
    }
}