using System;
using System.Collections.Generic;
using System.Globalization;
using Akka.Actor;
using Tauron.Localization.Provider;

namespace Tauron.Localization.Actor
{
    public sealed class LocCoordinator : ReceiveActor, IWithTimers
    {
        private readonly Dictionary<string, Request> _requests = new ();

        public ITimerScheduler Timers { get; set; } = null!;

        public LocCoordinator(IEnumerable<ILocStoreProducer> producers)
        {
            producers.Foreach(sp => Context.ActorOf(sp.GetProps(), sp.Name));
            Receive<RequestLocValue>(RequestLocValueHandler);
            Receive<LocStoreActorBase.QueryResponse>(QueryResponseHandler);
            Receive<SendInvalidate>(Invalidate);
        }

        private void QueryResponseHandler(LocStoreActorBase.QueryResponse obj)
        {
            if (!_requests.Remove(obj.Id, out var request)) return;

            request.Sender.Tell(new ResponseLocValue(obj.Value, request.Key));
        }

        private void RequestLocValueHandler(RequestLocValue msg)
        {
            var request = new Request(Context.Sender, msg.Key);
            var opId = Guid.NewGuid().ToString();

            _requests[opId] = request;

            foreach (var actorRef in Context.GetChildren())
                actorRef.Tell(new LocStoreActorBase.QueryRequest(request.Key, opId, msg.Lang));

            Timers.StartSingleTimer(Guid.NewGuid(), new SendInvalidate(opId), TimeSpan.FromSeconds(10));
        }

        private void Invalidate(SendInvalidate op)
        {
            if (!_requests.Remove(op.OpId, out var request)) return;

            request.Sender.Tell(new ResponseLocValue(null, request.Key));
        }

        private sealed record SendInvalidate(string OpId);

        public sealed record RequestLocValue(string Key, CultureInfo Lang);

        public sealed record ResponseLocValue(object? Result, string Key);

        private sealed record Request(IActorRef Sender, string Key);
    }
}