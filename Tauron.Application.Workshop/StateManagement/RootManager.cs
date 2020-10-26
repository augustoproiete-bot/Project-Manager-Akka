using Tauron.Application.Workshop.StateManagement.Dispatcher;

namespace Tauron.Application.Workshop.StateManagement
{
    public sealed class RootManager : IActionInvoker
    {
        internal RootManager(WorkspaceSuperviser superviser, IStateDispatcherConfigurator stateDispatcher)
        {
            
        }

        public TState GetState<TState>()
            => GetState<TState>("");

        public TState GetState<TState>(string key);

        public void Run(IStateAction action);
    }
}