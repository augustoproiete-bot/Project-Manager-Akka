using Akka.Actor;
using Tauron.Akka;
using Tauron.Application.Localizer.DataModel.Processing.Actors;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed class ProjectFileOperator : ExposedReceiveActor
    {
        public ProjectFileOperator()
        {
            Flow<LoadProjectFile>(b => b.Func(lp => new InternalLoadProject(lp, Context.Sender)).ToRef(ac => ac.ActorOf<ProjectLoader>()));

            Flow<SaveProject>(b => b.External(c => c.GetOrAdd<ProjectSaver>("Saver"), true));

            Flow<BuildRequest>(b => b.External(c => c.GetOrAdd<BuildActorCoordinator>("Builder"), true));
        }
    }
}