using System.Diagnostics;
using Amadevus.RecordGenerator;

namespace Akka.MGIHelper.Core.ProcessManager
{
    [Record]
    public sealed partial class ProcessStateChange
    {
        public ProcessChange Change { get; }

        public string Name { get; }

        public int Id { get; }

        public Process Process { get; }
    }
}