using System;
using System.IO;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands.Deployment.Build.Data
{
    [PublicAPI]
    public sealed class AppBinary : InternalSerializableBase
    {
        public int FileVersion { get; private set; }

        public string AppName { get; private set; } = string.Empty;

        public DateTime CreationTime { get; private set; }

        public bool Deleted { get; set; }

        public string Commit { get; set; } = string.Empty;

        public string Repository { get; set; } = string.Empty;

        public AppBinary(int fileVersion, DateTime creationTime, bool deleted, string commit, string repository)
        {
            FileVersion = fileVersion;
            CreationTime = creationTime;
            Deleted = deleted;
            Commit = commit;
            Repository = repository;
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
            Deleted = reader.ReadBoolean();
            Commit = reader.ReadString();
            Repository = reader.ReadString();
            base.ReadInternal(reader, manifest);
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            writer.Write(FileVersion);
            writer.Write(AppName);
            writer.Write(CreationTime.Ticks);
            writer.Write(Deleted);
            writer.Write(Commit);
            writer.Write(Repository);
            base.WriteInternal(writer);
        }
    }
}