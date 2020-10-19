using System.IO;

namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed class QueryHostApps : InternalHostMessages.CommandBase
    {
        public QueryHostApps(string target)
            : base(target, InternalHostMessages.CommandType.AppRegistry)
        {
            
        }
    }
}