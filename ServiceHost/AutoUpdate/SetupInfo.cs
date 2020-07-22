using JetBrains.Annotations;

namespace ServiceHost.AutoUpdate
{
    [PublicAPI]
    public sealed class SetupInfo
    {
        public sealed class SetupBuilder
        {
            private readonly string _downloadFile;

            public SetupBuilder(string downloadFile) => _downloadFile = downloadFile;

            public SetupInfo ToInfo(string hostExe, string hostPath, int hostProcessHandle)
                => new SetupInfo(_downloadFile, hostExe, hostPath, hostProcessHandle);
        }

        public string DownloadFile { get; }

        public string StartFile { get; }

        public string Target { get; }

        public int RunningProcess { get; }

        public int KillTime { get; }

        public SetupInfo(string downloadFile, string startFile, string target, int runningProcess, int killTime = 60000)
        {
            DownloadFile = downloadFile;
            StartFile = startFile;
            Target = target;
            RunningProcess = runningProcess;
            KillTime = killTime;
        }

        public static SetupBuilder New(string downloadFile)
            => new SetupBuilder(downloadFile);

        public string ToCommandLine() 
            => $"--downloadfile {DownloadFile} --startfile {StartFile} --target {Target} --runningprocess {RunningProcess} --killtime {KillTime}";
    }
}