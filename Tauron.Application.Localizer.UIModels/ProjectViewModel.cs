using System.Linq;
using System.Windows.Threading;
using Autofac;
using JetBrains.Annotations;
using MahApps.Metro.Controls.Dialogs;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Localizer.DataModel.Workspace;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels
{
    [UsedImplicitly]
    public sealed class ProjectViewModel : UiActor
    {
        private readonly LocLocalizer _localizer;
        private readonly IDialogCoordinator _dialogCoordinator;
        private ProjectFileWorkspace _workspace = ProjectFileWorkspace.Dummy;

        public UIObservableCollection<ProjectViewLanguageModel> Languages
        {
            get => Get<UIObservableCollection<ProjectViewLanguageModel>>()!;
        }

        public ProjectViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, LocLocalizer localizer, IDialogCoordinator dialogCoordinator) 
            : base(lifetimeScope, dispatcher)
        {
            _localizer = localizer;
            _dialogCoordinator = dialogCoordinator;
            Set(new UIObservableCollection<ProjectViewLanguageModel>(), nameof(Languages));

            RegisterCommand("AddLanguage", AddLanguage);

            Receive<InitProjectViewModel>(InitProjectViewModel);
        }

        private void AddLanguage()
        {
            
        }

        private void InitProjectViewModel(InitProjectViewModel obj)
        {
            _workspace = obj.Workspace;

            Languages.Add(new ProjectViewLanguageModel(_localizer.ProjectViewLanguageBoxFirstLabel, true));
            Languages.AddRange(obj.Project.ActiveLanguages.Select(al => new ProjectViewLanguageModel(al.Name, false)));
            // ReSharper disable once ExplicitCallerInfoArgument
            Set(0, "SelectedIndex");

        }
    }

    public sealed class ProjectViewLanguageModel
    {
        public string Name { get; }

        public bool IsEnabled { get; }

        public ProjectViewLanguageModel(string name, bool isEnabled)
        {
            Name = name;
            IsEnabled = isEnabled;
        }
    }
}