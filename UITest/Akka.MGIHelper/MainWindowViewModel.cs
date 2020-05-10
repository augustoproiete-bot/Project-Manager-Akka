using System.Windows.Threading;
using Akka.MGIHelper.UI;
using Akka.MGIHelper.UI.FanControl;
using Akka.MGIHelper.UI.MgiStarter;
using Autofac;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Model;

namespace Akka.MGIHelper
{
    public sealed class MainWindowViewModel : UiActor
    {
        private IViewModel<MgiStarterControlModel> MgiStarter
        {
            set => Set(value);
        }

        private IViewModel<AutoFanControlModel> FanControl
        {
            set => Set(value);
        }

        public MainWindowViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, IViewModel<MgiStarterControlModel> mgiStarter, IViewModel<AutoFanControlModel> autoFanControl) 
            : base(lifetimeScope, dispatcher)
        {
            mgiStarter.Init(Context, "MGI-Starter-Model");
            MgiStarter = mgiStarter;

            autoFanControl.Init(Context, "Auto-Fan-Control");
            FanControl = autoFanControl;

            RegisterCommand("OpenLogs", _ => ShowWindow<LogWindow>());
        }
    }
}