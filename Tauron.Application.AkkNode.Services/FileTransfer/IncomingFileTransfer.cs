using System;
using System.IO;
using System.Threading;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Application.AkkNode.Services.FileTransfer
{
    [PublicAPI]
    public sealed class IncomingFileTransfer : TransferMessages.TransferMessage
    {
        private readonly Timer _denyTimer;

        public IActorRef Manager { get; }

        public string Name { get; }

        public string? Data { get; }

        public IncomingFileTransfer(string operationId, IActorRef manager, string name, string? data) 
            : base(operationId)
        {
            Manager = manager;
            Name = name;
            Data = data;

            _denyTimer = new Timer(s => Deny(), null, TimeSpan.FromMinutes(1), Timeout.InfiniteTimeSpan);
        }

        public void Deny()
        {
            _denyTimer.Dispose();
            Manager.Tell(new TransferMessages.RequestDeny(OperationId));
        }

        public void Accept(Func<Stream> to)
        {
            _denyTimer.Dispose();
            Manager.Tell(new TransferMessages.RequestAccept(OperationId, to));
        }
    }
}