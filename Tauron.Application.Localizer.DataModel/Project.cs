using System.Collections.Immutable;
using System.IO;
using Amadevus.RecordGenerator;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel.Serialization;

namespace Tauron.Application.Localizer.DataModel
{
    [Record]
    [PublicAPI]
    public sealed partial class Project : IWriteable
    {
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

        public ImmutableList<LocEntry> Entries { get; }

        public string ProjectName { get; }

        public ImmutableList<ActiveLanguage> ActiveLanguages { get; }

        public ImmutableList<string> Imports { get; }

        public void Write(BinaryWriter writer)
        {
            writer.Write(ProjectName);

            BinaryHelper.WriteList(ActiveLanguages, writer);
            BinaryHelper.WriteList(Entries, writer);
            BinaryHelper.WriteList(Imports, writer);
        }

        public ActiveLanguage GetActiveLanguage(string shortcut)
        {
            return ActiveLanguages.Find(al => al.Shortcut == shortcut) ?? ActiveLanguage.Invariant;
        }

        public static Project ReadFrom(BinaryReader reader)
        {
            var project = new Builder
            {
                ProjectName = reader.ReadString(),
                ActiveLanguages = BinaryHelper.Read(reader, ActiveLanguage.ReadFrom),
                Entries = BinaryHelper.Read(reader, LocEntry.ReadFrom),
                Imports = BinaryHelper.ReadString(reader)
            };


            return project.ToImmutable();
        }
    }
}