using System.Windows.Threading;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Localizer.UIModels.Messages;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels
{
    public sealed class CenterViewModel : UiActor
    {
        public CenterViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher) 
            : base(lifetimeScope, dispatcher)
        {
            Receive<UpdateSource>(UpdateSourceHandler);
            Receive<SupplyNewProjectFile>(LoadNewProject);
        }

        private void LoadNewProject(SupplyNewProjectFile obj)
        {
            
        }

        private void UpdateSourceHandler(UpdateSource obj)
        {
            
        }
    }
}