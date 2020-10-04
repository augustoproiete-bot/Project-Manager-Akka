using System;
using System.Collections.Generic;
using System.Threading;
using Akka.Actor;
using Akka.Util;

namespace Tauron.Application.Wpf.Model
{
    public abstract class ViewModelActorRef : IViewModel
    {
        public abstract IActorRef Actor { get; }
        public abstract Type ModelType { get; }
        public abstract bool IsInitialized { get; }
        public abstract void AwaitInit(Action waiter);
        internal abstract void Init(IActorRef actor);
    }

    public sealed class ViewModelActorRef<TModel> : ViewModelActorRef,  IViewModel<TModel>
        where TModel : UiActor
    {
        private readonly AtomicBoolean _isInitialized = new AtomicBoolean();

        private List<Action>? _waiter = new List<Action>();
        private  IActorRef _actor = ActorRefs.Nobody;

        public override IActorRef Actor => _actor;

        public override Type ModelType => typeof(TModel);

        public override bool IsInitialized => _isInitialized.Value;

        public override void AwaitInit(Action waiter)
        {
            lock (this)
            {
                if (IsInitialized)
                    waiter();
                else
                    _waiter!.Add(waiter);
            }
        }

        internal override void Init(IActorRef actor)
        {
            Interlocked.Exchange(ref _actor, actor);
            _isInitialized.GetAndSet(true);

            lock (this)
            {
                foreach (var action in _waiter!) 
                    action();

                _waiter = null;
            }
        }
    }
}