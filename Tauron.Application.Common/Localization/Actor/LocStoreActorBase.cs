using System.Globalization;
using Akka.Actor;

namespace Tauron.Localization.Actor
{
    public abstract class LocStoreActorBase : UntypedActor
    {
        protected sealed override void OnReceive(object message)
        {
            if (message is QueryRequest query)
                Context.Sender.Tell(new QueryResponse(TryQuery(query.Key, query.CultureInfo), query.Id));
            else
                base.Unhandled(message);
        }

        protected abstract object? TryQuery(string name, CultureInfo target);

        public sealed class QueryRequest
        {
            public QueryRequest(string key, string id, CultureInfo cultureInfo)
            {
                Key = key;
                Id = id;
                CultureInfo = cultureInfo;
            }

            public string Key { get; }

            public string Id { get; }

            public CultureInfo CultureInfo { get; }
        }

        public sealed class QueryResponse
        {
            public QueryResponse(object? value, string id)
            {
                Value = value;
                Id = id;
            }

            public object? Value { get; }

            public string Id { get; }
        }
    }
}