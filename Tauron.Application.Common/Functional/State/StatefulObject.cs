using System;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Akka;
using static Tauron.Prelude;

namespace Tauron
{
    public sealed record EmptyState;
    
    public record ObjectState<TState>(TState Data)
    {
        public ObjectState<TState> Run(Func<Maybe<TState>, Maybe<TState>> operation) =>
            OrElse(from data in operation(May(Data))
                   select this with{ Data = data }, this);
    }

    public record LockedObjectState<TState>(object Lock, TState Data) : ObjectState<TState>(Data);
    
    public sealed class StatefulObjectLogic<TState>
    {
        private ObjectState<TState> _objectState;
        
        public StatefulObjectLogic(TState initialState, bool isLocked = false)
            => _objectState = isLocked ? new LockedObjectState<TState>(new object(), initialState) : new ObjectState<TState>(initialState);

        public TState ObjectState => _objectState.Data;

        public TState Run(Func<Maybe<TState>, Maybe<TState>> operation)
        {
            if (_objectState is LockedObjectState<TState> lockedObject)
            {
                lock (lockedObject.Lock)
                    _objectState = _objectState.Run(operation);
            }
            else
                _objectState = _objectState.Run(operation);

            return ObjectState;
        }
    }
    
    [PublicAPI]
    public abstract class StatefulObject<TState>
    {
        private StatefulObjectLogic<TState> _stateLogic;


        protected StatefulObject(TState initialState, bool isLocked = false)
            => _stateLogic = new StatefulObjectLogic<TState>(initialState, isLocked);

        protected TState ObjectState => _stateLogic.ObjectState;

        protected TState Run(Func<Maybe<TState>, Maybe<TState>> operation) 
            => _stateLogic.Run(operation);
    }

    [PublicAPI]
    public abstract class StatefulReceiveActor<TState> : ExposedReceiveActor
    {
        private StatefulObjectLogic<TState> _stateLogic;

        protected StatefulReceiveActor(TState initialState, bool isLocked = false)
            => _stateLogic = new StatefulObjectLogic<TState>(initialState, isLocked);

        protected TState ObjectState => _stateLogic.ObjectState;

        protected TState Run(Func<Maybe<TState>, Maybe<TState>> operation) 
            => _stateLogic.Run(operation);


        protected void Receive<T>(Func<T, Maybe<TState>, Maybe<TState>> handler, Predicate<T>? shouldHandle = null)
        {
            void ActualHandler(T msg) => Run(maybe => handler(msg, maybe));
            Receive(ActualHandler, shouldHandle);
        }

        protected void Receive<T>(Predicate<T> shouldHandle, Func<T, Maybe<TState>, Maybe<TState>> handler)
            => Receive(handler, shouldHandle);
    }
}