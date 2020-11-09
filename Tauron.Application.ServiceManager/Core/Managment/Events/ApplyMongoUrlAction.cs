using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement;

namespace Tauron.Application.ServiceManager.Core.Managment.Events
{
    public sealed class ApplyMongoUrlAction : IStateAction
    {
        public string ActionName => nameof(ApplyMongoUrlAction);
        public IQuery Query { get; } = EmptyQuery.Instance.WithHash(nameof(ApplyMongoUrlAction));

        public string Url { get; }

        public ApplyMongoUrlAction(string url) => Url = url;
    }
}