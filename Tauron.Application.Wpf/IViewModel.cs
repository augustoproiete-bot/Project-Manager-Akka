using System;
using Tauron.Akka;

namespace Tauron.Application.Wpf
{
    public interface IViewModel : IInitableActorRef
    {
        Type ModelType { get; }

        bool IsInitialized { get; }
        event Action? Initialized;

        void Reset();
    }

    public interface IViewModel<TModel> : IViewModel, IDefaultActorRef<TModel>
    {
    }
}