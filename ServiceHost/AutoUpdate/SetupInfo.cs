using JetBrains.Annotations;

namespace ServiceHost.AutoUpdate
{
    [PublicAPI]
    public sealed class SetupInfo
    {
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
        
        public string ToCommandLine() 
            => $"--downloadfile {DownloadFile} --startfile {StartFile} --target {Target} --runningprocess {RunningProcess} --killtime {KillTime}";
    }
}