using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Akka.Actor;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Core;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Workshop.Mutation
{
    [PublicAPI]
    public sealed class MutatingEngine<TData> : MutatingEngine, IEventSourceable<TData>
        where TData : class
    {
        private readonly Task<IActorRef> _mutator;
        private readonly IDataSource<TData> _dataSource;
        private readonly WorkspaceSuperviser _superviser;
        private readonly ResponderList _responder;

        internal MutatingEngine(Task<IActorRef> mutator, IDataSource<TData> dataSource, WorkspaceSuperviser superviser)
            : base(mutator, superviser)
        {
            _mutator = mutator;
            _dataSource = dataSource;
            _superviser = superviser;
            _responder = new ResponderList(_dataSource.SetData);
        }

        public MutatingEngine(IDataSource<TData> dataSource) : base(Task.FromResult<IActorRef>(ActorRefs.Nobody), new WorkspaceSuperviser())
        {
            _mutator = Task.FromResult<IActorRef>(ActorRefs.Nobody);
            _dataSource = dataSource;
            _superviser = new WorkspaceSuperviser();
            _responder = new ResponderList(_dataSource.SetData);
        }


        public void Mutate(string name, Func<Maybe<TData>, Maybe<TData>> transform, Maybe<object> hash = default) 
            => Mutate(CreateMutate(name, transform, hash));

        public IDataMutation CreateMutate(string name, Func<Maybe<TData>, Maybe<TData>> transform, Maybe<object> hash = default)
        {
            void Runner() => _responder.Push(transform(_dataSource.GetData()));
            return new DataMutation(Runner, name, hash);
        }

        public IDataMutation CreateMutate(string name, Func<Maybe<TData>, Task<Maybe<TData>>> transform, Maybe<object> hash = default)
        {
            async Task Runner() => _responder.Push(await transform(_dataSource.GetData()));
            return new AsyncDataMutation(Runner, name, hash);
        }

        public IEventSource<TRespond> EventSource<TRespond>(Func<Maybe<TData>, Maybe<TRespond>> transformer, Maybe<Func<Maybe<TData>, bool>> where) 
            => new EventSource<TRespond, TData>(_superviser, _mutator, transformer, where, _responder);

        public IEventSource<TRespond> EventSource<TRespond>(Func<Maybe<TData>, Maybe<TRespond>> transformer, Func<Maybe<TData>, bool> where) => EventSource(transformer, where.ToMaybe());

        public IEventSource<TRespond> EventSource<TRespond>(Func<Maybe<TData>, Maybe<TRespond>> transformer) => EventSource(transformer, Maybe<Func<Maybe<TData>, bool>>.Nothing);

        private sealed class ResponderList : IRespondHandler<TData>
        {
            private readonly List<Action<Maybe<TData>>> _handler = new();
            private readonly Action<Maybe<TData>> _root;

            public ResponderList(Action<Maybe<TData>> root) => _root = root;

            public void Register(Action<Maybe<TData>> responder)
            {
                lock (_handler)
                    _handler.Add(responder);
            }

            public void Push(Maybe<TData> data)
            {
                if (data.IsNothing()) return;

                lock (_handler)
                {
                    _root(data);
                    foreach (var action in _handler)
                        action(data);
                }
            }
        }
    }

    [PublicAPI]
    public sealed class ExtendedMutatingEngine<TData> : MutatingEngine, IEventSourceable<TData>
        where TData : class
    {
        private readonly Task<IActorRef> _mutator;
        private readonly IExtendedDataSource<TData> _dataSource;
        private readonly WorkspaceSuperviser _superviser;
        private readonly ResponderList _responder;

        internal ExtendedMutatingEngine(Task<IActorRef> mutator, IExtendedDataSource<TData> dataSource, WorkspaceSuperviser superviser)
            : base(mutator, superviser)
        {
            _mutator = mutator;
            _dataSource = dataSource;
            _superviser = superviser;
            _responder = new ResponderList(_dataSource.SetData, dataSource.OnCompled);
        }

        public void Mutate(string name, IQuery query, Func<Maybe<TData>, Maybe<TData>> transform)
            => Mutate(CreateMutate(name, query, transform));

        public IDataMutation CreateMutate(string name, IQuery query, Func<Maybe<TData>, Maybe<TData>> transform)
        {
            async Task Runner() => await _responder.Push(query, async () => transform(await _dataSource.GetData(query)));
            return new AsyncDataMutation(Runner, name, query.ToHash().Cast<string, object>());
        }

        public IDataMutation CreateMutate(string name, IQuery query, Func<Maybe<TData>, Task<Maybe<TData>>> transform)
        {
            async Task Runner() => await _responder.Push(query, async () => await transform(await _dataSource.GetData(query)));
            return new AsyncDataMutation(Runner, name, query.ToHash().Cast<string, object>());
        }

        public IEventSource<TRespond> EventSource<TRespond>(Func<Maybe<TData>, Maybe<TRespond>> transformer, Maybe<Func<Maybe<TData>, bool>> where)
            => new EventSource<TRespond, TData>(_superviser, _mutator, transformer, where, _responder);

        public IEventSource<TRespond> EventSource<TRespond>(Func<Maybe<TData>, Maybe<TRespond>> transformer, Func<Maybe<TData>, bool> where) => EventSource(transformer, where.ToMaybe());

        public IEventSource<TRespond> EventSource<TRespond>(Func<Maybe<TData>, Maybe<TRespond>> transformer) => EventSource(transformer, Maybe<Func<Maybe<TData>, bool>>.Nothing);

        private sealed class ResponderList : IRespondHandler<TData>
        {
            private readonly List<Action<Maybe<TData>>> _handler = new();
            private readonly Func<IQuery, Maybe<TData>, Task> _root;
            private readonly Func<IQuery, Task> _completer;

            public ResponderList(Func<IQuery, Maybe<TData>, Task> root, Func<IQuery, Task> completer)
            {
                _root = root;
                _completer = completer;
            }

            public void Register(Action<Maybe<TData>> responder)
            {
                lock (_handler)
                    _handler.Add(responder);
            }

            public async Task Push(IQuery query, Func<Task<Maybe<TData>>> dataFunc)
            {
                try
                {
                    var data = await dataFunc();
                    if (data.IsNothing()) return;

                    await _root(query, data);
                    lock (_handler)
                    {
                        foreach (var action in _handler)
                            action(data);
                    }
                }
                finally
                {
                    await _completer(query);
                }
            }
        }
    }

    [PublicAPI]
    public class MutatingEngine : DeferredActor<DeferredActorState>
    {
        public static MutatingEngine Create(WorkspaceSuperviser superviser, Func<Props, Props>? configurate = null)
        {
            var mutatorProps = Props.Create<MutationActor>();
            mutatorProps = configurate?.Invoke(mutatorProps) ?? mutatorProps;

            var mutator = superviser.Create(mutatorProps.ToMaybe(), "Mutator");
            return new MutatingEngine(mutator, superviser);
        }

        public static ExtendedMutatingEngine<TData> From<TData>(IExtendedDataSource<TData> source, WorkspaceSuperviser superviser, Func<Props, Props>? configurate = null)
            where TData : class
        {
            var mutatorProps = Props.Create<MutationActor>();
            mutatorProps = configurate?.Invoke(mutatorProps) ?? mutatorProps;

            var mutator = superviser.Create(mutatorProps.ToMaybe(), "Mutator");
            return new ExtendedMutatingEngine<TData>(mutator, source, superviser);
        }

        public static ExtendedMutatingEngine<TData> From<TData>(IExtendedDataSource<TData> source, MutatingEngine parent)
            where TData : class => new(parent._mutator, source, parent._superviser);

        public static MutatingEngine<TData> From<TData>(IDataSource<TData> source, WorkspaceSuperviser superviser, Func<Props, Props>? configurate = null) 
            where TData : class
        {
            var mutatorProps = Props.Create<MutationActor>();
            mutatorProps = configurate?.Invoke(mutatorProps) ?? mutatorProps;

            var mutator = superviser.Create(mutatorProps.ToMaybe(), "Mutator");
            return new MutatingEngine<TData>(mutator, source, superviser);
        }

        public static MutatingEngine<TData> From<TData>(IDataSource<TData> source, MutatingEngine parent) 
            where TData : class => new(parent._mutator, source, parent._superviser);

        public static MutatingEngine<TData> Dummy<TData>(IDataSource<TData> source) 
            where TData : class => new(source);

        private readonly Task<IActorRef> _mutator;
        private readonly WorkspaceSuperviser _superviser;

        protected MutatingEngine(Task<IActorRef> mutator, WorkspaceSuperviser superviser)
            : base(mutator, new DeferredActorState(ImmutableList<object>.Empty, ActorRefs.Nobody))
        {
            _mutator = mutator;
            _superviser = superviser;
        }

        public void Mutate(IDataMutation mutationOld)
            => TellToActor(mutationOld);
    }

    [PublicAPI]
    public static class MutatinEngineExtensions
    {
        public static IEventSource<TEvent> EventSource<TData, TEvent>(this IEventSourceable<MutatingContext<TData>> engine)
            where TEvent : MutatingChange
            => engine.EventSource(
                c =>
                    from con in c
                    select con.GetChange<TEvent>(),
                c =>
                (
                    from con in c
                    from change in con.Change
                    select change is TEvent
                ).OrElse(false)
            );
    }
}