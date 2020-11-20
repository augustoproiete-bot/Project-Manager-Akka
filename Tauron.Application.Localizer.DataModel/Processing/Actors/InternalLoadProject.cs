using Akka.Actor;

namespace Tauron.Application.Localizer.DataModel.Processing.Actors
{
    public sealed record InternalLoadProject(LoadProjectFile ProjectFile, IActorRef OriginalSender);
}