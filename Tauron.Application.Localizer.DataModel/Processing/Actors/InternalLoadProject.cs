using Akka.Actor;

namespace Tauron.Application.Localizer.DataModel.Processing.Actors
{
    public class InternalLoadProject
    {
        public InternalLoadProject(LoadProjectFile projectFile, IActorRef originalSender)
        {
            ProjectFile = projectFile;
            OriginalSender = originalSender;
        }

        public LoadProjectFile ProjectFile { get; }

        public IActorRef OriginalSender { get; }
    }
}