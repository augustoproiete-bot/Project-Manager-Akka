using System.IO;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands.Administration.Host
{
    public static class InternalHostMessages
    {
        public enum CommandType
        {
            AppManager,
            AppRegistry,
            Installer
        }

        public abstract class CommandBase : InternalSerializableBase
        {
            public string Target { get; private set; } = string.Empty;

            public CommandType Type { get; private set; }

            protected CommandBase(string target, CommandType type)
            {
                Target = target;
                Type = type;
            }

            protected CommandBase(BinaryReader reader)
                : base(reader) { }

            protected override void WriteInternal(ActorBinaryWriter writer)
            {
                writer.Write(Target);
                writer.Write((int)Type);
            }

            protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest)
            {
                if (manifest.WhenVersion(1))
                {
                    Target = reader.ReadString();
                    Type = (CommandType) reader.ReadInt32();
                }
            }
        }

        public sealed class GetHostName : InternalSerializableBase
        {
            public GetHostName()
            {
                
            }

            public GetHostName(BinaryReader reader)
                : base(reader)
            {
                
            }

        }

        public sealed class GetHostNameResult : InternalSerializableBase
        {
            public string Name { get; private set; } = string.Empty;

            public GetHostNameResult(string name) => Name = name;

            public GetHostNameResult(BinaryReader reader)
                : base(reader)
            {
                
            }

            protected override void WriteInternal(ActorBinaryWriter writer)
            {
                writer.Write(Name);
                base.WriteInternal(writer);
            }

            protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest)
            {
                if (manifest.WhenVersion(1))
                    Name = reader.ReadString();
                base.ReadInternal(reader, manifest);
            }
        }
    }
}