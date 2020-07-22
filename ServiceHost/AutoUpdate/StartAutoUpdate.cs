namespace ServiceHost.AutoUpdate
{
    public sealed class StartAutoUpdate
    {
        public SetupInfo.SetupBuilder Builder { get; }

        public StartAutoUpdate(SetupInfo.SetupBuilder builder) => Builder = builder;
    }
}