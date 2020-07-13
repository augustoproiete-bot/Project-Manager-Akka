using Akkatecture.ValueObjects;

namespace ServiceHost.ApplicationRegistry
{
    public sealed class ApplicationPath : SingleValueObject<string>
    {
        public ApplicationPath(string value) 
            : base(value)
        {
        }
    }
}