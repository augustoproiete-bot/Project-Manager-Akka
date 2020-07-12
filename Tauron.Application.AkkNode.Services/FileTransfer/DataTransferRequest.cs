using System;
using System.IO;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Application.AkkNode.Services.FileTransfer
{
    [PublicAPI]
    public abstract class DataTransferRequest : TransferMessages.TransferMessage
    {
        public abstract Func<Stream> Source { get; }

        public IActorRef Target { get; }

        public string? Data { get; }

        protected DataTransferRequest(string operationId, IActorRef target, string? data) 
            : base(operationId)
        {
            Target = target;
            Data = data;
        }

        public static DataTransferRequest FromFile(string file, IActorRef target, string? data = null)
            => new DataStreamTranferRequest(Guid.NewGuid().ToString(), file, target, data);

        public static DataTransferRequest FromStream(Stream stream, IActorRef target, string? data = null)
            => new StreamTransferRequest(Guid.NewGuid().ToString(), stream, target, data);

        public static DataTransferRequest FromStream(Func<Stream> stream, IActorRef target, string? data = null)
            => new StreamTransferRequest(Guid.NewGuid().ToString(), stream, target, data);

        public sealed class StreamTransferRequest : DataTransferRequest
        {
            public StreamTransferRequest(string operationId, Func<Stream> source, IActorRef target, string? data) 
                : base(operationId, target, data) =>
                Source = source;

            public StreamTransferRequest(string operationId, Stream source, IActorRef target, string? data)
                : this(operationId, () => source, target, data)
            {
            }

            public override Func<Stream> Source { get; }
        }

        public sealed class DataStreamTranferRequest : DataTransferRequest
        {
            public DataStreamTranferRequest(string operationId, string file, IActorRef target, string? data) 
                : base(operationId, target, data)
            {
                Source = () => File.OpenRead(file);
            }

            public override Func<Stream> Source { get; }
        }
    }
}