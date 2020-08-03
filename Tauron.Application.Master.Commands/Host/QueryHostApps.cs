using System.IO;

namespace Tauron.Application.Master.Commands.Host
{
    public sealed class QueryHostApps : InternalHostMessages.CommandBase
    {
        public QueryHostApps(string target)
            : base(target, InternalHostMessages.CommandType.AppRegistry)
        {
            
        }

        public QueryHostApps(BinaryReader reader)
            : base(reader)
        {
            
        }
    }
}