using Functional.Maybe;
using System;
using Akka.Actor;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.Analyzing.Actor
{
    public sealed class AnalyzerActor<TWorkspace, TData> : ReceiveActor
        where TWorkspace : WorkspaceBase<TData> where TData : class
    {
        private readonly Action<RuleIssuesChanged<TWorkspace, TData>> _issesAction;
        private readonly Maybe<TWorkspace> _workspace;

        public AnalyzerActor(Maybe<TWorkspace> workspace, Action<RuleIssuesChanged<TWorkspace, TData>> issesAction)
        {
            _workspace = workspace;
            _issesAction = issesAction;

            Receive<RegisterRule<TWorkspace, TData>>(RegisterRule);
            Receive<RuleIssuesChanged<TWorkspace, TData>>(RuleIssuesChanged);

            Receive<WatchIntrest>(wi => Context.WatchWith(wi.Target, new HandlerTerminated(wi.OnRemove)));
            Receive<HandlerTerminated>(ht => ht.Remover());
            Receive<Terminated>(_ => { });
        }

        private void RuleIssuesChanged(RuleIssuesChanged<TWorkspace, TData> obj) 
            => _issesAction(obj);

        private void RegisterRule(RegisterRule<TWorkspace, TData> obj)
        {
            var rule = obj.Rule;
            rule.Init(Context, _workspace);
        }

        private sealed record HandlerTerminated(Action Remover);
    }
}