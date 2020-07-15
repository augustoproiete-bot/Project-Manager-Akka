using Akka.Actor;
using ServiceHost.ApplicationRegistry;
using ServiceHost.Installer.Impl.Source;
using Tauron;
using Tauron.Application.ActorWorkflow;
using Tauron.Application.Workflow;

namespace ServiceHost.Installer.Impl
{
    public sealed class ActualInstallerActor : LambdaWorkflowActor<InstallerContext>
    {
        private static readonly StepId Preperation = new StepId(nameof(Preperation));
        private static readonly StepId Validation = new StepId(nameof(Validation));
        private static readonly StepId PreCopy = new StepId(nameof(PreCopy));
        private static readonly StepId Copy = new StepId(nameof(Copy));
        private static readonly StepId Registration = new StepId(nameof(Registration));
        private static readonly StepId Finalization = new StepId(nameof(Finalization));

        private readonly InstallationSourceSelector _installationSourceSelector = new InstallationSourceSelector();

        public ActualInstallerActor(IAppRegistry registry)
        {
            StartMessage<FileInstallationRequest>(HandleFileInstall);

            WhenStep(Preperation, config =>
                                  {
                                      config.OnExecute((context, step) =>
                                                           context.SetSource(_installationSourceSelector.Select, step.SetError)
                                                              .When(i => i != null, () =>
                                                                                    {

                                                                                        return StepId.Waiting;
                                                                                    }, StepId.Invalid));
                                  });
            
            WhenStep(Validation, confg =>
                                 {
                                     confg.OnExecute((context, step) =>
                                                     {
                                                         return PreCopy;
                                                     });
                                 });

            OnFinish(wr =>
                     {
                         Sender.Tell(new InstallerationCompled(wr.Succesfully, wr.Error), ActorRefs.NoSender);
                         Context.Stop(Self);
                     });
        }

        private void HandleFileInstall(FileInstallationRequest request) 
            => Start(new InstallerContext(InstallType.Manual, request.Name, request.Path,  request.Override));
    }
}