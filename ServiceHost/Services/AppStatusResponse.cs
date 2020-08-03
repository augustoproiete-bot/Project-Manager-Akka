using System;
using System.Collections.Immutable;

namespace ServiceHost.Services
{
    public sealed class AppStatusResponse
    {
        public Guid OpId { get; }

        public ImmutableDictionary<string, bool> Apps { get; }

        public AppStatusResponse(Guid opId, ImmutableDictionary<string, bool>? apps = null)
        {
            OpId = opId;
            Apps = apps ?? ImmutableDictionary<string, bool>.Empty;
        }
    }
}