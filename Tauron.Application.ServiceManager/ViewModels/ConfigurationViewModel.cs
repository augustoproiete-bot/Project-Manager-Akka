using System.Windows.Threading;
using Akka.Event;
using Autofac;
using Tauron.Application.ServiceManager.ViewModels.SetupDialog;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.ViewModels
{
    public sealed class ConfigurationViewModel : UiActor
    {
        public ConfigurationViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher) 
            : base(lifetimeScope, dispatcher)
        {
            Receive<StartConfigurationSetup>(_ =>
            {

            });
        }

        protected override void PostStop()
        {
            Context.System.EventStream.Unsubscribe<StartConfigurationSetup>(Self);
            base.PostStop();
        }

        protected override void PreStart()
        {
            Context.System.EventStream.Subscribe<StartConfigurationSetup>(Self);
            base.PreStart();
        }
    }
}