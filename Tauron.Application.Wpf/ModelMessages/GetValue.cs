using System.Diagnostics.CodeAnalysis;
using Functional.Maybe;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.ModelMessages
{
    public sealed record GetValueRequest(string Name);

    [PublicAPI]
    public sealed record GetValueResponse(string Name, Maybe<object?> Value)
    {
        [return: MaybeNull]
        public TValue TryCast<TValue>()
        {
            if (Value.OrElseDefault() is TValue value)
                return value;
            return default!;
        }
    }
}