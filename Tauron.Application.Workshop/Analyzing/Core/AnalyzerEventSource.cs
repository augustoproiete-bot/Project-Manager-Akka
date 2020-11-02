using System.Threading.Tasks;
using Akka.Actor;
using Tauron.Application.Workshop.Analyzing.Actor;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.Analyzing.Core
{
    public sealed class AnalyzerEventSource<TWorkspace, TData> : EventSourceBase<IssuesEvent>
        where TWorkspace : WorkspaceBase<TData> where TData : class
    {
        public AnalyzerEventSource(Task<IActorRef> mutator, WorkspaceSuperviser superviser)
            : base(mutator, superviser)
        {
        }

        public void SendEvent(RuleIssuesChanged<TWorkspace, TData> evt)
        {
            Send(evt.ToEvent());
        }
    }
}