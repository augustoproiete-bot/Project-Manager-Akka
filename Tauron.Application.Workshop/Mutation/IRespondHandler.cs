using System;
using Functional.Maybe;

namespace Tauron.Application.Workshop.Mutation
{
    public interface IRespondHandler<TData>
    {
        void Register(Action<Maybe<TData>> responder);
    }
}