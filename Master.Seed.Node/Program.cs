using System.Threading.Tasks;
using Akka.Cluster;
using BeaconLib;
using Master.Seed.Node.Commands;
using Petabridge.Cmd.Cluster;
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
            Beacon? beacon = null;

            try
            {
                await Boottrap.StartNode(args, KillRecpientType.Seed)
                   .ConfigurateAkkaSystem((context, system) =>
                                          {
                                              var cluster = Cluster.Get(system);
                                              cluster.RegisterOnMemberUp(() =>
                                                                         {
                                                                             var port = cluster.SelfAddress.Port;
                                                                             if (port != null)
                                                                             {
                                                                                 beacon = new Beacon(system.Name, (ushort)port)
                                                                                 {
                                                                                     BeaconData = cluster.SelfAddress.ToString()
                                                                                 };
                                                                                 beacon.Start();
                                                                             }
                                                                             ServiceRegistry.Start(system, new RegisterService(context.HostEnvironment.ApplicationName, cluster.SelfUniqueAddress));
                                                                         });

                                              var cmd = PetabridgeCmd.Get(system);
                                              cmd.RegisterCommandPalette(ClusterCommands.Instance);
                                              cmd.RegisterCommandPalette(MasterCommand.New);
                                              cmd.Start();
                                          })
                   .Build().Run();
            }
            finally
            {
                beacon?.Dispose();
            }
        }
    }
}
