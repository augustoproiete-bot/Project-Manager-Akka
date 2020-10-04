using System;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.Model
{
    [PublicAPI]
    public sealed class UIModel<TModel> : IViewModel<TModel>
    {
        private readonly UIProperty<IViewModel<TModel>> _model;

        public UIModel(UIProperty<IViewModel<TModel>> model) => _model = model;

        public IViewModel<TModel> Model => _model.Value;

        public IActorRef Actor => Model.Actor;

        public Type ModelType => Model.ModelType;

        public bool IsInitialized => Model.IsInitialized;

        public void AwaitInit(Action waiter) => Model.AwaitInit(waiter);
    }
}