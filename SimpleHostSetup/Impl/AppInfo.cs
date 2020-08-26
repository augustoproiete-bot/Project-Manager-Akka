namespace SimpleHostSetup.Impl
{
    public sealed class AppInfo
    {
        public AppType AppType { get; }

        public string ProjectName { get; }

        public AppInfo(AppType appType, string projectName)
        {
            AppType = appType;
            ProjectName = projectName;
        }
    }
}