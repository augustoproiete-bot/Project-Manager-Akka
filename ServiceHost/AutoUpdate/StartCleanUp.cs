namespace ServiceHost.AutoUpdate
{
    public sealed class StartCleanUp
    {
        public int Id { get; }

        public StartCleanUp(in int id) => Id = id;
    }
}