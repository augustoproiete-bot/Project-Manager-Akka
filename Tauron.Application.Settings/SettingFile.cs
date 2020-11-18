using System.Collections.Immutable;
using Functional.Maybe;
using static Tauron.Prelude;

namespace Tauron.Application.Settings
{
    public sealed class SettingFile : StatefulReceiveActor<SettingFile.State>
    {
        public record State(ISettingProvider Provider, ImmutableDictionary<string, string> Data, bool IsLoaded);

        public SettingFile(ISettingProvider provider)
            : base(new State(provider, ImmutableDictionary<string, string>.Empty, false))
        {
            Receive<RequestAllValues>(RequestAllValues);
            Receive<SetSettingValue>(SetSettingValue);
        }

        private Maybe<State> SetSettingValue(SetSettingValue obj, Maybe<State> mayState)
        {
            var (scope, name, value) = obj;

            return 
                from state in mayState
                from _ in To(Log).Info("Set Setting Value and Save {Scope}:{Key}--{Value}", scope, name, value)
                from data in state.Provider.Save(state.Data.SetItem(name, value))
                select state with{Data = data};
        }

        private void RequestAllValues(RequestAllValues obj)
        {
            Tell(Context.Sender,
            Run(s =>
                from state in s 
                where !state.IsLoaded
                from _ in To(Log).Info("Load Settings Value")
                from data in state.Provider.Load() 
                select state with{Data = data, IsLoaded = true})
                .Data);
        }
    }
}