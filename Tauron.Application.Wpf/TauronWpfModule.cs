using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Wpf.AppCore;

namespace Tauron.Application.Wpf
{
    [PublicAPI]
    public sealed class TauronWpfModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AppLifetime>().Named<AppLifetime>("default");
        }
    }
}