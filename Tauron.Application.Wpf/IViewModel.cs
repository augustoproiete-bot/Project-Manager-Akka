using System;
using Akka.Actor;

namespace Tauron.Application.Wpf
{
    public interface IViewModel
    {
        IActorRef Actor { get; }

        Type ModelType { get; }

        bool IsInitialized { get; }

        public void AwaitInit(Action waiter);
    }

    public interface IViewModel<TModel> : IViewModel
    {
    }
}