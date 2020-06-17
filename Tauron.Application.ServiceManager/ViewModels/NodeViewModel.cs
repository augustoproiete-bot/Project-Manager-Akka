using System.Windows.Threading;
using Autofac;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.ViewModels
{
    public class NodeViewModel : UiActor
    {
        public NodeViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher) : base(lifetimeScope, dispatcher)
        {
        }
    }
}