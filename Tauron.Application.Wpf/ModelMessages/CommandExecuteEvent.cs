using Amadevus.RecordGenerator;

namespace Tauron.Application.Wpf.ModelMessages
{
    [Record]
    public sealed partial class CommandExecuteEvent
    {
        public string Name { get; }

        public object? Parameter { get; }
    }
}