using System.Collections.Immutable;
using System.IO;
using Amadevus.RecordGenerator;
using JetBrains.Annotations;

namespace Tauron.Application.Localizer.DataModel
{
    [Record]
    [PublicAPI]
    public sealed partial class Project
    {
        public ImmutableList<LocEntry> Entries { get; }

        public string ProjectName { get; }

        public Project(BinaryReader reader)
        {
            ProjectName = reader.ReadString();
            var count = reader.ReadInt32();
            var list = ImmutableList<LocEntry>.Empty.ToBuilder();

            for (var i = 0; i < count; i++) 
                list.Add(new LocEntry(reader));
            
            Entries = list.ToImmutable();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(ProjectName);
            writer.Write(Entries.Count);

            foreach (var locEntry in Entries)
                locEntry.Write(writer);
        }
    }
}