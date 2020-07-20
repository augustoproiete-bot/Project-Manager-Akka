namespace ServiceHost.Services
{
    public sealed class StopResponse
    {
        public string Name { get; }

        public StopResponse(string name) => Name = name;
    }
}