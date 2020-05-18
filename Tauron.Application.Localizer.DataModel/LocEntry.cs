using System.Collections.Immutable;
using System.IO;
using Akka.Util.Internal;
using Amadevus.RecordGenerator;
using JetBrains.Annotations;

namespace Tauron.Application.Localizer.DataModel
{
    [Record]
    [PublicAPI]
    public sealed partial class LocEntry
    {
        public string Project { get; }

        public string Key { get; }

        public ImmutableDictionary<ActiveLanguage, string> Values { get; }

        public LocEntry(BinaryReader reader, Project project)
        {
            Project = reader.ReadString();
            Key = reader.ReadString();

            var builder = ImmutableDictionary<ActiveLanguage, string>.Empty.ToBuilder();
            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                builder.AddOrSet(project.GetActiveLanguage(reader.ReadString()), reader.ReadString());
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Project);
            writer.Write(Key);

            writer.Write(Values.Count);
            foreach (var (key, value) in Values)
            {
                writer.Write(key.Shortcut);
                writer.Write(value);
            }
        }
    }
}