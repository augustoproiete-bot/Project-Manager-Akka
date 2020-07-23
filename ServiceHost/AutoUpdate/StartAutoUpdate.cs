namespace ServiceHost.AutoUpdate
{
    public sealed class StartAutoUpdate
    {
        public string OriginalZip { get; }

        public StartAutoUpdate(string originalZip) => OriginalZip = originalZip;
    }
}