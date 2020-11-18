using Functional.Maybe;

namespace Tauron.Application.Wpf.ModelMessages
{
    public sealed record CommandExecuteEvent(string Name, Maybe<object> Parameter);
}