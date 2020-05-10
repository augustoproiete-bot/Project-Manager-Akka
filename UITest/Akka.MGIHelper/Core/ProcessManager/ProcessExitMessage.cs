using System.Diagnostics;
using Amadevus.RecordGenerator;

namespace Akka.MGIHelper.Core.ProcessManager
{
    [Record(Features.Deconstruct)]
    public sealed partial class ProcessExitMessage
    {
        public Process Target { get; }

        public string Name { get; }

        public int Id { get; }

        public ProcessExitMessage(Process target, string name, int id)
        {
            Target = target;
            Name = name;
            Id = id;
        }
    }
}