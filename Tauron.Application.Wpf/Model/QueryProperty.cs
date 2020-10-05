using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.Model
{
    public static class QueryProperty
    {
        public static QueryProperty<TData> Create<TData>(TData data) => new QueryProperty<TData>(data);

        public static QueryProperty<TData> Create<TData>() => new QueryProperty<TData>(default!);
    }

    [PublicAPI]
    public sealed class QueryProperty<TData>
    {
        private TData _value;
        private Action? _changed;

#pragma warning disable CS8618 // Das Non-Nullable-Feld ist nicht initialisiert. Deklarieren Sie das Feld ggf. als "Nullable".
        public QueryProperty(TData value)
#pragma warning restore CS8618 // Das Non-Nullable-Feld ist nicht initialisiert. Deklarieren Sie das Feld ggf. als "Nullable".
        {
            Value = value;
        }

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
        public static implicit operator TData(QueryProperty<TData> property)
        {
            return property.Value;
        }
        
        public static bool operator ==(QueryProperty<TData> prop, TData data)
        {
            return Equals(prop.Value, data);
        }

        public static bool operator !=(QueryProperty<TData> prop, TData data)
        {
            return !Equals(prop.Value, data);
        }

        public override bool Equals(object? obj)
        {
            return obj switch
            {
                TData data => _value?.Equals(data) == true,
                QueryProperty<TData> prop => prop.Value?.Equals(Value) == true,
                _ => false
            };
        }

        public override int GetHashCode() 
            => Value?.GetHashCode() ?? 0;
    }
}