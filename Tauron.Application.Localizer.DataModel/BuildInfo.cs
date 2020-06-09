using System.Collections.Immutable;
using System.IO;
using Amadevus.RecordGenerator;
using Tauron.Application.Localizer.DataModel.Serialization;

namespace Tauron.Application.Localizer.DataModel
{
    [Record]
    public sealed partial class BuildInfo : IWriteable
    {
        public bool IntigrateProjects { get; }

        public ImmutableDictionary<string, string> ProjectPaths { get; }

        public BuildInfo() 
            => ProjectPaths = ImmutableDictionary<string, string>.Empty;

        public void Write(BinaryWriter writer)
        {
            writer.Write(IntigrateProjects);
            BinaryHelper.WriteDic(ProjectPaths, writer);
        }

        public static BuildInfo ReadFrom(BinaryReader reader)
        {
            return new Builder
            {
                IntigrateProjects = reader.ReadBoolean(), 
                ProjectPaths = BinaryHelper.Read(reader, r => r.ReadString(), r => r.ReadString())
            }.ToImmutable();
        }
    }
}