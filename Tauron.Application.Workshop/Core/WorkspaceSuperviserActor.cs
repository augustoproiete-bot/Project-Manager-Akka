using System;
using System.Collections.Immutable;
using Akka.Actor;
using Akka.DI.Core;
using Akka.Util.Internal;
using Functional.Maybe;
using Tauron.Application.Workshop.Mutation;
using static Tauron.Prelude;

namespace Tauron.Application.Workshop.Core
{
    public sealed class WorkspaceSuperviserActor : StatefulReceiveActor<WorkspaceSuperviserActor.State>
    {
        public sealed record State(ImmutableDictionary<IActorRef, Action> Intrests);
        
        public WorkspaceSuperviserActor()
            : base(new State(ImmutableDictionary<IActorRef, Action>.Empty))
        {
            Receive<SuperviseActorBase>(CreateActor);

            Receive<WatchIntrest>((wi, s)
                                      => from state in s
                                         from _ in ContextWatch(wi.Target)
                                         let data = state.Intrests
                                         select state with{ Intrests = Update(data, wi.Target, wi.OnRemove)});
            Receive<Terminated>((t, s)
                                    => from state in s
                                       from remover in state.Intrests.Lookup(t.ActorRef)
                                       from _ in May(Prelude.Action(remover))
                                       select state with{Intrests = state.Intrests.Remove(t.ActorRef)});
        }

        private static ImmutableDictionary<IActorRef, Action> Update(ImmutableDictionary<IActorRef, Action> data, IActorRef actor, Action remover) 
            => data.ContainsKey(actor) ? data.SetItem(actor, data[actor].Combine(remover)) : data.Add(actor, remover);

        private static Maybe<Unit> ContextWatch(IActorRef actorRef)
        {
            Context.Watch(actorRef);
            return Unit.MayInstance;
        }
        
        private void CreateActor(SuperviseActorBase obj)
        {
            Maybe<Props> props = default;
            
            try
            {
                props = obj.Props(Context).Or(DeadLetters.DeadLetter);

                var actor =
                    from prop in props
                    select new NewActor(Context.ActorOf(prop, obj.Name));

                if(Sender.IsNobody()) return;
                Sender.MayTell(actor);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error on Create an new Actor {TypeName}", props.OrElseDefault()?.TypeName ?? "Unkowen");

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

         private sealed class DeadLetters : ActorBase
         {
             public static readonly Props DeadLetter = Props.Create<DeadLetters>();
        
             protected override bool Receive(object message)
             {
                 Unhandled(message);
                 return true;
             }
         }

        internal abstract class SuperviseActorBase
        {
            public abstract Func<IUntypedActorContext, Maybe<Props>> Props { get; }

            public string Name { get; }

            protected SuperviseActorBase(string name) => Name = name;
        }

        internal sealed class SupervisePropsActor : SuperviseActorBase
        {
            public SupervisePropsActor(Maybe<Props> props, string name)
                : base(name)
            {
                Props = _ => props;
            }

            public override Func<IUntypedActorContext, Maybe<Props>> Props { get; }
        }

        internal sealed class SuperviseDiActor : SuperviseActorBase
        {
            private readonly Type _actorType;

            public SuperviseDiActor(Type actorType, string name) : base(name) => _actorType = actorType;

            public override Func<IUntypedActorContext, Maybe<Props>> Props => c => c.System.DI().Props(_actorType).ToMaybe();
        }

        internal sealed record NewActor(IActorRef ActorRef);
    }
}