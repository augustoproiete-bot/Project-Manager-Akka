using System.IO;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.Configuration;
using Master.Seed.Node.Commands;
using Petabridge.Cmd.Host;
using Serilog;
using Tauron.Application.Master.Commands;

namespace Master.Seed.Node
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.ColoredConsole().CreateLogger();

            var config = ConfigurationFactory.ParseString(await File.ReadAllTextAsync("Master.conf"));
            using var master = ActorSystem.Create("Project-Manager", config);

            KillSwitch.Enable(master);

            var cmd = PetabridgeCmd.Get(master);
            cmd.RegisterCommandPalette(Petabridge.Cmd.Cluster.ClusterCommands.Instance);
            cmd.RegisterCommandPalette(MasterCommand.New);
            cmd.Start();

            await master.WhenTerminated;
        }
    }
}
