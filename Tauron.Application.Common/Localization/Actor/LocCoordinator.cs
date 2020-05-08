using System;
using System.Collections.Generic;
using System.Globalization;
using Akka.Actor;
using Tauron.Localization.Provider;

namespace Tauron.Localization.Actor
{
    public sealed class LocCoordinator : ReceiveActor
    {
        public sealed class RequestLocValue
        {
            public string key { get; }

            public CultureInfo Lang { get; }

            public RequestLocValue(string name, CultureInfo lang)
            {
                key = name;
                Lang = lang;
            }
        }

        public sealed class ResponseLocValue
        {
            public object? Result { get; }

            public string Key { get; set; }

            public ResponseLocValue(object? result, string key)
            {
                Result = result;
                Key = key;
            }
        }

        private sealed class Request
        {
            public IActorRef Sender { get; }

            public string Key { get; }

            public Request(IActorRef sender, string key)
            {
                Sender = sender;
                Key = key;
            }
        }

        private readonly Dictionary<string, Request> _requests = new Dictionary<string, Request>();

        public LocCoordinator(IEnumerable<ILocStoreProducer> producers)
        {
            producers.Foreach(sp => Context.ActorOf(sp.GetProps(), sp.Name));
            Receive<RequestLocValue>(RequestLocValueHandler);
            Receive<LocStoreActorBase.QueryResponse>(QueryResponseHandler);
            Receive<string>(Invalidate);
        }

        private void QueryResponseHandler(LocStoreActorBase.QueryResponse obj)
        {
            if(!_requests.Remove(obj.Id, out var request)) return;

            request.Sender.Tell(new ResponseLocValue(obj.Value, request.Key));
        }

        private void RequestLocValueHandler(RequestLocValue msg)
        {
            var request = new Request(Context.Sender, msg.key);
            var opId = Guid.NewGuid().ToString();

            _requests[opId] = request;

            foreach (var actorRef in Context.GetChildren()) 
                actorRef.Tell(new LocStoreActorBase.QueryRequest(request.Key, opId, msg.Lang));

            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(10), Context.Self, opId, Context.Sender);
        }

        private void Invalidate(string op)
        {
            if(!_requests.Remove(op, out var request)) return;

            request.Sender.Tell(new ResponseLocValue(null, request.Key));
        }
    }
}