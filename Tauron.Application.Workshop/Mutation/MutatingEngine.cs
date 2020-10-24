using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Core;

namespace Tauron.Application.Workshop.Mutation
{
    [PublicAPI]
    public sealed class MutatingEngine<TData> : DeferredActor
    {
        private readonly Task<IActorRef> _mutator;
        private readonly IDataSource<TData> _dataSource;
        private readonly WorkspaceSuperviser _superviser;
        private readonly ResponderList _responder;

        internal MutatingEngine(Task<IActorRef> mutator, IDataSource<TData> dataSource, WorkspaceSuperviser superviser)
            : base(mutator)
        {
            _mutator = mutator;
            _dataSource = dataSource;
            _superviser = superviser;
            _responder = new ResponderList(_dataSource.SetData);
        }

        internal MutatingEngine(IDataSource<TData> dataSource)
            : base(Task.FromResult<IActorRef>(ActorRefs.Nobody))
        {
            _mutator = Task.FromResult<IActorRef>(ActorRefs.Nobody);
            _dataSource = dataSource;
            _responder = new ResponderList(_dataSource.SetData);
            _superviser = new WorkspaceSuperviser();
        }

        public void Mutate(string name, Func<TData, TData> transform) 
            => TellToActor(new DataMutation<TData>(transform, _dataSource.GetData, _responder.Push, name));

        public IEventSource<TRespond> EventSource<TRespond>(Func<TData, TRespond> transformer, Func<TData, bool>? where = null) 
            => new EventSource<TRespond, TData>(_superviser, _mutator, transformer, @where, _responder);

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

            public void Push(TData data)
            {
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
    public static class MutatingEngine
    {
        public static MutatingEngine<TData> From<TData>(IDataSource<TData> source, WorkspaceSuperviser superviser, Func<Props, Props>? configurate = null)
        {
            var mutatorProps = Props.Create<MutationActor<TData>>();
            mutatorProps = configurate?.Invoke(mutatorProps) ?? mutatorProps;

            var mutator = superviser.Create(mutatorProps, "Mutator");
            return new MutatingEngine<TData>(mutator, source, superviser);
        }

        public static MutatingEngine<TData> Dummy<TData>(IDataSource<TData> source)
        {
            return new MutatingEngine<TData>(source);
        }
    }
}