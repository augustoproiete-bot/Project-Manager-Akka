namespace ServiceHost.ApplicationRegistry
{
    public class UpdateRegistrationRequest
    {
        public string Name { get; }

        public UpdateRegistrationRequest(string name) 
            => Name = name;
    }
}