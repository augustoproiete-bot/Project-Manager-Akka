using System;
using System.IO;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Application.AkkNode.Services.FileTransfer
{
    [PublicAPI]
    public abstract class FileTransferRequest : TransferMessages.TransferMessage
    {
        public abstract Func<Stream> Source { get; }

        public IActorRef Target { get; }

        protected FileTransferRequest(string operationId, IActorRef target) 
            : base(operationId)
        {
            Target = target;
        }

        public static FileTransferRequest FromFile(string file, IActorRef target)
            => new FileStreamTranferRequest(Guid.NewGuid().ToString(), file, target);

        public static FileTransferRequest FromStream(Stream stream, IActorRef target)
            => new StreamTransferRequest(Guid.NewGuid().ToString(), stream, target);

        public static FileTransferRequest FromStream(Func<Stream> stream, IActorRef target)
            => new StreamTransferRequest(Guid.NewGuid().ToString(), stream, target);

        public sealed class StreamTransferRequest : FileTransferRequest
        {
            public StreamTransferRequest(string operationId, Func<Stream> source, IActorRef target) 
                : base(operationId, target) =>
                Source = source;

            public StreamTransferRequest(string operationId, Stream source, IActorRef target)
                : this(operationId, () => source, target)
            {
            }

            public override Func<Stream> Source { get; }
        }

        public sealed class FileStreamTranferRequest : FileTransferRequest
        {
            public FileStreamTranferRequest(string operationId, string file, IActorRef target) 
                : base(operationId, target)
            {
                Source = () => File.OpenRead(file);
            }

            public override Func<Stream> Source { get; }
        }
    }
}