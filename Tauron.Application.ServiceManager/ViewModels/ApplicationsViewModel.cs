using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Akka.Actor;
using Akka.Event;
using Autofac;
using Tauron.Application.Localizer.Generated;
using Tauron.Application.Master.Commands.Administration.Host;
using Tauron.Application.ServiceManager.ViewModels.ApplicationModelData;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Helper;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.ViewModels
{
    public sealed class ApplicationsViewModel : UiActor
    {
        private readonly Dictionary<ActorPath, TabItem> _hosts = new Dictionary<ActorPath, TabItem>();

        public ApplicationsViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher) 
            : base(lifetimeScope, dispatcher)
        {
            var modelView = LifetimeScope.Resolve<IViewModel<ApplicationManagerTabViewModel>>();
            modelView.InitModel(Context, "Application_Manager");

            Items = this.RegisterUiCollection<TabItem>(nameof(Items))
               .AndInitialElements(new TabItem(LocLocalizer.Inst.ApplicationsView.GlobalManagerHeader, modelView))
               .AndAsync();

            CurrentTab = RegisterProperty<int>(nameof(CurrentTab));

            var hostApi = HostApi.CreateOrGet(Context.System);

            AddResource(hostApi.Event<HostEntryChanged>());

            Receive<HostEntryChanged>(e =>
            {
                static string GetName(HostEntryChanged entry)
                {
                    if (string.IsNullOrWhiteSpace(entry.Name))
                        return entry.Path.ToString() ?? LocLocalizer.Inst.Common.Unkowen;
                    return entry.Name;
                }

                if (e.Removed)
                {
                    if (_hosts.Remove(e.Path, out var item))
                    {
                        Items.Remove(item);
                        Context.Stop(item.Content.Actor);
                    }
                }
                else
                {
                    if (_hosts.TryGetValue(e.Path, out var item))
                    {
                        item.Header = string.Format(LocLocalizer.Inst.ApplicationsView.HostsApplicationHeader, GetName(e));
                        item.Metadata = e.Name;
                    }
                    else
                    {
                        var model = LifetimeScope.Resolve<IViewModel<HostApplicationManagerTabViewModel>>();
                        model.InitModel(Context);
                        model.AwaitInit(() => model.Actor.Tell(e));

                        var newItem = new TabItem(string.Format(LocLocalizer.Inst.ApplicationsView.HostsApplicationHeader, GetName(e)), model);
                        _hosts.Add(e.Path, newItem);
                        Items.Add(newItem);
                    }
                }
            });

            Receive<DisplayApplications>(e =>
            {
                var target = _hosts.Values.FirstOrDefault(t => t.Metadata == e.HostName);
                if(target == null)
                    CurrentTab.Set(0);
                else
                {
                    var index = Items.IndexOf(target);
                    CurrentTab.Set(index == -1 ? 0 : index);
                }
            });
        }
        
        public UICollectionProperty<TabItem> Items { get; }

        public UIProperty<int> CurrentTab { get; }

        protected override void PostStop()
        {
            Context.System.EventStream.Unsubscribe<DisplayApplications>(Self);
            base.PostStop();
        }

        protected override void PreStart()
        {
            Context.System.EventStream.Subscribe<DisplayApplications>(Self);
            base.PreStart();
        }
    }
}