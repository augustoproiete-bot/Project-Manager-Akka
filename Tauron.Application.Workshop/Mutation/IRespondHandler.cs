using System;
using Functional.Maybe;

namespace Tauron.Application.Workshop.Mutation
{
    public interface IRespondHandler<out TData>
    {
        void Register(Action<Maybe<TData>> responder);
    }
}