using System.Collections.Immutable;
using System.IO;
using Amadevus.RecordGenerator;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel.Serialization;

namespace Tauron.Application.Localizer.DataModel
{
    [Record]
    [PublicAPI]
    public sealed partial class LocEntry : IWriteable
    {
        public LocEntry(string project, string name)
        {
            Project = project;
            Key = name;
            Values = ImmutableDictionary<ActiveLanguage, string>.Empty;
        }

        public string Project { get; }

        public string Key { get; }

        public ImmutableDictionary<ActiveLanguage, string> Values { get; }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Project);
            writer.Write(Key);

            BinaryHelper.WriteDic(Values, writer);
        }

        public static LocEntry ReadFrom(BinaryReader reader)
        {
            var builder = new Builder
            {
                Project = reader.ReadString(),
                Key = reader.ReadString(),
                Values = BinaryHelper.Read(reader, ActiveLanguage.ReadFrom, binaryReader => binaryReader.ReadString())
            };


            return builder.ToImmutable();
        }
    }
}