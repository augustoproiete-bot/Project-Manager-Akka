using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Wpf.AppCore;
using Tauron.Application.Wpf.UI;

namespace Tauron.Application.Wpf
{
    [PublicAPI]
    public sealed class TauronWpfModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AppLifetime>().Named<AppLifetime>("default");
            builder.RegisterType<AutoViewLocation>().AsSelf();
        }
    }
}