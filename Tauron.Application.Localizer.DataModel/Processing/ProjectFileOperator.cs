using Akka.Actor;
using Tauron.Akka;
using Tauron.Application.Localizer.DataModel.Processing.Actors;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed class ProjectFileOperator : ExposedReceiveActor
    {
        public ProjectFileOperator()
        {
            this.Flow<LoadProjectFile>()
                .To.Func(lp => new InternalLoadProject(lp, Context.Sender)).ToRef(ac => ac.ActorOf<ProjectLoader>())
                .AndReceive();

            this.Flow<SaveProject>()
                .To.Func(s => s).

            Receive<SaveProject>(SaveProject);
        }

        private void SaveProject(SaveProject obj)
        {
            var actor = Context.GetOrAdd<ProjectSaver>("Saver");
            actor.Forward(obj);
        }
    }
}