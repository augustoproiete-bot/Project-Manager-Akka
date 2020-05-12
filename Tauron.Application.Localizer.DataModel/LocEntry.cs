using System.IO;
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

        public string Value { get; }

        public LocEntry(BinaryReader reader)
        {
            Project = reader.ReadString();
            Key = reader.ReadString();
            Value = reader.ReadString();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Project);
            writer.Write(Key);
            writer.Write(Value);
        }
    }
}