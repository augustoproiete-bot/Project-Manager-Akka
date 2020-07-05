using System;
using System.Collections.Generic;
using System.Net;

namespace BeaconLib
{
    internal sealed class IpEndPointComparer : IComparer<IPEndPoint>
    {
        public static readonly IpEndPointComparer Instance = new IpEndPointComparer();

        public int Compare(IPEndPoint? x, IPEndPoint? y)
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (x == null && y == null)
                return 0;
            if (x == null)
                return -1;
            if (y == null)
                return 1;

            var c = string.Compare(x.Address.ToString(), y.Address.ToString(), StringComparison.Ordinal);
            if (c != 0) return c;
            return y.Port - x.Port;
        }
    }
}
