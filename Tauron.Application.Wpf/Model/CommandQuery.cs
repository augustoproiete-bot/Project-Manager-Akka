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

    public sealed class CommandTrigger
    {
        private Action? _watcher;

        internal void Register(Action watcher) => _watcher = _watcher.Combine(watcher);

        public void Trigger() => _watcher?.Invoke();
    }

    [PublicAPI]
    public sealed class CommandQueryBuilder
    {
        public static readonly CommandQueryBuilder Instance = new CommandQueryBuilder();

        public CommandQuery Combine(params CommandQuery[] queries)
            => new CombineCommandQuery(queries);

        public CommandQuery FromProperty<TData>(IQueryProperty<TData> prop, Func<TData, bool> check)
            => new QueryPropertyCommandQuery<TData>(prop, check);

        public CommandQuery FromProperty(IQueryProperty<bool> prop)
            => new QueryPropertyCommandQuery<bool>(prop, b => b);

        public CommandQuery NotNull<TData>(IQueryProperty<TData?> prop)
            where TData : class
            => new QueryPropertyCommandQuery<TData?>(prop, d => d != null);

        public CommandQuery FromProperty<TData>(UIProperty<TData> prop, Func<TData, bool> check)
            => new UIPropertyCommandQuery<TData>(prop, check);

        public CommandQuery FromProperty(UIProperty<bool> prop)
            => new UIPropertyCommandQuery<bool>(prop, b => b);

        public CommandQuery FromExternal<TData>(Func<TData, bool> check, Action<Action<TData>> registrar, TData value = default)
            => new ExternalCommandQuery<TData>(value!, registrar, check);

        public CommandQuery And(params CommandQuery[] queries)
            => new CompareQuery(queries, QueryCompareType.And);

        public CommandQuery Or(params CommandQuery[] queries)
            => new CompareQuery(queries, QueryCompareType.Or);

        public CommandQuery FromTrigger(Func<bool> check, out Action trigger)
        {
            var trig = new TriggerCommandQuery(check);
            trigger = trig.Trigger;
            return trig;
        }

        public CommandQuery FromTrigger(Func<bool> check, CommandTrigger trigger)
        {
            var trig = new TriggerCommandQuery(check);
            trigger.Register(trig.Trigger);
            return trig;
        }

        public enum QueryCompareType
        {
            And,
            Or
        }

        public sealed class CompareQuery : CommandQuery
        {
            private readonly QueryCompareType _type;
            private readonly CommandQuery[] _queries;
            private readonly bool[] _state;

            public CompareQuery(CommandQuery[] queries, QueryCompareType type)
            {
                _queries = queries;
                _type = type;

                _state = new bool[queries.Length];

                for (var i = 0; i < queries.Length; i++)
                {
                    _state[i] = queries[i].Run();
                    var stateIndex = i;

                    queries[i].Monitor(b =>
                    {
                        _state[stateIndex] = b;
                        Update();
                    });
                }
            }

            private void Update()
            {
                lock (this)
                {
                    switch (_type)
                    {
                        case QueryCompareType.And:
                            Monitors?.Invoke(_state.All(c => c));
                            break;
                        case QueryCompareType.Or:
                            Monitors?.Invoke(_state.Any(c => c));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            public override bool Run()
            {
                return _type switch
                {
                    QueryCompareType.And => _queries.All(c => c.Run()),
                    QueryCompareType.Or => _queries.Any(c => c.Run()),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
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
            private readonly IQueryProperty<TData> _property;
            private readonly Func<TData, bool> _check;

            public QueryPropertyCommandQuery(IQueryProperty<TData> property, Func<TData, bool> check)
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