using System;
using Akka.Actor;
using Autofac;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron.Application.Settings
{
    [PublicAPI]
    public static class SettingsManagerExtensions
    {
        public static void RegisterSettingsManager(this ContainerBuilder builder, Action<SettingsConfiguration>? config = null)
        {
            if (config != null)
            {
                var s = new SettingsConfiguration(builder);
                config(s);
            }

            builder.RegisterType<DefaultActorRef<SettingsManager>>().As<IDefaultActorRef<SettingsManager>>()
               .OnActivating(i => i.Instance.Init("Settings-Manager"))
               .OnRelease(sm => sm.Actor.OrElse(ActorRefs.Nobody).Tell(PoisonPill.Instance))
               .SingleInstance();
        }
    }
}