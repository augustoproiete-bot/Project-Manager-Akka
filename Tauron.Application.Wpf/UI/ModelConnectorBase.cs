using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Functional.Maybe;
using JetBrains.Annotations;
using Serilog;
using Tauron.Akka;
using Tauron.Application.Wpf.Helper;
using Tauron.Application.Wpf.ModelMessages;
using static Tauron.Prelude;

namespace Tauron.Application.Wpf.UI
{
    [PublicAPI]
    public abstract class ModelConnectorBase<TDrived>
    {
        protected readonly ILogger Log = Serilog.Log.ForContext<TDrived>();

        private IEventActor? _eventActor;

        private int _isInitializing = 1;

        protected ModelConnectorBase(string name, Maybe<DataContextPromise> mayPromise)
        {
            Name = name;

            var promise = mayPromise.Value;
            promise.OnUnload(OnUnload);

            promise.OnNoContext(NoDataContextFound);

            promise
               .OnContext((model, view) =>
                          {
                              View  = May(view);
                              Model = May(model);

                              if (model.IsInitialized)
                                  Task.Run(async () => await InitAsync());
                              else
                              {
                                  void OnModelOnInitialized()
                                      => Task.Run(async () => await InitAsync());

                                  model.AwaitInit(OnModelOnInitialized);
                              }
                          });
        }

        protected string            Name  { get; }
        protected Maybe<IViewModel> Model { get; private set; }

        protected Maybe<IView> View { get; private set; }

        protected int IsInitializing => _isInitializing;

        private async Task InitAsync()
        {
            try
            {
                Log.Debug("Init ModelConnector {Type} -- {Name}", typeof(TDrived), Name);

                if (Model.IsNothing()) return;
                OnLoad();
                //Log.Information("Ask For {Property}", _name);
                await DoAsync(from model in Model
                              select Func(async () =>
                                          {
                                              var eventActor = await Ask<IEventActor>(model.Actor, new MakeEventHook(Name), TimeSpan.FromSeconds(15));
                                              //Log.Information("Ask Compled For {Property}", _name);

                                              eventActor.Register(HookEvent.Create<PropertyChangedEvent>(PropertyChangedHandler));
                                              eventActor.Register(HookEvent.Create<ValidatingEvent>(ValidateCompled));

                                              Tell(model.Actor, new TrackPropertyEvent(Name), eventActor.OriginalRef);

                                              Interlocked.Exchange(ref _eventActor, eventActor);
                                              Interlocked.Exchange(ref _isInitializing, 0);
                                          }));
            }
            catch (Exception e)
            {
                Log.Error(e, "Error Bind Property {Name}", Name);
            }
        }

        protected virtual void OnUnload()
        {
            Log.Debug("Unload ModelConnector {Type} -- {Name}", typeof(TDrived), Name);
        }

        protected virtual void OnLoad()
        {
            Log.Debug("Load ModelConnector {Type} -- {Name}", typeof(TDrived), Name);
        }

        protected virtual void OnViewFound(IView view)
        {
            Log.Debug("View Found ModelConnector {Type} -- {Name}", typeof(TDrived), Name);
        }

        public void ForceUnload()
        {
            if (Model.IsNothing())
                return;

            OnUnload();
            Model = Maybe<IViewModel>.Nothing;
            _eventActor?.OriginalRef.Tell(PoisonPill.Instance);
        }

        protected abstract void NoDataContextFound();

        protected abstract void ValidateCompled(ValidatingEvent obj);

        protected abstract void PropertyChangedHandler(PropertyChangedEvent obj);
    }
}