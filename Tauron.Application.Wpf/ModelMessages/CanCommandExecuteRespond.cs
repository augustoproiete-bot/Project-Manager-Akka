using Amadevus.RecordGenerator;

namespace Tauron.Application.Wpf.ModelMessages
{
    [Record]
    public sealed partial class CanCommandExecuteRespond
    {
        public string Name { get; }

        public bool CanExecute { get; }
    }
}