namespace Tauron.Application.Workshop.StateManagement
{
    public interface IEffect
    {
		void Handle(IStateAction action, IActionInvoker dispatcher);

        bool ShouldReactToAction(IStateAction action);
	}
}