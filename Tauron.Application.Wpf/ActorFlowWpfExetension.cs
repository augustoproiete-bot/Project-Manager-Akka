using System;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Wpf
{
    [PublicAPI]
    public static class ActorFlowWpfExetension
    {
        #region Command

        public static CommandRegistrationBuilder ThenFlow<TStart>(this CommandRegistrationBuilder builder, EnterFlow<TStart> enter, Func<TStart> execute)
            => builder.WithExecute(() => enter(execute()));

        public static ActorFlowBuilder<TStart, CommandRegistrationBuilder> ThenFlow<TStart>(this CommandRegistrationBuilder builder, TStart trigger)
            => new ActorFlowBuilder<TStart, CommandRegistrationBuilder>(builder.Target, builder, flow => builder.WithExecute(() => flow(trigger)));

        public static ActorFlowBuilder<TStart, CommandRegistrationBuilder> ThenFlow<TStart>(this CommandRegistrationBuilder builder, Func<TStart> trigger)
            => new ActorFlowBuilder<TStart, CommandRegistrationBuilder>(builder.Target, builder, flow => builder.WithExecute(() => flow(trigger())));

        public static ReceiveBuilder<TRecieve, TNext, TStart, TParent> ToModel<TRecieve, TNext, TStart, TParent>(this AbastractTargetSelector<ReceiveBuilder<TRecieve, TNext, TStart, TParent>, TStart, TParent> selector, IViewModel model)
            => selector.ToRef(model.Actor);

        public static AyncReceiveBuilder<TRecieve, TNext, TStart, TParent> ToModel<TRecieve, TNext, TStart, TParent>(this AbastractTargetSelector<AyncReceiveBuilder<TRecieve, TNext, TStart, TParent>, TStart, TParent> selector, IViewModel model)
            => selector.ToRef(model.Actor);

        public static ActorFlowBuilder<TStart, TParent> ToModel<TStart, TParent>(this AbastractTargetSelector<ActorFlowBuilder<TStart, TParent>, TStart, TParent> selector, IViewModel model)
            => selector.ToRef(model.Actor);

        #endregion

        #region UIProperty

        public static ActorFlowBuilder<TData, FluentPropertyRegistration<TData>> ThenFlow<TData>(this FluentPropertyRegistration<TData> prop, IExposedReceiveActor owner)
            => new ActorFlowBuilder<TData, FluentPropertyRegistration<TData>>(owner, prop, flow => prop.OnChange(v => flow(v)));

        public static ActorFlowBuilder<TConvert, FluentPropertyRegistration<TData>> ThenFlow<TData, TConvert>(this FluentPropertyRegistration<TData> prop, Func<TData, TConvert> converter,
            IExposedReceiveActor owner)
            => new ActorFlowBuilder<TConvert, FluentPropertyRegistration<TData>>(owner, prop, flow => prop.OnChange(v => flow(converter(v))));

        #endregion
    }
}
