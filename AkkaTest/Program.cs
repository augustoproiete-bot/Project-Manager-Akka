using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Code.Configuration;
using Akka.Code.Configuration.Elements;
using Akka.Configuration;
using Akka.Dispatch;
using Akka.Logger.Serilog;
using Serilog;
using Tauron.Application.Akka.ServiceResolver;
using Tauron.Application.Akka.ServiceResolver.Actor;
using Tauron.Application.Akka.ServiceResolver.Configuration;
using Tauron.Application.Akka.ServiceResolver.Core;
using Tauron.Application.Akka.ServiceResolver.Data;

namespace AkkaTest
{
    [DebuggerNonUserCode]
    public sealed class CallingThreadDispatcherInternalConfigurator : MessageDispatcherConfigurator
    {
        private CallingThreadDispatcherInternal _dispatcher;

        public CallingThreadDispatcherInternalConfigurator(Config config, IDispatcherPrerequisites prerequisites) : base(config, prerequisites)
        {
            _dispatcher = new CallingThreadDispatcherInternal(this);
        }

        public override MessageDispatcher Dispatcher()
            => _dispatcher;
    }

    [DebuggerNonUserCode]
    public sealed class CallingThreadDispatcherInternal : MessageDispatcher
    {
        public CallingThreadDispatcherInternal(MessageDispatcherConfigurator configurator)
            : base(configurator)
        {

        }

        protected override void ExecuteTask(IRunnable run) 
            => run.Run();

        protected override void Shutdown()
        { }
    }

    public sealed class TestMessage
    {
        public string Message { get; }

        public TestMessage(string message) 
            => Message = message;
    }

    public sealed class KillService
    {

    }

    public sealed class TestService : ReceiveActor
    {
        public TestService()
        {
            Receive<TestMessage>(TestMessage);
            Receive<KillService>(KillService);
        }

        private void KillService(KillService obj) 
            => Context.Self.Tell(PoisonPill.Instance);

        private void TestMessage(TestMessage obj)
        {
            Console.WriteLine();
            Console.WriteLine(obj.Message);
            Console.WriteLine();
        }
    }

    class Program
    {
        [DebuggerNonUserCode]
        private sealed class TestSync : SynchronizationContext
        {
            public override void Post(SendOrPostCallback d, object? state) 
                => d(state);

            public override void Send(SendOrPostCallback d, object? state) 
                => d(state);
        }

        private sealed class TestClient : ReceiveActor
        {
            public TestClient()
            {
                Receive<string>(SendTest);
                Receive<KillService>(Kill);
            }

            private void Kill(KillService obj)
            {
                var service = Context.ResolveRemoteService(nameof(TestService));
                service.Service.Tell(obj);
            }

            private void SendTest(string obj)
            {
                var service = Context.ResolveRemoteService(nameof(TestService));
                service.Service.Tell(new TestMessage(obj));
            }
        }

        static async Task Main(string[] args)
        {
            //ProxyTest.TestProxy();
            //SynchronizationContext.SetSynchronizationContext(new TestSync());

            ////https://github.com/petabridge/akka-bootcamp/blob/master/src/Unit-3/lesson5/README.md
            //var config = ConfigurationFactory.ParseString(File.ReadAllText("akka.config.hocon"));
            var configRoot = new AkkaRootConfiguration();

            configRoot.Akka.Actor.DefaultDispatcher<DispatcherConfiguration>().Type = typeof(CallingThreadDispatcherInternalConfigurator);

            configRoot.Akka.Loggers.Add(typeof(SerilogLogger));

            var resolver = configRoot.ServiceResolver();
            resolver.IsGlobal = false;
            resolver.ResolverPath = "akka://Test/user/Global";
            resolver.Name = "Client";

            var config = configRoot.CreateConfig();
            string configString = config.Root.ToString();

            Log.Logger = new LoggerConfiguration().WriteTo.Console().WriteTo.File("Log.Log").CreateLogger();
            using var system = ActorSystem.Create("Test", config);

            var globalTemp = system.ActorOf<GlobalResolver>("Global");
            globalTemp.Tell(new GlobalResolver.Initialize(new ResolverSettings(Config.Empty) { IsGlobal = true }));

            var exz = system.AddServiceResolver();

            exz.RegisterEndpoint(ServiceRequirement.Empty, (nameof(TestService), Props.Create<TestService>()));
            var client = system.ActorOf<TestClient>();

            client.Tell("Hallo vom Resolver!");
            client.Tell(new KillService());

            Console.WriteLine("Zum Beenden Taste drücken...");
            Console.ReadKey();

            globalTemp.Tell(PoisonPill.Instance);
            await system.WhenTerminated;
        }
    }
}
