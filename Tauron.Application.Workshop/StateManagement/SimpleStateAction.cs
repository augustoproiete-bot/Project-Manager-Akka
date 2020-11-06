using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement
{
    [PublicAPI]
    public abstract class SimpleStateAction : IStateAction
    {
        public virtual string ActionName => GetType().Name;
        public virtual IQuery Query => EmptyQuery.Instance.WithHash(GetType().Name);
    }
}