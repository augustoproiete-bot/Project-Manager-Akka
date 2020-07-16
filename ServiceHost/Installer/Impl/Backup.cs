using System.IO;
using System.IO.Compression;
using Akka.Event;
using Tauron;

namespace ServiceHost.Installer.Impl
{
    public sealed class Backup
    {
        private static readonly string BackupLocation = Path.GetFullPath("Backup");

        private string _backupFile = string.Empty;
        private string _backFrom = string.Empty;

        public void Make(string from)
        {
            BackupLocation.CreateDirectoryIfNotExis();

            _backFrom = from;
            _backupFile = Path.Combine(BackupLocation, "Backup.zip");

            ZipFile.CreateFromDirectory(from, _backupFile);
        }

        public void Recover(ILoggingAdapter log)
        {
            log.Info("Recover Old Application from Backup during Recover");
            ZipFile.ExtractToDirectory(_backupFile, _backFrom, true);
            _backupFile.DeleteFile();
        }

        public void CleanUp() 
            => _backupFile.DeleteFile();
    }
}