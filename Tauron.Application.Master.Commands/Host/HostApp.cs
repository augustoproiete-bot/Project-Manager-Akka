using System.IO;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands.Host
{
    [PublicAPI]
    public sealed class HostApp : InternalSerializableBase
    {
        public string Name { get; private set; } = string.Empty;

        public string Path { get; private set; } = string.Empty;

        public int AppVersion { get; private set; } 

        public AppType AppType { get; private set; }

        public bool SupressWindow { get; private set; }

        public string Exe { get; private set; } = string.Empty;

        public bool Running { get; private set; }

        public HostApp(string name, string path, int version, AppType appType, bool supressWindow, string exe, bool running)
        {
            Name = name;
            Path = path;
            AppVersion = version;
            AppType = appType;
            SupressWindow = supressWindow;
            Exe = exe;
            Running = running;
        }

        public HostApp(BinaryReader reader)
            : base(reader)
        {
            
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(Path);
            writer.Write(AppVersion);
            writer.Write((int)AppType);
            writer.Write(SupressWindow);
            writer.Write(Exe);
            writer.Write(Running);
            
            base.WriteInternal(writer);
        }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest)
        {
            if (manifest.WhenVersion(1))
            {
                Name = reader.ReadString();
                Path = reader.ReadString();
                AppVersion = reader.ReadInt32();
                AppType = (AppType) reader.ReadInt32();
                SupressWindow = reader.ReadBoolean();
                Exe = reader.ReadString();
                Running = reader.ReadBoolean();
            }

            base.ReadInternal(reader, manifest);
        }
    }
}