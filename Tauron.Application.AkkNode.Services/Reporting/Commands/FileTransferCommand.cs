using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.FileTransfer;

namespace Tauron.Application.AkkNode.Services.Commands
{
    [PublicAPI]
    public abstract class FileTransferCommand<TSender, TThis> : ReporterCommandBase<TSender, TThis> 
        where TThis : FileTransferCommand<TSender, TThis> 
        where TSender : ISender
    {
        private DataTransferManager? _manager;

        [UsedImplicitly]
        public DataTransferManager? Manager
        {
            get => _manager;
            set
            {
                if(_manager != null)
                    throw new InvalidOperationException("Datamanager Should set only once");
                _manager = value;
            }
        }

        public Task<TransferMessages.TransferCompled> Send(TSender sender, TimeSpan timeout, DataTransferManager manager, Action<string> messages, Func<Stream?> getdata)
            => Send(sender, timeout, manager, messages, () =>
            {
                var str = getdata();
                return str == null ? null : new StreamData(str);
            });

        public async Task<TransferMessages.TransferCompled> Send(TSender sender, TimeSpan timeout, DataTransferManager manager, Action<string> messages, Func<ITransferData?> getdata)
        {
            Manager = manager;
            var id = await SendingHelper.Send<FileTransactionId, TThis>(sender, (TThis) this, messages, timeout, false);

            var tranfer = await Manager.AskAwaitOperation(new AwaitRequest(timeout, id.Id));

            return await tranfer.TryStart(getdata);
        }
    }
}