using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using Akka.Actor;
using JetBrains.Annotations;
using ServiceHost.ApplicationRegistry;
using Tauron.Akka;

namespace ServiceHost.Services.Impl
{
    [UsedImplicitly]
    public sealed class AppProcessActor : ExposedReceiveActor, IWithTimers
    {
        private readonly InstalledApp _app;

        private string _serviceComName = string.Empty;
        private AnonymousPipeServerStream? _serviceCom;
        private Process? _process;
        private bool _isProcesRunning;

        public ITimerScheduler Timers { get; set; } = default!;

        public AppProcessActor(InstalledApp app)
        {
            _app = app;

            CallSafe(() =>
            {
                _serviceCom = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
                _serviceComName = _serviceCom.GetClientHandleAsString();
            }, "Error while Initilizing Named pipe");

            Receive<CheckProcess>(_ =>
            {
                if(_process == null)
                {
                    if(_isProcesRunning)
                        Self.Tell(new InternalStartApp());
                    return;
                }

                if (!_process.HasExited || !_isProcesRunning) return;

                _process.Dispose();
                _process = null;

                Log.Info("Process killed Restarting {Name}", _app.Name);
                Self.Tell(new InternalStartApp());

            });

            Receive<InternalStartApp>(StartApp);
            Receive<InternalStopApp>(StopApp);

            Receive<GetName>(_ => Sender.Tell(new GetNameResponse(_app.Name, _isProcesRunning)));
        }

        private void StopApp(InternalStopApp obj)
        {
            CallSafe(
                () =>
                {
                    Log.Info("Stop Apps {Name}", _app.Name);
                    _isProcesRunning = false;
                    if (_serviceCom == null)
                    {
                        Log.Warning("None Comunication Pipe Killing {Name}", _app.Name);
                        _process?.Kill(true);
                    }
                    else
                    {
                        if(_process == null) return;

                        Log.Info("Sending KillCommand {Name}", _app.Name);
                        var writer = new BinaryWriter(_serviceCom);
                        writer.Write("Kill-Node");
                        writer.Flush();
                        
                        Log.Info("Wait for exit {Name}", _app.Name);
                        
                        var watch = Stopwatch.StartNew();

                        while (watch.Elapsed < TimeSpan.FromMinutes(1))
                        {
                            Thread.Sleep(1000);
                            if(_process.HasExited) return;
                        }

                        if (_process.HasExited) return;
                        
                        Log.Warning("Process not Exited Killing {Name}", _app.Name);
                        _process.Kill(true);
                    }
                },
                "Error while Stopping Apps",
                e =>
                {
                    _process?.Dispose();
                    if(!Sender.Equals(Context.Parent))
                        Sender.Tell(new StopResponse(_app.Name, e));
                    Context.Parent.Tell(new StopResponse(_app.Name, e));
                    Context.Stop(Self);
                });
        }

        private void StartApp(InternalStartApp obj)
        {
            CallSafe(
                () =>
                {
                    if(_isProcesRunning) return;
                    Log.Info("Start Apps {Name}", _app.Name);
                    _process = Process.Start(new ProcessStartInfo(Path.Combine(_app.Path, _app.Exe), $"--ComHandle {_serviceComName}")
                                             {
                                                 WorkingDirectory = _app.Path,
                                                 CreateNoWindow = _app.SuressWindow
                                             });
                    _isProcesRunning = true;
                }, "Error while Stratin Service");
        }

        protected override void PreStart()
        {
            Timers.StartPeriodicTimer(_serviceComName, new CheckProcess(), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));
            base.PreStart();
        }

        protected override void PostStop()
        {
            CallSafe(
                () =>
                {
                    _serviceCom?.Dispose();

                    _process?.Dispose();
                }, "Error ehile Disposing Named pipe");

            base.PostStop();
        }

        private sealed class CheckProcess { }

        public sealed class GetName
        {
            
        }

        public sealed class GetNameResponse
        {
            public string Name { get; }

            public bool Running { get; }

            public GetNameResponse(string name, bool running)
            {
                Name = name;
                Running = running;
            }
        }
    }
}