using System;
using System.IO;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands.Deployment.Deployment.Data
{
    [PublicAPI]
    public sealed class AppBinary : InternalSerializableBase
    {
        public int FileVersion { get; private set; }

        public string AppName { get; private set; } = string.Empty;

        public DateTime CreationTime { get; private set; }

        public AppBinary(int fileVersion, string appName, DateTime creationTime)
        {
            FileVersion = fileVersion;
            AppName = appName;
            CreationTime = creationTime;
        }

        public AppBinary(BinaryReader reader)
            : base(reader)
        {
            
        }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest)
        {
            FileVersion = reader.ReadInt32();
            AppName = reader.ReadString();
            CreationTime = new DateTime(reader.ReadInt64());
            base.ReadInternal(reader, manifest);
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            writer.Write(FileVersion);
            writer.Write(AppName);
            writer.Write(CreationTime.Ticks);
            base.WriteInternal(writer);
        }
    }
}