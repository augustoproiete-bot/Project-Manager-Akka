namespace ServiceHost.Services
{
    public sealed class StopResponse
    {
        public string Name { get; }
        public bool Error { get; }

        public StopResponse(string name, bool error)
        {
            Name = name;
            Error = error;
        }
    }
}