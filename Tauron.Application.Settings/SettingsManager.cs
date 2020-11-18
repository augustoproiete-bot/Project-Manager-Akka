using System.Collections.Generic;
using System.Collections.Immutable;
using Akka.Actor;
using Functional.Maybe;
using JetBrains.Annotations;
using static Tauron.Prelude;

namespace Tauron.Application.Settings
{
    [PublicAPI]
    public sealed class SettingsManager : ReceiveActor
    {
        public SettingsManager(IEnumerable<ISettingProviderConfiguration> configurations)
        {
            foreach (var configuration in configurations)
                Context.ActorOf(Props.Create(() => new SettingFile(configuration.Provider)), configuration.Scope);

            Receive<SetSettingValue>(SetSettingValue);
            Receive<RequestAllValues>(RequestAllValues);
        }

        private Maybe<IActorRef> GetChild(string name)
            => MayActor(Context.Child(name));


        private void RequestAllValues(RequestAllValues obj)
            => Match(GetChild(obj.SettingScope),
                actor => actor.Forward(obj),
                () => Context.Sender.Tell(ImmutableDictionary<string, string>.Empty));

        private void SetSettingValue(SetSettingValue obj)
            => Do(from actor in GetChild(obj.SettingsScope)
                  select Forward(actor, obj));
    }
}