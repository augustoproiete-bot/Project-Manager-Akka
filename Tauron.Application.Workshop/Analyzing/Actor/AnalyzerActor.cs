using System;
using Akka.Actor;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.Analyzing.Actor
{
    public sealed class AnalyzerActor<TWorkspace, TData> : ReceiveActor
        where TWorkspace : WorkspaceBase<TData>
    {
        private sealed class HandlerTerminated
        {
            public Action Remover { get; }

            public HandlerTerminated(Action remover) => Remover = remover;
        }

        private readonly TWorkspace _workspace;
        private readonly Action<RuleIssuesChanged<TWorkspace, TData>> _issesAction;

        public AnalyzerActor(TWorkspace workspace, Action<RuleIssuesChanged<TWorkspace, TData>> issesAction)
        {
            _workspace = workspace;
            _issesAction = issesAction;

            Receive<RegisterRule<TWorkspace, TData>>(RegisterRule);
            Receive<RuleIssuesChanged<TWorkspace, TData>>(RuleIssuesChanged);

            Receive<WatchIntrest>(wi => Context.WatchWith(wi.Target, new HandlerTerminated(wi.OnRemove)));
            Receive<HandlerTerminated>(ht => ht.Remover());
            Receive<Terminated>(t => { });
        }

        private void RuleIssuesChanged(RuleIssuesChanged<TWorkspace, TData> obj) => _issesAction(obj);

        private void RegisterRule(RegisterRule<TWorkspace, TData> obj)
        {
            var rule = obj.Rule;
            rule.Init(Context, _workspace);
        }
    }
}