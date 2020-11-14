using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement
{
    [PublicAPI]
    public interface IEffect
    {
		void Handle(IStateAction action, IActionInvoker dispatcher);

        bool ShouldReactToAction(IStateAction action);
	}
}