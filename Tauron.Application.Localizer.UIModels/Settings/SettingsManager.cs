using System;
using System.Collections.Immutable;
using Akka.Actor;
using Tauron.Application.Localizer.UIModels.Settings.Provider;

namespace Tauron.Application.Localizer.UIModels.Settings
{
    public sealed class SettingsManager : ReceiveActor
    {
        public SettingsManager()
        {
            void Create(string name)
            {
                ISettingProvider provider = name switch
                {
                    SettingTypes.AppConfig => new JsonProvider("appconfig.json"),
                    _ => throw new InvalidOperationException("Unkowen Provider SettingsScope")
                };

                Context.ActorOf(Props.Create(() => new SettingFile(provider)), name);
            }

            Create(SettingTypes.AppConfig);

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
            if(GetChild(obj.SettingScope, out var actor))
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