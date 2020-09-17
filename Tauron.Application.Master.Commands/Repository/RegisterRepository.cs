using System.IO;
using Akka.Actor;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands.Repository
{
    public sealed class RegisterRepository : RepositoryAction
    {
        public RegisterRepository(string repoName, IActorRef listner)
            : base(repoName, listner)
        {
        }

        public RegisterRepository(BinaryReader reader, ExtendedActorSystem system)
            : base(reader, system)
        {
            
        }
    }
}