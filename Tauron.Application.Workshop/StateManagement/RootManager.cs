using Tauron.Application.Workshop.StateManagement.Dispatcher;

namespace Tauron.Application.Workshop.StateManagement
{
    public sealed class RootManager : IActionInvoker
    {
        internal RootManager(WorkspaceSuperviser superviser, IStateDispatcherConfigurator stateDispatcher)
        {
            
        }

        public TState GetState<TState>();

        public void Run(object action);
    }
}