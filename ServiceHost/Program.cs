using System;
using System.IO;
using System.Threading;
using Akka.Actor;
using System.Threading.Tasks;
using Akka.Cluster;
using Akka.Persistence;
using Akka.Persistence.Query;
using Akka.Persistence.Query.Sql;
using Autofac;
using Microsoft.Data.Sqlite;
using ServiceHost.ApplicationRegistry;
using Tauron.Application.AkkaNode.Boottrap;
using Tauron.Application.Master.Commands;
using Tauron.Host;
using ApplicationId = ServiceHost.ApplicationRegistry.ApplicationId;

namespace ServiceHost
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var temp = ActorApplication.Create(args)
                .StartNode(KillRecpientType.Host)
                .ConfigureAutoFac(cb => cb.RegisterModule<HostModule>())
                .ConfigurateAkkaSystem((context, system) =>
                {
                    var cluster = Cluster.Get(system);
                    cluster.RegisterOnMemberUp(()
                        => ServiceRegistry.GetRegistry(system).RegisterService(new RegisterService(context.HostEnvironment.ApplicationName, cluster.SelfUniqueAddress)));


                })
                .Build().Run();

            await temp;
        }
    }
}
