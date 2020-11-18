using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.Model
{
    public static class QueryProperty
    {
        public static ReadonlyQueryProperty<TData> Create<TData>(Action<Action<TData>> setter) => new(setter);

        public static QueryProperty<TData> Create<TData>(TData data) => new(data);

        public static QueryProperty<TData> Create<TData>() => new(default!);
    }

    [PublicAPI]
    public sealed class ReadonlyQueryProperty<TData> : IQueryProperty<TData>
    {
        private Action? _changed;

        internal ReadonlyQueryProperty(Action<Action<TData>> setter)
        {
            setter(data =>
                   {
                       Value = data;
                       _changed?.Invoke();
                   });
        }

        public TData Value { get; private set; } = default!;

        public void NotifyChanged(Action action)
            => _changed = _changed.Combine(action);

        [return: MaybeNull]
        public static implicit operator TData(ReadonlyQueryProperty<TData> property) => property.Value;

        public static bool operator ==(ReadonlyQueryProperty<TData> prop, TData data) => Equals(prop.Value, data);

        public static bool operator !=(ReadonlyQueryProperty<TData> prop, TData data) => !Equals(prop.Value, data);

        public override bool Equals(object? obj)
        {
            return obj switch
            {
                TData data => Value?.Equals(data)                              == true,
                ReadonlyQueryProperty<TData> prop => prop.Value?.Equals(Value) == true,
                _ => false
            };
        }

        public override int GetHashCode()
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            => Value?.GetHashCode() ?? 0;
    }

    public interface IQueryProperty<out TData>
    {
        TData Value { get; }
        void  NotifyChanged(Action action);
    }

    [PublicAPI]
    public sealed class QueryProperty<TData> : IQueryProperty<TData>
    {
        private Action? _changed;
        private TData   _value = default!;

        internal QueryProperty(TData value)
            => Value = value;

        public TData Value
        {
            get => _value;
            set
            {
                _value = value;
                _changed?.Invoke();
            }
        }

        public void NotifyChanged(Action action)
            => _changed = _changed.Combine(action);

        [return: MaybeNull]
        public static implicit operator TData(QueryProperty<TData> property) => property.Value;

        public static bool operator ==(QueryProperty<TData> prop, TData data) => Equals(prop.Value, data);

        public static bool operator !=(QueryProperty<TData> prop, TData data) => !Equals(prop.Value, data);

        public override bool Equals(object? obj)
        {
            return obj switch
            {
                TData data => _value?.Equals(data)                     == true,
                QueryProperty<TData> prop => prop.Value?.Equals(Value) == true,
                _ => false
            };
        }

        public override int GetHashCode()
            => Value?.GetHashCode() ?? 0;
    }
}