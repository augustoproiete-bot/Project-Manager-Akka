using System;
using System.Collections.Immutable;
using Akka.Cluster;
using JetBrains.Annotations;

namespace Tauron.Application.Master.Commands
{
    public sealed class QueryRegistratedServices
    {
    }

    [PublicAPI]
    public sealed class MemberAddress : IEquatable<MemberAddress>, IEquatable<UniqueAddress>
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

        public int Uid { get; }

        public string Host { get; }

        public int? Port { get; }

        public string System { get; }

        public string Protocol { get; }

        public string UniqeAdress { get; }

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

    public sealed class MemberService
    {
        public string Name { get; }

        public MemberAddress Address { get; }

        public MemberService(string name, MemberAddress address)
        {
            Name = name;
            Address = address;
        }
    }

    public sealed class QueryRegistratedServicesResponse
    {
        public ImmutableList<MemberService> Services { get; }

        public QueryRegistratedServicesResponse(ImmutableList<MemberService> services)
            => Services = services;
    }
}