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
        protected ObjectState<TState> ObjectState { get; private set; }

        protected StatefulObject(bool isLocked = false) 
            => ObjectState = isLocked ? new LockedObjectState<TState>(new object(), CreateInitialState()) : new ObjectState<TState>(CreateInitialState());

        protected abstract TState CreateInitialState();

        protected void Run(Func<Maybe<TState>, Maybe<TState>> operation)
        {
            if (ObjectState is LockedObjectState<TState> lockedObject)
            {
                lock (lockedObject.Lock)
                    ObjectState = lockedObject.Run(operation);
            }
            else
                ObjectState = ObjectState.Run(operation);
        }
    }

    [PublicAPI]
    public abstract class StatefulReceiveActor<TState> : ExposedReceiveActor
    {
        protected ObjectState<TState> ObjectState { get; private set; }

        protected StatefulReceiveActor(bool isLocked = false)
            => ObjectState = isLocked ? new LockedObjectState<TState>(new object(), CreateInitialState()) : new ObjectState<TState>(CreateInitialState());

        protected abstract TState CreateInitialState();

        protected void Run(Func<Maybe<TState>, Maybe<TState>> operation)
        {
            if (ObjectState is LockedObjectState<TState> lockedObject)
            {
                lock (lockedObject.Lock)
                    ObjectState = lockedObject.Run(operation);
            }
            else
                ObjectState = ObjectState.Run(operation);
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