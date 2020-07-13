using Akkatecture.ValueObjects;

namespace ServiceHost.ApplicationRegistry
{
    public sealed class ApplicationVersion : SingleValueObject<int>
    {
        public ApplicationVersion(int value) 
            : base(value)
        {
        }
    }
}