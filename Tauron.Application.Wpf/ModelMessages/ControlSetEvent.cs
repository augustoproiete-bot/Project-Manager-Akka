using System.Windows;
using Amadevus.RecordGenerator;

namespace Tauron.Application.Wpf.ModelMessages
{
    [Record]
    public sealed partial class ControlSetEvent
    {
        public FrameworkElement Element { get; }

        public string Name { get; }
    }
}