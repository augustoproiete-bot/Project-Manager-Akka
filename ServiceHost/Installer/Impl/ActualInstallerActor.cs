using Akka.Event;
using ServiceHost.ApplicationRegistry;
using Tauron;
using Tauron.Akka;
using Tauron.Application.ActorWorkflow;
using Tauron.Application.Workflow;

namespace ServiceHost.Installer.Impl
{
    public sealed class ActualInstallerActor : LambdaWorkflowActor<InstallerContext>
    {
        private static readonly StepId Preperation = new StepId(nameof(Preperation));
        private static readonly StepId Validation = new StepId(nameof(Validation));
        private static readonly StepId Copy = new StepId(nameof(Copy));
        private static readonly StepId Registration = new StepId(nameof(Registration));
        private static readonly StepId Finalization = new StepId(nameof(Finalization));

        public ActualInstallerActor(IAppRegistry registry)
        {
            StartMessage<FileInstallationRequest>(HandleFileInstall);
        }

        private void HandleFileInstall(FileInstallationRequest request) 
            => Start(new InstallerContext(InstallType.Manual, request.Name, request.Path,  request.Override));
    }
}