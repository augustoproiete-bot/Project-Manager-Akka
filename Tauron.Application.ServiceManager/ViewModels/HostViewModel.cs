using System.Linq;
using System.Windows.Threading;
using Akka.Actor;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;
using Tauron.Application.Master.Commands.Host;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.ViewModels
{
    public sealed class HostViewModel : UiActor
    {
        private readonly IActorRef _hostConnector;

        private EventSubscribtion _eventSubscribtion = EventSubscribtion.Empty;

        public HostViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher) 
            : base(lifetimeScope, dispatcher)
        {
            _hostConnector = HostApi.Create(Context);

            HostEntries = this.RegisterUiCollection<UIHostEntry>(nameof(HostEntries)).AndAsync();

            this.Flow<HostEntryChanged>()
                .From.Action(he =>
                {
                    var entry = HostEntries.FirstOrDefault(e => e.ActorPath == he.Path);
                    if (he.Removed)
                    {
                        if(entry != null)
                            HostEntries.Remove(entry);
                    }
                    else
                    {
                        if (entry == null)
                            HostEntries.Add(new UIHostEntry(he.Path, he.Name));
                        else
                            entry.Name = he.Name;
                    }
                });
        }

        protected override void PreStart()
        {
            _eventSubscribtion = _hostConnector.SubscribeToEvent<HostEntryChanged>();
            base.PreStart();
        }

        protected override void PostStop()
        {
            _eventSubscribtion.Dispose();
            base.PostStop();
        }

        public UICollectionProperty<UIHostEntry> HostEntries { get; }
    }

    public sealed class UIHostEntry : ObservableObject
    {
        private string? _name;

        public string Name

        {
            get => string.IsNullOrWhiteSpace(_name) ? ActorPath.ToString() : _name;
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public ActorPath ActorPath { get; }

        public UIHostEntry(ActorPath actorPath, string name)
        {
            ActorPath = actorPath;
            _name = name;
        }
    }
}