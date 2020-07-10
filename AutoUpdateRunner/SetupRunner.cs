using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Serilog;

namespace AutoUpdateRunner
{
    public sealed class SetupRunner
    {
        private readonly ILogger _logger = Log.ForContext<SetupRunner>();

        private readonly SetupInfo _info;

        public SetupRunner(SetupInfo info) => _info = info;

        public void Run()
        {
            var backup = Path.GetFullPath("Backup");

            var failed = false;
            var backUpNeed = false;

            try
            {
                var setup =

                Directory.CreateDirectory(backup);

                backUpNeed = true;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error Running Copy");
                failed = true;
            }
            finally
            {
                if(failed && backUpNeed)
                    Revert(_info.Target, backup);
                Directory.Delete(backup, true);
                StartHost();
            }
        }

        private void Revert(string target, string backup)
        {
            _logger.Information("Replay Backup {Backup} To {Target}", backup, target);
            ClearDictionary(target);
            
            var elements = new Queue<(DirectoryInfo Dic, string Base, string Target)>();
            elements.Enqueue((new DirectoryInfo(backup), backup, target));

            while (elements.Count != 0)
            {
                var currentTarget = elements.Dequeue();
                foreach (var info in currentTarget.Dic.EnumerateFileSystemInfos())
                {
                    switch (info)
                    {
                        case FileInfo file:
                            file.CopyTo(file.FullName.Replace(currentTarget.Base, currentTarget.Target));
                            break;
                        case DirectoryInfo dic:
                            var newPath = dic.FullName.Replace(currentTarget.Base, currentTarget.Target);
                            Directory.CreateDirectory(newPath);
                            elements.Enqueue((dic, dic.FullName, newPath));
                            break;
                    }
                }
            }
        }

        private void StartHost()
        {
            try
            {
                Process.Start(_info.StartFile);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error on Starting Host {Path}", _info.StartFile);
            }
        }

        private static void ClearDictionary(string target)
        {
            Directory.Delete(target, true);
            Directory.CreateDirectory(target);
        }
    }
}