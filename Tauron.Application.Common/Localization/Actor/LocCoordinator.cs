using System;
using System.Collections.Generic;
using System.Globalization;
using Akka.Actor;
using Functional.Maybe;
using Tauron.Localization.Provider;

namespace Tauron.Localization.Actor
{
    public sealed class LocCoordinator : ReceiveActor, IWithTimers
    {
        private readonly Dictionary<string, Request> _requests = new ();

        public ITimerScheduler Timers { get; set; } = null!;

        public LocCoordinator(IEnumerable<ILocStoreProducer> producers)
        {
            foreach (var locStoreProducer in producers)
                Context.ActorOf(locStoreProducer.GetProps(), locStoreProducer.Name);

            Receive<RequestLocValue>(RequestLocValueHandler);
            Receive<LocStoreActorBase.QueryResponse>(QueryResponseHandler);
            Receive<SendInvalidate>(Invalidate);
        }

        private void QueryResponseHandler(LocStoreActorBase.QueryResponse obj)
        {
            var (result, id) = obj;

            var action = 
                from request in _requests.TryRemove(id)
                let originalSender = request.Sender
                let requerstKey = request.Key
                select new Action(() => originalSender.Tell(new ResponseLocValue(result, requerstKey)));

            action.Do(a => a());
        }

        private void RequestLocValueHandler(RequestLocValue msg)
        {
            var (key, cultureInfo) = msg;
            var request = new Request(Context.Sender, key);
            var opId = Guid.NewGuid().ToString();

            _requests[opId] = request;

            foreach (var actorRef in Context.GetChildren())
                actorRef.Tell(new LocStoreActorBase.QueryRequest(request.Key, opId, cultureInfo));

            Timers.StartSingleTimer(Guid.NewGuid(), new SendInvalidate(opId), TimeSpan.FromSeconds(10));
        }

        private void Invalidate(SendInvalidate op) 
            => QueryResponseHandler(new LocStoreActorBase.QueryResponse(Maybe<object>.Nothing, op.OpId));

        private sealed record SendInvalidate(string OpId);

        public sealed record RequestLocValue(string Key, Maybe<CultureInfo> Lang);

        public sealed record ResponseLocValue(Maybe<object> Result, string Key);

        private sealed record Request(IActorRef Sender, string Key);
    }
}