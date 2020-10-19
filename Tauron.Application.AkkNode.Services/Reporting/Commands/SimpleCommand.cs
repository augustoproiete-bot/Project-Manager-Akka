using System;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Application.AkkNode.Services.Commands
{
    [PublicAPI]
    public abstract class SimpleCommand<TSender, TThis> : ReporterCommandBase<TSender, TThis> 
        where TThis : SimpleCommand<TSender, TThis>
        where TSender : ISender
    {
        public Task Send(TSender sender, TimeSpan timeout, Action<string> messages)
            => SendingHelper.Send<object, TThis>(sender, (TThis)this, messages, timeout, true);
    }
}