using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using JetBrains.Annotations;
using MGIHelper.Core.Configuration;
using MGIHelper.Core.ProcessManager;

namespace MGIHelper.UI
{
    public sealed class MgiStarterControlModel : IDisposable
    {
        private readonly ProcessManager _processManager = new ProcessManager();

        public InternalMgiProcess MgiProcess { get; }

        public MgiStarterControlModel()
        {
            MgiProcess = new InternalMgiProcess();
            _processManager.Register(MgiProcess);
        }

        public void Dispose() 
            => _processManager.Dispose();

        public sealed class InternalMgiProcess : ITargetProcess, INotifyPropertyChanged
        {
            private readonly Lazy<ProcessConfig> _config = new Lazy<ProcessConfig>(ProcessConfig.Read);

            public Process? Client
            {
                get => _client;
                set
                {
                    if (Equals(value, _client)) return;
                    _client = value;
                    OnPropertyChanged();
                }
            }

            public Process? Kernel
            {
                get => _kernel;
                set
                {
                    if (Equals(value, _kernel)) return;
                    _kernel = value;
                    OnPropertyChanged();
                }
            }

            private string _status = string.Empty;

            private readonly Dispatcher _dispatcher = Application.Current.Dispatcher ?? throw new InvalidOperationException();

            private bool _isStarting;
            private Process? _kernel;
            private Process? _client;
            private bool _internalStart;

            public IEnumerable<string> FileNames => new[] {Path.GetFileNameWithoutExtension(_config.Value.Kernel), Path.GetFileNameWithoutExtension(_config.Value.Client)};

            public ICommand Start { get; }

            public ICommand Stop { get; }

            public string Status
            {
                get => _status;
                set
                {
                    if (value == _status) return;
                    _status = value;
                    OnPropertyChanged();
                }
            }

            public bool InternalStart
            {
                get => _internalStart;
                set
                {
                    if (value == _internalStart) return;
                    _internalStart = value;
                    OnPropertyChanged();
                }
            }

            public InternalMgiProcess()
            {
                Start = new SimpleCommand(TryStart, () => (_kernel == null && _client == null || _kernel != null && _client != null) && !_isStarting);
                Stop = new SimpleCommand(TryStop, () => _client != null || _kernel != null);
            }

            private void TryStop()
            {
                Status = "Beenden";
                InternalStart = false;

                Kill(Client);
                Client = null;
                Kill(Kernel);
                Kernel = null;
            }

            private static void Kill(Process? p)
            {
                try
                {
                    p?.Kill(true);
                    p?.Dispose();
                }
                catch
                {
                    //Ignored
                }
            }

            private void TryStart()
            {
                TryStop();

                if(_isStarting) return;
                _isStarting = true;

                InternalStart = true;

                try
                {
                    Status = "Starten";


                    Thread.Sleep(500);
                    if (!CheckKernelRunning())
                    {
                        MessageBox.Show("Kernel konnten nicht gestartet werden");
                        try
                        {
                            _kernel?.Kill(true);
                            _kernel?.Dispose();
                            _kernel = null;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        return;
                    }

                    Thread.Sleep(500);
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(_config.Value.Client));
                    Client = Process.Start(_config.Value.Client);

                    Thread.Sleep(500);
                    if (_client != null)
                        _client.PriorityClass = ProcessPriorityClass.High;

                    Status = "Anwendung Gestartet";
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.ToString());
                }
                finally
                {
                    _isStarting = false;
                }
            }

            private bool CheckKernelRunning()
            {
                var kernelPath = _config.Value.Kernel;
                var statusPath = Path.Combine(Path.GetDirectoryName(kernelPath) ?? string.Empty, "Status.ini");
                const int iterationCount = 60;
                var parser = new IniParser.FileIniDataParser();

                if (File.Exists(statusPath))
                    File.Delete(statusPath);

                Directory.SetCurrentDirectory(Path.GetDirectoryName(kernelPath));
                Kernel = Process.Start(kernelPath);

                if (_kernel != null)
                    _kernel.PriorityClass = ProcessPriorityClass.High;
                else
                    return false;

                Thread.Sleep(5000);

                for (var i = 0; i < iterationCount; i++)
                {
                    try
                    {
                        Thread.Sleep(1100);

                        var data = parser.ReadFile(statusPath);
                        var status = data.Sections["Status"].GetKeyData("Global").Value;
                        switch (status)
                        {
                            case "Ready":
                                return true;
                            case "Fail":
                            case "Close":
                                return false;
                        }
                    }
                    catch
                    {
                        Thread.Sleep(500);
                        continue;
                    }
                }

                return false;
            }

            public void Found(Process p)
            {
                try
                {

                    var kernel = Path.GetFileNameWithoutExtension(_config.Value.Kernel);
                    var client = Path.GetFileNameWithoutExtension(_config.Value.Client);

                    if (p.ProcessName.Contains(kernel))
                    {
                        if (p.Id == Kernel?.Id) return;
                        Kernel?.Dispose();
                        Kernel = p;
                        ConfigProcess(p);
                        InternalStart = false;
                    }
                    else if (p.ProcessName.Contains(client))
                    {
                        if (p.Id == Client?.Id) return;
                        Client?.Dispose();
                        Client = p;
                        ConfigProcess(p);
                        InternalStart = false;
                    }
                }
                finally
                {
                    if (!InternalStart && _kernel != null && _client != null)
                        Status = "Anwendung gefunden";
                    SuggestRequery();
                }
            }

            private static void ConfigProcess(Process p)
            {
                if (p.PriorityClass != ProcessPriorityClass.High)
                    p.PriorityClass = ProcessPriorityClass.High;
            }

            public void Exit(Process p)
            {
                try
                {
                    var kernel = Path.GetFileNameWithoutExtension(_config.Value.Kernel);
                    var client = Path.GetFileNameWithoutExtension(_config.Value.Client);

                    if (p.ProcessName.Contains(kernel))
                    {
                        Kernel?.Dispose();
                        Kernel = null;
                    }
                    else if (p.ProcessName.Contains(client))
                    {
                        Client?.Dispose();
                        Client = null;
                    }
                }
                finally
                {
                    if (!InternalStart && _kernel == null && _client == null)
                        Status = "Anwendung Gestoppt";
                    SuggestRequery();
                }
            }

            private void SuggestRequery() 
                => _dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);

            public event PropertyChangedEventHandler? PropertyChanged;

            [NotifyPropertyChangedInvocator]
            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}