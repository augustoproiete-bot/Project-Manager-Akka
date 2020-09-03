using System.Windows.Threading;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.ViewModels
{
    public sealed class SetupBuilderViewModel : UiActor
    {
        public SetupBuilderViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher) 
            : base(lifetimeScope, dispatcher)
        {
        }
    }
}