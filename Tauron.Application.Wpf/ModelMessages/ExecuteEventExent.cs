using Amadevus.RecordGenerator;
using Tauron.Application.Wpf.Commands;

namespace Tauron.Application.Wpf.ModelMessages
{
    [Record]
    public sealed partial class ExecuteEventExent
    {
        public EventData Data { get; }

        public string Name { get; }
    }
}