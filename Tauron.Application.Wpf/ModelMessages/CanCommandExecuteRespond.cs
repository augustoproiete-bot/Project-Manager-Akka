using System;

namespace Tauron.Application.Wpf.ModelMessages
{
    public sealed record CanCommandExecuteRespond(string Name, Func<bool> CanExecute);
}