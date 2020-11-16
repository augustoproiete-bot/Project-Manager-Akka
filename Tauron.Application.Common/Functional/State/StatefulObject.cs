// ReSharper disable VirtualMemberCallInConstructor

using System;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron
{
    [PublicAPI]
    public abstract class StatefulObject<TState>
    {
        private ObjectState<TState> _objectState;

        protected TState ObjectState => _objectState.Data;


        protected StatefulObject(TState initialState, bool isLocked = false) 
            => _objectState = isLocked ? new LockedObjectState<TState>(new object(), initialState) : new ObjectState<TState>(initialState);

        protected void Run(Func<Maybe<TState>, Maybe<TState>> operation)
        {
            if (_objectState is LockedObjectState<TState> lockedObject)
            {
                lock (lockedObject.Lock)
                    _objectState = lockedObject.Run(operation);
            }
            else
                _objectState = _objectState.Run(operation);
        }
    }

    [PublicAPI]
    public abstract class StatefulReceiveActor<TState> : ExposedReceiveActor
    {
        private ObjectState<TState> _objectState;

        protected TState ObjectState => _objectState.Data;

        protected StatefulReceiveActor(TState initialState, bool isLocked = false)
            => _objectState = isLocked ? new LockedObjectState<TState>(new object(), initialState) : new ObjectState<TState>(initialState);

        protected TState Run(Func<Maybe<TState>, Maybe<TState>> operation)
        {
            if (_objectState is LockedObjectState<TState> lockedObject)
            {
                lock (lockedObject.Lock)
                    _objectState = lockedObject.Run(operation);
            }
            else
                _objectState = _objectState.Run(operation);

            return _objectState.Data;
        }


        protected void Receive<T>(Func<T, Maybe<TState>, Maybe<TState>> handler, Predicate<T>? shouldHandle = null)
        {
            void ActualHandler(T msg) => Run(maybe => handler(msg, maybe));
            Receive(ActualHandler, shouldHandle);
        }

        protected void Receive<T>(Predicate<T> shouldHandle, Func<T, Maybe<TState>, Maybe<TState>> handler) 
            => Receive(handler, shouldHandle);
    }
}