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

            this.Flow<TransferMessages.TransmitRequest>()
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

            this.Flow<TransferMessages.DataTranfer>()
               .From.Action(tm => Context.Child(tm.OperationId).Tell(tm));

            this.Flow<DataTransferRequest>()
               .From.Action(r =>
                            {
                                var op = Context.Child(r.OperationId);
                                if (!op.Equals(ActorRefs.Nobody))
                                {
                                    Self.Tell(new TransferFailed(r.OperationId, FailReason.DuplicateOperationId, null));
                                    return;
                                }

                                Context.ActorOf(Props.Create<TransferOperatorActor>(), r.OperationId).Tell(r);
                            });

            this.Flow<IncomingDataTransfer>()
               .From.Action(dt => subscribe.Send(dt));

            this.Flow<TransferMessages.TransferCompled>()
               .From.Action(tc =>
                            {
                                Context.Stop(Context.Child(tc.OperationId));
                                subscribe.Send(tc, tc.GetType());
                            });

            this.Flow<TransferMessages.TransferMessage>()
               .From.Action(tm =>
                            {
                                Context.Child(tm.OperationId).Tell(tm);
                                subscribe.Send(tm, tm.GetType());
                            });

            subscribe.MakeReceive();
        }
    }
}