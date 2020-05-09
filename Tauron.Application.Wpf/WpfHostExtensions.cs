using System;
using Autofac;
using Autofac.Builder;
using JetBrains.Annotations;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.AppCore;

// ReSharper disable once CheckNamespace
namespace Tauron.Host
{
    [PublicAPI]
    public static class WpfHostExtensions
    {
        public static IApplicationBuilder UseWpf<TMainWindow>(this IApplicationBuilder hostBuilder, Action<WpfConfiguration>? config = null)
            where TMainWindow : class, IMainWindow
        {
            hostBuilder.ConfigureAutoFac(sc =>
            {
                sc.RegisterModule<TauronWpfModule>();

                sc.RegisterType<TMainWindow>().As<IMainWindow>();

                var wpf = new WpfConfiguration(sc);
                config?.Invoke(wpf);
            });

            return hostBuilder;
        }
        
        public static IRegistrationBuilder<SimpleSplashScreen<TWindow>, ConcreteReflectionActivatorData, SingleRegistrationStyle> 
            AddSplash<TWindow>(this ContainerBuilder collection) where TWindow : System.Windows.Window, new()
        {
            return collection.RegisterType<SimpleSplashScreen<TWindow>>().As<ISplashScreen>();
        }
    }
}