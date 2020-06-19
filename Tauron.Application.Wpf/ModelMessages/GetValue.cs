using System.Diagnostics.CodeAnalysis;
using Amadevus.RecordGenerator;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.ModelMessages
{
    public sealed class GetValueRequest
    {
        public string Name { get; }

        public GetValueRequest(string name) => Name = name;
    }

    [PublicAPI]
    public sealed class GetValueResponse
    {
        public string Name { get; }

        public object? Value { get; }

        public GetValueResponse(string name, object? value)
        {
            Name = name;
            Value = value;
        }

        [return: MaybeNull]
        public TValue TryCast<TValue>()
        {
            if (Value is TValue value)
                return value;
            return default!;
        }
    }
}