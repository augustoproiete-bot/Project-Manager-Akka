using Functional.Maybe;

namespace Tauron.Application.Workshop.Mutating.Changes
{
    public interface ICanApplyChange<out TData>
    {
        TData Apply(MutatingChange apply);
    }
}