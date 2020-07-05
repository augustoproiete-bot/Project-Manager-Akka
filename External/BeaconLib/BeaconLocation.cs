using System;
using System.Net;

namespace BeaconLib
{
    /// <summary>
    /// Class that represents a discovered beacon
    /// </summary>
    public sealed class BeaconLocation : IEquatable<BeaconLocation>
    {
        public BeaconLocation(IPEndPoint address, string data, DateTime lastAdvertised)
        {
            Address = address;
            Data    = data;
            LastAdvertised = lastAdvertised;
        }

        public IPEndPoint Address { get; }
        public string Data { get; }
        public DateTime LastAdvertised { get; }

        public override string ToString() => Data;

        public bool Equals(BeaconLocation? other) 
            => Equals(Address, other?.Address);

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((BeaconLocation) obj);
        }

        public override int GetHashCode() => (Address != null ? Address.GetHashCode() : 0);
    }
}
