using System;
using Akka.Actor;
using Tauron.Akka;

namespace Tauron.Application.Workshop.Core
{
    public sealed class WorkspaceSuperviserActor : ExposedReceiveActor
    {
        public WorkspaceSuperviserActor()
        {
            Receive<SuperviseActor>(CreateActor);
        }

        private void CreateActor(SuperviseActor obj)
        {
            try
            {
                Sender.Tell(new NewActor(Context.ActorOf(obj.Props, obj.Name)));
            }
            catch (Exception e)
            {
                Log.Error(e, "Error on Create an new Actor {TypeName}", obj.Props.TypeName);
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

        internal sealed class SuperviseActor
        {
            public Props Props { get; }

            public string Name { get; }

            public SuperviseActor(Props props, string name)
            {
                Props = props;
                Name = name;
            }
        }

        internal sealed class NewActor
        {
            public IActorRef ActorRef { get; }

            public NewActor(IActorRef actorRef) => ActorRef = actorRef;
        }
    }
}