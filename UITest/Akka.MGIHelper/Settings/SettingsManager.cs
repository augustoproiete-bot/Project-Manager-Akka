using System;
using System.Collections.Immutable;
using Akka.Actor;
using Akka.MGIHelper.Settings.Provider;

namespace Akka.MGIHelper.Settings
{
    public sealed class SettingsManager : ReceiveActor
    {
        public SettingsManager()
        {
            void Create(string name)
            {
                ISettingProvider provider = name switch
                {
                    SettingTypes.FanControlOptions => new JsonProvider("fancontrol.json"),
                    SettingTypes.WindowOptions => new JsonProvider("window.json"),
                    SettingTypes.ProcessOptions => new XmlProvider(),
                    _ => throw new InvalidOperationException("Unkowen Provider SettingsScope")
                };

                Context.ActorOf(Props.Create(() => new SettingFile(provider)), name);
            }

            Create(SettingTypes.ProcessOptions);
            Create(SettingTypes.WindowOptions);
            Create(SettingTypes.FanControlOptions);

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