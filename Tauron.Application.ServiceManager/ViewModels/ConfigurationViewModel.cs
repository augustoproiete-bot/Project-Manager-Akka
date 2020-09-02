using System.Windows.Threading;
using Autofac;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.ViewModels
{
    public sealed class ConfigurationViewModel : UiActor
    {
        public ConfigurationViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher) 
            : base(lifetimeScope, dispatcher)
        {
        }
    }
}