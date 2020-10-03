using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiquidProjections;

namespace Tauron.Akkatecture.Projections
{
    public class DomainProjector
    {
        protected internal readonly Projector Projector;

        public DomainProjector(IEventMap<ProjectionContext> map, IEnumerable<Projector>? children = null)
        {
            Projector = new Projector(map, children) {ShouldRetry = ShouldRetry};
        }

        protected virtual async Task<bool> ShouldRetry(ProjectionException exception, int attempts)
        {
            await Task.Delay((int)Math.Pow(2d, attempts));

            return (attempts < 3);
        }
    }
}