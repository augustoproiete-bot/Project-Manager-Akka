using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Cluster;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Newtonsoft.Json;
using Serilog;
using ServiceHost.Installer;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.Master.Commands;
using Tauron.Host;

namespace ServiceHost
{
    class Program
    {
        private const string MonitorName = @"Global\Tauron.Application.ProjectManagerHost";

        static async Task Main(string[] args)
        {
            bool createdNew = false;

            using var m = await Task.Factory.StartNew(() => new Mutex(true, MonitorName, out createdNew), CancellationToken.None, 
                TaskCreationOptions.HideScheduler | TaskCreationOptions.DenyChildAttach, MutexThread.Inst);
            try
            {
                if (createdNew)
                {
                    await StartApp(args);
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
                        Console.ReadKey();
                    }
                }
            }
            finally
            {
                if (createdNew)
                {
                    await Task.Factory.StartNew(() => m.ReleaseMutex(), CancellationToken.None,
                        TaskCreationOptions.HideScheduler | TaskCreationOptions.DenyChildAttach, MutexThread.Inst);
                    MutexThread.Inst.Dispose();
                }
                IncomingCommandHandler.Handler?.Stop();
            }
        }

        private static async Task StartApp(string[] args)
        {
            await Bootstrap.StartNode(args, KillRecpientType.Host)
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
                            ParseAndRunData(Encoding.UTF8.GetString(dataBuffer.Memory.Slice(0, count).Span));
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

            private void ParseAndRunData(string rawdata)
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(rawdata);
                var config = new ConfigurationBuilder().AddInMemoryCollection(data).Build();

                _installTrigger(config).Run();
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

        public sealed class MutexThread : TaskScheduler, IDisposable
        {
            public static readonly MutexThread Inst = new MutexThread();

            private readonly BlockingCollection<Task> _tasks = new BlockingCollection<Task>();

            private MutexThread()
            {
                Thread thread = new Thread(() =>
                {
                    foreach (var task in _tasks.GetConsumingEnumerable()) 
                        TryExecuteTask(task);

                    _tasks.Dispose();
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.IsBackground = true;

                thread.Start();
            }

            protected override IEnumerable<Task>? GetScheduledTasks() => _tasks;

            protected override void QueueTask(Task task) 
                => _tasks.Add(task);

            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                if (taskWasPreviouslyQueued)
                    return false;

                _tasks.Add(task);

                return false;
            }

            public void Dispose()
            {
                _tasks.CompleteAdding();
            }
        }
    }
}
