using System.IO;
using Akka.Actor;
using Akka.Cluster;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands
{
    public sealed class RegisterService : IInternalSerializable
    {
        public string Name { get; }

        public UniqueAddress Address { get; }

        public RegisterService(string name, UniqueAddress address)
        {
            Name = name;
            Address = address;
        }

        [UsedImplicitly]
        public RegisterService(BinaryReader reader)
        {
            var manifest = BinaryManifest.Read(reader);
            manifest.Verify(nameof(RegisterService), 0);

            if (manifest.WhenVersion(1))
            {
                Name = reader.ReadString();
                Address = new UniqueAddress(global::Akka.Actor.Address.Parse(reader.ReadString()), reader.ReadInt32());
            }
            else
            {
                Name = string.Empty;
                Address = new UniqueAddress(new Address(string.Empty, string.Empty), 0);
            }
        }

        public void Write(BinaryWriter writer)
        {
            BinaryManifest.Write(writer, nameof(RegisterService), 1);
            writer.Write(Name);
            writer.Write(Address.Address.ToString());
            writer.Write(Address.Uid);
        }
    }
}