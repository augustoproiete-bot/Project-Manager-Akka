using System;
using System.Collections.Generic;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.Mutation
{
    [PublicAPI]
    public sealed class MutatingEngine<TData>
    {
        private readonly IDataSource<TData> _dataSource;
        private readonly IActorRef _mutator;
        private readonly ResponderList _responder;

        internal MutatingEngine(IActorRef mutator, IDataSource<TData> dataSource)
        {
            _dataSource = dataSource;
            _mutator = mutator;
            _responder = new ResponderList(_dataSource.SetData);
        }

        internal MutatingEngine(IDataSource<TData> dataSource)
        {
            _dataSource = dataSource;
            _mutator = ActorRefs.Nobody;
            _responder = new ResponderList(_dataSource.SetData);
        }

        public void Mutate(string name, Func<TData, TData> transform)
        {
            _mutator.Tell(new DataMutation<TData>(transform, _dataSource.GetData, _responder.Push, name));
        }

        public IEventSource<TRespond> EventSource<TRespond>(Func<TData, TRespond> transformer, Func<TData, bool>? where = null)
        {
            return new EventSource<TRespond, TData>(_mutator, transformer, where, _responder);
        }

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
        public static MutatingEngine<TData> From<TData>(IDataSource<TData> source, IActorRefFactory factory)
        {
            var mutator = factory.ActorOf<MutationActor<TData>>("Mutator");
            return new MutatingEngine<TData>(mutator, source);
        }

        public static MutatingEngine<TData> Dummy<TData>(IDataSource<TData> source)
        {
            return new MutatingEngine<TData>(source);
        }
    }
}