using System;
using System.Threading.Tasks;

namespace Tauron.Application.AkkNode.Services.FileTransfer
{
    public sealed class AwaitResponse
    {
        private readonly IncomingDataTransfer? _request;

        public AwaitResponse(IncomingDataTransfer? request) => _request = request;

        public async Task<TransferMessages.TransferCompled> TryStart(Func<ITransferData?> getdata)
        {
            if(_request == null)
                return new TransferFailed(string.Empty, FailReason.Deny, "NoData");

            var data = getdata();
            if (data == null)
                return new TransferFailed(_request.OperationId, FailReason.Deny, _request.Data);

            return await _request.Accept(() => data);
        }
    }

    public sealed class AwaitRequest
    {
        public TimeSpan Timeout { get; }

        public string Id { get; }

        public AwaitRequest(TimeSpan timeout, string id)
        {
            Timeout = timeout;
            Id = id;
        }
    }
}