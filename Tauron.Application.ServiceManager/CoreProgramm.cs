using System.Threading.Tasks;
using Autofac;
using Syncfusion.Licensing;
using Tauron.Application.AkkaNode.Boottrap;
using Tauron.Host;

namespace Tauron.Application.ServiceManager
{
    public static class CoreProgramm
    {
        public static async Task Main(string[] args)
        {
            SyncfusionLicenseProvider.RegisterLicense("MjY0ODk0QDMxMzgyZTMxMmUzMEx6Vkt0M1ZIRFVPRWFqMEcwbWVrK3dqUldkYzZiaXA3TGFlWDFORDFNSms9");

            await ActorApplication.Create(args)
               .ConfigurateNode()
               .ConfigureAutoFac(cb => cb.RegisterModule<CoreModule>())
               .Build().Run();

        }
    }
}