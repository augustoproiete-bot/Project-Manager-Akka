using Functional.Maybe;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement
{
    public sealed record EmptyQuery(Maybe<string> Hash) : IQuery
    {
        public static readonly EmptyQuery Instance = new(Maybe<string>.Nothing);
        
        public Maybe<string> ToHash() => Hash.Or(nameof(EmptyQuery));

    }
}