using Amadevus.RecordGenerator;

namespace Akka.MGIHelper.Core.ProcessManager
{
    public sealed partial class RegisterProcessFile
    {
        public string FileName { get; }

        public RegisterProcessFile(string fileName) => FileName = fileName;
    }
}