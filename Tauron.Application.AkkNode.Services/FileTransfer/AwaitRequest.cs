using System;
using Akka.Actor;
using Akka.Util;

namespace Tauron.Application.AkkNode.Services.FileTransfer
{
    public sealed class AwaitResponse
    {
        private readonly Option<IncomingDataTransfer> _request;

        public AwaitResponse(IncomingDataTransfer? request)
        {
            if(request != null)
                _request = new Option<IncomingDataTransfer>(request);
        }

        public Option<IncomingDataTransfer> Request => _request;
    }

    public sealed class AwaitRequest
    {
        public TimeSpan Timeout { get; }

        public string Id { get; }

        public IActorRef ActorRef { get; }

        public AwaitRequest(TimeSpan timeout, string id, IActorRef actorRef)
        {
            Timeout = timeout;
            Id = id;
            ActorRef = actorRef;
        }
    }
}