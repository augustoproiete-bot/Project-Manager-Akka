using System.Windows.Threading;
using Akka.Actor;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Master.Commands.Host;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.ViewModels
{
    public sealed class HostViewModel : UiActor
    {
        private readonly IActorRef _hostConnector;

        public HostViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher) 
            : base(lifetimeScope, dispatcher)
        {
            _hostConnector = HostApi.Create(Context);
        }
    }
}