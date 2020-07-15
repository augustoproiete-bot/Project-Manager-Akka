namespace ServiceHost.ApplicationRegistry
{
    public static class Extemsions
    {
        public static bool IsEmpty(this InstalledApp app)
            => app == InstalledApp.Empty;
    }
}