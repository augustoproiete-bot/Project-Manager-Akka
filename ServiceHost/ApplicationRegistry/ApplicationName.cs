using Akkatecture.ValueObjects;

namespace ServiceHost.ApplicationRegistry
{
    public sealed class ApplicationName : SingleValueObject<string>
    {
        public ApplicationName(string value) 
            : base(value)
        {
        }
    }
}