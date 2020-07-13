using Akkatecture.Core;

namespace ServiceHost.ApplicationRegistry
{
    public sealed class ApplicationId : Identity<ApplicationId>
    {
        public ApplicationId(string value) 
            : base(value)
        { }
    }
}