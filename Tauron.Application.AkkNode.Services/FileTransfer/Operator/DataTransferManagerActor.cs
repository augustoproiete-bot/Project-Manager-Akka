using System;
using JetBrains.Annotations;
using Tauron.Akka;
using Akka.Actor;
using Tauron.Application.Master.Commands.Core;

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
                            })
               .AndReceive();

            this.Flow<TransferMessages.DataTranfer>()
               .From.Action(tm => Context.Child(tm.OperationId).Tell(tm))
               .AndReceive();

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
                            })
               .AndReceive();

            this.Flow<IncomingDataTransfer>()
               .From.Action(dt => subscribe.Send(dt))
               .AndReceive();

            this.Flow<TransferMessages.TransferCompled>()
               .From.Action(tc =>
                            {
                                Context.Stop(Context.Child(tc.OperationId));
                                subscribe.Send(tc, tc.GetType());
                            })
               .AndReceive();

            this.Flow<TransferMessages.TransferMessage>()
               .From.Action(tm =>
                            {
                                Context.Child(tm.OperationId).Tell(tm);
                                subscribe.Send(tm, tm.GetType());
                            })
               .AndReceive();

            subscribe.MakeReceive();
        }
    }
}