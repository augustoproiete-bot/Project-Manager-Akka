using Functional.Maybe;

namespace Tauron
{
    public readonly struct Unit
    {
        public static readonly Unit        Instance    = new();
        public static readonly Maybe<Unit> MayInstance = Instance.ToMaybe();
    }

    public static class UnitExtensions
    {
        public static Maybe<Unit> AsMayUnit<TType>(this TType? obj)
            => Unit.MayInstance;
        
        public static Unit AsUnit<TType>(this TType? obj)
            => Unit.Instance;
    }
}