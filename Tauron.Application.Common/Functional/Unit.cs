using Functional.Maybe;

namespace Tauron
{
    public struct Unit
    {
        public static readonly Unit Instance = new();
        public static readonly Maybe<Unit> MayInstance = Instance.ToMaybe();
    }
}