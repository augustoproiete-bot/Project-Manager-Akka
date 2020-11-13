using Functional.Maybe;
using System;
using System.Collections.Generic;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Workshop.Analyzing.Actor;

namespace Tauron.Application.Workshop.Analyzing.Rules
{
    [PublicAPI]
    public abstract class RuleBase<TWorkspace, TData> : IRule<TWorkspace, TData>
        where TWorkspace : WorkspaceBase<TData> where TData : class
    {
        private sealed class InternalRuleActor : ExposedReceiveActor
        {
            public InternalRuleActor(Action<IExposedReceiveActor> constructor) 
                => constructor(this);
        }

        protected Maybe<TWorkspace> Workspace { get; private set; }

        public abstract string Name { get; }

        public Maybe<IActorRef> Init(IActorRefFactory superviser, Maybe<TWorkspace> workspace)
        {
            Workspace = workspace;
            return superviser.ActorOf(() => new InternalRuleActor(ActorConstruct), Name);
        }

        protected abstract void ActorConstruct(IExposedReceiveActor actor);

        //protected abstract void RegisterResponds(TWorkspace workspace, IActorContext context);

        //protected abstract void RegisterRules(IActorDsl dsl);

        protected void SendIssues(IEnumerable<Issue.IssueCompleter> issues, IActorContext context)
            => context.Parent.Tell(new RuleIssuesChanged<TWorkspace, TData>(this, issues));
    }
}