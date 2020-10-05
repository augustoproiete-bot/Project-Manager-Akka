using System;
using System.Collections.Concurrent;
using System.Linq;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.Model
{
    public abstract class CommandQuery
    {
        protected Action<bool>? Monitors { get; private set; }

        public void Monitor(Action<bool> waiter) => Monitors = Monitors.Combine(waiter);

        public abstract bool Run();
    }

    [PublicAPI]
    public sealed class CommandQueryBuilder
    {
        public static readonly CommandQueryBuilder Instance = new CommandQueryBuilder();

        public CommandQuery Combine(params CommandQuery[] queries)
            => new CombineCommandQuery(queries);

        public CommandQuery FromProperty<TData>(QueryProperty<TData> prop, Func<TData, bool> check)
            => new QueryPropertyCommandQuery<TData>(prop, check);

        public CommandQuery FromProperty(QueryProperty<bool> prop)
            => new QueryPropertyCommandQuery<bool>(prop, b => b);

        public CommandQuery NotNull<TData>(QueryProperty<TData?> prop)
            where TData : class
            => new QueryPropertyCommandQuery<TData?>(prop, d => d != null);

        public CommandQuery FromProperty<TData>(UIProperty<TData> prop, Func<TData, bool> check)
            => new UIPropertyCommandQuery<TData>(prop, check);

        public CommandQuery FromProperty(UIProperty<bool> prop)
            => new UIPropertyCommandQuery<bool>(prop, b => b);

        public CommandQuery FromExternal<TData>(Func<TData, bool> check, Action<Action<TData>> registrar, TData value = default)
            => new ExternalCommandQuery<TData>(value, registrar, check);

        public CommandQuery FromTrigger(Func<bool> check, out Action trigger)
        {
            var trig = new TriggerCommandQuery(check);
            trigger = trig.Trigger;
            return trig;
        }

        public sealed class CombineCommandQuery : CommandQuery
        {
            private readonly CommandQuery[] _queries;
            private readonly ConcurrentDictionary<CommandQuery, bool> _states = new ConcurrentDictionary<CommandQuery, bool>();

            public CombineCommandQuery(CommandQuery[] queries)
            {
                _queries = queries;

                foreach (var query in queries)
                {
                    _states[query] = Run();
                    query.Monitor(b => QueryChanged(query, b));
                }
            }

            private void QueryChanged(CommandQuery query, bool state)
            {
                _states.AddOrUpdate(query, state, (c, b) => state);
                Monitors?.Invoke(_states.Values.All(p => p));
            }


            public override bool Run() 
                => _queries.All(c => c.Run());
        }

        public sealed class QueryPropertyCommandQuery<TData> : CommandQuery
        {
            private readonly QueryProperty<TData> _property;
            private readonly Func<TData, bool> _check;

            public QueryPropertyCommandQuery(QueryProperty<TData> property, Func<TData, bool> check)
            {
                _property = property;
                _property.NotifyChanged(() => Monitors?.Invoke(check(_property.Value)));
                _check = check;
            }

            public override bool Run() => _check(_property.Value);
        }

        public sealed class UIPropertyCommandQuery<TData> : CommandQuery
        {
            private readonly UIProperty<TData> _property;
            private readonly Func<TData, bool> _check;

            public UIPropertyCommandQuery(UIProperty<TData> property, Func<TData, bool> check)
            {
                _property = property;
                _property.PropertyValueChanged += () => Monitors?.Invoke(check(_property.Value));
                _check = check;
            }

            public override bool Run() => _check(_property.Value);
        }

        public sealed class ExternalCommandQuery<TData> : CommandQuery
        {
            private TData _currentValue;
            private readonly Func<TData, bool> _check;

            public ExternalCommandQuery(TData currentValue, Action<Action<TData>> registrar, Func<TData, bool> check)
            {
                _currentValue = currentValue;
                _check = check;

                registrar(data =>
                {
                    _currentValue = data;
                    Monitors?.Invoke(_check(_currentValue));
                });
            }

            public override bool Run() => _check(_currentValue);
        }

        public sealed class TriggerCommandQuery : CommandQuery
        { 
            private readonly Func<bool> _check;

            public TriggerCommandQuery(Func<bool> check)
            {
                _check = check;
            }

            public void Trigger() 
                => Monitors?.Invoke(_check());

            public override bool Run() => _check();
        }
    }
}