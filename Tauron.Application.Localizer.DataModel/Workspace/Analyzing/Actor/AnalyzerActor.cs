using Akka.Actor;

namespace Tauron.Application.Localizer.DataModel.Workspace.Analyzing.Actor
{
    public sealed class AnalyzerActor : ReceiveActor
    {
        private readonly ProjectFileWorkspace _workspace;

        public AnalyzerActor(ProjectFileWorkspace workspace)
        {
            _workspace = workspace;

            Receive<RegisterRule>(RegisterRule);
            Receive<RuleIssuesChanged>(RuleIssuesChanged);
        }

        private void RuleIssuesChanged(RuleIssuesChanged obj) => Context.Parent.Tell(obj);

        private void RegisterRule(RegisterRule obj)
        {
            var rule = obj.Rule;
            rule.Init(Context, _workspace);
        }
    }
}