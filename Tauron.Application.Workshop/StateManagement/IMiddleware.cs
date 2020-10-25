namespace Tauron.Application.Workshop.StateManagement
{
    public interface IMiddleware
    {
        void Initialize(RootManager store);

        void AfterInitializeAllMiddlewares();

        bool MayDispatchAction(object action);

        void BeforeDispatch(object action);

        void AfterDispatch(object action);
    }
}