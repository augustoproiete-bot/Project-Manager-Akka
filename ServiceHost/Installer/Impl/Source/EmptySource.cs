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

        public Task<Status> PreperforCopy(InstallerContext context) 
            => Task.FromResult<Status>(new Status.Failure(new NotImplementedException()));
        public Task<Status> CopyTo(InstallerContext context, string target) 
            => Task.FromResult<Status>(new Status.Failure(new NotImplementedException()));

        public void CleanUp(InstallerContext context)
        {
            
        }

        public int Version { get; } = -1;
        public string ToZipFile(InstallerContext context) => throw new NotImplementedException();
    }
}