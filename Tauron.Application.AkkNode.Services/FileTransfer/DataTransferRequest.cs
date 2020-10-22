using System;
using System.IO;
using JetBrains.Annotations;

namespace Tauron.Application.AkkNode.Services.FileTransfer
{
    [PublicAPI]
    public abstract class DataTransferRequest : TransferMessages.TransferMessage
    {
        public abstract Func<ITransferData> Source { get; }

        public DataTransferManager Target { get; }

        public string? Data { get; }

        public bool SendCompletionBack { get; set; }

        protected DataTransferRequest(string operationId, DataTransferManager target, string? data) 
            : base(operationId)
        {
            Target = target;
            Data = data;
        }

        public static DataTransferRequest FromStream(Func<ITransferData> stream, DataTransferManager target, string? data = null)
            => new StreamTransferRequest(Guid.NewGuid().ToString(), stream, target, data);

        public static DataTransferRequest FromFile(string file, DataTransferManager target, string? data = null)
            => new DataStreamTranferRequest(Guid.NewGuid().ToString(), file, target, data);

        public static DataTransferRequest FromStream(Stream stream, DataTransferManager target, string? data = null)
            => new StreamTransferRequest(Guid.NewGuid().ToString(), stream, target, data);

        public static DataTransferRequest FromStream(Func<Stream> stream, DataTransferManager target, string? data = null)
            => new StreamTransferRequest(Guid.NewGuid().ToString(), stream, target, data);

        public static DataTransferRequest FromFile(string opsId, string file, DataTransferManager target, string? data = null)
            => new DataStreamTranferRequest(opsId, file, target, data);

        public static DataTransferRequest FromStream(string opsId, Stream stream, DataTransferManager target, string? data = null)
            => new StreamTransferRequest(opsId, stream, target, data);

        public static DataTransferRequest FromStream(string opsId, Func<Stream> stream, DataTransferManager target, string? data = null)
            => new StreamTransferRequest(opsId, stream, target, data);

        public sealed class StreamTransferRequest : DataTransferRequest
        {
            public StreamTransferRequest(string operationId, Func<Stream> source, DataTransferManager target, string? data)
                : base(operationId, target, data) =>
                Source = () => new StreamData(source());

            public StreamTransferRequest(string operationId, Func<ITransferData> source, DataTransferManager target, string? data)
                : base(operationId, target, data) =>
                Source = source;

            public StreamTransferRequest(string operationId, Stream source, DataTransferManager target, string? data)
                : this(operationId, () => source, target, data)
            {
            }

            public override Func<ITransferData> Source { get; }
        }

        public sealed class DataStreamTranferRequest : DataTransferRequest
        {
            public DataStreamTranferRequest(string operationId, string file, DataTransferManager target, string? data) 
                : base(operationId, target, data)
            {
                Source = () => new StreamData(File.OpenRead(file));
            }

            public override Func<ITransferData> Source { get; }
        }
    }
}