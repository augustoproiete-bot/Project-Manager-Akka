using System;
using Tauron.Application.Wpf.Helper;
using Tauron.Application.Wpf.UI;

namespace Tauron.Application.Wpf
{
    public interface IView : IBinderControllable
    {
        string Key { get; }

        ViewManager ViewManager { get; }

        event Action? ControlUnload;
    }
}