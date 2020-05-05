using System;
using Akka.Actor;
using Akka.Code.Configuration;
using Akka.Logger.Serilog;
using Akka.Remote;
using Serilog;
using Tauron.Application.Akka.ServiceResolver;
using Tauron.Application.Akka.ServiceResolver.Configuration;

namespace AkkaServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Global Resolver";

            //var config = ConfigurationFactory.ParseString(@"
            //    akka {  
            //        actor.provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
            //        remote {
            //            dot-netty.tcp {
		          //          port = 8090
		          //          hostname = localhost
            //            }
            //        }
            //    }");

            var codeConfig = new AkkaRootConfiguration();
            codeConfig.Akka.Actor.Provider = typeof(RemoteActorRefProvider);

            var tcp = codeConfig.Akka.Remote.AddDotNettyTcp();
            tcp.Port = 8090;
            tcp.HostName = "localhost";

            codeConfig.ServiceResolver().IsGlobal = true;
            codeConfig.Akka.Loggers.Add(typeof(SerilogLogger));
            Log.Logger = new LoggerConfiguration().WriteTo.ColoredConsole().CreateLogger();

            using var system = ActorSystem.Create("DeployTarget", codeConfig.CreateConfig());

            system.AddServiceResolver();

            Console.WriteLine("Press Key to close");
            Console.ReadKey();
        }
    }
}
