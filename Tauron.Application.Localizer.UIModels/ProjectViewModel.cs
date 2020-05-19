using System.Windows.Threading;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Localizer.DataModel.Workspace;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels
{
    [UsedImplicitly]
    public sealed class ProjectViewModel : UiActor
    {
        private ProjectFileWorkspace _workspace = ProjectFileWorkspace.Dummy;

        public UIObservableCollection<ActiveLanguage> Languages
        {
            get => Get<UIObservableCollection<ActiveLanguage>>()!;
        }

        public ProjectViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher) 
            : base(lifetimeScope, dispatcher)
        {
            Set(new UIObservableCollection<ActiveLanguage>(), nameof(Languages));

            Receive<InitProjectViewModel>(InitProjectViewModel);
        }

        private void InitProjectViewModel(InitProjectViewModel obj)
        {
            _workspace = obj.Workspace;
            Languages.AddRange(obj.Project.ActiveLanguages);
        }
    }
}