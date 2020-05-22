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

        public ImmutableList<ActiveLanguage> ActiveLanguages { get; }

        public ImmutableList<string> Imports { get; } 

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

            count = reader.ReadInt32();
            var imports = ImmutableList<string>.Empty.ToBuilder();
            for (int i = 0; i < count; i++) 
                imports.Add(reader.ReadString());

            Imports = imports.ToImmutable();
        }

        public Project()
        {
            ProjectName = string.Empty;
            Entries = ImmutableList<LocEntry>.Empty;
            ActiveLanguages = ImmutableList<ActiveLanguage>.Empty;
            Imports = ImmutableList<string>.Empty;
        }

        public Project(string name)
            : this()
        {
            ProjectName = name;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(ProjectName);
            writer.Write(Entries.Count);

            writer.Write(ActiveLanguages.Count);
            foreach (var activeLanguage in ActiveLanguages)
                activeLanguage.Write(writer);

            writer.Write(Entries.Count);
            foreach (var locEntry in Entries)
                locEntry.Write(writer);

            writer.Write(Imports.Count);
            foreach (var import in Imports) 
                writer.Write(import);
        }

        public ActiveLanguage GetActiveLanguage(string shortcut)
        {
            return ActiveLanguages.Find(al => al.Shortcut == shortcut) ?? ActiveLanguage.Invariant;
        }
    }
}