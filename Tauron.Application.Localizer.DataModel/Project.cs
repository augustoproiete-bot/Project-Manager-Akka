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
        public ImmutableList<LocEntry> Entries { get; }

        public string ProjectName { get; }

        public ImmutableList<ActiveLanguage> ActiveLanguages { get; }

        public ImmutableList<string> Imports { get; }

        public Project()
        {
            ProjectName = string.Empty;
            Entries = ImmutableList<LocEntry>.Empty;
            ActiveLanguages = ImmutableList<ActiveLanguage>.Empty;
            Imports = ImmutableList<string>.Empty;
        }

        public Project(string name)
            : this() => ProjectName = name;

        public void Write(BinaryWriter writer)
        {
            writer.Write(ProjectName);

            Helper.WriteList(ActiveLanguages, writer);
            Helper.WriteList(Entries, writer);
            Helper.WriteList(Imports, writer);
        }

        public ActiveLanguage GetActiveLanguage(string shortcut) 
            => ActiveLanguages.Find(al => al.Shortcut == shortcut) ?? ActiveLanguage.Invariant;

        public static Project ReadFrom(BinaryReader reader)
        {
            var project = new Builder
                          {
                              ProjectName = reader.ReadString(), 
                              ActiveLanguages = Helper.Read(reader, ActiveLanguage.ReadFrom), 
                              Entries = Helper.Read(reader, LocEntry.ReadFrom), 
                              Imports = Helper.ReadString(reader)
                          };


            return project.ToImmutable();
        }
    }
}