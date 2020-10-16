using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands.Deployment.Build.Data
{
    [PublicAPI]
    public sealed class AppInfo : InternalSerializableBase
    {
        public string Name { get; private set; } = string.Empty;

        public int LastVersion { get; private set; }

        public DateTime UpdateDate { get; private set; }

        public DateTime CreationTime { get; private set; }

        public string Repository { get; private set; } = string.Empty;

        public ImmutableList<AppBinary> Binaries { get; private set; } = ImmutableList<AppBinary>.Empty;

        public bool Deleted { get; private set; }

        public AppInfo(string name, int lastVersion, DateTime updateDate, DateTime creationTime, string repository, IEnumerable<AppBinary> bin)
        {
            Name = name;
            LastVersion = lastVersion;
            UpdateDate = updateDate;
            CreationTime = creationTime;
            Repository = repository;
            Binaries = Binaries.AddRange(bin);
        }

        public AppInfo IsDeleted()
        {
            Deleted = true;
            return this;
        }

        public AppInfo(BinaryReader reader)
            : base(reader)
        { }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest)
        {
            Name = reader.ReadString();
            LastVersion = reader.ReadInt32();
            UpdateDate = new DateTime(reader.ReadInt64());
            CreationTime = new DateTime(reader.ReadInt64());
            Repository = reader.ReadString();
            Binaries = BinaryHelper.Read(reader, r => new AppBinary(reader));
            Deleted = reader.ReadBoolean();
            base.ReadInternal(reader, manifest);
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(LastVersion);
            writer.Write(UpdateDate.Ticks);
            writer.Write(CreationTime.Ticks);
            writer.Write(Repository);
            BinaryHelper.WriteList(Binaries, writer);
            writer.Write(Deleted);
            base.WriteInternal(writer);
        }
    }
}