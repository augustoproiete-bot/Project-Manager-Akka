using System.Diagnostics;
using Amadevus.RecordGenerator;

namespace Akka.MGIHelper.Core.ProcessManager
{
    [Record]
    public sealed partial class ProcessExitMessage
    {
        public Process Target { get; }

        public string Name { get; }

        public int Id { get; }
    }
}