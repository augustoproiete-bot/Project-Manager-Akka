using System;
using System.Threading.Tasks;
using Akka.Actor;

namespace ServiceHost.Installer.Impl.Source
{
    public sealed class EmptySource : IInstallationSource
    {
        public static EmptySource Instnace { get; } = new EmptySource(); 

        private EmptySource()
        {
            
        }

        public Status ValidateInput(InstallerContext name) => new Status.Failure(new NotImplementedException());

        public Task<Status> PreperforCopy(InstallerContext context) => Task.FromResult<Status>(new Status.Failure(new NotImplementedException()));
    }
}