namespace AkkaShared
{
    public sealed class StringMessage
    {
        public string Message { get; }

        public StringMessage(string message) 
            => Message = message;
    }
}