namespace Tauron.Application.Workshop.Mutating.Changes
{
    public interface ICanApplyChange<TData>
    {
        TData Apply(MutatingChange apply);
    }
}