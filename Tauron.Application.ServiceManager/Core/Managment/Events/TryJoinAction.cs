using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement;

namespace Tauron.Application.ServiceManager.Core.Managment.Events
{
    public sealed class TryJoinAction : IStateAction
    {
        public string ActionName => nameof(TryJoinAction);
        public IQuery Query { get; } = EmptyQuery.Instance.WithHash(nameof(TryJoinAction));
    }
}