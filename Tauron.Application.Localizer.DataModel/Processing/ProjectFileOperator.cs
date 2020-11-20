using Akka.Actor;
using Functional.Maybe;
using Tauron.Akka;
using Tauron.Application.Localizer.DataModel.Processing.Actors;
using static Tauron.Prelude;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed class ProjectFileOperator : ExposedReceiveActor
    {
        public ProjectFileOperator()
        {
            Flow<LoadProjectFile>(b => b.Func(maylp => maylp.Select(lp => new InternalLoadProject(lp, Context.Sender))).ToRef(ac => ac.ActorOf<ProjectLoader>()));

            Flow<SaveProject>(b => b.External(c => c.GetOrAdd<ProjectSaver>(May("Saver")), true));

            Flow<BuildRequest>(b => b.External(c => c.GetOrAdd<BuildActorCoordinator>(May("Builder")), true));
        }
    }
}