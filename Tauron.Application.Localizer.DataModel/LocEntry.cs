using System.Collections.Immutable;
using System.IO;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel.Serialization;
using static Tauron.Application.Localizer.DataModel.Serialization.BinaryHelper;

namespace Tauron.Application.Localizer.DataModel
{
    [PublicAPI]
    public sealed record LocEntry(string Project, string Key, ImmutableDictionary<ActiveLanguage, string> Values) : IWriteable
    {
        public LocEntry(string project, string name)
            : this(project, name, ImmutableDictionary<ActiveLanguage, string>.Empty)
        {
        }

        public Maybe<Unit> WriteData(Maybe<BinaryWriter> mayWriter)
        {
            return
                from p in Write(mayWriter, Project)
                from k in Write(mayWriter, Key)
                from v in WriteDic(mayWriter, Values)
                select v;
        }

        public static Maybe<LocEntry> ReadFrom(Maybe<BinaryReader> mayReader)
        {
            return
                from project in ReadString(mayReader)
                from key in ReadString(mayReader)
                from values in ReadDic(mayReader, ActiveLanguage.ReadFrom, ReadString)
                select new LocEntry(project, key, values);
        }
    }
}