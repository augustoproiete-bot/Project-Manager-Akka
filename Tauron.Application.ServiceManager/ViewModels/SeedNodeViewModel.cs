using System.Windows.Threading;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.ViewModels
{
    public class SeedNodeViewModel : UiActor
    {
        public SeedNodeViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher) 
            : base(lifetimeScope, dispatcher)
        {
        }
    }
}