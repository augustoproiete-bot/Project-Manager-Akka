using Akka.Actor;
using Tauron.Akka;
using Tauron.Application.Localizer.DataModel.Processing.Actors;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed class ProjectFileOperator : ReceiveActor
    {
        public ProjectFileOperator()
        {
            Receive<LoadProjectFile>(LoadProjectFile);
            Receive<SaveProject>(SaveProject);
        }

        private void SaveProject(SaveProject obj)
        {
            var actor = Context.GetOrAdd<ProjectSaver>("saver");
            actor.Forward(obj);
        }

        private void LoadProjectFile(LoadProjectFile obj) 
            => Context.ActorOf<ProjectLoader>().Tell(new InternalLoadProject(obj, Sender));
    }
}