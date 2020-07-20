namespace ServiceHost.Services
{
    public sealed class StopApp
    {
        public string Name { get; }

        public StopApp(string name) 
            => Name = name;
    }
}