using Akkatecture.Core;
using JetBrains.Annotations;

namespace Tauron.Akkatecture.Projections
{
    [PublicAPI]
    public interface IProjectorData<TKey>
        where TKey : IIdentity
    {
        public TKey Id { get; set; }
    }
}