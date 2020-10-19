using System;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Application.AkkNode.Services.Commands
{
    public abstract class ReporterCommandBase<TSender, TThis> : IReporterMessage
        where TSender : ISender 
        where TThis : ReporterCommandBase<TSender, TThis>
    {
        private IActorRef _listner;

        [UsedImplicitly]
        public IActorRef Listner
        {
            get => _listner;
            set
            {
                if (!Listner.IsNobody())
                    throw new InvalidOperationException("Only One Listner Can be Set");
                _listner = value;
            }
        }

        string IReporterMessage.Info => Info;

        protected abstract string Info { get; }
        
        public void SetListner(IActorRef listner)
        {
            Listner = listner;
        }
    }
}