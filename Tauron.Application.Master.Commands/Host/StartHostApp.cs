using System.IO;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands.Host
{
    public sealed class StartHostApp : InternalHostMessages.CommandBase
    {
        public string AppName { get; private set; } = string.Empty;


        public StartHostApp(string target, string appName) : base(target, InternalHostMessages.CommandType.AppManager)
        {
            AppName = appName;
        }

        [UsedImplicitly]
        public StartHostApp(BinaryReader reader)
            : base(reader)
        {
        }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest)
        {
            if (manifest.WhenVersion(1))
                AppName = reader.ReadString();
            base.ReadInternal(reader, manifest);
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            writer.Write(AppName);
            base.WriteInternal(writer);
        }
    }
}