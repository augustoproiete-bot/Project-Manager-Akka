using Akka.Actor;
using Autofac;
using Tauron.Akka;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Services;
using Tauron.Application.Localizer.UIModels.Settings;
using Tauron.Application.Settings;

namespace Tauron.Application.Localizer.UIModels
{
    public sealed class UIModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LocLocalizer>().AsSelf();
            builder.RegisterType<OperationManager>().As<IOperationManager>().SingleInstance();
            builder.RegisterType<AppConfig>().AsSelf().WithParameter("scope", SettingTypes.AppConfig).InstancePerLifetimeScope();

            builder.RegisterType<AppConfiguration>().As<ISettingProviderConfiguration>();

            builder.RegisterType<DefaultActorRef<SettingsManager>>().As<IDefaultActorRef<SettingsManager>>()
                .OnActivating(i => i.Instance.Init("Settings-Manager"))
                .OnRelease(sm => sm.Actor.Tell(PoisonPill.Instance))
                .SingleInstance();
        }
    }
}