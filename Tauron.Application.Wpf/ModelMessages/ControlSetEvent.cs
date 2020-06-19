using System.Windows;

namespace Tauron.Application.Wpf.ModelMessages
{
    public sealed class ControlSetEvent
    {
        public FrameworkElement Element { get; }

        public string Name { get; }

        public ControlSetEvent(FrameworkElement element, string name)
        {
            Element = element;
            Name = name;
        }
    }
}