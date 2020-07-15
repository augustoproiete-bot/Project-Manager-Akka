namespace ServiceHost.ApplicationRegistry
{
    public sealed class InstalledAppQuery
    {
        public string Name { get; }

        public InstalledAppQuery(string name) => Name = name;
    }
}