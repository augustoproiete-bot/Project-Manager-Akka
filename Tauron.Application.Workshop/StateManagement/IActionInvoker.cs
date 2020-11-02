using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement
{
    [PublicAPI]
    public interface IActionInvoker
    {
        TState? GetState<TState>()
            where TState : class;

        TState? GetState<TState>(string key)
            where TState : class;

        void Run(IStateAction action, bool? sendBack = null);
    }
}