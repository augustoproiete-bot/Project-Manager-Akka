using System;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.AppCore
{
    [PublicAPI]
    public interface IMainWindow : IWindowProvider
    {
        event EventHandler? Shutdown;
    }
}