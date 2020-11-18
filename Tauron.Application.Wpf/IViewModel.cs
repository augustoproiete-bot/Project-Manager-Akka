using System;
using Akka.Actor;
using Functional.Maybe;

namespace Tauron.Application.Wpf
{
    public interface IViewModel
    {
        Maybe<IActorRef> Actor { get; }

        Type ModelType { get; }

        bool IsInitialized { get; }

        public void AwaitInit(Action waiter);
    }

    public interface IViewModel<TModel> : IViewModel
    {
    }
}