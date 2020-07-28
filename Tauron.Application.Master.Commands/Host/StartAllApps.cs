using System.IO;
using JetBrains.Annotations;

namespace Tauron.Application.Master.Commands.Host
{
    public sealed class StartAllApps : InternalHostMessages.CommandBase
    {
        public StartAllApps(string target) : base(target, InternalHostMessages.CommandType.AppManager)
        {
        }

        [UsedImplicitly]
        public StartAllApps(BinaryReader reader)
            : base(reader)
        {
            
        }
    }
}