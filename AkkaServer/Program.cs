using System;
using Akka.Actor;
using Akka.Configuration;
using AkkaShared;
using AkkaShared.Test;

namespace AkkaServer
{
    class Program
    {
        static void Main(string[] args)
        {
            using var system = ActorSystem.Create("DeployTarget", ConfigurationFactory.ParseString(@"
                akka {  
                    actor.provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                    remote {
                        dot-netty.tcp {
		                    port = 8090
		                    hostname = localhost
                        }
                    }
                }"));

            ActorRegistry.Register(Services.TestService, system.ActorOf<EchoActor>(nameof(EchoActor)));

            Console.WriteLine("Press Key to close");
            Console.ReadKey();
        }
    }
}
