using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Akka.Actor;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Analyzing.Actor;
using Tauron.Application.Workshop.Analyzing.Core;
using Tauron.Application.Workshop.Analyzing.Rules;
using Tauron.Application.Workshop.Core;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.Analyzing
{
    [PublicAPI]
    public sealed class Analyzer<TWorkspace, TData> : DeferredActor<Analyzer<TWorkspace, TData>.AnalyserState>, IAnalyzer<TWorkspace, TData> 
        where TWorkspace : WorkspaceBase<TData> where TData : class
    {
        public sealed record AnalyserState(ImmutableHashSet<string> Rules, ImmutableList<object>? Stash, IActorRef Actor) : DeferredActorState(Stash, Actor)
        {
            public AnalyserState()
                : this(ImmutableHashSet<string>.Empty, ImmutableList<object>.Empty, ActorRefs.Nobody)
            {
                
            }
        }
        
        internal Analyzer(Task<IActorRef> actor, IEventSource<IssuesEvent> source)
            : base(actor, new AnalyserState()) => Issues = source;

        internal Analyzer()
            : base(Task.FromResult<IActorRef>(ActorRefs.Nobody), new AnalyserState()) 
            => Issues = new AnalyzerEventSource<TWorkspace, TData>(Task.FromResult<IActorRef>(ActorRefs.Nobody), new WorkspaceSuperviser());

        public void RegisterRule(IRule<TWorkspace, TData> rule)
        {
            if(ObjectState.Rules.Contains(rule.Name))
                return;
            
            Run(s =>
                    from state in s
                    let data = state.Rules.Add(rule.Name)
                    select state with{Rules = data});

            TellToActor(new RegisterRule<TWorkspace, TData>(rule));
        }

        public IEventSource<IssuesEvent> Issues { get; }
    }

    [PublicAPI]
    public static class Analyzer
    {
        public static IAnalyzer<TWorkspace, TData> From<TWorkspace, TData>(Maybe<TWorkspace> workspace, WorkspaceSuperviser superviser)
            where TWorkspace : WorkspaceBase<TData> where TData : class
        {
            var evtSource = new SourceFabricator<TWorkspace, TData>();

            var actor = superviser.Create(Props.Create(() => new AnalyzerActor<TWorkspace, TData>(workspace, evtSource.Send)).ToMaybe(), "AnalyzerActor");
            evtSource.Init(actor, superviser);

            return new Analyzer<TWorkspace, TData>(actor, evtSource.EventSource ?? throw new InvalidOperationException("Create Analyzer"));
        }

        private class SourceFabricator<TWorkspace, TData> where TWorkspace : WorkspaceBase<TData> where TData : class
        {
            public AnalyzerEventSource<TWorkspace, TData>? EventSource { get; private set; }


            public void Init(Task<IActorRef> actor, WorkspaceSuperviser superviser) 
                => EventSource = new AnalyzerEventSource<TWorkspace, TData>(actor, superviser);

            public void Send(RuleIssuesChanged<TWorkspace, TData> evt) 
                => EventSource?.SendEvent(evt);
        }
    }
}