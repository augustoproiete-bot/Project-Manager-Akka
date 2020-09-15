using System.Collections.Immutable;
using System.IO;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands.Host
{
    public sealed class HostAppsResponse : InternalSerializableBase
    {
        public ImmutableList<HostApp> Apps { get; private set; } = ImmutableList<HostApp>.Empty;

        public HostAppsResponse(ImmutableList<HostApp> apps) => Apps = apps;

        public HostAppsResponse(BinaryReader reader)
            : base(reader)
        {
            
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            BinaryHelper.WriteList(Apps, writer);
        }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest)
        {
            if (manifest.WhenVersion(1))
                Apps = BinaryHelper.Read(reader, r => new HostApp(r));
        }
    }
}