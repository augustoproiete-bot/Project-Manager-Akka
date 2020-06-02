namespace Akka.MGIHelper.Core.ProcessManager
{
    public sealed class RegisterProcessFile
    {
        public RegisterProcessFile(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; }
    }
}