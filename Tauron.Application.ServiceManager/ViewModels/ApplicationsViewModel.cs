using System.Windows.Threading;
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

        protected override void PreStart()
        {
            Context.System.EventStream.Subscribe(Self, typeof(DisplayApplications));
            base.PreStart();
        }
    }
}