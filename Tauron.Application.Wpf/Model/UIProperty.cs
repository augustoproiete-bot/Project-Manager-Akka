using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.Model
{
    [PublicAPI]
    #pragma warning disable CS0660 // Typ definiert Operator == oder Operator !=, überschreibt jedoch nicht Object.Equals(Objekt o)
    #pragma warning disable CS0661 // Typ definiert Operator == oder Operator !=, überschreibt jedoch nicht Object.GetHashCode()
    public sealed class UIProperty<TData> : UIPropertyBase
        #pragma warning restore CS0661 // Typ definiert Operator == oder Operator !=, überschreibt jedoch nicht Object.GetHashCode()
        #pragma warning restore CS0660 // Typ definiert Operator == oder Operator !=, überschreibt jedoch nicht Object.Equals(Objekt o)
    {
        internal UIProperty(string name)
            : base(name)
        {
            PropertyValueChanged += () => PropertyValueChangedFunc?.Invoke(Value);
        }

        public TData Value
        {
            [return: MaybeNull]
            get => InternalValue is TData data ? data : default!;
        }

        public event Action<TData>? PropertyValueChangedFunc;

        public void Set([AllowNull] TData data)
        {
            SetValue(data);
        }

        [return: MaybeNull]
        public static implicit operator TData(UIProperty<TData> property) => property.Value;

        public static UIProperty<TData> operator +(UIProperty<TData> prop, TData data)
        {
            prop.Set(data);
            return prop;
        }

        public static bool operator ==(UIProperty<TData> prop, TData data) => Equals(prop.Value, data);

        public static bool operator !=(UIProperty<TData> prop, TData data) => !Equals(prop.Value, data);

        public override string ToString() => Value?.ToString() ?? "null--" + typeof(TData);
    }
}