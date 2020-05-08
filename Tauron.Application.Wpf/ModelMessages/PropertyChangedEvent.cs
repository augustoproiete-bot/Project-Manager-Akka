using Amadevus.RecordGenerator;

namespace Tauron.Application.Wpf.ModelMessages
{
    [Record]
    public sealed partial class PropertyChangedEvent
    {
        public string Name { get; }

        public object Value { get; }
    }
}