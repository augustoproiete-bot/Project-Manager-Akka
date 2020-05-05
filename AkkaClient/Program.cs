using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Code.Configuration;
using Akka.Logger.Serilog;
using Akka.Remote;
using AkkaShared;
using Serilog;
using Tauron.Application.Akka.ServiceResolver;
using Tauron.Application.Akka.ServiceResolver.Configuration;
using Tauron.Application.Akka.ServiceResolver.Core;
using Tauron.Application.Akka.ServiceResolver.Data;

namespace AkkaClient
{
    class Program
    {
        private class ConsoleActor : ReceiveActor
        {
            public ConsoleActor()
            {
                Receive<StringMessage>(StringMessage);
                Receive<string>(SendMsg);
            }

            private void SendMsg(string obj)
            {
                var remote = Context.ResolveRemoteService(EchoService.Name);
                remote.Service.Tell(new StringMessage(obj));
            }

            private void StringMessage(StringMessage obj) 
                => Console.WriteLine(obj.Message);
        }

        private static bool _suspended;
        
        public static async Task Main(string[] args)
        {
            Console.Title = "Test Client";

            var codeConfig = new AkkaRootConfiguration();
            codeConfig.Akka.Actor.Provider = typeof(RemoteActorRefProvider);

            var tcp = codeConfig.Akka.Remote.AddDotNettyTcp();
            tcp.Port = 0;
            tcp.HostName = "localhost";

            var serviceResolver = codeConfig.ServiceResolver();
            serviceResolver.IsGlobal = false;
            serviceResolver.ResolverPath = "akka.tcp://DeployTarget@localhost:8090/user/GlobalResolver";
            serviceResolver.Name = "EchoServiceConsumer";

            codeConfig.Akka.Loggers.Add(typeof(SerilogLogger));
            var config = codeConfig.CreateConfig();

            Log.Logger = new LoggerConfiguration().WriteTo.ColoredConsole().CreateLogger();

            Console.WriteLine("Service Testen");
            Console.ReadKey();

            var system = ActorSystem.Create("DeployTarget", config);

            system.AddServiceResolver().RegisterEndpoint(EndpointConfig.New
               .WithServiceRequirement(ServiceRequirement.Create(EchoService.Name))
               .WithSuspensionTracker(new ActionSuspensionTracker(m =>
                                                                  {
                                                                      _suspended = m.IsSuspended;
                                                                      var state = _suspended ? "Angehaltem" : "Laufend";
                                                                      Console.WriteLine($"Service Zusand Geändert: {state}");
                                                                  })));

            var consoleActor = await system.HostLocalService("ConsoleActor", Props.Create<ConsoleActor>());

            while (true)
            {
                Console.WriteLine("Bitte Nachricht eingeben:");
                var msg = Console.ReadLine();
                if(msg == "exit") break;
                if (_suspended)
                {
                    Console.WriteLine("Service Angehalten");
                    continue;
                }

                consoleActor.Tell(msg);
            }

            await system.Terminate();
        }
    }
}
