using System;
using System.Threading.Tasks;

namespace Tauron.Application.AkkNode.Services.Commands
{
    public abstract class ResultCommand<TSender, TThis, TResult> : ReporterCommandBase<TSender, TThis> 
        where TSender : ISender 
        where TThis : ResultCommand<TSender, TThis, TResult>
    {
        public Task<TResult> Send(TSender sender, TimeSpan timeout, Action<string> messages)
            => SendingHelper.Send<TResult, TThis>(sender, (TThis) this, messages, timeout, false);
    }
}