using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Application.AkkNode.Services.FileTransfer
{
    [PublicAPI]
    public sealed class IncomingDataTransfer : TransferMessages.TransferMessage
    {
        private readonly Timer _denyTimer;

        public event Action? DenyEvent;

        public DataTransferManager Manager { get; }
        
        public string? Data { get; }

        public IncomingDataTransfer(string operationId, DataTransferManager manager, string? data) 
            : base(operationId)
        {
            Manager = manager;
            Data = data;

            _denyTimer = new Timer(s => Deny(), null, TimeSpan.FromMinutes(1), Timeout.InfiniteTimeSpan);
        }

        public void Deny()
        {
            DenyEvent?.Invoke();
            _denyTimer.Dispose();
            Manager.Actor.Tell(new TransferMessages.RequestDeny(OperationId));
        }

        public Task<TransferMessages.TransferCompled> Accept(Func<Stream> to) => Accept(() => new StreamData(to()));

        public Task<TransferMessages.TransferCompled> Accept(Func<ITransferData> to)
        {
            _denyTimer.Dispose();
            var source = new TaskCompletionSource<TransferMessages.TransferCompled>();
            Manager.Actor.Tell(new TransferMessages.RequestAccept(OperationId, to, source));
            return source.Task;
        }
    }
}