using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Actor.Setup;
using Akka.Cluster;
using Akka.Configuration;
using Akka.Event;
using Serilog;
using ServiceManager.ProjectRepository;
using Tauron.Akka;
using Tauron.Application.Master.Commands;
using Tauron.Application.ServiceManager.Core.Configuration;
using Tauron.Application.ServiceManager.Core.SetupBuilder;
using Tauron.Application.Settings;

namespace ProtoTyping
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var builder = new SetupBuilder("Main-Processor", "Main-Processor", new AppConfig(new EmptyActor<SettingsManager>()), Console.WriteLine, null);

            var result = builder.Run(null!, "12345", "127.0.0.1:85");

            result?.Dispose();

            return;

            Log.Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .Enrich.FromLogContext()
                .CreateLogger();

            string url = "akka.tcp://Project-Manager@192.168.105.18:8081";

            var system = ActorSystem.Create("Project-Manager", ConfigurationFactory.ParseString(await File.ReadAllTextAsync("akka.conf")));

            var c = Cluster.Get(system);
            //c.RegisterOnMemberUp(() => ServiceRegistry.Init(system));

            await c.JoinSeedNodesAsync(new[] { Address.Parse(url) });

            bool run = true;

            while (run)
            {
                switch (Console.ReadLine())
                {
                    case "e":
                        run = false;
                        break;
                    case "t":
                        try
                        {
                            var reg = ServiceRegistry.GetRegistry(system);
                            reg.RegisterService(new RegisterService("Test Service", Cluster.Get(system).SelfUniqueAddress));

                            var r = await reg.QueryService();

                            Console.WriteLine();
                            foreach (var mem in r.Services)
                                Console.WriteLine($"{mem.Name} -- {mem.Address}");

                            Console.WriteLine();
                            Console.WriteLine("Fertig");
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            run = false;
                        }
                        break;
                    default:
                        Console.WriteLine("Unbekannt");
                        break;

                }
            }

            await system.Terminate();
        }
    }
}