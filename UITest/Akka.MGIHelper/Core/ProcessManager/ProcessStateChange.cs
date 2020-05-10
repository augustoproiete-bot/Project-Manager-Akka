using System.Diagnostics;
using Amadevus.RecordGenerator;

namespace Akka.MGIHelper.Core.ProcessManager
{
    public sealed partial class ProcessStateChange
    {
        public ProcessChange Change { get; }

        public string Name { get; }

        public int Id { get; }

        public Process Process { get; }

        public ProcessStateChange(ProcessChange change, string name, int id, Process process)
        {
            Change = change;
            Name = name;
            Id = id;
            Process = process;
        }
    }
}