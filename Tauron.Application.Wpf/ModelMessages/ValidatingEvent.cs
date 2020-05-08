using Amadevus.RecordGenerator;

namespace Tauron.Application.Wpf.ModelMessages
{
    [Record]
    public sealed partial class ValidatingEvent
    {
        public string? Reason { get; }

        public string Name { get; }

        public bool Error => !string.IsNullOrWhiteSpace(Reason);
    }
}