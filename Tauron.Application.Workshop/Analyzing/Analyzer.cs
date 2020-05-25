using System.Collections.Generic;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Analyzing.Actor;
using Tauron.Application.Workshop.Analyzing.Rules;

namespace Tauron.Application.Workshop.Analyzing
{
    [PublicAPI]
    public sealed class Analyzer<TWorkspace, TData>
        where TWorkspace : WorkspaceBase<TData>
    {
        private readonly HashSet<string> _rules = new HashSet<string>();

        public IActorRef Actor { get; }

        public Analyzer(TWorkspace workspace, IActorRefFactory factory) 
            => Actor = factory.ActorOf(Props.Create(() => new AnalyzerActor<TWorkspace, TData>(workspace)), "AnalyzerActor");

        internal Analyzer() => Actor = ActorRefs.Nobody;

        public void RegisterRule(IRule<TWorkspace, TData> rule)
        {
            lock (_rules)
            {
                if(!_rules.Add(rule.Name))
                    return;
            }

            Actor.Tell(new RegisterRule<TWorkspace, TData>(rule));
        }
    }
}