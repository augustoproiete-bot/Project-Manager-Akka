using System.Diagnostics.CodeAnalysis;
using Amadevus.RecordGenerator;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.ModelMessages
{
    [Record]
    public sealed partial class GetValueRequest
    {
        public string Name { get; }
    }

    [Record]
    [PublicAPI]
    public sealed partial class GetValueResponse
    {
        public string Name { get; }

        public object? Value { get; }

        [return: MaybeNull]
        public TValue TryCast<TValue>()
        {
            if (Value is TValue value)
                return value;
            return default!;
        }
    }
}