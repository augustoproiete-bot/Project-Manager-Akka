using System.IO;
using Akka.Actor;
using Akka.Cluster;
using Google.Protobuf;
using Google.Protobuf.Reflection;

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

        public RegisterService(BinaryReader reader, ExtendedActorSystem system)
        {
            
        }
    }
}