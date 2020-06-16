using System.Threading.Tasks;
using Autofac;
using Tauron.Application.AkkaNode.Boottrap;
using Tauron.Host;

namespace Tauron.Application.ServiceManager
{
    public static class CoreProgramm
    {
        public static async Task Main(string[] args)
        {
            await ActorApplication.Create(args)
               .ConfigurateNode()
               .ConfigureAutoFac(cb => cb.RegisterModule<CoreModule>())
               .Build().Run();

        }
    }
}