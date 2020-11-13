using System;
using System.Threading.Tasks;
using Akka.Actor;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Core;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop
{
    [PublicAPI]
    public sealed class WorkspaceSuperviser
    {
        private IActorRef Superviser { get; }

        public WorkspaceSuperviser(IActorRefFactory context, string? name = null) 
            => Superviser = context.ActorOf<WorkspaceSuperviserActor>(name);

        internal WorkspaceSuperviser() => Superviser = ActorRefs.Nobody;

        public async Task<IActorRef> Create(Maybe<Props> props, string name)
        {
            var result = await Superviser.Ask<WorkspaceSuperviserActor.NewActor>(new WorkspaceSuperviserActor.SupervisePropsActor(props, name));
            return result.ActorRef;
        }

        public async Task<IActorRef> Create(Type actor, string name)
        {
            var result = await Superviser.Ask<WorkspaceSuperviserActor.NewActor>(new WorkspaceSuperviserActor.SuperviseDiActor(actor, name));
            return result.ActorRef;
        }

        public void CreateAnonym(Maybe<Props> props, string name)
        {
            Superviser.Tell(new WorkspaceSuperviserActor.SupervisePropsActor(props, name), ActorRefs.NoSender);
        }

        public void CreateAnonym(Type actor, string name)
        {
            Superviser.Tell(new WorkspaceSuperviserActor.SuperviseDiActor(actor, name), ActorRefs.NoSender);
        }

        public void WatchIntrest(WatchIntrest intrest)
            => Superviser.Tell(intrest);
    }
}