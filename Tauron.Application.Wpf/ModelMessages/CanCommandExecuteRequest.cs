using Amadevus.RecordGenerator;

namespace Tauron.Application.Wpf.ModelMessages
{
    [Record]
    public sealed partial class CanCommandExecuteRequest
    {
        public string Name { get; }

        public object? Parameter { get; }
    }
}