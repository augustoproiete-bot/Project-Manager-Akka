using System;
using System.IO;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.AkkNode.Services.FileTransfer
{
    [PublicAPI]
    public abstract class DataTransferRequest : TransferMessages.TransferMessage
    {
        public abstract Func<ITransferData> Source { get; }

        public IActorRef Target { get; }

        public string? Data { get; }

        protected DataTransferRequest(string operationId, IActorRef target, string? data) 
            : base(operationId)
        {
            Target = target;
            Data = data;
        }

        public static DataTransferRequest FromStream(Func<ITransferData> stream, IActorRef target, string? data = null)
            => new StreamTransferRequest(Guid.NewGuid().ToString(), stream, target, data);

        public static DataTransferRequest FromFile(string file, IActorRef target, string? data = null)
            => new DataStreamTranferRequest(Guid.NewGuid().ToString(), file, target, data);

        public static DataTransferRequest FromStream(Stream stream, IActorRef target, string? data = null)
            => new StreamTransferRequest(Guid.NewGuid().ToString(), stream, target, data);

        public static DataTransferRequest FromStream(Func<Stream> stream, IActorRef target, string? data = null)
            => new StreamTransferRequest(Guid.NewGuid().ToString(), stream, target, data);

        public static DataTransferRequest FromFile(string opsId, string file, IActorRef target, string? data = null)
            => new DataStreamTranferRequest(opsId, file, target, data);

        public static DataTransferRequest FromStream(string opsId, Stream stream, IActorRef target, string? data = null)
            => new StreamTransferRequest(opsId, stream, target, data);

        public static DataTransferRequest FromStream(string opsId, Func<Stream> stream, IActorRef target, string? data = null)
            => new StreamTransferRequest(opsId, stream, target, data);

        public sealed class StreamTransferRequest : DataTransferRequest
        {
            public StreamTransferRequest(string operationId, Func<Stream> source, IActorRef target, string? data)
                : base(operationId, target, data) =>
                Source = () => new StreamData(source());

            public StreamTransferRequest(string operationId, Func<ITransferData> source, IActorRef target, string? data)
                : base(operationId, target, data) =>
                Source = source;

            public StreamTransferRequest(string operationId, Stream source, IActorRef target, string? data)
                : this(operationId, () => source, target, data)
            {
            }

            public override Func<ITransferData> Source { get; }

            protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest) => throw new NotSupportedException();

            protected override void WriteInternal(ActorBinaryWriter writer) => throw new NotSupportedException();
        }

        public sealed class DataStreamTranferRequest : DataTransferRequest
        {
            public DataStreamTranferRequest(string operationId, string file, IActorRef target, string? data) 
                : base(operationId, target, data)
            {
                Source = () => new StreamData(File.OpenRead(file));
            }

            public override Func<ITransferData> Source { get; }

            protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest) => throw new NotSupportedException();

            protected override void WriteInternal(ActorBinaryWriter writer) => throw new NotSupportedException();
        }
    }
}