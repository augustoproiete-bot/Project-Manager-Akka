using JetBrains.Annotations;

namespace Tauron.Application.AkkNode.Services.Commands
{
    [PublicAPI]
    public sealed class FileTransactionId
    {
        public string Id { get; }

        public FileTransactionId(string id) => Id = id;

    }
}