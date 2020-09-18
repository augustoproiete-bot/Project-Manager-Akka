using System;
using System.IO;
using Akka.Actor;
using Akka.Serialization;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.AkkNode.Services.FileTransfer
{
    public static class TransferMessages
    {
        [PublicAPI]
        public abstract class TransferMessage : IInternalSerializable
        {
            protected virtual int Version => 1;

            public string OperationId { get; }

            protected TransferMessage(string operationId)
            {
                OperationId = operationId;
            }

            protected TransferMessage(BinaryReader reader)
            {
                // ReSharper disable once VirtualMemberCallInConstructor
                var manifest = BinaryManifest.Read(reader, GetType().Name, Version);
                OperationId = reader.ReadString();

                // ReSharper disable once VirtualMemberCallInConstructor
                ReadInternal(reader, manifest);
            }

            protected virtual void ReadInternal(BinaryReader reader, BinaryManifest manifest){}

            public void Write(ActorBinaryWriter writer)
            {
                BinaryManifest.Write(writer, GetType().Name, Version);
                writer.Write(OperationId);
                WriteInternal(writer);
            }

            protected virtual void WriteInternal(ActorBinaryWriter writer){}
        }

        public abstract class TransferCompled : TransferMessage
        {
            public string? Data { get; private set; }

            protected TransferCompled(string operationId, string? data) : base(operationId) => Data = data;

            protected TransferCompled(BinaryReader reader) : base(reader)
            {
            }

            protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest)
            {
                Data = BinaryHelper.ReadNull(reader, r => r.ReadString());
                base.ReadInternal(reader, manifest);
            }

            protected override void WriteInternal(ActorBinaryWriter writer)
            {
                BinaryHelper.WriteNull(Data, writer, writer.Write);
                base.WriteInternal(writer);
            }
        }

        public abstract class DataTranfer : TransferMessage
        {
            protected DataTranfer(string operationId) : base(operationId)
            {
            }

            protected DataTranfer(BinaryReader reader) : base(reader)
            {
            }
        }

        public sealed class TransferError : DataTranfer
        {
            public FailReason FailReason { get; private set; } = FailReason.Unkowen;

            public string? Data { get; private set; } = string.Empty;

            public TransferError(string operationId, FailReason failReason, string? data) 
                : base(operationId)
            {
                FailReason = failReason;
                Data = data;
            }
            
            [UsedImplicitly]
            public TransferError(BinaryReader reader)
                : base(reader) { }


            public TransferFailed ToFailed()
                => new TransferFailed(OperationId, FailReason, Data);

            protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest)
            {
                if (manifest.WhenVersion(1))
                {
                    FailReason = (FailReason) reader.ReadInt32();
                    if (reader.ReadBoolean())
                        Data = reader.ReadString();
                }
            }

            protected override void WriteInternal(ActorBinaryWriter writer)
            {
                writer.Write((int)FailReason);
                if (Data != null)
                {
                    writer.Write(true);
                    writer.Write(Data);
                }
                else
                    writer.Write(false);
            }
        }

        public sealed class NextChunk : DataTranfer
        {
            public byte[] Data { get; private set; } = Array.Empty<byte>();

            public int Count { get; private set; }

            public bool Finish { get; private set; }

            public uint Crc { get; private set; }

            public uint FinishCrc { get; private set; }

            public NextChunk(string operationId, byte[] data, int count, bool finish, uint crc, uint finishCrc) : base(operationId)
            {
                Data = data;
                Count = count;
                Finish = finish;
                Crc = crc;
                FinishCrc = finishCrc;
            }

            [UsedImplicitly]
            public NextChunk(BinaryReader reader)
                : base(reader)
            {
                
            }

            protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest)
            {
                if (manifest.WhenVersion(1))
                {
                    Data = reader.ReadBytes(reader.ReadInt32());
                    Count = reader.ReadInt32();
                    Finish = reader.ReadBoolean();
                    Crc = reader.ReadUInt32();
                    FinishCrc = reader.ReadUInt32();
                }
            }

            protected override void WriteInternal(ActorBinaryWriter writer)
            {
                writer.Write(Data.Length);
                writer.Write(Data);
                writer.Write(Count);
                writer.Write(Finish);
                writer.Write(Crc);
                writer.Write(FinishCrc);
            }
        }

        public sealed class SendNextChunk : DataTranfer
        {
            public SendNextChunk(string operationId) : base(operationId)
            { }

            [UsedImplicitly]
            public SendNextChunk(BinaryReader reader)
                : base(reader)
            {
                
            }

        }

        public sealed class SendingCompled : DataTranfer
        {
            public SendingCompled(string operationId) 
                : base(operationId)
            {
            }

            [UsedImplicitly]
            public SendingCompled(BinaryReader reader)
                : base(reader)
            {

            }
        }

        public sealed class RepeadChunk : DataTranfer
        {
            public RepeadChunk(string operationId) 
                : base(operationId)
            {
            }

            [UsedImplicitly]
            public RepeadChunk(BinaryReader reader)
                : base(reader)
            {

            }
        }

        public sealed class StartTrensfering : DataTranfer
        {
            public StartTrensfering(string operationId) 
                : base(operationId)
            {
            }

            [UsedImplicitly]
            public StartTrensfering(BinaryReader reader)
                : base(reader)
            {

            }
        }

        public sealed class BeginTransfering : DataTranfer
        {
            public BeginTransfering(string operationId) 
                : base(operationId)
            {
            }

            [UsedImplicitly]
            public BeginTransfering(BinaryReader reader)
                : base(reader)
            {

            }
        }

        public sealed class TransmitRequest : DataTranfer
        {
            private ExtendedActorSystem? _system;

            public IActorRef From { get; private set; } = ActorRefs.Nobody;

            public string? Data { get; private set; }

            public TransmitRequest(string operationId, IActorRef from, string? data) : base(operationId)
            {
                From = from;
                Data = data;
            }

            [UsedImplicitly]
            public TransmitRequest(BinaryReader reader, ExtendedActorSystem system)
                : base(reader) => _system = system;

            protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest)
            {
                if(_system == null)
                    throw new InvalidOperationException();

                if (manifest.WhenVersion(1))
                {
                    From = _system.Provider.ResolveActorRef(reader.ReadString());
                    Data = BinaryHelper.ReadNull(reader, r => r.ReadString());
                }

                _system = null!;
            }

            protected override void WriteInternal(ActorBinaryWriter writer)
            {
                writer.Write(Serialization.SerializedActorPath(From));
                BinaryHelper.WriteNull(Data, writer, writer.Write);
            }
        }

        public sealed class RequestAccept : DataTranfer
        {
            public Func<Stream> Target { get; }

            public RequestAccept(string operationId, Func<Stream> target)
                : base(operationId)
            {
                Target = target;
            }

            protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest) => throw new NotSupportedException();

            protected override void WriteInternal(ActorBinaryWriter writer) => throw new NotSupportedException();
        }

        public sealed class RequestDeny : DataTranfer
        {
            public RequestDeny(string operationId) : base(operationId)
            {
            }
        }
    }
}