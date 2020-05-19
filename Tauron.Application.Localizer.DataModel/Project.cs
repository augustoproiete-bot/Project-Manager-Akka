using System.Collections.Immutable;
using System.IO;
using System.Linq;
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

        public ImmutableList<ActiveLanguage> ActiveLanguages { get; } = ImmutableList<ActiveLanguage>.Empty;

        public Project(BinaryReader reader)
        {
            ProjectName = reader.ReadString();

            var langList = ImmutableList<ActiveLanguage>.Empty.ToBuilder();
            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
                langList.Add(new ActiveLanguage(reader));

            ActiveLanguages = langList.ToImmutable();

            count = reader.ReadInt32();
            var list = ImmutableList<LocEntry>.Empty.ToBuilder();
            for (var i = 0; i < count; i++)
                list.Add(new LocEntry(reader, this));

            Entries = list.ToImmutable();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(ProjectName);
            writer.Write(Entries.Count);

            writer.Write(ActiveLanguages.Count);
            foreach (var activeLanguage in ActiveLanguages)
                activeLanguage.Write(writer);

            foreach (var locEntry in Entries)
                locEntry.Write(writer);
        }

        public ActiveLanguage GetActiveLanguage(string shortcut)
        {
            return ActiveLanguages.Find(al => al.Shortcut == shortcut) ?? ActiveLanguage.Invariant;
        }
    }
}