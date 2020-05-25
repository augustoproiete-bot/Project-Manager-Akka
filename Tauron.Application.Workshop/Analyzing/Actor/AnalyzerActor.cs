using Akka.Actor;

namespace Tauron.Application.Workshop.Analyzing.Actor
{
    public sealed class AnalyzerActor<TWorkspace, TData> : ReceiveActor
        where TWorkspace : WorkspaceBase<TData>
    {
        private readonly TWorkspace _workspace;

        public AnalyzerActor(TWorkspace workspace)
        {
            _workspace = workspace;

            Receive<RegisterRule<TWorkspace, TData>>(RegisterRule);
            Receive<RuleIssuesChanged<TWorkspace, TData>>(RuleIssuesChanged);
        }

        private void RuleIssuesChanged(RuleIssuesChanged<TWorkspace, TData> obj) => Context.Parent.Tell(obj);

        private void RegisterRule(RegisterRule<TWorkspace, TData> obj)
        {
            var rule = obj.Rule;
            rule.Init(Context, _workspace);
        }
    }
}