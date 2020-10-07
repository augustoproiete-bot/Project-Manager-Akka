using System.Threading.Tasks;
using ServiceManager.ProjectDeployment;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.Master.Commands;

namespace DeploymentServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Bootstrap.StartNode(args, KillRecpientType.Service)
               .ConfigurateAkkaSystem((context, system) =>
                {
                    DeploymentManager.InitDeploymentManager(
                        system,
                        system.Settings.Config.GetString("akka.persistence.journal.mongodb.connection-string"));
                })
               .Build().Run();
        }
    }
}
