using System;
using Tauron.Application.Wpf.Helper;

namespace Tauron.Application.Wpf
{
    public interface IView : IBinderControllable
    {
        string Key { get; }

        event Action ControlUnload;
    }
}