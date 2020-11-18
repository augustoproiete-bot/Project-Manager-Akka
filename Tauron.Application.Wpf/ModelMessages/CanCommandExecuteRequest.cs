
using Functional.Maybe;

namespace Tauron.Application.Wpf.ModelMessages
{
    public sealed record CanCommandExecuteRequest(string Name, Maybe<object> Parameter);
}