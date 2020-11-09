using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Tauron.Application.Workshop.Mutating.Changes
{
    public sealed class MultiChange : MutatingChange
    {
        public ImmutableList<MutatingChange> Changes { get; }

        public MultiChange(ImmutableList<MutatingChange> changes) => Changes = changes;

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