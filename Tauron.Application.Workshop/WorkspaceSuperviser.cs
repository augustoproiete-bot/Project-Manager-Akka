using System.Threading.Tasks;
using Akka.Actor;
using Tauron.Application.Workshop.Core;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop
{
    public sealed class WorkspaceSuperviser
    {
        private IActorRef Superviser { get; }

        public WorkspaceSuperviser(IActorRefFactory context, string? name = null) 
            => Superviser = context.ActorOf<WorkspaceSuperviserActor>(name);

        internal WorkspaceSuperviser() => Superviser = ActorRefs.Nobody;

        public async Task<IActorRef> Create(Props props, string name)
        {
            var result = await Superviser.Ask<WorkspaceSuperviserActor.NewActor>(new WorkspaceSuperviserActor.SuperviseActor(props, name));
            return result.ActorRef;
        }

        public void WatchIntrest(WatchIntrest intrest)
            => Superviser.Tell(intrest);
    }
}