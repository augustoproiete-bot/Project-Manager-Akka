using System.Globalization;
using System.Linq;
using System.Windows.Threading;
using Akka.Actor;
using Autofac;
using JetBrains.Annotations;
using MahApps.Metro.Controls.Dialogs;
using Tauron.Application.Localizer.DataModel.Workspace;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Workshop;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels
{
    [UsedImplicitly]
    public sealed class ProjectViewModel : UiActor
    {
        private sealed class TryImport
        {
            public string? Import { get; }

            public TryImport(string? import) => Import = import;
        }

        private readonly LocLocalizer _localizer;
        private readonly IDialogCoordinator _dialogCoordinator;
        private string _project = string.Empty;
        private readonly ProjectFileWorkspace _workspace;

        public UICollectionProperty<ProjectViewLanguageModel> Languages { get; }
        public UIProperty<int> SelectedIndex { get; set; }
        public UICollectionProperty<ProjectEntryModel> ProjectEntrys { get; }

        public UIProperty<int> ImportSelectInfex { get; }

        public UICollectionProperty<string> ImportetProjects { get; }

        public ProjectViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, LocLocalizer localizer, IDialogCoordinator dialogCoordinator, ProjectFileWorkspace workspace) 
            : base(lifetimeScope, dispatcher)
        {
            _localizer = localizer;
            _dialogCoordinator = dialogCoordinator;
            _workspace = workspace;

            ProjectEntrys = this.RegisterUiCollection<ProjectEntryModel>(nameof(ProjectEntrys)).Async();
            SelectedIndex = RegisterProperty<int>(nameof(SelectedIndex));

            #region Imports

            ImportSelectInfex = RegisterProperty<int>(nameof(ImportSelectInfex)).WithDefaultValue(0);
            ImportetProjects = this.RegisterUiCollection<string>(nameof(ImportetProjects)).Async();
            this.RespondOnEventSource(_workspace.Projects.NewImport, AddImport);

            NewCommad.WithExecute(AddImportCommand).WithCanExecute(() => _workspace.ProjectFile.Projects.Count > 1).ThenRegister("AddImportCommand");

            #endregion

            #region AddLanguage

            Languages = this.RegisterUiCollection<ProjectViewLanguageModel>(nameof(Languages)).Async();
            NewCommad.WithExecute(AddLanguage).ThenRegister("AddLanguage");

            this.RespondOnEventSource(_workspace.Projects.NewLanguage, AddActiveLanguage);
            Receive<CultureInfo>(culture => _workspace.Projects.AddLanguage(_project, culture));
            Receive<TryImport>(TryImportExc);

            #endregion

            Receive<InitProjectViewModel>(InitProjectViewModel);
        }

        #region Imports

        private void AddImport(AddImport obj)
        {

        }

        private void TryImportExc(TryImport obj)
        {
            if(string.IsNullOrWhiteSpace(obj.Import)) return;

            _workspace.Projects.AddImport(_project, obj.Import);
        }

        private void AddImportCommand()
        {
            UICall(async c =>
            {
                var pro = _workspace.Get(_project);
                var diag = LifetimeScope.Resolve<IImportProjectDialog>();
                diag.Init(s => c.Self.Tell(new TryImport(s)), _workspace.ProjectFile.Projects
                    .Select(p => p.ProjectName)
                    .Where(s => s != _project && !pro.Imports.Contains(s)));

                await _dialogCoordinator.ShowMetroDialogAsync("MainWindow", diag.Dialog);
            });
        }

        #endregion

        #region AddLanguage

        private void AddActiveLanguage(AddActiveLanguage language)
        {
            if(language.ProjectName != _project) return;

            Languages.Add(new ProjectViewLanguageModel(language.ActiveLanguage.Name, false));

            foreach (var model in ProjectEntrys) 
                model.AddLanguage(language.ActiveLanguage);
        }

        private void AddLanguage()
        {
            UICall(async c =>
                   {
                       var diag = LifetimeScope.Resolve<ILanguageSelectorDialog>();
                       diag.Init(ci =>
                                 {
                                     if(ci != null)
                                         c.Self.Tell(ci, ActorRefs.NoSender);
                                 }, cultureInfo => _workspace.Get(_project).ActiveLanguages.Any(al => al.Shortcut == cultureInfo.Name));

                       await _dialogCoordinator.ShowMetroDialogAsync("MainWindow", diag.Dialog);
                   });
        }

        #endregion

        private void InitProjectViewModel(InitProjectViewModel obj)
        {
            _project = obj.Project.ProjectName;

            Languages.Add(new ProjectViewLanguageModel(_localizer.ProjectViewLanguageBoxFirstLabel, true));
            Languages.AddRange(obj.Project.ActiveLanguages.Select(al => new ProjectViewLanguageModel(al.Name, false)));
            SelectedIndex += 0;

        }
    }
}