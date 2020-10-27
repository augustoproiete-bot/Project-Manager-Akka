using System.Threading.Tasks;

namespace Tauron.Application.Workshop.StateManagement
{
    public interface IMiddleware
    {
        void Initialize(RootManager store);

        void AfterInitializeAllMiddlewares();

        bool MayDispatchAction(IStateAction action);

        void BeforeDispatch(IStateAction action);

    }
}