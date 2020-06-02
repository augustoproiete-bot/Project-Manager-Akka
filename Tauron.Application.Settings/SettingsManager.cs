using System.Collections.Generic;
using System.Collections.Immutable;
using Akka.Actor;
using JetBrains.Annotations;

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

        private bool GetChild(string name, out IActorRef actor)
        {
            actor = Context.Child(name);
            return actor.Equals(ActorRefs.Nobody);
        }

        private void RequestAllValues(RequestAllValues obj)
        {
            if (GetChild(obj.SettingScope, out var actor))
                Context.Sender.Tell(ImmutableDictionary<string, string>.Empty);
            else
                actor.Forward(obj);
        }

        private void SetSettingValue(SetSettingValue obj)
        {
            if (!GetChild(obj.SettingsScope, out var actor))
                actor.Forward(obj);
        }
    }
}