using System;
using JetBrains.Annotations;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Wpf
{
    [PublicAPI]
    public static class ActorFlowWpfExetension
    {
        public static CommandRegistrationBuilder ToFlow<TStart>(this CommandRegistrationBuilder builder, EnterFlow<TStart> enter, Func<TStart> execute)
        {
            return builder.WithExecute(() => enter(execute()));
        }

        public static ActorFlowBuilder<TStart, CommandRegistrationBuilder> ToFlow<TStart>(this CommandRegistrationBuilder builder, TStart trigger)
        {
            return new ActorFlowBuilder<TStart, CommandRegistrationBuilder>(builder.Target, builder, flow => builder.WithExecute(() => flow(trigger)));
        }

        public static ActorFlowBuilder<TStart, CommandRegistrationBuilder> ToFlow<TStart>(this CommandRegistrationBuilder builder, Func<TStart> trigger)
        {
            return new ActorFlowBuilder<TStart, CommandRegistrationBuilder>(builder.Target, builder, flow => builder.WithExecute(() => flow(trigger())));
        }

        public static ReceiveBuilder<TRecieve, TNext, TStart, TParent> ToModel<TRecieve, TNext, TStart, TParent>(this AbastractTargetSelector<ReceiveBuilder<TRecieve, TNext, TStart, TParent>, TStart, TParent> selector, IViewModel model)
        {
            return selector.ToRef(model.Actor);
        }

        public static AyncReceiveBuilder<TRecieve, TNext, TStart, TParent> ToModel<TRecieve, TNext, TStart, TParent>(this AbastractTargetSelector<AyncReceiveBuilder<TRecieve, TNext, TStart, TParent>, TStart, TParent> selector, IViewModel model)
        {
            return selector.ToRef(model.Actor);
        }

        public static ActorFlowBuilder<TStart, TParent> ToModel<TStart, TParent>(this AbastractTargetSelector<ActorFlowBuilder<TStart, TParent>, TStart, TParent> selector, IViewModel model)
        {
            return selector.ToRef(model.Actor);
        }
    }
}