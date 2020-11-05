using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement
{
    public sealed class EmptyQuery : IQuery
    {
        public static readonly IQuery Instance = new EmptyQuery();

        public string ToHash() => nameof(EmptyQuery);
    }
}