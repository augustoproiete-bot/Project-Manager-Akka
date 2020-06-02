using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Serilog;
using Tauron.Akka;
using Tauron.Application.Wpf.Helper;
using Tauron.Application.Wpf.ModelMessages;

namespace Tauron.Application.Wpf.UI
{
    [PublicAPI]
    public abstract class ModelConnectorBase<TDrived>
    {
        protected readonly ILogger Log = Serilog.Log.ForContext<TDrived>();

        private IEventActor? _eventActor;

        private int _isInitializing = 1;

        protected ModelConnectorBase(string name, DataContextPromise promise)
        {
            Name = name;

            promise.OnUnload(() => { });

            promise
                .OnContext(model =>
                {
                    Model = model;

                    if (model.IsInitialized)
                    {
                        Task.Run(async () => await InitAsync());
                    }
                    else
                    {
                        void OnModelOnInitialized()
                        {
                            Task.Run(async () => await InitAsync());
                            Model.Initialized -= OnModelOnInitialized;
                        }

                        model.Initialized += OnModelOnInitialized;
                    }
                });
        }

        protected string Name { get; }
        protected IViewModel? Model { get; private set; }
        protected int IsInitializing => _isInitializing;

        private async Task InitAsync()
        {
            try
            {
                OnLoad();
                if (Model == null) return;

                //Log.Information("Ask For {Property}", _name);
                var eventActor = await Model.Ask<IEventActor>(new MakeEventHook(), TimeSpan.FromSeconds(5));
                //Log.Information("Ask Compled For {Property}", _name);

                eventActor.Register(HookEvent.Create<PropertyChangedEvent>(PropertyChangedHandler));
                eventActor.Register(HookEvent.Create<ValidatingEvent>(ValidateCompled));

                Model.Tell(new TrackPropertyEvent(Name), eventActor.OriginalRef);

                Interlocked.Exchange(ref _eventActor, eventActor);
                Interlocked.Exchange(ref _isInitializing, 0);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error Bind Property {Name}", Name);
            }
        }

        protected virtual void OnUnload()
        {
        }

        protected virtual void OnLoad()
        {
        }

        public void ForceUnload()
        {
            if (Model == null)
                return;

            OnUnload();
            Model = null;
            _eventActor?.OriginalRef.Tell(PoisonPill.Instance);
        }

        protected abstract void ValidateCompled(ValidatingEvent obj);

        protected abstract void PropertyChangedHandler(PropertyChangedEvent obj);
    }
}