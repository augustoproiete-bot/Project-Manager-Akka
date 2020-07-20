using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Autofac;
using ServiceHost.Services;
using Tauron.Application.AkkaNode.Boottrap;
using Tauron.Application.Master.Commands;
using Tauron.Host;

namespace ServiceHost
{
    class Program
    {
        private sealed class TestKilling : IStartUpAction
        {
            private readonly IAppManager _manager;

            public TestKilling(IAppManager manager) => _manager = manager;

            public void Run()
            {
                Task.Run(() =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(15));
                    _manager.Actor.Tell(new StopApps(), ActorRefs.NoSender);
                });
            }
        }

        static async Task Main(string[] args)
        {
            await ActorApplication.Create(args)
                .StartNode(KillRecpientType.Host)
                .ConfigureAutoFac(cb =>
                {
                    cb.RegisterType<TestKilling>().As<IStartUpAction>();
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
    }
}
