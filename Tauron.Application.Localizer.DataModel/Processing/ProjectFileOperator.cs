using Akka.Actor;
using Tauron.Akka;
using Tauron.Application.Localizer.DataModel.Processing.Actors;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed class ProjectFileOperator : ExposedReceiveActor
    {
        public ProjectFileOperator()
        {
            Flow<LoadProjectFile>(this)
                .From.Func(lp => new InternalLoadProject(lp, Context.Sender)).ToRef(ac => ac.ActorOf<ProjectLoader>());

            Flow<SaveProject>(this)
               .From.External(c => c.GetOrAdd<ProjectSaver>("Saver"), true);

            Flow<BuildRequest>(this)
               .From.External(c => c.GetOrAdd<BuildActorCoordinator>("Builder"), true);
        }
    }
}