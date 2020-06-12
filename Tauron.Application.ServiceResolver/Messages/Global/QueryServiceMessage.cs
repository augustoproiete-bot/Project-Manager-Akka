using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Application.Akka.ServiceResolver.Messages.Global
{
    [PublicAPI]
    public class QueryServiceRequest
    {
        public QueryServiceRequest(string name) 
            => Name = name;

        public string Name { get; }

        public IActorRef Sender { get; private set; } = ActorRefs.Nobody;

        public QueryServiceRequest WithSender(IActorRef sender)
            => new QueryServiceRequest(Name) { Sender = sender};
    }
}