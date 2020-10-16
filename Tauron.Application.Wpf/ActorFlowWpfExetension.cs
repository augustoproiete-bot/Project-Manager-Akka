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

        public static CommandRegistrationBuilder ThenFlow<TStart>(this CommandRegistrationBuilder builder, TStart trigger, Action<ActorFlowBuilder<TStart>> flowBuilder)
        {
            var actorFlow = new ActorFlowBuilder<TStart>(builder.Target);

            flowBuilder(actorFlow);

             return actorFlow.OnTrigger(enterFlow => builder.WithExecute(() => enterFlow(trigger)));
        }

        public static CommandRegistrationBuilder ThenFlow<TStart>(this CommandRegistrationBuilder builder, Func<TStart> trigger, Action<ActorFlowBuilder<TStart>> flowBuilder)
        {
            var actorFlow = new ActorFlowBuilder<TStart>(builder.Target);

            flowBuilder(actorFlow);

            return actorFlow.OnTrigger(enterflow => builder.WithExecute(() => enterflow(trigger())));
        }

        public static ReceiveBuilder<TRecieve, TNext, TStart> ToModel<TRecieve, TNext, TStart>(this AbastractTargetSelector<ReceiveBuilder<TRecieve, TNext, TStart>, TStart> selector, IViewModel model) 
            => selector.ToRef(model.Actor);

        public static AyncReceiveBuilder<TRecieve, TNext, TStart> ToModel<TRecieve, TNext, TStart>(this AbastractTargetSelector<AyncReceiveBuilder<TRecieve, TNext, TStart>, TStart> selector, IViewModel model) 
            => selector.ToRef(model.Actor);

        public static ActorFlowBuilder<TStart> ToModel<TStart>(this AbastractTargetSelector<ActorFlowBuilder<TStart>, TStart> selector, IViewModel model) 
            => selector.ToRef(model.Actor);

        #endregion

        #region UIProperty

        public static FluentPropertyRegistration<TData> ThenFlow<TData>(this FluentPropertyRegistration<TData> prop, IExposedReceiveActor owner, Action<ActorFlowBuilder<TData>> flowBuilder)
        {
            var aFlow = new ActorFlowBuilder<TData>(owner);
            flowBuilder(aFlow);
            
            return aFlow.OnTrigger(ent =>  prop.OnChange(v => ent(v)));
        }

        public static FluentPropertyRegistration<TData> ThenFlow<TData, TConvert>(this FluentPropertyRegistration<TData> prop, Func<TData, TConvert> converter,
            IExposedReceiveActor owner, Action<ActorFlowBuilder<TConvert>> flowBuilder)
        {
            var aFlow = new ActorFlowBuilder<TConvert>(owner);
            flowBuilder(aFlow);

            return aFlow.OnTrigger(flow => prop.OnChange(v => flow(converter(v))));
        }

        #endregion
    }
}
