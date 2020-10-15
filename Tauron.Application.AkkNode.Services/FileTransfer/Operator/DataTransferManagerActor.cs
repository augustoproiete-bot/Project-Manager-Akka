using JetBrains.Annotations;
using Tauron.Akka;
using Akka.Actor;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.AkkNode.Services.FileTransfer.Operator
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class DataTransferManagerActor : ExposedReceiveActor
    {
        public DataTransferManagerActor()
        {
            var subscribe = new SubscribeAbility(this);

            Flow<TransferMessages.TransmitRequest>(this)
               .From.Action(r =>
                            {
                                var op = Context.Child(r.OperationId);
                                if (!op.Equals(ActorRefs.Nobody))
                                {
                                    r.From.Tell(new TransferFailed(r.OperationId, FailReason.DuplicateOperationId, null));
                                    return;
                                }

                                Context.ActorOf(Props.Create<TransferOperatorActor>(), r.OperationId).Tell(r);
                            });

            Flow<TransferMessages.DataTranfer>(this)
               .From.Action(tm => Context.Child(tm.OperationId).Tell(tm));

            Flow<DataTransferRequest>(this)
               .From.Action(r =>
                            {
                                var op = Context.Child(r.OperationId);
                                if (!op.Equals(ActorRefs.Nobody))
                                {
                                    r.Target.Tell(new TransferFailed(r.OperationId, FailReason.DuplicateOperationId, null));
                                    Self.Tell(new TransferFailed(r.OperationId, FailReason.DuplicateOperationId, null));
                                    return;
                                }

                                Context.ActorOf(Props.Create<TransferOperatorActor>(), r.OperationId).Tell(r);
                            });

            Flow<IncomingDataTransfer>(this)
               .From.Action(dt => subscribe.Send(dt));

            Flow<TransferMessages.TransferCompled>(this)
               .From.Action(tc =>
                            {
                                Context.Stop(Context.Child(tc.OperationId));
                                subscribe.Send(tc, tc.GetType());
                            });

            Flow<TransferMessages.TransferMessage>(this)
               .From.Action(tm =>
                            {
                                Context.Child(tm.OperationId).Tell(tm);
                                subscribe.Send(tm, tm.GetType());
                            });

            subscribe.MakeReceive();
        }

        protected override SupervisorStrategy SupervisorStrategy() => new OneForOneStrategy(e => Directive.Stop);
    }
}