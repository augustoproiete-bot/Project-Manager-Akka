using System.Globalization;
using Akka.Actor;
using Functional.Maybe;

namespace Tauron.Localization.Actor
{
    public abstract class LocStoreActorBase : ReceiveActor
    {
        protected LocStoreActorBase()
        {
            Receive<QueryRequest>(query => Context.Sender.Tell(new QueryResponse(TryQuery(query.Key, query.CultureInfo), query.Id)));
        }


        protected abstract Maybe<object> TryQuery(string name, Maybe<CultureInfo> target);

        public sealed record QueryRequest(string Key, string Id, Maybe<CultureInfo> CultureInfo);

        public sealed record QueryResponse(Maybe<object> Value, string Id);
    }
}