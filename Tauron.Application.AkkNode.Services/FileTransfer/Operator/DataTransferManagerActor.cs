using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Tauron.Akka;
using Akka.Actor;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.AkkNode.Services.FileTransfer.Operator
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class DataTransferManagerActor : ExposedReceiveActor, IWithTimers
    {
        private readonly Dictionary<string, IncomingDataTransfer> _pendingTransfers = new Dictionary<string, IncomingDataTransfer>();
        private readonly Dictionary<string, AwaitRequestInternal> _awaiters = new Dictionary<string, AwaitRequestInternal>();

        public ITimerScheduler Timers { get; set; } = null!;

        public DataTransferManagerActor()
        {
            var subscribe = new SubscribeAbility(this);

            Flow<TransferMessages.TransmitRequest>(b =>
                b.Action(r =>
                {
                    var op = Context.Child(r.OperationId);
                    if (!op.Equals(ActorRefs.Nobody))
                    {
                        r.From.Tell(new TransferFailed(r.OperationId, FailReason.DuplicateOperationId, null));
                        return;
                    }

                    Context.ActorOf(Props.Create<TransferOperatorActor>(), r.OperationId).Tell(r);
                }));

            Flow<TransferMessages.DataTranfer>(b => b.Action(tm =>
            {
                switch (tm)
                {
                    case TransferMessages.RequestAccept acc:
                        _pendingTransfers.Remove(acc.OperationId);
                        break;
                    case TransferMessages.RequestDeny den:
                        _pendingTransfers.Remove(den.OperationId);
                        break;
                    
                }
                Context.Child(tm.OperationId).Tell(tm);
            }));

            Flow<DataTransferRequest>(b =>
                b.Action(r =>
                {
                    var op = Context.Child(r.OperationId);
                    if (!op.Equals(ActorRefs.Nobody))
                    {
                        r.Target.Actor.Tell(new TransferFailed(r.OperationId, FailReason.DuplicateOperationId, null));
                        Self.Tell(new TransferFailed(r.OperationId, FailReason.DuplicateOperationId, null));
                        return;
                    }

                    Context.ActorOf(Props.Create<TransferOperatorActor>(), r.OperationId).Forward(r);
                }));

            Flow<IncomingDataTransfer>(b => b.Action(dt =>
            {
                if (_awaiters.TryGetValue(dt.OperationId, out var awaitRequest))
                {
                    awaitRequest.Target.Tell(new AwaitResponse(dt));
                    _awaiters.Remove(dt.OperationId);
                }
                else
                    _pendingTransfers[dt.OperationId] = dt;

                subscribe.Send(dt);
            }));

            Flow<TransferMessages.TransferCompled>(b =>
                b.Action(tc =>
                {
                    Context.Stop(Context.Child(tc.OperationId));
                    subscribe.Send(tc, tc.GetType());
                }));

            Flow<TransferMessages.TransferMessage>(b =>
                b.Action(tm =>
                {
                    Context.Child(tm.OperationId).Tell(tm);
                    subscribe.Send(tm, tm.GetType());
                }));

            Flow<AwaitRequest>(b => b.Action(r =>
            {
                if (_pendingTransfers.TryGetValue(r.Id, out var income))
                {
                    Sender.Tell(income);
                    _pendingTransfers.Remove(r.Id);
                }
                else
                {
                    _awaiters[r.Id] = new AwaitRequestInternal(Sender);
                    if (Timeout.InfiniteTimeSpan != r.Timeout)
                        Timers.StartSingleTimer(r.Id, new DeleteAwaiter(r.Id), r.Timeout);
                }
            }));

            Flow<DeleteAwaiter>(b => b.Action(d => _awaiters.Remove(d.Id)));

            subscribe.MakeReceive();
        }

        protected override SupervisorStrategy SupervisorStrategy() => new OneForOneStrategy(e => Directive.Stop);

        private class DeleteAwaiter
        {
            public string Id { get; }

            public DeleteAwaiter(string id) => Id = id;
        }

        private class AwaitRequestInternal
        {
            public IActorRef Target { get; }

            public AwaitRequestInternal(IActorRef target) => Target = target;
        }
    }
}