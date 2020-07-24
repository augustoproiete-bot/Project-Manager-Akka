using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Newtonsoft.Json;
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
                    try
                    {
                        await using var client = new NamedPipeClientStream(".", MonitorName, PipeDirection.In, PipeOptions.Asynchronous);
                        await client.ConnectAsync(10000, CancellationToken.None);
                        
                        byte[] data = Encoding.UTF8.GetBytes(new ExposedCommandLineProvider(args).Serialize());
                        await client.WriteAsync(BitConverter.GetBytes(data.Length));
                        await client.WriteAsync(data);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            finally
            {
                if (createdNew)
                    m.ReleaseMutex();
                IncomingCommandHandler.Handler?.Stop();
            }
        }

        private sealed class ExposedCommandLineProvider : CommandLineConfigurationProvider
        {
            public ExposedCommandLineProvider(IEnumerable<string> args) : base(args) { }

            public string Serialize()
            {
                Load();
                return JsonConvert.SerializeObject(Data);
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
            private readonly NamedPipeServerStream _reader = new NamedPipeServerStream(MonitorName, PipeDirection.Out, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();

            public IncomingCommandHandler(Func<IConfiguration, ManualInstallationTrigger> installTrigger)
            {
                _installTrigger = installTrigger;
                Task.Factory.StartNew(async () => await Reader(), TaskCreationOptions.LongRunning);
            }

            private async Task Reader()
            {

                try
                {
                    while (true)
                    {
                        await _reader.WaitForConnectionAsync(_cancellationToken.Token);

                        using var buffer = MemoryPool<byte>.Shared.Rent(4);
                        if (!await TryRead(buffer, 4, _cancellationToken.Token)) continue;

                        var count = BitConverter.ToInt32(buffer.Memory.Span);
                        using var dataBuffer = MemoryPool<byte>.Shared.Rent(count);

                        if (await TryRead(buffer, count, _cancellationToken.Token)) 
                            ParseData(Encoding.UTF8.GetString(dataBuffer.Memory.Slice(0, count).Span));
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e, "Error on Read CommandLine from outer process");
                }

                await _reader.DisposeAsync();
                Handler = null;
            }

            private async Task<bool> TryRead(IMemoryOwner<byte> buffer, int lenght, CancellationToken token)
            {
                var currentLenght = 0;

                while (true)
                {
                    if (_reader.IsMessageComplete)
                        return currentLenght == lenght;
                    if (currentLenght > lenght)
                        return false;

                    currentLenght = await _reader.ReadAsync(buffer.Memory.Slice(currentLenght, lenght - currentLenght), token);

                    if (currentLenght == lenght)
                        return _reader.IsMessageComplete;
                }
            }

            private void ParseData(string data)
            {

            }

            public void Stop() 
                => _cancellationToken.Cancel();

            public static void SetHandler(IncomingCommandHandler handler)
            {
                if(Handler != null)
                    handler.Stop();
                else
                    Handler = handler;
            }

            public static IncomingCommandHandler? Handler { get; private set; }
        }
    }
}
