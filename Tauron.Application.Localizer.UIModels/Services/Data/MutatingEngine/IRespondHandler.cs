using System;

namespace Tauron.Application.Localizer.UIModels.Services.Data.MutatingEngine
{
    public interface IRespondHandler<out TData>
    {
        void Register(Action<TData> responder);
    }
}