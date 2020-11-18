using Functional.Maybe;

namespace Tauron.Application.Wpf.ModelMessages
{
    public sealed record SetValue(string Name, Maybe<object?> Value);
}