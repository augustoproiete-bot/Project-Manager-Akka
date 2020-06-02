using System.Collections.Generic;
using Akka.Actor;
using Akka.Actor.Dsl;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Analyzing.Actor;

namespace Tauron.Application.Workshop.Analyzing.Rules
{
    [PublicAPI]
    public abstract class RuleBase<TWorkspace, TData> : IRule<TWorkspace, TData>
        where TWorkspace : WorkspaceBase<TData>
    {
        public abstract string Name { get; }

        public IActorRef Init(IActorRefFactory superviser, TWorkspace workspace)
        {
            return superviser.ActorOf((dsl, contxt) =>
            {
                ActorConstruct(dsl, contxt);
                RegisterResponds(workspace, contxt);
                RegisterRules(dsl);
            }, Name);
        }

        protected abstract void ActorConstruct(IActorDsl dsl, IActorContext context);

        protected abstract void RegisterResponds(TWorkspace workspace, IActorContext context);

        protected abstract void RegisterRules(IActorDsl dsl);

        protected void SendIssues(IEnumerable<Issue> issues, IActorContext context)
            => context.Parent.Tell(new RuleIssuesChanged<TWorkspace, TData>(this, issues));
    }
}