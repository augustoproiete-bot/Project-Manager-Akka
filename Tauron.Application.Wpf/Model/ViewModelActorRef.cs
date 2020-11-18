using System;
using System.Collections.Immutable;
using System.Threading;
using Akka.Actor;
using Functional.Maybe;
using static Tauron.Prelude;

namespace Tauron.Application.Wpf.Model
{
    public abstract class ViewModelActorRef : StatefulObject<ViewModelActorRef.ViewModelActorRefState>, IViewModel
    {
        public sealed record ViewModelActorRefState(Maybe<IActorRef> Actor, bool IsInitialized, ImmutableList<Action>? Waiter);

        protected ViewModelActorRef()
            : base(new ViewModelActorRefState(Maybe<IActorRef>.Nothing, false, ImmutableList<Action>.Empty), true)
        { }

        public abstract   Maybe<IActorRef> Actor         { get; }
        public abstract   Type      ModelType     { get; }
        public abstract   bool      IsInitialized { get; }
        public abstract   void      AwaitInit(Action waiter);
        internal abstract Maybe<Unit>   Init(IActorRef   actor);
    }

    public sealed class ViewModelActorRef<TModel> : ViewModelActorRef, IViewModel<TModel>
        where TModel : UiActor
    {
       public override Maybe<IActorRef> Actor => ObjectState.Actor;

        public override Type ModelType => typeof(TModel);

        public override bool IsInitialized => ObjectState.IsInitialized;

        public override void AwaitInit(Action waiter)
        {
            Run(s =>
                {
                    var initialized =
                        from state in s
                        where state.IsInitialized
                        select Unit.Instance;

                    return Match(initialized,
                                 u =>
                                     from state in s
                                     from _ in MayUse(waiter)
                                     select state,
                                 () => May(from state in s
                                           where state.Waiter != null
                                           select state with{Waiter = state.Waiter.Add(waiter)}));
                });
        }

        internal override Maybe<Unit> Init(IActorRef actor)
        {
            return Run(s =>
                           from state in s
                           from _ in MayUse(() => state?.Waiter.ForEach(a => a()))
                           select state with{Waiter = null, IsInitialized = true})
               .AsMayUnit();
        }
    }
}