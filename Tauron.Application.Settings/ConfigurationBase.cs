using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
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
        private sealed class ActualConfiguration : StatefulObject<ActualConfiguration.ConfigurationState>
        {
            public sealed record ConfigurationState(IDefaultActorRef<SettingsManager> Actor, Task Loader, string Scope, ImmutableDictionary<string, string> Dic, int IsBlocked);

            public ActualConfiguration(IDefaultActorRef<SettingsManager> actor, string scope)
                : base(CreateInitialState(actor, scope), true)
                => StartLoading();

            private static ConfigurationState CreateInitialState(IDefaultActorRef<SettingsManager> actor, string scope)
            {
                return actor is EmptyActor<SettingsManager> 
                    ? new ConfigurationState(actor, Task.CompletedTask, string.Empty, ImmutableDictionary<string, string>.Empty, 0) 
                    : new ConfigurationState(actor, Task.CompletedTask, scope, ImmutableDictionary<string, string>.Empty, 0);
            }

            public IDisposable BlockSet()
            {
                Run(s =>
                    from state in s 
                    select state with{IsBlocked = state.IsBlocked + 1});
                return new ActionDispose(() => Run(s =>
                    from state in s
                    where state.IsBlocked > 0
                    select state with{IsBlocked = state.IsBlocked - 1}));
            }

            private void StartLoading()
                => Run(s => 
                    from state in s
                    select state with{ Loader = LoadValues()});

            private async Task LoadValues()
            {
                try
                {
                    if(string.IsNullOrWhiteSpace(ObjectState.Scope))
                        return;

                    var result = await ObjectState.Actor.Ask<ImmutableDictionary<string, string>>(new RequestAllValues(ObjectState.Scope));
                    Run(s => 
                        from state in s
                        select state with{Dic = result});
                }
                catch (Exception e)
                {
                    Log.Logger.ForContext(GetType()).Error(e, "Error On Load Data");
                }
            }

            public Maybe<TValue> GetValue<TValue>(Func<string, TValue> converter, TValue? defaultValue, string? name)
            {
                var result = Try(() =>
                    from _ in WaitTask(ObjectState.Loader)
                    from key in MayNotNull(name)
                    from data in ObjectState.Dic.Lookup(key)
                    select converter(data));

                return Or(Match(result, _ => May(defaultValue!)), May(defaultValue!))!;
            }

            public void SetValue(string value, string? name)
            {
                Run(s => 
                    from state in s
                    from block in May(state.IsBlocked)
                    where block == 0
                    from key in MayNotNull(name)
                    from _ in state.Actor.Tell(new SetSettingValue(state.Scope, key, value))
                    select state with{Dic = state.Dic.SetItem(key, value)});
            }
        }

        private readonly ActualConfiguration _configuration;

        protected ConfigurationBase(IDefaultActorRef<SettingsManager> actor, string scope) 
            => _configuration = new ActualConfiguration(actor, scope);

        public IDisposable BlockSet() 
            => _configuration.BlockSet();
        protected Maybe<TValue> GetValue<TValue>(Func<string, TValue> converter, TValue defaultValue = default, [CallerMemberName] string? name = null) 
            => _configuration.GetValue(converter, defaultValue, name);

        protected void SetValue(string value, [CallerMemberName] string? name = null)
        {
            _configuration.SetValue(value, name);
            OnPropertyChanged(name);
        }
    }
}