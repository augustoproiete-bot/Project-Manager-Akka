using System;
using Akka.Actor;
using Tauron.Akka;

namespace Tauron.Application.Wpf
{
    public interface IViewModel : IActorRef, IInitableActorRef
    {
        Type ModelType { get; }

        bool IsInitialized { get; }
    }

    public interface IViewModel<TModel> : IViewModel, IDefaultActorRef<TModel>
    {

    }
}