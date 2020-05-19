using System.Collections.Generic;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel.Workspace.Analyzing.Actor;
using Tauron.Application.Localizer.DataModel.Workspace.Analyzing.Rules;

namespace Tauron.Application.Localizer.DataModel.Workspace.Analyzing
{
    [PublicAPI]
    public sealed class Analyzer
    {
        private readonly HashSet<string> _rules = new HashSet<string>();

        public IActorRef Actor { get; }

        public Analyzer(ProjectFileWorkspace workspace, IUntypedActorContext factory) 
            => Actor = factory.ActorOf(Props.Create(() => new AnalyzerActor(workspace)), nameof(AnalyzerActor));

        internal Analyzer() => Actor = ActorRefs.Nobody;

        public void RegisterRule(IRule rule)
        {
            lock (_rules)
            {
                if(!_rules.Add(rule.Name))
                    return;
            }

            Actor.Tell(new RegisterRule(rule));
        }
    }
}