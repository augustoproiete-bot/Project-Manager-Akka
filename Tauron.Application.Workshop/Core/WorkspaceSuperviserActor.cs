using System;
using System.Collections.Immutable;
using Akka.Actor;
using Akka.DI.Core;
using Tauron.Akka;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.Core
{
    public sealed class WorkspaceSuperviserActor : ExposedReceiveActor
    {
        private ImmutableDictionary<IActorRef, Action> _intrest = ImmutableDictionary<IActorRef, Action>.Empty;

        public WorkspaceSuperviserActor()
        {
            Receive<SuperviseActorBase>(CreateActor);

            Receive<WatchIntrest>(wi =>
            {
                ImmutableInterlocked.AddOrUpdate(ref _intrest, wi.Target, _ => wi.OnRemove, (_, action) => action.Combine(wi.OnRemove) ?? wi.OnRemove);
                Context.Watch(wi.Target);
            });
            Receive<Terminated>(t =>
            {
                if (!_intrest.TryGetValue(t.ActorRef, out var action)) return;

                action();
                _intrest = _intrest.Remove(t.ActorRef);
            });
        }

        private void CreateActor(SuperviseActorBase obj)
        {
            Props? props = null;
            
            try
            {
                props = obj.Props(Context);
                var newActor = Context.ActorOf(props, obj.Name);

                if(Sender.IsNobody()) return;
                Sender.Tell(new NewActor(newActor));
            }
            catch (Exception e)
            {
                Log.Error(e, "Error on Create an new Actor {TypeName}", props?.TypeName ?? "Unkowen");

                if(Sender.IsNobody()) return;
                Sender.Tell(new NewActor(ActorRefs.Nobody));
            }
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                Decider.From(Directive.Resume, 
                    Directive.Stop.When<ActorInitializationException>(),
                    Directive.Stop.When<ActorKilledException>(), 
                    Directive.Stop.When<DeathPactException>()));
        }

        internal abstract class SuperviseActorBase
        {
            public abstract Func<IUntypedActorContext, Props> Props { get; }

            public string Name { get; }

            protected SuperviseActorBase(string name) => Name = name;
        }

        internal sealed class SupervisePropsActor : SuperviseActorBase
        {
            public SupervisePropsActor(Props props, string name)
                : base(name)
            {
                Props = _ => props;
            }

            public override Func<IUntypedActorContext, Props> Props { get; }
        }

        internal sealed class SuperviseDiActor : SuperviseActorBase
        {
            private readonly Type _actorType;

            public SuperviseDiActor(Type actorType, string name) : base(name) => _actorType = actorType;

            public override Func<IUntypedActorContext, Props> Props => c => c.System.DI().Props(_actorType);
        }

        internal sealed class NewActor
        {
            public IActorRef ActorRef { get; }

            public NewActor(IActorRef actorRef) => ActorRef = actorRef;
        }
    }
}