using Akka.Actor;
using Petabridge.Cmd;
using Petabridge.Cmd.Host;

namespace Master.Seed.Node.Commands
{
    public sealed class MasterCommandHandler : CommandPaletteHandler
    {
        public static MasterCommandHandler New => new MasterCommandHandler();

        private MasterCommandHandler() 
            : base(MasterCommands.MasterPalette) =>
            HandlerProps = Props.Create<MasterCommandHandlerActor>();

        public override Props HandlerProps { get; }
    }
}