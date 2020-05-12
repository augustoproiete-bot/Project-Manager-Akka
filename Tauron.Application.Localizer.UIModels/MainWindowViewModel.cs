using System.Windows.Threading;
using Autofac;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels
{
    public sealed class MainWindowViewModel : UiActor
    {
        public MainWindowViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher) : base(lifetimeScope, dispatcher)
        {
        }
    }
}