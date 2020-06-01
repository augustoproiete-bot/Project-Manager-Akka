using System;

namespace Tauron.Application.Workshop.Mutation
{
    public interface IRespondHandler<out TData>
    {
        void Register(Action<TData> responder);
    }
}