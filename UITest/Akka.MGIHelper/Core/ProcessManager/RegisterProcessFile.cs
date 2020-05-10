using Amadevus.RecordGenerator;

namespace Akka.MGIHelper.Core.ProcessManager
{
    [Record]
    public sealed partial class RegisterProcessFile
    {
        public string FileName { get; }
    }
}