using System;
using System.Collections.Immutable;
using System.IO;
using Akka.Cluster;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands
{
    public sealed class QueryRegistratedServices : IInternalSerializable
    {
        public QueryRegistratedServices()
        {
            
        }

        [UsedImplicitly]
        public QueryRegistratedServices(BinaryReader reader) 
            => BinaryManifest.Read(reader).Verify(nameof(QueryRegistratedServices), 0);

        public void Write(ActorBinaryWriter writer) 
            => BinaryManifest.Write(writer, nameof(QueryRegistratedServices), 1);
    }

    public sealed class MemberAddress : IEquatable<MemberAddress>, IEquatable<UniqueAddress>, IInternalSerializable
    {
        public static MemberAddress Empty { get; } = new MemberAddress(0, string.Empty, null, string.Empty, string.Empty, string.Empty);

        public static MemberAddress From(UniqueAddress adress)
        {
            return new MemberAddress(
                adress.Uid,
                adress.Address.Host,
                adress.Address.Port,
                adress.Address.System,
                adress.Address.Protocol,
                adress.ToString());
        }

        private MemberAddress(int uid, string host, int? port, string system, string protocol, string uniqeAdress)
        {
            Uid = uid;
            Host = host;
            Port = port;
            System = system;
            Protocol = protocol;
            UniqeAdress = uniqeAdress;
        }

        [UsedImplicitly]
        public MemberAddress(BinaryReader reader)
        {
            var manifest = BinaryManifest.Read(reader);
            manifest.Verify(nameof(MemberAddress), 0);

            if (manifest.WhenVersion(1))
            {
                Uid = reader.ReadInt32();
                Host = reader.ReadString();
                Port = reader.ReadInt32();
                if (Port == -1)
                    Port = null;
                System = reader.ReadString();
                Protocol = reader.ReadString();
                UniqeAdress = reader.ReadString();
            }
        }

        public void Write(ActorBinaryWriter writer)
        {
            BinaryManifest.Write(writer, nameof(MemberAddress), 1);
            
            writer.Write(Uid);
            writer.Write(Host);
            writer.Write(Port ?? -1);
            writer.Write(System);
            writer.Write(Protocol);
            writer.Write(UniqeAdress);
        }

        public int Uid { get; }

        public string Host { get; } = string.Empty;

        public int? Port { get; }

        public string System { get; } = string.Empty;

        public string Protocol { get; } = string.Empty;

        public string UniqeAdress { get; } = string.Empty;

        public override string ToString() => UniqeAdress;

        public bool Equals(MemberAddress? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Uid == other.Uid && Host == other.Host && Port == other.Port && System == other.System && Protocol == other.Protocol && UniqeAdress == other.UniqeAdress;
        }

        public bool Equals(UniqueAddress other) => this == From(other);

        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is MemberAddress other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Uid, Host, Port, System, Protocol, UniqeAdress);

        public static bool operator ==(MemberAddress? left, MemberAddress? right) => Equals(left, right);

        public static bool operator !=(MemberAddress? left, MemberAddress? right) => !Equals(left, right);
    }

    public sealed class MemberService : IInternalSerializable
    {
        public string Name { get; } = string.Empty;

        public MemberAddress Address { get; } = MemberAddress.Empty;

        public MemberService(string name, MemberAddress address)
        {
            Name = name;
            Address = address;
        }

        [UsedImplicitly]
        public MemberService(BinaryReader reader)
        {
            var manifest = BinaryManifest.Read(reader);
            manifest.Verify(nameof(MemberService), 0);

            if (manifest.WhenVersion(1))
            {
                Name = reader.ReadString();
                Address = new MemberAddress(reader);
            }
        }

        public void Write(ActorBinaryWriter writer)
        {
            BinaryManifest.Write(writer, nameof(MemberService), 1);

            writer.Write(Name);
            Address.Write(writer);
        }
    }

    public sealed class QueryRegistratedServicesResponse : IInternalSerializable
    {
        public ImmutableList<MemberService> Services { get; } = ImmutableList<MemberService>.Empty;

        public QueryRegistratedServicesResponse(ImmutableList<MemberService> services) 
            => Services = services;

        [UsedImplicitly]
        public QueryRegistratedServicesResponse(BinaryReader reader)
        {
            var manifest = BinaryManifest.Read(reader);
            manifest.Verify(nameof(QueryRegistratedServicesResponse), 0);

            if (manifest.WhenVersion(1)) 
                Services = BinaryHelper.Read(reader, r => new MemberService(r));
        }

        public void Write(ActorBinaryWriter writer)
        {
            BinaryManifest.Write(writer, nameof(QueryRegistratedServicesResponse), 1);

            BinaryHelper.WriteList(Services, writer);
        }
    }
}