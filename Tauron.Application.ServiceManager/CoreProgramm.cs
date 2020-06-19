using System.Threading.Tasks;
using Autofac;
using Syncfusion.Licensing;
using Tauron.Application.AkkaNode.Boottrap;
using Tauron.Host;
using Tauron.Localization;

namespace Tauron.Application.ServiceManager
{
    public static class CoreProgramm
    {
        public static async Task Main(string[] args)
        {
            SyncfusionLicenseProvider.RegisterLicense("MjY0ODk0QDMxMzgyZTMxMmUzMEx6Vkt0M1ZIRFVPRWFqMEcwbWVrK3dqUldkYzZiaXA3TGFlWDFORDFNSms9");

            await ActorApplication.Create(args)
               .ConfigurateNode()
               .ConfigurateAkkaSystem((context, system) => system.RegisterLocalization())
               .ConfigureAutoFac(cb => cb.RegisterModule<CoreModule>())
               .UseWpf<MainWindow>(configuration => configuration.WithAppFactory(() => new App()))
               .Build().Run();

        }
    }
}