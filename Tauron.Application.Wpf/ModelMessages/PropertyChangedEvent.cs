using Functional.Maybe;

namespace Tauron.Application.Wpf.ModelMessages
{
    public sealed record PropertyChangedEvent(string Name, Maybe<object?> Value);
}