using System;
using System.IO;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.AkkNode.Services.FileTransfer
{
    public static class TransferMessages
    {
        public abstract class TransferMessage : IInternalSerializable
        {
            public string OperationId { get; }

            protected TransferMessage(string operationId) => OperationId = operationId;

            protected TransferMessage(BinaryReader reader, int version)
            {
                var manifest = BinaryManifest.Read(reader, GetType().Name, version);
                OperationId = reader.ReadString();

                // ReSharper disable once VirtualMemberCallInConstructor
                ReadInternal(reader, manifest);
            }

            protected abstract void ReadInternal(BinaryReader reader, BinaryManifest manifest);

            public void Write(BinaryWriter writer)
            {
                writer.Write(OperationId);
                WriteInternal(writer);
            }

            protected abstract void WriteInternal(BinaryWriter writer);
        }

        public abstract class TransferCompled : TransferMessage
        {
            protected TransferCompled(string operationId) : base(operationId)
            {
            }

            protected TransferCompled(BinaryReader reader, int version) : base(reader, version)
            {
            }
        }

        public abstract class DataTranfer : TransferMessage
        {
            protected DataTranfer(string operationId) : base(operationId)
            {
            }

            protected DataTranfer(BinaryReader reader, int version) : base(reader, version)
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
                : base(reader, 1) { }


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

            protected override void WriteInternal(BinaryWriter writer)
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
                : base(reader, 1)
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

            protected override void WriteInternal(BinaryWriter writer)
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
            public SendNextChunk([NotNull] string operationId) : base(operationId)
            { }
        }

        public sealed class SendingCompled : DataTranfer
        {
            public SendingCompled(string operationId) 
                : base(operationId)
            {
            }
        }

        public sealed class RepeadChunk : DataTranfer
        {
            public RepeadChunk(string operationId) 
                : base(operationId)
            {
            }
        }

        public sealed class StartTrensfering : DataTranfer
        {
            public StartTrensfering(string operationId) 
                : base(operationId)
            {
            }
        }

        public sealed class BeginTransfering : DataTranfer
        {
            public BeginTransfering(string operationId) 
                : base(operationId)
            {
            }
        }

        public sealed class TransmitRequest : DataTranfer
        {
            public IActorRef From { get; }

            public string? Data { get; }

            public TransmitRequest([NotNull] string operationId, IActorRef @from, string? data) : base(operationId)
            {
                From = @from;
                Data = data;
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
        }

        public sealed class RequestDeny : DataTranfer
        {
            public RequestDeny(string operationId) : base(operationId)
            {
            }
        }
    }
}