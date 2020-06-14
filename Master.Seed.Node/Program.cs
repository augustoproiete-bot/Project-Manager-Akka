using System.Threading.Tasks;
using Master.Seed.Node.Commands;
using Petabridge.Cmd.Host;
using Tauron.Application.AkkaNode.Boottrap;
using Tauron.Host;

namespace Master.Seed.Node
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await ActorApplication.Create(args)
               .StartNode()
               .ConfigurateAkkSystem((context, system) =>
                                     {
                                         var test = system.Settings.Config.ToString(true);
                                         var cmd = PetabridgeCmd.Get(system);
                                         cmd.RegisterCommandPalette(Petabridge.Cmd.Cluster.ClusterCommands.Instance);
                                         cmd.RegisterCommandPalette(MasterCommand.New);
                                         cmd.Start();
                                     })
               .Build().Run();
        }
    }
}
