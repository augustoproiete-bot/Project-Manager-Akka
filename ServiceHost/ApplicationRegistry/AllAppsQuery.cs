namespace ServiceHost.ApplicationRegistry
{
    public sealed class AllAppsQuery
    {
        
    }

    public sealed class AllAppsResponse
    {
        public string[] Apps { get; }

        public AllAppsResponse(string[] apps) 
            => Apps = apps;
    }
}