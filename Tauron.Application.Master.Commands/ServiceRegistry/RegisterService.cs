using Akka.Cluster;

namespace Tauron.Application.Master.Commands
{
    public sealed class RegisterService
    {
        public string Name { get; }

        public UniqueAddress Address { get; }

        public RegisterService(string name, UniqueAddress address)
        {
            Name = name;
            Address = address;
        }
    }
}