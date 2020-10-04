using System.Threading.Tasks;
using Akka.Actor;
using Tauron.Application.Workshop.Core;

namespace Tauron.Application.Workshop
{
    public sealed class WorkspaceSuperviser
    {
        private IActorRef Superviser { get; }

        public WorkspaceSuperviser(IActorRefFactory context, string? name = null) 
            => Superviser = context.ActorOf<WorkspaceSuperviserActor>(name);

        internal async Task<IActorRef> Create(Props props, string name)
        {
            var result = await Superviser.Ask<WorkspaceSuperviserActor.NewActor>(new WorkspaceSuperviserActor.SuperviseActor(props, name));
            return result.ActorRef;
        }
    }
}