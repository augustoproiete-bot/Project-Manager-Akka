using Functional.Maybe;

namespace Tauron.Application.Workshop.Mutation
{
    public interface IQuery
    {
        Maybe<string> ToHash();
    }
}