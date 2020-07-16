using System;
using System.Buffers;
using System.IO;
using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;
using static Tauron.Application.AkkNode.Services.FileTransfer.TransferMessages;

namespace Tauron.Application.AkkNode.Services.FileTransfer.Operator
{
    public enum OperatorState
    {
        Waiting,
        InitSending,
        InitReciving,
        Sending,
        Reciving,
        Failed,
        Compled
    }

    public sealed class OperatorData
    {
        public static readonly Crc32 Crc32 = new Crc32();

        public string OperationId { get; }

        public IActorRef TargetManager { get; }

        private Func<Stream> Data { get;  }

        private string? Metadata { get; }

        public CrcStream TransferStrem { get; }

        public TransferError? Error { get; }

        private OperatorData(string operationId, IActorRef targetManager, Func<Stream> data, string? metadata, Stream transferStrem, TransferError? error)
        {
            OperationId = operationId;
            TargetManager = targetManager;
            Data = data;
            Metadata = metadata;
            Error = error;

            if (transferStrem is CrcStream stream)
                TransferStrem = stream;
            else
                TransferStrem = new CrcStream(transferStrem);
        }

        public OperatorData()
            : this(string.Empty, ActorRefs.Nobody, () => Stream.Null, null, Stream.Null, null)
        {
            
        }

        private OperatorData Copy(string? id = null, IActorRef? target = null, Func<Stream>? data = null, string? metadata = null, Stream? stream = null, TransferError? fieled = null) 
            => new OperatorData(id ?? OperationId, target ?? TargetManager, data ?? Data, metadata ?? Metadata, stream ?? TransferStrem, fieled ?? Error);

        public OperatorData StartSending(DataTransferRequest id)
            => Copy(id.OperationId, id.Target, id.Source, id.Data);

        public OperatorData StartRecdiving(TransmitRequest id)
            => Copy(id.OperationId, id.From, metadata: id.Data);

        public OperatorData SetData(Func<Stream> data)
            => Copy(data: data);

        public OperatorData Open() => Copy(stream: Data());

        public OperatorData Failed(IActorRef parent, FailReason reason, string? errorData)
        {
            var failed = new TransferError(OperationId, reason, errorData);
            TargetManager.Tell(failed, parent);
            parent.Tell(failed.ToFailed());

            return Copy(fieled: failed);
        }

        public OperatorData InComingError(TransferError error)
            => Copy(fieled: error);
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class TransferOperatorActor : FSM<OperatorState, OperatorData>
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        private byte[]? _outgoningBytes;
        private int _sendingAttempts;
        private NextChunk? _lastChunk;

        private static IActorRef Parent => Context.Parent;

