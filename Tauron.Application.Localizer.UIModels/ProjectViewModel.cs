using System.Collections.Generic;
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
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels
{
    [UsedImplicitly]
    public sealed class ProjectViewModel : UiActor
    {
        private string _project = string.Empty;

        public UICollectionProperty<ProjectViewLanguageModel> Languages { get; }
        public UIProperty<int> SelectedIndex { get; set; }
        public UICollectionProperty<ProjectEntryModel> ProjectEntrys { get; }

        public UIProperty<int> ImportSelectInfex { get; }

        public UICollectionProperty<string> ImportetProjects { get; }

        public ProjectViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, LocLocalizer localizer, IDialogCoordinator dialogCoordinator, ProjectFileWorkspace workspace) 
            : base(lifetimeScope, dispatcher)
        {
            #region Imports

            void AddImport(AddImport obj)
            {

            }

            IEnumerable<string> GetImportableProjects()
            {
                var pro = workspace.Get(_project);
                return workspace.ProjectFile.Projects.Select(p => p.ProjectName).Where(p => p != _project && !pro.Imports.Contains(p));
            }

            ImportSelectInfex = RegisterProperty<int>(nameof(ImportSelectInfex)).WithDefaultValue(0);
            ImportetProjects = this.RegisterUiCollection<string>(nameof(ImportetProjects)).Async();

            NewCommad.WithCanExecute(() => GetImportableProjects().Any())
                .ToFlow(this.ShowDialog<IImportProjectDialog, ImportProjectDialogResult?, string>(GetImportableProjects()))
                .To.Mutate(workspace.Projects).For(pm => pm.NewImport, pm => r => pm.AddImport(_project, r!.Project)).ToSelf()
                .Then.Action(AddImport)
                .Return().ThenRegister("AddImportCommand");
            Context.Sender

            #endregion

            #region AddLanguage

            void AddActiveLanguage(AddActiveLanguage language)
            {
                if (language.ProjectName != _project) return;

                Languages.Add(new ProjectViewLanguageModel(language.ActiveLanguage.Name, false));

                foreach (var model in ProjectEntrys)
                    model.AddLanguage(language.ActiveLanguage);
            }


            Languages = this.RegisterUiCollection<ProjectViewLanguageModel>(nameof(Languages)).Async();

            NewCommad
               .ToFlow(this.ShowDialog<ILanguageSelectorDialog, AddLanguageDialogResult?, CultureInfo>(workspace.Get(_project).ActiveLanguages.Select(al => al.ToCulture()).ToArray()))
               .To.Mutate(workspace.Projects).For(pm => pm.NewLanguage, pm => d => pm.AddLanguage(_project, d!.CultureInfo)).ToSelf()
               .Then.Action(AddActiveLanguage).Return().ThenRegister("AddLanguage");
            
            #endregion

            #region Init

            ProjectEntrys = this.RegisterUiCollection<ProjectEntryModel>(nameof(ProjectEntrys)).Async();
            SelectedIndex = RegisterProperty<int>(nameof(SelectedIndex));

            void InitProjectViewModel(InitProjectViewModel obj)
            {
                _project = obj.Project.ProjectName;

                Languages.Add(new ProjectViewLanguageModel(localizer.ProjectViewLanguageBoxFirstLabel, true));
                Languages.AddRange(obj.Project.ActiveLanguages.Select(al => new ProjectViewLanguageModel(al.Name, false)));
                SelectedIndex += 0;

            }

            Receive<InitProjectViewModel>(InitProjectViewModel);

            #endregion
        }
    }
}