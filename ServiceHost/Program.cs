using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Autofac;
using Microsoft.Extensions.Configuration;
using Serilog;
using ServiceHost.Installer;
using ServiceHost.Services;
using Tauron.Application.AkkaNode.Boottrap;
using Tauron.Application.Master.Commands;
using Tauron.Host;

namespace ServiceHost
{
    class Program
    {
        private const string MonitorName = @"Global\Tauron.Application.ProjectManagerHost";

        static async Task Main(string[] args)
        {
            using var m = new Mutex(true, MonitorName, out var createdNew);
            try
            {
                if (createdNew)
                {
                    await ActorApplication.Create(args)
                        .StartNode(KillRecpientType.Host)
                        .ConfigureAutoFac(cb =>
                        {
                            cb.RegisterType<CommandHandlerStartUp>().As<IStartUpAction>();
                            cb.RegisterModule<HostModule>();
                        })
                        .ConfigurateAkkaSystem((context, system) =>
                        {
                            var cluster = Cluster.Get(system);
                            cluster.RegisterOnMemberUp(()
                                => ServiceRegistry.GetRegistry(system).RegisterService(new RegisterService(context.HostEnvironment.ApplicationName, cluster.SelfUniqueAddress)));
                        })
                        .Build().Run();
                }
                else
                {
                    var config = new ConfigurationBuilder().AddCommandLine(args).Build();

                }
            }
            finally
            {
                if (createdNew)
                    m.ReleaseMutex();
                IncomingCommandHandler.Handler?.Stop();
            }
        }

        private sealed class CommandHandlerStartUp : IStartUpAction
        {
            private readonly Func<IConfiguration, ManualInstallationTrigger> _installTrigger;

            public CommandHandlerStartUp(Func<IConfiguration, ManualInstallationTrigger> installTrigger) 
                => _installTrigger = installTrigger;

            public void Run() 
                => IncomingCommandHandler.SetHandler(new IncomingCommandHandler(_installTrigger));
        }

        public sealed class IncomingCommandHandler
        {
            private readonly Func<IConfiguration, ManualInstallationTrigger> _installTrigger;
            private readonly NamedPipeServerStream _reader = new NamedPipeServerStream(MonitorName, PipeDirection.In, 1);
            private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();

            public IncomingCommandHandler(Func<IConfiguration, ManualInstallationTrigger> installTrigger)
            {
                _installTrigger = installTrigger;
                Task.Factory.StartNew(Reader, TaskCreationOptions.LongRunning);
            }

            private void Reader()
            {
                
                try
                {
                    _reader.WaitForConnectionAsync(_cancellationToken.Token);
                    BitConverter.GetBytes()
                }
                catch(OperationCanceledException)
                {}
                catch (Exception e)
                {
                    Log.Logger.Error(e, "Error on Read CommandLine from outer process");
                }

                _reader.Dispose();
                Handler = null;
            }

            public void Stop() 
                => _cancellationToken.Cancel();

            public static void SetHandler(IncomingCommandHandler handler)
            {
                if(Handler != null)
                    handler.Dispose();
                else
                    Handler = handler;
            }

            public static IncomingCommandHandler? Handler { get; private set; }
        }
    }
}
