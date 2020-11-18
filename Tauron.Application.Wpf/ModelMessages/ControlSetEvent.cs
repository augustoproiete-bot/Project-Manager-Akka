using System.Windows;

namespace Tauron.Application.Wpf.ModelMessages
{
    public sealed record ControlSetEvent(FrameworkElement Element, string Name);

}