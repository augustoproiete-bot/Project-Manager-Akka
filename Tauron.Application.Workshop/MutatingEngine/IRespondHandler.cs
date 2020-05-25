using System;

namespace Tauron.Application.Workshop.MutatingEngine
{
    public interface IRespondHandler<out TData>
    {
        void Register(Action<TData> responder);
    }
}