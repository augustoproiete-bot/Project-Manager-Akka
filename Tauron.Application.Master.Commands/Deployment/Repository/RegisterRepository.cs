using System.IO;
using Akka.Actor;

namespace Tauron.Application.Master.Commands.Deployment.Repository
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