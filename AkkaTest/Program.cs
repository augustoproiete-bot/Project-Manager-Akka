using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Code.Configuration;
using Akka.Code.Configuration.Elements;
using Akka.Configuration;
using Akka.Dispatch;
using Akka.Logger.Serilog;
using Serilog;
using Tauron.Application.AkkNode.Services.FileTransfer;
using Tauron.Application.Master.Commands.Core;

namespace AkkaTest
{
    [DebuggerNonUserCode]
    public sealed class CallingThreadDispatcherInternalConfigurator : MessageDispatcherConfigurator
    {
        private readonly CallingThreadDispatcherInternal _dispatcher;

        public CallingThreadDispatcherInternalConfigurator(Config config, IDispatcherPrerequisites prerequisites) : base(config, prerequisites)
        {
            _dispatcher = new CallingThreadDispatcherInternal(this);
        }

        public override MessageDispatcher Dispatcher()
        {
            return _dispatcher;
        }
    }

    [DebuggerNonUserCode]
    public sealed class CallingThreadDispatcherInternal : MessageDispatcher
    {
        public CallingThreadDispatcherInternal(MessageDispatcherConfigurator configurator)
            : base(configurator)
        {
        }

        protected override void ExecuteTask(IRunnable run)
        {
            run.Run();
        }

        protected override void Shutdown()
        {
        }
    }


    internal class Program
    {
        private sealed class StartTest
        {
            
        }

        private sealed class Tester : ReceiveActor
        {
            private const string TestFile = @"F:\Test\Syncfusion.Tools.WPF.dll";
            private const string TestTarget = @"F:\Test\Syncfusion.Tools.WPF-Test.dll";

            public Tester()
            {
                Receive<StartTest>(_ => StartTestImpl());
                Receive<IncomingDataTransfer>(HandleEvent);
                Receive<TransferFailed>(HandleEvent);
                Receive<TransferCompled>(HandleEvent);
            }

            private void HandleEvent(TransferCompled obj) => Console.WriteLine($"Operation Compled Id:{obj.OperationId}");

            private void HandleEvent(TransferFailed obj) => Console.WriteLine($"Tarnsfering Failed Reason:{obj.Reason}  Data:{obj.Data}  Id:{obj.OperationId}");

            private void HandleEvent(IncomingDataTransfer obj) => obj.Accept(() => File.Create(TestTarget));

            private void StartTestImpl()
            {
                var sender = DataTransferManager.New(Context);
                var reciver = DataTransferManager.New(Context);

                sender.SubscribeToEvent<TransferCompled>();
                sender.SubscribeToEvent<TransferFailed>();
                reciver.SubscribeToEvent<IncomingDataTransfer>();

                sender.Tell(DataTransferRequest.FromFile(TestFile, reciver, "Test Sending"));
            }
        }

        private static async Task Main()
        {
            //ProxyTest.TestProxy();
            //SynchronizationContext.SetSynchronizationContext(new TestSync());
            //AutoFacTest.TestAutofac();

            ////https://github.com/petabridge/akka-bootcamp/blob/master/src/Unit-3/lesson5/README.md
            //var config = ConfigurationFactory.ParseString(File.ReadAllText("akka.config.hocon"));
            var configRoot = new AkkaRootConfiguration();

            configRoot.Akka.Actor.DefaultDispatcher<DispatcherConfiguration>().Type = typeof(CallingThreadDispatcherInternalConfigurator);

            configRoot.Akka.Loggers.Add(typeof(SerilogLogger));

            var config = configRoot.CreateConfig();

            Log.Logger = new LoggerConfiguration().WriteTo.File("Log.Log").CreateLogger();
            using var system = ActorSystem.Create("Test", config);
            
            system.ActorOf(Props.Create<Tester>()).Tell(new StartTest());

            Console.WriteLine("Zum Beenden Taste drücken...");
            Console.ReadKey();

            await system.Terminate();
        }

        //[DebuggerNonUserCode]
        //private sealed class TestSync : SynchronizationContext
        //{
        //    public override void Post(SendOrPostCallback d, object? state)
        //    {
        //        d(state);
        //    }

        //    public override void Send(SendOrPostCallback d, object? state)
        //    {
        //        d(state);
        //    }
        //}
    }
}