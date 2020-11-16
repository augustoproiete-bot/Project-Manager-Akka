using System.Collections.Immutable;
using Akka.Actor;
using Akka.Event;
using Functional.Maybe;
using static Tauron.Preload;

namespace Tauron.Application.Settings
{
    public sealed class SettingFile : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly ISettingProvider _provider;
        private ImmutableDictionary<string, string> _data = ImmutableDictionary<string, string>.Empty;
        private bool _isLoaded;

        public SettingFile(ISettingProvider provider)
        {
            _provider = provider;

            Receive<RequestAllValues>(RequestAllValues);
            Receive<SetSettingValue>(SetSettingValue);
        }

        private void SetSettingValue(SetSettingValue obj)
        {
            var (scope, name, value) = obj;

            Do(from _ in To(_log).Info("Set Setting Value and Save {Scope}:{Key}--{Value}", scope, name, value)
               let data = _data.SetItem(name, value)
               select _provider.Save(data),
                d => _data = d);
        }

        private void RequestAllValues(RequestAllValues obj)
        {
            Do(from isLoaded in May(_isLoaded)
               where !_isLoaded
               from _ in To(_log).Info("Load Settings Value")
               select _provider.Load(),
                dic =>
                {
                    _data = dic;
                    _isLoaded = true;
                });

            Context.Sender.Tell(_data);
        }
    }
}