using System.Collections.Generic;
using System.Linq;

namespace Tauron.Application.Workshop.Mutating.Changes
{
    public sealed class MultiChange : MutatingChange
    {
        public List<MutatingChange> Changes { get; } = new List<MutatingChange>();

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