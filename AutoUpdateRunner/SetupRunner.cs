using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
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
                KillProcess();

                if (ValidateData(out var newVersion)) return;

                MakeBackup(backup);
                backUpNeed = true;

                _logger.Information("Copy New Files");
                ClearDictionary(_info.Target);
                newVersion.ExtractToDirectory(_info.Target);

                _logger.Information("CleanUp");
                newVersion.Dispose();
                File.Delete(_info.DownloadFile);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error Running Copy");
                failed = true;
            }
            finally
            {
                if (failed && backUpNeed)
                {
                    _logger.Information("Replay Backup {Backup} To {Target}", backup, _info.Target);
                    MoveDic(backup, _info.Target);
                }
                if(Directory.Exists(backup))
                    Directory.Delete(backup, true);
                StartHost();
            }
        }

        private void MakeBackup(string backup)
        {
            _logger.Information("Making Backup");
            //if (Directory.Exists(backup))
            //    Directory.Delete(backup, true);
            //Directory.CreateDirectory(backup);

            MoveDic(_info.Target, backup);
        }

        private bool ValidateData(out ZipArchive newVersion)
        {
            _logger.Information("Validating Data");
            newVersion = ZipFile.Open(_info.DownloadFile, ZipArchiveMode.Read);
            // ReSharper disable once InvertIf
            if (Validate(_info.StartFile, nameof(_info.StartFile), Path.HasExtension)
                && Validate(_info.RunningProcess, nameof(_info.RunningProcess), i => i > 0)
                && Validate(_info.Target, nameof(_info.Target), Directory.Exists))
            {
                return false;
            }

            _logger.Warning("Validating Data Failed");
            return true;
        }

        private void KillProcess()
        {
            _logger.Information("Killing Host process");
            try
            {
                using var process = Process.GetProcessById(_info.RunningProcess);
                var time = _info.KillTime;

                while (!process.HasExited)
                {
                    Thread.Sleep(1000);
                    time -= 1000;

                    if (time >= 0) continue;

                    process.Kill(true);
                    break;
                }
            }
            catch (ArgumentException)
            {
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error on Getting Process");
            }
        }

        private void StartHost()
        {
            try
            {
                Directory.SetCurrentDirectory(_info.Target);
                Process.Start(Path.Combine(_info.Target, _info.StartFile), $"--cleanup true --id {Process.GetCurrentProcess().Id}");
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error on Starting Host {Path}", _info.StartFile);
            }
        }

        private bool Validate<TValue>(TValue value, string name, Func<TValue, bool> validator)
        {
            _logger.Information("Validating {Name}:{Value}", name, value);
            return validator(value);
        }

        private static void MoveDic(string @from, string to)
        {
            ClearDictionary(to);

            var elements = new Queue<(DirectoryInfo Dic, string Base, string Target)>();
            elements.Enqueue((new DirectoryInfo(from), @from, to));

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

        private static void ClearDictionary(string target)
        {
            if(Directory.Exists(target))
                Directory.Delete(target, true);
            Directory.CreateDirectory(target);
        }
    }
}