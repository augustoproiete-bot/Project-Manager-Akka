namespace Tauron.Application.Workshop.Mutating.Changes
{
    public abstract record MutatingChange
    {
        public virtual TChange? Cast<TChange>()
            where TChange : MutatingChange
            => this as TChange;
    }
}