namespace SimpleHostSetup.Impl
{
    public sealed class BuildSystemConfiguration
    {
        public string SearchStart { get; }

        public string SearchRootFile { get; }

        public BuildSystemConfiguration(string searchStart, string searchRootFile)
        {
            SearchStart = searchStart;
            SearchRootFile = searchRootFile;
        }
    }
}