using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Threading;
using Akka.Actor;
using Autofac;
using JetBrains.Annotations;
using MahApps.Metro.Controls.Dialogs;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Localizer.DataModel.Workspace;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels
{
    [UsedImplicitly]
    public sealed class ProjectViewModel : UiActor
    {
        private readonly LocLocalizer _localizer;
        private readonly IDialogCoordinator _dialogCoordinator;
        private Project _project = new Project();
        private ProjectFileWorkspace _workspace = ProjectFileWorkspace.Dummy;

        public UICollectionProperty<ProjectViewLanguageModel> Languages { get; }
        public UIProperty<int> SelectedIndex { get; set; }
        public UICollectionProperty<ProjectEntryModel> ProjectEntrys { get; }

        public UIProperty<int> ImportSelectInfex { get; }

        public UICollectionProperty<string> ImportetProjects { get; }

        public ProjectViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, LocLocalizer localizer, IDialogCoordinator dialogCoordinator) 
            : base(lifetimeScope, dispatcher)
        {
            _localizer = localizer;
            _dialogCoordinator = dialogCoordinator;

            ProjectEntrys = this.RegisterUiCollection<ProjectEntryModel>(nameof(ProjectEntrys)).Async();
            SelectedIndex = RegisterProperty<int>(nameof(SelectedIndex));

            #region Imports

            ImportSelectInfex = RegisterProperty<int>(nameof(ImportSelectInfex)).WithDefaultValue(0);
            ImportetProjects = this.RegisterUiCollection<string>(nameof(ImportetProjects)).Async();

            NewCommad.WithExecute(AddImport).WithCanExecute(() => _workspace.ProjectFile.Projects.Count > 1).ThenRegister("AddImport");

            #endregion

            #region AddLanguage

            Languages = this.RegisterUiCollection<ProjectViewLanguageModel>(nameof(Languages)).Async();
            NewCommad.WithExecute(AddLanguage).ThenRegister("AddLanguage");

            Receive<AddActiveLanguage>(AddActiveLanguage);
            Receive<CultureInfo>(culture => _workspace.Projects.AddLanguage(_project.ProjectName, culture));

            #endregion

            Receive<InitProjectViewModel>(InitProjectViewModel);
        }

        private void AddImport()
        {
            
        }

        #region AddLanguage

        private void AddActiveLanguage(AddActiveLanguage language)
        {
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
                                 }, c => _project.ActiveLanguages.Any(al => al.Shortcut == c.Name));

                       await _dialogCoordinator.ShowMetroDialogAsync("MainWindow", diag.Dialog);
                   });
        }

        #endregion

        private void InitProjectViewModel(InitProjectViewModel obj)
        {
            _project = obj.Project;
            _workspace = obj.Workspace;

            _workspace.Projects.NewLanguage.RespondOn(Self);
            Languages.Add(new ProjectViewLanguageModel(_localizer.ProjectViewLanguageBoxFirstLabel, true));
            Languages.AddRange(obj.Project.ActiveLanguages.Select(al => new ProjectViewLanguageModel(al.Name, false)));
            SelectedIndex += 0;

        }
    }
}