using System.Windows.Threading;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels
{
    [UsedImplicitly]
    public sealed class ProjectViewModel : UiActor
    {
        public ProjectViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher) 
            : base(lifetimeScope, dispatcher)
        {
            Receive<InitProjectViewModel>(InitProjectViewModel);
        }

        private void InitProjectViewModel(InitProjectViewModel obj)
        {
            
        }
    }
}