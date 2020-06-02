using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Threading;
using Akka.Actor;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel;
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
        private sealed class InitImportRemove
        {
            public string ToRemove { get; }

            public InitImportRemove(string remove) => ToRemove = remove;
        }

        private sealed class UpdateRequest
        {
            public string EntryName { get; }

            public ActiveLanguage Language { get; }

            public string Content { get; }

            public string ProjectName { get; }

            public UpdateRequest(string entryName, ActiveLanguage language, string content, string projectName)
            {
                EntryName = entryName;
                Language = language;
                Content = content;
                ProjectName = projectName;
            }
        }

        private sealed class RemoveRequest
        {
            public string EntryName { get; }

            public string ProjectName { get; }

            public RemoveRequest(string entryName, string projectName)
            {
                EntryName = entryName;
                ProjectName = projectName;
            }
        }

        private string _project = string.Empty;

        public UICollectionProperty<ProjectViewLanguageModel> Languages { get; }
        public UIProperty<int> SelectedIndex { get; set; }
        public UICollectionProperty<ProjectEntryModel> ProjectEntrys { get; }

        public UIProperty<int> ImportSelectIndex { get; }

        public UICollectionProperty<string> ImportetProjects { get; }

        public ProjectViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, LocLocalizer localizer, IDialogCoordinator dialogCoordinator, ProjectFileWorkspace workspace) 
            : base(lifetimeScope, dispatcher)
        {
            #region Remove Request

            void RemoveEntry(EntryRemove entry)
            {
                if(_project != entry.Entry.Project) return;

                var index = ProjectEntrys.FindIndex(em => em.EntryName == entry.Entry.Key);
                if(index == -1) return;

                ProjectEntrys.RemoveAt(index);
            }
            
            this.Flow<RemoveRequest>()
               .To.Mutate(workspace.Entrys).For(em => em.EntryRemove, em => rr => em.RemoveEntry(rr.ProjectName, rr.EntryName)).ToSelf()
               .Then.Action(RemoveEntry).Receive();

            #endregion

            #region Update Request

            void UpdateEntry(EntryUpdate obj)
            {
                if(_project != obj.Entry.Project) return;

                var model = ProjectEntrys.FirstOrDefault(m => m.EntryName == obj.Entry.Key);
                model.Update(obj.Entry);
            }

            this.Flow<UpdateRequest>()
               .To.Mutate(workspace.Entrys).For(em => em.EntryUpdate, em => ur => em.UpdateEntry(ur.ProjectName, ur.Language, ur.EntryName, ur.Content)).ToSelf()
               .Then.Action(UpdateEntry).Receive();
               

            #endregion

            #region Imports

            void AddImport(AddImport obj)
            {
                if(obj.ProjectName != _project) return;
                ImportetProjects.Add(obj.Import);
            }

            IEnumerable<string> GetImportableProjects()
            {
                var pro = workspace.Get(_project);
                return workspace.ProjectFile.Projects.Select(p => p.ProjectName).Where(p => p != _project && !pro.Imports.Contains(p));
            }

            ImportSelectIndex = RegisterProperty<int>(nameof(ImportSelectIndex)).WithDefaultValue(-1);
            ImportetProjects = this.RegisterUiCollection<string>(nameof(ImportetProjects)).Async();

            NewCommad.WithCanExecute(() => GetImportableProjects().Any())
                .ToFlow(this.ShowDialog<IImportProjectDialog, ImportProjectDialogResult?, string>(GetImportableProjects))
                .To.Mutate(workspace.Projects).For(pm => pm.NewImport, pm => r => pm.AddImport(_project, r!.Project)).ToSelf()
                .Then.Action(AddImport)
                .Return().ThenRegister("AddImport");

            void RemoveImport(RemoveImport import)
            {
                if(_project != import.TargetProject) return;

                ImportetProjects.Remove(import.ToRemove);
            }

            NewCommad.WithCanExecute(() => ImportSelectIndex.Value != -1)
               .ToFlow(() => new InitImportRemove(ImportetProjects[ImportSelectIndex]))
               .To.Mutate(workspace.Projects).For(pm => pm.RemoveImport, pm => ir => pm.TryRemoveImport(_project, ir.ToRemove)).ToSelf()
               .Then.Action(RemoveImport)
               .Return().ThenRegister("RemoveImport");
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
               .ToFlow(this.ShowDialog<ILanguageSelectorDialog, AddLanguageDialogResult?, CultureInfo>(() => workspace.Get(_project).ActiveLanguages.Select(al => al.ToCulture()).ToArray()))
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
                SelectedIndex.PropertyValueChanged += () =>
                {
                    if (SelectedIndex == 0)
                        return;
                    SelectedIndex += 0;
                };

                var self = Context.Self;

                foreach (var projectEntry in obj.Project.Entries)
                {
                    ProjectEntrys.Add(new ProjectEntryModel(obj.Project, projectEntry,
                        data =>
                        {
                            var (projectName, entryName, lang, content) = data;
                            self.Tell(new UpdateRequest(entryName, lang, content, projectName));
                        },
                        data =>
                        {
                            var (projectName, entryName) = data;
                            self.Tell(new RemoveRequest(entryName, projectName));
                        }));
                }
            }

            Receive<InitProjectViewModel>(InitProjectViewModel);

            #endregion
        }
    }
}