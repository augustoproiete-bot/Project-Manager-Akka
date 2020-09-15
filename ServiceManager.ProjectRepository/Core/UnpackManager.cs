using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Tauron;

namespace ServiceManager.ProjectRepository.Core
{
    public class UnpackManager : SharedObject<UnpackManager>
    {
        private bool _isUnpacked;

        private string _targetPath = string.Empty;
        private string _zip = string.Empty;

        public string UnpackRepo(string trackingId)
        {
            lock (Lock)
            {
                _targetPath = Path.Combine(Configuration.SourcePath, "Repo");
                _targetPath.CreateDirectoryIfNotExis();

                _zip = Path.Combine(Configuration.SourcePath, "Pack.zip");


                if (!_isUnpacked)
                {
                    // ReSharper disable once InvertIf
                    if (File.Exists(_zip))
                    {
                        LogMessage("Upacking Git Repository {Id}", trackingId);
                        ZipFile.ExtractToDirectory(_zip, _targetPath);
                    }

                    _isUnpacked = true;
                }
                else
                    LogMessage("Repository is Upacked {Id}", trackingId);

                return _targetPath;
            }
        }

        protected override void InternalDispose()
        {
            LogMessage("Packing Git Repository");

            using var clean = Process.Start(Configuration.DotNetPath, $"clean {Path.Combine(_targetPath, Configuration.Solotion)}");
            if (clean?.WaitForExit(TimeSpan.FromSeconds(10).Milliseconds) == false) clean.Kill();

            _zip.DeleteFile();

            ZipFile.CreateFromDirectory(_targetPath, _zip);

            foreach (var file in _targetPath.EnumerateAllFiles()) 
                File.SetAttributes(file, File.GetAttributes(file) & ~FileAttributes.ReadOnly);

            _targetPath.DeleteDirectory(true);
        }
    }
}