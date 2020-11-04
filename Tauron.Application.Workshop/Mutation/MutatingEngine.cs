using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
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


        public void Mutate(string name, Func<TData, TData?> transform, object? hash = null) 
            => Mutate(CreateMutate(name, transform, hash));

        public IDataMutation CreateMutate(string name, Func<TData, TData?> transform, object? hash = null)
        {
            void Runner() => _responder.Push(transform(_dataSource.GetData()));
            return new DataMutation<TData>(Runner, name, hash);
        }

        public IDataMutation CreateMutate(string name, Func<TData, Task<TData?>> transform, object? hash = null)
        {
            async Task Runner() => _responder.Push(await transform(_dataSource.GetData()));
            return new AsyncDataMutation<TData>(Runner, name, hash);
        }

        public IEventSource<TRespond> EventSource<TRespond>(Func<TData, TRespond> transformer, Func<TData, bool>? where = null) 
            => new EventSource<TRespond, TData>(_superviser, _mutator, transformer, where, _responder);
        
        private sealed class ResponderList : IRespondHandler<TData>
        {
            private readonly List<Action<TData>> _handler = new List<Action<TData>>();
            private readonly Action<TData> _root;

            public ResponderList(Action<TData> root) => _root = root;

            public void Register(Action<TData> responder)
            {
                lock (_handler)
                {
                    _handler.Add(responder);
                }
            }

            public void Push(TData? data)
            {
                if(data == null) return;

                _root(data);
                lock (_handler)
                {
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
            _responder = new ResponderList(_dataSource.SetData);
        }

        public void Mutate(string name, IQuery query, Func<TData, TData?> transform)
            => Mutate(CreateMutate(name, query, transform));

        public IDataMutation CreateMutate(string name, IQuery query, Func<TData, TData?> transform)
        {
            async Task Runner() => await _responder.Push(query, transform(await _dataSource.GetData(query)));
            return new AsyncDataMutation<TData>(Runner, name, query.ToHash());
        }

        public IDataMutation CreateMutate(string name, IQuery query, Func<TData, Task<TData?>> transform)
        {
            async Task Runner() => await _responder.Push(query, await transform(await _dataSource.GetData(query)));
            return new AsyncDataMutation<TData>(Runner, name, query.ToHash());
        }

        public IEventSource<TRespond> EventSource<TRespond>(Func<TData, TRespond> transformer, Func<TData, bool>? where = null)
            => new EventSource<TRespond, TData>(_superviser, _mutator, transformer, where, _responder);

        private sealed class ResponderList : IRespondHandler<TData>
        {
            private readonly List<Action<TData>> _handler = new List<Action<TData>>();
            private readonly Func<IQuery, TData, Task> _root;

            public ResponderList(Func<IQuery, TData, Task> root) => _root = root;

            public void Register(Action<TData> responder)
            {
                lock (_handler)
                {
                    _handler.Add(responder);
                }
            }

            public async Task Push(IQuery query, TData? data)
            {
                if (data == null) return;

                await _root(query, data);
                lock (_handler)
                {
                    foreach (var action in _handler)
                        action(data);
                }
            }
        }
    }

    [PublicAPI]
    public class MutatingEngine : DeferredActor
    {
        public static MutatingEngine Create(WorkspaceSuperviser superviser, Func<Props, Props>? configurate = null)
        {
            var mutatorProps = Props.Create<MutationActor>();
            mutatorProps = configurate?.Invoke(mutatorProps) ?? mutatorProps;

            var mutator = superviser.Create(mutatorProps, "Mutator");
            return new MutatingEngine(mutator, superviser);
        }

        public static ExtendedMutatingEngine<TData> From<TData>(IExtendedDataSource<TData> source, WorkspaceSuperviser superviser, Func<Props, Props>? configurate = null)
            where TData : class
        {
            var mutatorProps = Props.Create<MutationActor>();
            mutatorProps = configurate?.Invoke(mutatorProps) ?? mutatorProps;

            var mutator = superviser.Create(mutatorProps, "Mutator");
            return new ExtendedMutatingEngine<TData>(mutator, source, superviser);
        }

        public static ExtendedMutatingEngine<TData> From<TData>(IExtendedDataSource<TData> source, MutatingEngine parent)
            where TData : class => new ExtendedMutatingEngine<TData>(parent._mutator, source, parent._superviser);

        public static MutatingEngine<TData> From<TData>(IDataSource<TData> source, WorkspaceSuperviser superviser, Func<Props, Props>? configurate = null) 
            where TData : class
        {
            var mutatorProps = Props.Create<MutationActor>();
            mutatorProps = configurate?.Invoke(mutatorProps) ?? mutatorProps;

            var mutator = superviser.Create(mutatorProps, "Mutator");
            return new MutatingEngine<TData>(mutator, source, superviser);
        }

        public static MutatingEngine<TData> From<TData>(IDataSource<TData> source, MutatingEngine parent) 
            where TData : class => new MutatingEngine<TData>(parent._mutator, source, parent._superviser);

        public static MutatingEngine<TData> Dummy<TData>(IDataSource<TData> source) 
            where TData : class
        {
            return new MutatingEngine<TData>(source);
        }

        private readonly Task<IActorRef> _mutator;
        private readonly WorkspaceSuperviser _superviser;

        protected MutatingEngine(Task<IActorRef> mutator, WorkspaceSuperviser superviser)
            : base(mutator)
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
            => engine.EventSource(c => c.GetChange<TEvent>(), c => c.Change?.Cast<TEvent>() != null);
    }
}