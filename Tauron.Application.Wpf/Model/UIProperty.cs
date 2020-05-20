using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.Model
{
    [PublicAPI]
    public sealed class UIProperty<TData> : UIPropertyBase
    {
        public TData Value
        {
            [return: MaybeNull] get => InternalValue is TData data ? data : default!;
        }

        public void Set([AllowNull] TData data)
            => SetValue(data);

        [return: MaybeNull]
        public static implicit operator TData(UIProperty<TData> property)
            => property.Value;

        internal UIProperty(string name)
            : base(name)
        {
        }

        public static UIProperty<TData> operator +(UIProperty<TData> prop, TData data)
        {
            prop.Set(data);
            return prop;
        }

        public override string ToString() => Value?.ToString() ?? "null--" + typeof(TData);
    }
}