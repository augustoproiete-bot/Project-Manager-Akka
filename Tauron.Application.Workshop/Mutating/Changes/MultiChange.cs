using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.Mutating.Changes
{
    [PublicAPI]
    public sealed record MultiChange(ImmutableList<MutatingChange> Changes) : MutatingChange
    {
        public override TChange Cast<TChange>()
        {
            foreach (var change in Changes)
            {
                if (change is TChange c)
                    return c;
            }

            return null!;
        }
    }
}