using System;
using System.ComponentModel;
using System.Globalization;
using Functional.Maybe;
using JetBrains.Annotations;

namespace Tauron
{
    [PublicAPI]
    public enum RoundType : short
    {
        None = 0,

        Hour = 60,

        HalfHour = 30,

        QuaterHour = 15
    }

    [PublicAPI]
    public static class ObjectExtension
    {
        public static void DynamicUsing(this object resource, Action action)
        {
            try
            {
                action();
            }
            finally
            {
                if (resource is IDisposable d)
                    d.Dispose();
            }
        }
        
        public static DateTime CutSecond(this DateTime source) 
            => new DateTime(source.Year, source.Month, source.Day, source.Hour, source.Minute, 0);

        public static Maybe<T> GetService<T>(this IServiceProvider provider)
            where T : class
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            return Maybe.Cast<T>(provider.GetService(typeof(T)));
        }

        public static bool IsAlive<TType>(this WeakReference<TType> reference) where TType : class
        {
            return reference.TryGetTarget(out _);
        }

        public static DateTime Round(this DateTime source, RoundType type)
        {
            if (!Enum.IsDefined(typeof(RoundType), type))
                throw new InvalidEnumArgumentException(nameof(type), (int) type, typeof(RoundType));
            if (type == RoundType.None)
                throw new ArgumentNullException(nameof(type));

            return Round(source, (double) type);
        }

        public static DateTime Round(this DateTime source, double type)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (type == 0)
                throw new ArgumentNullException(nameof(type));

            var result = source;

            var minutes = type;

            Math.DivRem(source.Minute, (int) minutes, out var modulo);

            if (modulo <= 0) return result;

            result = modulo >= minutes / 2 ? source.AddMinutes(minutes - modulo) : source.AddMinutes(modulo * -1);

            result = result.AddSeconds(source.Second * -1);

            return result;
        }

        [StringFormatMethod("format")]
        public static string SFormat(this string format, params object[] args) 
            => string.Format(CultureInfo.InvariantCulture, format, args);

        public static Maybe<TType> TypedTarget<TType>(this WeakReference<TType> reference) where TType : class 
            => (reference.TryGetTarget(out var obj) ? obj.ToMaybe() : Maybe<TType>.Nothing);
    }
}