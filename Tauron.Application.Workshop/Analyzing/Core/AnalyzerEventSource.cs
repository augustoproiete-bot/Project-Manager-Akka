using Akka.Actor;
using Tauron.Application.Workshop.Analyzing.Actor;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.Analyzing.Core
{
    public sealed class AnalyzerEventSource<TWorkspace, TData> : EventSourceBase<IssuesEvent>
        where TWorkspace : WorkspaceBase<TData>
    {
        public AnalyzerEventSource(IActorRef mutator)
            : base(mutator)
        {
        }

        public void SendEvent(RuleIssuesChanged<TWorkspace, TData> evt)
        {
            Send(evt.ToEvent());
        }
    }
}