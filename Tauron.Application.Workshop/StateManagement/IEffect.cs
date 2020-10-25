using System.Threading.Tasks;

namespace Tauron.Application.Workshop.StateManagement
{
    public interface IEffect
    {
		void Handle(object action, IActionInvoker dispatcher);

        bool ShouldReactToAction(object action);
	}
}