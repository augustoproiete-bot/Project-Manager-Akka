using Petabridge.Cmd;
using Petabridge.Cmd.Host;
using Tauron.Application.Master.Commands;

namespace Master.Seed.Node.Commands
{
    public sealed class MasterCommandHandlerActor : CommandHandlerActor
    {
        public MasterCommandHandlerActor() 
            : base(MasterCommands.MasterPalette)
        {
            Process("kill", command => KillSwitch.KillCluster(Context.System));
        }
    }
}