using System;
using Tauron.Akka;

namespace Tauron.Application.Wpf
{
    public interface IViewModel
    {
        Type ModelType { get; }
    }

    public interface IViewModel<TModel> : IViewModel, IDefaultActorRef<TModel>
    {

    }
}