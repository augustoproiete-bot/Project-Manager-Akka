using System.Collections.Immutable;
using System.IO;
using Functional.Maybe;
using Tauron.Application.Localizer.DataModel.Serialization;
using static Tauron.Application.Localizer.DataModel.Serialization.BinaryHelper;

namespace Tauron.Application.Localizer.DataModel
{
    public sealed record BuildInfo(bool IntigrateProjects, ImmutableDictionary<string, string> ProjectPaths) : IWriteable
    {
        public BuildInfo()
            : this(true, ImmutableDictionary<string, string>.Empty){ }

        public Maybe<Unit> WriteData(Maybe<BinaryWriter> mayWriter)
        {
            return
                from a in Write(mayWriter, IntigrateProjects)
                from b in WriteDic(mayWriter, ProjectPaths)
                select b;
        }

        public static Maybe<BuildInfo> ReadFrom(Maybe<BinaryReader> mayReader)
        {
            return
                from intigrate in ReadBoolean(mayReader)
                from paths in ReadDic(mayReader, ReadString, ReadString)
                select new BuildInfo(intigrate, paths);
        }
    }
}