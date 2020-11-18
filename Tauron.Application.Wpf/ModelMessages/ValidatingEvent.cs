using Functional.Maybe;

namespace Tauron.Application.Wpf.ModelMessages
{
    public sealed record ValidatingEvent(Maybe<string> Reason, string Name)
    {
        public bool Error => Reason.IsSomething();

    }
}