using Akka.Actor;
using ServiceHost.ApplicationRegistry;
using ServiceHost.Installer.Impl;
using Tauron.Akka;

namespace ServiceHost.Installer
{
    public sealed class InstallManagerActor : ExposedReceiveActor
    {
        public InstallManagerActor(IAppRegistry registry)
        {
            ReceiveAny(o => Context.ActorOf(Props.Create<ActualInstallerActor>(registry)).Forward(o));
        }
    }
}