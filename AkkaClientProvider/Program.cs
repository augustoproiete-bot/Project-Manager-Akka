﻿using System;
using Akka.Actor;
using Akka.Code.Configuration;
using Akka.Logger.Serilog;
using Akka.Remote;
using AkkaShared;
using Serilog;
using Tauron.Application.Akka.ServiceResolver;
using Tauron.Application.Akka.ServiceResolver.Configuration;
using Tauron.Application.Akka.ServiceResolver.Data;

namespace AkkaClientProvider
{
    class Program
    {
        static void Main(string[] args)
        {
            var codeConfig = new AkkaRootConfiguration();
            codeConfig.Akka.Actor.Provider = typeof(RemoteActorRefProvider);

            var tcp = codeConfig.Akka.Remote.AddDotNettyTcp();
            tcp.Port = 0;
            tcp.HostName = "localhost";

            var serviceResolver = codeConfig.ServiceResolver();
            serviceResolver.IsGlobal = false;
            serviceResolver.ResolverPath = "akka.tcp://DeployTarget@localhost:8090/user/GlobalResolver";

            codeConfig.Akka.Loggers.Add(typeof(SerilogLogger));
            var config = codeConfig.CreateConfig();

            Log.Logger = new LoggerConfiguration().WriteTo.ColoredConsole().CreateLogger();

            using var system = ActorSystem.Create("DeployTarget", config);

            system.AddServiceResolver().RegisterEndpoint(ServiceRequirement.Empty, (EchoService.Name, Props.Create<EchoActor>()));

            Console.WriteLine("Press Key to close");
            Console.ReadKey();
        }
    }
}