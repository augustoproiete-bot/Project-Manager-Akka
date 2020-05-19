using System;

namespace Tauron.Application.Localizer.DataModel.Workspace.MutatingEngine
{
    public interface IRespondHandler<out TData>
    {
        void Register(Action<TData> responder);
    }
}