using System.Threading.Tasks;
using Akka.Cluster;
using Master.Seed.Node.Commands;
using Petabridge.Cmd.Host;
using Tauron.Application.AkkaNode.Boottrap;
using Tauron.Application.Master.Commands;
using Tauron.Host;

namespace Master.Seed.Node
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await ActorApplication.Create(args)
               .StartNode()
               .ConfigurateAkkaSystem((context, system) =>
                                      {
                                          var cluster = Cluster.Get(system);
                                          cluster.RegisterOnMemberUp(() => ServiceRegistry.Start(system));

                                         var cmd = PetabridgeCmd.Get(system);
                                         cmd.RegisterCommandPalette(Petabridge.Cmd.Cluster.ClusterCommands.Instance);
                                         cmd.RegisterCommandPalette(MasterCommand.New);
                                         cmd.Start();
                                     })
               .Build().Run();
        }
    }
}
