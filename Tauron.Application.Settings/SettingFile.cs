using System.Collections.Immutable;
using Akka.Actor;
using Akka.Event;

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
            _log.Info("Set Setting Value and Save {Scope}:{Key}--{Value}", obj.SettingsScope, obj.Name, obj.Value);
            _data = _data.SetItem(obj.Name, obj.Value);
            _provider.Save(_data);
        }

        private void RequestAllValues(RequestAllValues obj)
        {
            if (!_isLoaded)
            {
                _log.Info("Load Settings Value");
                _data = _provider.Load();
                _isLoaded = true;
            }

            Context.Sender.Tell(_data);
        }
    }
}