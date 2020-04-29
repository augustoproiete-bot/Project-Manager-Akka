﻿using System;
using Akka.Actor;
using Akka.Configuration;
using AkkaShared;
using AkkaShared.Test;

namespace AkkaClient
{
    class Program
    {
        class SayHello { }

        class HelloActor : ReceiveActor
        {
            private IActorRef _remoteActor;
            private int _helloCounter;
            private ICancelable _helloTask;

            public HelloActor(IActorRef remoteActor)
            {
                _remoteActor = remoteActor;
                Context.Watch(_remoteActor);
                Receive<Hello>(hello =>
                {
                    Console.WriteLine("Received {1} from {0}", Sender, hello.Message);
                });

                Receive<SayHello>(sayHello =>
                {
                    _remoteActor.Tell(new Hello("hello" + _helloCounter++));
                });

                Receive<Terminated>(terminated =>
                {
                    Console.WriteLine(terminated.ActorRef);
                    Console.WriteLine("Was address terminated? {0}", terminated.AddressTerminated);
                    _helloTask.Cancel();
                });
            }

            protected override void PreStart()
            {
                _helloTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1), Context.Self, new SayHello(), ActorRefs.NoSender);
            }

            protected override void PostStop()
            {
                _helloTask.Cancel();
            }
        }

        static void Main(string[] args)
        {
            using var system = ActorSystem.Create("Deployer", ConfigurationFactory.ParseString(@"
                akka {  
                    actor{
                        provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                        deployment {
                            /remoteresolver {
                                remote = ""akka.tcp://DeployTarget@localhost:8090""
                            }
                        }
                    }
                    remote {
                        dot-netty.tcp {
		                    port = 0
		                    hostname = localhost
                        }
                    }
                }"));

            var resolver = system.ActorOf(Props.Create(() => new ResolverActor()), "remoteresolver"); //deploy remotely via config

            var remoteEcho1 = resolver.Ask<ResolveResult>(new TryResolve(Services.TestService)).Result.Actor;

            system.ActorOf(Props.Create(() => new HelloActor(remoteEcho1)));
            remoteEcho1.Tell(new Hello("hi from selection!"));

            Console.WriteLine("Press Key to close");
            Console.ReadKey();
        }
    }
}
