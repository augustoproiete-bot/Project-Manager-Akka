using System.Threading.Tasks;
using Autofac;
using Tauron.Application.Logging;
using Tauron.Application.Wpf.SerilogViewer;
using Tauron.Host;
using Tauron.Localization;

namespace Akka.MGIHelper
{
    public static class CoreProgramm
    {
        public static async Task Main(string[] args)
        {
            var builder = ActorApplication.Create(args);

            builder
                //.ConfigureAkka(context =>
                //               {
                //                   //var config = new AkkaRootConfiguration();
                //                   //config.Akka.ElementAcessor.GetAddElement<DispatcherConfiguration>("CallingThread").Type = typeof(CallingThreadDispatcherInternalConfigurator);

                //                   //return config.CreateConfig();
                //               })
                .ConfigureLogging((context, configuration) => configuration.ConfigDefaultLogging("MGI_Helper").WriteTo.Sink<SeriLogViewerSink>())
                .ConfigureAutoFac(cb => cb.RegisterModule<MainModule>())
                .ConfigurateAkkaSystem((context, system) => system.RegisterLocalization())
                .UseWpf<MainWindow>(c => c.WithAppFactory(() => new App()));

            using var app = builder.Build();
            await app.Run();
        }
    }
}