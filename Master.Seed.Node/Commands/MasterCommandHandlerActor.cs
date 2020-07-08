using System;
using System.Text;
using Akka.Actor;
using Petabridge.Cmd;
using Petabridge.Cmd.Host;
using Tauron.Application.Master.Commands;

namespace Master.Seed.Node.Commands
{
    public sealed class MasterCommandHandlerActor : CommandHandlerActor
    {
        private IActorRef _lastSemder = ActorRefs.Nobody;

        public MasterCommandHandlerActor() 
            : base(MasterCommands.MasterPalette)
        {
            Process(MasterCommands.Kill.Name, command => KillSwitch.KillCluster());
            Process(MasterCommands.ListServices.Name, ListServices);
        }

        private void ListServices(Command obj)
        {
            var reg = ServiceRegistry.GetRegistry(Context.System);
            _lastSemder = Sender;

            reg.QueryService()
                .ContinueWith(r =>
                {
                    try
                    {
                        var builder = new StringBuilder();

                        foreach (var res in r.Result.Services) 
                            builder.AppendLine($"{res.Name} - -{res.Address}");
                        return new CommandResponse(builder.ToString());
                    }
                    catch (Exception e)
                    {
                        return new ErroredCommandResponse(e.Message);
                    }
                }).PipeTo(Sender);
        }
    }
}