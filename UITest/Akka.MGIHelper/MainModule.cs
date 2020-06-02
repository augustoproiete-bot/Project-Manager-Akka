using Akka.Actor;
using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Settings;
using Akka.MGIHelper.UI;
using Akka.MGIHelper.UI.FanControl;
using Akka.MGIHelper.UI.MgiStarter;
using Autofac;
using Tauron.Application.Wpf;

namespace Akka.MGIHelper
{
    public sealed class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterView<MainWindow, MainWindowViewModel>().OnActivating(m => m.Instance.Init("Main-Window"));
            builder.RegisterView<MgiStarterControl, MgiStarterControlModel>();
            builder.RegisterView<AutoFanControl, AutoFanControlModel>();
            builder.RegisterView<LogWindow, LogWindowViewModel>();

            builder.RegisterDefaultActor<SettingsManager>().SingleInstance()
                .OnActivating(e => e.Instance.Init(nameof(SettingsManager)))
                .OnRelease(sm => sm.Actor.Tell(PoisonPill.Instance));

            builder.RegisterType<WindowOptions>().AsSelf().InstancePerLifetimeScope().WithParameter("scope", SettingTypes.WindowOptions);
            builder.RegisterType<FanControlOptions>().AsSelf().InstancePerLifetimeScope().WithParameter("scope", SettingTypes.FanControlOptions);
            builder.RegisterType<ProcessConfig>().AsSelf().InstancePerLifetimeScope().WithParameter("scope", SettingTypes.ProcessOptions);

            builder.RegisterType<MgiStartingActor>().AsSelf();
        }
    }
}