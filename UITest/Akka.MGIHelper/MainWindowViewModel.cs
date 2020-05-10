using System.Windows.Threading;
using Akka.MGIHelper.UI;
using Autofac;
using Tauron.Application.Wpf.Model;

namespace Akka.MGIHelper
{
    public sealed class MainWindowViewModel : UiActor
    {
        public MainWindowViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher) 
            : base(lifetimeScope, dispatcher)
        {
            RegisterCommand("OpenLogs", _ => ShowWindow<LogWindow>());
        }
    }
}