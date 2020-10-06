using System.Threading.Tasks;
using Akka.Cluster;
using ServiceManager.ProjectRepository;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.Master.Commands;

namespace RepositoryServer
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await Bootstrap.StartNode(args, KillRecpientType.Service)
               .ConfigurateAkkaSystem((context, system) =>
                    {
                        RepositoryManager.InitRepositoryManager(system, system.Settings.Config.GetString("akka.persistence.journal.mongodb.connection-string"));
                    })
               .Build().Run();
        }
    }
}
