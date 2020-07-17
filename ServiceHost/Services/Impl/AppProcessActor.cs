using JKang.IpcServiceFramework;
using ServiceHost.ApplicationRegistry;
using Tauron.Akka;

namespace ServiceHost.Services.Impl
{
    public sealed class AppProcessActor : ExposedReceiveActor
    {
        public AppProcessActor(InstalledApp app, IpcServiceHost host)
        {
            
        }
    }
}