using Akka.Actor;

namespace Tauron.Application.Workshop.Analyzing.Rules
{
    public interface IRule<in TWorkspace, TData>
        where TWorkspace : WorkspaceBase<TData>
    {
        string Name { get; }

        IActorRef Init(IActorRefFactory superviser, TWorkspace workspace);
    }
}