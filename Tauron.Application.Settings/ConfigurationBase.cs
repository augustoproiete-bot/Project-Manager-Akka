using System;
using System.Collections.Immutable;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Functional.Maybe;
using JetBrains.Annotations;
using Serilog;
using Tauron.Akka;
using static Tauron.Preload;

namespace Tauron.Application.Settings
{
    [PublicAPI]
    public abstract class ConfigurationBase : ObservableObject
    {
        private readonly IDefaultActorRef<SettingsManager> _actor;
        private readonly Task _loader;
        private readonly string _scope;

        private ImmutableDictionary<string, string> _dic = ImmutableDictionary<string, string>.Empty;

        private bool _isBlocked;

        protected ConfigurationBase(IDefaultActorRef<SettingsManager> actor, string scope)
        {
            _actor = actor;

            if (actor is EmptyActor<SettingsManager>)
            {
                _scope = string.Empty;
                _loader = Task.CompletedTask;
            }
            else
            {
                _scope = scope;
                _loader = Task.Run(async () => await LoadValues());
            }
        }

        public IDisposable BlockSet()
        {
            _isBlocked = true;
            return new ActionDispose(() => _isBlocked = false);
        }

        private async Task LoadValues()
        {
            try
            {
                var result = await _actor.Ask<ImmutableDictionary<string, string>>(new RequestAllValues(_scope));
                Interlocked.Exchange(ref _dic, result);
            }
            catch (Exception e)
            {
                Log.Logger.ForContext(GetType()).Error(e, "Error On Load Data");
            }
        }

        protected Maybe<TValue> GetValue<TValue>(Func<string, TValue> converter, TValue defaultValue = default, [CallerMemberName] string? name = null)
        {
            var result = Try(() =>
                from _ in WaitTask(_loader)
                from key in MayNotNull(name)
                from data in _dic.Lookup(key)
                select converter(data));

            return Or(Match(result, _ => May(defaultValue)), May(defaultValue))!;
        }

        protected void SetValue(string value, [CallerMemberName] string? name = null)
        {
            Do(from block in May(_isBlocked)
               where !block
               from key in MayNotNull(name)
               from _ in May(ImmutableInterlocked.AddOrUpdate(ref _dic, key, value, (_, _) => value))
               from _ in _actor.Tell(new SetSettingValue(_scope, key, value))
               select MayOnPropertyChanged(key));
        }
    }
}