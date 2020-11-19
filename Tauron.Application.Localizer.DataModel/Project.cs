using System.Collections.Immutable;
using System.IO;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel.Serialization;
using static Tauron.Prelude;
using static Tauron.Application.Localizer.DataModel.Serialization.BinaryHelper;

namespace Tauron.Application.Localizer.DataModel
{
    [PublicAPI]
    public sealed record Project(ImmutableList<LocEntry> Entries, string ProjectName, ImmutableList<ActiveLanguage> ActiveLanguages, ImmutableList<string> Imports) : IWriteable
    {
        public Project()
            : this(ImmutableList<LocEntry>.Empty, string.Empty, ImmutableList<ActiveLanguage>.Empty, ImmutableList<string>.Empty)
        { }

        public Project(string name)
            : this() => ProjectName = name;

        public Maybe<Unit> WriteData(Maybe<BinaryWriter> mayWriter)
        {
            return
                from p in Write(mayWriter, ProjectName)
                from l in WriteList(mayWriter, ActiveLanguages)
                from e in WriteList(mayWriter, Entries)
                from i in WriteList(mayWriter, Imports) 
                select i;
        }


        public Maybe<ActiveLanguage> GetActiveLanguage(Maybe<string> mayShortcut)
        {
            return Either(
                from shortcut in mayShortcut
                select MayNotNull(ActiveLanguages.Find(al => al.Shortcut == shortcut)),
                ActiveLanguage.Invariant);
        }

        public static Maybe<Project> ReadFrom(Maybe<BinaryReader> reader)
        {
            return
                from name in ReadString(reader)
                from languages in ReadList(reader, ActiveLanguage.ReadFrom)
                from entries in ReadList(reader, LocEntry.ReadFrom)
                from imports in ReadList(reader)
                select new Project(entries, name, languages, imports);
        }
    }
}