using Amadevus.RecordGenerator;

namespace Akka.MGIHelper.Core.ProcessManager
{
    [Record]
    public sealed partial class TrackFile
    {
        public string FileName { get; } = string.Empty;
    }
}