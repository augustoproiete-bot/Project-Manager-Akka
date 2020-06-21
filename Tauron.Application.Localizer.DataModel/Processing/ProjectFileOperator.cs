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
                .From.Func(lp => new InternalLoadProject(lp, Context.Sender)).ToRef(ac => ac.ActorOf<ProjectLoader>())
                .AndReceive();

            this.Flow<SaveProject>()
               .From.External(c => c.GetOrAdd<ProjectSaver>("Saver"), true)
               .AndReceive();

            this.Flow<BuildRequest>()
               .From.External(c => c.GetOrAdd<BuildActorCoordinator>("Builder"), true)
               .AndReceive();
        }
    }
}