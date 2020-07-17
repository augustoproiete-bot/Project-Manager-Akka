namespace ServiceHost.Services
{
    public sealed class StartApp
    {
        public string Name { get; }

        public StartApp(string name) 
            => Name = name;
    }
}