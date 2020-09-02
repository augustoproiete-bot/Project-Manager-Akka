using System.Windows.Threading;
using Akka.Event;
using Autofac;
using Tauron.Application.ServiceManager.ViewModels.ApplicationModelData;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.ViewModels
{
    public sealed class ApplicationsViewModel : UiActor
    {
        public ApplicationsViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher) 
            : base(lifetimeScope, dispatcher)
        {
            CurrentHost = RegisterProperty<string>(nameof(CurrentHost));

            Receive<DisplayApplications>(e =>
            {

            });
        }

        public UIProperty<string> CurrentHost { get; }

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