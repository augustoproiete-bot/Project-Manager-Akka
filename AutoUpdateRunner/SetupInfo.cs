namespace AutoUpdateRunner
{
    public sealed class SetupInfo
    {
        public string DownloadFile { get; set; } = string.Empty;

        public string StartFile { get; set; } = string.Empty;

        public string Target { get; set; } = string.Empty;

        public int RunningProcess { get; set; } = 0;

        public int KillTime { get; set; }
    }
}