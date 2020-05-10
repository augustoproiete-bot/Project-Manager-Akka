using System;
using Akka.Actor;
using Tauron.Akka;

namespace Tauron.Application.Wpf
{
    public interface IViewModel : IInitableActorRef
    {
        event Action? Initialized;

        Type ModelType { get; }

        bool IsInitialized { get; }

        void Reset();
    }

    public interface IViewModel<TModel> : IViewModel, IDefaultActorRef<TModel>
    {

    }
}