using System;

namespace Tauron.Application.Master.Commands.Deployment.Deployment
{
    public sealed class QueryFailedException : Exception
    {
        public QueryFailedException(string msg)
            : base(msg)
        {
            
        }
    }
}