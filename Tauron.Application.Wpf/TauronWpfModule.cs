using System.Windows.Threading;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Wpf.AppCore;
using Tauron.Application.Wpf.Implementation;
using Tauron.Application.Wpf.UI;
using Tauron.Host;

namespace Tauron.Application.Wpf
{
    [PublicAPI]
    public sealed class TauronWpfModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AppLifetime>().Named<IAppRoute>("default");
            builder.RegisterType<AutoViewLocation>().AsSelf();
            builder.RegisterType<PackUriHelper>().As<IPackUriHelper>();
            builder.RegisterType<ImageHelper>().As<IImageHelper>().SingleInstance();
            builder.RegisterType<DialogFactory>().As<IDialogFactory>();
            builder.Register(cc => System.Windows.Application.Current.Dispatcher).As<Dispatcher>();
        }
    }
}