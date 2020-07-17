namespace ServiceHost.Services
{
    public sealed class StopService
    {
        public string Name { get; }

        public StopService(string name) 
            => Name = name;
    }
}