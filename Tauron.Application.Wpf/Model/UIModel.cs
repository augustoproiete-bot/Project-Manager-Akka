using System;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.Model
{
    [PublicAPI]
    public sealed class UIModel<TModel> : IViewModel<TModel>
    {
        private readonly UIProperty<IViewModel<TModel>> _model;


        public UIModel(UIProperty<IViewModel<TModel>> model)
        {
            _model = model;
        }

        public IViewModel<TModel> Model => _model.Value;

        public IActorRef Actor => Model.Actor;

        public void Init(string? name = null)
        {
            Model.Init(name);
        }

        public void Init(IActorRefFactory factory, string? name = null)
        {
            Model.Init(factory, name);
        }

        public event Action? Initialized
        {
            add => Model.Initialized += value;
            remove => Model.Initialized -= value;
        }

        public Type ModelType => Model.ModelType;

        public bool IsInitialized => Model.IsInitialized;

        void IViewModel.Reset()
        {
            Model.Reset();
        }
    }
}