        public TransferOperatorActor()
        {
            StartWith(OperatorState.Waiting, new OperatorData());

            When(OperatorState.Waiting,
                state =>
                {
                    switch (state.FsmEvent)
                    {
                        case TransmitRequest transmit:
                            _log.Info("Incoming Recieve Request  {id} -- {Data}", GetId(state), transmit.Data);
                            Parent.Tell(new IncomingDataTransfer(transmit.OperationId, Parent, transmit.Data));
                            return GoTo(OperatorState.InitReciving).Using(state.StateData.StartRecdiving(transmit));
                        case DataTransferRequest request:
                            _log.Info("Incoming Trensfer Request {id} -- {Data}", GetId(state), request.Data);
                            request.Target.Tell(new TransmitRequest(request.OperationId, Parent, request.Data), Parent);
                            return GoTo(OperatorState.InitSending).Using(state.StateData.StartSending(request));
                        default:
                            return null;
                    }
                });

            When(OperatorState.InitSending,
                state =>
                {
                    switch (state.FsmEvent)
                    {
                        case BeginTransfering _:
                            _log.Info("Start Tranfer {Id}", GetId(state));
                            try
                            {
                                var newState = GoTo(OperatorState.Sending).Using(state.StateData.Open());
                                Self.Tell(new StartTrensfering(state.StateData.OperationId));
                                return newState;
                            }
                            catch (Exception e)
                            {
                                _log.Error(e, "Open Sending Stream Failed {Id}", GetId(state));
                                return GoTo(OperatorState.Failed).Using(state.StateData.Failed(Parent, FailReason.StreamError, e.Message));
                            }
                        case RequestDeny _:
                            _log.Info("Tranfer Request Deny {Id}", state.StateData.OperationId);
                            return GoTo(OperatorState.Failed).Using(state.StateData.Failed(Parent, FailReason.Deny, null));
                        default:
                            return null;
                    }
                }, TimeSpan.FromSeconds(2));

            When(OperatorState.InitReciving,
                state =>
                {
                    switch (state.FsmEvent)
                    {
                        case RequestDeny deny:
                            _log.Info("Tranfer Request Deny {Id}", state.StateData.OperationId);
                            state.StateData.TargetManager.Tell(deny, Parent);
                            return GoTo(OperatorState.Failed);
                        case RequestAccept accept:
                            _log.Info("Request Accepted {Id}", GetId(state));
                            try
                            {
                                var newState = GoTo(OperatorState.Reciving).Using(state.StateData.SetData(accept.Target).Open());
                                state.StateData.TargetManager.Tell(new BeginTransfering(state.StateData.OperationId));
                                return newState;
                            }
                            catch (Exception e)
                            {
                                _log.Error(e, "Open Reciving Stream Failed {Id}", GetId(state));
                                return GoTo(OperatorState.Failed).Using(state.StateData.Failed(Parent, FailReason.StreamError, e.Message));
                            }
                        default:
                            return null;
                    }
                }, TimeSpan.FromSeconds(2));

            When(OperatorState.Sending,
                state =>
                {
                    switch (state.FsmEvent)
                    {
                        case SendNextChunk _:
                        case StartTrensfering _:
                            _outgoningBytes ??= ArrayPool<byte>.Shared.Rent(1024 * 1024);
                            try
                            {
                                _sendingAttempts = 0;
                                var count = state.StateData.TransferStrem.Read(_outgoningBytes, 0, _outgoningBytes.Length);
                                var last = count < _outgoningBytes.Length;
                                var crc = OperatorData.Crc32.ComputeChecksum(_outgoningBytes);

                                state.StateData.TargetManager.Tell(
                                    _lastChunk = new NextChunk(state.StateData.OperationId, _outgoningBytes, count, last, crc, state.StateData.TransferStrem.ReadCrc), 
                                    Parent);

                                return Stay();
                            }
                            catch(Exception e)
                            {
                                _log.Error(e, "Error on Read Stream or Sending");
                                return GoTo(OperatorState.Failed).Using(state.StateData.Failed(Parent, FailReason.ReadError, e.Message));
                            }
                        case SendingCompled _: 
                            state.StateData.TransferStrem.Dispose();
                            ArrayPool<byte>.Shared.Return(_outgoningBytes);
                            _outgoningBytes = null;
                            Parent.Tell(new TransferCompled(state.StateData.OperationId));
                            return GoTo(OperatorState.Compled);
                        case RepeadChunk _:
                            _sendingAttempts += 1;
                            if (_sendingAttempts > 5)
                                return GoTo(OperatorState.Failed).Using(state.StateData.Failed(Parent, FailReason.ToManyResends, null));

                            state.StateData.TargetManager.Tell(_lastChunk, Parent);
                            return Stay();
                        default:
                            return null;
                    }
                }, TimeSpan.FromSeconds(10));

            When(OperatorState.Reciving,
                state =>
                {
                    switch (state.FsmEvent)
                    {
                        case NextChunk chunk:
                            try
                            {
                                var reciveCrc = OperatorData.Crc32.ComputeChecksum(chunk.Data);
                                if (reciveCrc != chunk.Crc)
                                    state.StateData.TargetManager.Tell(new RepeadChunk(state.StateData.OperationId), Parent);
                                else
                                {
                                    if(chunk.Count > 0)
                                        state.StateData.TransferStrem.Write(chunk.Data, 0, chunk.Count);

                                    if (chunk.Finish)
                                    {
                                        var data = state.StateData;
                                        try
                                        {
                                            if (data.TransferStrem.WriteCrc != chunk.FinishCrc)
                                                return GoTo(OperatorState.Failed).Using(state.StateData.Failed(Parent, FailReason.ComunicationError, null));

                                            data.TargetManager.Tell(new SendingCompled(state.StateData.OperationId));
                                            Parent.Tell(new TransferCompled(state.StateData.OperationId));

                                            return GoTo(OperatorState.Compled);
                                        }
                                        finally
                                        {
                                            data.TransferStrem.Dispose();
                                        }
                                    }

                                    state.StateData.TargetManager.Tell(new SendNextChunk(state.StateData.OperationId));
                                }

                                return Stay();
                            }
                            catch (Exception e)
                            {
                                _log.Error(e, "Error on Write Stream");
                                return GoTo(OperatorState.Failed).Using(state.StateData.Failed(Parent, FailReason.WriteError, e.Message));
                            }
                        case RequestDeny _:
                            return Stay();
                        default:
                            return null;
                    }
                }, TimeSpan.FromSeconds(10));

            When(OperatorState.Failed, state =>
                                       {
                                           _log.Warning("Transfer Failed {Id}", GetId(state));

                                           if(state.FsmEvent is TransferError error)
                                               Parent.Tell(error.ToFailed());
                                           else
                                           {
                                               var manmesg = state.StateData.Error ??
                                                   new TransferError((state.FsmEvent as TransferMessage)?.OperationId ?? state.StateData.OperationId, FailReason.CorruptState, null);

                                               state.StateData.TargetManager.Tell(manmesg, Parent);
                                               Parent.Tell(manmesg.ToFailed());
                                           }

                                           return Stay();
                                       });

            When(OperatorState.Compled, s => null);

            OnTransition((state, nextState) =>
                         {
                             if (nextState == OperatorState.Failed)
                             {
                                 NextStateData.TransferStrem.Dispose();
                                 if(_outgoningBytes != null)
                                     ArrayPool<byte>.Shared.Return(_outgoningBytes);
                             }
                         });

            WhenUnhandled(
                state =>
                {
                    switch (state.FsmEvent)
                    {
                        case StateTimeout _:
                            _log.Error("Trisnmission Timeout {Id}", GetId(state));
                            return GoTo(OperatorState.Failed).Using(state.StateData.Failed(Parent, FailReason.Timeout, null));
                        case TransferError error:
                            _log.Warning("Incoming Transfer Failed {Id}", GetId(state));
                            return GoTo(OperatorState.Failed).Using(state.StateData.InComingError(error));
                        case DataTranfer _:
                            _log.Warning("Incorrect DataTransfer Event {Id}", GetId(state));
                            return GoTo(OperatorState.Failed).Using(state.StateData.Failed(Parent, FailReason.ComunicationError, null));
                        default:
                            _log.Warning("Unkown or Incorrect message Incming {Type}", state.FsmEvent.GetType());
                            return Stay();
                    }
                });

            Initialize();
        }
        
        private string GetId(Event<OperatorData> message)
        {
            var id = (message.FsmEvent as TransferMessage)?.OperationId ?? message.StateData.OperationId;
            if (string.IsNullOrWhiteSpace(id))
                id = "Unkowen";

            return id;
        }
    }
}