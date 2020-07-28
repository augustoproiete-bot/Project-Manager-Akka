using System.IO;
using JetBrains.Annotations;

namespace Tauron.Application.Master.Commands.Host
{
    public sealed class StopAllApps : InternalHostMessages.CommandBase
    {
        public StopAllApps(string target) 
            : base(target, InternalHostMessages.CommandType.AppManager)
        {
        }

        [UsedImplicitly]
        public StopAllApps(BinaryReader reader)
            : base(reader)
        {
            
        }
    }
}