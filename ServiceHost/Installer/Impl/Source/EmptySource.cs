using System;
using Akka.Actor;

namespace ServiceHost.Installer.Impl.Source
{
    public sealed class EmptySource : IInstallationSource
    {
        public static EmptySource Instnace { get; } = new EmptySource(); 

        private EmptySource()
        {
            
        }

        public Status ValidateInput(InstallerContext name)
        {
            return new Status.Failure(new NotImplementedException());
        }
    }
}