using Akka.Actor;
using Functional.Maybe;

namespace Tauron.Application.Workshop.Analyzing.Rules
{
    public interface IRule<in TWorkspace, TData>
        where TWorkspace : WorkspaceBase<TData> where TData : class
    {
        string Name { get; }

        Maybe<IActorRef> Init(IActorRefFactory superviser, Maybe<TWorkspace> workspace);
    }
}