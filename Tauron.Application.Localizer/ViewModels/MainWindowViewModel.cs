using System.Windows.Threading;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.ViewModels
{
    public sealed class MainWindowViewModel : UiActor
    {
        public MainWindowViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher) : base(lifetimeScope, dispatcher)
        {
        }
    }
}